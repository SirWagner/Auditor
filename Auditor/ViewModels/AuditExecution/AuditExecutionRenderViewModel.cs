using Auditor.Models;
using System.Collections.Generic;

namespace Auditor.ViewModels.AuditExecution
{
    public class AuditExecutionRenderViewModel
    {
        public long ExecutionId { get; set; }

        public string AuditName { get; set; }

        public string SiteName { get; set; }

        public List<QuestionViewModel> Questions { get; set; }

    }
}