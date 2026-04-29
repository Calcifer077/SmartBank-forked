namespace SmartBank.API.DTOs.Reports
{
    public class ActiveCustomersReportDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string KycStatus { get; set; } = string.Empty;
        public int AccountCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}