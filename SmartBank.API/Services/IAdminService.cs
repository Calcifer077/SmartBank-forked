using SmartBank.API.DTOs.Admin;

namespace SmartBank.API.Services
{
    public interface IAdminService
    {
        Task<AdminDashboardDto> GetDashboardStatsAsync();
        Task<List<AdminUserDto>> GetAllUsersAsync();
        Task<List<AdminAccountDto>> GetAllAccountsAsync();
        Task<List<RoleDto>> GetAllRolesAsync();
        Task<string> FreezeAccountAsync(FreezeAccountDto dto);
        Task<string> ChangeUserRoleAsync(ChangeRoleDto dto);
    }
}