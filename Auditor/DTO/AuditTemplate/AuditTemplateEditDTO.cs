namespace Auditor.DTO.AuditTemplate
{
    // Class that Is used to create AuditTemplates using the AuditTemplateService --CREATION ONLY
    public sealed record AuditTemplateEditDTO(long Id,string Name, string Description, string Version,
        bool IsActive, long ModifiedBy, List<AuditTemplateEditItemsDTO> AuditTemplateItemsDTO)
    {
    }

    public sealed record class AuditTemplateEditItemsDTO(long QuestionBankId, bool Mandatory, int Sequence)
    {
    }
}
