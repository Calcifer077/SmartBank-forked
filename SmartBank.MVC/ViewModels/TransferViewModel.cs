using System.ComponentModel.DataAnnotations;

namespace SmartBank.MVC.ViewModels
{
    public class TransferViewModel
    {
        public int FromAccountId { get; set; }
        public string FromAccountNumber { get; set; } = string.Empty;
        public decimal CurrentBalance { get; set; }

        // For dropdown population
        public List<AccountSummaryViewModel> UserAccounts { get; set; } = new();

        [Required(ErrorMessage = "Destination account number is required.")]
        public string ToAccountNumberString { get; set; } = string.Empty;

        [Required(ErrorMessage = "Amount is required.")]
        [Range(1, 1000000, ErrorMessage = "Amount must be between ₹1 and ₹10,00,000.")]
        public decimal Amount { get; set; }

        [MaxLength(200)]
        public string Description { get; set; } = string.Empty;
    }
}