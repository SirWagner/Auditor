using Auditor.DTO.AuditTemplate;
using Auditor.Models;
using Auditor.Services.Interfaces;
using Auditor.ViewModels.AuditTemplate;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Auditor.Services
{
    public class AuditTemplateService : IAuditTemplateService
    {
        private readonly AuditorContext _context;
        private readonly ILogger<AuditTemplateService> _logger;

        public AuditTemplateService(AuditorContext context,
    ILogger<AuditTemplateService> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<List<AuditTemplateListViewModel>> GetAllAsync(long? scopeAirportId = null)
        {
            return await _context.AuditTemplates
                .Where(t => scopeAirportId == null || t.AirportId == null || t.AirportId == scopeAirportId)
                .Include(t => t.CreatedByNavigation)
                .Include(t => t.AuditTemplateItems)
                .Select(t => new AuditTemplateListViewModel
                {
                    Id = t.Id,
                    Name = t.Name,
                    Version = t.Version,
                    IsActive = t.IsActive,
                    CreatedByName = t.CreatedByNavigation.DisplayName,
                    CreatedDate = t.CreatedDate,
                    QuestionCount = t.AuditTemplateItems.Count,
                    AirportCode = t.Airport != null ? t.Airport.Code : null,
                    SiteCount = t.Sites.Count
                })
                .ToListAsync();
        }


        public async Task CreateAsync(AuditTemplateCreateDTO AuditTemplateCreateDTO) 
        {
            ArgumentNullException.ThrowIfNull(AuditTemplateCreateDTO);

            if (AuditTemplateCreateDTO.AuditTemplateItemsDTO == null || !AuditTemplateCreateDTO.AuditTemplateItemsDTO.Any())
                throw new ArgumentException("The audit template must contain at least one question.", nameof(AuditTemplateCreateDTO));

            _logger.LogInformation(
                "Creating audit template. ");

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var template = new AuditTemplate
                {
                    Name = AuditTemplateCreateDTO.Name,
                    Description = AuditTemplateCreateDTO.Description,
                    Version = AuditTemplateCreateDTO.Version,
                    IsActive = AuditTemplateCreateDTO.IsActive,
                    CreatedBy = AuditTemplateCreateDTO.CreatedBy,
                    CreatedDate = DateTime.UtcNow,
                    AirportId = AuditTemplateCreateDTO.AirportId,
                };

                await SyncSitesAsync(template, AuditTemplateCreateDTO.SiteIds);

                _context.AuditTemplates.Add(template);

                // Persist first so the template ID is generated.
                await _context.SaveChangesAsync();

                var templateQuestions = AuditTemplateCreateDTO.AuditTemplateItemsDTO.Select(item => new AuditTemplateItem
                {
                    Mandatory = item.Mandatory,
                    QuestionBankId = item.QuestionBankId,
                    Sequence = item.Sequence,
                    Section = item.Section,
                    Responsibility = item.Responsibility,
                    ServiceProvider = item.ServiceProvider,
                    TemplateId = template.Id
                });

                _context.AuditTemplateItems.AddRange(templateQuestions);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation(
                    "Audit template created successfully. TemplateId: {TemplateId}",
                    template.Id);
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();

                _logger.LogError(
                    ex,
                    "Database error while creating audit template. CreatorId: {CreatorId}",
                    AuditTemplateCreateDTO.CreatedBy);

                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                _logger.LogError(
                    ex,
                    "Unexpected error while creating audit template. CreatorId: {CreatorId}",
                    AuditTemplateCreateDTO.CreatedBy);

                throw;
            }
        }
       

        public async Task<AuditTemplateDetailsDTO> GetById(long id)
        {
            var template = await _context.AuditTemplates
                .Include(t => t.AuditTemplateItems)
                    .ThenInclude(i => i.QuestionBank)
                    .ThenInclude(qb => qb.QuestionType)
                .Include(t => t.AuditTemplateItems)
                    .ThenInclude(i => i.QuestionBank)
                    .ThenInclude(qb => qb.Category)
                .Include(user => user.CreatedByNavigation)
                .Include(t => t.Airport)
                .Include(t => t.Sites).ThenInclude(s => s.Airport)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (template == null)
                return null;

            var Questions = template.AuditTemplateItems.Select(item=>new AuditTemplateItemsQuestionDetailsDTO(item.QuestionBankId, item.Mandatory, item.Sequence,
                item.QuestionBank.QuestionText, item.QuestionBank.QuestionType.Name,item.QuestionBank.Category.Name, item.Section, item.Id, item.Responsibility, item.ServiceProvider))
                .OrderBy(i => i.Sequence).ThenBy(i => i.ItemId).ToList();

            var AuditTemplateDetails = new AuditTemplateDetailsDTO(template.Name, template.Description, template.Version,
                template.IsActive, template.CreatedByNavigation?.DisplayName ?? "", Questions,
                template.AirportId, template.Airport?.Name,
                template.Sites.Select(s => s.Id).ToList(),
                template.Sites.OrderBy(s => s.Name).Select(s => $"{s.Airport?.Code} · {s.Name}").ToList());

            return AuditTemplateDetails;

            
        }

        public async Task UpdateAsync(AuditTemplateEditDTO AuditTemplateEditDTO)
        {
            var ExistingAuditTemplate = await _context.AuditTemplates
                .Include(t => t.AuditTemplateItems)
                .Include(t => t.Sites)
                .FirstOrDefaultAsync(t => t.Id == AuditTemplateEditDTO.Id);

            if (ExistingAuditTemplate == null)
                return;

            ExistingAuditTemplate.Name = AuditTemplateEditDTO.Name;
            ExistingAuditTemplate.Description = AuditTemplateEditDTO.Description;
            ExistingAuditTemplate.Version = AuditTemplateEditDTO.Version;
            ExistingAuditTemplate.IsActive = AuditTemplateEditDTO.IsActive;
            ExistingAuditTemplate.ModifiedBy = AuditTemplateEditDTO.ModifiedBy;
            ExistingAuditTemplate.ModifiedDate = DateTime.UtcNow;
            ExistingAuditTemplate.AirportId = AuditTemplateEditDTO.AirportId;
            await SyncSitesAsync(ExistingAuditTemplate, AuditTemplateEditDTO.SiteIds);

            // Pair each posted row with the row it edits: by item id, else the same question in the same section.
            // The same question may sit in several sections (e.g. Arrival and Seating), so question id alone isn't enough.
            var unmatched = ExistingAuditTemplate.AuditTemplateItems.ToList();
            var pairs = new List<(AuditTemplateEditItemsDTO Dto, AuditTemplateItem? Existing)>();
            foreach (var dto in AuditTemplateEditDTO.AuditTemplateItemsDTO)
            {
                var existing = unmatched.FirstOrDefault(i => dto.ItemId != null && i.Id == dto.ItemId && i.QuestionBankId == dto.QuestionBankId)
                    ?? unmatched.FirstOrDefault(i => i.QuestionBankId == dto.QuestionBankId && string.Equals(i.Section ?? "", dto.Section ?? "", StringComparison.OrdinalIgnoreCase));
                if (existing != null)
                    unmatched.Remove(existing);
                pairs.Add((dto, existing));
            }

            var AuditTemplateItemsToRemove = unmatched;

            // Questions that already have audit answers stay on the template so past results remain intact.
            var removeIds = AuditTemplateItemsToRemove.Select(i => i.Id).ToList();
            var answeredIds = await _context.AuditResponses
                .Where(r => removeIds.Contains(r.TemplateItemId))
                .Select(r => r.TemplateItemId)
                .Distinct()
                .ToListAsync();

            // Remove old items
            _context.AuditTemplateItems.RemoveRange(AuditTemplateItemsToRemove.Where(i => !answeredIds.Contains(i.Id)));

            foreach (var (item, existingItem) in pairs)
            {
                if (existingItem != null)
                {
                    if (existingItem.Mandatory != item.Mandatory || existingItem.Sequence != item.Sequence || existingItem.Section != item.Section
                        || existingItem.Responsibility != item.Responsibility || existingItem.ServiceProvider != item.ServiceProvider)
                    {
                        existingItem.ModifiedBy = AuditTemplateEditDTO.ModifiedBy;
                        existingItem.ModifiedDate = DateTime.UtcNow;
                    }
                    existingItem.Mandatory = item.Mandatory;
                    existingItem.Sequence = item.Sequence;
                    existingItem.Section = item.Section;
                    existingItem.Responsibility = item.Responsibility;
                    existingItem.ServiceProvider = item.ServiceProvider;
                }
                else
                {
                    _context.AuditTemplateItems.Add(new AuditTemplateItem
                    {
                        TemplateId = ExistingAuditTemplate.Id,
                        QuestionBankId = item.QuestionBankId,
                        Mandatory = item.Mandatory,
                        Sequence = item.Sequence,
                        Section = item.Section,
                        Responsibility = item.Responsibility,
                        ServiceProvider = item.ServiceProvider,
                        ModifiedBy = AuditTemplateEditDTO.ModifiedBy,
                        ModifiedDate = DateTime.UtcNow,
                    });
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var template = await _context.AuditTemplates
                .Include(t => t.AuditTemplateItems)
                .Include(t => t.AuditSchedules)
                .Include(t => t.Sites)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (template == null)
                return false;

            if (template.AuditSchedules?.Count > 0)
                return false;

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                template.Sites.Clear();
                _context.AuditTemplateItems.RemoveRange(template.AuditTemplateItems);
                _context.AuditTemplates.Remove(template);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Audit template deleted successfully. TemplateId: {TemplateId}", id);

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                _logger.LogError(ex, "Error deleting audit template. TemplateId: {TemplateId}", id);

                throw;
            }
        }

        // Replaces the template's site links with the given sites (an empty list means "all sites").
        private async Task SyncSitesAsync(AuditTemplate template, List<long>? siteIds)
        {
            siteIds ??= new();
            foreach (var site in template.Sites.Where(s => !siteIds.Contains(s.Id)).ToList())
                template.Sites.Remove(site);

            var toAdd = siteIds.Except(template.Sites.Select(s => s.Id)).ToList();
            if (toAdd.Count > 0)
                foreach (var site in await _context.AuditSites.Where(s => toAdd.Contains(s.Id)).ToListAsync())
                    template.Sites.Add(site);
        }

        public async Task<bool> ToggleActiveAsync(long id, long modifiedByUserId)
        {
            var template = await _context.AuditTemplates.FirstOrDefaultAsync(t => t.Id == id);

            if (template == null)
                return false;

            template.IsActive = !template.IsActive;
            template.ModifiedBy = modifiedByUserId;
            template.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
