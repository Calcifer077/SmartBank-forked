namespace SmartBank.MVC.ViewModels
{
    public class TransactionItemViewModel
    {
        public int TransactionId { get; set; }
        public string Type { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal BalanceAfter { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsCredit => Type == "Deposit" || Type == "Transfer In";
    }

    public class TransactionHistoryViewModel
    {
        public int AccountId { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty;
        public decimal CurrentBalance { get; set; }
        public List<TransactionItemViewModel> Transactions { get; set; } = new();
    }
}