using Microsoft.AspNetCore.Mvc;
using SmartBank.MVC.Helpers;
using SmartBank.MVC.Services;
using SmartBank.MVC.ViewModels;

namespace SmartBank.MVC.Controllers
{
    public class ProfileController : Controller
    {
        private readonly IApiService _api;

        public ProfileController(IApiService api)
        {
            _api = api;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                return RedirectToAction("Login", "Auth");

            var token = SessionHelper.GetToken(HttpContext.Session);
            var result = await _api.GetAsync<ApiResponseWrapper<ProfileViewModel>>(
                "api/profile", token);

            if (result is null || !result.Success || result.Data is null)
            {
                TempData["Error"] = "Could not load profile.";
                return RedirectToAction("Index", "Dashboard");
            }

            return View(result.Data);
        }
    }
}