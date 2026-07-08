using Auditor.DTO.AuditTemplate;
using Auditor.ViewModels.AuditTemplate;

namespace Auditor.Services.Interfaces
{
    public interface IAuditTemplateService
    {
        Task<List<AuditTemplateListViewModel>> GetAllAsync();
        Task<AuditTemplateDetailsDTO> GetById(long id);

        Task CreateAsync(AuditTemplateCreateDTO auditTemplateDTO);
        //Task<AuditTemplateEditViewModel> GetEditViewModelAsync(long id);
        Task UpdateAsync(AuditTemplateEditDTO AuditTemplateEditDTO);

        /// <summary>Deletes the template. Returns false (no-op) if it is referenced by any AuditSchedules.</summary>
        Task<bool> DeleteAsync(long id);

        /// <summary>Flips IsActive. Returns false if the template does not exist.</summary>
        Task<bool> ToggleActiveAsync(long id, long modifiedByUserId);

    }
}
