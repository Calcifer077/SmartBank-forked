namespace SmartBank.Models.Entities
{
    public class Transaction
    {
        public int TransactionId { get; set; }
        public string Type { get; set; } = string.Empty; // Deposit, Withdrawal
        public decimal Amount { get; set; }
        public decimal BalanceAfter { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // FK
        public int AccountId { get; set; }
        public Account Account { get; set; } = null!;
    }
}