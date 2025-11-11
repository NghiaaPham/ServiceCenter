namespace EVServiceCenter.Core.Domains.Payments.Constants;

/// <summary>
/// Payment status constants
/// </summary>
public static class PaymentStatus
{
    public const string Pending = "Pending";
    public const string Processing = "Processing";
    public const string Completed = "Completed";
    public const string Failed = "Failed";
    public const string Cancelled = "Cancelled";
    public const string Refunded = "Refunded";
    public const string PartiallyRefunded = "PartiallyRefunded";
}

/// <summary>
/// Payment method type constants
/// </summary>
public static class PaymentMethodType
{
    public const string Cash = "Cash";
    public const string BankTransfer = "BankTransfer";
    public const string VNPay = "VNPay";
    public const string MoMo = "MoMo";
    public const string Card = "Card";
}

/// <summary>
/// Payment gateway response codes
/// </summary>
public static class PaymentResponseCode
{
    // Success
    public const string Success = "00";

    // VNPay specific
    public const string VNPay_Suspicious = "07";
    public const string VNPay_NotRegistered = "09";
    public const string VNPay_AuthFailed = "10";
    public const string VNPay_Timeout = "11";
    public const string VNPay_InvalidCard = "12";
    public const string VNPay_InvalidAmount = "13";
    public const string VNPay_InsufficientFunds = "51";
    public const string VNPay_ExceededLimit = "65";
    public const string VNPay_Maintenance = "75";
    public const string VNPay_InvalidPassword = "79";

    // MoMo specific
    public const string MoMo_InvalidSignature = "10";
    public const string MoMo_InvalidParameter = "11";
    public const string MoMo_PaymentNotFound = "20";
    public const string MoMo_PaymentExpired = "21";
    public const string MoMo_InsufficientFunds = "1001";
    public const string MoMo_Timeout = "1002";
    public const string MoMo_InvalidAccount = "1003";
    public const string MoMo_UserCancelled = "1004";
    public const string MoMo_TransactionFailed = "1005";
    public const string MoMo_SystemError = "9000";
}

/// <summary>
/// Refund status constants
/// </summary>
public static class RefundStatus
{
    public const string Pending = "Pending";
    public const string Processing = "Processing";
    public const string Completed = "Completed";
    public const string Failed = "Failed";
    public const string Cancelled = "Cancelled";
}
