using Microsoft.AspNetCore.Mvc.Rendering;

namespace Auditor.ViewModels.AuditTemplate
{
    public class AuditTemplateDetailsViewModel
    {
        // Template Info
        public string Name { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public bool IsActive { get; set; }

        public string CreatedBy { get; set; }

        // Questions
        public List<AuditTemplateDetailsQuestionsViewModel> Items { get; set; } = new();

        // Dropdowns
        //public List<SelectListItem> Users { get; set; } = new();
        //public List<SelectListItem> QuestionBank { get; set; } = new();
    }
    public class AuditTemplateDetailsQuestionsViewModel
    {

        public bool Mandatory { get; set; }
        public int Sequence { get ;set;}
        public string QuestionText { get ;set;}
        public string Type { get ;set;}
        public string Category { get ;set;}
    }
}
