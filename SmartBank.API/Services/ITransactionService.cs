using SmartBank.API.DTOs.Transactions;

namespace SmartBank.API.Services
{
    public interface ITransactionService
    {
        Task<TransactionResponseDto> DepositAsync(int userId, DepositWithdrawDto dto);
        Task<TransactionResponseDto> WithdrawAsync(int userId, DepositWithdrawDto dto);
        Task<TransferResponseDto> TransferAsync(int userId, TransferDto dto);
        Task<List<TransactionResponseDto>> GetHistoryAsync(int userId, int accountId);
    }
}