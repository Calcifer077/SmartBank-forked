namespace SmartBank.API.DTOs.Loans
{
    public class LoanReviewDto
    {
        public int LoanId { get; set; }
        public string Decision { get; set; } = string.Empty; // Approved, Rejected
    }
}