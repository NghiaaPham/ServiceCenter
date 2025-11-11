namespace EVServiceCenter.Core.Domains.Payments.DTOs.Responses;

public enum PaymentCallbackStatus
{
    Success,
    AlreadyProcessed,
    PaymentNotFound,
    InvalidSignature,
    InvalidAmount,
    Failed,
    Error
}

public readonly record struct PaymentCallbackResult(PaymentCallbackStatus Status, string? Message = null)
{
    public bool IsSuccess => Status == PaymentCallbackStatus.Success || Status == PaymentCallbackStatus.AlreadyProcessed;
}

