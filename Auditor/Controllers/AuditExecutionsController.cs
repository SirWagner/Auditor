using System.Security.Claims;
using Auditor.Models;
using Auditor.Models.Lookups;
using Auditor.Security;
using Auditor.Services;
using Auditor.ViewModels.AuditExecution;
using Auditor.ViewModels.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;

namespace Auditor.Controllers
{
    /// <summary>
    /// An execution is one auditor's run of a schedule:
    /// start → accept (or decline) → answer and save drafts → submit → manager closes it or returns it for changes.
    /// </summary>
    public class AuditExecutionsController : Controller
    {
        private readonly AuditorContext _context;
        private readonly AttachmentStorage _storage;
        private readonly UserScope _scope;
        private readonly AuditFilterOptions _filterOptions;
        private readonly ResponsibilitySnapshot _responsibility;
        private readonly ILogger<AuditExecutionsController> _logger;

        public AuditExecutionsController(AuditorContext context, AttachmentStorage storage, UserScope scope, AuditFilterOptions filterOptions, ResponsibilitySnapshot responsibility, ILogger<AuditExecutionsController> logger)
        {
            _responsibility = responsibility;
            _context = context;
            _storage = storage;
            _scope = scope;
            _filterOptions = filterOptions;
            _logger = logger;
        }

        private long CurrentUserId =>
            long.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

        private bool IsReviewer => User.IsInRole(Roles.Admin) || User.IsInRole(Roles.Manager);

        public static readonly (string Value, string Label)[] StatusOptions =
            new[] { ExecutionStatuses.PendingAcceptance, ExecutionStatuses.InProgress, ExecutionStatuses.Completed, ExecutionStatuses.Rejected, ExecutionStatuses.Closed, ExecutionStatuses.Declined }
                .Select(s => (s, ExecutionStatuses.Label(s)))
                .ToArray();

        // ------------------------------------------------------------------ lists

        // GET: AuditExecutions?AirportId=…&SiteId=…&From=…  (every filter optional; default shows all)
        public async Task<IActionResult> Index([FromQuery] AuditFilter filter)
        {
            filter.Normalize();
            filter.AirportId = await _scope.NarrowAsync(filter.AirportId);
            var isAuditor = User.IsInRole(Roles.Auditor);
            if (isAuditor)
                filter.AuditorId = null;

            var userId = CurrentUserId;
            var executions = filter.Apply(_context.AuditExecutions
                .Include(e => e.Schedule).ThenInclude(s => s.Template)
                .Include(e => e.Schedule).ThenInclude(s => s.Site).ThenInclude(s => s.Airport)
                .Include(e => e.Auditor)
                .Include(e => e.AuditExecutionResult)
                .AsQueryable());

            if (isAuditor)
            {
                executions = executions.Where(e => e.AuditorId == userId);

                // Assigned schedules this auditor hasn't started (or declined) yet.
                var toStart = new AuditFilter
                {
                    AirportId = filter.AirportId, Area = filter.Area, Location = filter.Location, SiteId = filter.SiteId,
                    TemplateId = filter.TemplateId, From = filter.From, To = filter.To,
                };
                ViewBag.ToStart = await toStart.Apply(_context.AuditSchedules
                        .Include(s => s.Template)
                        .Include(s => s.Site).ThenInclude(s => s.Airport))
                    .Where(s => (s.Status == ScheduleStatuses.Scheduled || s.Status == ScheduleStatuses.InProgress)
                                && s.Auditors.Any(a => a.Id == userId)
                                && !s.AuditExecutions.Any(e => e.AuditorId == userId))
                    .OrderBy(s => s.ScheduledDate)
                    .ToListAsync();
            }

            ViewBag.Filter = await _filterOptions.BuildAsync(filter, "AuditExecutions", StatusOptions, showAuditor: !isAuditor);
            return View(await executions.OrderByDescending(e => e.Id).ToListAsync());
        }

        // ------------------------------------------------------------------ start / accept / decline

        // POST: AuditExecutions/Start?scheduleId=5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.Auditor)]
        public async Task<IActionResult> Start(long scheduleId)
        {
            var userId = CurrentUserId;
            var schedule = await _context.AuditSchedules
                .Include(s => s.Auditors)
                .FirstOrDefaultAsync(s => s.Id == scheduleId);

            if (schedule == null)
                return NotFound();

            if (!schedule.Auditors.Any(a => a.Id == userId))
            {
                TempData["Error"] = "You aren't assigned to this audit.";
                return RedirectToAction(nameof(Index));
            }

            if (schedule.Status is not (ScheduleStatuses.Scheduled or ScheduleStatuses.InProgress))
            {
                TempData["Error"] = $"This audit is {ScheduleStatuses.Label(schedule.Status).ToLowerInvariant()} and can't be started.";
                return RedirectToAction(nameof(Index));
            }

            var existing = await _context.AuditExecutions.FirstOrDefaultAsync(e => e.ScheduleId == scheduleId && e.AuditorId == userId);
            if (existing != null)
                return RedirectToAction(nameof(Accept), new { executionId = existing.Id });

            var execution = new AuditExecution
            {
                ScheduleId = scheduleId,
                AuditorId = userId,
                Status = ExecutionStatuses.PendingAcceptance,
                OriginalAuditDate = schedule.ScheduledDate,
            };

            _context.AuditExecutions.Add(execution);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Accept), new { executionId = execution.Id });
        }

        // GET: AuditExecutions/Accept?executionId=5
        public async Task<IActionResult> Accept(long executionId)
        {
            var execution = await LoadOwnExecutionAsync(executionId, includeSchedule: true);
            if (execution == null)
                return NotFound();

            if (execution.Status != ExecutionStatuses.PendingAcceptance)
                return RedirectToNextStep(execution);

            return View(execution);
        }

        // POST: AuditExecutions/Accept
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Accept(long executionId, bool confirm)
        {
            var execution = await LoadOwnExecutionAsync(executionId, includeSchedule: true);
            if (execution == null)
                return NotFound();

            if (execution.Status != ExecutionStatuses.PendingAcceptance)
                return RedirectToNextStep(execution);

            execution.AcceptanceDate = DateTime.UtcNow;
            execution.Status = ExecutionStatuses.InProgress;
            if (execution.Schedule.Status == ScheduleStatuses.Scheduled)
                execution.Schedule.Status = ScheduleStatuses.InProgress;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Execute), new { executionId });
        }

        // POST: AuditExecutions/Decline
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Decline(long executionId, string? reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["Error"] = "Enter a reason for declining the audit.";
                return RedirectToAction(nameof(Accept), new { executionId });
            }

            var execution = await LoadOwnExecutionAsync(executionId, includeSchedule: true);
            if (execution == null)
                return NotFound();

            if (execution.Status != ExecutionStatuses.PendingAcceptance)
                return RedirectToNextStep(execution);

            execution.Status = ExecutionStatuses.Declined;
            execution.RejectionReason = reason.Trim();
            await _context.SaveChangesAsync();

            TempData["Success"] = "You declined the audit. The scheduler can see your reason.";
            return RedirectToAction(nameof(Index));
        }

        // ------------------------------------------------------------------ execute

        // GET: AuditExecutions/Execute?executionId=5
        public async Task<IActionResult> Execute(long executionId)
        {
            var execution = await _context.AuditExecutions
                .Include(e => e.Schedule).ThenInclude(s => s.Template)
                .Include(e => e.Schedule).ThenInclude(s => s.Site).ThenInclude(s => s.Airport)
                .Include(e => e.Auditor)
                .FirstOrDefaultAsync(e => e.Id == executionId);

            if (execution == null || execution.AuditorId != CurrentUserId)
                return NotFound();

            if (execution.Status is not (ExecutionStatuses.InProgress or ExecutionStatuses.Rejected))
                return RedirectToNextStep(execution);

            return View(await BuildRenderModelAsync(execution));
        }

        // POST: AuditExecutions/SaveDraft  (AJAX, multipart)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(60 * 1024 * 1024)]
        public async Task<IActionResult> SaveDraft(AuditSubmissionModel model)
        {
            var execution = await _context.AuditExecutions.FirstOrDefaultAsync(e => e.Id == model.ExecutionId);
            if (execution == null || execution.AuditorId != CurrentUserId)
                return Json(new { success = false, message = "This audit wasn't found or isn't assigned to you." });

            if (execution.Status is not (ExecutionStatuses.InProgress or ExecutionStatuses.Rejected))
                return Json(new { success = false, message = $"This audit is {ExecutionStatuses.Label(execution.Status).ToLowerInvariant()} and can't be changed." });

            var fileErrors = ValidateFiles(model);
            if (fileErrors.Count > 0)
                return Json(new { success = false, message = string.Join(" ", fileErrors) });

            if (NormalizeTimes(model) is { } timeError)
                return Json(new { success = false, message = timeError });

            await using var transaction = await _context.Database.BeginTransactionAsync();
            await SaveResponsesAsync(execution, model.Responses);
            await transaction.CommitAsync();

            return Json(new { success = true, savedAt = DateTime.Now.ToString("HH:mm") });
        }

        // POST: AuditExecutions/Submit  (AJAX, multipart)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(60 * 1024 * 1024)]
        public async Task<IActionResult> Submit(AuditSubmissionModel model)
        {
            var execution = await _context.AuditExecutions
                .Include(e => e.Schedule)
                .FirstOrDefaultAsync(e => e.Id == model.ExecutionId);

            if (execution == null || execution.AuditorId != CurrentUserId)
                return Json(new { success = false, message = "This audit wasn't found or isn't assigned to you." });

            if (execution.Status is not (ExecutionStatuses.InProgress or ExecutionStatuses.Rejected))
                return Json(new { success = false, message = $"This audit is {ExecutionStatuses.Label(execution.Status).ToLowerInvariant()} and can't be submitted." });

            var fileErrors = ValidateFiles(model);
            if (fileErrors.Count > 0)
                return Json(new { success = false, message = string.Join(" ", fileErrors) });

            if (NormalizeTimes(model) is { } timeError)
                return Json(new { success = false, message = timeError });

            await using var transaction = await _context.Database.BeginTransactionAsync();
            await SaveResponsesAsync(execution, model.Responses);

            var missing = await FindIncompleteItemsAsync(execution);
            if (missing.Count > 0)
            {
                // Keep the answers given so far, but don't submit.
                await transaction.CommitAsync();
                return Json(new
                {
                    success = false,
                    message = $"{missing.Count} question(s) still need an answer or a reason. They're highlighted on the page.",
                    missing,
                });
            }

            await CalculateResultAsync(execution);
            await _responsibility.ApplyAsync(execution.Id);
            execution.Status = ExecutionStatuses.Completed;
            execution.SubmissionDate = DateTime.UtcNow;
            execution.RejectionReason = null;
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Execution {ExecutionId} submitted by {UserId}", execution.Id, CurrentUserId);
            return Json(new { success = true, redirect = Url.Action(nameof(Details), new { id = execution.Id }) });
        }

        // ------------------------------------------------------------------ details / review

        // GET: AuditExecutions/Details/5
        public async Task<IActionResult> Details(long id)
        {
            var execution = await _context.AuditExecutions
                .Include(e => e.Auditor)
                .Include(e => e.AuditExecutionResult)
                .Include(e => e.Schedule).ThenInclude(s => s.Template)
                .Include(e => e.Schedule).ThenInclude(s => s.Site).ThenInclude(s => s.Airport)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (execution == null)
                return NotFound();

            if (User.IsInRole(Roles.Auditor) ? execution.AuditorId != CurrentUserId : !await _scope.CanAccessAirportAsync(execution.Schedule.Site.AirportId))
                return Forbid();

            ViewBag.Render = await BuildRenderModelAsync(execution);
            ViewBag.CanReview = IsReviewer && execution.Status == ExecutionStatuses.Completed;
            return View(execution);
        }

        // POST: AuditExecutions/Review/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.AdminOrManager)]
        public async Task<IActionResult> Review(long id, string decision, string? reason)
        {
            var execution = await _context.AuditExecutions
                .Include(e => e.Schedule)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (execution == null)
                return NotFound();

            if (execution.Status != ExecutionStatuses.Completed)
            {
                TempData["Error"] = "Only submitted audits can be reviewed.";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (decision == "close")
            {
                execution.Status = ExecutionStatuses.Closed;
                execution.Schedule.Status = ScheduleStatuses.Completed;
                TempData["Success"] = "The audit was approved and closed.";
            }
            else if (decision == "return")
            {
                if (string.IsNullOrWhiteSpace(reason))
                {
                    TempData["Error"] = "Tell the auditor what needs to change.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                execution.Status = ExecutionStatuses.Rejected;
                execution.RejectionReason = reason.Trim();
                TempData["Success"] = "The audit was returned to the auditor for changes.";
            }
            else
            {
                return BadRequest();
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: AuditExecutions/Attachment/5
        public async Task<IActionResult> Attachment(long id)
        {
            var attachment = await _context.AuditResponseAttachments
                .Include(a => a.Response).ThenInclude(r => r.Execution)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (attachment == null)
                return NotFound();

            if (User.IsInRole(Roles.Auditor) && attachment.Response.Execution.AuditorId != CurrentUserId)
                return Forbid();

            var path = _storage.Resolve(attachment.FileUrl);
            if (path == null)
                return NotFound();

            if (!new FileExtensionContentTypeProvider().TryGetContentType(path, out var contentType))
                contentType = "application/octet-stream";

            return PhysicalFile(path, contentType, attachment.FileName, enableRangeProcessing: true);
        }

        // ------------------------------------------------------------------ helpers

        private async Task<AuditExecution?> LoadOwnExecutionAsync(long executionId, bool includeSchedule)
        {
            var query = _context.AuditExecutions.AsQueryable();
            if (includeSchedule)
                query = query
                    .Include(e => e.Schedule).ThenInclude(s => s.Template)
                    .Include(e => e.Schedule).ThenInclude(s => s.Site).ThenInclude(s => s.Airport);

            var execution = await query.FirstOrDefaultAsync(e => e.Id == executionId);
            return execution != null && execution.AuditorId == CurrentUserId ? execution : null;
        }

        private IActionResult RedirectToNextStep(AuditExecution execution) => execution.Status switch
        {
            ExecutionStatuses.PendingAcceptance => RedirectToAction(nameof(Accept), new { executionId = execution.Id }),
            ExecutionStatuses.InProgress or ExecutionStatuses.Rejected => RedirectToAction(nameof(Execute), new { executionId = execution.Id }),
            _ => RedirectToAction(nameof(Details), new { id = execution.Id }),
        };

        private async Task<AuditExecutionRenderViewModel> BuildRenderModelAsync(AuditExecution execution)
        {
            var items = await _context.AuditTemplateItems
                .Include(t => t.QuestionBank).ThenInclude(q => q.QuestionType)
                .Include(t => t.QuestionBank).ThenInclude(q => q.Category)
                .Include(t => t.QuestionBank).ThenInclude(q => q.QuestionBankOptions)
                .Include(t => t.QuestionBank).ThenInclude(q => q.QuestionBankReasons)
                .Include(t => t.QuestionBank).ThenInclude(q => q.ReferenceList).ThenInclude(l => l!.Items)
                .Where(t => t.TemplateId == execution.Schedule.TemplateId)
                .OrderBy(t => t.Sequence).ThenBy(t => t.Id)
                .AsNoTracking()
                .ToListAsync();

            var responses = await _context.AuditResponses
                .Include(r => r.Reasons)
                .Include(r => r.AuditResponseAttachments)
                .Where(r => r.ExecutionId == execution.Id)
                .AsNoTracking()
                .ToDictionaryAsync(r => r.TemplateItemId);

            var vm = new AuditExecutionRenderViewModel
            {
                ExecutionId = execution.Id,
                AuditName = execution.Schedule.Template.Name,
                SiteName = execution.Schedule.Site.Name,
                AirportName = execution.Schedule.Site.Airport?.Name,
                AuditorName = execution.Auditor?.DisplayName ?? "",
                ScheduledDate = execution.Schedule.ScheduledDate,
                DueDate = execution.Schedule.DueDate,
                Status = execution.Status,
                ReturnReason = execution.Status == ExecutionStatuses.Rejected ? execution.RejectionReason : null,
                Questions = items.Select(t =>
                {
                    responses.TryGetValue(t.Id, out var r);
                    var type = t.QuestionBank.QuestionType.Name;
                    var multi = type is "CHECKLIST" or "MULTIPLE_CHOICE";
                    return new QuestionViewModel
                    {
                        TemplateItemId = t.Id,
                        QuestionText = t.QuestionBank.QuestionText,
                        Section = t.Section,
                        CategoryName = t.QuestionBank.Category?.Name,
                        Recommendation = t.QuestionBank.ServiceStandardRecommendation,
                        QuestionTypeName = type,
                        IsMandatory = t.Mandatory,
                        Options = t.QuestionBank.QuestionBankOptions.OrderBy(o => o.DisplayOrder).ThenBy(o => o.Id).ToList(),
                        ReasonOptions = t.QuestionBank.QuestionBankReasons.OrderBy(o => o.Id).ToList(),
                        SelectedOptionId = r?.SelectedOptionId,
                        SelectedOptionIds = multi ? ParseIds(r?.AnswerValue) : new(),
                        AnswerValue = multi ? null : r?.AnswerValue,
                        Comment = r?.Comment,
                        Responsibility = r?.Responsibility ?? t.Responsibility,
                        ServiceProvider = r?.ServiceProvider ?? t.ServiceProvider,
                        ListCode = t.QuestionBank.ReferenceList?.Code,
                        ListItems = t.QuestionBank.ReferenceList?.Items
                            .Where(i => i.IsActive && (i.AirportId == null || i.AirportId == execution.Schedule.Site.AirportId))
                            .OrderBy(i => i.DisplayOrder).ThenBy(i => i.Name)
                            .ToList() ?? new(),
                        SelectedReasonIds = r?.Reasons.Select(x => x.Id).ToHashSet() ?? new(),
                        Attachments = r?.AuditResponseAttachments.ToList() ?? new(),
                    };
                }).ToList(),
            };

            // Questions are shown grouped by section; number them in the order the auditor sees them.
            vm.Layout = Auditor.ViewModels.Shared.SectionTree.Build(vm.Questions, q => q.Section);
            vm.Questions = vm.Layout.Ordered.ToList();
            var number = 0;
            vm.Questions.ForEach(q => q.Number = ++number);
            return vm;
        }

        private static HashSet<long> ParseIds(string? csv) =>
            (csv ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => long.TryParse(s, out var v) ? v : 0)
                .Where(v => v > 0)
                .ToHashSet();

        // Times are stored as HH:mm:ss; older HH:mm answers and typed values are brought to that form. Returns the problem, if any.
        private string? NormalizeTimes(AuditSubmissionModel model)
        {
            var itemIds = (model.Responses ?? new()).Select(r => r.TemplateItemId).ToList();
            var timeItems = _context.AuditTemplateItems
                .Where(t => itemIds.Contains(t.Id) && t.QuestionBank.QuestionType.Name == "TIME")
                .Select(t => t.Id)
                .ToHashSet();

            foreach (var response in (model.Responses ?? new()).Where(r => timeItems.Contains(r.TemplateItemId) && !string.IsNullOrWhiteSpace(r.AnswerValue)))
            {
                if (!TimeOnly.TryParseExact(response.AnswerValue!.Trim(), new[] { "HH:mm:ss", "H:mm:ss", "HH:mm", "H:mm" }, out var time))
                    return $"\"{response.AnswerValue}\" isn't a valid time. Use hours, minutes and seconds, for example 14:05:30.";
                response.AnswerValue = time.ToString("HH:mm:ss");
            }

            return null;
        }

        private static List<string> ValidateFiles(AuditSubmissionModel model) =>
            (model.Responses ?? new())
                .SelectMany(r => r.Attachments ?? new())
                .Select(AttachmentStorage.Validate)
                .Where(e => e != null)
                .Select(e => e!)
                .ToList();

        // Upserts one AuditResponse per template item, so drafts and re-submissions never duplicate rows.
        private async Task SaveResponsesAsync(AuditExecution execution, List<AuditResponseSubmission>? submissions)
        {
            if (submissions == null || submissions.Count == 0)
                return;

            var itemIds = submissions.Select(s => s.TemplateItemId).ToList();
            var templateId = await _context.AuditSchedules.Where(s => s.Id == execution.ScheduleId).Select(s => s.TemplateId).FirstAsync();
            var items = await _context.AuditTemplateItems
                .Include(t => t.QuestionBank).ThenInclude(q => q.QuestionType)
                .Include(t => t.QuestionBank).ThenInclude(q => q.QuestionBankOptions)
                .Where(t => itemIds.Contains(t.Id) && t.TemplateId == templateId)
                .ToDictionaryAsync(t => t.Id);

            var existing = await _context.AuditResponses
                .Include(r => r.Reasons)
                .Where(r => r.ExecutionId == execution.Id && itemIds.Contains(r.TemplateItemId))
                .ToDictionaryAsync(r => r.TemplateItemId);

            var reasonIds = submissions.SelectMany(s => s.SelectedReasonIds ?? new()).Distinct().ToList();
            var reasons = await _context.QuestionBankReasons.Where(r => reasonIds.Contains(r.Id)).ToDictionaryAsync(r => r.Id);

            var pendingFiles = new List<(AuditResponse Response, IFormFile File)>();

            foreach (var submission in submissions)
            {
                if (!items.TryGetValue(submission.TemplateItemId, out var item))
                    continue;

                if (!existing.TryGetValue(item.Id, out var response))
                {
                    response = new AuditResponse { ExecutionId = execution.Id, TemplateItemId = item.Id };
                    _context.AuditResponses.Add(response);
                    existing[item.Id] = response;
                }

                var question = item.QuestionBank;
                var type = question.QuestionType.Name;
                var options = question.QuestionBankOptions;

                response.Comment = Trim(submission.Comment, 2000);
                response.SelectedOptionId = null;
                response.AnswerValue = null;
                response.Compliant = null;

                if (type is "CHECKLIST" or "MULTIPLE_CHOICE")
                {
                    var chosen = (submission.SelectedOptionIds ?? new()).Where(id => options.Any(o => o.Id == id)).Distinct().ToList();
                    response.AnswerValue = chosen.Count > 0 ? string.Join(",", chosen) : null;
                }
                else if (submission.SelectedOptionId is long optionId && options.FirstOrDefault(o => o.Id == optionId) is { } option)
                {
                    response.SelectedOptionId = option.Id;
                    response.Compliant = ComplianceOf(type, option);
                }
                else
                {
                    response.AnswerValue = Trim(submission.AnswerValue, 2000);
                }

                // Reasons only apply to an answer that asks for one.
                response.Reasons.Clear();
                var needsReason = response.SelectedOptionId != null && options.First(o => o.Id == response.SelectedOptionId).RequiresReason;
                if (needsReason)
                {
                    foreach (var id in submission.SelectedReasonIds ?? new())
                        if (reasons.TryGetValue(id, out var reason) && reason.QuestionBankId == question.Id)
                            response.Reasons.Add(reason);

                    var custom = Trim(submission.CustomReason, 1000);
                    if (custom != null)
                    {
                        var match = await _context.QuestionBankReasons.FirstOrDefaultAsync(r => r.QuestionBankId == question.Id && r.ReasonText == custom);
                        response.Reasons.Add(match ?? new QuestionBankReason { QuestionBankId = question.Id, ReasonText = custom });
                    }
                }

                foreach (var file in submission.Attachments ?? new())
                    pendingFiles.Add((response, file));
            }

            await _context.SaveChangesAsync();

            if (pendingFiles.Count == 0)
                return;

            foreach (var (response, file) in pendingFiles)
            {
                var key = await _storage.SaveAsync(execution.Id, response.Id, file);
                _context.AuditResponseAttachments.Add(new AuditResponseAttachment
                {
                    ResponseId = response.Id,
                    FileName = Path.GetFileName(file.FileName),
                    FileUrl = key,
                    ContentType = file.ContentType,
                    FileSize = file.Length,
                    UploadedDate = DateTime.UtcNow,
                });
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>Compliant = true / false for compliance-style answers; null when not applicable or not a compliance question.</summary>
        private static bool? ComplianceOf(string type, QuestionBankOption option)
        {
            if (type != "YES_NO")
                return null;
            if (string.Equals(option.OptionValue, "NA", StringComparison.OrdinalIgnoreCase))
                return null;
            return !option.RequiresReason;
        }

        private async Task<List<long>> FindIncompleteItemsAsync(AuditExecution execution)
        {
            var templateId = await _context.AuditSchedules.Where(s => s.Id == execution.ScheduleId).Select(s => s.TemplateId).FirstAsync();
            var items = await _context.AuditTemplateItems
                .Include(t => t.QuestionBank).ThenInclude(q => q.QuestionType)
                .Where(t => t.TemplateId == templateId)
                .ToListAsync();

            var responses = await _context.AuditResponses
                .Include(r => r.Reasons)
                .Include(r => r.SelectedOption)
                .Where(r => r.ExecutionId == execution.Id)
                .ToDictionaryAsync(r => r.TemplateItemId);

            var missing = new List<long>();
            foreach (var item in items)
            {
                responses.TryGetValue(item.Id, out var r);
                var answered = r != null && (r.SelectedOptionId != null || !string.IsNullOrWhiteSpace(r.AnswerValue));

                if (item.Mandatory && !answered)
                    missing.Add(item.Id);
                else if (r?.SelectedOption?.RequiresReason == true && r.Reasons.Count == 0)
                    missing.Add(item.Id);
            }

            return missing;
        }

        private async Task CalculateResultAsync(AuditExecution execution)
        {
            var responses = await _context.AuditResponses
                .Include(r => r.TemplateItem).ThenInclude(t => t.QuestionBank).ThenInclude(q => q.QuestionType)
                .Include(r => r.SelectedOption)
                .Where(r => r.ExecutionId == execution.Id)
                .ToListAsync();

            var compliance = responses.Where(r => r.TemplateItem.QuestionBank.QuestionType.Name == "YES_NO" && r.SelectedOptionId != null).ToList();
            var compliant = compliance.Count(r => r.Compliant == true);
            var nonCompliant = compliance.Count(r => r.Compliant == false);
            var notApplicable = compliance.Count(r => r.Compliant == null);
            var scored = compliant + nonCompliant;

            var result = await _context.AuditExecutionResults.FirstOrDefaultAsync(r => r.ExecutionId == execution.Id);
            if (result == null)
            {
                result = new AuditExecutionResult { ExecutionId = execution.Id };
                _context.AuditExecutionResults.Add(result);
            }

            result.TotalQuestions = responses.Count;
            result.CompliantCount = compliant;
            result.NonCompliantCount = nonCompliant;
            result.NotApplicableCount = notApplicable;
            result.ScorePercentage = scored == 0 ? 100m : Math.Round(compliant * 100m / scored, 2);
            result.CalculatedDate = DateTime.UtcNow;
        }

        private static string? Trim(string? value, int max)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;
            value = value.Trim();
            return value.Length > max ? value[..max] : value;
        }
    }
}
