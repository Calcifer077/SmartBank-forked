using Microsoft.AspNetCore.Mvc;
using SmartBank.MVC.Helpers;
using SmartBank.MVC.Services;
using SmartBank.MVC.ViewModels;

namespace SmartBank.MVC.Controllers
{
    public class ReportsController : Controller
    {
        private readonly IApiService _api;

        public ReportsController(IApiService api)
        {
            _api = api;
        }

        private bool IsAdminOrAuditor()
        {
            var role = SessionHelper.GetUserRole(HttpContext.Session);
            return SessionHelper.IsLoggedIn(HttpContext.Session) &&
                   (role == "Admin" || role == "Auditor");
        }

        // GET /Reports
        public IActionResult Index()
        {
            if (!IsAdminOrAuditor()) return RedirectToAction("Login", "Auth");
            return View();
        }

        // GET /Reports/DailyTransactions
        public async Task<IActionResult> DailyTransactions(DateTime? date)
        {
            if (!IsAdminOrAuditor()) return RedirectToAction("Login", "Auth");

            var reportDate = date ?? DateTime.Today;
            var token = SessionHelper.GetToken(HttpContext.Session);

            var result = await _api.GetAsync<ApiResponseWrapper<List<DailyTransactionItemViewModel>>>(
                $"api/reports/daily?date={reportDate:yyyy-MM-dd}", token);

            return View(new DailyTransactionReportViewModel
            {
                ReportDate = reportDate,
                Transactions = result?.Data ?? new List<DailyTransactionItemViewModel>()
            });
        }

        // GET /Reports/LoanReport
        public async Task<IActionResult> LoanReport()
        {
            if (!IsAdminOrAuditor()) return RedirectToAction("Login", "Auth");

            var token = SessionHelper.GetToken(HttpContext.Session);
            var result = await _api.GetAsync<ApiResponseWrapper<List<LoanReportItemViewModel>>>(
                "api/reports/loans", token);

            return View(new LoanReportViewModel
            {
                Loans = result?.Data ?? new List<LoanReportItemViewModel>()
            });
        }

        // GET /Reports/ActiveCustomers
        public async Task<IActionResult> ActiveCustomers()
        {
            if (!IsAdminOrAuditor()) return RedirectToAction("Login", "Auth");

            var token = SessionHelper.GetToken(HttpContext.Session);
            var result = await _api.GetAsync<ApiResponseWrapper<List<ActiveCustomerItemViewModel>>>(
                "api/reports/active-customers", token);

            return View(new ActiveCustomersReportViewModel
            {
                Customers = result?.Data ?? new List<ActiveCustomerItemViewModel>()
            });
        }

        // GET /Reports/LowBalance
        public async Task<IActionResult> LowBalance(decimal threshold = 1000)
        {
            if (!IsAdminOrAuditor()) return RedirectToAction("Login", "Auth");

            var token = SessionHelper.GetToken(HttpContext.Session);
            var result = await _api.GetAsync<ApiResponseWrapper<List<LowBalanceItemViewModel>>>(
                $"api/reports/low-balance?threshold={threshold}", token);

            return View(new LowBalanceReportViewModel
            {
                Threshold = threshold,
                Accounts = result?.Data ?? new List<LowBalanceItemViewModel>()
            });
        }
    }
}