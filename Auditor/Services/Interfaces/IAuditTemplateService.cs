using Auditor.ViewModels.AuditTemplate;

namespace Auditor.Services.Interfaces
{
    public interface IAuditTemplateService
    {
        Task<List<AuditTemplateListViewModel>> GetAllAsync();
        Task<AuditTemplateCreateViewModel> GetCreateViewModelAsync();
        Task CreateAsync(AuditTemplateCreateViewModel model);
        Task<AuditTemplateEditViewModel> GetEditViewModelAsync(long id);
        Task UpdateAsync(AuditTemplateEditViewModel model);

    }
}
