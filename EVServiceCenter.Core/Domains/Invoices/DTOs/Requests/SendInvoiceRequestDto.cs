namespace EVServiceCenter.Core.Domains.Invoices.DTOs.Requests;

/// <summary>
/// Request DTO for sending invoice to customer
/// </summary>
public class SendInvoiceRequestDto
{
    /// <summary>
    /// Send method: Email, SMS, Both
    /// </summary>
    public string SendMethod { get; set; } = "Email";

    /// <summary>
    /// Optional: Override email address (use customer's email by default)
    /// </summary>
    public string? EmailAddress { get; set; }

    /// <summary>
    /// Optional: Override phone number (use customer's phone by default)
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Optional: Additional message to include
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Include PDF attachment (default: true)
    /// </summary>
    public bool IncludePdf { get; set; } = true;
}
