using Auditor.Models;
using Auditor.ViewModels.Dashboard;
using Auditor.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Auditor.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly AuditorContext _context;

        public DashboardService(AuditorContext context)
        {
            _context = context;
        }

        public async Task<DashboardViewModel> GetDashboardStatsAsync()
        {
            var now = DateTime.UtcNow;

            var vm = new DashboardViewModel
            {
                TotalAuditors = await _context.AppUsers.CountAsync(u => u.Role == "Auditor"),
                ActiveAuditors = await _context.AppUsers.CountAsync(u => u.Role == "Auditor" && u.IsActive),

                TotalUsers = await _context.AppUsers.CountAsync(),
                AdminCount = await _context.AppUsers.CountAsync(u => u.Role == "Admin"),
                ViewerCount = await _context.AppUsers.CountAsync(u => u.Role == "Viewer"),

                TotalTemplates = await _context.AuditTemplates.CountAsync(),
                ActiveTemplates = await _context.AuditTemplates.CountAsync(t => t.IsActive),

                TotalSites = await _context.AuditSites.CountAsync(),
                ActiveSites = await _context.AuditSites.CountAsync(s => s.Status == "Active"),

                ScheduledAudits = await _context.AuditSchedules.CountAsync(s => s.Status == "SCHEDULED"),
                CancelledSchedules = await _context.AuditSchedules.CountAsync(s => s.Status == "CANCELLED"),
                // Schedules the monitoring background service hasn't caught yet (it polls periodically,
                // so this can be briefly non-zero even when everything is working as intended).
                OverdueSchedules = await _context.AuditSchedules
                    .CountAsync(s => s.Status == "SCHEDULED" && s.DueDate < now),

                ExecutionsPendingAcceptance = await _context.AuditExecutions.CountAsync(e => e.Status == "PENDING_ACCEPTANCE"),
                ExecutionsInProgress = await _context.AuditExecutions.CountAsync(e => e.Status == "IN_PROGRESS"),
                ExecutionsCompleted = await _context.AuditExecutions.CountAsync(e => e.Status == "COMPLETED"),
                ExecutionsClosed = await _context.AuditExecutions.CountAsync(e => e.Status == "CLOSED"),
                ExecutionsRejected = await _context.AuditExecutions.CountAsync(e => e.Status == "REJECTED"),

                PendingChangeRequests = await _context.ScheduleChangeRequests.CountAsync(r => r.Status == "Pending"),
            };

            vm.RecentExecutions = await _context.AuditExecutions
                .Include(e => e.Schedule)
                    .ThenInclude(s => s.Template)
                .Include(e => e.Schedule)
                    .ThenInclude(s => s.Site)
                .OrderByDescending(e => e.OriginalAuditDate)
                .Take(8)
                .Select(e => new RecentExecutionViewModel
                {
                    Id = e.Id,
                    TemplateName = e.Schedule.Template.Name,
                    SiteName = e.Schedule.Site.Name,
                    Status = e.Status,
                    Date = e.OriginalAuditDate
                })
                .ToListAsync();

            return vm;
        }
    }
}
