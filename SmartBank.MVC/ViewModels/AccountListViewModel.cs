namespace SmartBank.MVC.ViewModels
{
    public class AccountListViewModel
    {
        public List<AccountSummaryViewModel> Accounts { get; set; } = new();
        public bool HasSavings => Accounts.Any(a => a.AccountType == "Savings");
        public bool HasCurrent => Accounts.Any(a => a.AccountType == "Current");
    }
}