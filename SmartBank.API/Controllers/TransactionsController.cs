using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBank.API.DTOs.Transactions;
using SmartBank.API.Helpers;
using SmartBank.API.Services;
using System.Security.Claims;

namespace SmartBank.API.Controllers
{
    [ApiController]
    [Route("api/transactions")]
    [Authorize]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _txService;
        private readonly IValidator<DepositWithdrawDto> _dwValidator;
        private readonly IValidator<TransferDto> _transferValidator;

        public TransactionsController(
            ITransactionService txService,
            IValidator<DepositWithdrawDto> dwValidator,
            IValidator<TransferDto> transferValidator)
        {
            _txService = txService;
            _dwValidator = dwValidator;
            _transferValidator = transferValidator;
        }

        // POST /api/transactions/deposit
        [HttpPost("deposit")]
        public async Task<IActionResult> Deposit([FromBody] DepositWithdrawDto dto)
        {
            var validation = await _dwValidator.ValidateAsync(dto);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(
                    string.Join(", ", validation.Errors.Select(e => e.ErrorMessage))));

            var result = await _txService.DepositAsync(GetUserId(), dto);
            return Ok(ApiResponse<TransactionResponseDto>.Ok(result, "Deposit successful."));
        }

        // POST /api/transactions/withdraw
        [HttpPost("withdraw")]
        public async Task<IActionResult> Withdraw([FromBody] DepositWithdrawDto dto)
        {
            var validation = await _dwValidator.ValidateAsync(dto);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(
                    string.Join(", ", validation.Errors.Select(e => e.ErrorMessage))));

            var result = await _txService.WithdrawAsync(GetUserId(), dto);
            return Ok(ApiResponse<TransactionResponseDto>.Ok(result, "Withdrawal successful."));
        }

        // POST /api/transactions/transfer
        [HttpPost("transfer")]
        public async Task<IActionResult> Transfer([FromBody] TransferDto dto)
        {
            var validation = await _transferValidator.ValidateAsync(dto);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(
                    string.Join(", ", validation.Errors.Select(e => e.ErrorMessage))));

            var result = await _txService.TransferAsync(GetUserId(), dto);
            return Ok(ApiResponse<TransferResponseDto>.Ok(result, "Transfer successful."));
        }

        // GET /api/transactions/history/{accountId}
        [HttpGet("history/{accountId:int}")]
        public async Task<IActionResult> History(int accountId)
        {
            var result = await _txService.GetHistoryAsync(GetUserId(), accountId);
            return Ok(ApiResponse<List<TransactionResponseDto>>.Ok(result));
        }

        // ── Helper ────────────────────────────────────────────────────
        private int GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(claim, out int userId))
                throw new UnauthorizedAccessException("Invalid token.");
            return userId;
        }
    }
}