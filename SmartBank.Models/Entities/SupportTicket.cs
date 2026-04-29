namespace SmartBank.Models.Entities
{
    public class SupportTicket
    {
        public int TicketId { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = "Open"; // Open, InProgress, Resolved
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ResolvedAt { get; set; }

        // FK
        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}