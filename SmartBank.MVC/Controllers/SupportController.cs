using Microsoft.AspNetCore.Mvc;
using SmartBank.MVC.Helpers;
using SmartBank.MVC.Services;
using SmartBank.MVC.ViewModels;

namespace SmartBank.MVC.Controllers
{
    public class SupportController : Controller
    {
        private readonly IApiService _api;

        public SupportController(IApiService api)
        {
            _api = api;
        }

        // GET /Support
        public async Task<IActionResult> Index()
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                return RedirectToAction("Login", "Auth");

            var token = SessionHelper.GetToken(HttpContext.Session);
            var result = await _api.GetAsync<ApiResponseWrapper<List<TicketItemViewModel>>>(
                "api/tickets/my", token);

            var vm = new TicketListViewModel
            {
                Tickets = result?.Data ?? new List<TicketItemViewModel>()
            };

            return View(vm);
        }

        // GET /Support/Create
        public IActionResult Create()
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                return RedirectToAction("Login", "Auth");

            return View(new TicketCreateViewModel());
        }

        // POST /Support/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TicketCreateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var token = SessionHelper.GetToken(HttpContext.Session);
            var result = await _api.PostAsync<ApiResponseWrapper<object>>(
                "api/tickets/create",
                new { model.Subject, model.Description },
                token);

            if (result is null || !result.Success)
            {
                ModelState.AddModelError("", result?.Message ?? "Failed to raise ticket.");
                return View(model);
            }

            TempData["Success"] = "Support ticket raised successfully!";
            return RedirectToAction("Index");
        }
    }
}