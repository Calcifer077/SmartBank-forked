using Microsoft.AspNetCore.Mvc;
using SmartBank.MVC.Helpers;
using SmartBank.MVC.Services;
using SmartBank.MVC.ViewModels;

namespace SmartBank.MVC.Controllers
{
    public class AuthController : Controller
    {
        private readonly IApiService _api;

        public AuthController(IApiService api)
        {
            _api = api;
        }

        // ── Register ──────────────────────────────────────────────────

        [HttpGet]
        public IActionResult Register()
        {
            if (SessionHelper.IsLoggedIn(HttpContext.Session))
                return RedirectToAction("Index", "Dashboard");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            Console.WriteLine(_api);
            var result = await _api.PostAsync<ApiResponseWrapper<AuthDataDto>>(
                "api/auth/register", new
                {
                    model.FullName,
                    model.Email,
                    model.Password,
                    model.PhoneNumber,
                    model.Address
                });

            if (result is null || !result.Success || result.Data is null)
            {
                ModelState.AddModelError("", result?.Message ?? "Registration failed. Try again.");
                return View(model);
            }

            // Store session and go straight to dashboard
            SessionHelper.SetUserSession(HttpContext.Session,
                result.Data.Token,
                result.Data.FullName,
                result.Data.Email,
                result.Data.Role,
                result.Data.UserId);

            TempData["Success"] = $"Welcome, {result.Data.FullName}! Account created.";
            return RedirectToAction("Index", "Dashboard");
        }

        // ── Login ─────────────────────────────────────────────────────

        [HttpGet]
        public IActionResult Login()
        {
            if (SessionHelper.IsLoggedIn(HttpContext.Session))
                return RedirectToAction("Index", "Dashboard");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            Console.WriteLine("commiting to api");

            var result = await _api.PostAsync<ApiResponseWrapper<AuthDataDto>>(
                "api/auth/login", new
                {
                    model.Email,
                    model.Password
                });

            if (result is null || !result.Success || result.Data is null)
            {
                ModelState.AddModelError("", result?.Message ?? "Invalid credentials.");
                return View(model);
            }

            SessionHelper.SetUserSession(HttpContext.Session,
                result.Data.Token,
                result.Data.FullName,
                result.Data.Email,
                result.Data.Role,
                result.Data.UserId);

            TempData["Success"] = $"Welcome back, {result.Data.FullName}!";
            return RedirectToAction("Index", "Dashboard");
        }

        // ── Logout ────────────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            SessionHelper.ClearSession(HttpContext.Session);
            return RedirectToAction("Login");
        }
    }

    // Local DTO — matches AuthResponseDto from API
    public class AuthDataDto
    {
        public string Token { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int UserId { get; set; }
    }
}