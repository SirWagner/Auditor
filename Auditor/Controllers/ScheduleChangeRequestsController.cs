using System.Security.Claims;
using Auditor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Auditor.Controllers
{
    public class ScheduleChangeRequestsController : Controller
    {
        private readonly AuditorContext _context;

        public ScheduleChangeRequestsController(AuditorContext context)
        {
            _context = context;
        }

        // GET: ScheduleChangeRequests?status=Pending
        public async Task<IActionResult> Index(string status = "Pending")
        {
            var query = _context.ScheduleChangeRequests
                .Include(r => r.Schedule)
                .Include(r => r.RequestedByNavigation)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status) && status != "All")
                query = query.Where(r => r.Status == status);

            ViewBag.CurrentStatus = status;

            var requests = await query
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();

            return View(requests);
        }

        // GET: ScheduleChangeRequests/Details/5
        public async Task<IActionResult> Details(long id)
        {
            var request = await _context.ScheduleChangeRequests
                .Include(r => r.Schedule)
                    .ThenInclude(s => s.Template)
                .Include(r => r.Schedule)
                    .ThenInclude(s => s.Site)
                .Include(r => r.RequestedByNavigation)
                .Include(r => r.Authorizer)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
                return NotFound();

            return View(request);
        }

        // POST: ScheduleChangeRequests/Authorize/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Authorize(long id, bool approve, string decisionNotes)
        {
            var request = await _context.ScheduleChangeRequests.FirstOrDefaultAsync(r => r.Id == id);
            if (request == null)
                return NotFound();

            var authorizerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            long.TryParse(authorizerIdClaim, out var authorizerId);

            request.Status = approve ? "Approved" : "Rejected";
            request.AuthorizerId = authorizerId > 0 ? authorizerId : null;
            request.DecisionDate = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(decisionNotes))
                request.Reason += $"\n\n[Decision notes] {decisionNotes}";

            await _context.SaveChangesAsync();

            TempData["Message"] = approve
                ? "Change request approved. The requester can now update the schedule via Edit."
                : "Change request rejected.";

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
