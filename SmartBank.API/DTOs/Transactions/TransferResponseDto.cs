namespace SmartBank.API.DTOs.Transactions
{
    public class TransferResponseDto
    {
        public int TransferId { get; set; }
        public decimal Amount { get; set; }
        public string FromAccountNumber { get; set; } = string.Empty;
        public string ToAccountNumber { get; set; } = string.Empty;
        public decimal NewBalance { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
