using SmartBank.API.DTOs.Accounts;

namespace SmartBank.API.Services
{
    public interface IAccountService
    {
        Task<AccountResponseDto> CreateAccountAsync(int userId, AccountCreateDto dto);
        Task<List<AccountSummaryDto>> GetUserAccountsAsync(int userId);
        Task<AccountResponseDto> GetAccountDetailAsync(int accountId, int userId);
    }
}