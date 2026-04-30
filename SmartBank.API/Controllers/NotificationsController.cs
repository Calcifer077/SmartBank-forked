using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartBank.API.Helpers;
using SmartBank.Data;
using System.Security.Claims;

namespace SmartBank.API.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly SmartBankDbContext _db;

        public NotificationsController(SmartBankDbContext db)
        {
            _db = db;
        }

        // GET /api/notifications
        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized(ApiResponse<object>.Fail("Invalid token."));

            var notifications = await _db.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(10)
                .Select(n => new
                {
                    n.NotificationId,
                    n.Title,
                    n.Message,
                    n.CreatedAt,
                    n.IsRead
                })
                .ToListAsync();

            return Ok(ApiResponse<object>.Ok(new
            {
                Notifications = notifications,
                UnreadCount = notifications.Count(n => !n.IsRead)
            }));
        }

        // POST /api/notifications/{id}/read
        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized(ApiResponse<object>.Fail("Invalid token."));

            var notification = await _db.Notifications.FindAsync(id);
            if (notification is null || notification.UserId != userId)
                return NotFound(ApiResponse<object>.Fail("Notification not found."));

            notification.IsRead = true;
            await _db.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(null!, "Notification marked as read."));
        }

        // POST /api/notifications/read-all
        [HttpPost("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized(ApiResponse<object>.Fail("Invalid token."));

            var notifications = await _db.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
                notification.IsRead = true;

            await _db.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(null!, "All notifications marked as read."));
        }
    }
}
