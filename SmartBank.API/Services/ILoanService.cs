using SmartBank.API.DTOs.Loans;

namespace SmartBank.API.Services
{
    public interface ILoanService
    {
        Task<LoanResponseDto> ApplyAsync(int userId, LoanApplyDto dto);
        Task<List<LoanResponseDto>> GetMyLoansAsync(int userId);
        Task<LoanResponseDto> GetDetailAsync(int loanId, int userId);
        Task<List<LoanResponseDto>> GetAllLoansAsync();         // Admin
        Task<LoanResponseDto> ReviewLoanAsync(LoanReviewDto dto); // Admin
    }
}