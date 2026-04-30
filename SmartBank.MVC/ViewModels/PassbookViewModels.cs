namespace SmartBank.MVC.ViewModels
{
    public class PassbookTransactionViewModel
    {
        public int TransactionId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Type { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal BalanceAfter { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? OtherPartyName { get; set; }
        public string? OtherPartyAccountNumber { get; set; }
        public string? OtherPartyEmail { get; set; }
    }

    public class PassbookViewModel
    {
        public int AccountId { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty;
        public decimal CurrentBalance { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public DateTime AccountOpenedAt { get; set; }
        public List<PassbookTransactionViewModel> Transactions { get; set; } = new();
    }
}