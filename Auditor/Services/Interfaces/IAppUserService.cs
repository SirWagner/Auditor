using Auditor.DTO.AppUsers;

namespace Auditor.Services.Interfaces
{
    public interface IAppUserService
    {
        public Task<List<AppUserInfoDTO>> GetAll();
    }
}