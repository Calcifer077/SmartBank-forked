namespace SmartBank.Models.Entities
{
    public class Transfer
    {
        public int TransferId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = "Completed"; // Completed, Failed
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // FK — two sides of the transfer
        public int FromAccountId { get; set; }
        public Account FromAccount { get; set; } = null!;

        public int ToAccountId { get; set; }
        public Account ToAccount { get; set; } = null!;
    }
}