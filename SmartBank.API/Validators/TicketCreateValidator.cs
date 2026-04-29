using FluentValidation;
using SmartBank.API.DTOs.Support;

namespace SmartBank.API.Validators
{
    public class TicketCreateValidator : AbstractValidator<TicketCreateDto>
    {
        public TicketCreateValidator()
        {
            RuleFor(x => x.Subject)
                .NotEmpty().WithMessage("Subject is required.")
                .MaximumLength(150).WithMessage("Subject cannot exceed 150 characters.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.")
                .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.");
        }
    }
}