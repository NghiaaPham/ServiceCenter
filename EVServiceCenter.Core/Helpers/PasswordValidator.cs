// EVServiceCenter.Core/Helpers/PasswordValidator.cs
namespace EVServiceCenter.Core.Helpers
{
  public static class PasswordValidator
  {
    public static (bool IsValid, string? ErrorMessage) ValidatePassword(string password)
    {
      if (string.IsNullOrWhiteSpace(password))
        return (false, "Mật khẩu không được để trống");

      if (password.Length < 6)
        return (false, "Mật khẩu phải có ít nhất 6 ký tự");

      if (!password.Any(char.IsDigit))
        return (false, "Mật khẩu phải chứa ít nhất 1 số");

      if (!password.Any(char.IsLetter))
        return (false, "Mật khẩu phải chứa ít nhất 1 chữ cái");
      return (true, null);
    }
  }
}