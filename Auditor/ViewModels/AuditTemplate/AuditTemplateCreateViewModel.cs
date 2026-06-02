using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Auditor.ViewModels.AuditTemplate
{
    public class AuditTemplateCreateViewModel
    {
        // Template basic info
        public string Name { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public bool IsActive { get; set; }
        public long CreatedBy { get; set; }

        // Selected Questions
        public List<AuditTemplateItemViewModel> Items { get; set; } = new();

        // Dropdown data
        public List<SelectListItem> Users { get; set; } = new();
        public List<SelectListItem> QuestionBankTypes { get; set; } = new();
        public List<QuestionBankListViewModel> QuestionBank { get; set; } = new();

    }
    // New ViewModel for question bank list
    public class QuestionBankListViewModel
    {
        public long Id { get; set; }
        public string Text { get; set; }
        public string QuestionType { get; set; }  // "Boolean", "Date", "Radio", "Checklist", etc.
        public string Description { get; set; }
    }
}