using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBank.API.DTOs.Admin;
using SmartBank.API.Helpers;
using SmartBank.API.Services;

namespace SmartBank.API.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        // GET /api/admin/dashboard
        [HttpGet("dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            var result = await _adminService.GetDashboardStatsAsync();
            return Ok(ApiResponse<AdminDashboardDto>.Ok(result));
        }

        // GET /api/admin/users
        [HttpGet("users")]
        public async Task<IActionResult> Users()
        {
            var result = await _adminService.GetAllUsersAsync();
            return Ok(ApiResponse<List<AdminUserDto>>.Ok(result));
        }

        // POST /api/admin/freeze
        [HttpPost("freeze")]
        public async Task<IActionResult> Freeze([FromBody] FreezeAccountDto dto)
        {
            var message = await _adminService.FreezeAccountAsync(dto);
            return Ok(ApiResponse<object>.Ok(null!, message));
        }
    }
}