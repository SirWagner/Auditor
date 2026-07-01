using Auditor.DTO.AuditTemplate;
using Auditor.ViewModels.AuditTemplate;

namespace Auditor.Services.Interfaces
{
    public interface IAuditTemplateService
    {
        Task<List<AuditTemplateListViewModel>> GetAllAsync();
        Task<AuditTemplateDetailsViewModel> GetById(long id);

        Task CreateAsync(AuditTemplateCreateDTO auditTemplateDTO);
        Task<AuditTemplateEditViewModel> GetEditViewModelAsync(long id);
        Task UpdateAsync(AuditTemplateEditViewModel model);

    }
}
