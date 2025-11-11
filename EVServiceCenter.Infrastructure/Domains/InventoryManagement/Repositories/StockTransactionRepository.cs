using EVServiceCenter.Core.Domains.InventoryManagement.DTOs.Requests;
using EVServiceCenter.Core.Domains.InventoryManagement.DTOs.Responses;
using EVServiceCenter.Core.Domains.InventoryManagement.Interfaces;
using EVServiceCenter.Core.Domains.Shared.Models;
using EVServiceCenter.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EVServiceCenter.Infrastructure.Domains.InventoryManagement.Repositories;

/// <summary>
/// Repository for stock transaction operations with audit trail
/// </summary>
public class StockTransactionRepository : IStockTransactionRepository
{
    private readonly EVDbContext _context;
    private readonly ILogger<StockTransactionRepository> _logger;

    public StockTransactionRepository(
        EVDbContext context,
        ILogger<StockTransactionRepository> _logger)
    {
        _context = context;
        this._logger = _logger;
    }

    public async Task<PagedResult<StockTransactionResponseDto>> GetTransactionsAsync(
        StockTransactionQueryDto query,
        CancellationToken cancellationToken = default)
    {
        var baseQuery = _context.Set<StockTransaction>().AsNoTracking();

        // Apply filters
        if (query.PartId.HasValue)
            baseQuery = baseQuery.Where(st => st.PartId == query.PartId.Value);

        if (query.ServiceCenterId.HasValue)
            baseQuery = baseQuery.Where(st => st.CenterId == query.ServiceCenterId.Value);

        if (!string.IsNullOrEmpty(query.TransactionType))
            baseQuery = baseQuery.Where(st => st.TransactionType == query.TransactionType);

        if (!string.IsNullOrEmpty(query.ReferenceType))
            baseQuery = baseQuery.Where(st => st.ReferenceType == query.ReferenceType);

        if (query.SupplierId.HasValue)
            baseQuery = baseQuery.Where(st => st.SupplierId == query.SupplierId.Value);

        if (query.DateFrom.HasValue)
            baseQuery = baseQuery.Where(st => st.TransactionDate >= query.DateFrom.Value);

        if (query.DateTo.HasValue)
            baseQuery = baseQuery.Where(st => st.TransactionDate <= query.DateTo.Value);

        var totalCount = await baseQuery.CountAsync(cancellationToken);

        // Project to DTO
        var projectedQuery = baseQuery.Select(st => new StockTransactionResponseDto
        {
            TransactionId = st.TransactionId,
            PartId = st.PartId,
            PartCode = st.Part.PartCode,
            PartName = st.Part.PartName,
            ServiceCenterId = st.CenterId,
            ServiceCenterName = st.Center != null ? st.Center.CenterName : null,
            TransactionType = st.TransactionType,
            Quantity = st.Quantity,
            StockBefore = 0, // Will be calculated
            StockAfter = 0,  // Will be calculated
            UnitCost = st.UnitCost,
            TotalCost = st.TotalCost,
            ReferenceType = st.ReferenceType,
            ReferenceId = st.ReferenceId,
            ReferenceNumber = st.InvoiceNumber,
            SupplierId = st.SupplierId,
            SupplierName = st.Supplier != null ? st.Supplier.SupplierName : null,
            InvoiceNumber = st.InvoiceNumber,
            BatchNumber = st.BatchNumber,
            ExpiryDate = st.ExpiryDate,
            Notes = st.Notes,
            TransactionDate = st.TransactionDate ?? DateTime.UtcNow,
            CreatedBy = st.CreatedBy,
            CreatedByName = st.CreatedByNavigation != null ? st.CreatedByNavigation.FullName : null
        });

        // Apply sorting
        projectedQuery = query.SortDirection.ToLower() == "asc"
            ? projectedQuery.OrderBy(st => st.TransactionDate)
            : projectedQuery.OrderByDescending(st => st.TransactionDate);

        var items = await projectedQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<StockTransactionResponseDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
        };
    }

    public async Task<StockTransactionResponseDto?> GetByIdAsync(
        int transactionId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<StockTransaction>()
            .AsNoTracking()
            .Where(st => st.TransactionId == transactionId)
            .Select(st => new StockTransactionResponseDto
            {
                TransactionId = st.TransactionId,
                PartId = st.PartId,
                PartCode = st.Part.PartCode,
                PartName = st.Part.PartName,
                ServiceCenterId = st.CenterId,
                ServiceCenterName = st.Center != null ? st.Center.CenterName : null,
                TransactionType = st.TransactionType,
                Quantity = st.Quantity,
                UnitCost = st.UnitCost,
                TotalCost = st.TotalCost,
                ReferenceType = st.ReferenceType,
                ReferenceId = st.ReferenceId,
                ReferenceNumber = st.InvoiceNumber,
                SupplierId = st.SupplierId,
                SupplierName = st.Supplier != null ? st.Supplier.SupplierName : null,
                InvoiceNumber = st.InvoiceNumber,
                BatchNumber = st.BatchNumber,
                ExpiryDate = st.ExpiryDate,
                Notes = st.Notes,
                TransactionDate = st.TransactionDate ?? DateTime.UtcNow,
                CreatedBy = st.CreatedBy,
                CreatedByName = st.CreatedByNavigation != null ? st.CreatedByNavigation.FullName : null
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<StockTransactionResponseDto>> GetRecentTransactionsByPartAsync(
        int partId,
        int limit = 10,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<StockTransaction>()
            .AsNoTracking()
            .Where(st => st.PartId == partId)
            .OrderByDescending(st => st.TransactionDate)
            .Take(limit)
            .Select(st => new StockTransactionResponseDto
            {
                TransactionId = st.TransactionId,
                PartId = st.PartId,
                PartCode = st.Part.PartCode,
                PartName = st.Part.PartName,
                TransactionType = st.TransactionType,
                Quantity = st.Quantity,
                TransactionDate = st.TransactionDate ?? DateTime.UtcNow,
                Notes = st.Notes
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<StockTransactionResponseDto> CreateTransactionAsync(
        StockAdjustmentRequestDto request,
        int createdBy,
        CancellationToken cancellationToken = default)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // Create transaction record
            var stockTransaction = new StockTransaction
            {
                PartId = request.PartId,
                CenterId = request.ServiceCenterId,
                TransactionType = request.TransactionType,
                Quantity = request.Quantity,
                UnitCost = request.UnitCost,
                TotalCost = request.UnitCost.HasValue ? request.UnitCost.Value * Math.Abs(request.Quantity) : null,
                ReferenceType = request.ReferenceType,
                ReferenceId = request.ReferenceId,
                SupplierId = request.SupplierId,
                InvoiceNumber = request.InvoiceNumber,
                BatchNumber = request.BatchNumber,
                ExpiryDate = request.ExpiryDate,
                Notes = request.Notes,
                TransactionDate = DateTime.UtcNow,
                CreatedBy = createdBy
            };

            _context.Set<StockTransaction>().Add(stockTransaction);

            // Update inventory
            var inventory = await _context.Set<PartInventory>()
                .FirstOrDefaultAsync(pi => pi.PartId == request.PartId && pi.CenterId == request.ServiceCenterId, cancellationToken);

            if (inventory != null)
            {
                inventory.CurrentStock = (inventory.CurrentStock ?? 0) + request.Quantity;
                inventory.AvailableStock = (inventory.AvailableStock ?? 0) + request.Quantity;
                inventory.UpdatedDate = DateTime.UtcNow;
                inventory.UpdatedBy = createdBy;

                if (!string.IsNullOrEmpty(request.Location))
                    inventory.Location = request.Location;
            }

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation("Created stock transaction {Type} for Part {PartId}, Qty: {Qty}",
                request.TransactionType, request.PartId, request.Quantity);

            return await GetByIdAsync(stockTransaction.TransactionId, cancellationToken)
                ?? throw new InvalidOperationException("Failed to retrieve created transaction");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Error creating stock transaction");
            throw;
        }
    }

    public async Task<object> GetStockMovementSummaryAsync(
        int? serviceCenterId,
        DateTime? dateFrom,
        DateTime? dateTo,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Set<StockTransaction>().AsNoTracking();

        if (serviceCenterId.HasValue)
            query = query.Where(st => st.CenterId == serviceCenterId.Value);

        if (dateFrom.HasValue)
            query = query.Where(st => st.TransactionDate >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(st => st.TransactionDate <= dateTo.Value);

        var summary = await query
            .GroupBy(st => st.TransactionType)
            .Select(g => new
            {
                TransactionType = g.Key,
                TotalTransactions = g.Count(),
                TotalQuantity = g.Sum(st => st.Quantity),
                TotalValue = g.Sum(st => st.TotalCost ?? 0)
            })
            .ToListAsync(cancellationToken);

        return new
        {
            Summary = summary,
            Period = new { DateFrom = dateFrom, DateTo = dateTo },
            ServiceCenterId = serviceCenterId
        };
    }
}
