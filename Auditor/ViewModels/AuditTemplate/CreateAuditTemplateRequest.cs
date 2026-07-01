namespace Auditor.ViewModels.AuditTemplate
{
    public sealed record CreateAuditTemplateRequest (string Name, string Description,
                                                    string Version, bool IsActive
                                                    , List<AuditTemplateItemRequest> Items)
    {
    }
    public sealed record AuditTemplateItemRequest(long QuestionBankId, bool Mandatory, int Sequence) { }
}
