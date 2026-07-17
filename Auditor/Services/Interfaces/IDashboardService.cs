using Auditor.ViewModels.Dashboard;

namespace Auditor.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardViewModel> GetDashboardStatsAsync();
    }
}
