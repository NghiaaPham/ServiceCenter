using EVServiceCenter.Core.Domains.Invoices.DTOs.Requests;
using EVServiceCenter.Core.Domains.Invoices.DTOs.Responses;
using EVServiceCenter.Core.Domains.Invoices.Interfaces;
using EVServiceCenter.Core.Domains.Shared.Models;
using EVServiceCenter.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EVServiceCenter.Infrastructure.Domains.Invoices.Repositories;

/// <summary>
/// Repository for invoice data access with performance optimization
/// </summary>
public class InvoiceRepository : IInvoiceRepository
{
    private readonly EVDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<InvoiceRepository> _logger;

    public InvoiceRepository(
        EVDbContext context,
        IConfiguration configuration,
        ILogger<InvoiceRepository> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<PagedResult<InvoiceSummaryDto>> GetInvoicesAsync(
        InvoiceQueryDto query, CancellationToken cancellationToken)
    {
        var baseQuery = _context.Set<Invoice>().AsNoTracking();

        // Apply filters
        if (query.CustomerId.HasValue)
            baseQuery = baseQuery.Where(i => i.CustomerId == query.CustomerId.Value);

        if (query.WorkOrderId.HasValue)
            baseQuery = baseQuery.Where(i => i.WorkOrderId == query.WorkOrderId.Value);

        if (!string.IsNullOrEmpty(query.Status))
            baseQuery = baseQuery.Where(i => i.Status == query.Status);

        if (query.InvoiceDateFrom.HasValue)
            baseQuery = baseQuery.Where(i => i.InvoiceDate >= query.InvoiceDateFrom.Value);

        if (query.InvoiceDateTo.HasValue)
            baseQuery = baseQuery.Where(i => i.InvoiceDate <= query.InvoiceDateTo.Value);

        if (query.DueDateFrom.HasValue)
        {
            var dueDateFrom = DateOnly.FromDateTime(query.DueDateFrom.Value);
            baseQuery = baseQuery.Where(i => i.DueDate >= dueDateFrom);
        }

        if (query.DueDateTo.HasValue)
        {
            var dueDateTo = DateOnly.FromDateTime(query.DueDateTo.Value);
            baseQuery = baseQuery.Where(i => i.DueDate <= dueDateTo);
        }

        if (query.IsOverdue.HasValue && query.IsOverdue.Value)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            baseQuery = baseQuery.Where(i => i.DueDate < today && i.OutstandingAmount > 0);
        }

        if (!string.IsNullOrEmpty(query.SearchTerm))
        {
            var search = query.SearchTerm.ToLower();
            baseQuery = baseQuery.Where(i =>
                i.InvoiceCode.ToLower().Contains(search) ||
                i.Customer!.FullName.ToLower().Contains(search));
        }

        var totalCount = await baseQuery.CountAsync(cancellationToken);

        // Apply sorting
        baseQuery = query.SortBy.ToLowerInvariant() switch
        {
            "duedate" => query.SortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase)
                ? baseQuery.OrderByDescending(i => i.DueDate)
                : baseQuery.OrderBy(i => i.DueDate),
            "grandtotal" => query.SortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase)
                ? baseQuery.OrderByDescending(i => i.GrandTotal)
                : baseQuery.OrderBy(i => i.GrandTotal),
            "status" => query.SortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase)
                ? baseQuery.OrderByDescending(i => i.Status)
                : baseQuery.OrderBy(i => i.Status),
            _ => query.SortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase)
                ? baseQuery.OrderByDescending(i => i.InvoiceDate)
                : baseQuery.OrderBy(i => i.InvoiceDate)
        };

        // Project to DTO in database
        var invoices = await baseQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(i => new InvoiceSummaryDto
            {
                InvoiceId = i.InvoiceId,
                InvoiceCode = i.InvoiceCode,
                WorkOrderId = i.WorkOrderId,
                WorkOrderCode = i.WorkOrder != null ? i.WorkOrder.WorkOrderCode : null,
                CustomerId = i.CustomerId,
                CustomerName = i.Customer!.FullName,
                InvoiceDate = i.InvoiceDate ?? DateTime.UtcNow,
                DueDate = i.DueDate.HasValue ? i.DueDate.Value.ToDateTime(TimeOnly.MinValue) : DateTime.UtcNow,
                IsOverdue = i.DueDate.HasValue && i.DueDate.Value < DateOnly.FromDateTime(DateTime.UtcNow) && i.OutstandingAmount > 0,
                DaysOverdue = i.DueDate.HasValue && i.DueDate.Value < DateOnly.FromDateTime(DateTime.UtcNow) && i.OutstandingAmount > 0
                    ? (int)(DateTime.UtcNow - i.DueDate.Value.ToDateTime(TimeOnly.MinValue)).TotalDays : 0,
                GrandTotal = i.GrandTotal ?? 0,
                PaidAmount = i.PaidAmount ?? 0,
                OutstandingAmount = i.OutstandingAmount ?? 0,
                PaymentProgress = i.GrandTotal > 0 ? (i.PaidAmount ?? 0) / i.GrandTotal.Value * 100 : 0,
                Status = i.Status ?? "Draft",
                SentToCustomer = i.SentToCustomer ?? false,
                CreatedDate = i.CreatedDate ?? DateTime.UtcNow
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<InvoiceSummaryDto>
        {
            Items = invoices,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
        };
    }

    public async Task<InvoiceResponseDto?> GetInvoiceByIdAsync(int invoiceId, CancellationToken cancellationToken)
    {
        return await BuildInvoiceQuery()
            .Where(i => i.InvoiceId == invoiceId)
            .Select(i => MapToInvoiceResponse(i))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<InvoiceResponseDto?> GetInvoiceByCodeAsync(string invoiceCode, CancellationToken cancellationToken)
    {
        return await BuildInvoiceQuery()
            .Where(i => i.InvoiceCode == invoiceCode)
            .Select(i => MapToInvoiceResponse(i))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<InvoiceResponseDto?> GetInvoiceByWorkOrderIdAsync(int workOrderId, CancellationToken cancellationToken)
    {
        return await BuildInvoiceQuery()
            .Where(i => i.WorkOrderId == workOrderId)
            .Select(i => MapToInvoiceResponse(i))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<InvoiceResponseDto> CreateInvoiceAsync(
        GenerateInvoiceRequestDto request, int createdBy, CancellationToken cancellationToken)
    {
        // Get WorkOrder with all details INCLUDING Appointment
        var workOrder = await _context.Set<WorkOrder>()
            .Include(w => w.WorkOrderServices!)
                .ThenInclude(ws => ws.Service)
            .Include(w => w.WorkOrderParts!)
                .ThenInclude(wp => wp.Part)
            .Include(w => w.Customer)
            .Include(w => w.Appointment) // 🔧 FIX GAP #3: Include Appointment for payment sync
            .FirstOrDefaultAsync(w => w.WorkOrderId == request.WorkOrderId, cancellationToken);

        if (workOrder == null)
            throw new KeyNotFoundException($"WorkOrder {request.WorkOrderId} not found");

        // 🔧 FIX GAP #10: Get VAT rate from configuration (default 10%)
        var vatRate = _configuration.GetValue<decimal>("Tax:VATRate", 0.10m);

        // Check if customer/services are tax-exempt
        var isTaxExempt = await IsTaxExemptAsync(workOrder, cancellationToken);

        // Calculate totals
        var serviceSubTotal = workOrder.WorkOrderServices?.Sum(ws => ws.TotalPrice ?? 0) ?? 0;
        var serviceDiscount = workOrder.WorkOrderServices?.Sum(ws => ws.DiscountAmount ?? 0) ?? 0;

        // 🔧 FIX GAP #10: Calculate service tax based on VAT rate
        var serviceTaxableAmount = serviceSubTotal - serviceDiscount;
        var serviceTax = isTaxExempt ? 0m : serviceTaxableAmount * vatRate;

        var partsSubTotal = workOrder.WorkOrderParts?.Sum(wp => wp.TotalPrice ?? 0) ?? 0;
        var partsDiscount = workOrder.WorkOrderParts?.Sum(wp => wp.DiscountAmount ?? 0) ?? 0;

        // 🔧 FIX GAP #10: Calculate parts tax based on VAT rate
        var partsTaxableAmount = partsSubTotal - partsDiscount;
        var partsTax = isTaxExempt ? 0m : partsTaxableAmount * vatRate;

        var serviceTotal = serviceSubTotal - serviceDiscount + serviceTax;
        var partsTotal = partsSubTotal - partsDiscount + partsTax;
        var subTotal = serviceTotal + partsTotal;
        var totalDiscount = serviceDiscount + partsDiscount;
        var totalTax = serviceTax + partsTax;

        // Apply additional discount if provided (TIER 3 - Admin manual discount)
        if (request.AdditionalDiscountPercent.HasValue)
        {
            var additionalDiscount = subTotal * request.AdditionalDiscountPercent.Value / 100;
            totalDiscount += additionalDiscount;
            subTotal -= additionalDiscount;
        }
        else if (request.AdditionalDiscountAmount.HasValue)
        {
            totalDiscount += request.AdditionalDiscountAmount.Value;
            subTotal -= request.AdditionalDiscountAmount.Value;
        }

        var grandTotal = subTotal;

        // 🔧 FIX GAP #3: Check if appointment already paid
        decimal alreadyPaidAmount = 0;
        string initialStatus = "Unpaid";

        if (workOrder.Appointment != null)
        {
            alreadyPaidAmount = workOrder.Appointment.PaidAmount ?? 0;

            if (alreadyPaidAmount > 0)
            {
                _logger.LogInformation(
                    "Appointment {AppointmentId} already paid {Amount}đ. " +
                    "Syncing to Invoice {InvoiceCode}",
                    workOrder.Appointment.AppointmentId, alreadyPaidAmount);

                // Determine initial status based on payment
                if (alreadyPaidAmount >= grandTotal)
                {
                    initialStatus = "Paid";
                }
                else if (alreadyPaidAmount > 0)
                {
                    initialStatus = "PartiallyPaid";
                }
            }
        }

        // Create invoice
        var invoice = new Invoice
        {
            InvoiceCode = GenerateInvoiceCode(),
            WorkOrderId = request.WorkOrderId,
            CustomerId = workOrder.CustomerId,
            InvoiceDate = DateTime.UtcNow,
            DueDate = request.DueDate.HasValue ? DateOnly.FromDateTime(request.DueDate.Value) : DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
            ServiceSubTotal = serviceSubTotal,
            ServiceDiscount = serviceDiscount,
            ServiceTax = serviceTax,
            ServiceTotal = serviceTotal,
            PartsSubTotal = partsSubTotal,
            PartsDiscount = partsDiscount,
            PartsTax = partsTax,
            PartsTotal = partsTotal,
            SubTotal = subTotal,
            TotalDiscount = totalDiscount,
            TotalTax = totalTax,
            GrandTotal = grandTotal,
            PaidAmount = alreadyPaidAmount, // 🔧 FIX GAP #3: Sync from Appointment
            OutstandingAmount = grandTotal - alreadyPaidAmount,
            Status = initialStatus, // 🔧 FIX GAP #3: Reflect actual payment status
            PaymentTerms = request.PaymentTerms ?? "Net 30",
            Notes = request.Notes,
            SentToCustomer = false,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        _context.Set<Invoice>().Add(invoice);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Invoice {InvoiceCode} created for WorkOrder {WorkOrderId}. " +
            "GrandTotal: {GrandTotal}đ, Tax: {Tax}đ ({VATRate}%), " +
            "AlreadyPaid: {Paid}đ, Status: {Status}",
            invoice.InvoiceCode, request.WorkOrderId, grandTotal, totalTax,
            vatRate * 100, alreadyPaidAmount, initialStatus);

        return (await GetInvoiceByIdAsync(invoice.InvoiceId, cancellationToken))!;
    }

    public async Task<InvoiceResponseDto> UpdateInvoiceAsync(
        int invoiceId, UpdateInvoiceRequestDto request, CancellationToken cancellationToken)
    {
        var invoice = await _context.Set<Invoice>()
            .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId, cancellationToken);

        if (invoice == null)
            throw new KeyNotFoundException($"Invoice {invoiceId} not found");

        if (request.DueDate.HasValue)
            invoice.DueDate = DateOnly.FromDateTime(request.DueDate.Value);

        if (!string.IsNullOrEmpty(request.PaymentTerms))
            invoice.PaymentTerms = request.PaymentTerms;

        if (request.Notes != null)
            invoice.Notes = request.Notes;

        if (!string.IsNullOrEmpty(request.Status))
            invoice.Status = request.Status;

        await _context.SaveChangesAsync(cancellationToken);

        return (await GetInvoiceByIdAsync(invoiceId, cancellationToken))!;
    }

    public async Task<bool> MarkInvoiceAsSentAsync(int invoiceId, string sendMethod, CancellationToken cancellationToken)
    {
        var invoice = await _context.Set<Invoice>()
            .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId, cancellationToken);

        if (invoice == null) return false;

        invoice.SentToCustomer = true;
        invoice.SentDate = DateTime.UtcNow;
        invoice.SentMethod = sendMethod;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UpdateInvoiceStatusAsync(int invoiceId, string status, CancellationToken cancellationToken)
    {
        var invoice = await _context.Set<Invoice>()
            .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId, cancellationToken);

        if (invoice == null) return false;

        invoice.Status = status;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> RecordPaymentAsync(int invoiceId, decimal amount, CancellationToken cancellationToken)
    {
        var invoice = await _context.Set<Invoice>()
            .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId, cancellationToken);

        if (invoice == null) return false;

        invoice.PaidAmount = (invoice.PaidAmount ?? 0) + amount;
        invoice.OutstandingAmount = (invoice.GrandTotal ?? 0) - invoice.PaidAmount;

        // Update status based on payment
        if (invoice.OutstandingAmount <= 0)
            invoice.Status = "Paid";
        else if (invoice.PaidAmount > 0)
            invoice.Status = "PartiallyPaid";

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> CancelInvoiceAsync(int invoiceId, CancellationToken cancellationToken)
    {
        var invoice = await _context.Set<Invoice>()
            .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId, cancellationToken);

        if (invoice == null) return false;

        invoice.Status = "Cancelled";
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> InvoiceExistsAsync(int invoiceId, CancellationToken cancellationToken)
    {
        return await _context.Set<Invoice>()
            .AnyAsync(i => i.InvoiceId == invoiceId, cancellationToken);
    }

    public async Task<bool> InvoiceExistsByWorkOrderAsync(int workOrderId, CancellationToken cancellationToken)
    {
        return await _context.Set<Invoice>()
            .AnyAsync(i => i.WorkOrderId == workOrderId, cancellationToken);
    }

    #region Helper Methods

    private IQueryable<Invoice> BuildInvoiceQuery()
    {
        return _context.Set<Invoice>()
            .AsNoTracking()
            .Include(i => i.WorkOrder)
                .ThenInclude(wo => wo.WorkOrderServices!)
                    .ThenInclude(ws => ws.Service)
            .Include(i => i.WorkOrder)
                .ThenInclude(wo => wo.WorkOrderParts!)
                    .ThenInclude(wp => wp.Part)
            .Include(i => i.Customer)
            .Include(i => i.Payments!)
                .ThenInclude(p => p.Method);
    }

    private static InvoiceResponseDto MapToInvoiceResponse(Invoice i)
    {
        var response = new InvoiceResponseDto
        {
            InvoiceId = i.InvoiceId,
            InvoiceCode = i.InvoiceCode,
            WorkOrderId = i.WorkOrderId,
            WorkOrderCode = i.WorkOrder?.WorkOrderCode,
            CustomerId = i.CustomerId,
            CustomerName = i.Customer!.FullName,
            CustomerEmail = i.Customer.Email,
            CustomerPhone = i.Customer.PhoneNumber,
            InvoiceDate = i.InvoiceDate ?? DateTime.UtcNow,
            DueDate = i.DueDate.HasValue ? i.DueDate.Value.ToDateTime(TimeOnly.MinValue) : DateTime.UtcNow,
            IsOverdue = i.DueDate.HasValue && i.DueDate.Value < DateOnly.FromDateTime(DateTime.UtcNow) && i.OutstandingAmount > 0,
            DaysOverdue = i.DueDate.HasValue && i.DueDate.Value < DateOnly.FromDateTime(DateTime.UtcNow) && i.OutstandingAmount > 0
                ? (int)(DateTime.UtcNow - i.DueDate.Value.ToDateTime(TimeOnly.MinValue)).TotalDays : 0,
            ServiceSubTotal = i.ServiceSubTotal ?? 0,
            ServiceDiscount = i.ServiceDiscount ?? 0,
            ServiceTax = i.ServiceTax ?? 0,
            ServiceTotal = i.ServiceTotal ?? 0,
            PartsSubTotal = i.PartsSubTotal ?? 0,
            PartsDiscount = i.PartsDiscount ?? 0,
            PartsTax = i.PartsTax ?? 0,
            PartsTotal = i.PartsTotal ?? 0,
            SubTotal = i.SubTotal ?? 0,
            TotalDiscount = i.TotalDiscount ?? 0,
            TotalTax = i.TotalTax ?? 0,
            GrandTotal = i.GrandTotal ?? 0,
            PaidAmount = i.PaidAmount ?? 0,
            OutstandingAmount = i.OutstandingAmount ?? 0,
            PaymentProgress = i.GrandTotal > 0 ? (i.PaidAmount ?? 0) / i.GrandTotal.Value * 100 : 0,
            Status = i.Status ?? "Draft",
            PaymentTerms = i.PaymentTerms,
            Notes = i.Notes,
            SentToCustomer = i.SentToCustomer ?? false,
            SentDate = i.SentDate,
            SentMethod = i.SentMethod,
            Payments = i.Payments!.Select(p => new InvoicePaymentDto
            {
                PaymentId = p.PaymentId,
                PaymentCode = p.PaymentCode,
                PaymentMethod = p.Method!.MethodName,
                Amount = p.Amount,
                PaymentDate = p.PaymentDate ?? DateTime.UtcNow,
                Status = p.Status ?? "Pending",
                TransactionRef = p.TransactionRef
            }).ToList(),
            CreatedDate = i.CreatedDate ?? DateTime.UtcNow,
            CreatedBy = i.CreatedBy,
            CreatedByName = null // TODO: Load from User if needed
        };

        if (i.WorkOrder?.WorkOrderServices != null)
        {
            response.ServiceLines = i.WorkOrder.WorkOrderServices.Select(ws => new InvoiceServiceLineDto
            {
                ServiceId = ws.ServiceId,
                ServiceCode = ws.Service!.ServiceCode,
                ServiceName = ws.Service.ServiceName,
                Description = ws.Service.Description,
                Quantity = 1,
                UnitPrice = ws.UnitPrice ?? 0,
                DiscountAmount = ws.DiscountAmount ?? 0,
                TaxAmount = 0,
                TotalPrice = ws.TotalPrice ?? 0
            }).ToList();
        }

        if (i.WorkOrder?.WorkOrderParts != null)
        {
            response.PartLines = i.WorkOrder.WorkOrderParts.Select(wp => new InvoicePartLineDto
            {
                PartId = wp.PartId,
                PartCode = wp.Part!.PartCode,
                PartName = wp.Part.PartName,
                Quantity = wp.Quantity,
                UnitPrice = wp.UnitPrice ?? 0,
                DiscountAmount = wp.DiscountAmount ?? 0,
                TaxAmount = 0,
                TotalPrice = wp.TotalPrice ?? 0,
                WarrantyPeriod = wp.WarrantyPeriod
            }).ToList();
        }

        return response;
    }

    private static string GenerateInvoiceCode()
    {
        return $"INV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
    }

    /// <summary>
    /// 🔧 FIX GAP #10: Check if customer or services are tax-exempt
    /// Tax-exempt cases:
    /// 1. Government/diplomatic customers
    /// 2. Tax-exempt service categories (e.g., educational, medical)
    /// 3. Special export transactions
    ///
    /// Note: For future enhancement, consider adding:
    /// - Customer.IsTaxExempt field
    /// - MaintenanceService.IsTaxExempt field
    /// - Service category-based exemptions
    /// </summary>
    private Task<bool> IsTaxExemptAsync(WorkOrder workOrder, CancellationToken cancellationToken)
    {
        // Configuration-based global tax exemption (for testing or special cases)
        var globalTaxExempt = _configuration.GetValue<bool>("Tax:GlobalExemption", false);

        // Future: Check customer type for tax exemption
        // var customerTypeTaxExempt = workOrder.Customer?.Type?.IsTaxExempt ?? false;

        // Future: Check service-level tax exemption
        // var hasTaxExemptServices = workOrder.WorkOrderServices?
        //     .Any(ws => ws.Service?.IsTaxExempt == true) ?? false;

        return Task.FromResult(globalTaxExempt);
    }

    #endregion
}
