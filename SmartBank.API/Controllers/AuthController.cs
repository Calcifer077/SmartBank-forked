using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SmartBank.API.DTOs.Auth;
using SmartBank.API.Helpers;
using SmartBank.API.Services;

namespace SmartBank.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IValidator<RegisterRequestDto> _registerValidator;
        private readonly IValidator<LoginRequestDto> _loginValidator;

        public AuthController(
            IAuthService authService,
            IValidator<RegisterRequestDto> registerValidator,
            IValidator<LoginRequestDto> loginValidator)
        {
            _authService = authService;
            _registerValidator = registerValidator;
            _loginValidator = loginValidator;
        }

        // POST /api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
        {
            var validation = await _registerValidator.ValidateAsync(dto);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(
                    string.Join(", ", validation.Errors.Select(e => e.ErrorMessage))));

            var result = await _authService.RegisterAsync(dto);
            return Ok(ApiResponse<AuthResponseDto>.Ok(result, "Registration successful."));
        }

        // POST /api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            var validation = await _loginValidator.ValidateAsync(dto);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(
                    string.Join(", ", validation.Errors.Select(e => e.ErrorMessage))));

            var result = await _authService.LoginAsync(dto);
            return Ok(ApiResponse<AuthResponseDto>.Ok(result, "Login successful."));
        }
    }
}