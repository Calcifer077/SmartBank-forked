using SmartBank.Data;
using SmartBank.Models.Entities;

namespace SmartBank.Tests.Helpers
{
    public static class SeedHelper
    {
        public static void SeedRoles(SmartBankDbContext context)
        {
            if (context.Roles.Any()) return;

            context.Roles.AddRange(
                new Role { RoleId = 1, RoleName = "Admin" },
                new Role { RoleId = 2, RoleName = "Customer" },
                new Role { RoleId = 3, RoleName = "Teller" },
                new Role { RoleId = 4, RoleName = "Manager" },
                new Role { RoleId = 5, RoleName = "Auditor" }
            );
            context.SaveChanges();
        }

        public static User SeedCustomerUser(SmartBankDbContext context)
        {
            SeedRoles(context);

            // ── NO hardcoded UserId — let InMemory DB auto-assign ────
            var user = new User
            {
                FullName = "Test User",
                Email = "test@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password1"),
                PhoneNumber = "9876543210",
                Address = "123 Test St",
                RoleId = 2,
                KycStatus = "Pending",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(user);
            context.SaveChanges();
            return user; // UserId is now populated by EF after SaveChanges
        }

        public static Account SeedAccount(
            SmartBankDbContext context, int userId, string type = "Savings")
        {
            // ── NO hardcoded AccountId ────────────────────────────────
            var account = new Account
            {
                AccountNumber = $"SB-{DateTime.UtcNow.Ticks}", // unique per call
                AccountType = type,
                Balance = 10000,
                Status = "Active",
                UserId = userId,
                OpenedAt = DateTime.UtcNow
            };

            context.Accounts.Add(account);
            context.SaveChanges();
            return account;
        }
    }
}