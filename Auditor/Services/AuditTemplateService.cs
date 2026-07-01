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
        public async Task<List<AuditTemplateListViewModel>> GetAllAsync()
        {
            return await _context.AuditTemplates
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
                    QuestionCount = t.AuditTemplateItems.Count
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
                    CreatedDate = DateTime.UtcNow
                };

                _context.AuditTemplates.Add(template);

                // Persist first so the template ID is generated.
                await _context.SaveChangesAsync();

                var templateQuestions = AuditTemplateCreateDTO.AuditTemplateItemsDTO.Select(item => new AuditTemplateItem
                {
                    Mandatory = item.Mandatory,
                    QuestionBankId = item.QuestionBankId,
                    Sequence = item.Sequence,
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
        public async Task UpdateAsync(AuditTemplateEditViewModel model)
        {
            var template = await _context.AuditTemplates
                .Include(t => t.AuditTemplateItems)
                .FirstOrDefaultAsync(t => t.Id == model.Id);

            if (template == null)
                return;

            template.Name = model.Name;
            template.Description = model.Description;
            template.Version = model.Version;
            template.IsActive = model.IsActive;

            // Remove old items
            _context.AuditTemplateItems.RemoveRange(template.AuditTemplateItems);

            int sequence = 1;

            foreach (var item in model.Items)
            {
                _context.AuditTemplateItems.Add(new AuditTemplateItem
                {
                    TemplateId = template.Id,
                    QuestionBankId = item.QuestionBankId,
                    Mandatory = item.Mandatory,
                    Sequence = sequence++
                });
            }

            await _context.SaveChangesAsync();
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
                .FirstOrDefaultAsync(t => t.Id == id);

            if (template == null)
                return null;

            var Questions = template.AuditTemplateItems.Select(item=>new AuditTemplateItemsDetailsDTO(item.Mandatory, item.Sequence,
                item.QuestionBank.QuestionText, item.QuestionBank.QuestionType.Name,item.QuestionBank.Category.Name)).ToList();

            var AuditTemplateDetails = new AuditTemplateDetailsDTO(template.Name, template.Description, template.Version,
                template.IsActive, template.CreatedByNavigation.DisplayName,Questions);

            return AuditTemplateDetails;

            
        }
    }
}
