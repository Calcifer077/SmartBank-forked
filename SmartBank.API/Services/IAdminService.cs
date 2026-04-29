using SmartBank.API.DTOs.Admin;

namespace SmartBank.API.Services
{
    public interface IAdminService
    {
        Task<AdminDashboardDto> GetDashboardStatsAsync();
        Task<List<AdminUserDto>> GetAllUsersAsync();
        Task<string> FreezeAccountAsync(FreezeAccountDto dto);
    }
}