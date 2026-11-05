using Auditor.Models;
using Auditor.Security;
using Auditor.ViewModels.Shared;
using Microsoft.EntityFrameworkCore;

namespace Auditor.Services
{
    /// <summary>Builds the option lists for the audit filter pop-up, limited to the user's airport scope.</summary>
    public class AuditFilterOptions
    {
        private readonly AuditorContext _context;
        private readonly UserScope _scope;

        public AuditFilterOptions(AuditorContext context, UserScope scope)
        {
            _context = context;
            _scope = scope;
        }

        public async Task<AuditFilterPanel> BuildAsync(AuditFilter filter, string controller, IEnumerable<(string Value, string Label)> statuses, bool showAuditor)
        {
            var scopeAirport = await _scope.AirportIdAsync();

            return new AuditFilterPanel
            {
                Filter = filter,
                Controller = controller,
                ShowAuditor = showAuditor,
                AirportLocked = scopeAirport != null,
                Statuses = statuses.ToList(),
                Airports = (await _context.Airports
                        .Where(a => scopeAirport == null || a.Id == scopeAirport)
                        .OrderBy(a => a.Code)
                        .Select(a => new { a.Id, a.Code, a.Name })
                        .ToListAsync())
                    .Select(a => (a.Id, a.Code, a.Name)).ToList(),
                Sites = await _context.AuditSites
                    .Where(s => scopeAirport == null || s.AirportId == scopeAirport)
                    .OrderBy(s => s.Name)
                    .Select(s => new FilterSite(s.Id, s.Name, s.AirportId, s.SiteSubCategory, s.SiteType))
                    .ToListAsync(),
                Templates = (await _context.AuditTemplates
                        .Where(t => scopeAirport == null || t.AirportId == null || t.AirportId == scopeAirport)
                        .OrderBy(t => t.Name)
                        .Select(t => new { t.Id, t.Name })
                        .ToListAsync())
                    .Select(t => (t.Id, t.Name)).ToList(),
                Auditors = showAuditor
                    ? (await _context.AppUsers
                            .Where(u => u.Role == Roles.Auditor && (scopeAirport == null || u.AirportId == null || u.AirportId == scopeAirport))
                            .OrderBy(u => u.DisplayName)
                            .Select(u => new { u.Id, u.DisplayName })
                            .ToListAsync())
                        .Select(u => (u.Id, u.DisplayName)).ToList()
                    : new(),
            };
        }
    }
}
