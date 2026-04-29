namespace SmartBank.Models.Entities
{
    public class Loan
    {
        public int LoanId { get; set; }
        public decimal Amount { get; set; }
        public string Purpose { get; set; } = string.Empty;
        public int TermMonths { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
        public string DocumentPath { get; set; } = string.Empty;
        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReviewedAt { get; set; }

        // FK
        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}