namespace SmartBank.API.DTOs.Transactions
{
    public class TransferDto
    {
        public int FromAccountId { get; set; }
        public string ToAccountNumberString { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
