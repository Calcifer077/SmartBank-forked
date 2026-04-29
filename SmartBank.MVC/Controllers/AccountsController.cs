using Microsoft.AspNetCore.Mvc;
using SmartBank.MVC.Helpers;
using SmartBank.MVC.Services;
using SmartBank.MVC.ViewModels;

namespace SmartBank.MVC.Controllers
{
    public class AccountsController : Controller
    {
        private readonly IApiService _api;

        public AccountsController(IApiService api)
        {
            _api = api;
        }

        // GET /Accounts
        public async Task<IActionResult> Index()
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                return RedirectToAction("Login", "Auth");

            var token = SessionHelper.GetToken(HttpContext.Session);

            var result = await _api.GetAsync<ApiResponseWrapper<List<AccountSummaryViewModel>>>(
                "api/accounts", token);

            var vm = new AccountListViewModel
            {
                Accounts = result?.Data ?? new List<AccountSummaryViewModel>()
            };

            return View(vm);
        }

        // GET /Accounts/Create
        public IActionResult Create()
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                return RedirectToAction("Login", "Auth");

            return View(new AccountCreateViewModel());
        }

        // POST /Accounts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AccountCreateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var token = SessionHelper.GetToken(HttpContext.Session);

            var result = await _api.PostAsync<ApiResponseWrapper<AccountDetailViewModel>>(
                "api/accounts/create",
                new { model.AccountType },
                token);

            if (result is null || !result.Success)
            {
                ModelState.AddModelError("", result?.Message ?? "Failed to open account.");
                return View(model);
            }

            TempData["Success"] = $"{model.AccountType} account opened successfully!";
            return RedirectToAction("Index");
        }

        // GET /Accounts/Detail/{id}
        public async Task<IActionResult> Detail(int id)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                return RedirectToAction("Login", "Auth");

            var token = SessionHelper.GetToken(HttpContext.Session);

            var result = await _api.GetAsync<ApiResponseWrapper<AccountDetailViewModel>>(
                $"api/accounts/{id}", token);

            if (result is null || !result.Success || result.Data is null)
            {
                TempData["Error"] = "Account not found.";
                return RedirectToAction("Index");
            }

            return View(result.Data);
        }
    }
}