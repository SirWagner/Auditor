namespace Auditor.ViewModels.AuditExecution
{
    public class AuditExecutionRenderViewModel
    {
        public long ExecutionId { get; set; }

        public string AuditName { get; set; } = "";

        public string SiteName { get; set; } = "";

        public string? AirportName { get; set; }

        public string AuditorName { get; set; } = "";

        public DateTime ScheduledDate { get; set; }

        public DateTime DueDate { get; set; }

        public string Status { get; set; } = "";

        public string? ReturnReason { get; set; }

        public List<QuestionViewModel> Questions { get; set; } = new();

        /// <summary>Questions arranged as section tabs and sub-location groups (see <see cref="Shared.SectionTree"/>).</summary>
        public Shared.SectionLayout<QuestionViewModel> Layout { get; set; } = new();
    }
}
