using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using SmartBank.API.DTOs.Auth;
using SmartBank.API.Services;
using SmartBank.Tests.Helpers;

namespace SmartBank.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IConfiguration> _configMock;
        private readonly Mock<ILogger<AuthService>> _loggerMock;

        public AuthServiceTests()
        {
            _configMock = new Mock<IConfiguration>();
            _loggerMock = new Mock<ILogger<AuthService>>();

            // Mock JWT config sections
            _configMock.Setup(c => c["JwtSettings:SecretKey"])
                .Returns("SuperSecretTestKey_1234567890_ABCDE");
            _configMock.Setup(c => c["JwtSettings:Issuer"])
                .Returns("SmartBank");
            _configMock.Setup(c => c["JwtSettings:Audience"])
                .Returns("SmartBankUsers");
            _configMock.Setup(c => c["JwtSettings:ExpiryMinutes"])
                .Returns("60");
        }

        // ── Register Tests ────────────────────────────────────────────

        [Fact]
        public async Task Register_WithValidData_ShouldReturnToken()
        {
            // Arrange
            var context = DbContextHelper.GetInMemoryContext("auth_register_valid");
            SeedHelper.SeedRoles(context);
            var service = new AuthService(context, _configMock.Object, _loggerMock.Object);

            var dto = new RegisterRequestDto
            {
                FullName = "John Doe",
                Email = "john@example.com",
                Password = "Password1",
                PhoneNumber = "9876543210",
                Address = "123 Main St"
            };

            // Act
            var result = await service.RegisterAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.Token.Should().NotBeNullOrEmpty();
            result.Email.Should().Be(dto.Email);
            result.Role.Should().Be("Customer");
        }

        [Fact]
        public async Task Register_WithDuplicateEmail_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var context = DbContextHelper.GetInMemoryContext("auth_register_duplicate");
            SeedHelper.SeedCustomerUser(context);
            var service = new AuthService(context, _configMock.Object, _loggerMock.Object);

            var dto = new RegisterRequestDto
            {
                FullName = "Another User",
                Email = "test@example.com", // already exists
                Password = "Password1",
                PhoneNumber = "9876543210"
            };

            // Act
            var act = async () => await service.RegisterAsync(dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*already registered*");
        }

        // ── Login Tests ───────────────────────────────────────────────

        [Fact]
        public async Task Login_WithValidCredentials_ShouldReturnToken()
        {
            // Arrange
            var context = DbContextHelper.GetInMemoryContext("auth_login_valid");
            SeedHelper.SeedCustomerUser(context);
            var service = new AuthService(context, _configMock.Object, _loggerMock.Object);

            var dto = new LoginRequestDto
            {
                Email = "test@example.com",
                Password = "Password1"
            };

            // Act
            var result = await service.LoginAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.Token.Should().NotBeNullOrEmpty();
            result.Email.Should().Be(dto.Email);
        }

        [Fact]
        public async Task Login_WithWrongPassword_ShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            var context = DbContextHelper.GetInMemoryContext("auth_login_wrong_password");
            SeedHelper.SeedCustomerUser(context);
            var service = new AuthService(context, _configMock.Object, _loggerMock.Object);

            var dto = new LoginRequestDto
            {
                Email = "test@example.com",
                Password = "WrongPassword"
            };

            // Act
            var act = async () => await service.LoginAsync(dto);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("*Invalid email or password*");
        }

        [Fact]
        public async Task Login_WithNonExistentEmail_ShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            var context = DbContextHelper.GetInMemoryContext("auth_login_no_user");
            SeedHelper.SeedRoles(context);
            var service = new AuthService(context, _configMock.Object, _loggerMock.Object);

            var dto = new LoginRequestDto
            {
                Email = "nobody@example.com",
                Password = "Password1"
            };

            // Act
            var act = async () => await service.LoginAsync(dto);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        [Fact]
        public async Task Login_WithFrozenAccount_ShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            var context = DbContextHelper.GetInMemoryContext("auth_login_frozen");
            var user = SeedHelper.SeedCustomerUser(context);
            user.IsActive = false;
            context.SaveChanges();

            var service = new AuthService(context, _configMock.Object, _loggerMock.Object);

            var dto = new LoginRequestDto
            {
                Email = "test@example.com",
                Password = "Password1"
            };

            // Act
            var act = async () => await service.LoginAsync(dto);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("*frozen*");
        }
    }
}