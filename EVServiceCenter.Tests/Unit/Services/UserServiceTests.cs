using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using AutoMapper;
using EVServiceCenter.Infrastructure.Domains.Identity.Services;
using EVServiceCenter.Core.Domains.Identity.Interfaces;
using EVServiceCenter.Core.Domains.Identity.Entities;
using EVServiceCenter.Core.Domains.Identity.DTOs.Responses;
using EVServiceCenter.Core.Enums;
using EVServiceCenter.Core.Helpers;
using EVServiceCenter.Core.Constants;
using EVServiceCenter.Core.Domains.Shared.Interfaces;
using System.Text;
using Xunit;

namespace EVServiceCenter.Tests.Unit.Services;

/// <summary>
/// Unit tests for UserService
/// </summary>
public class UserServiceTests
{
    private readonly Mock<IUserRepository> _mockRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<UserService>> _mockLogger;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly Mock<IHttpContextService> _mockHttpContextService;
    private readonly UserService _service;

    public UserServiceTests()
    {
        _mockRepository = new Mock<IUserRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<UserService>>();
        _mockEmailService = new Mock<IEmailService>();
        _mockHttpContextService = new Mock<IHttpContextService>();
        _service = new UserService(
            _mockRepository.Object,
            _mockMapper.Object,
            _mockLogger.Object,
            _mockEmailService.Object,
            _mockHttpContextService.Object);
    }

    [Fact]
    public async Task RegisterCustomerUserAsync_WithValidData_ShouldReturnUserResponse()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            FullName = "Test User",
            Email = "test@example.com",
            RoleId = (int)UserRoles.Customer
        };
        var password = "Test123";

        var createdUser = new User
        {
            UserId = 1,
            Username = "testuser",
            FullName = "Test User",
            Email = "test@example.com",
            RoleId = (int)UserRoles.Customer
        };

        var expectedResponse = new UserResponseDto
        {
            UserId = 1,
            Username = "testuser",
            FullName = "Test User",
            Email = "test@example.com",
            RoleId = (int)UserRoles.Customer
        };

        _mockRepository
            .Setup(x => x.IsUsernameExistsAsync("testuser"))
            .ReturnsAsync(false);

        _mockRepository
            .Setup(x => x.IsEmailExistsAsync("test@example.com"))
            .ReturnsAsync(false);

        _mockRepository
            .Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdUser);

        _mockMapper
            .Setup(x => x.Map<UserResponseDto>(createdUser))
            .Returns(expectedResponse);

        // Act
        var result = await _service.RegisterCustomerUserAsync(user, password);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedResponse);
        _mockRepository.Verify(x => x.IsUsernameExistsAsync("testuser"), Times.Once);
        _mockRepository.Verify(x => x.IsEmailExistsAsync("test@example.com"), Times.Once);
        _mockRepository.Verify(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterCustomerUserAsync_WithInvalidRole_ShouldThrowArgumentException()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            RoleId = (int)UserRoles.Admin // Wrong role for customer registration
        };
        var password = "Test123";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.RegisterCustomerUserAsync(user, password));
        _mockRepository.Verify(x => x.IsUsernameExistsAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RegisterCustomerUserAsync_WithWeakPassword_ShouldThrowArgumentException()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            RoleId = (int)UserRoles.Customer
        };
        var weakPassword = "123"; // Too short

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.RegisterCustomerUserAsync(user, weakPassword));
        _mockRepository.Verify(x => x.IsUsernameExistsAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RegisterCustomerUserAsync_WithDuplicateUsername_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var user = new User
        {
            Username = "existinguser",
            RoleId = (int)UserRoles.Customer
        };
        var password = "Test123";

        _mockRepository
            .Setup(x => x.IsUsernameExistsAsync("existinguser"))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.RegisterCustomerUserAsync(user, password));
        _mockRepository.Verify(x => x.IsUsernameExistsAsync("existinguser"), Times.Once);
        _mockRepository.Verify(x => x.IsEmailExistsAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RegisterCustomerUserAsync_WithDuplicateEmail_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            Email = "existing@example.com",
            RoleId = (int)UserRoles.Customer
        };
        var password = "Test123";

        _mockRepository
            .Setup(x => x.IsUsernameExistsAsync("testuser"))
            .ReturnsAsync(false);

        _mockRepository
            .Setup(x => x.IsEmailExistsAsync("existing@example.com"))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.RegisterCustomerUserAsync(user, password));
        _mockRepository.Verify(x => x.IsUsernameExistsAsync("testuser"), Times.Once);
        _mockRepository.Verify(x => x.IsEmailExistsAsync("existing@example.com"), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnUser()
    {
        // Arrange
        var username = "testuser";
        var password = "Test123";
        var hashedPassword = SecurityHelper.HashPassword(password, SecurityHelper.GenerateSalt());

        var user = new User
        {
            UserId = 1,
            Username = username,
            PasswordHash = Encoding.UTF8.GetBytes(hashedPassword),
            IsActive = true,
            EmailVerified = true
        };

        var expectedResponse = new UserResponseDto
        {
            UserId = 1,
            Username = username
        };

        _mockRepository
            .Setup(x => x.GetByUsernameAsync(username))
            .ReturnsAsync(user);

        _mockRepository
            .Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockMapper
            .Setup(x => x.Map<UserResponseDto>(user))
            .Returns(expectedResponse);

        _mockHttpContextService
            .Setup(x => x.GetClientIpAddress())
            .Returns("127.0.0.1");

        _mockHttpContextService
            .Setup(x => x.GetUserAgent())
            .Returns("TestAgent");

        // Act
        var result = await _service.LoginAsync(username, password);

        // Assert
        result.User.Should().NotBeNull();
        result.User.Should().BeEquivalentTo(expectedResponse);
        result.ErrorCode.Should().BeNull();
        result.ErrorMessage.Should().BeNull();
        _mockRepository.Verify(x => x.GetByUsernameAsync(username), Times.Once);
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidUsername_ShouldReturnError()
    {
        // Arrange
        var username = "nonexistent";
        var password = "Test123";

        _mockRepository
            .Setup(x => x.GetByUsernameAsync(username))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _service.LoginAsync(username, password);

        // Assert
        result.User.Should().BeNull();
        result.ErrorCode.Should().Be(ErrorCodes.INVALID_CREDENTIALS);
        result.ErrorMessage.Should().Be(ErrorMessages.INVALID_USERNAME_PASSWORD);
        _mockRepository.Verify(x => x.GetByUsernameAsync(username), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithInactiveAccount_ShouldReturnError()
    {
        // Arrange
        var username = "testuser";
        var password = "Test123";

        var user = new User
        {
            UserId = 1,
            Username = username,
            IsActive = false
        };

        _mockRepository
            .Setup(x => x.GetByUsernameAsync(username))
            .ReturnsAsync(user);

        // Act
        var result = await _service.LoginAsync(username, password);

        // Assert
        result.User.Should().BeNull();
        result.ErrorCode.Should().Be(ErrorCodes.ACCOUNT_LOCKED);
        result.ErrorMessage.Should().Contain("vô hiệu hóa");
        _mockRepository.Verify(x => x.GetByUsernameAsync(username), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ShouldIncrementFailedAttempts()
    {
        // Arrange
        var username = "testuser";
        var password = "WrongPassword";
        var correctPassword = "Test123";
        var hashedPassword = SecurityHelper.HashPassword(correctPassword, SecurityHelper.GenerateSalt());

        var user = new User
        {
            UserId = 1,
            Username = username,
            PasswordHash = Encoding.UTF8.GetBytes(hashedPassword),
            IsActive = true,
            EmailVerified = true,
            FailedLoginAttempts = 0
        };

        _mockRepository
            .Setup(x => x.GetByUsernameAsync(username))
            .ReturnsAsync(user);

        _mockRepository
            .Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _service.LoginAsync(username, password);

        // Assert
        result.User.Should().BeNull();
        result.ErrorCode.Should().Be(ErrorCodes.INVALID_CREDENTIALS);
        result.ErrorMessage.Should().Contain("Bạn còn 4 lần thử");
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithTooManyFailedAttempts_ShouldLockAccount()
    {
        // Arrange
        var username = "testuser";
        var password = "WrongPassword";
        var correctPassword = "Test123";
        var hashedPassword = SecurityHelper.HashPassword(correctPassword, SecurityHelper.GenerateSalt());

        var user = new User
        {
            UserId = 1,
            Username = username,
            PasswordHash = Encoding.UTF8.GetBytes(hashedPassword),
            IsActive = true,
            EmailVerified = true,
            FailedLoginAttempts = 4 // One more will lock it
        };

        _mockRepository
            .Setup(x => x.GetByUsernameAsync(username))
            .ReturnsAsync(user);

        _mockRepository
            .Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _service.LoginAsync(username, password);

        // Assert
        result.User.Should().BeNull();
        result.ErrorCode.Should().Be(ErrorCodes.ACCOUNT_LOCKED);
        result.ErrorMessage.Should().Contain("30 phút");
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithUnverifiedEmail_ShouldReturnError()
    {
        // Arrange
        var username = "testuser";
        var password = "Test123";
        var hashedPassword = SecurityHelper.HashPassword(password, SecurityHelper.GenerateSalt());

        var user = new User
        {
            UserId = 1,
            Username = username,
            PasswordHash = Encoding.UTF8.GetBytes(hashedPassword),
            IsActive = true,
            EmailVerified = false,
            Email = "test@example.com"
        };

        _mockRepository
            .Setup(x => x.GetByUsernameAsync(username))
            .ReturnsAsync(user);

        _mockRepository
            .Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _service.LoginAsync(username, password);

        // Assert
        result.User.Should().BeNull();
        result.ErrorCode.Should().Be(ErrorCodes.EMAIL_NOT_VERIFIED);
        result.ErrorMessage.Should().Be(ErrorMessages.EMAIL_NOT_VERIFIED);
        _mockRepository.Verify(x => x.GetByUsernameAsync(username), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task LoginAsync_WithEmptyUsername_ShouldReturnValidationError(string username)
    {
        // Act
        var result = await _service.LoginAsync(username, "password");

        // Assert
        result.User.Should().BeNull();
        result.ErrorCode.Should().Be(ErrorCodes.VALIDATION_ERROR);
        result.ErrorMessage.Should().Contain("không được để trống");
        _mockRepository.Verify(x => x.GetByUsernameAsync(It.IsAny<string>()), Times.Never);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task LoginAsync_WithEmptyPassword_ShouldReturnValidationError(string password)
    {
        // Arrange
        var username = "testuser";

        // Act
        var result = await _service.LoginAsync(username, password);

        // Assert
        result.User.Should().BeNull();
        result.ErrorCode.Should().Be(ErrorCodes.VALIDATION_ERROR);
        result.ErrorMessage.Should().Contain("không được để trống");
        _mockRepository.Verify(x => x.GetByUsernameAsync(username), Times.Never);
    }

    [Fact]
    public async Task UpdateUserPasswordAsync_WithValidCurrentPassword_ShouldUpdatePassword()
    {
        // Arrange
        var userId = 1;
        var currentPassword = "OldPassword123";
        var newPassword = "NewPassword123";
        var salt = SecurityHelper.GenerateSalt();
        var currentHash = SecurityHelper.HashPassword(currentPassword, salt);

        var user = new User
        {
            UserId = userId,
            PasswordHash = Encoding.UTF8.GetBytes(currentHash)
        };

        _mockRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockRepository
            .Setup(x => x.UpdatePasswordAsync(userId, It.IsAny<byte[]>(), It.IsAny<byte[]>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.UpdateUserPasswordAsync(userId, currentPassword, newPassword);

        // Assert
        _mockRepository.Verify(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(x => x.UpdatePasswordAsync(userId, It.IsAny<byte[]>(), It.IsAny<byte[]>()), Times.Once);
    }

    [Fact]
    public async Task UpdateUserPasswordAsync_WithInvalidCurrentPassword_ShouldThrowException()
    {
        // Arrange
        var userId = 1;
        var currentPassword = "WrongPassword";
        var newPassword = "NewPassword123";
        var salt = SecurityHelper.GenerateSalt();
        var correctHash = SecurityHelper.HashPassword("CorrectPassword", salt);

        var user = new User
        {
            UserId = userId,
            PasswordHash = Encoding.UTF8.GetBytes(correctHash)
        };

        _mockRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _service.UpdateUserPasswordAsync(userId, currentPassword, newPassword));
        _mockRepository.Verify(x => x.UpdatePasswordAsync(It.IsAny<int>(), It.IsAny<byte[]>(), It.IsAny<byte[]>()), Times.Never);
    }

    [Fact]
    public async Task UpdateUserPasswordAsync_WithWeakNewPassword_ShouldThrowException()
    {
        // Arrange
        var userId = 1;
        var currentPassword = "OldPassword123";
        var newPassword = "123"; // Too weak
        var salt = SecurityHelper.GenerateSalt();
        var currentHash = SecurityHelper.HashPassword(currentPassword, salt);

        var user = new User
        {
            UserId = userId,
            PasswordHash = Encoding.UTF8.GetBytes(currentHash)
        };

        _mockRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.UpdateUserPasswordAsync(userId, currentPassword, newPassword));
        _mockRepository.Verify(x => x.UpdatePasswordAsync(It.IsAny<int>(), It.IsAny<byte[]>(), It.IsAny<byte[]>()), Times.Never);
    }

    [Fact]
    public async Task ForgotPasswordAsync_WithValidEmail_ShouldReturnTrue()
    {
        // Arrange
        var email = "test@example.com";
        var user = new User
        {
            UserId = 1,
            Email = email,
            FullName = "Test User",
            IsActive = true
        };

        _mockRepository
            .Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync(user);

        _mockRepository
            .Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockEmailService
            .Setup(x => x.SendPasswordResetAsync(email, user.FullName, It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        _mockHttpContextService
            .Setup(x => x.GetClientIpAddress())
            .Returns("127.0.0.1");

        // Act
        var result = await _service.ForgotPasswordAsync(email);

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(x => x.GetByEmailAsync(email), Times.Once);
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockEmailService.Verify(x => x.SendPasswordResetAsync(email, user.FullName, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ForgotPasswordAsync_WithNonExistentEmail_ShouldReturnTrueForSecurity()
    {
        // Arrange
        var email = "nonexistent@example.com";

        _mockRepository
            .Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync((User?)null);

        _mockHttpContextService
            .Setup(x => x.GetClientIpAddress())
            .Returns("127.0.0.1");

        // Act
        var result = await _service.ForgotPasswordAsync(email);

        // Assert
        result.Should().BeTrue(); // Returns true for security (prevent email enumeration)
        _mockRepository.Verify(x => x.GetByEmailAsync(email), Times.Once);
        _mockEmailService.Verify(x => x.SendPasswordResetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task ForgotPasswordAsync_WithEmptyEmail_ShouldThrowArgumentException(string email)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.ForgotPasswordAsync(email));
        _mockRepository.Verify(x => x.GetByEmailAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ResetPasswordAsync_WithValidToken_ShouldReturnTrue()
    {
        // Arrange
        var email = "test@example.com";
        var token = "valid-token";
        var newPassword = "NewPassword123";
        var salt = SecurityHelper.GenerateSalt();
        var currentHash = SecurityHelper.HashPassword("OldPassword", salt);

        var user = new User
        {
            UserId = 1,
            Email = email,
            FullName = "Test User",
            IsActive = true,
            ResetToken = Encoding.UTF8.GetBytes(token),
            ResetTokenExpiry = DateTime.UtcNow.AddHours(1),
            PasswordHash = Encoding.UTF8.GetBytes(currentHash)
        };

        _mockRepository
            .Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync(user);

        _mockRepository
            .Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockEmailService
            .Setup(x => x.SendNotificationAsync(email, It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.ResetPasswordAsync(email, token, newPassword);

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(x => x.GetByEmailAsync(email), Times.Once);
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ResetPasswordAsync_WithInvalidToken_ShouldReturnFalse()
    {
        // Arrange
        var email = "test@example.com";
        var token = "invalid-token";
        var newPassword = "NewPassword123";

        var user = new User
        {
            UserId = 1,
            Email = email,
            ResetToken = Encoding.UTF8.GetBytes("different-token"),
            ResetTokenExpiry = DateTime.UtcNow.AddHours(1)
        };

        _mockRepository
            .Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync(user);

        // Act
        var result = await _service.ResetPasswordAsync(email, token, newPassword);

        // Assert
        result.Should().BeFalse();
        _mockRepository.Verify(x => x.GetByEmailAsync(email), Times.Once);
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ResetPasswordAsync_WithExpiredToken_ShouldReturnFalse()
    {
        // Arrange
        var email = "test@example.com";
        var token = "expired-token";
        var newPassword = "NewPassword123";

        var user = new User
        {
            UserId = 1,
            Email = email,
            ResetToken = Encoding.UTF8.GetBytes(token),
            ResetTokenExpiry = DateTime.UtcNow.AddHours(-1) // Expired
        };

        _mockRepository
            .Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync(user);

        // Act
        var result = await _service.ResetPasswordAsync(email, token, newPassword);

        // Assert
        result.Should().BeFalse();
        _mockRepository.Verify(x => x.GetByEmailAsync(email), Times.Once);
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task VerifyEmailAsync_WithValidToken_ShouldReturnTrue()
    {
        // Arrange
        var email = "test@example.com";
        var token = "valid-token";

        var user = new User
        {
            UserId = 1,
            Email = email,
            FullName = "Test User",
            EmailVerificationToken = Encoding.UTF8.GetBytes(token),
            EmailVerificationExpiry = DateTime.UtcNow.AddHours(1)
        };

        _mockRepository
            .Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync(user);

        _mockRepository
            .Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockEmailService
            .Setup(x => x.SendWelcomeEmailAsync(email, user.FullName))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.VerifyEmailAsync(email, token);

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(x => x.GetByEmailAsync(email), Times.Once);
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockEmailService.Verify(x => x.SendWelcomeEmailAsync(email, user.FullName), Times.Once);
    }

    [Fact]
    public async Task VerifyEmailAsync_WithInvalidToken_ShouldReturnFalse()
    {
        // Arrange
        var email = "test@example.com";
        var token = "invalid-token";

        var user = new User
        {
            UserId = 1,
            Email = email,
            EmailVerificationToken = Encoding.UTF8.GetBytes("different-token"),
            EmailVerificationExpiry = DateTime.UtcNow.AddHours(1)
        };

        _mockRepository
            .Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync(user);

        // Act
        var result = await _service.VerifyEmailAsync(email, token);

        // Assert
        result.Should().BeFalse();
        _mockRepository.Verify(x => x.GetByEmailAsync(email), Times.Once);
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
