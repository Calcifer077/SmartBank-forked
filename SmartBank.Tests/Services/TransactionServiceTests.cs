

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SmartBank.API.DTOs.Transactions;
using SmartBank.API.Services;
using SmartBank.Models.Entities;
using SmartBank.Tests.Helpers;

namespace SmartBank.Tests.Services
{
    public class TransactionServiceTests
    {
        private readonly Mock<ILogger<TransactionService>> _loggerMock = new();

        // ── Deposit ───────────────────────────────────────────────────

        [Fact]
        public async Task Deposit_ValidAmount_ShouldIncreaseBalance()
        {
            // Arrange
            var context = DbContextHelper.GetInMemoryContext("tx_deposit_valid");
            var user = SeedHelper.SeedCustomerUser(context);
            var account = SeedHelper.SeedAccount(context, user.UserId);
            var service = new TransactionService(context, _loggerMock.Object);

            var dto = new DepositWithdrawDto
            {
                AccountId = account.AccountId,
                Amount = 5000,
                Description = "Test deposit"
            };

            // Act
            var result = await service.DepositAsync(user.UserId, dto);

            // Assert
            result.Should().NotBeNull();
            result.Amount.Should().Be(5000);
            result.BalanceAfter.Should().Be(15000); // 10000 + 5000

            var updatedAccount = context.Accounts.Find(account.AccountId);
            updatedAccount!.Balance.Should().Be(15000);
        }

        [Fact]
        public async Task Deposit_FrozenAccount_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var context = DbContextHelper.GetInMemoryContext("tx_deposit_frozen");
            var user = SeedHelper.SeedCustomerUser(context);
            var account = SeedHelper.SeedAccount(context, user.UserId);
            account.Status = "Frozen";
            context.SaveChanges();

            var service = new TransactionService(context, _loggerMock.Object);

            var dto = new DepositWithdrawDto
            {
                AccountId = account.AccountId,
                Amount = 1000
            };

            // Act
            var act = async () => await service.DepositAsync(user.UserId, dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*frozen*");
        }

        // ── Withdraw ──────────────────────────────────────────────────

        [Fact]
        public async Task Withdraw_ValidAmount_ShouldDecreaseBalance()
        {
            // Arrange
            var context = DbContextHelper.GetInMemoryContext("tx_withdraw_valid");
            var user = SeedHelper.SeedCustomerUser(context);
            var account = SeedHelper.SeedAccount(context, user.UserId);
            var service = new TransactionService(context, _loggerMock.Object);

            var dto = new DepositWithdrawDto
            {
                AccountId = account.AccountId,
                Amount = 3000
            };

            // Act
            var result = await service.WithdrawAsync(user.UserId, dto);

            // Assert
            result.BalanceAfter.Should().Be(7000); // 10000 - 3000

            var updatedAccount = context.Accounts.Find(account.AccountId);
            updatedAccount!.Balance.Should().Be(7000);
        }

        [Fact]
        public async Task Withdraw_InsufficientBalance_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var context = DbContextHelper.GetInMemoryContext("tx_withdraw_insufficient");
            var user = SeedHelper.SeedCustomerUser(context);
            var account = SeedHelper.SeedAccount(context, user.UserId);
            var service = new TransactionService(context, _loggerMock.Object);

            var dto = new DepositWithdrawDto
            {
                AccountId = account.AccountId,
                Amount = 99999 // more than balance
            };

            // Act
            var act = async () => await service.WithdrawAsync(user.UserId, dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Insufficient balance*");
        }

        // ── Transfer ──────────────────────────────────────────────────

        [Fact]
        public async Task Transfer_ValidTransfer_ShouldUpdateBothBalances()
        {
            // Arrange
            var context = DbContextHelper.GetInMemoryContext("tx_transfer_valid");
            var user = SeedHelper.SeedCustomerUser(context);
            var fromAccount = SeedHelper.SeedAccount(context, user.UserId);

            // Second account — different user
            var toAccount = new Account
            {
                AccountId = 2,
                AccountNumber = "SB-20249999999999",
                AccountType = "Savings",
                Balance = 5000,
                Status = "Active",
                UserId = user.UserId,
                OpenedAt = DateTime.UtcNow
            };
            context.Accounts.Add(toAccount);
            context.SaveChanges();

            var service = new TransactionService(context, _loggerMock.Object);

            var dto = new TransferDto
            {
                FromAccountId = fromAccount.AccountId,
                ToAccountNumberString = toAccount.AccountNumber,
                Amount = 2000
            };

            // Act
            var result = await service.TransferAsync(user.UserId, dto);

            // Assert
            result.Should().NotBeNull();
            result.Amount.Should().Be(2000);
            result.Status.Should().Be("Completed");

            var updatedFrom = context.Accounts.Find(fromAccount.AccountId);
            var updatedTo = context.Accounts.Find(toAccount.AccountId);

            updatedFrom!.Balance.Should().Be(8000);  // 10000 - 2000
            updatedTo!.Balance.Should().Be(7000);    // 5000 + 2000
        }

        [Fact]
        public async Task Transfer_SameAccount_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var context = DbContextHelper.GetInMemoryContext("tx_transfer_same");
            var user = SeedHelper.SeedCustomerUser(context);
            var account = SeedHelper.SeedAccount(context, user.UserId);
            var service = new TransactionService(context, _loggerMock.Object);

            var dto = new TransferDto
            {
                FromAccountId = account.AccountId,
                ToAccountNumberString = account.AccountNumber,
                Amount = 500
            };

            // Act
            var act = async () => await service.TransferAsync(user.UserId, dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*same account*");
        }

        [Fact]
        public async Task Transfer_InsufficientBalance_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var context = DbContextHelper.GetInMemoryContext("tx_transfer_insufficient");
            var user = SeedHelper.SeedCustomerUser(context);
            var fromAccount = SeedHelper.SeedAccount(context, user.UserId);

            var toAccount = new Account
            {
                AccountId = 2,
                AccountNumber = "SB-20249999999998",
                AccountType = "Savings",
                Balance = 0,
                Status = "Active",
                UserId = user.UserId,
                OpenedAt = DateTime.UtcNow
            };
            context.Accounts.Add(toAccount);
            context.SaveChanges();

            var service = new TransactionService(context, _loggerMock.Object);

            var dto = new TransferDto
            {
                FromAccountId = fromAccount.AccountId,
                ToAccountNumberString = toAccount.AccountNumber,
                Amount = 99999
            };

            // Act
            var act = async () => await service.TransferAsync(user.UserId, dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Insufficient balance*");
        }
    }
}
