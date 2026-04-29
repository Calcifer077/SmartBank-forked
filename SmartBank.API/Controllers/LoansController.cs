using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBank.API.DTOs.Loans;
using SmartBank.API.Helpers;
using SmartBank.API.Services;
using System.Security.Claims;

namespace SmartBank.API.Controllers
{
    [ApiController]
    [Route("api/loans")]
    [Authorize]
    public class LoansController : ControllerBase
    {
        private readonly ILoanService _loanService;
        private readonly IValidator<LoanApplyDto> _validator;

        public LoansController(ILoanService loanService, IValidator<LoanApplyDto> validator)
        {
            _loanService = loanService;
            _validator = validator;
        }

        // POST /api/loans/apply
        [HttpPost("apply")]
        public async Task<IActionResult> Apply([FromBody] LoanApplyDto dto)
        {
            var validation = await _validator.ValidateAsync(dto);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(
                    string.Join(", ", validation.Errors.Select(e => e.ErrorMessage))));

            var result = await _loanService.ApplyAsync(GetUserId(), dto);
            return Ok(ApiResponse<LoanResponseDto>.Ok(result, "Loan application submitted."));
        }

        // GET /api/loans/my
        [HttpGet("my")]
        public async Task<IActionResult> MyLoans()
        {
            var result = await _loanService.GetMyLoansAsync(GetUserId());
            return Ok(ApiResponse<List<LoanResponseDto>>.Ok(result));
        }

        // GET /api/loans/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Detail(int id)
        {
            var result = await _loanService.GetDetailAsync(id, GetUserId());
            return Ok(ApiResponse<LoanResponseDto>.Ok(result));
        }

        // GET /api/loans/all  — Admin/Manager only
        [HttpGet("all")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> AllLoans()
        {
            var result = await _loanService.GetAllLoansAsync();
            return Ok(ApiResponse<List<LoanResponseDto>>.Ok(result));
        }

        // POST /api/loans/review  — Admin/Manager only
        [HttpPost("review")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Review([FromBody] LoanReviewDto dto)
        {
            var result = await _loanService.ReviewLoanAsync(dto);
            return Ok(ApiResponse<LoanResponseDto>.Ok(
                result, $"Loan {dto.Decision} successfully."));
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