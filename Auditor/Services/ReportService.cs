using Auditor.Models;
using Auditor.Models.Lookups;
using Auditor.ViewModels.Reports;
using Microsoft.EntityFrameworkCore;

namespace Auditor.Services
{
    /// <summary>
    /// Builds the reports from submitted audits. Compliance counts Compliant and Not compliant answers only:
    /// Not applicable answers and typed values (times, flight details) aren't scored. Responsibility and service
    /// provider are read from the answer, where they were recorded at submission.
    /// </summary>
    public class ReportService
    {
        private const int PageRows = 500;
        private const int ExportRows = 50000;

        private readonly AuditorContext _context;

        public ReportService(AuditorContext context)
        {
            _context = context;
        }

        // ------------------------------------------------------------------ queries

        private IQueryable<AuditExecution> Executions(ReportFilter f)
        {
            var q = f.Apply(_context.AuditExecutions.Where(e => e.Status == ExecutionStatuses.Completed || e.Status == ExecutionStatuses.Closed));
            if (f.Airline != null)
                q = q.Where(e => e.AuditResponses.Any(r => r.TemplateItem.QuestionBank.ReferenceList!.Code == ReferenceLists.Airlines && r.AnswerValue == f.Airline));
            return q;
        }

        private IQueryable<AuditResponse> ScoredAnswers(ReportFilter f)
        {
            var executionIds = Executions(f).Select(e => e.Id);
            var q = _context.AuditResponses.Where(r => r.Compliant != null && executionIds.Contains(r.ExecutionId));

            if (f.Responsibility != null) q = q.Where(r => r.Responsibility == f.Responsibility);
            if (f.ServiceProvider == Responsibilities.NotNamed) q = q.Where(r => r.ServiceProvider == null);
            else if (f.ServiceProvider != null) q = q.Where(r => r.ServiceProvider == f.ServiceProvider);
            if (f.CategoryId != null) q = q.Where(r => r.TemplateItem.QuestionBank.CategoryId == f.CategoryId);
            return q;
        }

        // "No responsibility" answers belong to no company, unless someone asks for them.
        private IQueryable<AuditResponse> ProviderAnswers(ReportFilter f)
        {
            var answers = ScoredAnswers(f);
            return f.Responsibility != null
                ? answers
                : answers.Where(r => r.Responsibility != Responsibilities.None && r.Responsibility != Responsibilities.NotAssigned);
        }

        public async Task FillSummaryAsync(ReportViewModel vm)
        {
            var answers = vm.Filter.Report == ReportTypes.Provider ? ProviderAnswers(vm.Filter) : ScoredAnswers(vm.Filter);
            vm.Compliant = await answers.CountAsync(r => r.Compliant == true);
            vm.NonCompliant = await answers.CountAsync(r => r.Compliant == false);
            vm.Audits = await answers.Select(r => r.ExecutionId).Distinct().CountAsync();
            vm.CompliancePercent = Percent(vm.Compliant, vm.NonCompliant);
        }

        public Task<ReportTable> BuildTableAsync(ReportFilter f, string reportUrl, string auditUrl, bool export = false) => f.Report switch
        {
            ReportTypes.Provider => ProviderTableAsync(f, reportUrl),
            ReportTypes.Questions => QuestionTableAsync(f, reportUrl),
            ReportTypes.Register => RegisterTableAsync(f, auditUrl, export),
            ReportTypes.Audits => AuditTableAsync(f, auditUrl, export),
            _ => ResponsibilityTableAsync(f, reportUrl),
        };

        // ------------------------------------------------------------------ filter choices

        public Task<List<string>> ProvidersAsync(ReportFilter f) =>
            ScoredAnswers(new ReportFilter { AirportId = f.AirportId })
                .Where(r => r.ServiceProvider != null)
                .Select(r => r.ServiceProvider!)
                .Distinct()
                .OrderBy(p => p)
                .ToListAsync();

        public Task<List<string>> ResponsibilitiesAsync(ReportFilter f) =>
            ScoredAnswers(new ReportFilter { AirportId = f.AirportId })
                .Where(r => r.Responsibility != null)
                .Select(r => r.Responsibility!)
                .Distinct()
                .OrderBy(p => p)
                .ToListAsync();

        public Task<List<string>> AirlinesAsync(ReportFilter f)
        {
            var executionIds = Executions(new ReportFilter { AirportId = f.AirportId }).Select(e => e.Id);
            return _context.AuditResponses
                .Where(r => executionIds.Contains(r.ExecutionId) && r.AnswerValue != null
                            && r.TemplateItem.QuestionBank.ReferenceList!.Code == ReferenceLists.Airlines)
                .Select(r => r.AnswerValue!)
                .Distinct()
                .OrderBy(a => a)
                .ToListAsync();
        }

        // ------------------------------------------------------------------ reports

        private static List<ReportColumn> ScoreColumns() => new()
        {
            new("Audits", Numeric: true, Width: "9%"),
            new("Compliant", Numeric: true, Width: "10%"),
            new("Not compliant", Numeric: true, Width: "11%"),
            new("Compliance", Width: "24%"),
        };

        private static IEnumerable<ReportCell> ScoreCells(int audits, int compliant, int nonCompliant)
        {
            var percent = Percent(compliant, nonCompliant);
            yield return audits.ToString();
            yield return compliant.ToString();
            yield return nonCompliant.ToString();
            yield return new ReportCell(percent == null ? "—" : $"{percent:0.#}%", Percent: percent);
        }

        private async Task<ReportTable> ResponsibilityTableAsync(ReportFilter f, string reportUrl)
        {
            var rows = await ScoredAnswers(f)
                .GroupBy(r => r.Responsibility)
                .Select(g => new
                {
                    g.Key,
                    Audits = g.Select(x => x.ExecutionId).Distinct().Count(),
                    Compliant = g.Count(x => x.Compliant == true),
                    NonCompliant = g.Count(x => x.Compliant == false),
                })
                .ToListAsync();

            var table = new ReportTable { Columns = new List<ReportColumn> { new("Responsibility") }.Concat(ScoreColumns()).ToList() };
            // Lowest compliance first; "nobody" rows go last because there is no one to act on them.
            foreach (var r in rows.OrderBy(r => IsNobody(r.Key)).ThenBy(r => Percent(r.Compliant, r.NonCompliant)).ThenBy(r => r.Key))
            {
                var name = r.Key ?? Responsibilities.NotAssigned;
                var link = reportUrl + f.Query(("Report", ReportTypes.Provider), ("Responsibility", name));
                table.Rows.Add(new List<ReportCell> { new(name, link, Strong: true) }.Concat(ScoreCells(r.Audits, r.Compliant, r.NonCompliant)).ToList());
            }
            return table;
        }

        private async Task<ReportTable> ProviderTableAsync(ReportFilter f, string reportUrl)
        {
            var rows = await ProviderAnswers(f)
                .GroupBy(r => new { r.ServiceProvider, r.Responsibility })
                .Select(g => new
                {
                    g.Key.ServiceProvider,
                    g.Key.Responsibility,
                    Audits = g.Select(x => x.ExecutionId).Distinct().Count(),
                    Compliant = g.Count(x => x.Compliant == true),
                    NonCompliant = g.Count(x => x.Compliant == false),
                })
                .ToListAsync();

            var table = new ReportTable
            {
                Columns = new List<ReportColumn> { new("Service provider"), new("Responsibility") }.Concat(ScoreColumns()).ToList(),
            };
            foreach (var r in rows.OrderBy(r => r.ServiceProvider == null).ThenBy(r => Percent(r.Compliant, r.NonCompliant)).ThenBy(r => r.ServiceProvider))
            {
                var name = r.ServiceProvider ?? Responsibilities.NotNamed;
                var link = reportUrl + f.Query(("Report", ReportTypes.Register), ("ServiceProvider", name), ("Responsibility", r.Responsibility));
                table.Rows.Add(new List<ReportCell> { new(name, link, Strong: true), r.Responsibility }
                    .Concat(ScoreCells(r.Audits, r.Compliant, r.NonCompliant)).ToList());
            }
            return table;
        }

        private async Task<ReportTable> QuestionTableAsync(ReportFilter f, string reportUrl)
        {
            var rows = await ScoredAnswers(f)
                .GroupBy(r => new { r.TemplateItem.QuestionBank.QuestionText, r.Responsibility })
                .Select(g => new
                {
                    g.Key.QuestionText,
                    g.Key.Responsibility,
                    Audits = g.Select(x => x.ExecutionId).Distinct().Count(),
                    Compliant = g.Count(x => x.Compliant == true),
                    NonCompliant = g.Count(x => x.Compliant == false),
                })
                .Where(g => g.NonCompliant > 0)
                .OrderByDescending(g => g.NonCompliant)
                .Take(PageRows)
                .ToListAsync();

            var table = new ReportTable
            {
                Columns = new List<ReportColumn> { new("Question", Width: "38%"), new("Responsibility") }.Concat(ScoreColumns()).ToList(),
            };
            foreach (var r in rows)
                table.Rows.Add(new List<ReportCell> { r.QuestionText, r.Responsibility }.Concat(ScoreCells(r.Audits, r.Compliant, r.NonCompliant)).ToList());
            return table;
        }

        private async Task<ReportTable> RegisterTableAsync(ReportFilter f, string auditUrl, bool export)
        {
            var failures = ScoredAnswers(f).Where(r => r.Compliant == false);
            var total = await failures.CountAsync();
            var limit = export ? ExportRows : PageRows;

            var rows = await failures
                .OrderByDescending(r => r.Execution.Schedule.ScheduledDate).ThenBy(r => r.ExecutionId).ThenBy(r => r.TemplateItem.Sequence)
                .Take(limit)
                .Select(r => new
                {
                    r.ExecutionId,
                    r.Execution.Schedule.ScheduledDate,
                    Airport = r.Execution.Schedule.Site.Airport.Code,
                    Site = r.Execution.Schedule.Site.Name,
                    Template = r.Execution.Schedule.Template.Name,
                    r.TemplateItem.Section,
                    Question = r.TemplateItem.QuestionBank.QuestionText,
                    r.Responsibility,
                    r.ServiceProvider,
                    Reasons = r.Reasons.Select(x => x.ReasonText).ToList(),
                    r.Comment,
                    Auditor = r.Execution.Auditor!.DisplayName,
                })
                .AsSplitQuery()
                .ToListAsync();

            var table = new ReportTable
            {
                TotalRows = total > limit ? total : null,
                Columns = new()
                {
                    new("Date", Width: "8%"), new("Airport", Width: "6%"), new("Site"), new("Audit"), new("Section"), new("Question", Width: "20%"),
                    new("Responsibility"), new("Service provider"), new("Reasons"), new("Comment"), new("Auditor"),
                },
            };
            foreach (var r in rows)
            {
                table.Rows.Add(new List<ReportCell>
                {
                    new(r.ScheduledDate.ToString("yyyy-MM-dd"), $"{auditUrl}/{r.ExecutionId}"),
                    r.Airport, r.Site, r.Template, r.Section, r.Question,
                    r.Responsibility, r.ServiceProvider ?? Responsibilities.NotNamed,
                    string.Join("; ", r.Reasons), r.Comment, r.Auditor,
                });
            }
            return table;
        }

        private async Task<ReportTable> AuditTableAsync(ReportFilter f, string auditUrl, bool export)
        {
            var executions = Executions(f);
            if (f.FiltersAnswers)
            {
                var matching = ScoredAnswers(f).Select(r => r.ExecutionId);
                executions = executions.Where(e => matching.Contains(e.Id));
            }

            var total = await executions.CountAsync();
            var limit = export ? ExportRows : PageRows;
            var rows = await executions
                .OrderByDescending(e => e.Schedule.ScheduledDate).ThenByDescending(e => e.Id)
                .Take(limit)
                .Select(e => new
                {
                    e.Id,
                    e.Schedule.ScheduledDate,
                    Airport = e.Schedule.Site.Airport.Code,
                    Site = e.Schedule.Site.Name,
                    Template = e.Schedule.Template.Name,
                    Auditor = e.Auditor!.DisplayName,
                    e.Status,
                    Score = e.AuditExecutionResult == null ? (decimal?)null : e.AuditExecutionResult.ScorePercentage,
                    NonCompliant = e.AuditExecutionResult == null ? 0 : e.AuditExecutionResult.NonCompliantCount,
                })
                .ToListAsync();

            // Flight details are ordinary answers; pick out the ones the list needs.
            var ids = rows.Select(r => r.Id).ToList();
            var details = (await _context.AuditResponses
                    .Where(r => ids.Contains(r.ExecutionId) && r.AnswerValue != null
                                && (r.TemplateItem.QuestionBank.ReferenceList != null || r.TemplateItem.QuestionBank.QuestionText.StartsWith("Record flight number")))
                    .Select(r => new { r.ExecutionId, r.AnswerValue, Code = r.TemplateItem.QuestionBank.ReferenceList!.Code })
                    .ToListAsync())
                .ToLookup(r => (r.ExecutionId, r.Code));
            string? Detail(long id, string? code) => details[(id, code)].FirstOrDefault()?.AnswerValue;

            var showFlight = details.Count > 0;
            var table = new ReportTable
            {
                TotalRows = total > limit ? total : null,
                Columns = new() { new("Date", Width: "9%"), new("Airport", Width: "6%"), new("Site"), new("Audit"), new("Auditor") },
            };
            if (showFlight)
                table.Columns.AddRange(new ReportColumn[] { new("Flight"), new("Airline"), new("Ground handler"), new("Aircraft") });
            table.Columns.AddRange(new ReportColumn[] { new("Status"), new("Not compliant", Numeric: true), new("Score", Width: "14%") });

            foreach (var r in rows)
            {
                var row = new List<ReportCell>
                {
                    new(r.ScheduledDate.ToString("yyyy-MM-dd"), $"{auditUrl}/{r.Id}"),
                    r.Airport, r.Site, r.Template, r.Auditor,
                };
                if (showFlight)
                    row.AddRange(new ReportCell[] { Detail(r.Id, null), Detail(r.Id, ReferenceLists.Airlines), Detail(r.Id, ReferenceLists.GroundHandlers), Detail(r.Id, ReferenceLists.AircraftTypes) });
                row.Add(ExecutionStatuses.Label(r.Status));
                row.Add(r.NonCompliant.ToString());
                row.Add(new ReportCell(r.Score == null ? "—" : $"{r.Score:0.#}%", Percent: r.Score));
                table.Rows.Add(row);
            }
            return table;
        }

        // ------------------------------------------------------------------ helpers

        private static decimal? Percent(int compliant, int nonCompliant) =>
            compliant + nonCompliant == 0 ? null : Math.Round(compliant * 100m / (compliant + nonCompliant), 1);

        private static bool IsNobody(string? responsibility) =>
            responsibility is null or Responsibilities.None or Responsibilities.NotAssigned;
    }
}
