using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Auditor.Models;

namespace Auditor.Controllers
{
    public class AuditSchedulesController : Controller
    {
        private readonly AuditorContext _context;

        public AuditSchedulesController(AuditorContext context)
        {
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

            var auditSchedule = await _context.AuditSchedules.FindAsync(id);
            if (auditSchedule == null)
            {
                return NotFound();
            }
            ViewData["SchedulerId"] = new SelectList(_context.AppUsers, "Id", "Id", auditSchedule.SchedulerId);
            ViewData["SiteId"] = new SelectList(_context.AuditSites, "Id", "Name", auditSchedule.SiteId);
            ViewData["TemplateId"] = new SelectList(_context.AuditTemplates, "Id", "Name", auditSchedule.TemplateId);
            return View(auditSchedule);
        }

        // POST: AuditSchedules/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,TemplateId,SiteId,SchedulerId,ScheduledDate,DueDate,Status,ModificationReason,CancellationReason,CreatedDate")] AuditSchedule auditSchedule)
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
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var auditSchedule = await _context.AuditSchedules.FindAsync(id);
            if (auditSchedule != null)
            {
                _context.AuditSchedules.Remove(auditSchedule);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AuditScheduleExists(long id)
        {
            return _context.AuditSchedules.Any(e => e.Id == id);
        }
    }
}
