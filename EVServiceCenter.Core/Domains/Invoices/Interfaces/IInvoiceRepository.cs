using EVServiceCenter.Core.Domains.Invoices.DTOs.Requests;
using EVServiceCenter.Core.Domains.Invoices.DTOs.Responses;
using EVServiceCenter.Core.Domains.Shared.Models;

namespace EVServiceCenter.Core.Domains.Invoices.Interfaces;

/// <summary>
/// Repository interface for invoice data access
/// </summary>
public interface IInvoiceRepository
{
    // Query Operations
    Task<PagedResult<InvoiceSummaryDto>> GetInvoicesAsync(
        InvoiceQueryDto query, CancellationToken cancellationToken);

    Task<InvoiceResponseDto?> GetInvoiceByIdAsync(
        int invoiceId, CancellationToken cancellationToken);

    Task<InvoiceResponseDto?> GetInvoiceByCodeAsync(
        string invoiceCode, CancellationToken cancellationToken);

    Task<InvoiceResponseDto?> GetInvoiceByWorkOrderIdAsync(
        int workOrderId, CancellationToken cancellationToken);

    // Create Operations
    Task<InvoiceResponseDto> CreateInvoiceAsync(
        GenerateInvoiceRequestDto request, int createdBy, CancellationToken cancellationToken);

    // Update Operations
    Task<InvoiceResponseDto> UpdateInvoiceAsync(
        int invoiceId, UpdateInvoiceRequestDto request, CancellationToken cancellationToken);

    Task<bool> MarkInvoiceAsSentAsync(
        int invoiceId, string sendMethod, CancellationToken cancellationToken);

    Task<bool> UpdateInvoiceStatusAsync(
        int invoiceId, string status, CancellationToken cancellationToken);

    // Payment Operations
    Task<bool> RecordPaymentAsync(
        int invoiceId, decimal amount, CancellationToken cancellationToken);

    // Delete Operations
    Task<bool> CancelInvoiceAsync(int invoiceId, CancellationToken cancellationToken);

    // Validation
    Task<bool> InvoiceExistsAsync(int invoiceId, CancellationToken cancellationToken);
    Task<bool> InvoiceExistsByWorkOrderAsync(int workOrderId, CancellationToken cancellationToken);
}
