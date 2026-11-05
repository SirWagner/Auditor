using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Auditor.Models;
using Auditor.Models.Lookups;
using Auditor.ViewModels.Airport;

namespace Auditor.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = Auditor.Security.Roles.AdminOrManager)]
    public class AirportsController : Controller
    {
        private readonly AuditorContext _context;
        private readonly ILogger<AirportsController> _logger;
        private readonly Auditor.Security.UserScope _scope;

        public AirportsController(AuditorContext context, ILogger<AirportsController> logger, Auditor.Security.UserScope scope)
        {
            _context = context;
            _logger = logger;
            _scope = scope;
        }

        // Users linked to one airport only see that airport and can't add or remove airports.
        private async Task<bool> CanAccessAsync(Airport? airport) =>
            airport != null && await _scope.CanAccessAirportAsync(airport.Id);

        private async Task<bool> IsScopedAsync() => await _scope.AirportIdAsync() != null;

        // GET: Airports
        public async Task<IActionResult> Index()
        {
            var scopeAirport = await _scope.AirportIdAsync();
            ViewBag.CanAddAirports = scopeAirport == null;
            var airports = await _context.Airports
                .Where(a => scopeAirport == null || a.Id == scopeAirport)
                .Include(a => a.Locations)
                .Include(a => a.AuditSites)
                .OrderBy(a => a.Name)
                .ToListAsync();

            return View(airports);
        }

        // GET: Airports/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            var airport = await _context.Airports
                .Include(a => a.Locations)
                .Include(a => a.AuditSites)
                    .ThenInclude(s => s.Department)
                .Include(a => a.AuditSites)
                    .ThenInclude(s => s.Terminal)
                .Include(a => a.AuditSites)
                    .ThenInclude(s => s.CreatedByNavigation)
                .Include(a => a.AuditSites)
                    .ThenInclude(s => s.AuditSchedules)
                .Include(a => a.AuditSites)
                    .ThenInclude(s => s.Templates)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (!await CanAccessAsync(airport))
                return NotFound();

            return View(airport);
        }

        // GET: Airports/Create
        public async Task<IActionResult> Create()
        {
            if (await IsScopedAsync())
                return Forbid();

            return View(new AirportFormViewModel());
        }

        // POST: Airports/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AirportFormViewModel form)
        {
            if (await IsScopedAsync())
                return Forbid();

            Normalize(form);
            await ValidateAsync(form);

            if (!ModelState.IsValid)
                return View(form);

            var airport = new Airport();
            Apply(form, airport);
            SyncLocations(form, airport);

            _context.Airports.Add(airport);

            if (!await TrySaveAsync(form))
                return View(form);

            TempData["Success"] = $"Airport \"{airport.Name}\" was created.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Airports/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
                return NotFound();

            var airport = await _context.Airports
                .Include(a => a.Locations)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (!await CanAccessAsync(airport))
                return NotFound();

            return View(ToForm(airport!));
        }

        // POST: Airports/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, AirportFormViewModel form)
        {
            if (id != form.Id)
                return NotFound();

            var airport = await _context.Airports
                .Include(a => a.Locations)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (airport == null || !await CanAccessAsync(airport))
                return NotFound();

            Normalize(form);
            await ValidateAsync(form);
            await ValidateLocationsStillUnusedAsync(form, airport);

            if (!ModelState.IsValid)
                return View(form);

            Apply(form, airport);
            SyncLocations(form, airport);

            if (!await TrySaveAsync(form))
                return View(form);

            TempData["Success"] = $"Airport \"{airport.Name}\" was updated.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Airports/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
                return NotFound();
            if (await IsScopedAsync())
                return Forbid();

            var airport = await _context.Airports
                .Include(a => a.AuditSites)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (airport == null)
                return NotFound();

            return View(airport);
        }

        // POST: Airports/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            if (await IsScopedAsync())
                return Forbid();

            var airport = await _context.Airports
                .Include(a => a.AuditSites)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (airport == null)
                return RedirectToAction(nameof(Index));

            if (airport.AuditSites.Count > 0)
            {
                TempData["Error"] = $"\"{airport.Name}\" has {airport.AuditSites.Count} audit site(s) and cannot be deleted. Set it to Inactive instead.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            _context.Airports.Remove(airport);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Airport \"{airport.Name}\" was deleted.";
            return RedirectToAction(nameof(Index));
        }

        // Trims, collapses repeated spaces and upper-cases the code so "ORT", " ort " and "O R T"-style
        // near-duplicates can't slip past the uniqueness check.
        private static void Normalize(AirportFormViewModel form)
        {
            form.Code = form.Code?.Trim().ToUpperInvariant();
            form.Name = CollapseSpaces(form.Name);
            form.AirportManager = CollapseSpaces(form.AirportManager);
            form.EmailAddress = form.EmailAddress?.Trim();
            form.TelNumber = string.IsNullOrWhiteSpace(form.TelNumber) ? null : form.TelNumber.Trim();

            form.Terminals = CleanList(form.Terminals);
            form.TerminalLocations = CleanList(form.TerminalLocations);
            form.AirsideLocations = CleanList(form.AirsideLocations);
            form.LandsideLocations = CleanList(form.LandsideLocations);
        }

        private async Task ValidateAsync(AirportFormViewModel form)
        {
            // Attribute validation ran on the raw input during binding; re-run it on the normalized values.
            ModelState.Clear();
            TryValidateModel(form);

            if (form.AirportType != null && !AirportLookups.AirportTypes.Contains(form.AirportType))
                ModelState.AddModelError(nameof(form.AirportType), "Select a valid Airport Type.");

            if (form.Country != null && !AirportLookups.RegionsByCountry.ContainsKey(form.Country))
                ModelState.AddModelError(nameof(form.Country), "Select a valid Country.");
            else if (form.Region != null && !AirportLookups.IsValidRegion(form.Country, form.Region))
                ModelState.AddModelError(nameof(form.Region), $"Select a Region that belongs to {form.Country}.");

            foreach (var (areaType, names) in form.AreaLists)
            {
                if (names.Any(n => n.Length > 150))
                    ModelState.AddModelError(areaType, $"{AirportLookups.AreaTypes[areaType]} names cannot exceed 150 characters.");
            }

            // SQL Server's default collation is case-insensitive, so these comparisons are too.
            if (!string.IsNullOrEmpty(form.Code)
                && await _context.Airports.AnyAsync(a => a.Id != form.Id && a.Code == form.Code))
            {
                ModelState.AddModelError(nameof(form.Code), $"An airport with code \"{form.Code}\" already exists.");
            }

            if (!string.IsNullOrEmpty(form.Name)
                && await _context.Airports.AnyAsync(a => a.Id != form.Id && a.Name == form.Name))
            {
                ModelState.AddModelError(nameof(form.Name), $"An airport named \"{form.Name}\" already exists.");
            }
        }

        // A terminal or location that an audit site points to can't be removed from the airport.
        private async Task ValidateLocationsStillUnusedAsync(AirportFormViewModel form, Airport airport)
        {
            var kept = form.AreaLists
                .SelectMany(l => l.Names.Select(n => (l.AreaType, Name: n.ToUpperInvariant())))
                .ToHashSet();

            var removed = airport.Locations
                .Where(l => !kept.Contains((l.AreaType, l.Name.ToUpperInvariant())))
                .ToList();

            if (removed.Count == 0)
                return;

            var removedTerminalIds = removed.Where(l => l.AreaType == AirportLookups.Terminal).Select(l => l.Id).ToList();
            var usedTerminals = await _context.AuditSites
                .Where(s => s.TerminalId != null && removedTerminalIds.Contains(s.TerminalId.Value))
                .Select(s => s.Terminal!.Name)
                .Distinct()
                .ToListAsync();

            foreach (var name in usedTerminals)
                ModelState.AddModelError(AirportLookups.Terminal, $"Terminal \"{name}\" is used by an audit site and cannot be removed.");

            foreach (var (subCategory, areaType) in AuditSiteLookups.LocationAreaForSubCategory)
            {
                var removedNames = removed.Where(l => l.AreaType == areaType).Select(l => l.Name).ToList();
                if (removedNames.Count == 0)
                    continue;

                var usedNames = await _context.AuditSites
                    .Where(s => s.AirportId == airport.Id
                                && s.SiteSubCategory == subCategory
                                && s.SiteType != null
                                && removedNames.Contains(s.SiteType))
                    .Select(s => s.SiteType!)
                    .Distinct()
                    .ToListAsync();

                foreach (var name in usedNames)
                    ModelState.AddModelError(areaType, $"{AirportLookups.AreaTypes[areaType]}: \"{name}\" is used by an audit site and cannot be removed.");
            }
        }

        private async Task<bool> TrySaveAsync(AirportFormViewModel form)
        {
            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                // A concurrent request may have inserted the same code or name after our check.
                _logger.LogWarning(ex, "Saving airport {Code} failed", form.Code);
                ModelState.AddModelError(string.Empty,
                    "The airport could not be saved. Another airport may already use this code or name — check both and try again.");
                return false;
            }
        }

        private static void Apply(AirportFormViewModel form, Airport airport)
        {
            airport.Code = form.Code!;
            airport.Name = form.Name!;
            airport.AirportType = form.AirportType;
            airport.Country = form.Country;
            airport.Region = form.Region;
            airport.AirportManager = form.AirportManager;
            airport.EmailAddress = form.EmailAddress;
            airport.TelNumber = form.TelNumber;
            airport.IsActive = form.IsActive;
        }

        // Adds new area names and removes dropped ones, keeping existing rows (and their ids) intact.
        private static void SyncLocations(AirportFormViewModel form, Airport airport)
        {
            foreach (var (areaType, names) in form.AreaLists)
            {
                var existing = airport.Locations.Where(l => l.AreaType == areaType).ToList();

                foreach (var location in existing.Where(l => !names.Contains(l.Name, StringComparer.OrdinalIgnoreCase)))
                    airport.Locations.Remove(location);

                foreach (var name in names.Where(n => !existing.Any(l => string.Equals(l.Name, n, StringComparison.OrdinalIgnoreCase))))
                    airport.Locations.Add(new AirportLocation { AreaType = areaType, Name = name });
            }
        }

        private static AirportFormViewModel ToForm(Airport airport)
        {
            List<string> namesOf(string areaType) => airport.Locations
                .Where(l => l.AreaType == areaType)
                .Select(l => l.Name)
                .OrderBy(n => n)
                .ToList();

            return new AirportFormViewModel
            {
                Id = airport.Id,
                Code = airport.Code,
                Name = airport.Name,
                AirportType = airport.AirportType,
                Country = airport.Country,
                Region = airport.Region,
                AirportManager = airport.AirportManager,
                EmailAddress = airport.EmailAddress,
                TelNumber = airport.TelNumber,
                IsActive = airport.IsActive,
                Terminals = namesOf(AirportLookups.Terminal),
                TerminalLocations = namesOf(AirportLookups.TerminalLocation),
                AirsideLocations = namesOf(AirportLookups.Airside),
                LandsideLocations = namesOf(AirportLookups.Landside),
            };
        }

        private static string? CollapseSpaces(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : Regex.Replace(value.Trim(), @"\s+", " ");

        private static List<string> CleanList(IEnumerable<string>? values) =>
            (values ?? Enumerable.Empty<string>())
                .Select(CollapseSpaces)
                .Where(v => v != null)
                .Select(v => v!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
    }
}
