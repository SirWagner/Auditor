namespace Auditor.ViewModels.AuditTemplate
{
    public class AuditTemplateInfoViewModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public bool IsActive { get; set; }
        public long CreatedBy { get; set; }
    }
}
