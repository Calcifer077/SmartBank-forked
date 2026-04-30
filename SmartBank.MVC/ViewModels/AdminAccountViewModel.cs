namespace SmartBank.MVC.ViewModels
{
    public class AdminAccountItemViewModel
    {
        public int AccountId { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public DateTime OpenedAt { get; set; }
    }

    public class AdminAccountListViewModel
    {
        public List<AdminAccountItemViewModel> Accounts { get; set; } = new();
    }
}
