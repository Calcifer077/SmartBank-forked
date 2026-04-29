using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBank.API.DTOs.Accounts;
using SmartBank.API.Helpers;
using SmartBank.API.Services;
using System.Security.Claims;

namespace SmartBank.API.Controllers
{
    [ApiController]
    [Route("api/accounts")]
    [Authorize]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly IValidator<AccountCreateDto> _validator;

        public AccountsController(
            IAccountService accountService,
            IValidator<AccountCreateDto> validator)
        {
            _accountService = accountService;
            _validator = validator;
        }

        // GET /api/accounts
        [HttpGet]
        public async Task<IActionResult> GetMyAccounts()
        {
            int userId = GetUserId();
            var accounts = await _accountService.GetUserAccountsAsync(userId);
            return Ok(ApiResponse<List<AccountSummaryDto>>.Ok(accounts));
        }

        // GET /api/accounts/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetDetail(int id)
        {
            int userId = GetUserId();
            var account = await _accountService.GetAccountDetailAsync(id, userId);
            return Ok(ApiResponse<AccountResponseDto>.Ok(account));
        }

        // POST /api/accounts/create
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] AccountCreateDto dto)
        {
            var validation = await _validator.ValidateAsync(dto);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(
                    string.Join(", ", validation.Errors.Select(e => e.ErrorMessage))));

            int userId = GetUserId();
            var account = await _accountService.CreateAccountAsync(userId, dto);
            return Ok(ApiResponse<AccountResponseDto>.Ok(account, "Account opened successfully."));
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