using FluentAssertions;
using EVServiceCenter.Core.Helpers;
using Xunit;

namespace EVServiceCenter.Tests.Unit.Helpers;

/// <summary>
/// Unit tests for PasswordValidator class
/// </summary>
public class PasswordValidatorTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void ValidatePassword_WithNullOrEmptyPassword_ShouldReturnInvalid(string password)
    {
        // Act
        var result = PasswordValidator.ValidatePassword(password);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("Mật khẩu không được để trống");
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("abc")]
    [InlineData("a")]
    public void ValidatePassword_WithShortPassword_ShouldReturnInvalid(string password)
    {
        // Act
        var result = PasswordValidator.ValidatePassword(password);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("Mật khẩu phải có ít nhất 6 ký tự");
    }

    [Theory]
    [InlineData("abcdef")]
    [InlineData("password")]
    [InlineData("abcdefgh")]
    public void ValidatePassword_WithNoDigits_ShouldReturnInvalid(string password)
    {
        // Act
        var result = PasswordValidator.ValidatePassword(password);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("Mật khẩu phải chứa ít nhất 1 số");
    }

    [Theory]
    [InlineData("123456")]
    [InlineData("12345678")]
    [InlineData("987654321")]
    public void ValidatePassword_WithNoLetters_ShouldReturnInvalid(string password)
    {
        // Act
        var result = PasswordValidator.ValidatePassword(password);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("Mật khẩu phải chứa ít nhất 1 chữ cái");
    }

    [Theory]
    [InlineData("abc123")]
    [InlineData("Password1")]
    [InlineData("test123")]
    [InlineData("MyPassword123")]
    [InlineData("a1b2c3")]
    public void ValidatePassword_WithValidPassword_ShouldReturnValid(string password)
    {
        // Act
        var result = PasswordValidator.ValidatePassword(password);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }

    [Theory]
    [InlineData("ABC123")]
    [InlineData("PASSWORD1")]
    [InlineData("TEST123")]
    public void ValidatePassword_WithUppercaseLettersAndDigits_ShouldReturnValid(string password)
    {
        // Act
        var result = PasswordValidator.ValidatePassword(password);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }

    [Theory]
    [InlineData("abc123!@#")]
    [InlineData("Password1$")]
    [InlineData("test123%")]
    public void ValidatePassword_WithSpecialCharacters_ShouldReturnValid(string password)
    {
        // Act
        var result = PasswordValidator.ValidatePassword(password);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void ValidatePassword_WithMixedCaseAndDigits_ShouldReturnValid()
    {
        // Arrange
        var password = "MySecure123";

        // Act
        var result = PasswordValidator.ValidatePassword(password);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }
}
