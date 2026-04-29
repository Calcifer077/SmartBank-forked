using FluentValidation;
using SmartBank.API.DTOs.Transactions;

namespace SmartBank.API.Validators
{
    public class DepositWithdrawValidator : AbstractValidator<DepositWithdrawDto>
    {
        public DepositWithdrawValidator()
        {
            RuleFor(x => x.AccountId)
                .GreaterThan(0).WithMessage("Invalid account.");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than zero.")
                .LessThanOrEqualTo(1000000).WithMessage("Amount cannot exceed ₹10,00,000 per transaction.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.")
                .MaximumLength(200).WithMessage("Description cannot exceed 200 characters.");
        }
    }
}