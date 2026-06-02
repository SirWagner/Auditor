using Auditor.Models;

namespace Auditor.ViewModels.AuditExecution
{
    public class AuditExecutionVM
    {
        public Auditor.Models.AuditExecution Execution { get; set; }

        public List<AuditTemplateItem> Questions { get; set; }
    }
}
