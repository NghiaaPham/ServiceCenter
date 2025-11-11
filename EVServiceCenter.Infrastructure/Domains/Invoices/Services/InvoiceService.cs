using EVServiceCenter.Core.Domains.Invoices.DTOs.Requests;
using EVServiceCenter.Core.Domains.Invoices.DTOs.Responses;
using EVServiceCenter.Core.Domains.Invoices.Interfaces;
using EVServiceCenter.Core.Domains.Shared.Models;
using EVServiceCenter.Core.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace EVServiceCenter.Infrastructure.Domains.Invoices.Services;

/// <summary>
/// Service for invoice business logic
/// </summary>
public class InvoiceService : IInvoiceService
{
    private readonly IInvoiceRepository _repository;
    private readonly ILogger<InvoiceService> _logger;
    private readonly EVDbContext _context;

    public InvoiceService(
        IInvoiceRepository repository,
        ILogger<InvoiceService> logger,
        EVDbContext context)
    {
        _repository = repository;
        _logger = logger;
        _context = context;
    }

    public async Task<PagedResult<InvoiceSummaryDto>> GetInvoicesAsync(
        InvoiceQueryDto query, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting invoices with query");
        return await _repository.GetInvoicesAsync(query, cancellationToken);
    }

    public async Task<InvoiceResponseDto> GetInvoiceByIdAsync(int invoiceId, CancellationToken cancellationToken)
    {
        var invoice = await _repository.GetInvoiceByIdAsync(invoiceId, cancellationToken);
        if (invoice == null)
            throw new KeyNotFoundException($"Invoice {invoiceId} not found");
        return invoice;
    }

    public async Task<Core.Entities.Invoice> CreatePackageSubscriptionInvoiceAsync(
        int subscriptionId,
        int customerId,
        string packageName,
        decimal amount,
        int? createdByUserId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Creating subscription invoice for Subscription {SubscriptionId}, Customer {CustomerId}",
            subscriptionId, customerId);

        var invoiceCode = await GenerateInvoiceCodeAsync(cancellationToken);

        var invoice = new Core.Entities.Invoice
        {
            InvoiceCode = invoiceCode,
            CustomerId = customerId,
            InvoiceDate = DateTime.UtcNow,
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            ServiceSubTotal = amount,
            ServiceDiscount = 0,
            ServiceTax = 0,
            ServiceTotal = amount,
            PartsSubTotal = 0,
            PartsDiscount = 0,
            PartsTax = 0,
            PartsTotal = 0,
            SubTotal = amount,
            TotalDiscount = 0,
            TotalTax = 0,
            GrandTotal = amount,
            OutstandingAmount = amount,
            PaidAmount = 0,
            Status = "Pending",
            PaymentTerms = "Package subscription payment - due within 24 hours.",
            Notes = $"[Subscription] #{subscriptionId} - {packageName}",
            CreatedDate = DateTime.UtcNow,
            CreatedBy = createdByUserId
        };

        await _context.Invoices.AddAsync(invoice, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Subscription invoice {InvoiceCode} created for subscription {SubscriptionId}",
            invoice.InvoiceCode,
            subscriptionId);

        return invoice;
    }

    public async Task<InvoiceResponseDto> GetInvoiceByCodeAsync(string invoiceCode, CancellationToken cancellationToken)
    {
        var invoice = await _repository.GetInvoiceByCodeAsync(invoiceCode, cancellationToken);
        if (invoice == null)
            throw new KeyNotFoundException($"Invoice {invoiceCode} not found");
        return invoice;
    }

    public async Task<InvoiceResponseDto> GetInvoiceByWorkOrderIdAsync(int workOrderId, CancellationToken cancellationToken)
    {
        var invoice = await _repository.GetInvoiceByWorkOrderIdAsync(workOrderId, cancellationToken);
        if (invoice == null)
            throw new KeyNotFoundException($"No invoice found for WorkOrder {workOrderId}");
        return invoice;
    }

    public async Task<InvoiceResponseDto> GenerateInvoiceFromWorkOrderAsync(
        GenerateInvoiceRequestDto request, int createdBy, CancellationToken cancellationToken)
    {
        // Check if invoice already exists
        if (await _repository.InvoiceExistsByWorkOrderAsync(request.WorkOrderId, cancellationToken))
            throw new InvalidOperationException($"Invoice already exists for WorkOrder {request.WorkOrderId}");

        _logger.LogInformation("Generating invoice from WorkOrder {WorkOrderId}", request.WorkOrderId);

        var invoice = await _repository.CreateInvoiceAsync(request, createdBy, cancellationToken);

        // Send to customer if requested
        if (request.SendToCustomer && !string.IsNullOrEmpty(request.SendMethod))
        {
            var sendRequest = new SendInvoiceRequestDto
            {
                SendMethod = request.SendMethod,
                IncludePdf = true
            };
            await SendInvoiceToCustomerAsync(invoice.InvoiceId, sendRequest, cancellationToken);
        }

        return invoice;
    }

    public async Task<InvoiceResponseDto> UpdateInvoiceAsync(
        int invoiceId, UpdateInvoiceRequestDto request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating invoice {InvoiceId}", invoiceId);
        return await _repository.UpdateInvoiceAsync(invoiceId, request, cancellationToken);
    }

    public async Task<bool> SendInvoiceToCustomerAsync(
        int invoiceId, SendInvoiceRequestDto request, CancellationToken cancellationToken)
    {
        var invoice = await GetInvoiceByIdAsync(invoiceId, cancellationToken);

        _logger.LogInformation("Sending invoice {InvoiceCode} via {SendMethod}", invoice.InvoiceCode, request.SendMethod);

        // TODO: Implement email/SMS sending logic
        // For now, just mark as sent
        await _repository.MarkInvoiceAsSentAsync(invoiceId, request.SendMethod, cancellationToken);

        return true;
    }

    public async Task<byte[]> GenerateInvoicePdfAsync(int invoiceId, CancellationToken cancellationToken)
    {
        var invoice = await GetInvoiceByIdAsync(invoiceId, cancellationToken);

        _logger.LogInformation("Generating PDF for invoice {InvoiceCode}", invoice.InvoiceCode);

        // TODO: Implement PDF generation
        // Return empty byte array for now
        return Array.Empty<byte>();
    }

    public async Task<bool> CancelInvoiceAsync(int invoiceId, string reason, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cancelling invoice {InvoiceId}. Reason: {Reason}", invoiceId, reason);

        var invoice = await GetInvoiceByIdAsync(invoiceId, cancellationToken);

        if (invoice.PaidAmount > 0)
            throw new InvalidOperationException("Cannot cancel invoice with payments. Create refund instead.");

        return await _repository.CancelInvoiceAsync(invoiceId, cancellationToken);
    }

    public async Task<InvoiceResponseDto> RecordPaymentAsync(
        int invoiceId, decimal amount, int paymentMethodId, string? transactionRef,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Recording payment of {Amount} for invoice {InvoiceId}", amount, invoiceId);

        var invoice = await GetInvoiceByIdAsync(invoiceId, cancellationToken);

        if (amount > invoice.OutstandingAmount)
            throw new InvalidOperationException($"Payment amount {amount} exceeds outstanding amount {invoice.OutstandingAmount}");

        await _repository.RecordPaymentAsync(invoiceId, amount, cancellationToken);

        return await GetInvoiceByIdAsync(invoiceId, cancellationToken);
    }

    /// <summary>
    /// ✨ GAP #1 FIX: Create pre-payment invoice for appointment before check-in
    /// Used when customer wants to pay online before arriving at service center
    /// </summary>
    public async Task<Core.Entities.Invoice> CreatePrePaymentInvoiceAsync(
        int appointmentId,
        int paymentIntentId,
        int? createdByUserId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Creating pre-payment invoice for Appointment {AppointmentId}, PaymentIntent {PaymentIntentId}",
            appointmentId, paymentIntentId);

        // Load appointment with services
        var appointment = await _context.Appointments
            .Include(a => a.Customer)
            .Include(a => a.AppointmentServices)
                .ThenInclude(aps => aps.Service)
            .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId, cancellationToken);

        if (appointment == null)
            throw new InvalidOperationException($"Appointment {appointmentId} not found");

        // Calculate totals from AppointmentServices (exclude subscription services)
        var serviceTotal = appointment.AppointmentServices
            .Where(aps => aps.ServiceSource != "Subscription")
            .Sum(aps => aps.Price);

        var prePaymentNotePrefix = $"[PrePayment] Appointment: {appointment.AppointmentCode} (ID: {appointmentId})";

        var creatorUserId = createdByUserId
            ?? appointment.CreatedBy
            ?? appointment.Customer.UserId;

        // Reuse existing pre-payment invoice if it is still outstanding instead of creating duplicates
        var existingInvoice = await _context.Invoices
            .Where(i =>
                i.CustomerId == appointment.CustomerId &&
                i.Notes != null &&
                i.Notes.Contains(prePaymentNotePrefix) &&
                i.Status != "Cancelled")
            .OrderByDescending(i => i.CreatedDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingInvoice != null)
        {
            if (!existingInvoice.Notes!.Contains($"PaymentIntent: {paymentIntentId}"))
            {
                existingInvoice.Notes = $"{existingInvoice.Notes} | PaymentIntent: {paymentIntentId}";
                existingInvoice.UpdatedDate = DateTime.UtcNow;
                existingInvoice.UpdatedBy = creatorUserId;
                await _context.SaveChangesAsync(cancellationToken);
            }

            _logger.LogInformation(
                "Reusing existing pre-payment invoice {InvoiceCode} for Appointment {AppointmentId}. Outstanding: {OutstandingAmount}",
                existingInvoice.InvoiceCode,
                appointmentId,
                existingInvoice.OutstandingAmount);

            return existingInvoice;
        }

        var taxRate = 0.08m;
        var taxAmount = serviceTotal * taxRate;
        var grandTotal = serviceTotal + taxAmount;

        // Generate invoice code
        var invoiceCode = await GenerateInvoiceCodeAsync(cancellationToken);

        var invoice = new Core.Entities.Invoice
        {
            InvoiceCode = invoiceCode,
            WorkOrderId = null, // Pre-payment invoice may be created before a WorkOrder exists
            CustomerId = appointment.CustomerId,
            InvoiceDate = DateTime.UtcNow,
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)), // 24h to pay

            ServiceSubTotal = serviceTotal,
            ServiceTax = taxAmount,
            ServiceTotal = serviceTotal + taxAmount,

            PartsSubTotal = 0, // No parts yet
            PartsTax = 0,
            PartsTotal = 0,

            SubTotal = serviceTotal,
            TotalTax = taxAmount,
            GrandTotal = grandTotal,
            OutstandingAmount = grandTotal,
            PaidAmount = 0,

            Status = "Pending",
            PaymentTerms = "Pre-payment for appointment. Pay within 24 hours.",
            Notes = $"[PrePayment] Appointment: {appointment.AppointmentCode} (ID: {appointmentId}), PaymentIntent: {paymentIntentId}",

            CreatedDate = DateTime.UtcNow,
            CreatedBy = creatorUserId
        };

        await _context.Invoices.AddAsync(invoice, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "✅ Pre-payment invoice created: {InvoiceCode} for Appointment {AppointmentId}, Amount: {Amount}đ",
            invoiceCode, appointmentId, grandTotal);

        return invoice;
    }

    /// <summary>
    /// Generate unique invoice code in format: INV-YYYYMMDD-XXXX
    /// </summary>
    private async Task<string> GenerateInvoiceCodeAsync(CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var prefix = $"INV-{today:yyyyMMdd}-";

        // Find max invoice number for today
        var maxCode = await _context.Invoices
            .Where(i => i.InvoiceCode.StartsWith(prefix))
            .OrderByDescending(i => i.InvoiceCode)
            .Select(i => i.InvoiceCode)
            .FirstOrDefaultAsync(cancellationToken);

        int nextNumber = 1;
        if (maxCode != null)
        {
            var numberPart = maxCode.Substring(prefix.Length);
            if (int.TryParse(numberPart, out int currentNumber))
            {
                nextNumber = currentNumber + 1;
            }
        }

        return $"{prefix}{nextNumber:D4}";
    }
}
