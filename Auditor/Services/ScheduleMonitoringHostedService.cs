using Auditor.Models;
using Microsoft.EntityFrameworkCore;

namespace Auditor.Services
{
    /// <summary>
    /// Polls for schedules past their due date and pending change requests that have aged out,
    /// implementing BR7.5 (auto-cancellation) and BR7.8 (escalation) without a full job scheduler.
    /// </summary>
    public class ScheduleMonitoringHostedService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ScheduleMonitoringHostedService> _logger;
        private static readonly TimeSpan PollInterval = TimeSpan.FromMinutes(15);
        private static readonly TimeSpan EscalationAge = TimeSpan.FromDays(2);

        public ScheduleMonitoringHostedService(IServiceProvider serviceProvider, ILogger<ScheduleMonitoringHostedService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(PollInterval);

            do
            {
                try
                {
                    await RunOnceAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Schedule monitoring pass failed");
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }

        private async Task RunOnceAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AuditorContext>();

            var now = DateTime.UtcNow;

            // BR7.5: auto-cancel schedules past their due date with no action taken.
            var overdueSchedules = await context.AuditSchedules
                .Where(s => s.Status == "SCHEDULED" && s.DueDate < now)
                .ToListAsync(cancellationToken);

            foreach (var schedule in overdueSchedules)
            {
                schedule.Status = "CANCELLED";
                schedule.CancellationReason = "Auto-cancelled: no action taken before due date";
            }

            if (overdueSchedules.Count > 0)
                _logger.LogInformation("Auto-cancelled {Count} overdue audit schedule(s)", overdueSchedules.Count);

            // BR7.8: escalate change requests that have sat pending too long.
            var staleRequests = await context.ScheduleChangeRequests
                .Where(r => r.Status == "Pending" && r.RequestDate < now - EscalationAge)
                .ToListAsync(cancellationToken);

            foreach (var request in staleRequests)
            {
                request.EscalationLevel = (request.EscalationLevel ?? 0) + 1;
            }

            if (staleRequests.Count > 0)
                _logger.LogInformation("Escalated {Count} stale schedule change request(s)", staleRequests.Count);

            if (overdueSchedules.Count > 0 || staleRequests.Count > 0)
                await context.SaveChangesAsync(cancellationToken);
        }
    }
}
