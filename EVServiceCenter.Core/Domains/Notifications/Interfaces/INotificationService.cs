using EVServiceCenter.Core.Domains.Notifications.DTOs.Requests;
using EVServiceCenter.Core.Domains.Notifications.DTOs.Responses;

namespace EVServiceCenter.Core.Domains.Notifications.Interfaces;

/// <summary>
/// Notification service interface
/// Handles notification CRUD and delivery operations
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Get notifications for current user with filtering and pagination
    /// </summary>
    Task<NotificationListResponseDto> GetUserNotificationsAsync(
        int userId,
        int? customerId,
        NotificationQueryDto query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get notification by ID
    /// </summary>
    Task<NotificationResponseDto> GetNotificationByIdAsync(
        int notificationId,
        int userId,
        int? customerId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Mark notification as read
    /// </summary>
    Task<bool> MarkAsReadAsync(
        int notificationId,
        int userId,
        int? customerId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Mark all notifications as read
    /// </summary>
    Task<int> MarkAllAsReadAsync(
        int userId,
        int? customerId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get unread count
    /// </summary>
    Task<int> GetUnreadCountAsync(
        int userId,
        int? customerId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete notification (soft delete by updating status)
    /// </summary>
    Task<bool> DeleteNotificationAsync(
        int notificationId,
        int userId,
        int? customerId,
        CancellationToken cancellationToken = default);
}
