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

        public AuditTemplateService(AuditorContext context)
        {
            _context = context;
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

        public async Task<AuditTemplateCreateViewModel> GetCreateViewModelAsync()
        {

            var vm = new AuditTemplateCreateViewModel
            {
                Users = await _context.AppUsers
                    .Select(u => new SelectListItem
                    {
                        Value = u.Id.ToString(),
                        Text = u.DisplayName
                    })
                    .ToListAsync(),

                QuestionBankTypes = await _context.QuestionTypes
                    .Select(q => new SelectListItem
                    {
                        Value = q.Id.ToString(),
                        Text = q.Name
                    })
                    .ToListAsync(),
                QuestionBank = await _context.QuestionBanks
                   .Select(q => new QuestionBankListViewModel
                   {
                       Id = q.Id,
                       Text = q.QuestionText,
                       QuestionType = q.QuestionType.Name,  // or however you reference the type
                       Description = q.QuestionText
                   })
                   .ToListAsync()
            };

            return vm;
        }

        public async Task CreateAsync(AuditTemplateCreateViewModel model)
        {
            var template = new AuditTemplate
            {
                Name = model.Name,
                Description = model.Description,
                Version = model.Version,
                IsActive = model.IsActive,
                CreatedBy = model.CreatedBy,
                CreatedDate = DateTime.UtcNow
            };

            _context.AuditTemplates.Add(template);
            await _context.SaveChangesAsync();

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
        public async Task<AuditTemplateEditViewModel> GetEditViewModelAsync(long id)
        {
            var template = await _context.AuditTemplates
                .Include(t => t.AuditTemplateItems)
                .ThenInclude(i => i.QuestionBank)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (template == null)
                return null;

            var vm = new AuditTemplateEditViewModel
            {
                Id = template.Id,
                Name = template.Name,
                Description = template.Description,
                Version = template.Version,
                IsActive = template.IsActive,
                CreatedBy = template.CreatedBy,

                Items = template.AuditTemplateItems
                    .OrderBy(i => i.Sequence)
                    .Select(i => new AuditTemplateItemViewModel
                    {
                        QuestionBankId = i.QuestionBankId,
                        Mandatory = i.Mandatory,
                        Sequence = i.Sequence,
                        QuestionText = i.QuestionBank.QuestionText
                    }).ToList(),

                Users = await _context.AppUsers
                    .Select(u => new SelectListItem
                    {
                        Value = u.Id.ToString(),
                        Text = u.DisplayName
                    }).ToListAsync(),

                QuestionBank = await _context.QuestionBanks
                    .Where(q => q.IsActive)
                    .Select(q => new SelectListItem
                    {
                        Value = q.Id.ToString(),
                        Text = q.QuestionText
                    }).ToListAsync()
            };

            return vm;
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
    }
}
