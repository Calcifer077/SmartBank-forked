using FluentValidation;
using SmartBank.API.DTOs.Transactions;

namespace SmartBank.API.Validators
{
    public class TransferValidator : AbstractValidator<TransferDto>
    {
        public TransferValidator()
        {
            RuleFor(x => x.FromAccountId)
                .GreaterThan(0).WithMessage("Invalid source account.");

            RuleFor(x => x.ToAccountNumberString)
                .NotEmpty().WithMessage("Destination account number is required.")
                .MaximumLength(20);

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than zero.")
                .LessThanOrEqualTo(1000000).WithMessage("Amount cannot exceed ₹10,00,000 per transfer.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.")
                .MaximumLength(200).WithMessage("Description cannot exceed 200 characters.");
        }
    }
}