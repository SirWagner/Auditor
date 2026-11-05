namespace Auditor.ViewModels.AuditTemplate
{
    public class AuditTemplateItemViewModel
    {
        /// <summary>The question bank id.</summary>
        public long Id { get; set; }
        /// <summary>The template item row being edited (0 for a question added in this edit).</summary>
        public long ItemId { get; set; }
        public string? Section { get; set; }
        public string? Responsibility { get; set; }
        public string? ServiceProvider { get; set; }
        public bool Mandatory { get; set; }
        public int Sequence { get; set; }
        public string Type { get; set; }
        public string QuestionText { get; set; } // for display only
    }
}