using FluentValidation;
using SmartBank.API.DTOs.Accounts;

namespace SmartBank.API.Validators
{
    public class AccountCreateValidator : AbstractValidator<AccountCreateDto>
    {
        private static readonly string[] AllowedTypes = { "Savings", "Current" };

        public AccountCreateValidator()
        {
            RuleFor(x => x.AccountType)
                .NotEmpty().WithMessage("Account type is required.")
                .Must(t => AllowedTypes.Contains(t))
                .WithMessage("Account type must be Savings or Current.");
        }
    }
}