namespace Auditor.DTO.AuditTemplate
{
    public record AuditTemplateDetailsDTO (string Name, string Description, string Version,
        bool IsActive, string UserDisplayName,List<AuditTemplateItemsQuestionDetailsDTO> ItemsDetails)
    {
    }
    public sealed record class AuditTemplateItemsQuestionDetailsDTO(long Id, bool Mandatory, int Sequence, string QuestionText, string Type, string Category)
    {
    }
}
