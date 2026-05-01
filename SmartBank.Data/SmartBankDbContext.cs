using Microsoft.EntityFrameworkCore;
using SmartBank.Models.Entities;

namespace SmartBank.Data
{
    public class SmartBankDbContext : DbContext
    {
        public SmartBankDbContext(DbContextOptions<SmartBankDbContext> options)
            : base(options) { }

        // DbSets — all 8 tables
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Account> Accounts => Set<Account>();
        public DbSet<Transaction> Transactions => Set<Transaction>();
        public DbSet<Transfer> Transfers => Set<Transfer>();
        public DbSet<Loan> Loans => Set<Loan>();
        public DbSet<SupportTicket> SupportTickets => Set<SupportTicket>();
        public DbSet<Notification> Notifications => Set<Notification>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── Role ──────────────────────────────────────────────
            modelBuilder.Entity<Role>(e =>
            {
                e.HasKey(r => r.RoleId);
                e.Property(r => r.RoleName).IsRequired().HasMaxLength(50);
            });

            // ── User ──────────────────────────────────────────────
            modelBuilder.Entity<User>(e =>
            {
                e.HasKey(u => u.UserId);
                e.Property(u => u.FullName).IsRequired().HasMaxLength(100);
                e.Property(u => u.Email).IsRequired().HasMaxLength(150);
                e.HasIndex(u => u.Email).IsUnique();
                e.Property(u => u.PasswordHash).IsRequired();
                e.Property(u => u.PhoneNumber).HasMaxLength(15);
                e.Property(u => u.KycStatus).HasMaxLength(20);

                e.HasOne(u => u.Role)
                 .WithMany(r => r.Users)
                 .HasForeignKey(u => u.RoleId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ── Account ───────────────────────────────────────────
            modelBuilder.Entity<Account>(e =>
            {
                e.HasKey(a => a.AccountId);
                e.Property(a => a.AccountNumber).IsRequired().HasMaxLength(20);
                e.HasIndex(a => a.AccountNumber).IsUnique();
                e.Property(a => a.AccountType).IsRequired().HasMaxLength(20);
                e.Property(a => a.Balance).HasColumnType("decimal(18,2)");
                e.Property(a => a.Status).HasMaxLength(20);

                e.HasOne(a => a.User)
                 .WithMany(u => u.Accounts)
                 .HasForeignKey(a => a.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ── Transaction ───────────────────────────────────────
            modelBuilder.Entity<Transaction>(e =>
            {
                e.HasKey(t => t.TransactionId);
                e.Property(t => t.Amount).HasColumnType("decimal(18,2)");
                e.Property(t => t.BalanceAfter).HasColumnType("decimal(18,2)");
                e.Property(t => t.Type).IsRequired().HasMaxLength(20);

                e.HasOne(t => t.Account)
                 .WithMany(a => a.Transactions)
                 .HasForeignKey(t => t.AccountId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ── Transfer ──────────────────────────────────────────
            // Two FKs to Account — must restrict to avoid multiple cascade paths
            modelBuilder.Entity<Transfer>(e =>
            {
                e.HasKey(t => t.TransferId);
                e.Property(t => t.Amount).HasColumnType("decimal(18,2)");
                e.Property(t => t.Status).HasMaxLength(20);

                e.HasOne(t => t.FromAccount)
                 .WithMany(a => a.SentTransfers)
                 .HasForeignKey(t => t.FromAccountId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(t => t.ToAccount)
                 .WithMany(a => a.ReceivedTransfers)
                 .HasForeignKey(t => t.ToAccountId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ── Loan ──────────────────────────────────────────────
            modelBuilder.Entity<Loan>(e =>
            {
                e.HasKey(l => l.LoanId);
                e.Property(l => l.Amount).HasColumnType("decimal(18,2)");
                e.Property(l => l.Status).HasMaxLength(20);
                e.Property(l => l.Purpose).HasMaxLength(200);

                e.HasOne(l => l.User)
                 .WithMany(u => u.Loans)
                 .HasForeignKey(l => l.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ── SupportTicket ─────────────────────────────────────
            modelBuilder.Entity<SupportTicket>(e =>
            {
                e.HasKey(s => s.TicketId);
                e.Property(s => s.Subject).IsRequired().HasMaxLength(150);
                e.Property(s => s.Status).HasMaxLength(20);

                e.HasOne(s => s.User)
                 .WithMany(u => u.SupportTickets)
                 .HasForeignKey(s => s.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ── Notification ──────────────────────────────────────
            modelBuilder.Entity<Notification>(e =>
            {
                e.HasKey(n => n.NotificationId);
                e.Property(n => n.Title).IsRequired().HasMaxLength(100);
                e.Property(n => n.Message).IsRequired().HasMaxLength(500);

                e.HasOne(n => n.User)
                 .WithMany(u => u.Notifications)
                 .HasForeignKey(n => n.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ── Seed Roles ────────────────────────────────────────
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = 1, RoleName = "Admin" },
                new Role { RoleId = 2, RoleName = "Customer" },
                new Role { RoleId = 3, RoleName = "Teller" },
                new Role { RoleId = 4, RoleName = "Manager" },
                new Role { RoleId = 5, RoleName = "Auditor" }
            );

        
        }
    }
}