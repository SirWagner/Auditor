using Auditor.Models;

namespace Auditor.ViewModels.AuditExecution
{
    public class QuestionViewModel
    {
        public long TemplateItemId { get; set; }

        public string QuestionText { get; set; }

        public long QuestionTypeId { get; set; }
        public string QuestionTypeName { get; set; }  

        public bool IsMandatory { get; set; }

        public bool RequiresReason { get; set; }

        public List<QuestionBankOption> Options { get; set; }

        public List<QuestionBankReason> ReasonOptions { get; set; }
        public List<IFormFile> Attachments { get; set; } = new List<IFormFile>();

    }
}