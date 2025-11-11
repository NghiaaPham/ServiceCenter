using EVServiceCenter.Core.Domains.Invoices.DTOs.Requests;
using EVServiceCenter.Core.Domains.Invoices.DTOs.Responses;
using EVServiceCenter.Core.Domains.Shared.Models;

namespace EVServiceCenter.Core.Domains.Invoices.Interfaces;

/// <summary>
/// Service interface for invoice business logic
/// </summary>
public interface IInvoiceService
{
    // Query Operations
    Task<PagedResult<InvoiceSummaryDto>> GetInvoicesAsync(
        InvoiceQueryDto query, CancellationToken cancellationToken);

    Task<InvoiceResponseDto> GetInvoiceByIdAsync(
        int invoiceId, CancellationToken cancellationToken);

    Task<InvoiceResponseDto> GetInvoiceByCodeAsync(
        string invoiceCode, CancellationToken cancellationToken);

    Task<InvoiceResponseDto> GetInvoiceByWorkOrderIdAsync(
        int workOrderId, CancellationToken cancellationToken);

    // Generate Operations
    Task<InvoiceResponseDto> GenerateInvoiceFromWorkOrderAsync(
        GenerateInvoiceRequestDto request, int createdBy, CancellationToken cancellationToken);

    // Update Operations
    Task<InvoiceResponseDto> UpdateInvoiceAsync(
        int invoiceId, UpdateInvoiceRequestDto request, CancellationToken cancellationToken);

    // Send Operations
    Task<bool> SendInvoiceToCustomerAsync(
        int invoiceId, SendInvoiceRequestDto request, CancellationToken cancellationToken);

    // PDF Operations
    Task<byte[]> GenerateInvoicePdfAsync(
        int invoiceId, CancellationToken cancellationToken);

    // Cancel Operations
    Task<bool> CancelInvoiceAsync(
        int invoiceId, string reason, CancellationToken cancellationToken);

    // Payment Operations
    Task<InvoiceResponseDto> RecordPaymentAsync(
        int invoiceId, decimal amount, int paymentMethodId, string? transactionRef,
        CancellationToken cancellationToken);

    /// <summary>
    /// Create pre-payment invoice for appointment before check-in
    /// Used when customer wants to pay before arriving at service center
    /// </summary>
    Task<Core.Entities.Invoice> CreatePrePaymentInvoiceAsync(
        int appointmentId,
        int paymentIntentId,
        int? createdByUserId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Create invoice for package subscription purchase
    /// </summary>
    Task<Core.Entities.Invoice> CreatePackageSubscriptionInvoiceAsync(
        int subscriptionId,
        int customerId,
        string packageName,
        decimal amount,
        int? createdByUserId,
        CancellationToken cancellationToken = default);
}
