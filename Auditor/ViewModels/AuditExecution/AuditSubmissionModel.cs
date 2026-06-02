namespace Auditor.ViewModels.AuditExecution
{
    public class AuditSubmissionModel
    {
        public long ExecutionId { get; set; }
        public List<AuditResponseSubmission> Responses { get; set; }
    }

    public class AuditResponseSubmission
    {
        public long TemplateItemId { get; set; }
        public long? SelectedOptionId { get; set; }
        public bool? Compliant { get; set; }
        public string Comment { get; set; }

        public List<long> SelectedReasonIds { get; set; }
        public string CustomReason { get; set; }
    }
}
