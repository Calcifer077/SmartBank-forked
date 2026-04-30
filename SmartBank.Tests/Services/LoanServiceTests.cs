using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SmartBank.API.DTOs.Loans;
using SmartBank.API.Services;
using SmartBank.Models.Entities;
using SmartBank.Tests.Helpers;

namespace SmartBank.Tests.Services
{
    public class LoanServiceTests
    {
        private readonly Mock<ILogger<LoanService>> _loggerMock = new();

        [Fact]
        public async Task Apply_ValidLoan_ShouldReturnPendingLoan()
        {
            // Arrange
            var context = DbContextHelper.GetInMemoryContext("loan_apply_valid");
            var user = SeedHelper.SeedCustomerUser(context);
            var service = new LoanService(context, _loggerMock.Object);

            var dto = new LoanApplyDto
            {
                Amount = 100000,
                TermMonths = 12,
                Purpose = "Home renovation"
            };

            // Act
            var result = await service.ApplyAsync(user.UserId, dto);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be("Pending");
            result.Amount.Should().Be(100000);
            result.MonthlyPayment.Should().Be(Math.Round(100000m / 12, 2));
        }

        [Fact]
        public async Task Apply_WithExistingPendingLoan_ShouldThrow()
        {
            // Arrange
            var context = DbContextHelper.GetInMemoryContext("loan_apply_duplicate");
            var user = SeedHelper.SeedCustomerUser(context);

            context.Loans.Add(new Loan
            {
                LoanId = 1,
                UserId = user.UserId,
                Amount = 50000,
                TermMonths = 6,
                Purpose = "Existing loan",
                Status = "Pending",
                AppliedAt = DateTime.UtcNow
            });
            context.SaveChanges();

            var service = new LoanService(context, _loggerMock.Object);

            var dto = new LoanApplyDto
            {
                Amount = 100000,
                TermMonths = 12,
                Purpose = "Another loan"
            };

            // Act
            var act = async () => await service.ApplyAsync(user.UserId, dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*pending loan*");
        }

        [Fact]
        public async Task ReviewLoan_Approve_ShouldUpdateStatusAndNotify()
        {
            // Arrange
            var context = DbContextHelper.GetInMemoryContext("loan_review_approve");
            var user = SeedHelper.SeedCustomerUser(context);

            context.Loans.Add(new Loan
            {
                LoanId = 1,
                UserId = user.UserId,
                Amount = 50000,
                TermMonths = 12,
                Purpose = "Test",
                Status = "Pending",
                AppliedAt = DateTime.UtcNow
            });
            context.SaveChanges();

            var service = new LoanService(context, _loggerMock.Object);

            // Act
            var result = await service.ReviewLoanAsync(
                new LoanReviewDto { LoanId = 1, Decision = "Approved" });

            // Assert
            result.Status.Should().Be("Approved");
            result.ReviewedAt.Should().NotBeNull();

            var notification = context.Notifications
                .FirstOrDefault(n => n.UserId == user.UserId);
            notification.Should().NotBeNull();
            notification!.Title.Should().Be("Loan Approved");
        }

        [Fact]
        public async Task ReviewLoan_AlreadyReviewed_ShouldThrow()
        {
            // Arrange
            var context = DbContextHelper.GetInMemoryContext("loan_review_already");
            var user = SeedHelper.SeedCustomerUser(context);

            context.Loans.Add(new Loan
            {
                LoanId = 1,
                UserId = user.UserId,
                Amount = 50000,
                TermMonths = 12,
                Purpose = "Test",
                Status = "Approved",
                AppliedAt = DateTime.UtcNow,
                ReviewedAt = DateTime.UtcNow
            });
            context.SaveChanges();

            var service = new LoanService(context, _loggerMock.Object);

            // Act
            var act = async () => await service.ReviewLoanAsync(
                new LoanReviewDto { LoanId = 1, Decision = "Rejected" });

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*already Approved*");
        }
    }
}