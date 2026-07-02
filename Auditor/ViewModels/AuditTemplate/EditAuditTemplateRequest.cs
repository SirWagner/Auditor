namespace Auditor.ViewModels.AuditTemplate
{
    public sealed record EditAuditTemplateRequest (long Id, string Name, string Description,
                                                    string Version, bool IsActive
                                                    , List<AuditTemplateEditItemRequest> Items)
    {
    }
    public sealed record AuditTemplateEditItemRequest(long QuestionBankId, bool Mandatory, int Sequence) { }
}
