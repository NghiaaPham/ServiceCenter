namespace EVServiceCenter.Core.Domains.Payments.DTOs.Requests;

/// <summary>
/// Request to create a new payment (manual or gateway initiation)
/// </summary>
public class CreatePaymentRequestDto
{
    /// <summary>
    /// Invoice ID to pay for
    /// </summary>
    public int InvoiceId { get; set; }

    /// <summary>
    /// Payment amount (must not exceed outstanding amount)
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Payment method: Cash, BankTransfer, VNPay, MoMo, Card
    /// </summary>
    public string PaymentMethod { get; set; } = null!;

    /// <summary>
    /// Optional transaction reference (for bank transfer)
    /// </summary>
    public string? TransactionRef { get; set; }

    /// <summary>
    /// Optional notes
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Return URL for gateway redirects (VNPay, MoMo)
    /// </summary>
    public string? ReturnUrl { get; set; }

    /// <summary>
    /// Optional customer info for gateway (name, email, phone)
    /// </summary>
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
}
