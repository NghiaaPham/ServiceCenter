using EVServiceCenter.Core.Domains.Invoices.DTOs.Requests;
using EVServiceCenter.Core.Domains.Invoices.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVServiceCenter.API.Controllers.Invoices;

/// <summary>
/// Invoice Management
/// Handles invoice CRUD operations and sending
/// </summary>
[ApiController]
[Route("api/invoices")]
[ApiExplorerSettings(GroupName = "Invoice & Payment")]
[Authorize]
public class InvoiceController : ControllerBase
{
    private readonly IInvoiceService _invoiceService;
    private readonly ILogger<InvoiceController> _logger;

    public InvoiceController(IInvoiceService invoiceService, ILogger<InvoiceController> logger)
    {
        _invoiceService = invoiceService;
        _logger = logger;
    }

    /// <summary>
    /// [List] Get all invoices with filtering and pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInvoices(
        [FromQuery] InvoiceQueryDto query, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _invoiceService.GetInvoicesAsync(query, cancellationToken);
            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting invoices");
            return StatusCode(500, new { success = false, message = "Error retrieving invoices" });
        }
    }

    /// <summary>
    /// [Details] Get invoice by ID
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInvoice(int id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _invoiceService.GetInvoiceByIdAsync(id, cancellationToken);

            // SECURITY: Verify ownership for customer role
            if (!await VerifyInvoiceOwnershipAsync(result.InvoiceId, result.CustomerId, cancellationToken))
            {
                _logger.LogWarning("User {UserId} attempted to access invoice {InvoiceId} without ownership",
                    GetCurrentUserId(), id);
                return Forbid();
            }

            return Ok(new { success = true, data = result });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { success = false, message = $"Invoice {id} not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting invoice {InvoiceId}", id);
            return StatusCode(500, new { success = false, message = "Error retrieving invoice" });
        }
    }

    /// <summary>
    /// [Details] Get invoice by code
    /// </summary>
    [HttpGet("by-code/{code}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInvoiceByCode(string code, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _invoiceService.GetInvoiceByCodeAsync(code, cancellationToken);

            // SECURITY: Verify ownership for customer role
            if (!await VerifyInvoiceOwnershipAsync(result.InvoiceId, result.CustomerId, cancellationToken))
            {
                _logger.LogWarning("User {UserId} attempted to access invoice {Code} without ownership",
                    GetCurrentUserId(), code);
                return Forbid();
            }

            return Ok(new { success = true, data = result });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { success = false, message = $"Invoice {code} not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting invoice by code {Code}", code);
            return StatusCode(500, new { success = false, message = "Error retrieving invoice" });
        }
    }

    /// <summary>
    /// [Details] Get invoice by work order ID
    /// </summary>
    [HttpGet("by-work-order/{workOrderId:int}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInvoiceByWorkOrder(int workOrderId, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _invoiceService.GetInvoiceByWorkOrderIdAsync(workOrderId, cancellationToken);

            // SECURITY: Verify ownership for customer role
            if (!await VerifyInvoiceOwnershipAsync(result.InvoiceId, result.CustomerId, cancellationToken))
            {
                _logger.LogWarning("User {UserId} attempted to access invoice for WorkOrder {WorkOrderId} without ownership",
                    GetCurrentUserId(), workOrderId);
                return Forbid();
            }

            return Ok(new { success = true, data = result });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { success = false, message = $"No invoice found for WorkOrder {workOrderId}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting invoice for WorkOrder {WorkOrderId}", workOrderId);
            return StatusCode(500, new { success = false, message = "Error retrieving invoice" });
        }
    }

    /// <summary>
    /// [Create] Generate invoice from work order
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager,Staff")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerateInvoice(
        [FromBody] GenerateInvoiceRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _invoiceService.GenerateInvoiceFromWorkOrderAsync(request, userId, cancellationToken);

            return CreatedAtAction(
                nameof(GetInvoice),
                new { id = result.InvoiceId },
                new { success = true, data = result, message = "Invoice generated successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating invoice");
            return StatusCode(500, new { success = false, message = "Error generating invoice" });
        }
    }

    /// <summary>
    /// [Update] Update invoice details
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager,Staff")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateInvoice(
        int id, [FromBody] UpdateInvoiceRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _invoiceService.UpdateInvoiceAsync(id, request, cancellationToken);
            return Ok(new { success = true, data = result, message = "Invoice updated successfully" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { success = false, message = $"Invoice {id} not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating invoice {InvoiceId}", id);
            return StatusCode(500, new { success = false, message = "Error updating invoice" });
        }
    }

    /// <summary>
    /// [Send] Send invoice to customer via email/SMS
    /// </summary>
    [HttpPost("{id:int}/send")]
    [Authorize(Roles = "Admin,Manager,Staff")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SendInvoice(
        int id, [FromBody] SendInvoiceRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _invoiceService.SendInvoiceToCustomerAsync(id, request, cancellationToken);
            return Ok(new { success = true, data = result, message = $"Invoice sent via {request.SendMethod}" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { success = false, message = $"Invoice {id} not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending invoice {InvoiceId}", id);
            return StatusCode(500, new { success = false, message = "Error sending invoice" });
        }
    }

    /// <summary>
    /// [PDF] Download invoice as PDF
    /// </summary>
    [HttpGet("{id:int}/pdf")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadInvoicePdf(int id, CancellationToken cancellationToken)
    {
        try
        {
            var invoice = await _invoiceService.GetInvoiceByIdAsync(id, cancellationToken);

            // SECURITY: Verify ownership for customer role before generating PDF
            if (!await VerifyInvoiceOwnershipAsync(invoice.InvoiceId, invoice.CustomerId, cancellationToken))
            {
                _logger.LogWarning("User {UserId} attempted to download PDF for invoice {InvoiceId} without ownership",
                    GetCurrentUserId(), id);
                return Forbid();
            }

            var pdfBytes = await _invoiceService.GenerateInvoicePdfAsync(id, cancellationToken);
            return File(pdfBytes, "application/pdf", $"{invoice.InvoiceCode}.pdf");
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { success = false, message = $"Invoice {id} not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating PDF for invoice {InvoiceId}", id);
            return StatusCode(500, new { success = false, message = "Error generating PDF" });
        }
    }

    /// <summary>
    /// [Cancel] Cancel invoice
    /// </summary>
    [HttpPost("{id:int}/cancel")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelInvoice(int id, [FromBody] string reason, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _invoiceService.CancelInvoiceAsync(id, reason, cancellationToken);
            return Ok(new { success = true, message = "Invoice cancelled successfully" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { success = false, message = $"Invoice {id} not found" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling invoice {InvoiceId}", id);
            return StatusCode(500, new { success = false, message = "Error cancelling invoice" });
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

    /// <summary>
    /// Verify that the current user owns the invoice
    /// Staff/Admin/Manager bypass this check (via role)
    /// Customers must own the invoice via CustomerId claim
    /// </summary>
    private async Task<bool> VerifyInvoiceOwnershipAsync(int invoiceId, int invoiceCustomerId, CancellationToken cancellationToken)
    {
        // Staff and above can access any invoice
        if (User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Staff"))
        {
            return true;
        }

        // For customers, verify ownership via CustomerId
        var customerIdClaim = User.FindFirst("CustomerId");
        if (customerIdClaim == null || !int.TryParse(customerIdClaim.Value, out int customerId))
        {
            _logger.LogWarning("CustomerId claim not found for user {UserId}", GetCurrentUserId());
            return false;
        }

        return invoiceCustomerId == customerId;
    }

    #endregion
}
