namespace Auditor.ViewModels.AuditTemplate
{
    public class AuditTemplateItemViewModel
    {
        public long Id { get; set; }
        public bool Mandatory { get; set; }
        public int Sequence { get; set; }
        public string Type { get; set; }
        public string QuestionText { get; set; } // for display only
    }
}