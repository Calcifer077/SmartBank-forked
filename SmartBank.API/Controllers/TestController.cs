using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBank.API.Helpers;
using System.Security.Claims;

namespace SmartBank.API.Controllers
{
    [ApiController]
    [Route("api/test")]
    public class TestController : ControllerBase
    {
        // GET /api/test/public
        [HttpGet("public")]
        public IActionResult Public()
        {
            return Ok(ApiResponse<string>.Ok("Public endpoint works."));
        }

        // GET /api/test/protected
        [Authorize]
        [HttpGet("protected")]
        public IActionResult Protected()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            return Ok(ApiResponse<object>.Ok(new
            {
                Message = "Protected endpoint reached.",
                Email = email,
                Role = role
            }));
        }

        // GET /api/test/admin-only
        [Authorize(Roles = "Admin")]
        [HttpGet("admin-only")]
        public IActionResult AdminOnly()
        {
            return Ok(ApiResponse<string>.Ok("Admin access confirmed."));
        }
    }
}