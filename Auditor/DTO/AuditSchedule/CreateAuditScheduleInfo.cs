using Microsoft.AspNetCore.Mvc.Rendering;

namespace Auditor.DTO.AuditSchedule
{
    public sealed record CreateAuditScheduleInfo(
            int? SchedulerId,
            int? SiteId,
            int? TemplateId,
            DateTime? DueDate,
            IReadOnlyList<SelectListItem> Schedulers,
            IReadOnlyList<SelectListItem> Sites,
            IReadOnlyList<SelectListItem> Templates
        );
}
