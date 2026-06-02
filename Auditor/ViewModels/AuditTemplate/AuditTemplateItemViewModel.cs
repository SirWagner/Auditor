namespace Auditor.ViewModels.AuditTemplate
{
    public class AuditTemplateItemViewModel
    {
        public long QuestionBankId { get; set; }
        public bool Mandatory { get; set; }
        public int Sequence { get; set; }
        public string QuestionText { get; set; } // for display only
    }
}