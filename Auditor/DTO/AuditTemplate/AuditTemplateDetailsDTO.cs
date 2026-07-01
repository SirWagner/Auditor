namespace Auditor.DTO.AuditTemplate
{
    public record AuditTemplateDetailsDTO (string Name, string Description, string Version,
        bool IsActive, string UserDisplayName,List<AuditTemplateItemsDetailsDTO> ItemsDetails)
    {
    }
    public sealed record class AuditTemplateItemsDetailsDTO(bool Mandatory, int Sequence, string QuestionText, string Type, string Category)
    {
    }
}
