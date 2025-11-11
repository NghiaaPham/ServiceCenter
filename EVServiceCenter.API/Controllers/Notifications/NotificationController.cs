using EVServiceCenter.Core.Domains.Notifications.DTOs.Requests;
using EVServiceCenter.Core.Domains.Notifications.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVServiceCenter.API.Controllers.Notifications;

/// <summary>
/// Notification Management
/// Handles user notifications, read status, and delivery tracking
/// </summary>
[ApiController]
[Route("api/notifications")]
[ApiExplorerSettings(GroupName = "Notifications")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationController> _logger;

    public NotificationController(
        INotificationService notificationService,
        ILogger<NotificationController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>
    /// [List] Get notifications for current user with filtering and pagination
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/notifications?isRead=false&pageNumber=1&pageSize=20
    ///
    /// Query parameters:
    /// - isRead: Filter by read status (true = read, false = unread, null = all)
    /// - status: Filter by status (Pending, Sent, Delivered, Failed)
    /// - channel: Filter by channel (Email, SMS, InApp, Push)
    /// - priority: Filter by priority (High, Medium, Low)
    /// - relatedType: Filter by related type (Appointment, WorkOrder, Invoice, Payment)
    /// - startDate: Start date for filtering
    /// - endDate: End date for filtering
    /// - pageNumber: Page number (default: 1)
    /// - pageSize: Page size (default: 20, max: 100)
    /// - sortBy: Sort by (CreatedDate, SendDate, ReadDate)
    /// - sortDirection: Sort direction (Asc, Desc)
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] NotificationQueryDto query,
        CancellationToken cancellationToken)
    {
        try
        {
            var (userId, customerId) = GetCurrentUserInfo();

            var result = await _notificationService.GetUserNotificationsAsync(
                userId,
                customerId,
                query,
                cancellationToken);

            return Ok(new
            {
                success = true,
                data = result,
                message = "Notifications retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving notifications");
            return StatusCode(500, new
            {
                success = false,
                message = "Error retrieving notifications"
            });
        }
    }

    /// <summary>
    /// [Details] Get notification by ID
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetNotification(
        int id,
        CancellationToken cancellationToken)
    {
        try
        {
            var (userId, customerId) = GetCurrentUserInfo();

            var notification = await _notificationService.GetNotificationByIdAsync(
                id,
                userId,
                customerId,
                cancellationToken);

            return Ok(new
            {
                success = true,
                data = notification
            });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new
            {
                success = false,
                message = $"Notification {id} not found or access denied"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving notification {NotificationId}", id);
            return StatusCode(500, new
            {
                success = false,
                message = "Error retrieving notification"
            });
        }
    }

    /// <summary>
    /// [Update] Mark notification as read
    /// </summary>
    [HttpPut("{id:int}/read")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(
        int id,
        CancellationToken cancellationToken)
    {
        try
        {
            var (userId, customerId) = GetCurrentUserInfo();

            var success = await _notificationService.MarkAsReadAsync(
                id,
                userId,
                customerId,
                cancellationToken);

            if (!success)
            {
                return NotFound(new
                {
                    success = false,
                    message = $"Notification {id} not found or access denied"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Notification marked as read"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification {NotificationId} as read", id);
            return StatusCode(500, new
            {
                success = false,
                message = "Error updating notification"
            });
        }
    }

    /// <summary>
    /// [Update] Mark all notifications as read
    /// </summary>
    [HttpPut("read-all")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken)
    {
        try
        {
            var (userId, customerId) = GetCurrentUserInfo();

            var count = await _notificationService.MarkAllAsReadAsync(
                userId,
                customerId,
                cancellationToken);

            return Ok(new
            {
                success = true,
                data = new { markedCount = count },
                message = $"{count} notifications marked as read"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking all notifications as read");
            return StatusCode(500, new
            {
                success = false,
                message = "Error updating notifications"
            });
        }
    }

    /// <summary>
    /// [Quick] Get unread count
    /// </summary>
    [HttpGet("unread-count")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUnreadCount(CancellationToken cancellationToken)
    {
        try
        {
            var (userId, customerId) = GetCurrentUserInfo();

            var count = await _notificationService.GetUnreadCountAsync(
                userId,
                customerId,
                cancellationToken);

            return Ok(new
            {
                success = true,
                data = new { unreadCount = count }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread count");
            return StatusCode(500, new
            {
                success = false,
                message = "Error retrieving unread count"
            });
        }
    }

    /// <summary>
    /// [Delete] Delete notification (soft delete)
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteNotification(
        int id,
        CancellationToken cancellationToken)
    {
        try
        {
            var (userId, customerId) = GetCurrentUserInfo();

            var success = await _notificationService.DeleteNotificationAsync(
                id,
                userId,
                customerId,
                cancellationToken);

            if (!success)
            {
                return NotFound(new
                {
                    success = false,
                    message = $"Notification {id} not found or access denied"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Notification deleted successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting notification {NotificationId}", id);
            return StatusCode(500, new
            {
                success = false,
                message = "Error deleting notification"
            });
        }
    }

    #region Helper Methods

    private (int userId, int? customerId) GetCurrentUserInfo()
    {
        var userIdClaim = User.FindFirst("UserId") ?? User.FindFirst("sub");
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            throw new UnauthorizedAccessException("User ID not found in token");
        }

        var customerIdClaim = User.FindFirst("CustomerId");
        int? customerId = null;
        if (customerIdClaim != null && int.TryParse(customerIdClaim.Value, out int custId))
        {
            customerId = custId;
        }

        return (userId, customerId);
    }

    #endregion
}
