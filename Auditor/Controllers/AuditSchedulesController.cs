using Auditor.DTO.AuditSchedule;
using Auditor.Models;
using Auditor.Services.Interfaces;
using Auditor.ViewModels.AuditSchedule;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Auditor.Controllers
{
    public class AuditSchedulesController : Controller
    {
        private readonly IAuditScheduleService _auditScheduleService;
        private readonly AuditorContext _context;

        public AuditSchedulesController(IAuditScheduleService AuditScheduleService, AuditorContext context)
        {
            _auditScheduleService = AuditScheduleService;
            _context = context;
        }

        /*
            ========================================
            PAGE 1
            Scheduled Audits for Auditor
            ========================================
            */

        public async Task<IActionResult> Index()
        {
            var schedules = await _context.AuditSchedules
                .Include(s => s.Template)
                .Include(s => s.Site)
                .Where(s => s.Status == "SCHEDULED")
                .Where(s => !_context.AuditExecutions
                    .Any(e => e.ScheduleId == s.Id))
                .ToListAsync();

            return View(schedules);
        }

        /*
        ========================================
        PAGE 2
        Schedule Details
        ========================================
        */

        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var auditSchedule = await _context.AuditSchedules
                .Include(a => a.Scheduler)
                .Include(a => a.Site)
                .Include(a => a.Template)
                .Include(a => a.Auditors)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (auditSchedule == null)
            {
                return NotFound();
            }

            return View(auditSchedule);
        }

        // GET: AuditSchedules/Create
        public IActionResult Create()
        {
            ViewData["SchedulerId"] = new SelectList(_context.AppUsers, "Id", "Id");
            ViewData["SiteId"] = new SelectList(_context.AuditSites, "Id", "Name");
            ViewData["TemplateId"] = new SelectList(_context.AuditTemplates, "Id", "Name");
            return View();
        }

        // POST: AuditSchedules/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TemplateId,SiteId,SchedulerId,ScheduledDate,DueDate,Status,ModificationReason,CancellationReason,CreatedDate")] AuditSchedule auditSchedule)
        {
            if (ModelState.IsValid)
            {
                _context.Add(auditSchedule);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["SchedulerId"] = new SelectList(_context.AppUsers, "Id", "Id", auditSchedule.SchedulerId);
            ViewData["SiteId"] = new SelectList(_context.AuditSites, "Id", "Name", auditSchedule.SiteId);
            ViewData["TemplateId"] = new SelectList(_context.AuditTemplates, "Id", "Name", auditSchedule.TemplateId);
            return View(auditSchedule);
        }

        // GET: AuditSchedules/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var auditSchedule = await _context.AuditSchedules
                .Include(a => a.Auditors)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (auditSchedule == null)
            {
                return NotFound();
            }
            ViewData["SchedulerId"] = new SelectList(_context.AppUsers, "Id", "Id", auditSchedule.SchedulerId);
            ViewData["SiteId"] = new SelectList(_context.AuditSites, "Id", "Name", auditSchedule.SiteId);
            ViewData["TemplateId"] = new SelectList(_context.AuditTemplates, "Id", "Name", auditSchedule.TemplateId);
            ViewData["Auditors"] = new MultiSelectList(_context.AppUsers, "Id", "DisplayName", auditSchedule.Auditors.Select(a => a.Id));
            return View(auditSchedule);
        }

        // POST: AuditSchedules/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,TemplateId,SiteId,SchedulerId,ScheduledDate,DueDate,Status,ModificationReason,CancellationReason,CreatedDate")] AuditSchedule auditSchedule, List<long> selectedAuditorIds)
        {
            if (id != auditSchedule.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(auditSchedule);

                    var existing = await _context.AuditSchedules
                        .Include(s => s.Auditors)
                        .FirstAsync(s => s.Id == id);

                    selectedAuditorIds ??= new List<long>();

                    var toRemove = existing.Auditors.Where(a => !selectedAuditorIds.Contains(a.Id)).ToList();
                    foreach (var auditor in toRemove)
                        existing.Auditors.Remove(auditor);

                    var existingIds = existing.Auditors.Select(a => a.Id).ToList();
                    var toAddIds = selectedAuditorIds.Except(existingIds).ToList();
                    if (toAddIds.Count > 0)
                    {
                        var toAdd = await _context.AppUsers.Where(u => toAddIds.Contains(u.Id)).ToListAsync();
                        foreach (var auditor in toAdd)
                            existing.Auditors.Add(auditor);
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AuditScheduleExists(auditSchedule.Id))
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
            ViewData["SchedulerId"] = new SelectList(_context.AppUsers, "Id", "Id", auditSchedule.SchedulerId);
            ViewData["SiteId"] = new SelectList(_context.AuditSites, "Id", "Name", auditSchedule.SiteId);
            ViewData["TemplateId"] = new SelectList(_context.AuditTemplates, "Id", "Name", auditSchedule.TemplateId);
            ViewData["Auditors"] = new MultiSelectList(_context.AppUsers, "Id", "DisplayName", selectedAuditorIds);
            return View(auditSchedule);
        }

        // GET: AuditSchedules/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var auditSchedule = await _context.AuditSchedules
                .Include(a => a.Scheduler)
                .Include(a => a.Site)
                .Include(a => a.Template)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (auditSchedule == null)
            {
                return NotFound();
            }

            return View(auditSchedule);
        }

        // POST: AuditSchedules/Delete/5
        // Hard delete is only allowed for schedules that haven't progressed past the initial
        // "SCHEDULED" state and have no executions yet (true accidental-record removal).
        // Anything further along must go through Cancel so the lifecycle is preserved.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var auditSchedule = await _context.AuditSchedules
                .Include(s => s.AuditExecutions)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (auditSchedule == null)
                return RedirectToAction(nameof(Index));

            if (auditSchedule.Status != "SCHEDULED" || auditSchedule.AuditExecutions.Count > 0)
            {
                TempData["Error"] = "This schedule can no longer be deleted directly. Use Cancel instead.";
                return RedirectToAction(nameof(Details), new { id });
            }

            _context.AuditSchedules.Remove(auditSchedule);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(long id, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["Error"] = "A cancellation reason is required.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var auditSchedule = await _context.AuditSchedules.FirstOrDefaultAsync(s => s.Id == id);
            if (auditSchedule == null)
                return NotFound();

            auditSchedule.Status = "CANCELLED";
            auditSchedule.CancellationReason = reason;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id });
        }

        private bool AuditScheduleExists(long id)
        {
            return _context.AuditSchedules.Any(e => e.Id == id);
        }
    }
}
