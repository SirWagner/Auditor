using Auditor.Models;

namespace Auditor.ViewModels.Shared
{
    /// <summary>
    /// Advanced filter for the audit lists (schedules and executions). Every field is optional; empty means "all".
    /// Bound from the query string so filtered views can be bookmarked and shared.
    /// </summary>
    public class AuditFilter
    {
        public long? AirportId { get; set; }

        /// <summary>Site sub-category: Terminal, Airside, Landside, Food &amp; Beverage, Retail…</summary>
        public string? Area { get; set; }

        /// <summary>Site type / location within the area, e.g. "Baggage Reclaim" or "Sit-Down".</summary>
        public string? Location { get; set; }

        public long? SiteId { get; set; }

        public long? TemplateId { get; set; }

        public long? AuditorId { get; set; }

        public DateTime? From { get; set; }

        public DateTime? To { get; set; }

        public string? Status { get; set; }

        public int ActiveCount =>
            new object?[] { AirportId, Area, Location, SiteId, TemplateId, AuditorId, From, To, Status }
                .Count(v => v is not null && (v is not string s || !string.IsNullOrWhiteSpace(s)));

        public void Normalize()
        {
            Area = string.IsNullOrWhiteSpace(Area) ? null : Area.Trim();
            Location = string.IsNullOrWhiteSpace(Location) ? null : Location.Trim();
            Status = string.IsNullOrWhiteSpace(Status) ? null : Status.Trim();
            if (From != null && To != null && To < From)
                (From, To) = (To, From);
        }

        public IQueryable<Auditor.Models.AuditSchedule> Apply(IQueryable<Auditor.Models.AuditSchedule> q)
        {
            if (AirportId != null) q = q.Where(s => s.Site.AirportId == AirportId);
            if (Area != null) q = q.Where(s => s.Site.SiteSubCategory == Area);
            if (Location != null) q = q.Where(s => s.Site.SiteType == Location);
            if (SiteId != null) q = q.Where(s => s.SiteId == SiteId);
            if (TemplateId != null) q = q.Where(s => s.TemplateId == TemplateId);
            if (AuditorId != null) q = q.Where(s => s.Auditors.Any(a => a.Id == AuditorId));
            if (From != null) q = q.Where(s => s.ScheduledDate >= From.Value.Date);
            if (To != null) q = q.Where(s => s.ScheduledDate < To.Value.Date.AddDays(1));
            return q;
        }

        public IQueryable<Auditor.Models.AuditExecution> Apply(IQueryable<Auditor.Models.AuditExecution> q)
        {
            if (AirportId != null) q = q.Where(e => e.Schedule.Site.AirportId == AirportId);
            if (Area != null) q = q.Where(e => e.Schedule.Site.SiteSubCategory == Area);
            if (Location != null) q = q.Where(e => e.Schedule.Site.SiteType == Location);
            if (SiteId != null) q = q.Where(e => e.Schedule.SiteId == SiteId);
            if (TemplateId != null) q = q.Where(e => e.Schedule.TemplateId == TemplateId);
            if (AuditorId != null) q = q.Where(e => e.AuditorId == AuditorId);
            if (From != null) q = q.Where(e => e.Schedule.ScheduledDate >= From.Value.Date);
            if (To != null) q = q.Where(e => e.Schedule.ScheduledDate < To.Value.Date.AddDays(1));
            if (Status != null) q = q.Where(e => e.Status == Status);
            return q;
        }

        /// <summary>The query-string values, for links that keep the current filter.</summary>
        public virtual Dictionary<string, string> RouteValues()
        {
            var d = new Dictionary<string, string>();
            void Add(string k, object? v) { if (v != null && !(v is string s && s.Length == 0)) d[k] = v is DateTime dt ? dt.ToString("yyyy-MM-dd") : v.ToString()!; }
            Add("AirportId", AirportId); Add("Area", Area); Add("Location", Location); Add("SiteId", SiteId);
            Add("TemplateId", TemplateId); Add("AuditorId", AuditorId); Add("From", From); Add("To", To); Add("Status", Status);
            return d;
        }
    }

    /// <summary>Everything the filter pop-up needs to render.</summary>
    public class AuditFilterPanel
    {
        public AuditFilter Filter { get; set; } = new();
        public string Action { get; set; } = "Index";
        public string Controller { get; set; } = "";
        public bool ShowAuditor { get; set; } = true;
        public bool AirportLocked { get; set; }
        public List<(long Id, string Code, string Name)> Airports { get; set; } = new();
        public List<FilterSite> Sites { get; set; } = new();
        public List<(long Id, string Name)> Templates { get; set; } = new();
        public List<(long Id, string Name)> Auditors { get; set; } = new();
        public List<(string Value, string Label)> Statuses { get; set; } = new();

        public string Title { get; set; } = "Filter audits";

        /// <summary>Where Reset and Clear all go, when not to the bare page.</summary>
        public string? ClearUrl { get; set; }

        /// <summary>Values the form keeps as they are, such as the report being viewed.</summary>
        public Dictionary<string, string> Hidden { get; set; } = new();

        /// <summary>More dropdowns for pages with filters of their own.</summary>
        public List<FilterExtra> Extras { get; set; } = new();

        /// <summary>The active filters in words, for a printed report's heading.</summary>
        public List<string> Summary()
        {
            var parts = new List<string>();
            var airport = Airports.FirstOrDefault(a => a.Id == Filter.AirportId);
            parts.Add(airport.Code ?? "All airports");
            if (Filter.Area != null) parts.Add(Filter.Area);
            if (Filter.Location != null) parts.Add(Filter.Location);
            if (Filter.SiteId != null) parts.Add(Sites.FirstOrDefault(s => s.Id == Filter.SiteId)?.Name ?? "");
            if (Filter.From != null || Filter.To != null) parts.Add($"{Filter.From:dd MMM yyyy} to {Filter.To:dd MMM yyyy}");
            parts.AddRange(Extras.Where(x => x.Selected != null).Select(x => $"{x.Label}: {x.SelectedLabel}"));
            return parts;
        }
    }

    public record FilterExtra(string Key, string Label, string AllLabel, string? Selected, List<(string Value, string Label)> Options)
    {
        public string? SelectedLabel => Selected == null ? null : Options.FirstOrDefault(o => o.Value == Selected).Label ?? Selected;
    }

    public record FilterSite(long Id, string Name, long AirportId, string? Area, string? Location);
}
