using SmartBank.API.DTOs.Reports;

namespace SmartBank.API.Services
{
    public interface IReportService
    {
        Task<List<DailyTransactionReportDto>> GetDailyTransactionsAsync(DateTime date);
        Task<List<LoanReportDto>> GetLoanReportAsync();
        Task<List<ActiveCustomersReportDto>> GetActiveCustomersAsync();
        Task<List<LowBalanceReportDto>> GetLowBalanceAccountsAsync(decimal threshold);
    }
}