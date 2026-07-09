using Microsoft.VisualBasic;

namespace Auditor.ViewModels.AuditSchedule
{
    public sealed record CreateAuditScheduleRequest(long TemplateId, long SiteId, long SchedulerId, DateTime ScheduledDate, DateTime DueDate)
    {
    }
}
