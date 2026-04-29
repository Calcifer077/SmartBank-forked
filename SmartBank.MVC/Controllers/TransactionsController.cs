using Microsoft.AspNetCore.Mvc;
using SmartBank.MVC.Helpers;
using SmartBank.MVC.Services;
using SmartBank.MVC.ViewModels;

namespace SmartBank.MVC.Controllers
{
    public class TransactionsController : Controller
    {
        private readonly IApiService _api;

        public TransactionsController(IApiService api)
        {
            _api = api;
        }

        // ── Deposit ───────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> Deposit(int accountId)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                return RedirectToAction("Login", "Auth");

            var vm = await BuildDepositWithdrawVm(accountId);
            if (vm == null)
            {
                TempData["Error"] = "Account not found.";
                return RedirectToAction("Index", "Accounts");
            }
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deposit(DepositWithdrawViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var token = SessionHelper.GetToken(HttpContext.Session);
            var result = await _api.PostAsync<ApiResponseWrapper<object>>(
                "api/transactions/deposit",
                new { model.AccountId, model.Amount, model.Description },
                token);

            if (result is null || !result.Success)
            {
                ModelState.AddModelError("", result?.Message ?? "Deposit failed.");
                return View(model);
            }

            TempData["Success"] = $"₹{model.Amount:N2} deposited successfully!";
            return RedirectToAction("History", new { accountId = model.AccountId });
        }

        // ── Withdraw ──────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> Withdraw(int accountId)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                return RedirectToAction("Login", "Auth");

            var vm = await BuildDepositWithdrawVm(accountId);
            if (vm == null)
            {
                TempData["Error"] = "Account not found.";
                return RedirectToAction("Index", "Accounts");
            }
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Withdraw(DepositWithdrawViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var token = SessionHelper.GetToken(HttpContext.Session);
            var result = await _api.PostAsync<ApiResponseWrapper<object>>(
                "api/transactions/withdraw",
                new { model.AccountId, model.Amount, model.Description },
                token);

            if (result is null || !result.Success)
            {
                ModelState.AddModelError("", result?.Message ?? "Withdrawal failed.");
                return View(model);
            }

            TempData["Success"] = $"₹{model.Amount:N2} withdrawn successfully!";
            return RedirectToAction("History", new { accountId = model.AccountId });
        }

        // ── Transfer ──────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> Transfer(int accountId)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                return RedirectToAction("Login", "Auth");

            var token = SessionHelper.GetToken(HttpContext.Session);

            // Load user accounts for dropdown
            var accountsResult = await _api.GetAsync<ApiResponseWrapper<List<AccountSummaryViewModel>>>(
                "api/accounts", token);

            var accounts = accountsResult?.Data ?? new List<AccountSummaryViewModel>();
            var selected = accounts.FirstOrDefault(a => a.AccountId == accountId);

            var vm = new TransferViewModel
            {
                FromAccountId = accountId,
                FromAccountNumber = selected?.AccountNumber ?? string.Empty,
                CurrentBalance = selected?.Balance ?? 0,
                UserAccounts = accounts
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Transfer(TransferViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var token = SessionHelper.GetToken(HttpContext.Session);
            var result = await _api.PostAsync<ApiResponseWrapper<object>>(
                "api/transactions/transfer",
                new
                {
                    model.FromAccountId,
                    model.ToAccountNumberString,
                    model.Amount,
                    model.Description
                },
                token);

            if (result is null || !result.Success)
            {
                ModelState.AddModelError("", result?.Message ?? "Transfer failed.");
                return View(model);
            }

            TempData["Success"] = $"₹{model.Amount:N2} transferred successfully!";
            return RedirectToAction("History", new { accountId = model.FromAccountId });
        }

        // ── History ───────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> History(int accountId)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                return RedirectToAction("Login", "Auth");

            var token = SessionHelper.GetToken(HttpContext.Session);

            // Get account detail for header
            var accountResult = await _api.GetAsync<ApiResponseWrapper<AccountDetailViewModel>>(
                $"api/accounts/{accountId}", token);

            // Get transaction history
            var historyResult = await _api.GetAsync<ApiResponseWrapper<List<TransactionItemViewModel>>>(
                $"api/transactions/history/{accountId}", token);

            if (accountResult is null || !accountResult.Success || accountResult.Data is null)
            {
                TempData["Error"] = "Account not found.";
                return RedirectToAction("Index", "Accounts");
            }

            var vm = new TransactionHistoryViewModel
            {
                AccountId = accountId,
                AccountNumber = accountResult.Data.AccountNumber,
                AccountType = accountResult.Data.AccountType,
                CurrentBalance = accountResult.Data.Balance,
                Transactions = historyResult?.Data ?? new List<TransactionItemViewModel>()
            };

            return View(vm);
        }

        // ── Private Helper ────────────────────────────────────────────
        private async Task<DepositWithdrawViewModel?> BuildDepositWithdrawVm(int accountId)
        {
            var token = SessionHelper.GetToken(HttpContext.Session);
            var result = await _api.GetAsync<ApiResponseWrapper<AccountDetailViewModel>>(
                $"api/accounts/{accountId}", token);

            if (result is null || !result.Success || result.Data is null)
                return null;

            return new DepositWithdrawViewModel
            {
                AccountId = accountId,
                AccountNumber = result.Data.AccountNumber,
                AccountType = result.Data.AccountType,
                CurrentBalance = result.Data.Balance
            };
        }
    }
}