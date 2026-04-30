using Microsoft.AspNetCore.Mvc;
using SmartBank.MVC.Services;
using SmartBank.MVC.Helpers;
using SmartBank.MVC.ViewModels;

namespace SmartBank.MVC.Controllers
{
    public class PassbookController : Controller
    {
        private readonly IApiService _apiService;
        private readonly ILogger<PassbookController> _logger;

        public PassbookController(IApiService apiService, ILogger<PassbookController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        // GET: /Passbook/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                return RedirectToAction("Login", "Auth");

            try
            {
                var token = SessionHelper.GetToken(HttpContext.Session);
                var result = await _apiService.GetAsync<ApiResponseWrapper<List<AccountSummaryViewModel>>>(
                    "api/accounts", token);

                var accounts = (result?.Data ?? new List<AccountSummaryViewModel>())
                    .Select(account => new AccountForPassbookDto
                    {
                        AccountId = account.AccountId,
                        AccountNumber = account.AccountNumber,
                        AccountType = account.AccountType,
                        Balance = account.Balance,
                        Status = account.Status
                    })
                    .ToList();

                ViewData["Accounts"] = accounts;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading passbook page");
                TempData["Error"] = "Error loading accounts.";
                return RedirectToAction("Index", "Dashboard");
            }
        }

        // GET: /Passbook/View/{accountId}
        [HttpGet("Passbook/View/{accountId:int}")]
        public async Task<IActionResult> View(int accountId, int numberOfTransactions = 10)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                return RedirectToAction("Login", "Auth");

            try
            {
                if (numberOfTransactions < 1 || numberOfTransactions > 100)
                    numberOfTransactions = 10;

                var token = SessionHelper.GetToken(HttpContext.Session);
                var result = await _apiService.GetAsync<ApiResponseWrapper<PassbookViewModel>>(
                    $"api/transactions/passbook/{accountId}?numberOfTransactions={numberOfTransactions}", token);
                
                ViewData["NumberOfTransactions"] = numberOfTransactions;
                return View(result?.Data ?? new PassbookViewModel());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading passbook for account {AccountId}", accountId);
                TempData["Error"] = "Error loading passbook data.";
                return RedirectToAction("Index");
            }
        }

        // GET: /Passbook/Print/{accountId}
        [HttpGet("Passbook/Print/{accountId:int}")]
        public async Task<IActionResult> Print(int accountId, int numberOfTransactions = 10)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                return RedirectToAction("Login", "Auth");

            try
            {
                if (numberOfTransactions < 1 || numberOfTransactions > 100)
                    numberOfTransactions = 10;

                var token = SessionHelper.GetToken(HttpContext.Session);
                var result = await _apiService.GetAsync<ApiResponseWrapper<PassbookViewModel>>(
                    $"api/transactions/passbook/{accountId}?numberOfTransactions={numberOfTransactions}", token);
                
                ViewData["NumberOfTransactions"] = numberOfTransactions;
                return View(result?.Data ?? new PassbookViewModel());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading print passbook for account {AccountId}", accountId);
                TempData["Error"] = "Error loading passbook data for printing.";
                return RedirectToAction("View", new { accountId, numberOfTransactions });
            }
        }
    }
}
