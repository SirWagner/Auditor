namespace Auditor.ViewModels.Dashboard
{
    public class DashboardViewModel
    {
        public int TotalAuditors { get; set; }
        public int ActiveAuditors { get; set; }

        public int TotalUsers { get; set; }
        public int AdminCount { get; set; }
        public int ViewerCount { get; set; }

        public int TotalTemplates { get; set; }
        public int ActiveTemplates { get; set; }

        public int TotalSites { get; set; }
        public int ActiveSites { get; set; }

        public int ScheduledAudits { get; set; }
        public int CancelledSchedules { get; set; }
        public int OverdueSchedules { get; set; }

        public int ExecutionsPendingAcceptance { get; set; }
        public int ExecutionsInProgress { get; set; }
        public int ExecutionsCompleted { get; set; }
        public int ExecutionsClosed { get; set; }
        public int ExecutionsRejected { get; set; }

        public int PendingChangeRequests { get; set; }

        public List<RecentExecutionViewModel> RecentExecutions { get; set; } = new();
    }

    public class RecentExecutionViewModel
    {
        public long Id { get; set; }
        public string TemplateName { get; set; }
        public string SiteName { get; set; }
        public string Status { get; set; }
        public DateTime Date { get; set; }
    }
}
