using System.ComponentModel.DataAnnotations;

namespace SmartBank.MVC.ViewModels
{
    public class AccountCreateViewModel
    {
        [Required(ErrorMessage = "Please select an account type.")]
        public string AccountType { get; set; } = string.Empty;
    }
}