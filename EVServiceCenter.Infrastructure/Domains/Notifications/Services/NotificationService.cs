using EVServiceCenter.Core.Domains.Notifications.DTOs.Requests;
using EVServiceCenter.Core.Domains.Notifications.DTOs.Responses;
using EVServiceCenter.Core.Domains.Notifications.Interfaces;
using EVServiceCenter.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EVServiceCenter.Infrastructure.Domains.Notifications.Services;

/// <summary>
/// Notification service implementation
/// Handles notification retrieval and status updates with performance optimization
/// </summary>
public class NotificationService : INotificationService
{
    private readonly EVDbContext _context;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        EVDbContext context,
        ILogger<NotificationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<NotificationListResponseDto> GetUserNotificationsAsync(
        int userId,
        int? customerId,
        NotificationQueryDto query,
        CancellationToken cancellationToken = default)
    {
        // Build base query with AsNoTracking for performance
        var baseQuery = _context.Set<Notification>()
            .AsNoTracking()
            .Where(n => (n.UserId == userId) || (customerId.HasValue && n.CustomerId == customerId.Value));

        // Apply filters
        if (query.IsRead.HasValue)
        {
            if (query.IsRead.Value)
                baseQuery = baseQuery.Where(n => n.ReadDate != null);
            else
                baseQuery = baseQuery.Where(n => n.ReadDate == null);
        }

        if (!string.IsNullOrEmpty(query.Status))
        {
            baseQuery = baseQuery.Where(n => n.Status == query.Status);
        }

        if (!string.IsNullOrEmpty(query.Channel))
        {
            baseQuery = baseQuery.Where(n => n.Channel == query.Channel);
        }

        if (!string.IsNullOrEmpty(query.Priority))
        {
            baseQuery = baseQuery.Where(n => n.Priority == query.Priority);
        }

        if (!string.IsNullOrEmpty(query.RelatedType))
        {
            baseQuery = baseQuery.Where(n => n.RelatedType == query.RelatedType);
        }

        if (query.StartDate.HasValue)
        {
            baseQuery = baseQuery.Where(n => n.CreatedDate >= query.StartDate.Value);
        }

        if (query.EndDate.HasValue)
        {
            baseQuery = baseQuery.Where(n => n.CreatedDate <= query.EndDate.Value);
        }

        // Materialize counts sequentially to avoid DbContext concurrency issues
        var trackingQuery = baseQuery.AsNoTracking();
        var totalCount = await trackingQuery.CountAsync(cancellationToken);
        var unreadCount = await trackingQuery.Where(n => n.ReadDate == null).CountAsync(cancellationToken);

        // Apply sorting
        var sortedQuery = trackingQuery;
        sortedQuery = query.SortBy?.ToLower() switch
        {
            "senddate" => query.SortDirection.ToLower() == "asc"
                ? sortedQuery.OrderBy(n => n.SendDate)
                : sortedQuery.OrderByDescending(n => n.SendDate),
            "readdate" => query.SortDirection.ToLower() == "asc"
                ? sortedQuery.OrderBy(n => n.ReadDate)
                : sortedQuery.OrderByDescending(n => n.ReadDate),
            _ => query.SortDirection.ToLower() == "asc"
                ? sortedQuery.OrderBy(n => n.CreatedDate)
                : sortedQuery.OrderByDescending(n => n.CreatedDate)
        };

        // Apply pagination
        var pageSize = Math.Min(query.PageSize, 100); // Max 100 per page
        var skip = (query.PageNumber - 1) * pageSize;

        var notifications = await sortedQuery
            .Skip(skip)
            .Take(pageSize)
            .Select(n => new NotificationResponseDto
            {
                NotificationId = n.NotificationId,
                NotificationCode = n.NotificationCode,
                Channel = n.Channel,
                Priority = n.Priority,
                Subject = n.Subject,
                Message = n.Message,
                ScheduledDate = n.ScheduledDate,
                SendDate = n.SendDate,
                DeliveredDate = n.DeliveredDate,
                ReadDate = n.ReadDate,
                Status = n.Status,
                RelatedType = n.RelatedType,
                RelatedId = n.RelatedId,
                CreatedDate = n.CreatedDate
            })
            .ToListAsync(cancellationToken);

        return new NotificationListResponseDto
        {
            Notifications = notifications,
            TotalCount = totalCount,
            UnreadCount = unreadCount,
            PageNumber = query.PageNumber,
            PageSize = pageSize
        };
    }

    public async Task<NotificationResponseDto> GetNotificationByIdAsync(
        int notificationId,
        int userId,
        int? customerId,
        CancellationToken cancellationToken = default)
    {
        var notification = await _context.Set<Notification>()
            .AsNoTracking()
            .Where(n => n.NotificationId == notificationId
                && ((n.UserId == userId) || (customerId.HasValue && n.CustomerId == customerId.Value)))
            .Select(n => new NotificationResponseDto
            {
                NotificationId = n.NotificationId,
                NotificationCode = n.NotificationCode,
                Channel = n.Channel,
                Priority = n.Priority,
                Subject = n.Subject,
                Message = n.Message,
                ScheduledDate = n.ScheduledDate,
                SendDate = n.SendDate,
                DeliveredDate = n.DeliveredDate,
                ReadDate = n.ReadDate,
                Status = n.Status,
                RelatedType = n.RelatedType,
                RelatedId = n.RelatedId,
                CreatedDate = n.CreatedDate
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (notification == null)
        {
            throw new KeyNotFoundException($"Notification {notificationId} not found or access denied");
        }

        return notification;
    }

    public async Task<bool> MarkAsReadAsync(
        int notificationId,
        int userId,
        int? customerId,
        CancellationToken cancellationToken = default)
    {
        var notification = await _context.Set<Notification>()
            .Where(n => n.NotificationId == notificationId
                && ((n.UserId == userId) || (customerId.HasValue && n.CustomerId == customerId.Value)))
            .FirstOrDefaultAsync(cancellationToken);

        if (notification == null)
        {
            _logger.LogWarning("Notification {NotificationId} not found or access denied for user {UserId}",
                notificationId, userId);
            return false;
        }

        if (notification.ReadDate.HasValue)
        {
            // Already read
            return true;
        }

        notification.ReadDate = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Notification {NotificationId} marked as read by user {UserId}",
            notificationId, userId);

        return true;
    }

    public async Task<int> MarkAllAsReadAsync(
        int userId,
        int? customerId,
        CancellationToken cancellationToken = default)
    {
        var unreadNotifications = await _context.Set<Notification>()
            .Where(n => ((n.UserId == userId) || (customerId.HasValue && n.CustomerId == customerId.Value))
                && n.ReadDate == null)
            .ToListAsync(cancellationToken);

        if (!unreadNotifications.Any())
        {
            return 0;
        }

        var readTime = DateTime.UtcNow;
        foreach (var notification in unreadNotifications)
        {
            notification.ReadDate = readTime;
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Marked {Count} notifications as read for user {UserId}",
            unreadNotifications.Count, userId);

        return unreadNotifications.Count;
    }

    public async Task<int> GetUnreadCountAsync(
        int userId,
        int? customerId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<Notification>()
            .AsNoTracking()
            .CountAsync(n => ((n.UserId == userId) || (customerId.HasValue && n.CustomerId == customerId.Value))
                && n.ReadDate == null,
                cancellationToken);
    }

    public async Task<bool> DeleteNotificationAsync(
        int notificationId,
        int userId,
        int? customerId,
        CancellationToken cancellationToken = default)
    {
        var notification = await _context.Set<Notification>()
            .Where(n => n.NotificationId == notificationId
                && ((n.UserId == userId) || (customerId.HasValue && n.CustomerId == customerId.Value)))
            .FirstOrDefaultAsync(cancellationToken);

        if (notification == null)
        {
            return false;
        }

        // Soft delete by updating status
        notification.Status = "Deleted";
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Notification {NotificationId} deleted by user {UserId}",
            notificationId, userId);

        return true;
    }
}
