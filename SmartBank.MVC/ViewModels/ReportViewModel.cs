namespace SmartBank.MVC.ViewModels
{
    public class DailyTransactionItemViewModel
    {
        public int TransactionId { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal BalanceAfter { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsCredit => Type == "Deposit" || Type == "Transfer In";
    }

    public class DailyTransactionReportViewModel
    {
        public DateTime ReportDate { get; set; }
        public List<DailyTransactionItemViewModel> Transactions { get; set; } = new();
        public decimal TotalDeposits =>
            Transactions.Where(t => t.Type == "Deposit").Sum(t => t.Amount);
        public decimal TotalWithdrawals =>
            Transactions.Where(t => t.Type == "Withdrawal").Sum(t => t.Amount);
    }

    public class LoanReportItemViewModel
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

    public class LoanReportViewModel
    {
        public List<LoanReportItemViewModel> Loans { get; set; } = new();
        public int TotalApproved => Loans.Count(l => l.Status == "Approved");
        public int TotalRejected => Loans.Count(l => l.Status == "Rejected");
        public int TotalPending => Loans.Count(l => l.Status == "Pending");
        public decimal TotalApprovedAmount =>
            Loans.Where(l => l.Status == "Approved").Sum(l => l.Amount);
    }

    public class ActiveCustomerItemViewModel
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string KycStatus { get; set; } = string.Empty;
        public int AccountCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ActiveCustomersReportViewModel
    {
        public List<ActiveCustomerItemViewModel> Customers { get; set; } = new();
    }

    public class LowBalanceItemViewModel
    {
        public int AccountId { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public string OwnerEmail { get; set; } = string.Empty;
    }

    public class LowBalanceReportViewModel
    {
        public decimal Threshold { get; set; }
        public List<LowBalanceItemViewModel> Accounts { get; set; } = new();
    }
}