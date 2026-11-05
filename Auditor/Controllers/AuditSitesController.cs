using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Auditor.Models;
using Auditor.Models.Lookups;
using Auditor.ViewModels.AuditSite;

namespace Auditor.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = Auditor.Security.Roles.AdminOrManager)]
    public class AuditSitesController : Controller
    {
        private readonly AuditorContext _context;
        private readonly ILogger<AuditSitesController> _logger;
        private readonly Auditor.Security.UserScope _scope;

        public AuditSitesController(AuditorContext context, ILogger<AuditSitesController> logger, Auditor.Security.UserScope scope)
        {
            _context = context;
            _logger = logger;
            _scope = scope;
        }

        // Index
        public async Task<IActionResult> Index()
        {
            var scopeAirport = await _scope.AirportIdAsync();
            var auditSites = await _context.AuditSites
                .Where(a => scopeAirport == null || a.AirportId == scopeAirport)
                .Include(a => a.Airport)
                .Include(a => a.Terminal)
                .Include(a => a.Department)
                .Include(a => a.CreatedByNavigation)
                .OrderByDescending(a => a.CreatedDate)
                .ToListAsync();

            return View(auditSites);
        }

        // Create (GET)
        public async Task<IActionResult> Create(long? airportId)
        {
            var form = new AuditSiteFormViewModel { AirportId = await _scope.NarrowAsync(airportId) };
            await ReloadFormData(form);
            return View(form);
        }

        // Create (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AuditSiteFormViewModel form)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !long.TryParse(userId, out var userIdLong))
            {
                ModelState.AddModelError("", "Your session has expired. Sign in again, then create the audit site.");
                await ReloadFormData(form);
                return View(form);
            }

            Normalize(form);
            await ValidateAsync(form, allowInactiveAirportId: null);

            if (ModelState.IsValid)
            {
                var auditSite = new AuditSite
                {
                    CreatedBy = userIdLong,
                    CreatedDate = DateTime.Now,
                };
                Apply(form, auditSite);

                // Link the standard checklist for this kind of site, if one exists.
                var defaultTemplate = AuditSiteLookups.DefaultTemplateFor(form.SiteSubCategory, form.SiteType);
                var template = defaultTemplate == null
                    ? null
                    : await _context.AuditTemplates.FirstOrDefaultAsync(t => t.Name == defaultTemplate && t.IsActive);
                if (template != null)
                    auditSite.Templates.Add(template);

                try
                {
                    _context.Add(auditSite);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = template == null
                        ? $"Audit site \"{auditSite.Name}\" was created."
                        : $"Audit site \"{auditSite.Name}\" was created and linked to the \"{template.Name}\" checklist.";
                    return RedirectToAction(nameof(Details), new { id = auditSite.Id });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating audit site");
                    ModelState.AddModelError("", "The audit site could not be saved. Try again, and contact support if it keeps failing.");
                }
            }

            await ReloadFormData(form);
            return View(form);
        }

        // Edit (GET)
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
                return NotFound();

            var auditSite = await _context.AuditSites.FirstOrDefaultAsync(m => m.Id == id);

            if (auditSite == null || !await _scope.CanAccessAirportAsync(auditSite.AirportId))
                return NotFound();

            var form = ToForm(auditSite);
            await ReloadFormData(form);
            return View(form);
        }

        // Edit (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, AuditSiteFormViewModel form)
        {
            if (id != form.Id)
                return NotFound();

            var auditSite = await _context.AuditSites.FirstOrDefaultAsync(m => m.Id == id);

            if (auditSite == null || !await _scope.CanAccessAirportAsync(auditSite.AirportId))
                return NotFound();

            if (auditSite.Status == AuditSiteLookups.Inactive)
            {
                ModelState.AddModelError("", "This site is Inactive and cannot be updated. Reactivate it first via Change Status on the site's details page.");
                await ReloadFormData(form);
                return View(form);
            }

            Normalize(form);
            // An existing site may stay on its airport even if that airport has since been deactivated.
            await ValidateAsync(form, allowInactiveAirportId: auditSite.AirportId);

            if (ModelState.IsValid)
            {
                Apply(form, auditSite);

                try
                {
                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Audit site \"{auditSite.Name}\" was updated.";
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
                    ModelState.AddModelError("", "The audit site could not be saved. Try again, and contact support if it keeps failing.");
                }
            }

            await ReloadFormData(form);
            return View(form);
        }

        // Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
                return NotFound();

            var auditSite = await _context.AuditSites
                .Include(a => a.AuditSchedules)
                .Include(a => a.Templates)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (auditSite == null || !await _scope.CanAccessAirportAsync(auditSite.AirportId))
                return NotFound();

            if (auditSite.AuditSchedules?.Count > 0 || auditSite.Templates?.Count > 0)
            {
                TempData["Error"] = $"\"{auditSite.Name}\" has linked audits or templates and cannot be deleted. Set it to Inactive instead.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.AuditSites.Remove(auditSite);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Audit site \"{auditSite.Name}\" was deleted.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting audit site");
                TempData["Error"] = "The audit site could not be deleted. Try again, and contact support if it keeps failing.";
                return RedirectToAction(nameof(Index));
            }
        }

        // Dropdown data for the selected airport: its terminals and its site types per Operations sub-category.
        [HttpGet]
        public async Task<IActionResult> GetAirportLocations(long airportId)
        {
            if (!await _scope.CanAccessAirportAsync(airportId))
                return NotFound();

            var locations = await _context.AirportLocations
                .Where(l => l.AirportId == airportId)
                .OrderBy(l => l.Name)
                .Select(l => new { l.Id, l.AreaType, l.Name })
                .ToListAsync();

            var siteTypes = AuditSiteLookups.LocationAreaForSubCategory.ToDictionary(
                kv => kv.Key,
                kv => locations.Where(l => l.AreaType == kv.Value).Select(l => l.Name).ToList());

            foreach (var (subCategory, types) in AuditSiteLookups.FixedSiteTypes)
                siteTypes[subCategory] = types.ToList();

            return Json(new
            {
                terminals = locations
                    .Where(l => l.AreaType == AirportLookups.Terminal)
                    .Select(l => new { l.Id, l.Name }),
                siteTypes,
            });
        }

        public async Task<IActionResult> Details(long? id)
        {
            var auditSite = await _context.AuditSites
                .Include(a => a.Airport)
                .Include(a => a.Terminal)
                .Include(a => a.Department)
                .Include(a => a.CreatedByNavigation)
                .Include(a => a.Templates)
                    .ThenInclude(t => t.CreatedByNavigation)
                .Include(a => a.AuditSchedules)
                    .ThenInclude(s => s.Scheduler)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (auditSite == null || !await _scope.CanAccessAirportAsync(auditSite.AirportId))
                return NotFound();

            ViewBag.AllTemplates = await _context.AuditTemplates
                .Where(t => t.AirportId == null || t.AirportId == auditSite.AirportId)
                .OrderBy(t => t.Name)
                .ToListAsync();

            return View(auditSite);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(long id, string newStatus)
        {
            if (!AuditSiteLookups.Statuses.Contains(newStatus))
            {
                TempData["Error"] = "Invalid status value.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var auditSite = await _context.AuditSites.FirstOrDefaultAsync(m => m.Id == id);

            if (auditSite == null || !await _scope.CanAccessAirportAsync(auditSite.AirportId))
                return NotFound();

            auditSite.Status = newStatus;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateTemplates(long id, List<long> selectedTemplateIds)
        {
            var auditSite = await _context.AuditSites
                .Include(a => a.Templates)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (auditSite == null || !await _scope.CanAccessAirportAsync(auditSite.AirportId))
                return NotFound();

            selectedTemplateIds ??= new List<long>();

            var toRemove = auditSite.Templates
                .Where(t => !selectedTemplateIds.Contains(t.Id))
                .ToList();

            foreach (var template in toRemove)
                auditSite.Templates.Remove(template);

            var existingIds = auditSite.Templates.Select(t => t.Id).ToList();
            var toAddIds = selectedTemplateIds.Except(existingIds).ToList();

            if (toAddIds.Count > 0)
            {
                var templatesToAdd = await _context.AuditTemplates
                    .Where(t => toAddIds.Contains(t.Id))
                    .ToListAsync();

                foreach (var template in templatesToAdd)
                    auditSite.Templates.Add(template);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id });
        }

        // Helper methods
        private async Task ReloadFormData(AuditSiteFormViewModel form)
        {
            // Active airports only, plus the site's current airport so editing an older site still works.
            var scopeAirport = await _scope.AirportIdAsync();
            ViewBag.Airports = await _context.Airports
                .Where(a => a.IsActive || a.Id == form.AirportId)
                .Where(a => scopeAirport == null || a.Id == scopeAirport)
                .OrderBy(a => a.Name)
                .ToListAsync();

            ViewBag.Departments = await _context.Departments
                .OrderBy(d => d.Name)
                .ToListAsync();

            ViewBag.ServiceProviders = await _context.ReferenceListItems
                .Where(i => i.List.Code == ReferenceLists.ServiceProviders && i.IsActive)
                .OrderBy(i => i.Name)
                .Select(i => i.Name)
                .ToListAsync();
        }

        private static void Normalize(AuditSiteFormViewModel form)
        {
            form.Name = CollapseSpaces(form.Name);
            form.ServiceProvider = CollapseSpaces(form.ServiceProvider);
            form.ContactName = CollapseSpaces(form.ContactName);
            form.ContactEmail = CollapseSpaces(form.ContactEmail);
            form.ContactNumber = CollapseSpaces(form.ContactNumber);

            if (!AuditSiteLookups.RequiresTerminal(form.SiteCategory, form.SiteSubCategory) && form.TerminalId == 0)
                form.TerminalId = null;
        }

        private async Task ValidateAsync(AuditSiteFormViewModel form, long? allowInactiveAirportId)
        {
            // Attribute validation ran on the raw input during binding; re-run it on the normalized values.
            ModelState.Clear();
            TryValidateModel(form);

            if (form.AirportId != null
                && !await _context.Airports.AnyAsync(a => a.Id == form.AirportId && (a.IsActive || a.Id == allowInactiveAirportId)))
            {
                ModelState.AddModelError(nameof(form.AirportId), "Select an active airport.");
            }

            if (form.AirportId != null && !await _scope.CanAccessAirportAsync(form.AirportId.Value))
                ModelState.AddModelError(nameof(form.AirportId), "Your account is limited to another airport.");

            if (form.DepartmentId != null && !await _context.Departments.AnyAsync(d => d.Id == form.DepartmentId))
                ModelState.AddModelError(nameof(form.DepartmentId), "Selected department does not exist.");

            if (!AuditSiteLookups.Statuses.Contains(form.Status))
                ModelState.AddModelError(nameof(form.Status), "Status must be Active or Inactive.");

            if (form.SiteCategory != null && !AuditSiteLookups.Categories.Contains(form.SiteCategory))
            {
                ModelState.AddModelError(nameof(form.SiteCategory), "Select a valid Site Category.");
            }
            else if (form.SiteSubCategory != null && !AuditSiteLookups.IsValidSubCategory(form.SiteCategory, form.SiteSubCategory))
            {
                ModelState.AddModelError(nameof(form.SiteSubCategory), $"Select a Sub-Category that belongs to {form.SiteCategory}.");
            }
            else if (form.SiteSubCategory != null && form.SiteType != null && form.AirportId != null
                     && !await IsValidSiteTypeAsync(form.AirportId.Value, form.SiteSubCategory, form.SiteType))
            {
                ModelState.AddModelError(nameof(form.SiteType), $"Select a Site Type from the {form.SiteSubCategory} list for this airport.");
            }

            if (AuditSiteLookups.RequiresTerminal(form.SiteCategory, form.SiteSubCategory) && form.TerminalId == null)
                ModelState.AddModelError(nameof(form.TerminalId), "Terminal is required for Operations sites in a terminal.");

            if (form.TerminalId != null && form.AirportId != null
                && !await _context.AirportLocations.AnyAsync(l => l.Id == form.TerminalId
                                                                  && l.AirportId == form.AirportId
                                                                  && l.AreaType == AirportLookups.Terminal))
            {
                ModelState.AddModelError(nameof(form.TerminalId), "Select a terminal that belongs to the selected airport.");
            }

            // Site names are unique per airport (case-insensitive under SQL Server's default collation).
            if (form.Name != null && form.AirportId != null
                && await _context.AuditSites.AnyAsync(s => s.Id != form.Id && s.AirportId == form.AirportId && s.Name == form.Name))
            {
                ModelState.AddModelError(nameof(form.Name), $"This airport already has a site named \"{form.Name}\".");
            }
        }

        private async Task<bool> IsValidSiteTypeAsync(long airportId, string subCategory, string siteType)
        {
            if (AuditSiteLookups.FixedSiteTypes.TryGetValue(subCategory, out var fixedTypes))
                return fixedTypes.Contains(siteType);

            if (AuditSiteLookups.LocationAreaForSubCategory.TryGetValue(subCategory, out var areaType))
                return await _context.AirportLocations.AnyAsync(l => l.AirportId == airportId && l.AreaType == areaType && l.Name == siteType);

            return false;
        }

        private static void Apply(AuditSiteFormViewModel form, AuditSite auditSite)
        {
            auditSite.AirportId = form.AirportId!.Value;
            auditSite.DepartmentId = form.DepartmentId!.Value;
            auditSite.TerminalId = form.TerminalId;
            auditSite.Name = form.Name!;
            auditSite.SiteCategory = form.SiteCategory;
            auditSite.SiteSubCategory = form.SiteSubCategory;
            auditSite.SiteType = form.SiteType;
            auditSite.ServiceProvider = form.ServiceProvider;
            auditSite.ContactName = form.ContactName;
            auditSite.ContactEmail = form.ContactEmail;
            auditSite.ContactNumber = form.ContactNumber;
            auditSite.Status = form.Status;
        }

        private static AuditSiteFormViewModel ToForm(AuditSite auditSite) => new()
        {
            Id = auditSite.Id,
            AirportId = auditSite.AirportId,
            DepartmentId = auditSite.DepartmentId,
            TerminalId = auditSite.TerminalId,
            Name = auditSite.Name,
            SiteCategory = auditSite.SiteCategory,
            SiteSubCategory = auditSite.SiteSubCategory,
            SiteType = auditSite.SiteType,
            ServiceProvider = auditSite.ServiceProvider,
            ContactName = auditSite.ContactName,
            ContactEmail = auditSite.ContactEmail,
            ContactNumber = auditSite.ContactNumber,
            Status = auditSite.Status,
        };

        private static string? CollapseSpaces(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : Regex.Replace(value.Trim(), @"\s+", " ");

        private async Task<bool> AuditSiteExists(long id)
        {
            return await _context.AuditSites.AnyAsync(e => e.Id == id);
        }
    }
}
