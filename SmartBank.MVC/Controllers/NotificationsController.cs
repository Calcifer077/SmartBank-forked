using Microsoft.AspNetCore.Mvc;
using SmartBank.MVC.Helpers;
using SmartBank.MVC.Services;
using SmartBank.MVC.ViewModels;

namespace SmartBank.MVC.Controllers
{
    [Route("api/notifications")]
    public class NotificationsController : Controller
    {
        private readonly IApiService _api;
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(IApiService api, ILogger<NotificationsController> logger)
        {
            _api = api;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                return Unauthorized();

            var token = SessionHelper.GetToken(HttpContext.Session);
            var result = await _api.GetAsync<ApiResponseWrapper<NotificationListViewModel>>("api/notifications", token);
            return Ok(result);
        }

        [HttpPost("{id:int}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                return Unauthorized();

            var token = SessionHelper.GetToken(HttpContext.Session);
            var result = await _api.PostAsync<ApiResponseWrapper<object>>($"api/notifications/{id}/read", new { }, token);
            return Ok(result);
        }

        [HttpPost("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                return Unauthorized();

            var token = SessionHelper.GetToken(HttpContext.Session);
            var result = await _api.PostAsync<ApiResponseWrapper<object>>("api/notifications/read-all", new { }, token);
            return Ok(result);
        }
    }
}
