using System.Security.Claims;
using Auditor.Models;
using Microsoft.EntityFrameworkCore;

namespace Auditor.Security
{
    /// <summary>
    /// The signed-in user's airport scope. A user linked to an airport only sees and works with that airport;
    /// a user with no airport sees all of them. Read from the database once per request, so a change an
    /// administrator makes applies on the user's next click.
    /// </summary>
    public class UserScope
    {
        private readonly AuditorContext _context;
        private readonly IHttpContextAccessor _http;
        private bool _loaded;
        private long? _airportId;

        public UserScope(AuditorContext context, IHttpContextAccessor http)
        {
            _context = context;
            _http = http;
        }

        public long? UserId =>
            long.TryParse(_http.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

        /// <summary>The user's airport, or null for all airports.</summary>
        public async Task<long?> AirportIdAsync()
        {
            if (!_loaded)
            {
                var userId = UserId;
                _airportId = userId == null
                    ? null
                    : await _context.AppUsers.Where(u => u.Id == userId).Select(u => u.AirportId).FirstOrDefaultAsync();
                _loaded = true;
            }
            return _airportId;
        }

        public async Task<bool> CanAccessAirportAsync(long airportId)
        {
            var scope = await AirportIdAsync();
            return scope == null || scope == airportId;
        }

        /// <summary>Applies the scope to an airport filter chosen in the UI: a scoped user always gets their own airport.</summary>
        public async Task<long?> NarrowAsync(long? requestedAirportId)
        {
            var scope = await AirportIdAsync();
            return scope ?? requestedAirportId;
        }
    }
}
