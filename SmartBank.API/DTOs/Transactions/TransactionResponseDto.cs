namespace SmartBank.API.DTOs.Transactions
{
    public class TransactionResponseDto
    {
        public int TransactionId { get; set; }
        public string Type { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal BalanceAfter { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        // For transfers
        public string? OtherPartyName { get; set; } // Name of person sending/receiving from
        public string? OtherPartyAccountNumber { get; set; }
        public string? OtherPartyEmail { get; set; }
    }
}
