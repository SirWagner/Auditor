namespace Auditor.DTO.AuditTemplate
{
    public sealed record AuditTemplateEditDTO(long Id, string Name, string Description, string Version,
        bool IsActive, long ModifiedBy, List<AuditTemplateEditItemsDTO> AuditTemplateItemsDTO,
        long? AirportId = null, List<long>? SiteIds = null)
    {
    }

    public sealed record class AuditTemplateEditItemsDTO(long QuestionBankId, bool Mandatory, int Sequence, string? Section = null, long? ItemId = null, string? Responsibility = null, string? ServiceProvider = null)
    {
    }
}
