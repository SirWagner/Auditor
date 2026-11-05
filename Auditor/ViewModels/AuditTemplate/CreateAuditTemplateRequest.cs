namespace Auditor.ViewModels.AuditTemplate
{
    public sealed record CreateAuditTemplateRequest(string? Name, string? Description,
                                                    string? Version, bool IsActive,
                                                    List<AuditTemplateItemRequest>? Items,
                                                    long? AirportId = null, List<long>? SiteIds = null)
    {
    }
    public sealed record AuditTemplateItemRequest(long QuestionBankId, bool Mandatory, int Sequence, string? Section = null, long? Id = null, string? Responsibility = null, string? ServiceProvider = null) { }
}
