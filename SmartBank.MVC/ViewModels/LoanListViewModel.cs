namespace SmartBank.MVC.ViewModels
{
    public class LoanItemViewModel
    {
        public int LoanId { get; set; }
        public decimal Amount { get; set; }
        public int TermMonths { get; set; }
        public string Purpose { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime AppliedAt { get; set; }
        public decimal MonthlyPayment { get; set; }
    }

    public class LoanListViewModel
    {
        public List<LoanItemViewModel> Loans { get; set; } = new();
        public bool HasPending => Loans.Any(l => l.Status == "Pending");
    }
}