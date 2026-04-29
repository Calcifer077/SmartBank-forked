namespace SmartBank.API.DTOs.Support
{
    public class TicketResponseDto
    {
        public int TicketId { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string ApplicantName { get; set; } = string.Empty;
    }
}