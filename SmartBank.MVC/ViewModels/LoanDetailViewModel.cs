namespace SmartBank.MVC.ViewModels
{
    public class LoanDetailViewModel
    {
        public int LoanId { get; set; }
        public decimal Amount { get; set; }
        public int TermMonths { get; set; }
        public string Purpose { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime AppliedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string ApplicantName { get; set; } = string.Empty;
        public string ApplicantEmail { get; set; } = string.Empty;
        public decimal MonthlyPayment { get; set; }
    }
}