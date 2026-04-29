using Microsoft.EntityFrameworkCore;
using SmartBank.API.DTOs.Reports;
using SmartBank.Data;

namespace SmartBank.API.Services
{
    public class ReportService : IReportService
    {
        private readonly SmartBankDbContext _db;

        public ReportService(SmartBankDbContext db)
        {
            _db = db;
        }

        public async Task<List<DailyTransactionReportDto>> GetDailyTransactionsAsync(DateTime date)
        {
            var targetDate = date.Date;

            return await _db.Transactions
                .Include(t => t.Account)
                    .ThenInclude(a => a.User)
                .Where(t => t.CreatedAt.Date == targetDate)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new DailyTransactionReportDto
                {
                    TransactionId = t.TransactionId,
                    AccountNumber = t.Account.AccountNumber,
                    CustomerName = t.Account.User.FullName,
                    Type = t.Type,
                    Amount = t.Amount,
                    BalanceAfter = t.BalanceAfter,
                    Description = t.Description,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<List<LoanReportDto>> GetLoanReportAsync()
        {
            return await _db.Loans
                .Include(l => l.User)
                .OrderByDescending(l => l.AppliedAt)
                .Select(l => new LoanReportDto
                {
                    LoanId = l.LoanId,
                    CustomerName = l.User.FullName,
                    CustomerEmail = l.User.Email,
                    Amount = l.Amount,
                    TermMonths = l.TermMonths,
                    Purpose = l.Purpose,
                    Status = l.Status,
                    AppliedAt = l.AppliedAt,
                    ReviewedAt = l.ReviewedAt
                })
                .ToListAsync();
        }

        public async Task<List<ActiveCustomersReportDto>> GetActiveCustomersAsync()
        {
            return await _db.Users
                .Include(u => u.Accounts)
                .Where(u => u.IsActive)
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new ActiveCustomersReportDto
                {
                    UserId = u.UserId,
                    FullName = u.FullName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    KycStatus = u.KycStatus,
                    AccountCount = u.Accounts.Count,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<List<LowBalanceReportDto>> GetLowBalanceAccountsAsync(decimal threshold)
        {
            return await _db.Accounts
                .Include(a => a.User)
                .Where(a => a.Balance < threshold && a.Status == "Active")
                .OrderBy(a => a.Balance)
                .Select(a => new LowBalanceReportDto
                {
                    AccountId = a.AccountId,
                    AccountNumber = a.AccountNumber,
                    AccountType = a.AccountType,
                    Balance = a.Balance,
                    OwnerName = a.User.FullName,
                    OwnerEmail = a.User.Email,
                    Status = a.Status
                })
                .ToListAsync();
        }
    }
}