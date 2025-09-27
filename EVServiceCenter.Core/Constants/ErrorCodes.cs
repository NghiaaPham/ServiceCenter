namespace EVServiceCenter.Core.Constants
{
  // Error Codes cho API Response
  public static class ErrorCodes
  {
    public const string VALIDATION_ERROR = "VALIDATION_ERROR";
    public const string INVALID_CREDENTIALS = "INVALID_CREDENTIALS";
    public const string EMAIL_NOT_VERIFIED = "EMAIL_NOT_VERIFIED";
    public const string ACCOUNT_LOCKED = "ACCOUNT_LOCKED";
    public const string INVALID_TOKEN = "INVALID_TOKEN";
    public const string TOKEN_EXPIRED = "TOKEN_EXPIRED";
    public const string USER_NOT_FOUND = "USER_NOT_FOUND";
    public const string DUPLICATE_USERNAME = "DUPLICATE_USERNAME";
    public const string DUPLICATE_EMAIL = "DUPLICATE_EMAIL";
    public const string EMAIL_VERIFICATION_REQUIRED = "EMAIL_VERIFICATION_REQUIRED";
    public const string EMAIL_ALREADY_VERIFIED = "EMAIL_ALREADY_VERIFIED";
    public const string INTERNAL_ERROR = "INTERNAL_ERROR";
    public const string ACCESS_DENIED = "ACCESS_DENIED";
    public const string PASSWORD_EXPIRED = "PASSWORD_EXPIRED";
  }
}