using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SmartBank.API.DTOs.Accounts;
using SmartBank.API.Services;
using SmartBank.Tests.Helpers;

namespace SmartBank.Tests.Services
{
    public class AccountServiceTests
    {
        private readonly Mock<ILogger<AccountService>> _loggerMock = new();

        [Fact]
        public async Task CreateAccount_WithValidData_ShouldReturnAccount()
        {
            // Arrange
            var context = DbContextHelper.GetInMemoryContext("acc_create_valid");
            var user = SeedHelper.SeedCustomerUser(context);
            var service = new AccountService(context, _loggerMock.Object);

            var dto = new AccountCreateDto { AccountType = "Savings" };

            // Act
            var result = await service.CreateAccountAsync(user.UserId, dto);

            // Assert
            result.Should().NotBeNull();
            result.AccountType.Should().Be("Savings");
            result.Balance.Should().Be(0);
            result.AccountNumber.Should().StartWith("SB-");
            result.Status.Should().Be("Active");
        }

        [Fact]
        public async Task CreateAccount_DuplicateType_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var context = DbContextHelper.GetInMemoryContext("acc_create_duplicate");
            var user = SeedHelper.SeedCustomerUser(context);
            SeedHelper.SeedAccount(context, user.UserId, "Savings");
            var service = new AccountService(context, _loggerMock.Object);

            var dto = new AccountCreateDto { AccountType = "Savings" };

            // Act
            var act = async () => await service.CreateAccountAsync(user.UserId, dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*already have a Savings account*");
        }

        [Fact]
        public async Task CreateAccount_BothTypes_ShouldSucceed()
        {
            // Arrange
            var context = DbContextHelper.GetInMemoryContext("acc_both_types");
            var user = SeedHelper.SeedCustomerUser(context);
            SeedHelper.SeedAccount(context, user.UserId, "Savings");
            var service = new AccountService(context, _loggerMock.Object);

            var dto = new AccountCreateDto { AccountType = "Current" };

            // Act
            var result = await service.CreateAccountAsync(user.UserId, dto);

            // Assert
            result.Should().NotBeNull();
            result.AccountType.Should().Be("Current");
        }

        [Fact]
        public async Task GetUserAccounts_ShouldReturnOnlyUserAccounts()
        {
            // Arrange
            var context = DbContextHelper.GetInMemoryContext("acc_get_user");
            var user = SeedHelper.SeedCustomerUser(context);
            SeedHelper.SeedAccount(context, user.UserId);
            var service = new AccountService(context, _loggerMock.Object);

            // Act
            var result = await service.GetUserAccountsAsync(user.UserId);

            // Assert
            result.Should().HaveCount(1);
            result.First().AccountType.Should().Be("Savings");
        }

        [Fact]
        public async Task GetAccountDetail_WithWrongUser_ShouldThrowUnauthorized()
        {
            // Arrange
            var context = DbContextHelper.GetInMemoryContext("acc_detail_wrong_user");
            var user = SeedHelper.SeedCustomerUser(context);
            var account = SeedHelper.SeedAccount(context, user.UserId);
            var service = new AccountService(context, _loggerMock.Object);

            // Act — different userId
            var act = async () =>
                await service.GetAccountDetailAsync(account.AccountId, 999);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }
    }
}