using Auditor.DTO.AppUsers;
using Auditor.Models;
using Auditor.Services.Interfaces;

namespace Auditor.Services
{
    public class AppUserService : IAppUserService
    {
        private readonly AuditorContext _context;
        private readonly ILogger<AppUserService> _logger;

        public AppUserService(AuditorContext context,
                            ILogger<AppUserService> logger)
        {
            _context = context;
        }
        public async Task<List<AppUserInfoDTO>> GetAll()
        {
            return _context.AppUsers.Select(user=> new AppUserInfoDTO(user.Id, user.DisplayName)).ToList();
        }
    }
}
