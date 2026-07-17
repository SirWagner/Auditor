namespace Auditor.ViewModels.AuditTemplate
{
    public class AuditTemplateDeleteViewModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public bool IsActive { get; set; }
        public string CreatedByName { get; set; }
        public int QuestionCount { get; set; }
    }
}
