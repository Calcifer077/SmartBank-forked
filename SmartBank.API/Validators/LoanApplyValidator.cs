using FluentValidation;
using SmartBank.API.DTOs.Loans;

namespace SmartBank.API.Validators
{
    public class LoanApplyValidator : AbstractValidator<LoanApplyDto>
    {
        public LoanApplyValidator()
        {
            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Loan amount must be greater than zero.")
                .LessThanOrEqualTo(5000000).WithMessage("Loan cannot exceed ₹50,00,000.");

            RuleFor(x => x.TermMonths)
                .InclusiveBetween(3, 360)
                .WithMessage("Loan term must be between 3 and 360 months.");

            RuleFor(x => x.Purpose)
                .NotEmpty().WithMessage("Loan purpose is required.")
                .MaximumLength(200).WithMessage("Purpose cannot exceed 200 characters.");
        }
    }
}