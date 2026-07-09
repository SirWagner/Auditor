using Auditor.DTO.AuditSchedule;
using Auditor.Models;

namespace Auditor.Services.Interfaces
{
    public interface IAuditScheduleService
    {
        public Task<AuditSchedule> Create(AuditScheduleCreateDTO AuditScheduleCreateDTO);
    }
}
