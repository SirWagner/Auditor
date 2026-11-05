namespace Auditor.ViewModels.Dashboard
{
    public class DashboardFilter
    {
        public static readonly int[] PeriodOptions = { 30, 90, 365 };

        public int Days { get; set; } = 90;

        public long? AirportId { get; set; }

        public void Normalize()
        {
            if (!PeriodOptions.Contains(Days))
                Days = 90;
        }
    }

    public class DashboardViewModel
    {
        public DashboardFilter Filter { get; set; } = new();

        /// <summary>True when the figures are limited to the signed-in auditor's own audits.</summary>
        public bool PersonalScope { get; set; }

        public List<AirportOption> Airports { get; set; } = new();

        /// <summary>True when the signed-in user is linked to one airport, so the airport picker is fixed.</summary>
        public bool AirportLocked { get; set; }

        public decimal TargetScore { get; set; }

        // ---- KPIs (selected period) ----
        public decimal? AverageScore { get; set; }
        public decimal? PreviousAverageScore { get; set; }
        public int AuditsSubmitted { get; set; }
        public int PreviousAuditsSubmitted { get; set; }
        public int NonCompliantFindings { get; set; }
        public int QuestionsAnswered { get; set; }

        // ---- KPIs (right now) ----
        public int AwaitingReview { get; set; }
        public int OverdueSchedules { get; set; }
        public int DueThisWeek { get; set; }
        public int ReadyToStart { get; set; }

        // ---- Programme size ----
        public int ActiveAirports { get; set; }
        public int ActiveSites { get; set; }
        public int ActiveTemplates { get; set; }
        public int ActiveAuditors { get; set; }

        // ---- Charts ----
        public List<TrendPoint> Trend { get; set; } = new();
        public List<ScoreRow> ByAirport { get; set; } = new();
        public List<ScoreRow> ByCategory { get; set; } = new();
        public List<ScoreRow> LowestSites { get; set; } = new();
        public List<PipelineStage> Pipeline { get; set; } = new();

        // ---- Lists ----
        public List<CountRow> TopFailedQuestions { get; set; } = new();
        public List<CountRow> TopReasons { get; set; } = new();
        public List<ScheduleRow> Upcoming { get; set; } = new();
        public List<ReviewRow> ReviewQueue { get; set; } = new();
    }

    public record AirportOption(long Id, string Code, string Name);

    /// <summary>One bucket on the compliance trend (a week or a month).</summary>
    public record TrendPoint(string Label, DateTime Start, decimal? Score, int Audits);

    /// <summary>A labelled compliance score with the number of audits (or answers) behind it.</summary>
    public record ScoreRow(long? Id, string Label, string? Detail, decimal Score, int Count);

    public record PipelineStage(string Label, int Count, string CssClass);

    public record CountRow(string Label, int Count, string? Detail = null);

    public record ScheduleRow(long Id, string Template, string Site, string AirportCode, DateTime Scheduled, DateTime Due, string Status, string Auditors, bool Overdue);

    public record ReviewRow(long ExecutionId, string Template, string Site, string Auditor, DateTime? Submitted, decimal? Score);
}

namespace Auditor.ViewModels.Dashboard
{
    /// <summary>Input for the _ScoreBars partial. Kind: "airport", "site" or "category" (decides the row link).</summary>
    public record ScoreBarsModel(List<ScoreRow> Rows, decimal Target, string Kind, string EmptyText);
}
