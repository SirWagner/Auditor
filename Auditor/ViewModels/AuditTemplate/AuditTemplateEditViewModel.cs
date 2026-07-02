using Microsoft.AspNetCore.Mvc.Rendering;

namespace Auditor.ViewModels.AuditTemplate
{
    public class AuditTemplateEditViewModel
    {
        public long Id { get; set; }

        // Template Info
        public AuditTemplateInfoViewModel AuditTemplateInfoViewModel { get; set; }


        // Questions
        public List<AuditTemplateItemViewModel> AuditTemplateItemViewModel { get; set; } = new();

        // Dropdowns
        public List<SelectListItem> Users { get; set; } = new();
        public List<AuditTemplateEditQuestionBankViewModel> QuestionBank { get; set; } = new();
    }

    public class AuditTemplateEditQuestionBankViewModel
    {
        public long Id { get; set; }
        public string Text { get; set; }
        public string QuestionType { get; set; }  // "Boolean", "Date", "Radio", "Checklist", etc.
        public string Description { get; set; }
    }
}