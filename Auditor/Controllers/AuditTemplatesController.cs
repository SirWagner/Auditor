using System.Security.Claims;
using System.Text.RegularExpressions;
using Auditor.DTO.AuditTemplate;
using Auditor.Models;
using Auditor.Models.Lookups;
using Auditor.Security;
using Auditor.Services.Interfaces;
using Auditor.ViewModels.AuditTemplate;
using Auditor.ViewModels.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Auditor.Controllers
{
    [Authorize(Roles = Roles.AdminOrManager)]
    public class AuditTemplatesController : Controller
    {
        private readonly IAuditTemplateService _auditTemplateService;
        private readonly IAppUserService _appUserService;
        private readonly IQuestionBankService _questionBankService;
        private readonly AuditorContext _context;
        private readonly UserScope _scope;
        private readonly ILogger<AuditTemplatesController> _logger;

        public AuditTemplatesController(IAuditTemplateService service,
                                        IAppUserService appUserService,
                                        IQuestionBankService questionBankService,
                                        AuditorContext context,
                                        UserScope scope,
                                        ILogger<AuditTemplatesController> logger)
        {
            _auditTemplateService = service;
            _appUserService = appUserService;
            _questionBankService = questionBankService;
            _context = context;
            _scope = scope;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var templates = await _auditTemplateService.GetAllAsync(await _scope.AirportIdAsync());
            return View(templates);
        }

        // GET: AuditTemplates/Create
        public async Task<IActionResult> Create()
        {
            var vm = new AuditTemplateCreateViewModel();
            vm.AuditTemplateInfoViewModel.Version = "1.0";
            vm.AuditTemplateInfoViewModel.IsActive = true;
            vm.AuditTemplateInfoViewModel.AirportId = await _scope.AirportIdAsync();
            await FillFormOptionsAsync(vm);
            return View(vm);
        }

        public async Task<IActionResult> Details(long id)
        {
            var template = await _auditTemplateService.GetById(id);
            if (template == null || !await CanSeeAsync(id))
                return NotFound();

            var vm = new AuditTemplateDetailsViewModel
            {
                Id = id,
                Name = template.Name,
                Description = template.Description,
                Version = template.Version,
                IsActive = template.IsActive,
                CreatedBy = template.UserDisplayName,
                Items = template.ItemsDetails
                    .OrderBy(i => i.Sequence)
                    .Select(i => new AuditTemplateDetailsQuestionsViewModel
                    {
                        Mandatory = i.Mandatory,
                        Sequence = i.Sequence,
                        QuestionText = i.QuestionText,
                        Category = i.Category,
                        Type = i.Type,
                        Section = i.Section,
                        Responsibility = i.Responsibility,
                        ServiceProvider = i.ServiceProvider,
                    }).ToList(),
            };

            ViewBag.AvailableAt = template.AirportName ?? "All airports";
            ViewBag.Sites = template.SiteNames ?? new List<string>();
            ViewBag.CanEdit = await CanChangeAsync(id);
            return View(vm);
        }

        // POST: AuditTemplates/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAuditTemplateRequest request)
        {
            var (name, version, items) = await ValidateAsync(null, request.Name, request.Version, request.AirportId, request.SiteIds,
                request.Items?.Select(i => new ItemInput(i.Id, i.QuestionBankId, i.Mandatory, i.Sequence, i.Section, i.Responsibility, i.ServiceProvider)).ToList());

            if (!ModelState.IsValid)
            {
                var vm = new AuditTemplateCreateViewModel();
                Refill(vm, request.Name, request.Description, request.Version, request.IsActive, request.AirportId, request.SiteIds, items);
                await FillFormOptionsAsync(vm);
                return View(vm);
            }

            try
            {
                await _auditTemplateService.CreateAsync(new AuditTemplateCreateDTO(
                    name, request.Description?.Trim(), version, request.IsActive, CurrentUserId,
                    items.Select(i => new AuditTemplateItemsDTO(i.QuestionBankId, i.Mandatory, i.Sequence, i.Section, i.Responsibility, i.ServiceProvider)).ToList(),
                    request.AirportId, request.SiteIds));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Saving audit template {Name} failed", name);
                ModelState.AddModelError(string.Empty, "The template couldn't be saved. Check the name isn't already used, then try again.");
                var vm = new AuditTemplateCreateViewModel();
                Refill(vm, request.Name, request.Description, request.Version, request.IsActive, request.AirportId, request.SiteIds, items);
                await FillFormOptionsAsync(vm);
                return View(vm);
            }

            TempData["Success"] = $"Template \"{name}\" was created with {items.Count} question(s).";
            return RedirectToAction(nameof(Index));
        }

        // GET: AuditTemplates/Edit/5
        public async Task<IActionResult> Edit(long id)
        {
            var template = await _auditTemplateService.GetById(id);
            if (template == null)
                return NotFound();
            if (!await CanChangeAsync(id))
                return Forbid();

            var vm = new AuditTemplateEditViewModel
            {
                Id = id,
                AuditTemplateInfoViewModel = new AuditTemplateInfoViewModel
                {
                    Name = template.Name,
                    Description = template.Description,
                    Version = template.Version,
                    IsActive = template.IsActive,
                    AirportId = template.AirportId,
                    SiteIds = template.SiteIds ?? new(),
                },
                AuditTemplateItems = template.ItemsDetails.Select(q => new AuditTemplateItemViewModel
                {
                    Mandatory = q.Mandatory,
                    Id = q.Id,
                    ItemId = q.ItemId,
                    Section = q.Section,
                    Responsibility = q.Responsibility,
                    ServiceProvider = q.ServiceProvider,
                    QuestionText = q.QuestionText,
                    Sequence = q.Sequence,
                    Type = q.Type,
                }).ToList(),
            };

            await FillFormOptionsAsync(vm);
            return View(vm);
        }

        // POST: AuditTemplates/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditAuditTemplateRequest request)
        {
            if (!await _context.AuditTemplates.AnyAsync(t => t.Id == request.Id))
                return NotFound();
            if (!await CanChangeAsync(request.Id))
                return Forbid();

            var (name, version, items) = await ValidateAsync(request.Id, request.Name, request.Version, request.AirportId, request.SiteIds,
                request.Items?.Select(i => new ItemInput(i.Id, i.QuestionBankId, i.Mandatory, i.Sequence, i.Section, i.Responsibility, i.ServiceProvider)).ToList());

            if (!ModelState.IsValid)
            {
                var vm = new AuditTemplateEditViewModel { Id = request.Id };
                Refill(vm, request.Name, request.Description, request.Version, request.IsActive, request.AirportId, request.SiteIds, items);
                await FillFormOptionsAsync(vm);
                return View(vm);
            }

            try
            {
                await _auditTemplateService.UpdateAsync(new AuditTemplateEditDTO(
                    request.Id, name, request.Description?.Trim(), version, request.IsActive, CurrentUserId,
                    items.Select(i => new AuditTemplateEditItemsDTO(i.QuestionBankId, i.Mandatory, i.Sequence, i.Section, i.ItemId, i.Responsibility, i.ServiceProvider)).ToList(),
                    request.AirportId, request.SiteIds));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Updating audit template {Id} failed", request.Id);
                ModelState.AddModelError(string.Empty, "The template couldn't be saved. Check the name isn't already used, then try again.");
                var vm = new AuditTemplateEditViewModel { Id = request.Id };
                Refill(vm, request.Name, request.Description, request.Version, request.IsActive, request.AirportId, request.SiteIds, items);
                await FillFormOptionsAsync(vm);
                return View(vm);
            }

            TempData["Success"] = $"Template \"{name}\" was updated.";
            return RedirectToAction(nameof(Index));
        }

        // GET: AuditTemplates/Delete/5
        public async Task<IActionResult> Delete(long id)
        {
            var template = await _auditTemplateService.GetById(id);

            if (template == null)
                return NotFound();
            if (!await CanChangeAsync(id))
                return Forbid();

            var vm = new AuditTemplateDeleteViewModel
            {
                Id = id,
                Name = template.Name,
                Description = template.Description,
                Version = template.Version,
                IsActive = template.IsActive,
                CreatedByName = template.UserDisplayName,
                QuestionCount = template.ItemsDetails.Count
            };

            return View(vm);
        }

        // POST: AuditTemplates/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            if (!await CanChangeAsync(id))
                return Forbid();

            var deleted = await _auditTemplateService.DeleteAsync(id);

            if (!deleted)
            {
                TempData["Error"] = "This template can't be deleted because audits have been scheduled with it. Make it inactive instead.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            TempData["Success"] = "The template was deleted.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(long id)
        {
            if (!await CanChangeAsync(id))
                return Forbid();

            await _auditTemplateService.ToggleActiveAsync(id, CurrentUserId);
            return RedirectToAction(nameof(Index));
        }

        // ------------------------------------------------------------------ helpers

        /// <summary>One question row posted by the wizard. ItemId is set for rows that already existed on the template.</summary>
        private sealed record ItemInput(long? ItemId, long QuestionBankId, bool Mandatory, int Sequence, string? Section, string? Responsibility, string? ServiceProvider);

        // Users linked to an airport see their airport's templates and the all-airport ones, but only change their own airport's.
        private async Task<bool> CanSeeAsync(long id)
        {
            var scope = await _scope.AirportIdAsync();
            return scope == null || await _context.AuditTemplates.AnyAsync(t => t.Id == id && (t.AirportId == null || t.AirportId == scope));
        }

        private async Task<bool> CanChangeAsync(long id)
        {
            var scope = await _scope.AirportIdAsync();
            return scope == null || await _context.AuditTemplates.AnyAsync(t => t.Id == id && t.AirportId == scope);
        }

        private async Task<(string Name, string Version, List<ItemInput> Items)> ValidateAsync(
            long? id, string? rawName, string? rawVersion, long? airportId, List<long>? siteIds,
            List<ItemInput>? rawItems)
        {
            // The wizard posts plain fields; attribute validation on the request record isn't meaningful here.
            ModelState.Clear();

            var name = string.IsNullOrWhiteSpace(rawName) ? "" : Regex.Replace(rawName.Trim(), @"\s+", " ");
            var version = string.IsNullOrWhiteSpace(rawVersion) ? "1.0" : rawVersion.Trim().TrimStart('v', 'V');

            if (name.Length == 0)
                ModelState.AddModelError("Name", "Template name is required.");
            else if (name.Length > 255)
                ModelState.AddModelError("Name", "Template name can't be longer than 255 characters.");
            else if (await _context.AuditTemplates.AnyAsync(t => t.Id != id && t.Name == name))
                ModelState.AddModelError("Name", $"A template named \"{name}\" already exists. Choose another name, or edit that template instead.");

            if (version.Length > 50)
                ModelState.AddModelError("Version", "Version can't be longer than 50 characters.");

            // A question appears once per section (the same check can be asked on Arrival and again on Seating),
            // in the order the user arranged them.
            var items = (rawItems ?? new())
                .Where(i => i.QuestionBankId > 0)
                .Select(i => i with
                {
                    Section = SectionTree.Normalize(i.Section),
                    Responsibility = string.IsNullOrWhiteSpace(i.Responsibility) ? null : i.Responsibility.Trim(),
                    ServiceProvider = string.IsNullOrWhiteSpace(i.ServiceProvider) ? null : i.ServiceProvider.Trim(),
                })
                .GroupBy(i => (i.QuestionBankId, (i.Section ?? "").ToLowerInvariant()))
                .Select(g => g.First())
                .OrderBy(i => i.Sequence)
                .Select((i, index) => i with { Sequence = index + 1 })
                .ToList();

            if (items.Any(i => i.Responsibility?.Length > 100 || i.ServiceProvider?.Length > 200))
                ModelState.AddModelError("Items", "A responsibility can't be longer than 100 characters, or a service provider 200.");

            if (items.Any(i => i.Section?.Length > SectionTree.MaxLength))
                ModelState.AddModelError("Items", $"A section name can't be longer than {SectionTree.MaxLength} characters.");

            if (items.Count == 0)
                ModelState.AddModelError("Items", "Add at least one question.");
            else
            {
                var ids = items.Select(i => i.QuestionBankId).Distinct().ToList();
                var found = await _context.QuestionBanks.CountAsync(q => ids.Contains(q.Id));
                if (found != ids.Count)
                    ModelState.AddModelError("Items", "One or more questions no longer exist. Remove them and try again.");
            }

            var scopeAirport = await _scope.AirportIdAsync();
            if (scopeAirport != null && airportId != scopeAirport)
                ModelState.AddModelError("AirportId", "You can only create templates for your own airport.");
            else if (airportId != null && !await _context.Airports.AnyAsync(a => a.Id == airportId))
                ModelState.AddModelError("AirportId", "Select a valid airport, or leave it blank for all airports.");

            siteIds ??= new();
            if (siteIds.Count > 0)
            {
                var sites = await _context.AuditSites.Where(s => siteIds.Contains(s.Id)).Select(s => new { s.Id, s.AirportId }).ToListAsync();
                if (sites.Count != siteIds.Distinct().Count())
                    ModelState.AddModelError("SiteIds", "One or more selected sites no longer exist.");
                else if (airportId != null && sites.Any(s => s.AirportId != airportId))
                    ModelState.AddModelError("SiteIds", "Selected sites must belong to the selected airport.");
            }

            return (name, version, items);
        }

        private void Refill(AuditTemplateFormViewModel vm, string? name, string? description, string? version, bool isActive,
            long? airportId, List<long>? siteIds, List<ItemInput> items)
        {
            vm.AuditTemplateInfoViewModel = new AuditTemplateInfoViewModel
            {
                Name = name ?? "",
                Description = description ?? "",
                Version = version ?? "",
                IsActive = isActive,
                AirportId = airportId,
                SiteIds = siteIds ?? new(),
            };

            var ids = items.Select(i => i.QuestionBankId).ToList();
            var questions = _context.QuestionBanks
                .Include(q => q.QuestionType)
                .Where(q => ids.Contains(q.Id))
                .ToDictionary(q => q.Id);

            vm.AuditTemplateItems = items
                .Where(i => questions.ContainsKey(i.QuestionBankId))
                .Select(i => new AuditTemplateItemViewModel
                {
                    Id = i.QuestionBankId,
                    ItemId = i.ItemId ?? 0,
                    Section = i.Section,
                    Responsibility = i.Responsibility,
                    ServiceProvider = i.ServiceProvider,
                    Mandatory = i.Mandatory,
                    Sequence = i.Sequence,
                    QuestionText = questions[i.QuestionBankId].QuestionText,
                    Type = questions[i.QuestionBankId].QuestionType?.Name ?? "",
                })
                .ToList();
        }

        private async Task FillFormOptionsAsync(AuditTemplateFormViewModel vm)
        {
            var scopeAirport = await _scope.AirportIdAsync();
            var info = vm.AuditTemplateInfoViewModel;

            info.Airports = await _context.Airports
                .Where(a => (a.IsActive || a.Id == info.AirportId) && (scopeAirport == null || a.Id == scopeAirport))
                .OrderBy(a => a.Code)
                .Select(a => new TemplateAirportOption(a.Id, a.Code, a.Name))
                .ToListAsync();

            info.Sites = await _context.AuditSites
                .Where(s => (s.Status == AuditSiteLookups.Active || info.SiteIds.Contains(s.Id)) && (scopeAirport == null || s.AirportId == scopeAirport))
                .OrderBy(s => s.Airport.Code).ThenBy(s => s.Name)
                .Select(s => new TemplateSiteOption(s.Id, s.Name, s.AirportId, s.Airport.Code, s.SiteSubCategory))
                .ToListAsync();

            vm.Users = (await _appUserService.GetAll())
                .Select(u => new SelectListItem { Value = u.Id.ToString(), Text = u.DisplayName })
                .ToList();

            vm.QuestionBank = (await _questionBankService.GetAll())
                .Select(q => new QuestionBankListViewModel
                {
                    Id = q.Id,
                    Text = q.QuestionText,
                    QuestionType = q.QuestionType,
                    Description = q.Category,
                    Contractor = q.Contractor,
                })
                .ToList();

            vm.Responsibilities = await ListNamesAsync(ReferenceLists.Responsibilities);
            vm.ServiceProviders = (await ListNamesAsync(ReferenceLists.ServiceProviders))
                .Concat(await ListNamesAsync(ReferenceLists.GroundHandlers))
                .Distinct()
                .OrderBy(n => n)
                .ToList();
        }

        private Task<List<string>> ListNamesAsync(string code) =>
            _context.ReferenceListItems
                .Where(i => i.List.Code == code && i.IsActive)
                .OrderBy(i => i.DisplayOrder).ThenBy(i => i.Name)
                .Select(i => i.Name)
                .ToListAsync();

        private long CurrentUserId =>
            long.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
    }
}
