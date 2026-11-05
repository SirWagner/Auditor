using Auditor.ViewModels.Shared;

namespace Auditor.ViewModels.Reports
{
    /// <summary>The report asked for and its filters. Bound from the query string so a report can be bookmarked and shared.</summary>
    public class ReportFilter : AuditFilter
    {
        public string? Report { get; set; }

        public string? Responsibility { get; set; }

        public string? ServiceProvider { get; set; }

        public long? CategoryId { get; set; }

        public string? Airline { get; set; }

        /// <summary>True when a filter narrows the answers, not just the audits.</summary>
        public bool FiltersAnswers => Responsibility != null || ServiceProvider != null || CategoryId != null;

        public new void Normalize()
        {
            base.Normalize();
            Report = ReportTypes.All.Any(t => t.Key == Report) ? Report : ReportTypes.Responsibility;
            Responsibility = string.IsNullOrWhiteSpace(Responsibility) ? null : Responsibility.Trim();
            ServiceProvider = string.IsNullOrWhiteSpace(ServiceProvider) ? null : ServiceProvider.Trim();
            Airline = string.IsNullOrWhiteSpace(Airline) ? null : Airline.Trim();
        }

        public override Dictionary<string, string> RouteValues()
        {
            var d = base.RouteValues();
            void Add(string k, object? v) { if (v != null) d[k] = v.ToString()!; }
            Add("Report", Report); Add("Responsibility", Responsibility); Add("ServiceProvider", ServiceProvider);
            Add("CategoryId", CategoryId); Add("Airline", Airline);
            return d;
        }

        /// <summary>The query string for this filter with some values replaced. A null value removes that filter.</summary>
        public string Query(params (string Key, string? Value)[] overrides)
        {
            var values = RouteValues();
            foreach (var (key, value) in overrides)
            {
                if (value == null) values.Remove(key);
                else values[key] = value;
            }
            return "?" + string.Join("&", values.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"));
        }
    }

    public static class ReportTypes
    {
        public const string Responsibility = "responsibility";
        public const string Provider = "provider";
        public const string Questions = "questions";
        public const string Register = "register";
        public const string Audits = "audits";

        public static readonly (string Key, string Title, string Description)[] All =
        {
            (Responsibility, "By responsibility", "Compliance for each party: ACSA, ramp handling company, cleaning, M&E and others."),
            (Provider, "By service provider", "Compliance for each company: BidAir, Swissport, Menzies, cleaning contractors, concessionaires."),
            (Questions, "Failing questions", "The questions most often answered Not compliant."),
            (Register, "Non-compliance register", "Every Not compliant answer, with its reasons and comments."),
            (Audits, "Audit list", "Every submitted audit, with its score and flight details."),
        };
    }

    /// <summary>A report as rows and columns: the page and the CSV export both draw from this.</summary>
    public class ReportTable
    {
        public List<ReportColumn> Columns { get; set; } = new();
        public List<List<ReportCell>> Rows { get; set; } = new();

        /// <summary>Set when more rows matched than are shown on the page.</summary>
        public int? TotalRows { get; set; }
    }

    public record ReportColumn(string Title, bool Numeric = false, string? Width = null);

    public record ReportCell(string Text, string? Link = null, decimal? Percent = null, bool Strong = false)
    {
        public static implicit operator ReportCell(string? text) => new(text ?? "");
    }

    public class ReportViewModel
    {
        public ReportFilter Filter { get; set; } = new();
        public AuditFilterPanel Panel { get; set; } = new();
        public ReportTable Table { get; set; } = new();
        public int Audits { get; set; }
        public int Compliant { get; set; }
        public int NonCompliant { get; set; }
        public decimal? CompliancePercent { get; set; }
        public decimal TargetScore { get; set; }
    }
}
