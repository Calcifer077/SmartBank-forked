namespace SmartBank.API.DTOs.Admin
{
    public class AdminDashboardDto
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalAccounts { get; set; }
        public int FrozenAccounts { get; set; }
        public int PendingLoans { get; set; }
        public int OpenTickets { get; set; }
        public decimal TotalDepositsToday { get; set; }
        public decimal TotalWithdrawalsToday { get; set; }
        public int TransactionsToday { get; set; }
    }
}