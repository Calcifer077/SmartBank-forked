using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartBank.API.Helpers;
using SmartBank.Data;
using System.Security.Claims;

namespace SmartBank.API.Controllers
{
    [ApiController]
    [Route("api/profile")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly SmartBankDbContext _db;

        public ProfileController(SmartBankDbContext db)
        {
            _db = db;
        }

        // GET /api/profile
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized(ApiResponse<object>.Fail("Invalid token."));

            var user = await _db.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user is null)
                return NotFound(ApiResponse<object>.Fail("User not found."));

            return Ok(ApiResponse<object>.Ok(new
            {
                user.UserId,
                user.FullName,
                user.Email,
                user.PhoneNumber,
                user.Address,
                user.KycStatus,
                Role = user.Role.RoleName,
                user.CreatedAt
            }));
        }
    }
}