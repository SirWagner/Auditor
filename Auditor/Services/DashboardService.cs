using System.Globalization;
using Auditor.Models;
using Auditor.Models.Lookups;
using Auditor.Security;
using Auditor.Services.Interfaces;
using Auditor.ViewModels.Dashboard;
using Microsoft.EntityFrameworkCore;

namespace Auditor.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly AuditorContext _context;
        private readonly decimal _targetScore;

        public DashboardService(AuditorContext context, IConfiguration config)
        {
            _context = context;
            _targetScore = config.GetValue("Dashboard:TargetScore", 90m);
        }

        public async Task<DashboardViewModel> GetDashboardAsync(DashboardFilter filter, long? auditorId, long? scopeAirportId = null)
        {
            filter.Normalize();
            if (scopeAirportId != null)
                filter.AirportId = scopeAirportId;

            var nowUtc = DateTime.UtcNow;
            var now = DateTime.Now;
            var since = nowUtc.AddDays(-filter.Days);
            var previousSince = since.AddDays(-filter.Days);

            var vm = new DashboardViewModel
            {
                Filter = filter,
                PersonalScope = auditorId != null,
                TargetScore = _targetScore,
                AirportLocked = scopeAirportId != null,
                Airports = await _context.Airports
                    .Where(a => a.IsActive && (scopeAirportId == null || a.Id == scopeAirportId))
                    .OrderBy(a => a.Code)
                    .Select(a => new AirportOption(a.Id, a.Code, a.Name))
                    .ToListAsync(),
            };

            // ------------------------------------------------------------ submitted audits in scope

            var submitted = _context.AuditExecutions
                .Where(e => (e.Status == ExecutionStatuses.Completed || e.Status == ExecutionStatuses.Closed)
                            && e.SubmissionDate != null
                            && e.AuditExecutionResult != null);

            if (filter.AirportId != null)
                submitted = submitted.Where(e => e.Schedule.Site.AirportId == filter.AirportId);
            if (auditorId != null)
                submitted = submitted.Where(e => e.AuditorId == auditorId);

            var results = await submitted
                .Where(e => e.SubmissionDate >= since)
                .Select(e => new
                {
                    e.Id,
                    Submitted = e.SubmissionDate!.Value,
                    Score = e.AuditExecutionResult.ScorePercentage,
                    e.AuditExecutionResult.NonCompliantCount,
                    Answered = e.AuditExecutionResult.TotalQuestions,
                    AirportId = e.Schedule.Site.AirportId,
                    AirportCode = e.Schedule.Site.Airport.Code,
                    AirportName = e.Schedule.Site.Airport.Name,
                    SiteId = e.Schedule.SiteId,
                    SiteName = e.Schedule.Site.Name,
                })
                .ToListAsync();

            var previous = await submitted
                .Where(e => e.SubmissionDate >= previousSince && e.SubmissionDate < since)
                .Select(e => e.AuditExecutionResult.ScorePercentage)
                .ToListAsync();

            vm.AuditsSubmitted = results.Count;
            vm.PreviousAuditsSubmitted = previous.Count;
            vm.AverageScore = results.Count > 0 ? Math.Round(results.Average(r => r.Score), 1) : null;
            vm.PreviousAverageScore = previous.Count > 0 ? Math.Round(previous.Average(), 1) : null;
            vm.NonCompliantFindings = results.Sum(r => r.NonCompliantCount);
            vm.QuestionsAnswered = results.Sum(r => r.Answered);

            // ------------------------------------------------------------ trend

            var weekly = filter.Days <= 31;
            var buckets = new List<(DateTime Start, DateTime End, string Label)>();
            if (weekly)
            {
                var start = StartOfWeek(now.Date).AddDays(-7 * (int)Math.Ceiling(filter.Days / 7.0) + 7);
                for (var d = start; d <= now.Date; d = d.AddDays(7))
                    buckets.Add((d, d.AddDays(7), d.ToString("dd MMM", CultureInfo.InvariantCulture)));
            }
            else
            {
                var months = filter.Days >= 365 ? 12 : 3;
                var first = new DateTime(now.Year, now.Month, 1).AddMonths(-(months - 1));
                for (var i = 0; i < months; i++)
                {
                    var m = first.AddMonths(i);
                    buckets.Add((m, m.AddMonths(1), m.ToString(months > 6 ? "MMM" : "MMM yyyy", CultureInfo.InvariantCulture)));
                }
            }

            vm.Trend = buckets.Select(b =>
            {
                var inBucket = results.Where(r => r.Submitted.ToLocalTime() >= b.Start && r.Submitted.ToLocalTime() < b.End).ToList();
                return new TrendPoint(b.Label, b.Start, inBucket.Count > 0 ? Math.Round(inBucket.Average(r => r.Score), 1) : null, inBucket.Count);
            }).ToList();

            // ------------------------------------------------------------ breakdowns

            vm.ByAirport = results
                .GroupBy(r => new { r.AirportId, r.AirportCode, r.AirportName })
                .Select(g => new ScoreRow(g.Key.AirportId, g.Key.AirportCode, g.Key.AirportName, Math.Round(g.Average(r => r.Score), 1), g.Count()))
                .OrderBy(r => r.Score)
                .ToList();

            vm.LowestSites = results
                .GroupBy(r => new { r.SiteId, r.SiteName, r.AirportCode })
                .Select(g => new ScoreRow(g.Key.SiteId, g.Key.SiteName, g.Key.AirportCode, Math.Round(g.Average(r => r.Score), 1), g.Count()))
                .OrderBy(r => r.Score)
                .ThenByDescending(r => r.Count)
                .Take(6)
                .ToList();

            var ids = results.Select(r => r.Id).ToList();
            if (ids.Count > 0)
            {
                var answers = _context.AuditResponses.Where(r => ids.Contains(r.ExecutionId) && r.Compliant != null);

                var categories = await answers
                    .GroupBy(r => r.TemplateItem.QuestionBank.Category.Name)
                    .Select(g => new { Name = g.Key, Total = g.Count(), Compliant = g.Count(r => r.Compliant == true) })
                    .ToListAsync();

                vm.ByCategory = categories
                    .Where(c => c.Total > 0)
                    .Select(c => new ScoreRow(null, c.Name, null, Math.Round(c.Compliant * 100m / c.Total, 1), c.Total))
                    .OrderBy(c => c.Score)
                    .ToList();

                vm.TopFailedQuestions = (await answers
                    .Where(r => r.Compliant == false)
                    .GroupBy(r => new { r.TemplateItem.QuestionBank.QuestionText, Category = r.TemplateItem.QuestionBank.Category.Name })
                    .Select(g => new { g.Key.QuestionText, g.Key.Category, Count = g.Count() })
                    .OrderByDescending(g => g.Count)
                    .Take(6)
                    .ToListAsync())
                    .Select(g => new CountRow(g.QuestionText, g.Count, g.Category))
                    .ToList();

                vm.TopReasons = (await answers
                    .Where(r => r.Compliant == false)
                    .SelectMany(r => r.Reasons)
                    .GroupBy(x => x.ReasonText)
                    .Select(g => new { Reason = g.Key, Count = g.Count() })
                    .OrderByDescending(g => g.Count)
                    .Take(6)
                    .ToListAsync())
                    .Select(g => new CountRow(g.Reason, g.Count))
                    .ToList();
            }

            // ------------------------------------------------------------ live work queues

            var openSchedules = _context.AuditSchedules
                .Where(s => s.Status == ScheduleStatuses.Scheduled || s.Status == ScheduleStatuses.InProgress);
            if (filter.AirportId != null)
                openSchedules = openSchedules.Where(s => s.Site.AirportId == filter.AirportId);
            if (auditorId != null)
                openSchedules = openSchedules.Where(s => s.Auditors.Any(a => a.Id == auditorId)
                                                         && !s.AuditExecutions.Any(e => e.AuditorId == auditorId
                                                                                        && (e.Status == ExecutionStatuses.Completed || e.Status == ExecutionStatuses.Closed || e.Status == ExecutionStatuses.Declined)));

            vm.OverdueSchedules = await openSchedules.CountAsync(s => s.DueDate < now);
            vm.DueThisWeek = await openSchedules.CountAsync(s => s.DueDate >= now && s.DueDate < now.AddDays(7));

            vm.Upcoming = (await openSchedules
                .OrderBy(s => s.DueDate)
                .Take(8)
                .Select(s => new
                {
                    s.Id,
                    Template = s.Template.Name,
                    Site = s.Site.Name,
                    s.Site.Airport.Code,
                    s.ScheduledDate,
                    s.DueDate,
                    s.Status,
                    Auditors = s.Auditors.Select(a => a.DisplayName).ToList(),
                })
                .ToListAsync())
                .Select(s => new ScheduleRow(s.Id, s.Template, s.Site, s.Code, s.ScheduledDate, s.DueDate, s.Status,
                    string.Join(", ", s.Auditors), s.DueDate < now))
                .ToList();

            var executions = _context.AuditExecutions.AsQueryable();
            if (filter.AirportId != null)
                executions = executions.Where(e => e.Schedule.Site.AirportId == filter.AirportId);
            if (auditorId != null)
                executions = executions.Where(e => e.AuditorId == auditorId);

            var byStatus = await executions
                .Where(e => e.Status != ExecutionStatuses.Closed && e.Status != ExecutionStatuses.Declined)
                .GroupBy(e => e.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.Status, g => g.Count);

            int Status(string s) => byStatus.TryGetValue(s, out var c) ? c : 0;

            vm.AwaitingReview = Status(ExecutionStatuses.Completed);

            if (auditorId != null)
            {
                vm.ReadyToStart = await openSchedules.CountAsync(s => !s.AuditExecutions.Any(e => e.AuditorId == auditorId));
            }
            else
            {
                vm.ReadyToStart = await openSchedules.CountAsync(s => !s.AuditExecutions.Any());
            }

            vm.Pipeline = new List<PipelineStage>
            {
                new("Not started", vm.ReadyToStart, "pipe-waiting"),
                new("Awaiting acceptance", Status(ExecutionStatuses.PendingAcceptance), "pipe-accept"),
                new("In progress", Status(ExecutionStatuses.InProgress), "pipe-progress"),
                new("Returned for changes", Status(ExecutionStatuses.Rejected), "pipe-returned"),
                new("Awaiting review", Status(ExecutionStatuses.Completed), "pipe-review"),
            };

            if (auditorId == null)
            {
                vm.ReviewQueue = (await executions
                    .Where(e => e.Status == ExecutionStatuses.Completed)
                    .OrderBy(e => e.SubmissionDate)
                    .Take(6)
                    .Select(e => new
                    {
                        e.Id,
                        Template = e.Schedule.Template.Name,
                        Site = e.Schedule.Site.Name,
                        Auditor = e.Auditor.DisplayName,
                        e.SubmissionDate,
                        Score = (decimal?)e.AuditExecutionResult.ScorePercentage,
                    })
                    .ToListAsync())
                    .Select(e => new ReviewRow(e.Id, e.Template, e.Site, e.Auditor, e.SubmissionDate, e.Score))
                    .ToList();
            }

            // ------------------------------------------------------------ programme size

            vm.ActiveAirports = await _context.Airports.CountAsync(a => a.IsActive && (scopeAirportId == null || a.Id == scopeAirportId));
            vm.ActiveSites = await _context.AuditSites.CountAsync(s => s.Status == AuditSiteLookups.Active
                                                                      && (filter.AirportId == null || s.AirportId == filter.AirportId));
            vm.ActiveTemplates = await _context.AuditTemplates.CountAsync(t => t.IsActive);
            vm.ActiveAuditors = await _context.AppUsers.CountAsync(u => u.IsActive && u.Role == Roles.Auditor);

            return vm;
        }

        private static DateTime StartOfWeek(DateTime date) =>
            date.AddDays(-(((int)date.DayOfWeek + 6) % 7));
    }
}
