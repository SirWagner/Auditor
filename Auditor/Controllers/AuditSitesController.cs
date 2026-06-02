using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Auditor.Models;
using System.Security.Claims;

namespace Auditor.Controllers
{
    //[Authorize]
    public class AuditSitesController : Controller
    {
        private readonly AuditorContext _context;
        private readonly ILogger<AuditSitesController> _logger;

        public AuditSitesController(AuditorContext context, ILogger<AuditSitesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Index
        public async Task<IActionResult> Index()
        {
            var auditSites = await _context.AuditSites
                .Include(a => a.Airport)
                .Include(a => a.Department)
                .Include(a => a.CreatedByNavigation)
                .OrderByDescending(a => a.CreatedDate)
                .ToListAsync();

            return View(auditSites);
        }

        // Create (GET)
        public async Task<IActionResult> Create()
        {
            var airports = await _context.Airports.ToListAsync();
            ViewBag.Airports = airports;

            var departments = await _context.Departments.ToListAsync();
            ViewBag.Departments = departments;

            return View();
        }

        // Create (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AuditSite auditSite)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !long.TryParse(userId, out var userIdLong))
            {
                ModelState.AddModelError("", "User information could not be retrieved.");
                await ReloadFormData();
                return View(auditSite);
            }

            if (!await _context.Airports.AnyAsync(a => a.Id == auditSite.AirportId))
            {
                ModelState.AddModelError(nameof(auditSite.AirportId), "Selected airport does not exist.");
            }

            if (!await _context.Departments.AnyAsync(d => d.Id == auditSite.DepartmentId))
            {
                ModelState.AddModelError(nameof(auditSite.DepartmentId), "Selected department does not exist.");
            }

            //if (ModelState.IsValid)
            //{
            //    var department = await _context.Departments.FindAsync(auditSite.DepartmentId);
            //    if (department?.AirportId != auditSite.AirportId)
            //    {
            //        ModelState.AddModelError(nameof(auditSite.DepartmentId),
            //            "Selected department does not belong to the selected airport.");
            //    }
            //}

            if (ModelState.IsValid)
            {
                auditSite.CreatedBy = userIdLong;
                auditSite.CreatedDate = DateTime.Now;

                if (string.IsNullOrEmpty(auditSite.Status))
                {
                    auditSite.Status = "Active";
                }

                try
                {
                    _context.Add(auditSite);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating audit site");
                    ModelState.AddModelError("", "An error occurred while saving the audit site.");
                }
            }

            await ReloadFormData();
            return View(auditSite);
        }

        // Edit (GET)
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
                return NotFound();

            var auditSite = await _context.AuditSites
                .Include(a => a.Airport)
                .Include(a => a.Department)
                .Include(a => a.CreatedByNavigation)
                .Include(a => a.AuditSchedules)
                .Include(a => a.Templates)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (auditSite == null)
                return NotFound();

            await ReloadFormData();
            return View(auditSite);
        }

        // Edit (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, AuditSite auditSite)
        {
            if (id != auditSite.Id)
                return NotFound();

            var originalAuditSite = await _context.AuditSites.AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (originalAuditSite == null)
                return NotFound();

            if (!await _context.Airports.AnyAsync(a => a.Id == auditSite.AirportId))
            {
                ModelState.AddModelError(nameof(auditSite.AirportId), "Selected airport does not exist.");
            }

            if (!await _context.Departments.AnyAsync(d => d.Id == auditSite.DepartmentId))
            {
                ModelState.AddModelError(nameof(auditSite.DepartmentId), "Selected department does not exist.");
            }

            //if (ModelState.IsValid)
            //{
            //    var department = await _context.Departments.FindAsync(auditSite.DepartmentId);
            //    if (department?.AirportId != auditSite.AirportId)
            //    {
            //        ModelState.AddModelError(nameof(auditSite.DepartmentId),
            //            "Selected department does not belong to the selected airport.");
            //    }
            //}

            if (ModelState.IsValid)
            {
                try
                {
                    auditSite.CreatedBy = originalAuditSite.CreatedBy;
                    auditSite.CreatedDate = originalAuditSite.CreatedDate;

                    _context.Update(auditSite);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await AuditSiteExists(auditSite.Id))
                        return NotFound();
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating audit site");
                    ModelState.AddModelError("", "An error occurred while saving the audit site.");
                }
            }

            await ReloadFormData();
            return View(auditSite);
        }

        // Delete
        [HttpPost]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
                return NotFound();

            var auditSite = await _context.AuditSites
                .Include(a => a.AuditSchedules)
                .Include(a => a.Templates)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (auditSite == null)
                return NotFound();

            if (auditSite.AuditSchedules?.Count > 0 || auditSite.Templates?.Count > 0)
            {
                return BadRequest("Cannot delete audit site with related audits or templates.");
            }

            try
            {
                _context.AuditSites.Remove(auditSite);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting audit site");
                return BadRequest("An error occurred while deleting the audit site.");
            }
        }

        // Helper methods
        public async Task ReloadFormData()
        {
            var airports = await _context.Airports.ToListAsync();
            ViewBag.Airports = airports;

            var departments = await _context.Departments.ToListAsync();
            ViewBag.Departments = departments;
        }

        private async Task<bool> AuditSiteExists(long id)
        {
            return await _context.AuditSites.AnyAsync(e => e.Id == id);
        }

        [HttpGet]
        public async Task<IActionResult> GetDepartmentsByAirport(long airportId)
        {
            var departments = await _context.Departments
                //.Where(d => d.AirportId == airportId)
                .Select(d => new { d.Id, d.Name })
                .ToListAsync();

            return Json(departments);
        }
        public async Task<IActionResult> Details(long? id)
        {
            var auditSite = await _context.AuditSites
                .Include(a => a.Airport)
                .Include(a => a.Department)
                .Include(a => a.CreatedByNavigation)
                .Include(a => a.Templates)
                    .ThenInclude(t => t.CreatedByNavigation)
                .Include(a => a.AuditSchedules)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (auditSite == null)
                return NotFound();

            return View(auditSite);
        }
    }
}