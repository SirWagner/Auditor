using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Auditor.Models;
using Auditor.ViewModels.AuditExecution;

namespace Auditor.Controllers
{
    public class AuditExecutionsController : Controller
    {
        private readonly AuditorContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AuditExecutionsController(AuditorContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        private bool TryGetCurrentUserId(out long userId)
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return long.TryParse(claim, out userId);
        }

        /*
                ========================================
                START AUDIT
                Creates execution record, pending the auditor's acceptance
                ========================================
                */

        public async Task<IActionResult> Start(long scheduleId)
        {
            var schedule = await _context.AuditSchedules
                .FirstOrDefaultAsync(s => s.Id == scheduleId);

            if (schedule == null)
                return NotFound();

            if (!TryGetCurrentUserId(out var auditorId))
                return Forbid();

            var execution = new AuditExecution
            {
                ScheduleId = scheduleId,
                AuditorId = auditorId,
                Status = "PENDING_ACCEPTANCE",
                OriginalAuditDate = schedule.ScheduledDate
            };

            _context.AuditExecutions.Add(execution);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Accept), new { executionId = execution.Id });
        }

        /*
        ========================================
        ACCEPT / REJECT AUDIT
        ========================================
        */

        public async Task<IActionResult> Accept(long executionId)
        {
            var execution = await _context.AuditExecutions
                .Include(e => e.Schedule)
                    .ThenInclude(s => s.Template)
                .Include(e => e.Schedule)
                    .ThenInclude(s => s.Site)
                .FirstOrDefaultAsync(e => e.Id == executionId);

            if (execution == null)
                return NotFound();

            if (execution.Status != "PENDING_ACCEPTANCE")
                return RedirectToAction(nameof(Details), new { id = executionId });

            return View(execution);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Accept(long executionId, bool confirm)
        {
            var execution = await _context.AuditExecutions.FirstOrDefaultAsync(e => e.Id == executionId);
            if (execution == null)
                return NotFound();

            if (execution.Status != "PENDING_ACCEPTANCE")
                return RedirectToAction(nameof(Details), new { id = executionId });

            execution.AcceptanceDate = DateTime.UtcNow;
            execution.Status = "IN_PROGRESS";
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Execute), new { executionId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(long executionId, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["Error"] = "A rejection reason is required.";
                return RedirectToAction(nameof(Accept), new { executionId });
            }

            var execution = await _context.AuditExecutions
                .Include(e => e.Schedule)
                .FirstOrDefaultAsync(e => e.Id == executionId);

            if (execution == null)
                return NotFound();

            if (execution.Status != "PENDING_ACCEPTANCE")
                return RedirectToAction(nameof(Details), new { id = executionId });

            execution.Status = "REJECTED";
            execution.RejectionReason = reason;

            if (execution.Schedule != null)
                execution.Schedule.Status = "SCHEDULED";

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        /*
        ========================================
        EXECUTE AUDIT
        ========================================
        */

        public async Task<IActionResult> Execute(long executionId)
        {
            var execution = await _context.AuditExecutions
                .Include(e => e.Schedule)
                    .ThenInclude(s => s.Template)
                .Include(e => e.Schedule)
                    .ThenInclude(s => s.Site)
                .Include(a=>a.Auditor)
                .FirstOrDefaultAsync(e => e.Id == executionId);

            if (execution == null)
                return NotFound();

            if (execution.Status != "IN_PROGRESS" && execution.Status != "REJECTED")
                return RedirectToAction(nameof(Details), new { id = executionId });

            var templateItems = await _context.AuditTemplateItems
                .Include(t => t.QuestionBank)
                    .ThenInclude(q => q.QuestionType)
                .Include(t => t.QuestionBank)
                    .ThenInclude(q => q.QuestionBankOptions)
                .Include(t => t.QuestionBank)
                    .ThenInclude(q => q.QuestionBankReasons)
                .Where(t => t.TemplateId == execution.Schedule.TemplateId)
                .OrderBy(t => t.Sequence)
                .ToListAsync();

            var vm = new AuditExecutionRenderViewModel
            {
                ExecutionId = execution.Id,
                AuditName = execution.Schedule.Template.Name,
                SiteName = execution.Schedule.Site.Name,
                Status = execution.Status,

                Questions = templateItems.Select(t => new QuestionViewModel
                {
                    TemplateItemId = t.Id,
                    QuestionText = t.QuestionBank.QuestionText,
                    QuestionTypeId = t.QuestionBank.QuestionTypeId,
                    QuestionTypeName = t.QuestionBank.QuestionType.Name,
                    IsMandatory = t.Mandatory,
                    RequiresReason = t.QuestionBank.QuestionBankOptions
                                        .Any(o => o.RequiresReason),
                    Options = t.QuestionBank.QuestionBankOptions.ToList(),
                    ReasonOptions = t.QuestionBank.QuestionBankReasons.ToList()
                }).ToList()
            };

            return View(vm);
        }

        public async Task<IActionResult> Index()
        {
            var audits = await _context.AuditExecutions
                .Include(x => x.Schedule)
                    .ThenInclude(s => s.Template)
                .Include(x => x.Schedule)
                    .ThenInclude(s => s.Site)
                .Include(x => x.Auditor)
                .ToListAsync();

            return View(audits);
        }


        [HttpPost]
        public async Task<IActionResult> Finalize(long id)
        {
            var execution = await _context.AuditExecutions
                .FirstOrDefaultAsync(e => e.Id == id);

            if (execution == null)
                return Json(new { success = false, message = "Execution not found." });

            if (execution.Status != "COMPLETED")
                return Json(new { success = false, message = "Only completed audits can be finalized." });

            execution.Status = "CLOSED";
            execution.SubmissionDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
        /*
        -------------------------------------------------
        PAGE 2
        Audit Details
        -------------------------------------------------
        */

        // GET: AuditExecution/Details/5
        public async Task<IActionResult> Details(long id)
        {
            var execution = await _context.AuditExecutions
                .Include(e => e.Auditor)
                .Include(e => e.Schedule)
                    .ThenInclude(s => s.Template)
                        .ThenInclude(t => t.AuditTemplateItems)
                            .ThenInclude(i => i.QuestionBank)
                                .ThenInclude(q => q.QuestionType)
                .Include(e => e.Schedule)
                    .ThenInclude(s => s.Template)
                        .ThenInclude(t => t.AuditTemplateItems)
                            .ThenInclude(i => i.QuestionBank)
                                .ThenInclude(q => q.QuestionBankOptions)
                .Include(e => e.AuditResponses)
                    .ThenInclude(r => r.SelectedOption)
                .Include(e => e.AuditResponses)
                    .ThenInclude(r => r.Reasons)
                .Include(e => e.AuditResponses)
                    .ThenInclude(r => r.AuditResponseAttachments)
                .Include(e => e.AuditAttachments)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (execution == null)
                return NotFound();

            return View(execution);
        }
        [HttpPost]
        public async Task<IActionResult> Submit(AuditSubmissionModel model)
        {
            var execution = await _context.AuditExecutions.FirstOrDefaultAsync(x => x.Id == model.ExecutionId);

            if (execution == null)
                return Json(new { success = false, message = "Execution not found." });

            if (execution.Status != "IN_PROGRESS")
                return Json(new { success = false, message = $"Cannot submit an execution with status {execution.Status}." });

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                await SaveResponsesAsync(execution, model.Responses);

                execution.Status = "COMPLETED";
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = ex.InnerException?.Message ?? ex.Message });
            }
        }

        // Persists responses (as an upsert keyed on TemplateItemId) without changing execution status.
        // Used for the in-progress draft-save workflow: the auditor can leave and come back without
        // losing answers already entered.
        [HttpPost]
        public async Task<IActionResult> SaveDraft(AuditSubmissionModel model)
        {
            var execution = await _context.AuditExecutions.FirstOrDefaultAsync(x => x.Id == model.ExecutionId);

            if (execution == null)
                return Json(new { success = false, message = "Execution not found." });

            if (execution.Status != "IN_PROGRESS")
                return Json(new { success = false, message = $"Cannot save a draft for status {execution.Status}." });

            try
            {
                await SaveResponsesAsync(execution, model.Responses);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.InnerException?.Message ?? ex.Message });
            }
        }

        // Re-submits a previously rejected execution: updates the existing responses in place and
        // moves the execution back to COMPLETED for review.
        [HttpPost]
        public async Task<IActionResult> Resubmit(AuditSubmissionModel model)
        {
            var execution = await _context.AuditExecutions.FirstOrDefaultAsync(x => x.Id == model.ExecutionId);

            if (execution == null)
                return Json(new { success = false, message = "Execution not found." });

            if (execution.Status != "REJECTED")
                return Json(new { success = false, message = "Only rejected executions can be resubmitted." });

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                await SaveResponsesAsync(execution, model.Responses);

                execution.Status = "COMPLETED";
                execution.RejectionReason = null;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = ex.InnerException?.Message ?? ex.Message });
            }
        }

        // Shared upsert used by Submit/SaveDraft/Resubmit: one AuditResponse row per TemplateItemId,
        // updated in place on repeated calls so drafts and resubmits don't duplicate rows.
        private async Task SaveResponsesAsync(AuditExecution execution, List<AuditResponseSubmission> responses)
        {
            if (responses == null || responses.Count == 0)
                return;

            var templateItemIds = responses.Select(r => r.TemplateItemId).ToList();

            var templateItems = await _context.AuditTemplateItems
                .Where(x => templateItemIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id);

            foreach (var response in responses)
            {
                if (!templateItems.TryGetValue(response.TemplateItemId, out var templateItem))
                    continue;

                var entity = await _context.AuditResponses
                    .Include(r => r.Reasons)
                    .FirstOrDefaultAsync(r => r.ExecutionId == execution.Id && r.TemplateItemId == response.TemplateItemId);

                if (entity == null)
                {
                    entity = new AuditResponse
                    {
                        ExecutionId = execution.Id,
                        TemplateItemId = response.TemplateItemId
                    };

                    _context.AuditResponses.Add(entity);
                    await _context.SaveChangesAsync(); // need the Id for attachments below
                }

                entity.SelectedOptionId = response.SelectedOptionId;
                entity.Comment = response.Comment;
                entity.Compliant = response.Compliant;

                entity.Reasons.Clear();

                if (response.SelectedReasonIds is { Count: > 0 })
                {
                    var existingReasons = await _context.QuestionBankReasons
                        .Where(r => response.SelectedReasonIds.Contains(r.Id))
                        .ToListAsync();

                    foreach (var reason in existingReasons)
                        entity.Reasons.Add(reason);
                }

                if (!string.IsNullOrWhiteSpace(response.CustomReason))
                {
                    var newReason = new QuestionBankReason
                    {
                        QuestionBankId = templateItem.QuestionBankId,
                        ReasonText = response.CustomReason
                    };

                    _context.QuestionBankReasons.Add(newReason);
                    await _context.SaveChangesAsync();

                    entity.Reasons.Add(newReason);
                }

                await _context.SaveChangesAsync();

                if (response.Attachments is { Count: > 0 })
                {
                    var uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", execution.Id.ToString());
                    Directory.CreateDirectory(uploadDir);

                    foreach (var file in response.Attachments)
                    {
                        if (file.Length == 0)
                            continue;

                        var safeFileName = $"{entity.Id}_{Guid.NewGuid():N}_{Path.GetFileName(file.FileName)}";
                        var filePath = Path.Combine(uploadDir, safeFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        _context.AuditResponseAttachments.Add(new AuditResponseAttachment
                        {
                            ResponseId = entity.Id,
                            FileName = file.FileName,
                            FileUrl = $"/uploads/{execution.Id}/{safeFileName}",
                            ContentType = file.ContentType,
                            FileSize = file.Length,
                            UploadedDate = DateTime.UtcNow
                        });
                    }

                    await _context.SaveChangesAsync();
                }
            }
        }

        // GET: AuditExecutions/Create
        public IActionResult Create()
        {
            ViewData["AuditorId"] = new SelectList(_context.AppUsers, "Id", "DisplayName");
            ViewData["ScheduleId"] = new SelectList(GetScheduleOptions(), "Id", "Label");
            return View();
        }

        // POST: AuditExecutions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ScheduleId,AuditorId,Status,AcceptanceDate,RejectionReason,SubmissionDate,OriginalAuditDate")] AuditExecution auditExecution)
        {
            if (ModelState.IsValid)
            {
                _context.Add(auditExecution);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AuditorId"] = new SelectList(_context.AppUsers, "Id", "DisplayName", auditExecution.AuditorId);
            ViewData["ScheduleId"] = new SelectList(GetScheduleOptions(), "Id", "Label", auditExecution.ScheduleId);
            return View(auditExecution);
        }

        // GET: AuditExecutions/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var auditExecution = await _context.AuditExecutions.FindAsync(id);
            if (auditExecution == null)
            {
                return NotFound();
            }
            ViewData["AuditorId"] = new SelectList(_context.AppUsers, "Id", "DisplayName", auditExecution.AuditorId);
            ViewData["ScheduleId"] = new SelectList(GetScheduleOptions(), "Id", "Label", auditExecution.ScheduleId);
            return View(auditExecution);
        }

        // Builds a friendly "Template @ Site (date)" label for the schedule picker, since AuditSchedule
        // has no single display-worthy field on its own.
        private IEnumerable<object> GetScheduleOptions()
        {
            return _context.AuditSchedules
                .Include(s => s.Template)
                .Include(s => s.Site)
                .OrderByDescending(s => s.ScheduledDate)
                .Select(s => new
                {
                    s.Id,
                    Label = s.Template.Name + " @ " + s.Site.Name + " (" + s.ScheduledDate.ToString("MMM dd, yyyy") + ")"
                })
                .ToList();
        }

        // POST: AuditExecutions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,ScheduleId,AuditorId,Status,AcceptanceDate,RejectionReason,SubmissionDate,OriginalAuditDate")] AuditExecution auditExecution)
        {
            if (id != auditExecution.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(auditExecution);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AuditExecutionExists(auditExecution.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["AuditorId"] = new SelectList(_context.AppUsers, "Id", "Id", auditExecution.AuditorId);
            ViewData["ScheduleId"] = new SelectList(_context.AuditSchedules, "Id", "Status", auditExecution.ScheduleId);
            return View(auditExecution);
        }

        // GET: AuditExecutions/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var auditExecution = await _context.AuditExecutions
                .Include(a => a.Auditor)
                .Include(a => a.Schedule)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (auditExecution == null)
            {
                return NotFound();
            }

            return View(auditExecution);
        }

        // POST: AuditExecutions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var auditExecution = await _context.AuditExecutions.FindAsync(id);
            if (auditExecution != null)
            {
                _context.AuditExecutions.Remove(auditExecution);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AuditExecutionExists(long id)
        {
            return _context.AuditExecutions.Any(e => e.Id == id);
        }
    }
}
