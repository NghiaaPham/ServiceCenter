using EVServiceCenter.Core.Domains.ServiceRatings.DTOs.Requests;
using EVServiceCenter.Core.Domains.ServiceRatings.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVServiceCenter.API.Controllers.ServiceRatings;

/// <summary>
/// Service Rating Management
/// Handles service ratings and reviews for completed work orders
/// </summary>
[ApiController]
[Route("api")]
[ApiExplorerSettings(GroupName = "Service Ratings")]
public class ServiceRatingController : ControllerBase
{
    private readonly IServiceRatingService _serviceRatingService;
    private readonly ILogger<ServiceRatingController> _logger;

    public ServiceRatingController(
        IServiceRatingService serviceRatingService,
        ILogger<ServiceRatingController> logger)
    {
        _serviceRatingService = serviceRatingService;
        _logger = logger;
    }

    /// <summary>
    /// [Create] Submit rating for work order
    /// Original requirement: POST /api/appointments/{id}/rating
    /// Note: Using work-orders instead of appointments as per system design
    /// </summary>
    [HttpPost("work-orders/{workOrderId:int}/rating")]
    [Authorize(Roles = "Customer")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateRating(
        int workOrderId,
        [FromBody] CreateServiceRatingRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var customerId = GetCurrentCustomerId();

            // Override workOrderId from route
            request.WorkOrderId = workOrderId;

            var rating = await _serviceRatingService.CreateRatingAsync(
                request,
                customerId,
                cancellationToken);

            return CreatedAtAction(
                nameof(GetRating),
                new { id = rating.RatingId },
                new
                {
                    success = true,
                    data = rating,
                    message = "Rating submitted successfully"
                });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating rating for WorkOrder {WorkOrderId}", workOrderId);
            return StatusCode(500, new
            {
                success = false,
                message = "Error submitting rating"
            });
        }
    }

    /// <summary>
    /// [Details] Get rating by ID
    /// </summary>
    [HttpGet("ratings/{id:int}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRating(
        int id,
        CancellationToken cancellationToken)
    {
        try
        {
            var rating = await _serviceRatingService.GetRatingByIdAsync(id, cancellationToken);

            if (rating == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = $"Rating {id} not found"
                });
            }

            return Ok(new
            {
                success = true,
                data = rating
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving rating {RatingId}", id);
            return StatusCode(500, new
            {
                success = false,
                message = "Error retrieving rating"
            });
        }
    }

    /// <summary>
    /// [Details] Get rating by work order ID
    /// </summary>
    [HttpGet("work-orders/{workOrderId:int}/rating")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWorkOrderRating(
        int workOrderId,
        CancellationToken cancellationToken)
    {
        try
        {
            var rating = await _serviceRatingService.GetRatingByWorkOrderIdAsync(
                workOrderId,
                cancellationToken);

            if (rating == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = $"No rating found for WorkOrder {workOrderId}"
                });
            }

            return Ok(new
            {
                success = true,
                data = rating
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving rating for WorkOrder {WorkOrderId}", workOrderId);
            return StatusCode(500, new
            {
                success = false,
                message = "Error retrieving rating"
            });
        }
    }

    /// <summary>
    /// [List] Get service center ratings summary
    /// Original requirement: GET /api/service-centers/{id}/ratings
    /// </summary>
    [HttpGet("service-centers/{serviceCenterId:int}/ratings")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetServiceCenterRatings(
        int serviceCenterId,
        [FromQuery] int? top = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var ratings = await _serviceRatingService.GetServiceCenterRatingsAsync(
                serviceCenterId,
                top,
                cancellationToken);

            return Ok(new
            {
                success = true,
                data = ratings,
                message = "Service center ratings retrieved successfully"
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving ratings for ServiceCenter {ServiceCenterId}",
                serviceCenterId);
            return StatusCode(500, new
            {
                success = false,
                message = "Error retrieving ratings"
            });
        }
    }

    #region Helper Methods

    private int GetCurrentCustomerId()
    {
        var customerIdClaim = User.FindFirst("CustomerId");
        if (customerIdClaim != null && int.TryParse(customerIdClaim.Value, out int customerId))
        {
            return customerId;
        }

        throw new UnauthorizedAccessException("Customer ID not found in token");
    }

    #endregion
}
