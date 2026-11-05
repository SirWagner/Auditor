using Microsoft.AspNetCore.Http;

namespace Auditor.ViewModels.AuditExecution
{
    public class AuditSubmissionModel
    {
        public long ExecutionId { get; set; }
        public List<AuditResponseSubmission> Responses { get; set; } = new();
    }

    public class AuditResponseSubmission
    {
        public long TemplateItemId { get; set; }

        /// <summary>YES_NO, BOOLEAN, RATING-style single choice.</summary>
        public long? SelectedOptionId { get; set; }

        /// <summary>CHECKLIST and MULTIPLE_CHOICE.</summary>
        public List<long>? SelectedOptionIds { get; set; }

        /// <summary>TEXT, NUMBER, DATE, TIME and RATING values.</summary>
        public string? AnswerValue { get; set; }

        public string? Comment { get; set; }

        public List<long>? SelectedReasonIds { get; set; }

        public string? CustomReason { get; set; }

        public List<IFormFile>? Attachments { get; set; }
    }
}
