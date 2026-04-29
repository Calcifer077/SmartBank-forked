using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmartBank.API.DTOs.Auth;
using SmartBank.Data;
using SmartBank.Models.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SmartBank.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly SmartBankDbContext _db;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthService> _logger;

        public AuthService(SmartBankDbContext db, IConfiguration config, ILogger<AuthService> logger)
        {
            _db = db;
            _config = config;
            _logger = logger;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto)
        {
            // Duplicate email check
            bool emailExists = await _db.Users.AnyAsync(u => u.Email == dto.Email);
            if (emailExists)
                throw new InvalidOperationException("Email is already registered.");

            // Get Customer role (default for self-registration)
            var customerRole = await _db.Roles.FirstOrDefaultAsync(r => r.RoleName == "Customer")
                ?? throw new InvalidOperationException("Customer role not found. Check role seeding.");

            // Hash password
            string hash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = hash,
                PhoneNumber = dto.PhoneNumber,
                Address = dto.Address,
                RoleId = customerRole.RoleId,
                KycStatus = "Pending",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            _logger.LogInformation("New user registered: {Email}", dto.Email);

            return new AuthResponseDto
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Role = customerRole.RoleName,
                Token = GenerateJwt(user, customerRole.RoleName)
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto dto)
        {
            // Find user with role
            var user = await _db.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                _logger.LogWarning("Failed login attempt for: {Email}", dto.Email);
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            if (!user.IsActive)
                throw new UnauthorizedAccessException("Account is frozen. Contact support.");

            _logger.LogInformation("User logged in: {Email}", dto.Email);

            return new AuthResponseDto
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.RoleName,
                Token = GenerateJwt(user, user.Role.RoleName)
            };
        }

        // ── JWT Generation ────────────────────────────────────────────────
        private string GenerateJwt(User user, string roleName)
        {
            var secretKey = _config["JwtSettings:SecretKey"]
                ?? throw new InvalidOperationException("JWT SecretKey is not configured.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, roleName)
            };

            int expiryMinutes = int.Parse(_config["JwtSettings:ExpiryMinutes"] ?? "60");

            var token = new JwtSecurityToken(
                issuer: _config["JwtSettings:Issuer"],
                audience: _config["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}