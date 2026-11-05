namespace Auditor.DTO.AuditTemplate
{
    public record AuditTemplateDetailsDTO(string Name, string Description, string Version,
        bool IsActive, string UserDisplayName, List<AuditTemplateItemsQuestionDetailsDTO> ItemsDetails,
        long? AirportId = null, string? AirportName = null, List<long>? SiteIds = null, List<string>? SiteNames = null)
    {
    }
    public sealed record class AuditTemplateItemsQuestionDetailsDTO(long Id, bool Mandatory, int Sequence, string QuestionText, string Type, string Category,
        string? Section = null, long ItemId = 0, string? Responsibility = null, string? ServiceProvider = null)
    {
    }
}
