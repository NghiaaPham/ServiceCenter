using FluentAssertions;
using FluentValidation;
using EVServiceCenter.API.Validators;
using EVServiceCenter.Core.Domains.Identity.DTOs.Requests;
using EVServiceCenter.Core.Enums;
using EVServiceCenter.Core.Constants;
using Xunit;

namespace EVServiceCenter.Tests.Unit.Validators;

/// <summary>
/// Unit tests for RegisterRequestValidator
/// </summary>
public class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _validator;

    public RegisterRequestValidatorTests()
    {
        _validator = new RegisterRequestValidator();
    }

    [Fact]
    public void Validate_WithValidRequest_ShouldPass()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Username = "testuser",
            Password = "Test123",
            FullName = "Test User",
            Email = "test@example.com",
            PhoneNumber = "0913456789",
            RoleId = (int)UserRoles.Customer
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyUsername_ShouldFail()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Username = "",
            Password = "Test123",
            FullName = "Test User",
            RoleId = (int)UserRoles.Customer
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Username" && e.ErrorMessage.Contains("required"));
    }

    [Fact]
    public void Validate_WithUsernameTooShort_ShouldFail()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Username = "ab", // Too short
            Password = "Test123",
            FullName = "Test User",
            RoleId = (int)UserRoles.Customer
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Username" && e.ErrorMessage.Contains("between"));
    }

    [Fact]
    public void Validate_WithUsernameTooLong_ShouldFail()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Username = new string('a', SystemConstants.USERNAME_MAX_LENGTH + 1), // Too long
            Password = "Test123",
            FullName = "Test User",
            RoleId = (int)UserRoles.Customer
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Username" && e.ErrorMessage.Contains("between"));
    }

    [Fact]
    public void Validate_WithEmptyPassword_ShouldFail()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Username = "testuser",
            Password = "",
            FullName = "Test User",
            RoleId = (int)UserRoles.Customer
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password" && e.ErrorMessage.Contains("required"));
    }

    [Fact]
    public void Validate_WithPasswordTooShort_ShouldFail()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Username = "testuser",
            Password = "123", // Too short
            FullName = "Test User",
            RoleId = (int)UserRoles.Customer
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password" && e.ErrorMessage.Contains("between"));
    }

    [Fact]
    public void Validate_WithPasswordTooLong_ShouldFail()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Username = "testuser",
            Password = new string('a', SystemConstants.PASSWORD_MAX_LENGTH + 1), // Too long
            FullName = "Test User",
            RoleId = (int)UserRoles.Customer
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password" && e.ErrorMessage.Contains("between"));
    }

    [Fact]
    public void Validate_WithEmptyFullName_ShouldFail()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Username = "testuser",
            Password = "Test123",
            FullName = "",
            RoleId = (int)UserRoles.Customer
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FullName" && e.ErrorMessage.Contains("required"));
    }

    [Fact]
    public void Validate_WithFullNameTooLong_ShouldFail()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Username = "testuser",
            Password = "Test123",
            FullName = new string('a', SystemConstants.USERNAME_MAX_LENGTH + 1), // Too long
            RoleId = (int)UserRoles.Customer
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FullName" && e.ErrorMessage.Contains("up to"));
    }

    [Fact]
    public void Validate_WithInvalidEmail_ShouldFail()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Username = "testuser",
            Password = "Test123",
            FullName = "Test User",
            Email = "invalid-email",
            RoleId = (int)UserRoles.Customer
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email" && e.ErrorMessage.Contains("email"));
    }

    [Fact]
    public void Validate_WithValidEmail_ShouldPass()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Username = "testuser",
            Password = "Test123",
            FullName = "Test User",
            Email = "test@example.com",
            RoleId = (int)UserRoles.Customer
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyEmail_ShouldPass()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Username = "testuser",
            Password = "Test123",
            FullName = "Test User",
            Email = "",
            RoleId = (int)UserRoles.Customer
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmailTooLong_ShouldFail()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Username = "testuser",
            Password = "Test123",
            FullName = "Test User",
            Email = new string('a', SystemConstants.USERNAME_MAX_LENGTH + 1), // Too long
            RoleId = (int)UserRoles.Customer
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email" && e.ErrorMessage.Contains("up to"));
    }

    [Fact]
    public void Validate_WithPhoneNumberTooLong_ShouldFail()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Username = "testuser",
            Password = "Test123",
            FullName = "Test User",
            PhoneNumber = new string('1', SystemConstants.USERNAME_MAX_LENGTH + 1), // Too long
            RoleId = (int)UserRoles.Customer
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PhoneNumber" && e.ErrorMessage.Contains("up to"));
    }

    [Fact]
    public void Validate_WithValidPhoneNumber_ShouldPass()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Username = "testuser",
            Password = "Test123",
            FullName = "Test User",
            PhoneNumber = "0913456789",
            RoleId = (int)UserRoles.Customer
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyPhoneNumber_ShouldPass()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Username = "testuser",
            Password = "Test123",
            FullName = "Test User",
            PhoneNumber = "",
            RoleId = (int)UserRoles.Customer
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithInvalidRoleId_ShouldFail()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Username = "testuser",
            Password = "Test123",
            FullName = "Test User",
            RoleId = 999 // Invalid role ID
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "RoleId" && e.ErrorMessage.Contains("valid UserRoles"));
    }

    [Theory]
    [InlineData(UserRoles.Customer)]
    [InlineData(UserRoles.Admin)]
    [InlineData(UserRoles.Staff)]
    [InlineData(UserRoles.Technician)]
    public void Validate_WithValidRoleId_ShouldPass(UserRoles role)
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Username = "testuser",
            Password = "Test123",
            FullName = "Test User",
            RoleId = (int)role
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithAllFieldsValid_ShouldPass()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Username = "testuser123",
            Password = "Test123456",
            FullName = "Test User Full Name",
            Email = "test.user@example.com",
            PhoneNumber = "0913456789",
            RoleId = (int)UserRoles.Customer
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithMultipleValidationErrors_ShouldFailWithMultipleErrors()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Username = "", // Empty
            Password = "12", // Too short
            FullName = "", // Empty
            Email = "invalid-email", // Invalid format
            PhoneNumber = new string('1', 200), // Too long
            RoleId = 999 // Invalid role
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThan(1);
        result.Errors.Should().Contain(e => e.PropertyName == "Username");
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
        result.Errors.Should().Contain(e => e.PropertyName == "FullName");
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
        result.Errors.Should().Contain(e => e.PropertyName == "PhoneNumber");
        result.Errors.Should().Contain(e => e.PropertyName == "RoleId");
    }
}
