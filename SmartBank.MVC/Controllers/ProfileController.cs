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

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                return RedirectToAction("Login", "Auth");

            var token = SessionHelper.GetToken(HttpContext.Session);
            var result = await _api.GetAsync<ApiResponseWrapper<ProfileViewModel>>(
                "api/profile", token);

            if (result is null || !result.Success || result.Data is null)
            {
                TempData["Error"] = "Could not load profile.";
                return RedirectToAction("Index");
            }

            return View(result.Data);
        }

        [HttpPost]
        public async Task<IActionResult> Update(string fullName, string phoneNumber, string address)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                return RedirectToAction("Login", "Auth");

            var token = SessionHelper.GetToken(HttpContext.Session);
            var updateDto = new { FullName = fullName, PhoneNumber = phoneNumber, Address = address };
            
            var result = await _api.PutAsync<ApiResponseWrapper<object>>(
                "api/profile/update", updateDto, token);

            if (result is null || !result.Success)
            {
                TempData["Error"] = result?.Message ?? "Failed to update profile.";
                return RedirectToAction("Edit");
            }

            TempData["Success"] = "Profile updated successfully!";
            return RedirectToAction("Index");
        }
    }
}