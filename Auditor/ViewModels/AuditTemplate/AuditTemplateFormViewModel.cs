using Microsoft.AspNetCore.Mvc.Rendering;

namespace Auditor.ViewModels.AuditTemplate
{
    public abstract class AuditTemplateFormViewModel
    {
        public long? Id { get; set; }

        public AuditTemplateInfoViewModel AuditTemplateInfoViewModel { get; set; }
            = new();

        public List<SelectListItem> Users { get; set; }
            = new();

        public List<QuestionBankListViewModel> QuestionBank { get; set; }
            = new();

        public List<AuditTemplateItemViewModel> AuditTemplateItems { get; set; }
            = new();
        public abstract string FormAction { get; }
        public abstract string SubmitText { get; }
    }
    public class QuestionBankListViewModel
    {
        public long Id { get; set; }
        public string Text { get; set; }
        public string QuestionType { get; set; }  // "Boolean", "Date", "Radio", "Checklist", etc.
        public string Description { get; set; }
    }
}
