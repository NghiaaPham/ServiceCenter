using System.Security.Cryptography;

namespace EVServiceCenter.Core.Helpers
{
  public static class SecurityHelper
  {
    /// <summary>
    /// Generate a cryptographically secure random token
    /// </summary>
    /// <param name="size">Size in bytes (default 32)</param>
    /// <returns>Base64 URL-safe encoded token</returns>
    public static string GenerateSecureToken(int size = 32)
    {
      var tokenBytes = new byte[size];
      using var rng = RandomNumberGenerator.Create();
      rng.GetBytes(tokenBytes);

      // Make it URL-safe
      return Convert.ToBase64String(tokenBytes)
          .Replace("+", "-")
          .Replace("/", "_")
          .Replace("=", "");
    }

    /// <summary>
    /// Generate a numeric OTP code
    /// </summary>
    /// <param name="length">Length of OTP (default 6)</param>
    /// <returns>Numeric OTP string</returns>
    public static string GenerateOTP(int length = 6)
    {
      var otp = "";
      using var rng = RandomNumberGenerator.Create();
      var bytes = new byte[4];

      for (int i = 0; i < length; i++)
      {
        rng.GetBytes(bytes);
        var randomNumber = BitConverter.ToUInt32(bytes, 0);
        otp += (randomNumber % 10).ToString();
      }

      return otp;
    }

    /// <summary>
    /// Generate a secure password salt
    /// </summary>
    /// <returns>BCrypt salt string</returns>
    public static string GenerateSalt()
    {
      return BCrypt.Net.BCrypt.GenerateSalt(12);
    }

    /// <summary>
    /// Hash a password with salt
    /// </summary>
    public static string HashPassword(string password, string salt)
    {
      return BCrypt.Net.BCrypt.HashPassword(password, salt);
    }

    /// <summary>
    /// Verify a password against a hash
    /// </summary>
    public static bool VerifyPassword(string password, string hash)
    {
      return BCrypt.Net.BCrypt.Verify(password, hash);
    }
  }
}