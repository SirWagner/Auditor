namespace Auditor.ViewModels.AuditTemplate
{
    public sealed record EditAuditTemplateRequest(long Id, string? Name, string? Description,
                                                  string? Version, bool IsActive,
                                                  List<AuditTemplateEditItemRequest>? Items,
                                                  long? AirportId = null, List<long>? SiteIds = null)
    {
    }
    public sealed record AuditTemplateEditItemRequest(long QuestionBankId, bool Mandatory, int Sequence, string? Section = null, long? Id = null, string? Responsibility = null, string? ServiceProvider = null) { }
}
