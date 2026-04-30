using Microsoft.AspNetCore.Mvc;
using SmartBank.MVC.Helpers;
using SmartBank.MVC.Services;
using SmartBank.MVC.ViewModels;

namespace SmartBank.MVC.Controllers
{
    public class AdminController : Controller
    {
        private readonly IApiService _api;

        public AdminController(IApiService api)
        {
            _api = api;
        }

        // ── Guard helper ──────────────────────────────────────────────
        private bool IsAdmin() =>
            SessionHelper.IsLoggedIn(HttpContext.Session) &&
            SessionHelper.GetUserRole(HttpContext.Session) == "Admin";

        // GET /Admin
        public async Task<IActionResult> Index()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            var token = SessionHelper.GetToken(HttpContext.Session);
            var result = await _api.GetAsync<ApiResponseWrapper<AdminDashboardViewModel>>(
                "api/admin/dashboard", token);

            return View(result?.Data ?? new AdminDashboardViewModel());
        }

        // GET /Admin/Users
        public async Task<IActionResult> Users()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            var token = SessionHelper.GetToken(HttpContext.Session);
            var result = await _api.GetAsync<ApiResponseWrapper<List<AdminUserItemViewModel>>>(
                "api/admin/users", token);

            return View(new AdminUserListViewModel
            {
                Users = result?.Data ?? new List<AdminUserItemViewModel>()
            });
        }

        // GET /Admin/Accounts
        public async Task<IActionResult> Accounts()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            var token = SessionHelper.GetToken(HttpContext.Session);
            var result = await _api.GetAsync<ApiResponseWrapper<List<AdminAccountItemViewModel>>>(
                "api/admin/accounts", token);

            return View(new AdminAccountListViewModel
            {
                Accounts = result?.Data ?? new List<AdminAccountItemViewModel>()
            });
        }

        // POST /Admin/FreezeAccount
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FreezeAccount(int accountId, string action, string? returnTo = null)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            var token = SessionHelper.GetToken(HttpContext.Session);
            var result = await _api.PostAsync<ApiResponseWrapper<object>>(
                "api/admin/freeze",
                new { AccountId = accountId, Action = action },
                token);

            TempData[result?.Success == true ? "Success" : "Error"] =
                result?.Message ?? "Action failed.";

            // Redirect back to the page where the action was triggered from
            return RedirectToAction(returnTo == "Users" ? "Users" : "Accounts");
        }

        // POST /Admin/ChangeRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeRole(int userId, int roleId)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            var token = SessionHelper.GetToken(HttpContext.Session);
            var result = await _api.PostAsync<ApiResponseWrapper<object>>(
                "api/admin/change-role",
                new { UserId = userId, RoleId = roleId },
                token);

            TempData[result?.Success == true ? "Success" : "Error"] =
                result?.Message ?? "Action failed.";

            return RedirectToAction("Users");
        }

        // GET /Admin/Loans
        public async Task<IActionResult> Loans()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            var token = SessionHelper.GetToken(HttpContext.Session);
            var result = await _api.GetAsync<ApiResponseWrapper<List<AdminLoanItemViewModel>>>(
                "api/loans/all", token);

            return View(new AdminLoanListViewModel
            {
                Loans = result?.Data ?? new List<AdminLoanItemViewModel>()
            });
        }

        // POST /Admin/ReviewLoan
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReviewLoan(int loanId, string decision)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            var token = SessionHelper.GetToken(HttpContext.Session);
            var result = await _api.PostAsync<ApiResponseWrapper<object>>(
                "api/loans/review",
                new { LoanId = loanId, Decision = decision },
                token);

            TempData[result?.Success == true ? "Success" : "Error"] =
                result?.Message ?? "Action failed.";

            return RedirectToAction("Loans");
        }

        // GET /Admin/Tickets
        public async Task<IActionResult> Tickets()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            var token = SessionHelper.GetToken(HttpContext.Session);
            var result = await _api.GetAsync<ApiResponseWrapper<List<AdminTicketItemViewModel>>>(
                "api/tickets/all", token);

            return View(new AdminTicketListViewModel
            {
                Tickets = result?.Data ?? new List<AdminTicketItemViewModel>()
            });
        }

        // POST /Admin/ResolveTicket
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResolveTicket(int ticketId)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            var token = SessionHelper.GetToken(HttpContext.Session);
            var result = await _api.PostAsync<ApiResponseWrapper<object>>(
                $"api/tickets/resolve/{ticketId}", new { }, token);

            TempData[result?.Success == true ? "Success" : "Error"] =
                result?.Message ?? "Action failed.";

            return RedirectToAction("Tickets");
        }
    }
}