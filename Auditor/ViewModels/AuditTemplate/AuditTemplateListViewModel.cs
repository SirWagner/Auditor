namespace Auditor.ViewModels.AuditTemplate
{
    public class AuditTemplateListViewModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public bool IsActive { get; set; }
        public string CreatedByName { get; set; }
        public DateTime CreatedDate { get; set; }
        public int QuestionCount { get; set; }
    }
}