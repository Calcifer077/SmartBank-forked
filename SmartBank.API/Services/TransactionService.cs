using Microsoft.EntityFrameworkCore;
using SmartBank.Data;
using SmartBank.Models.Entities;
using SmartBank.API.DTOs.Transactions;

namespace SmartBank.API.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly SmartBankDbContext _db;
        private readonly ILogger<TransactionService> _logger;

        public TransactionService(SmartBankDbContext db, ILogger<TransactionService> logger)
        {
            _db = db;
            _logger = logger;
        }

        // ── Deposit ───────────────────────────────────────────────────
        public async Task<TransactionResponseDto> DepositAsync(int userId, DepositWithdrawDto dto)
        {
            var account = await GetOwnedAccountAsync(dto.AccountId, userId);

            if (account.Status != "Active")
                throw new InvalidOperationException("Cannot deposit into a frozen or closed account.");

            // Begin atomic DB transaction
            await using var dbTx = await _db.Database.BeginTransactionAsync();
            try
            {
                account.Balance += dto.Amount;

                var transaction = new Transaction
                {
                    AccountId = account.AccountId,
                    Type = "Deposit",
                    Amount = dto.Amount,
                    BalanceAfter = account.Balance,
                    Description = string.IsNullOrWhiteSpace(dto.Description)
                        ? "Deposit" : dto.Description,
                    CreatedAt = DateTime.UtcNow
                };

                _db.Transactions.Add(transaction);
                await _db.SaveChangesAsync();
                await dbTx.CommitAsync();

                _logger.LogInformation(
                    "Deposit ₹{Amount} to account {AccountId}", dto.Amount, dto.AccountId);

                return MapTransaction(transaction, account.AccountNumber);
            }
            catch
            {
                await dbTx.RollbackAsync();
                throw;
            }
        }

        // ── Withdraw ──────────────────────────────────────────────────
        public async Task<TransactionResponseDto> WithdrawAsync(int userId, DepositWithdrawDto dto)
        {
            var account = await GetOwnedAccountAsync(dto.AccountId, userId);

            if (account.Status != "Active")
                throw new InvalidOperationException("Cannot withdraw from a frozen or closed account.");

            if (account.Balance < dto.Amount)
                throw new InvalidOperationException(
                    $"Insufficient balance. Available: ₹{account.Balance:N2}");

            await using var dbTx = await _db.Database.BeginTransactionAsync();
            try
            {
                account.Balance -= dto.Amount;

                var transaction = new Transaction
                {
                    AccountId = account.AccountId,
                    Type = "Withdrawal",
                    Amount = dto.Amount,
                    BalanceAfter = account.Balance,
                    Description = string.IsNullOrWhiteSpace(dto.Description)
                        ? "Withdrawal" : dto.Description,
                    CreatedAt = DateTime.UtcNow
                };

                _db.Transactions.Add(transaction);
                await _db.SaveChangesAsync();
                await dbTx.CommitAsync();

                _logger.LogInformation(
                    "Withdrawal ₹{Amount} from account {AccountId}", dto.Amount, dto.AccountId);

                return MapTransaction(transaction, account.AccountNumber);
            }
            catch
            {
                await dbTx.RollbackAsync();
                throw;
            }
        }

        // ── Transfer ──────────────────────────────────────────────────
        public async Task<TransferResponseDto> TransferAsync(int userId, TransferDto dto)
        {
            // Validate source account ownership
            var fromAccount = await GetOwnedAccountAsync(dto.FromAccountId, userId);

            if (fromAccount.Status != "Active")
                throw new InvalidOperationException("Source account is frozen or closed.");

            // Find destination by account number
            var toAccount = await _db.Accounts
                .FirstOrDefaultAsync(a => a.AccountNumber == dto.ToAccountNumberString)
                ?? throw new KeyNotFoundException("Destination account not found.");

            if (toAccount.Status != "Active")
                throw new InvalidOperationException("Destination account is frozen or closed.");

            // Cannot transfer to same account
            if (fromAccount.AccountId == toAccount.AccountId)
                throw new InvalidOperationException("Cannot transfer to the same account.");

            if (fromAccount.Balance < dto.Amount)
                throw new InvalidOperationException(
                    $"Insufficient balance. Available: ₹{fromAccount.Balance:N2}");

            await using var dbTx = await _db.Database.BeginTransactionAsync();
            try
            {
                // Debit source
                fromAccount.Balance -= dto.Amount;

                // Credit destination
                toAccount.Balance += dto.Amount;

                var description = string.IsNullOrWhiteSpace(dto.Description)
                    ? $"Transfer to {toAccount.AccountNumber}" : dto.Description;

                // Log debit transaction
                _db.Transactions.Add(new Transaction
                {
                    AccountId = fromAccount.AccountId,
                    Type = "Transfer Out",
                    Amount = dto.Amount,
                    BalanceAfter = fromAccount.Balance,
                    Description = description,
                    CreatedAt = DateTime.UtcNow
                });

                // Log credit transaction
                _db.Transactions.Add(new Transaction
                {
                    AccountId = toAccount.AccountId,
                    Type = "Transfer In",
                    Amount = dto.Amount,
                    BalanceAfter = toAccount.Balance,
                    Description = $"Transfer from {fromAccount.AccountNumber}",
                    CreatedAt = DateTime.UtcNow
                });

                // Transfer record
                var transfer = new Transfer
                {
                    FromAccountId = fromAccount.AccountId,
                    ToAccountId = toAccount.AccountId,
                    Amount = dto.Amount,
                    Description = description,
                    Status = "Completed",
                    CreatedAt = DateTime.UtcNow
                };

                _db.Transfers.Add(transfer);
                await _db.SaveChangesAsync();
                await dbTx.CommitAsync();

                _logger.LogInformation(
                    "Transfer ₹{Amount} from {From} to {To}",
                    dto.Amount, fromAccount.AccountNumber, toAccount.AccountNumber);

                return new TransferResponseDto
                {
                    TransferId = transfer.TransferId,
                    Amount = dto.Amount,
                    FromAccountNumber = fromAccount.AccountNumber,
                    ToAccountNumber = toAccount.AccountNumber,
                    NewBalance = fromAccount.Balance,
                    Status = transfer.Status,
                    CreatedAt = transfer.CreatedAt
                };
            }
            catch
            {
                await dbTx.RollbackAsync();
                throw;
            }
        }

        // ── History ───────────────────────────────────────────────────
        public async Task<List<TransactionResponseDto>> GetHistoryAsync(int userId, int accountId)
        {
            // Ownership check
            var account = await GetOwnedAccountAsync(accountId, userId);

            return await _db.Transactions
                .Where(t => t.AccountId == accountId)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new TransactionResponseDto
                {
                    TransactionId = t.TransactionId,
                    Type = t.Type,
                    Amount = t.Amount,
                    BalanceAfter = t.BalanceAfter,
                    Description = t.Description,
                    CreatedAt = t.CreatedAt,
                    AccountNumber = account.AccountNumber
                })
                .ToListAsync();
        }

        // ── Shared helpers ────────────────────────────────────────────
        private async Task<Account> GetOwnedAccountAsync(int accountId, int userId)
        {
            var account = await _db.Accounts.FindAsync(accountId)
                ?? throw new KeyNotFoundException("Account not found.");

            if (account.UserId != userId)
                throw new UnauthorizedAccessException("Access denied.");

            return account;
        }

        private static TransactionResponseDto MapTransaction(
            Transaction t, string accountNumber) => new()
        {
            TransactionId = t.TransactionId,
            Type = t.Type,
            Amount = t.Amount,
            BalanceAfter = t.BalanceAfter,
            Description = t.Description,
            CreatedAt = t.CreatedAt,
            AccountNumber = accountNumber
        };
    }
}