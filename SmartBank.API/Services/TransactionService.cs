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

            return await ExecuteWithOptionalTransactionAsync(async () =>
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

                // Create notification for deposit
                var user = account.User;
                _db.Notifications.Add(new Notification
                {
                    UserId = user.UserId,
                    Title = "Deposit Received",
                    Message = $"₹{dto.Amount:N2} deposited into account {account.AccountNumber}. " +
                              $"Your new balance: ₹{account.Balance:N2}",
                    CreatedAt = DateTime.UtcNow
                });

                await _db.SaveChangesAsync();

                _logger.LogInformation(
                    "Deposit ₹{Amount} to account {AccountId}", dto.Amount, dto.AccountId);

                return MapTransaction(transaction, account.AccountNumber);
            });
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

            return await ExecuteWithOptionalTransactionAsync(async () =>
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

                // Create notification for withdrawal
                var user = account.User;
                _db.Notifications.Add(new Notification
                {
                    UserId = user.UserId,
                    Title = "Withdrawal Completed",
                    Message = $"₹{dto.Amount:N2} withdrawn from account {account.AccountNumber}. " +
                              $"Your new balance: ₹{account.Balance:N2}",
                    CreatedAt = DateTime.UtcNow
                });

                await _db.SaveChangesAsync();

                _logger.LogInformation(
                    "Withdrawal ₹{Amount} from account {AccountId}", dto.Amount, dto.AccountId);

                return MapTransaction(transaction, account.AccountNumber);
            });
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
                .Include(a => a.User)
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

            return await ExecuteWithOptionalTransactionAsync(async () =>
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

                // Create notifications for both sender and receiver
                var fromUser = fromAccount.User;
                var toUser = toAccount.User;

                // Notification for sender (From)
                _db.Notifications.Add(new Notification
                {
                    UserId = fromUser.UserId,
                    Title = "Money Transferred",
                    Message = $"₹{dto.Amount:N2} transferred to {toUser.FullName} ({toAccount.AccountNumber}). " +
                              $"Your new balance: ₹{fromAccount.Balance:N2}",
                    CreatedAt = DateTime.UtcNow
                });

                // Notification for receiver (To)
                _db.Notifications.Add(new Notification
                {
                    UserId = toUser.UserId,
                    Title = "Money Received",
                    Message = $"₹{dto.Amount:N2} received from {fromUser.FullName} ({fromAccount.AccountNumber}). " +
                              $"Your new balance: ₹{toAccount.Balance:N2}",
                    CreatedAt = DateTime.UtcNow
                });

                await _db.SaveChangesAsync();

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
            });
        }

        // ── History ───────────────────────────────────────────────────
        public async Task<List<TransactionResponseDto>> GetHistoryAsync(int userId, int accountId)
        {
            // Ownership check
            var account = await GetOwnedAccountAsync(accountId, userId);

            var transactions = await _db.Transactions
                .Where(t => t.AccountId == accountId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            var result = new List<TransactionResponseDto>();

            foreach (var t in transactions)
            {
                var dto = new TransactionResponseDto
                {
                    TransactionId = t.TransactionId,
                    Type = t.Type,
                    Amount = t.Amount,
                    BalanceAfter = t.BalanceAfter,
                    Description = t.Description,
                    CreatedAt = t.CreatedAt,
                    AccountNumber = account.AccountNumber
                };

                // For transfers, populate other party information
                if (t.Type == "Transfer Out")
                {
                    var transfer = await _db.Transfers
                        .Where(tr => tr.FromAccountId == accountId && 
                                    EF.Functions.DateDiffSecond(tr.CreatedAt, t.CreatedAt) == 0)
                        .Include(tr => tr.ToAccount)
                        .ThenInclude(a => a.User)
                        .FirstOrDefaultAsync();

                    if (transfer?.ToAccount?.User != null)
                    {
                        dto.OtherPartyName = transfer.ToAccount.User.FullName;
                        dto.OtherPartyAccountNumber = transfer.ToAccount.AccountNumber;
                        dto.OtherPartyEmail = transfer.ToAccount.User.Email;
                    }
                }
                else if (t.Type == "Transfer In")
                {
                    var transfer = await _db.Transfers
                        .Where(tr => tr.ToAccountId == accountId && 
                                    EF.Functions.DateDiffSecond(tr.CreatedAt, t.CreatedAt) == 0)
                        .Include(tr => tr.FromAccount)
                        .ThenInclude(a => a.User)
                        .FirstOrDefaultAsync();

                    if (transfer?.FromAccount?.User != null)
                    {
                        dto.OtherPartyName = transfer.FromAccount.User.FullName;
                        dto.OtherPartyAccountNumber = transfer.FromAccount.AccountNumber;
                        dto.OtherPartyEmail = transfer.FromAccount.User.Email;
                    }
                }

                result.Add(dto);
            }

            return result;
        }

        // ── Passbook ──────────────────────────────────────────────────
        public async Task<PassbookDataDto> GetPassbookDataAsync(int userId, int accountId, int numberOfTransactions = 10)
        {
            var account = await GetOwnedAccountAsync(accountId, userId);
            var user = await _db.Users.FindAsync(userId)
                ?? throw new KeyNotFoundException("User not found.");

            var transactionList = await _db.Transactions
                .Where(t => t.AccountId == accountId)
                .OrderByDescending(t => t.CreatedAt)
                .Take(numberOfTransactions)
                .ToListAsync();

            var transactions = new List<PassbookTransactionDto>();

            foreach (var t in transactionList)
            {
                var dto = new PassbookTransactionDto
                {
                    TransactionId = t.TransactionId,
                    CreatedAt = t.CreatedAt,
                    Type = t.Type,
                    Amount = t.Amount,
                    BalanceAfter = t.BalanceAfter,
                    Description = t.Description
                };

                // For transfers, populate other party information
                if (t.Type == "Transfer Out")
                {
                    var transfer = await _db.Transfers
                        .Where(tr => tr.FromAccountId == accountId && 
                                    EF.Functions.DateDiffSecond(tr.CreatedAt, t.CreatedAt) == 0)
                        .Include(tr => tr.ToAccount)
                        .ThenInclude(a => a.User)
                        .FirstOrDefaultAsync();

                    if (transfer?.ToAccount?.User != null)
                    {
                        dto.OtherPartyName = transfer.ToAccount.User.FullName;
                        dto.OtherPartyAccountNumber = transfer.ToAccount.AccountNumber;
                        dto.OtherPartyEmail = transfer.ToAccount.User.Email;
                    }
                }
                else if (t.Type == "Transfer In")
                {
                    var transfer = await _db.Transfers
                        .Where(tr => tr.ToAccountId == accountId && 
                                    EF.Functions.DateDiffSecond(tr.CreatedAt, t.CreatedAt) == 0)
                        .Include(tr => tr.FromAccount)
                        .ThenInclude(a => a.User)
                        .FirstOrDefaultAsync();

                    if (transfer?.FromAccount?.User != null)
                    {
                        dto.OtherPartyName = transfer.FromAccount.User.FullName;
                        dto.OtherPartyAccountNumber = transfer.FromAccount.AccountNumber;
                        dto.OtherPartyEmail = transfer.FromAccount.User.Email;
                    }
                }

                transactions.Add(dto);
            }

            return new PassbookDataDto
            {
                AccountId = account.AccountId,
                AccountNumber = account.AccountNumber,
                AccountType = account.AccountType,
                CurrentBalance = account.Balance,
                UserName = user.FullName,
                UserEmail = user.Email,
                AccountOpenedAt = account.OpenedAt,
                Transactions = transactions
            };
        }

        // ── Shared helpers ────────────────────────────────────────────
        private async Task<Account> GetOwnedAccountAsync(int accountId, int userId)
        {
            var account = await _db.Accounts
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.AccountId == accountId)
                ?? throw new KeyNotFoundException("Account not found.");

            if (account.UserId != userId)
                throw new UnauthorizedAccessException("Access denied.");

            return account;
        }

        private async Task<TResult> ExecuteWithOptionalTransactionAsync<TResult>(Func<Task<TResult>> action)
        {
            if (!_db.Database.IsRelational())
                return await action();

            await using var dbTx = await _db.Database.BeginTransactionAsync();
            try
            {
                var result = await action();
                await dbTx.CommitAsync();
                return result;
            }
            catch
            {
                await dbTx.RollbackAsync();
                throw;
            }
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