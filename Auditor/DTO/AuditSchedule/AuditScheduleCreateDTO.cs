namespace Auditor.DTO.AuditSchedule
{
    public record AuditScheduleCreateDTO (long TemplateId, long SiteId, long SchedulerId, DateTime ScheduledDate, DateTime DueDate)
    {
    }
}
