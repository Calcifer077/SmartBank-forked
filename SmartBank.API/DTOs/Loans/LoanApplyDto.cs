namespace SmartBank.API.DTOs.Loans
{
    public class LoanApplyDto
    {
        public decimal Amount { get; set; }
        public int TermMonths { get; set; }
        public string Purpose { get; set; } = string.Empty;
    }
}