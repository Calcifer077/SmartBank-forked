using System.ComponentModel.DataAnnotations;

namespace SmartBank.MVC.ViewModels
{
    public class DepositWithdrawViewModel
    {
        public int AccountId { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty;
        public decimal CurrentBalance { get; set; }

        [Required(ErrorMessage = "Amount is required.")]
        [Range(1, 1000000, ErrorMessage = "Amount must be between ₹1 and ₹10,00,000.")]
        public decimal Amount { get; set; }

        [MaxLength(200)]
        public string Description { get; set; } = string.Empty;
    }
}