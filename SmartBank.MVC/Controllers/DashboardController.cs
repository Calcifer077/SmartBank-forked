using Microsoft.AspNetCore.Mvc;
using SmartBank.MVC.Helpers;
using SmartBank.MVC.ViewModels;

namespace SmartBank.MVC.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                return RedirectToAction("Login", "Auth");

            var vm = new DashboardViewModel
            {
                FullName = SessionHelper.GetUserName(HttpContext.Session),
                Email = SessionHelper.GetUserEmail(HttpContext.Session),
                Role = SessionHelper.GetUserRole(HttpContext.Session),
                KycStatus = "Pending" // will come from API in sprint 2
            };

            return View(vm);
        }
    }
}