using FluentAssertions;
using SmartBank.API.DTOs.Auth;
using SmartBank.API.DTOs.Accounts;
using SmartBank.API.DTOs.Transactions;
using SmartBank.API.DTOs.Loans;
using SmartBank.API.Validators;

namespace SmartBank.Tests.Validators
{
    public class ValidatorTests
    {
        // ── Register Validator ────────────────────────────────────────

        [Fact]
        public async Task RegisterValidator_ValidData_ShouldPass()
        {
            var validator = new RegisterValidator();
            var dto = new RegisterRequestDto
            {
                FullName = "John Doe",
                Email = "john@example.com",
                Password = "Password1",
                PhoneNumber = "9876543210"
            };

            var result = await validator.ValidateAsync(dto);
            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData("", "john@example.com", "Password1", "9876543210")]
        [InlineData("John", "notanemail", "Password1", "9876543210")]
        [InlineData("John", "john@example.com", "weak", "9876543210")]
        [InlineData("John", "john@example.com", "Password1", "123")]
        public async Task RegisterValidator_InvalidData_ShouldFail(
            string name, string email, string password, string phone)
        {
            var validator = new RegisterValidator();
            var dto = new RegisterRequestDto
            {
                FullName = name,
                Email = email,
                Password = password,
                PhoneNumber = phone
            };

            var result = await validator.ValidateAsync(dto);
            result.IsValid.Should().BeFalse();
        }

        // ── Account Validator ─────────────────────────────────────────

        [Theory]
        [InlineData("Savings", true)]
        [InlineData("Current", true)]
        [InlineData("Fixed", false)]
        [InlineData("", false)]
        public async Task AccountCreateValidator_ShouldValidateType(
            string type, bool expected)
        {
            var validator = new AccountCreateValidator();
            var dto = new AccountCreateDto { AccountType = type };

            var result = await validator.ValidateAsync(dto);
            result.IsValid.Should().Be(expected);
        }

   

        // ── Loan Validator ────────────────────────────────────────────

        [Theory]
        [InlineData(100000, 12, "Home renovation", true)]
        [InlineData(0, 12, "Purpose", false)]       // zero amount
        [InlineData(100000, 0, "Purpose", false)]   // invalid term
        [InlineData(100000, 12, "", false)]          // empty purpose
        [InlineData(6000000, 12, "Purpose", false)] // over limit
        public async Task LoanValidator_ShouldValidate(
            decimal amount, int term, string purpose, bool expected)
        {
            var validator = new LoanApplyValidator();
            var dto = new LoanApplyDto
            {
                Amount = amount,
                TermMonths = term,
                Purpose = purpose
            };

            var result = await validator.ValidateAsync(dto);
            result.IsValid.Should().Be(expected);
        }
    }
}