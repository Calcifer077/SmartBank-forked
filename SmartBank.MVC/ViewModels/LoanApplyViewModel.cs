using System.ComponentModel.DataAnnotations;

namespace SmartBank.MVC.ViewModels
{
    public class LoanApplyViewModel
    {
        [Required(ErrorMessage = "Loan amount is required.")]
        [Range(1000, 5000000, ErrorMessage = "Amount must be between ₹1,000 and ₹50,00,000.")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Loan term is required.")]
        [Range(3, 360, ErrorMessage = "Term must be between 3 and 360 months.")]
        public int TermMonths { get; set; }

        [Required(ErrorMessage = "Purpose is required.")]
        [MaxLength(200)]
        public string Purpose { get; set; } = string.Empty;
    }
}