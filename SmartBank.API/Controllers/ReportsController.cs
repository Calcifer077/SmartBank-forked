using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBank.API.DTOs.Reports;
using SmartBank.API.Helpers;
using SmartBank.API.Services;

namespace SmartBank.API.Controllers
{
    [ApiController]
    [Route("api/reports")]
    [Authorize(Roles = "Admin,Auditor")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        // GET /api/reports/daily?date=2024-01-15
        [HttpGet("daily")]
        public async Task<IActionResult> Daily([FromQuery] DateTime? date)
        {
            var targetDate = date ?? DateTime.UtcNow;
            var result = await _reportService.GetDailyTransactionsAsync(targetDate);
            return Ok(ApiResponse<List<DailyTransactionReportDto>>.Ok(result));
        }

        // GET /api/reports/loans
        [HttpGet("loans")]
        public async Task<IActionResult> Loans()
        {
            var result = await _reportService.GetLoanReportAsync();
            return Ok(ApiResponse<List<LoanReportDto>>.Ok(result));
        }

        // GET /api/reports/active-customers
        [HttpGet("active-customers")]
        public async Task<IActionResult> ActiveCustomers()
        {
            var result = await _reportService.GetActiveCustomersAsync();
            return Ok(ApiResponse<List<ActiveCustomersReportDto>>.Ok(result));
        }

        // GET /api/reports/low-balance?threshold=1000
        [HttpGet("low-balance")]
        public async Task<IActionResult> LowBalance([FromQuery] decimal threshold = 1000)
        {
            var result = await _reportService.GetLowBalanceAccountsAsync(threshold);
            return Ok(ApiResponse<List<LowBalanceReportDto>>.Ok(result));
        }
    }
}