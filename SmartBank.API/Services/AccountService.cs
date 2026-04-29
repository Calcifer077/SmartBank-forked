using Microsoft.EntityFrameworkCore;
using SmartBank.API.DTOs.Accounts;
using SmartBank.API.Helpers;
using SmartBank.Data;
using SmartBank.Models.Entities;

namespace SmartBank.API.Services
{
    public class AccountService : IAccountService
    {
        private readonly SmartBankDbContext _db;
        private readonly ILogger<AccountService> _logger;

        public AccountService(SmartBankDbContext db, ILogger<AccountService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<AccountResponseDto> CreateAccountAsync(int userId, AccountCreateDto dto)
        {
            // Verify user exists
            var user = await _db.Users.FindAsync(userId)
                ?? throw new KeyNotFoundException("User not found.");

            // One account per type per user rule
            bool alreadyExists = await _db.Accounts
                .AnyAsync(a => a.UserId == userId && a.AccountType == dto.AccountType);

            if (alreadyExists)
                throw new InvalidOperationException(
                    $"You already have a {dto.AccountType} account.");

            // Generate unique account number
            string accountNumber;
            do
            {
                accountNumber = AccountNumberGenerator.Generate();
            }
            while (await _db.Accounts.AnyAsync(a => a.AccountNumber == accountNumber));

            var account = new Account
            {
                AccountNumber = accountNumber,
                AccountType = dto.AccountType,
                Balance = 0,
                Status = "Active",
                UserId = userId,
                OpenedAt = DateTime.UtcNow
            };

            _db.Accounts.Add(account);
            await _db.SaveChangesAsync();

            _logger.LogInformation(
                "Account created: {AccountNumber} for UserId: {UserId}", accountNumber, userId);

            return MapToResponse(account, user.FullName, user.Email);
        }

        public async Task<List<AccountSummaryDto>> GetUserAccountsAsync(int userId)
        {
            return await _db.Accounts
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.OpenedAt)
                .Select(a => new AccountSummaryDto
                {
                    AccountId = a.AccountId,
                    AccountNumber = a.AccountNumber,
                    AccountType = a.AccountType,
                    Balance = a.Balance,
                    Status = a.Status
                })
                .ToListAsync();
        }

        public async Task<AccountResponseDto> GetAccountDetailAsync(int accountId, int userId)
        {
            var account = await _db.Accounts
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.AccountId == accountId)
                ?? throw new KeyNotFoundException("Account not found.");

            // Ownership check — customer can only see their own account
            if (account.UserId != userId)
                throw new UnauthorizedAccessException("Access denied.");

            return MapToResponse(account, account.User.FullName, account.User.Email);
        }

        // ── Mapper ────────────────────────────────────────────────────
        private static AccountResponseDto MapToResponse(
            Account account, string ownerName, string ownerEmail) => new()
        {
            AccountId = account.AccountId,
            AccountNumber = account.AccountNumber,
            AccountType = account.AccountType,
            Balance = account.Balance,
            Status = account.Status,
            OpenedAt = account.OpenedAt,
            OwnerName = ownerName,
            OwnerEmail = ownerEmail
        };
    }
}