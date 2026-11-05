using System.Text;
using Auditor.Models;
using Auditor.Models.Lookups;
using Auditor.Security;
using Auditor.Services;
using Auditor.ViewModels.Reports;
using Auditor.ViewModels.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Auditor.Controllers
{
    [Authorize(Roles = Roles.Reporting)]
    public class ReportsController : Controller
    {
        private static readonly (string Value, string Label)[] StatusOptions =
            new[] { ExecutionStatuses.Completed, ExecutionStatuses.Closed }
                .Select(s => (s, ExecutionStatuses.Label(s)))
                .ToArray();

        private readonly AuditorContext _context;
        private readonly ReportService _reports;
        private readonly AuditFilterOptions _filterOptions;
        private readonly UserScope _scope;
        private readonly IConfiguration _config;

        public ReportsController(AuditorContext context, ReportService reports, AuditFilterOptions filterOptions, UserScope scope, IConfiguration config)
        {
            _context = context;
            _reports = reports;
            _filterOptions = filterOptions;
            _scope = scope;
            _config = config;
        }

        // GET: Reports?Report=provider&Responsibility=Ramp Handling Company&From=2026-10-01
        public async Task<IActionResult> Index([FromQuery] ReportFilter filter)
        {
            await PrepareAsync(filter);

            var vm = new ReportViewModel
            {
                Filter = filter,
                Panel = await BuildPanelAsync(filter),
                Table = await _reports.BuildTableAsync(filter, Url.Action(nameof(Index))!, Url.Action("Details", "AuditExecutions")!),
                TargetScore = _config.GetValue("Dashboard:TargetScore", 90m),
            };
            await _reports.FillSummaryAsync(vm);
            return View(vm);
        }

        // GET: Reports/Export  the report on screen as a CSV file, with every row
        public async Task<IActionResult> Export([FromQuery] ReportFilter filter)
        {
            await PrepareAsync(filter);
            var table = await _reports.BuildTableAsync(filter, Url.Action(nameof(Index))!, Url.Action("Details", "AuditExecutions")!, export: true);

            var csv = new StringBuilder();
            csv.AppendLine(string.Join(",", table.Columns.Select(c => CsvField(c.Title))));
            foreach (var row in table.Rows)
                csv.AppendLine(string.Join(",", row.Select(c => CsvField(c.Text))));

            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray();
            return File(bytes, "text/csv", $"auditor-{filter.Report}-{DateTime.Now:yyyy-MM-dd}.csv");
        }

        private async Task PrepareAsync(ReportFilter filter)
        {
            filter.Normalize();
            filter.AirportId = await _scope.NarrowAsync(filter.AirportId);
        }

        private async Task<AuditFilterPanel> BuildPanelAsync(ReportFilter filter)
        {
            var panel = await _filterOptions.BuildAsync(filter, "Reports", StatusOptions, showAuditor: true);
            panel.Title = "Filter report";
            panel.ClearUrl = Url.Action(nameof(Index)) + "?Report=" + filter.Report;
            panel.Hidden["Report"] = filter.Report!;

            var responsibilities = await _reports.ResponsibilitiesAsync(filter);
            var providers = await _reports.ProvidersAsync(filter);
            var airlines = await _reports.AirlinesAsync(filter);
            var categories = await _context.QuestionCategories.OrderBy(c => c.Name).Select(c => new { c.Id, c.Name }).ToListAsync();

            panel.Extras.Add(new FilterExtra("Responsibility", "Responsibility", "All responsibilities", filter.Responsibility,
                responsibilities.Select(r => (r, r)).ToList()));
            panel.Extras.Add(new FilterExtra("ServiceProvider", "Service provider", "All service providers", filter.ServiceProvider,
                providers.Append(Responsibilities.NotNamed).Select(p => (p, p)).ToList()));
            panel.Extras.Add(new FilterExtra("CategoryId", "Category", "All categories", filter.CategoryId?.ToString(),
                categories.Select(c => (c.Id.ToString(), c.Name)).ToList()));
            panel.Extras.Add(new FilterExtra("Airline", "Airline", "All airlines", filter.Airline,
                airlines.Select(a => (a, a)).ToList()));
            return panel;
        }

        // Quotes the field, and stops a value starting with =, +, - or @ being run as a spreadsheet formula.
        private static string CsvField(string? value)
        {
            value ??= "";
            if (value.Length > 0 && "=+-@\t\r".Contains(value[0]))
                value = "'" + value;
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }
    }
}
