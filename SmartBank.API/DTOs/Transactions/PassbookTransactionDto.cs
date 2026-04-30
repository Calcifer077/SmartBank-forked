namespace SmartBank.API.DTOs.Transactions
{
    public class PassbookTransactionDto
    {
        public int TransactionId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Type { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal BalanceAfter { get; set; }
        public string Description { get; set; } = string.Empty;
        // For transfers
        public string? OtherPartyName { get; set; }
        public string? OtherPartyAccountNumber { get; set; }
        public string? OtherPartyEmail { get; set; }
    }

    public class PassbookDataDto
    {
        public int AccountId { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty;
        public decimal CurrentBalance { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public DateTime AccountOpenedAt { get; set; }
        public List<PassbookTransactionDto> Transactions { get; set; } = new();
    }

    public class PassbookDownloadRequestDto
    {
        public int AccountId { get; set; }
        public int NumberOfTransactions { get; set; } = 10; // Default to last 10
    }
}
