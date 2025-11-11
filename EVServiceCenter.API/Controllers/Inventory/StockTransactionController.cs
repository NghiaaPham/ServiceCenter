using EVServiceCenter.Core.Domains.InventoryManagement.DTOs.Requests;
using EVServiceCenter.Core.Domains.InventoryManagement.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVServiceCenter.API.Controllers.Inventory;

/// <summary>
/// Stock Transaction Management
/// Handles stock IN/OUT/ADJUST operations and transaction history
/// </summary>
[ApiController]
[Route("api/stock-transactions")]
[ApiExplorerSettings(GroupName = "Inventory Management")]
[Authorize]
public class StockTransactionController : ControllerBase
{
    private readonly IInventoryService _inventoryService;
    private readonly ILogger<StockTransactionController> _logger;

    public StockTransactionController(
        IInventoryService inventoryService,
        ILogger<StockTransactionController> logger)
    {
        _inventoryService = inventoryService;
        _logger = logger;
    }

    /// <summary>
    /// [Create] Adjust stock levels (IN/OUT/ADJUST/TRANSFER)
    /// </summary>
    /// <remarks>
    /// **Transaction Types:**
    /// - **IN**: Receive stock from supplier (positive quantity)
    /// - **OUT**: Issue stock for work order (positive quantity, auto-converted to negative)
    /// - **ADJUST**: Manual correction (positive or negative)
    /// - **TRANSFER**: Move between service centers
    ///
    /// **Business Rules:**
    /// - IN transactions: Supplier and UnitCost recommended
    /// - OUT transactions: Validates sufficient stock before deduction
    /// - All transactions create audit trail
    /// - Inventory updated atomically with transaction
    ///
    /// **Request Fields:**
    /// - PartId, ServiceCenterId, TransactionType, Quantity: Required
    /// - UnitCost: Required for IN
    /// - SupplierId, InvoiceNumber: Optional for IN
    /// - ReferenceType, ReferenceId: For WO, PO linkage
    /// - BatchNumber, ExpiryDate: For batch tracking
    /// - Location: Storage location update
    /// - Notes: Transaction reason/notes
    /// </remarks>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager,Staff")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AdjustStock(
        [FromBody] StockAdjustmentRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _inventoryService.AdjustStockAsync(request, userId, cancellationToken);

            return CreatedAtAction(
                nameof(GetTransactionById),
                new { id = result.TransactionId },
                new { success = true, data = result, message = "Stock adjusted successfully" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adjusting stock");
            return StatusCode(500, new { success = false, message = "Error adjusting stock" });
        }
    }

    /// <summary>
    /// [List] Get transaction history with filtering
    /// </summary>
    /// <remarks>
    /// **Filters:**
    /// - PartId: Filter by specific part
    /// - ServiceCenterId: Filter by service center
    /// - TransactionType: IN, OUT, ADJUST, TRANSFER
    /// - ReferenceType: PO, WO, ADJ
    /// - SupplierId: Filter by supplier
    /// - DateFrom, DateTo: Date range filter
    ///
    /// **Sorting:**
    /// - SortBy: transactionDate (default), quantity, totalCost
    /// - SortDirection: desc (default), asc
    ///
    /// **Response includes:**
    /// - Transaction details with before/after stock levels
    /// - Part and service center information
    /// - Supplier information (for IN transactions)
    /// - Reference linkage (WO, PO)
    /// - Audit trail (who created, when)
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTransactions(
        [FromQuery] StockTransactionQueryDto query,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _inventoryService.GetTransactionHistoryAsync(query, cancellationToken);
            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transaction history");
            return StatusCode(500, new { success = false, message = "Error retrieving transactions" });
        }
    }

    /// <summary>
    /// [Details] Get transaction by ID
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTransactionById(
        int id,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = new StockTransactionQueryDto { Page = 1, PageSize = 1 };
            var result = await _inventoryService.GetTransactionHistoryAsync(query, cancellationToken);

            // Note: Ideally we'd have GetByIdAsync on service, but for now filter from query
            var transaction = result.Items.FirstOrDefault(t => t.TransactionId == id);

            if (transaction == null)
            {
                return NotFound(new { success = false, message = $"Transaction {id} not found" });
            }

            return Ok(new { success = true, data = transaction });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transaction {TransactionId}", id);
            return StatusCode(500, new { success = false, message = "Error retrieving transaction" });
        }
    }

    /// <summary>
    /// [History] Get recent transactions for a specific part
    /// </summary>
    /// <remarks>
    /// Returns last N transactions for a part, useful for:
    /// - Part usage analysis
    /// - Quick audit trail
    /// - Stock movement pattern
    ///
    /// **Default:** Last 10 transactions
    /// **Max:** 50 transactions
    /// </remarks>
    [HttpGet("part/{partId:int}/recent")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecentTransactionsByPart(
        int partId,
        CancellationToken cancellationToken,
        [FromQuery] int limit = 10)
    {
        try
        {
            if (limit > 50)
            {
                limit = 50;
            }

            var transactions = await _inventoryService.GetRecentTransactionsByPartAsync(
                partId, limit, cancellationToken);

            return Ok(new { success = true, data = transactions, count = transactions.Count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent transactions for Part {PartId}", partId);
            return StatusCode(500, new { success = false, message = "Error retrieving transactions" });
        }
    }

    /// <summary>
    /// [Report] Get stock movement summary for period
    /// </summary>
    /// <remarks>
    /// Aggregated stock movement statistics:
    /// - Total IN transactions (quantity, value)
    /// - Total OUT transactions (quantity, value)
    /// - Total ADJUST transactions
    /// - Net movement
    ///
    /// **Use Cases:**
    /// - Monthly inventory reports
    /// - Stock turnover analysis
    /// - Purchasing trends
    ///
    /// **Default Period:** Current month
    /// </remarks>
    [HttpGet("movement-summary")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStockMovementSummary(
        [FromQuery] int? serviceCenterId,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        CancellationToken cancellationToken)
    {
        try
        {
            // Default to current month if no dates provided
            dateFrom ??= new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            dateTo ??= DateTime.UtcNow;

            var summary = await _inventoryService.GetStockMovementSummaryAsync(
                serviceCenterId, dateFrom, dateTo, cancellationToken);

            return Ok(new { success = true, data = summary });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stock movement summary");
            return StatusCode(500, new { success = false, message = "Error retrieving summary" });
        }
    }

    #region Helper Methods

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("UserId") ?? User.FindFirst("sub");
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
        {
            return userId;
        }
        throw new UnauthorizedAccessException("User ID not found in token");
    }

    #endregion
}
