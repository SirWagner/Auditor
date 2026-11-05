using Auditor.Models;

namespace Auditor.ViewModels.AuditExecution
{
    /// <summary>One question on the execution screen, with any answer already saved as a draft.</summary>
    public class QuestionViewModel
    {
        public long TemplateItemId { get; set; }

        public int Number { get; set; }

        public string QuestionText { get; set; } = "";

        public string? Section { get; set; }

        public string? CategoryName { get; set; }

        public string? Recommendation { get; set; }

        public string QuestionTypeName { get; set; } = "";

        /// <summary>For LIST questions: the list's code and the items offered at this site's airport.</summary>
        public string? ListCode { get; set; }

        public List<ReferenceListItem> ListItems { get; set; } = new();

        public string? Responsibility { get; set; }

        public string? ServiceProvider { get; set; }

        public bool IsMandatory { get; set; }

        public List<QuestionBankOption> Options { get; set; } = new();

        public List<QuestionBankReason> ReasonOptions { get; set; } = new();

        // Saved answer
        public long? SelectedOptionId { get; set; }

        public HashSet<long> SelectedOptionIds { get; set; } = new();

        public string? AnswerValue { get; set; }

        public string? Comment { get; set; }

        public HashSet<long> SelectedReasonIds { get; set; } = new();

        public List<AuditResponseAttachment> Attachments { get; set; } = new();
    }
}
