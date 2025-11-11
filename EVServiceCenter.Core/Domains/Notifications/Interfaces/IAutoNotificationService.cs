namespace EVServiceCenter.Core.Domains.Notifications.Interfaces;

/// <summary>
/// Auto notification service interface
/// Handles automatic notification creation based on rules and templates
/// </summary>
public interface IAutoNotificationService
{
    /// <summary>
    /// Process auto notification rules and create notifications
    /// </summary>
    Task ProcessAutoNotificationsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Create notification from appointment event
    /// </summary>
    Task CreateAppointmentNotificationAsync(
        int appointmentId,
        string eventType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Create notification from work order event
    /// </summary>
    Task CreateWorkOrderNotificationAsync(
        int workOrderId,
        string eventType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Create notification from invoice event
    /// </summary>
    Task CreateInvoiceNotificationAsync(
        int invoiceId,
        string eventType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send scheduled notifications
    /// </summary>
    Task SendScheduledNotificationsAsync(CancellationToken cancellationToken = default);
}
