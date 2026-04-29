namespace SmartBank.MVC.ViewModels
{
    public class AdminLoanItemViewModel
    {
        public int LoanId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int TermMonths { get; set; }
        public string Purpose { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime AppliedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
    }

    public class AdminLoanListViewModel
    {
        public List<AdminLoanItemViewModel> Loans { get; set; } = new();
    }
}