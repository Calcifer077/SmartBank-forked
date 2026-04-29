namespace SmartBank.Models.Entities
{
    public class Account
    {
        public int AccountId { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty; // Savings, Current
        public decimal Balance { get; set; } = 0;
        public string Status { get; set; } = "Active"; // Active, Frozen, Closed
        public DateTime OpenedAt { get; set; } = DateTime.UtcNow;

        // FK
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        // Navigation
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public ICollection<Transfer> SentTransfers { get; set; } = new List<Transfer>();
        public ICollection<Transfer> ReceivedTransfers { get; set; } = new List<Transfer>();
    }
}