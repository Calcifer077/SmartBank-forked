using Microsoft.EntityFrameworkCore;
using SmartBank.API.DTOs.Admin;
using SmartBank.Data;
using SmartBank.Models.Entities;

namespace SmartBank.API.Services
{
    public class AdminService : IAdminService
    {
        private readonly SmartBankDbContext _db;
        private readonly ILogger<AdminService> _logger;

        public AdminService(SmartBankDbContext db, ILogger<AdminService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<AdminDashboardDto> GetDashboardStatsAsync()
        {
            var today = DateTime.UtcNow.Date;

            var todayTransactions = await _db.Transactions
                .Where(t => t.CreatedAt.Date == today)
                .ToListAsync();

            return new AdminDashboardDto
            {
                TotalUsers = await _db.Users.CountAsync(),
                ActiveUsers = await _db.Users.CountAsync(u => u.IsActive),
                TotalAccounts = await _db.Accounts.CountAsync(),
                FrozenAccounts = await _db.Accounts
                    .CountAsync(a => a.Status == "Frozen"),
                PendingLoans = await _db.Loans
                    .CountAsync(l => l.Status == "Pending"),
                OpenTickets = await _db.SupportTickets
                    .CountAsync(t => t.Status == "Open"),
                TransactionsToday = todayTransactions.Count,
                TotalDepositsToday = todayTransactions
                    .Where(t => t.Type == "Deposit")
                    .Sum(t => t.Amount),
                TotalWithdrawalsToday = todayTransactions
                    .Where(t => t.Type == "Withdrawal")
                    .Sum(t => t.Amount)
            };
        }

        public async Task<List<AdminUserDto>> GetAllUsersAsync()
        {
            return await _db.Users
                .Include(u => u.Role)
                .Include(u => u.Accounts)
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new AdminUserDto
                {
                    UserId = u.UserId,
                    FullName = u.FullName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    Role = u.Role.RoleName,
                    KycStatus = u.KycStatus,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt,
                    AccountCount = u.Accounts.Count
                })
                .ToListAsync();
        }

        public async Task<List<AdminAccountDto>> GetAllAccountsAsync()
        {
            return await _db.Accounts
                .Include(a => a.User)
                .OrderByDescending(a => a.OpenedAt)
                .Select(a => new AdminAccountDto
                {
                    AccountId = a.AccountId,
                    AccountNumber = a.AccountNumber,
                    AccountType = a.AccountType,
                    Status = a.Status,
                    Balance = a.Balance,
                    UserId = a.UserId,
                    UserName = a.User.FullName,
                    UserEmail = a.User.Email,
                    OpenedAt = a.OpenedAt
                })
                .ToListAsync();
        }

        public async Task<string> FreezeAccountAsync(FreezeAccountDto dto)
        {
            var allowed = new[] { "Freeze", "Unfreeze" };
            if (!allowed.Contains(dto.Action))
                throw new InvalidOperationException("Action must be Freeze or Unfreeze.");

            var account = await _db.Accounts
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.AccountId == dto.AccountId)
                ?? throw new KeyNotFoundException("Account not found.");

            var newStatus = dto.Action == "Freeze" ? "Frozen" : "Active";

            if (account.Status == newStatus)
                throw new InvalidOperationException(
                    $"Account is already {newStatus}.");

            account.Status = newStatus;

            // Notify account owner
            _db.Notifications.Add(new Notification
            {
                UserId = account.UserId,
                Title = $"Account {newStatus}",
                Message = dto.Action == "Freeze"
                    ? $"Your account {account.AccountNumber} has been frozen. " +
                      $"Contact support for assistance."
                    : $"Your account {account.AccountNumber} has been reactivated.",
                CreatedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();

            _logger.LogInformation(
                "Account {AccountId} {Action} by admin", dto.AccountId, dto.Action);

            return $"Account {newStatus} successfully.";
        }

        public async Task<List<RoleDto>> GetAllRolesAsync()
        {
            return await _db.Roles
                .OrderBy(r => r.RoleId)
                .Select(r => new RoleDto
                {
                    RoleId = r.RoleId,
                    RoleName = r.RoleName
                })
                .ToListAsync();
        }

        public async Task<string> ChangeUserRoleAsync(ChangeRoleDto dto)
        {
            var user = await _db.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == dto.UserId)
                ?? throw new KeyNotFoundException("User not found.");

            var newRole = await _db.Roles.FindAsync(dto.RoleId)
                ?? throw new KeyNotFoundException("Role not found.");

            if (user.RoleId == dto.RoleId)
                throw new InvalidOperationException(
                    $"User already has {newRole.RoleName} role.");

            var oldRole = user.Role.RoleName;
            user.RoleId = dto.RoleId;

            // Notify user of role change
            _db.Notifications.Add(new Notification
            {
                UserId = user.UserId,
                Title = "Role Changed",
                Message = $"Your role has been changed from {oldRole} to {newRole.RoleName} by an administrator.",
                CreatedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();

            _logger.LogInformation(
                "User {UserId} role changed from {OldRole} to {NewRole} by admin", 
                dto.UserId, oldRole, newRole.RoleName);

            return $"User role changed to {newRole.RoleName} successfully.";
        }
    }
}