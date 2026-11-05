using Auditor.ViewModels.Dashboard;

namespace Auditor.Services.Interfaces
{
    public interface IDashboardService
    {
        /// <param name="auditorId">When set, figures are limited to this auditor's own audits.</param>
        /// <param name="scopeAirportId">When set, the user is limited to this airport.</param>
        Task<DashboardViewModel> GetDashboardAsync(DashboardFilter filter, long? auditorId, long? scopeAirportId = null);
    }
}
