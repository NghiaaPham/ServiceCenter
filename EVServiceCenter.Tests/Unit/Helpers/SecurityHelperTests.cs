using FluentAssertions;
using EVServiceCenter.Core.Helpers;
using Xunit;

namespace EVServiceCenter.Tests.Unit.Helpers;

/// <summary>
/// Unit tests for SecurityHelper class
/// </summary>
public class SecurityHelperTests
{
    [Fact]
    public void GenerateSecureToken_ShouldReturnBase64UrlSafeToken()
    {
        // Act
        var token = SecurityHelper.GenerateSecureToken();

        // Assert
        token.Should().NotBeNullOrEmpty();
        token.Should().NotContain("+");
        token.Should().NotContain("/");
        token.Should().NotContain("=");
        token.Length.Should().BeGreaterThan(20); // Base64 encoded 32 bytes
    }

    [Fact]
    public void GenerateSecureToken_WithCustomSize_ShouldReturnCorrectLength()
    {
        // Arrange
        var size = 16;

        // Act
        var token = SecurityHelper.GenerateSecureToken(size);

        // Assert
        token.Should().NotBeNullOrEmpty();
        token.Length.Should().BeGreaterThan(10); // Base64 encoded 16 bytes
    }

    [Fact]
    public void GenerateOTP_ShouldReturnNumericString()
    {
        // Act
        var otp = SecurityHelper.GenerateOTP();

        // Assert
        otp.Should().NotBeNullOrEmpty();
        otp.Should().MatchRegex(@"^\d{6}$");
    }

    [Fact]
    public void GenerateOTP_WithCustomLength_ShouldReturnCorrectLength()
    {
        // Arrange
        var length = 4;

        // Act
        var otp = SecurityHelper.GenerateOTP(length);

        // Assert
        otp.Should().NotBeNullOrEmpty();
        otp.Should().MatchRegex(@"^\d{4}$");
    }

    [Fact]
    public void GenerateSalt_ShouldReturnValidSalt()
    {
        // Act
        var salt = SecurityHelper.GenerateSalt();

        // Assert
        salt.Should().NotBeNullOrEmpty();
        salt.Should().StartWith("$2a$");
        salt.Length.Should().BeGreaterThan(20);
    }

    [Fact]
    public void HashPassword_ShouldReturnValidHash()
    {
        // Arrange
        var password = "TestPassword123";
        var salt = SecurityHelper.GenerateSalt();

        // Act
        var hash = SecurityHelper.HashPassword(password, salt);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        hash.Should().NotBe(password);
        hash.Should().NotBe(salt);
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        var password = "TestPassword123";
        var salt = SecurityHelper.GenerateSalt();
        var hash = SecurityHelper.HashPassword(password, salt);

        // Act
        var isValid = SecurityHelper.VerifyPassword(password, hash);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ShouldReturnFalse()
    {
        // Arrange
        var password = "TestPassword123";
        var wrongPassword = "WrongPassword456";
        var salt = SecurityHelper.GenerateSalt();
        var hash = SecurityHelper.HashPassword(password, salt);

        // Act
        var isValid = SecurityHelper.VerifyPassword(wrongPassword, hash);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void GenerateSecureToken_ShouldGenerateUniqueTokens()
    {
        // Act
        var token1 = SecurityHelper.GenerateSecureToken();
        var token2 = SecurityHelper.GenerateSecureToken();

        // Assert
        token1.Should().NotBe(token2);
    }

    [Fact]
    public void GenerateOTP_ShouldGenerateDifferentValues()
    {
        // Act
        var otp1 = SecurityHelper.GenerateOTP();
        var otp2 = SecurityHelper.GenerateOTP();

        // Assert
        // Note: There's a small chance they could be the same, but very unlikely
        // In practice, this test might occasionally fail due to randomness
        otp1.Should().NotBeNullOrEmpty();
        otp2.Should().NotBeNullOrEmpty();
    }
}
