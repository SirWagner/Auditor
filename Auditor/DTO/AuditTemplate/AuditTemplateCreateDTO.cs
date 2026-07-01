namespace Auditor.DTO.AuditTemplate
{
    // Class that Is used to create AuditTemplates using the AuditTemplateService --CREATION ONLY
    public sealed record AuditTemplateCreateDTO(string Name, string Description, string Version,
        bool IsActive, long CreatedBy, List<AuditTemplateItemsDTO> AuditTemplateItemsDTO)
    {
    }

    public sealed record class AuditTemplateItemsDTO(long QuestionBankId, bool Mandatory, int Sequence)
    {
    }
}
