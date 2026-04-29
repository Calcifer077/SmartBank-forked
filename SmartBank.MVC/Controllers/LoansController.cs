using Microsoft.AspNetCore.Mvc;
using SmartBank.MVC.Helpers;
using SmartBank.MVC.Services;
using SmartBank.MVC.ViewModels;

namespace SmartBank.MVC.Controllers
{
    public class LoansController : Controller
    {
        private readonly IApiService _api;

        public LoansController(IApiService api)
        {
            _api = api;
        }

        // GET /Loans
        public async Task<IActionResult> Index()
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                return RedirectToAction("Login", "Auth");

            var token = SessionHelper.GetToken(HttpContext.Session);
            var result = await _api.GetAsync<ApiResponseWrapper<List<LoanItemViewModel>>>(
                "api/loans/my", token);

            var vm = new LoanListViewModel
            {
                Loans = result?.Data ?? new List<LoanItemViewModel>()
            };

            return View(vm);
        }

        // GET /Loans/Apply
        public IActionResult Apply()
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                return RedirectToAction("Login", "Auth");

            return View(new LoanApplyViewModel());
        }

        // POST /Loans/Apply
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(LoanApplyViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var token = SessionHelper.GetToken(HttpContext.Session);
            var result = await _api.PostAsync<ApiResponseWrapper<LoanDetailViewModel>>(
                "api/loans/apply",
                new { model.Amount, model.TermMonths, model.Purpose },
                token);

            if (result is null || !result.Success)
            {
                ModelState.AddModelError("", result?.Message ?? "Loan application failed.");
                return View(model);
            }

            TempData["Success"] = "Loan application submitted successfully!";
            return RedirectToAction("Index");
        }

        // GET /Loans/Detail/{id}
        public async Task<IActionResult> Detail(int id)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                return RedirectToAction("Login", "Auth");

            var token = SessionHelper.GetToken(HttpContext.Session);
            var result = await _api.GetAsync<ApiResponseWrapper<LoanDetailViewModel>>(
                $"api/loans/{id}", token);

            if (result is null || !result.Success || result.Data is null)
            {
                TempData["Error"] = "Loan not found.";
                return RedirectToAction("Index");
            }

            return View(result.Data);
        }
    }
}