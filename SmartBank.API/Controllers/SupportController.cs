using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBank.API.DTOs.Support;
using SmartBank.API.Helpers;
using SmartBank.API.Services;
using System.Security.Claims;

namespace SmartBank.API.Controllers
{
    [ApiController]
    [Route("api/tickets")]
    [Authorize]
    public class SupportController : ControllerBase
    {
        private readonly ISupportService _supportService;
        private readonly IValidator<TicketCreateDto> _validator;

        public SupportController(
            ISupportService supportService,
            IValidator<TicketCreateDto> validator)
        {
            _supportService = supportService;
            _validator = validator;
        }

        // POST /api/tickets/create
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] TicketCreateDto dto)
        {
            var validation = await _validator.ValidateAsync(dto);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(
                    string.Join(", ", validation.Errors.Select(e => e.ErrorMessage))));

            var result = await _supportService.CreateTicketAsync(GetUserId(), dto);
            return Ok(ApiResponse<TicketResponseDto>.Ok(result, "Ticket raised successfully."));
        }

        // GET /api/tickets/my
        [HttpGet("my")]
        public async Task<IActionResult> MyTickets()
        {
            var result = await _supportService.GetMyTicketsAsync(GetUserId());
            return Ok(ApiResponse<List<TicketResponseDto>>.Ok(result));
        }

        // GET /api/tickets/all  — Admin/Teller only
        [HttpGet("all")]
        [Authorize(Roles = "Admin,Teller")]
        public async Task<IActionResult> AllTickets()
        {
            var result = await _supportService.GetAllTicketsAsync();
            return Ok(ApiResponse<List<TicketResponseDto>>.Ok(result));
        }

        // POST /api/tickets/resolve/{id}  — Admin/Teller only
        [HttpPost("resolve/{id:int}")]
        [Authorize(Roles = "Admin,Teller")]
        public async Task<IActionResult> Resolve(int id)
        {
            var result = await _supportService.ResolveTicketAsync(id);
            return Ok(ApiResponse<TicketResponseDto>.Ok(result, "Ticket resolved."));
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