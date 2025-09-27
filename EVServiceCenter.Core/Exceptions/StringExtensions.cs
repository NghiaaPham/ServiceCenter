using System.Text;

namespace EVServiceCenter.Core.Extensions
{
  public static class StringExtensions
  {
    public static string ToBase64(this string plainText)
    {
      var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
      return Convert.ToBase64String(plainTextBytes);
    }

    public static string FromBase64(this string base64EncodedData)
    {
      var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
      return Encoding.UTF8.GetString(base64EncodedBytes);
    }

    public static string ToUrlSafeBase64(this string base64)
    {
      return base64.Replace("+", "-").Replace("/", "_").Replace("=", "");
    }

    public static string FromUrlSafeBase64(this string urlSafeBase64)
    {
      string base64 = urlSafeBase64.Replace("-", "+").Replace("_", "/");
      int padding = 4 - (base64.Length % 4);
      if (padding < 4)
      {
        base64 += new string('=', padding);
      }
      return base64;
    }
  }
}