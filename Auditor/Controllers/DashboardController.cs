using System.Security.Claims;
using Auditor.Security;
using Auditor.Services.Interfaces;
using Auditor.ViewModels.Dashboard;
using Microsoft.AspNetCore.Mvc;

namespace Auditor.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;
        private readonly UserScope _scope;

        public DashboardController(IDashboardService dashboardService, UserScope scope)
        {
            _dashboardService = dashboardService;
            _scope = scope;
        }

        // GET: /  and  /Dashboard?days=90&airportId=5
        [HttpGet("/")]
        [HttpGet("/Dashboard")]
        [HttpGet("/Dashboard/Index")]
        public async Task<IActionResult> Index([FromQuery] DashboardFilter filter)
        {
            long? auditorId = null;
            if (User.IsInRole(Roles.Auditor) && long.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id))
                auditorId = id;

            var vm = await _dashboardService.GetDashboardAsync(filter, auditorId, await _scope.AirportIdAsync());
            return View(vm);
        }
    }
}
