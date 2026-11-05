using System.Security.Claims;
using Auditor.Models;
using Auditor.Models.Lookups;
using Auditor.Security;
using Auditor.Services;
using Auditor.ViewModels.AuditSchedule;
using Auditor.ViewModels.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Auditor.Controllers
{
    [Authorize(Roles = Roles.AdminOrManager)]
    public class AuditSchedulesController : Controller
    {
        private readonly AuditorContext _context;
        private readonly UserScope _scope;
        private readonly AuditFilterOptions _filterOptions;
        private readonly ILogger<AuditSchedulesController> _logger;

        public AuditSchedulesController(AuditorContext context, UserScope scope, AuditFilterOptions filterOptions, ILogger<AuditSchedulesController> logger)
        {
            _context = context;
            _scope = scope;
            _filterOptions = filterOptions;
            _logger = logger;
        }

        public static readonly (string Value, string Label)[] StatusOptions =
        {
            ("open", "Open (scheduled or in progress)"),
            (ScheduleStatuses.Scheduled, ScheduleStatuses.Label(ScheduleStatuses.Scheduled)),
            (ScheduleStatuses.InProgress, ScheduleStatuses.Label(ScheduleStatuses.InProgress)),
            (ScheduleStatuses.Completed, ScheduleStatuses.Label(ScheduleStatuses.Completed)),
            (ScheduleStatuses.Cancelled, ScheduleStatuses.Label(ScheduleStatuses.Cancelled)),
        };

        // GET: AuditSchedules?AirportId=…&Area=…&From=…  (every filter optional; default shows all)
        public async Task<IActionResult> Index([FromQuery] AuditFilter filter)
        {
            filter.Normalize();
            filter.AirportId = await _scope.NarrowAsync(filter.AirportId);

            var schedules = filter.Apply(_context.AuditSchedules
                .Include(s => s.Template)
                .Include(s => s.Site).ThenInclude(s => s.Airport)
                .Include(s => s.Auditors)
                .AsQueryable());

            schedules = filter.Status switch
            {
                null => schedules,
                "open" => schedules.Where(s => s.Status == ScheduleStatuses.Scheduled || s.Status == ScheduleStatuses.InProgress),
                _ => schedules.Where(s => s.Status == filter.Status),
            };

            ViewBag.Filter = await _filterOptions.BuildAsync(filter, "AuditSchedules", StatusOptions, showAuditor: true);
            return View(await schedules.OrderBy(s => s.ScheduledDate).ToListAsync());
        }

        // GET: AuditSchedules/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            var schedule = await _context.AuditSchedules
                .Include(a => a.Scheduler)
                .Include(a => a.Site).ThenInclude(s => s.Airport)
                .Include(a => a.Template).ThenInclude(t => t.AuditTemplateItems)
                .Include(a => a.Auditors)
                .Include(a => a.AuditExecutions).ThenInclude(e => e.Auditor)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (schedule == null || !await _scope.CanAccessAirportAsync(schedule.Site.AirportId))
                return NotFound();

            return View(schedule);
        }

        // GET: AuditSchedules/Create
        public async Task<IActionResult> Create(long? siteId, long? templateId)
        {
            var form = new AuditScheduleFormViewModel
            {
                SiteId = siteId,
                TemplateId = templateId,
                ScheduledDate = DateTime.Today.AddDays(1).AddHours(8),
                DueDate = DateTime.Today.AddDays(8).AddHours(17),
            };

            if (siteId != null)
                form.AirportId = await _context.AuditSites.Where(s => s.Id == siteId).Select(s => (long?)s.AirportId).FirstOrDefaultAsync();

            await LoadFormDataAsync(form);
            return View(form);
        }

        // POST: AuditSchedules/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AuditScheduleFormViewModel form)
        {
            form.Id = 0;
            await ValidateAsync(form);

            if (!ModelState.IsValid)
            {
                await LoadFormDataAsync(form);
                return View(form);
            }

            var schedule = new AuditSchedule
            {
                TemplateId = form.TemplateId!.Value,
                SiteId = form.SiteId!.Value,
                SchedulerId = CurrentUserId,
                ScheduledDate = form.ScheduledDate!.Value,
                DueDate = form.DueDate!.Value,
                Status = ScheduleStatuses.Scheduled,
                CreatedDate = DateTime.UtcNow,
            };

            var auditors = await _context.AppUsers.Where(u => form.AuditorIds.Contains(u.Id)).ToListAsync();
            foreach (var auditor in auditors)
                schedule.Auditors.Add(auditor);

            _context.AuditSchedules.Add(schedule);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Audit schedule {ScheduleId} created by {UserId}", schedule.Id, CurrentUserId);

            TempData["Success"] = $"Audit scheduled for {schedule.ScheduledDate:dd MMM yyyy HH:mm} and assigned to {auditors.Count} auditor(s).";
            return RedirectToAction(nameof(Details), new { id = schedule.Id });
        }

        // GET: AuditSchedules/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            var schedule = await _context.AuditSchedules
                .Include(s => s.Auditors)
                .Include(s => s.Site)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (schedule == null || !await _scope.CanAccessAirportAsync(schedule.Site.AirportId))
                return NotFound();

            if (schedule.Status != ScheduleStatuses.Scheduled)
            {
                TempData["Error"] = $"This schedule is {ScheduleStatuses.Label(schedule.Status).ToLowerInvariant()} and can no longer be changed.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var form = new AuditScheduleFormViewModel
            {
                Id = schedule.Id,
                AirportId = schedule.Site.AirportId,
                SiteId = schedule.SiteId,
                TemplateId = schedule.TemplateId,
                ScheduledDate = schedule.ScheduledDate,
                DueDate = schedule.DueDate,
                AuditorIds = schedule.Auditors.Select(a => a.Id).ToList(),
            };

            await LoadFormDataAsync(form);
            return View(form);
        }

        // POST: AuditSchedules/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, AuditScheduleFormViewModel form)
        {
            if (id != form.Id)
                return NotFound();

            var schedule = await _context.AuditSchedules
                .Include(s => s.Auditors)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (schedule == null)
                return NotFound();

            if (schedule.Status != ScheduleStatuses.Scheduled)
            {
                TempData["Error"] = "This schedule has already started and can no longer be changed.";
                return RedirectToAction(nameof(Details), new { id });
            }

            await ValidateAsync(form);
            if (string.IsNullOrWhiteSpace(form.ModificationReason))
                ModelState.AddModelError(nameof(form.ModificationReason), "Give a reason for the change; it's kept on the schedule's history.");

            if (!ModelState.IsValid)
            {
                await LoadFormDataAsync(form);
                return View(form);
            }

            schedule.TemplateId = form.TemplateId!.Value;
            schedule.SiteId = form.SiteId!.Value;
            schedule.ScheduledDate = form.ScheduledDate!.Value;
            schedule.DueDate = form.DueDate!.Value;
            schedule.ModificationReason = form.ModificationReason!.Trim();

            foreach (var auditor in schedule.Auditors.Where(a => !form.AuditorIds.Contains(a.Id)).ToList())
                schedule.Auditors.Remove(auditor);

            var toAdd = form.AuditorIds.Except(schedule.Auditors.Select(a => a.Id)).ToList();
            foreach (var auditor in await _context.AppUsers.Where(u => toAdd.Contains(u.Id)).ToListAsync())
                schedule.Auditors.Add(auditor);

            await _context.SaveChangesAsync();

            TempData["Success"] = "The schedule was updated.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: AuditSchedules/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(long id, string? reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["Error"] = "Enter a cancellation reason.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var schedule = await _context.AuditSchedules.FirstOrDefaultAsync(s => s.Id == id);
            if (schedule == null)
                return NotFound();

            if (schedule.Status is ScheduleStatuses.Completed or ScheduleStatuses.Cancelled)
            {
                TempData["Error"] = $"This schedule is already {ScheduleStatuses.Label(schedule.Status).ToLowerInvariant()}.";
                return RedirectToAction(nameof(Details), new { id });
            }

            schedule.Status = ScheduleStatuses.Cancelled;
            schedule.CancellationReason = reason.Trim();
            await _context.SaveChangesAsync();

            TempData["Success"] = "The schedule was cancelled.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: AuditSchedules/Delete/5
        // Hard delete only for schedules that never started; anything further along must be cancelled.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(long id)
        {
            var schedule = await _context.AuditSchedules
                .Include(s => s.AuditExecutions)
                .Include(s => s.Auditors)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (schedule == null)
                return RedirectToAction(nameof(Index));

            if (schedule.Status != ScheduleStatuses.Scheduled || schedule.AuditExecutions.Count > 0)
            {
                TempData["Error"] = "This schedule has started and can't be deleted. Cancel it instead.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var changeRequests = await _context.ScheduleChangeRequests.Where(r => r.ScheduleId == id).ToListAsync();
            _context.ScheduleChangeRequests.RemoveRange(changeRequests);
            schedule.Auditors.Clear();
            _context.AuditSchedules.Remove(schedule);
            await _context.SaveChangesAsync();

            TempData["Success"] = "The schedule was deleted.";
            return RedirectToAction(nameof(Index));
        }

        // GET: AuditSchedules/SiteOptions?airportId=5
        // Sites at the airport, each with the templates allowed there and which of those are linked to it.
        [HttpGet]
        public async Task<IActionResult> SiteOptions(long airportId)
        {
            if (!await _scope.CanAccessAirportAsync(airportId))
                return Json(Array.Empty<object>());

            var sites = await _context.AuditSites
                .Where(s => s.AirportId == airportId && s.Status == AuditSiteLookups.Active)
                .OrderBy(s => s.Name)
                .Select(s => new { s.Id, s.Name, s.AirportId })
                .ToListAsync();

            var templates = await ActiveTemplateRulesAsync();

            return Json(sites.Select(s => new
            {
                s.Id,
                s.Name,
                templateIds = templates.Where(t => t.AllowedAt(s.AirportId, s.Id)).Select(t => t.Id).ToList(),
                linkedTemplateIds = templates.Where(t => t.SiteIds.Contains(s.Id)).Select(t => t.Id).ToList(),
            }));
        }

        /// <summary>
        /// Where a template may be scheduled: its airport (blank = any) and its site list (empty = any site).
        /// </summary>
        private sealed record TemplateRule(long Id, long? AirportId, List<long> SiteIds)
        {
            public bool AllowedAt(long airportId, long siteId) =>
                (AirportId == null || AirportId == airportId) && (SiteIds.Count == 0 || SiteIds.Contains(siteId));
        }

        private async Task<List<TemplateRule>> ActiveTemplateRulesAsync() =>
            (await _context.AuditTemplates
                .Where(t => t.IsActive)
                .Select(t => new { t.Id, t.AirportId, SiteIds = t.Sites.Select(s => s.Id).ToList() })
                .ToListAsync())
            .Select(t => new TemplateRule(t.Id, t.AirportId, t.SiteIds))
            .ToList();

        private async Task ValidateAsync(AuditScheduleFormViewModel form)
        {
            form.AuditorIds = (form.AuditorIds ?? new()).Distinct().ToList();

            ModelState.Clear();
            TryValidateModel(form);

            AuditSite? site = null;
            if (form.SiteId != null)
            {
                site = await _context.AuditSites.FirstOrDefaultAsync(s => s.Id == form.SiteId);
                if (site == null || site.Status != AuditSiteLookups.Active)
                    ModelState.AddModelError(nameof(form.SiteId), "Select an active audit site.");
                else if (form.AirportId != null && site.AirportId != form.AirportId)
                    ModelState.AddModelError(nameof(form.SiteId), "Select a site at the selected airport.");
                else if (!await _scope.CanAccessAirportAsync(site.AirportId))
                    ModelState.AddModelError(nameof(form.SiteId), "You can only schedule audits at your own airport.");
            }

            if (form.TemplateId != null)
            {
                var rule = (await ActiveTemplateRulesAsync()).FirstOrDefault(t => t.Id == form.TemplateId);
                if (rule == null)
                    ModelState.AddModelError(nameof(form.TemplateId), "Select an active audit template.");
                else if (site != null && !rule.AllowedAt(site.AirportId, site.Id))
                    ModelState.AddModelError(nameof(form.TemplateId), "This template isn't available for the selected site. Pick one from the list.");
            }

            if (form.ScheduledDate != null && form.DueDate != null && form.DueDate < form.ScheduledDate)
                ModelState.AddModelError(nameof(form.DueDate), "The due date can't be before the scheduled date.");

            if (form.IsNew && form.ScheduledDate != null && form.ScheduledDate < DateTime.Today)
                ModelState.AddModelError(nameof(form.ScheduledDate), "The scheduled date can't be in the past.");

            if (form.AuditorIds.Count == 0)
            {
                ModelState.AddModelError(nameof(form.AuditorIds), "Assign at least one auditor.");
            }
            else
            {
                var auditors = await _context.AppUsers
                    .Where(u => form.AuditorIds.Contains(u.Id))
                    .Select(u => new { u.IsActive, u.Role, u.AirportId })
                    .ToListAsync();
                if (auditors.Count != form.AuditorIds.Count || auditors.Any(u => !u.IsActive || u.Role != Roles.Auditor))
                    ModelState.AddModelError(nameof(form.AuditorIds), "Only active users with the Auditor role can be assigned.");
                else if (site != null && auditors.Any(u => u.AirportId != null && u.AirportId != site.AirportId))
                    ModelState.AddModelError(nameof(form.AuditorIds), "One or more auditors work at a different airport. Assign auditors from this airport, or ones who cover all airports.");
            }
        }

        private async Task LoadFormDataAsync(AuditScheduleFormViewModel form)
        {
            var scopeAirport = await _scope.AirportIdAsync();
            form.AirportId ??= scopeAirport;

            ViewBag.Airports = await _context.Airports
                .Where(a => (a.IsActive || a.Id == form.AirportId) && (scopeAirport == null || a.Id == scopeAirport))
                .OrderBy(a => a.Name)
                .ToListAsync();

            ViewBag.Templates = await _context.AuditTemplates
                .Where(t => t.IsActive || t.Id == form.TemplateId)
                .Where(t => scopeAirport == null || t.AirportId == null || t.AirportId == scopeAirport || t.Id == form.TemplateId)
                .OrderBy(t => t.Name)
                .Select(t => new TemplateOption(t.Id, t.Name, t.Version, t.AuditTemplateItems.Count))
                .ToListAsync();

            ViewBag.Auditors = await _context.AppUsers
                .Where(u => (u.IsActive && u.Role == Roles.Auditor && (scopeAirport == null || u.AirportId == null || u.AirportId == scopeAirport))
                            || form.AuditorIds.Contains(u.Id))
                .OrderBy(u => u.DisplayName)
                .ToListAsync();
        }

        private long CurrentUserId =>
            long.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
    }
}
