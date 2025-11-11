using EVServiceCenter.Core.Domains.AppointmentManagement.Entities;
using EVServiceCenter.Core.Domains.Notifications.Interfaces;
using EVServiceCenter.Core.Entities;
using EVServiceCenter.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EVServiceCenter.Infrastructure.Domains.Notifications.Services;

/// <summary>
/// Auto notification service implementation
/// Processes notification rules and creates automatic notifications
/// </summary>
public class AutoNotificationService : IAutoNotificationService
{
    private readonly EVDbContext _context;
    private readonly ILogger<AutoNotificationService> _logger;
    private readonly SubscriptionRenewalReminderOptions _renewalOptions;

    public AutoNotificationService(
        EVDbContext context,
        ILogger<AutoNotificationService> logger,
        IOptions<SubscriptionRenewalReminderOptions>? renewalOptions = null)
    {
        _context = context;
        _logger = logger;
        _renewalOptions = renewalOptions?.Value ?? new SubscriptionRenewalReminderOptions();
    }

    public async Task ProcessAutoNotificationsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing auto notifications...");

            // Get active auto notification rules
            var activeRules = await _context.Set<AutoNotificationRule>()
                .Include(r => r.Template)
                .Where(r => r.IsActive == true)
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Found {Count} active notification rules", activeRules.Count);

            // Process each rule
            foreach (var rule in activeRules)
            {
                await ProcessRuleAsync(rule, cancellationToken);
            }

            await ProcessSubscriptionRenewalRemindersAsync(cancellationToken);

            _logger.LogInformation("Completed processing auto notifications");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing auto notifications");
        }
    }

    public async Task CreateAppointmentNotificationAsync(
        int appointmentId,
        string eventType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var appointment = await _context.Set<Appointment>()
                .Include(a => a.Customer)
                .Include(a => a.Slot)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId, cancellationToken);

            if (appointment == null)
            {
                _logger.LogWarning("Appointment {AppointmentId} not found", appointmentId);
                return;
            }

            // Find matching template
            var template = await _context.Set<NotificationTemplate>()
                .FirstOrDefaultAsync(t =>
                    t.TriggerEvent == eventType &&
                    t.IsActive == true &&
                    t.IsAutomatic == true,
                    cancellationToken);

            if (template == null)
            {
                _logger.LogDebug("No template found for event {EventType}", eventType);
                return;
            }

            // Create notification
            var notification = new Notification
            {
                NotificationCode = $"APT-{appointmentId}-{DateTime.UtcNow:yyyyMMddHHmmss}",
                TemplateId = template.TemplateId,
                UserId = appointment.Customer?.UserId,
                CustomerId = appointment.CustomerId,
                Channel = template.Channel,
                Priority = "Medium",
                Subject = ReplaceTemplatePlaceholders(template.Subject ?? "", appointment),
                Message = ReplaceTemplatePlaceholders(template.MessageTemplate ?? "", appointment),
                RelatedType = "Appointment",
                RelatedId = appointmentId,
                ScheduledDate = template.SendDelay.HasValue
                    ? DateTime.UtcNow.AddMinutes(template.SendDelay.Value)
                    : DateTime.UtcNow,
                Status = "Pending",
                CreatedDate = DateTime.UtcNow
            };

            _context.Set<Notification>().Add(notification);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created notification for appointment {AppointmentId}, event {EventType}",
                appointmentId, eventType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating appointment notification for {AppointmentId}", appointmentId);
        }
    }

    public async Task CreateWorkOrderNotificationAsync(
        int workOrderId,
        string eventType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var workOrder = await _context.Set<WorkOrder>()
                .Include(w => w.Customer)
                .Include(w => w.Status)
                .FirstOrDefaultAsync(w => w.WorkOrderId == workOrderId, cancellationToken);

            if (workOrder == null)
            {
                _logger.LogWarning("WorkOrder {WorkOrderId} not found", workOrderId);
                return;
            }

            var template = await _context.Set<NotificationTemplate>()
                .FirstOrDefaultAsync(t =>
                    t.TriggerEvent == eventType &&
                    t.IsActive == true &&
                    t.IsAutomatic == true,
                    cancellationToken);

            if (template == null)
            {
                _logger.LogDebug("No template found for event {EventType}", eventType);
                return;
            }

            var notification = new Notification
            {
                NotificationCode = $"WO-{workOrderId}-{DateTime.UtcNow:yyyyMMddHHmmss}",
                TemplateId = template.TemplateId,
                UserId = workOrder.Customer?.UserId,
                CustomerId = workOrder.CustomerId,
                Channel = template.Channel,
                Priority = "High",
                Subject = ReplaceWorkOrderPlaceholders(template.Subject ?? "", workOrder),
                Message = ReplaceWorkOrderPlaceholders(template.MessageTemplate ?? "", workOrder),
                RelatedType = "WorkOrder",
                RelatedId = workOrderId,
                ScheduledDate = template.SendDelay.HasValue
                    ? DateTime.UtcNow.AddMinutes(template.SendDelay.Value)
                    : DateTime.UtcNow,
                Status = "Pending",
                CreatedDate = DateTime.UtcNow
            };

            _context.Set<Notification>().Add(notification);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created notification for work order {WorkOrderId}, event {EventType}",
                workOrderId, eventType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating work order notification for {WorkOrderId}", workOrderId);
        }
    }

    public async Task CreateInvoiceNotificationAsync(
        int invoiceId,
        string eventType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var invoice = await _context.Set<Invoice>()
                .Include(i => i.Customer)
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId, cancellationToken);

            if (invoice == null)
            {
                _logger.LogWarning("Invoice {InvoiceId} not found", invoiceId);
                return;
            }

            var template = await _context.Set<NotificationTemplate>()
                .FirstOrDefaultAsync(t =>
                    t.TriggerEvent == eventType &&
                    t.IsActive == true &&
                    t.IsAutomatic == true,
                    cancellationToken);

            if (template == null)
            {
                _logger.LogDebug("No template found for event {EventType}", eventType);
                return;
            }

            var notification = new Notification
            {
                NotificationCode = $"INV-{invoiceId}-{DateTime.UtcNow:yyyyMMddHHmmss}",
                TemplateId = template.TemplateId,
                UserId = invoice.Customer?.UserId,
                CustomerId = invoice.CustomerId,
                Channel = template.Channel,
                Priority = "Medium",
                Subject = ReplaceInvoicePlaceholders(template.Subject ?? "", invoice),
                Message = ReplaceInvoicePlaceholders(template.MessageTemplate ?? "", invoice),
                RelatedType = "Invoice",
                RelatedId = invoiceId,
                ScheduledDate = template.SendDelay.HasValue
                    ? DateTime.UtcNow.AddMinutes(template.SendDelay.Value)
                    : DateTime.UtcNow,
                Status = "Pending",
                CreatedDate = DateTime.UtcNow
            };

            _context.Set<Notification>().Add(notification);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created notification for invoice {InvoiceId}, event {EventType}",
                invoiceId, eventType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating invoice notification for {InvoiceId}", invoiceId);
        }
    }

    public async Task SendScheduledNotificationsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending scheduled notifications...");

            var pendingNotifications = await _context.Set<Notification>()
                .Where(n => n.Status == "Pending" &&
                           n.ScheduledDate <= DateTime.UtcNow)
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Found {Count} pending notifications to send", pendingNotifications.Count);

            foreach (var notification in pendingNotifications)
            {
                try
                {
                    // Mark as sent (actual sending logic would go to email/SMS service)
                    notification.SendDate = DateTime.UtcNow;
                    notification.DeliveredDate = DateTime.UtcNow;
                    notification.Status = "Sent";

                    _logger.LogDebug("Sent notification {NotificationId} to channel {Channel}",
                        notification.NotificationId, notification.Channel);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending notification {NotificationId}", notification.NotificationId);
                    notification.Status = "Failed";
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Completed sending {Count} notifications", pendingNotifications.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending scheduled notifications");
        }
    }

    private async Task ProcessSubscriptionRenewalRemindersAsync(CancellationToken cancellationToken)
    {
        if (!_renewalOptions.Enabled)
        {
            _logger.LogDebug("Subscription renewal reminders disabled via configuration.");
            return;
        }

        var reminderStages = BuildReminderStages();
        if (reminderStages.Count == 0)
        {
            _logger.LogDebug("No subscription renewal reminder stages configured.");
            return;
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var targetDates = reminderStages
            .Select(stage => today.AddDays(stage.DaysBefore))
            .Distinct()
            .ToHashSet();

        var subscriptions = await _context.CustomerPackageSubscriptions
            .AsNoTracking()
            .Include(s => s.Customer)
            .ThenInclude(c => c.User)
            .Include(s => s.Package)
            .Where(s => s.Status != null && s.Status != "Cancelled" && s.Status != "Inactive")
            .Where(s => s.NextPaymentDate != null)
            .ToListAsync(cancellationToken);

        subscriptions = subscriptions
            .Where(s => s.NextPaymentDate.HasValue && targetDates.Contains(s.NextPaymentDate!.Value))
            .ToList();

        if (subscriptions.Count == 0)
        {
            _logger.LogDebug("No subscriptions matched renewal reminder criteria for {Date}.", today);
            return;
        }

        _logger.LogInformation("Processing renewal reminders for {Count} subscriptions.", subscriptions.Count);

        foreach (var subscription in subscriptions)
        {
            foreach (var stage in reminderStages)
            {
                var targetDate = today.AddDays(stage.DaysBefore);
                if (subscription.NextPaymentDate == targetDate)
                {
                    await EnsureRenewalReminderAsync(subscription, stage, cancellationToken);
                }
            }
        }
    }

    private async Task EnsureRenewalReminderAsync(
        CustomerPackageSubscription subscription,
        ReminderStageDefinition stage,
        CancellationToken cancellationToken)
    {
        var subject = stage.Subject;

        var hasExistingReminder = await _context.Notifications
            .AsNoTracking()
            .AnyAsync(n =>
                n.RelatedType == "Subscription" &&
                n.RelatedId == subscription.SubscriptionId &&
                n.Subject == subject &&
                (n.Status == "Pending" || n.Status == "Sent"),
                cancellationToken);

        if (hasExistingReminder)
        {
            _logger.LogDebug(
                "SubscriptionId={SubscriptionId} already has reminder with subject '{Subject}'. Skipping.",
                subscription.SubscriptionId,
                subject);
            return;
        }

        var packageName = subscription.Package?.PackageName ?? "gói dịch vụ";
        var dueDate = subscription.NextPaymentDate ?? subscription.RenewalDate ?? subscription.ExpirationDate;
        var dueDateText = dueDate?.ToString("dd/MM/yyyy") ?? "sắp tới";

        var messageBuilder = new StringBuilder();
        messageBuilder.AppendLine($"Gói dịch vụ {packageName} của bạn sẽ đến hạn thanh toán vào ngày {dueDateText}.");

        if (subscription.PaymentAmount.HasValue)
        {
            messageBuilder.AppendLine(
                $"Số tiền cần thanh toán: {subscription.PaymentAmount.Value:N0} đ.");
        }

        messageBuilder.Append("Vui lòng gia hạn sớm để tiếp tục sử dụng đầy đủ quyền lợi.");
        if (stage.IsFinal)
        {
            messageBuilder.Append(" Đây là nhắc cuối cùng trước khi gói hết hạn.");
        }

        var notification = new Notification
        {
            NotificationCode = $"SUB-{subscription.SubscriptionId}-{DateTime.UtcNow:MMddHHmmss}",
            TemplateId = null,
            RecipientType = "Customer",
            UserId = subscription.Customer?.UserId,
            CustomerId = subscription.CustomerId,
            Channel = _renewalOptions.Channel,
            Priority = stage.IsFinal ? "High" : "Medium",
            Subject = subject,
            Message = messageBuilder.ToString().Trim(),
            RecipientName = subscription.Customer?.FullName,
            ScheduledDate = DateTime.UtcNow,
            Status = "Pending",
            RelatedType = "Subscription",
            RelatedId = subscription.SubscriptionId,
            CreatedDate = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Created subscription renewal reminder. SubscriptionId={SubscriptionId}, Stage={Stage}, DueDate={DueDate}.",
            subscription.SubscriptionId,
            stage.Stage,
            dueDateText);
    }

    private List<ReminderStageDefinition> BuildReminderStages()
    {
        var stages = new List<ReminderStageDefinition>();

        var skipFirstStage = _renewalOptions.FinalReminderDaysBefore >= 0
            && _renewalOptions.FirstReminderDaysBefore == _renewalOptions.FinalReminderDaysBefore;

        if (_renewalOptions.FirstReminderDaysBefore >= 0 && !skipFirstStage)
        {
            stages.Add(new ReminderStageDefinition
            {
                Stage = 1,
                DaysBefore = _renewalOptions.FirstReminderDaysBefore,
                Subject = string.IsNullOrWhiteSpace(_renewalOptions.FirstReminderSubject)
                    ? "Nhắc gia hạn gói dịch vụ"
                    : _renewalOptions.FirstReminderSubject,
                IsFinal = false
            });
        }

        if (_renewalOptions.FinalReminderDaysBefore >= 0)
        {
            stages.Add(new ReminderStageDefinition
            {
                Stage = skipFirstStage ? 1 : 2,
                DaysBefore = _renewalOptions.FinalReminderDaysBefore,
                Subject = string.IsNullOrWhiteSpace(_renewalOptions.FinalReminderSubject)
                    ? "Gói dịch vụ sắp hết hạn"
                    : _renewalOptions.FinalReminderSubject,
                IsFinal = true
            });
        }

        return stages
            .OrderBy(s => s.DaysBefore)
            .ThenBy(s => s.Stage)
            .ToList();
    }

    private sealed record ReminderStageDefinition
    {
        public int Stage { get; init; }
        public int DaysBefore { get; init; }
        public string Subject { get; init; } = string.Empty;
        public bool IsFinal { get; init; }
    }

    #region Private Helper Methods

    private async Task ProcessRuleAsync(AutoNotificationRule rule, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Processing rule {RuleName}", rule.RuleName);

            if (rule.Template == null)
            {
                _logger.LogWarning("Rule {RuleName} has no template configured", rule.RuleName);
                return;
            }

            // Process based on trigger table
            switch (rule.TriggerTable?.ToLower())
            {
                case "appointments":
                    await ProcessAppointmentRuleAsync(rule, cancellationToken);
                    break;

                case "workorders":
                    await ProcessWorkOrderRuleAsync(rule, cancellationToken);
                    break;

                case "invoices":
                    await ProcessInvoiceRuleAsync(rule, cancellationToken);
                    break;

                default:
                    _logger.LogDebug("Trigger table {TriggerTable} not supported for rule {RuleName}",
                        rule.TriggerTable, rule.RuleName);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing rule {RuleName}", rule.RuleName);
        }
    }

    private async Task ProcessAppointmentRuleAsync(AutoNotificationRule rule, CancellationToken cancellationToken)
    {
        // Get appointments matching the trigger condition
        var query = _context.Set<Appointment>()
            .Include(a => a.Customer)
            .Include(a => a.Slot)
            .Include(a => a.Status)
            .Where(a => a.CustomerId > 0);

        // Apply trigger condition filters
        if (!string.IsNullOrEmpty(rule.TriggerCondition))
        {
            // Simple condition parsing for common scenarios
            // Example: "Status=Pending" or "Status=Confirmed"
            var conditions = rule.TriggerCondition.Split(';');
            foreach (var condition in conditions)
            {
                var parts = condition.Split('=');
                if (parts.Length == 2)
                {
                    var field = parts[0].Trim();
                    var value = parts[1].Trim();

                    if (field.Equals("Status", StringComparison.OrdinalIgnoreCase))
                    {
                        query = query.Where(a => a.Status!.StatusName == value);
                    }
                    else if (field.Equals("AppointmentDate", StringComparison.OrdinalIgnoreCase))
                    {
                        if (value.Equals("Today", StringComparison.OrdinalIgnoreCase))
                        {
                            var today = DateOnly.FromDateTime(DateTime.UtcNow);
                            query = query.Where(a => a.Slot!.SlotDate == today);
                        }
                        else if (value.Equals("Tomorrow", StringComparison.OrdinalIgnoreCase))
                        {
                            var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
                            query = query.Where(a => a.Slot!.SlotDate == tomorrow);
                        }
                    }
                }
            }
        }

        var appointments = await query
            .Take(100) // Limit to prevent excessive notifications
            .ToListAsync(cancellationToken);

        _logger.LogDebug("Found {Count} appointments matching rule {RuleName}",
            appointments.Count, rule.RuleName);

        // Create notification for each matching appointment
        foreach (var appointment in appointments)
        {
            // Check if notification already exists to prevent duplicates
            var existingNotification = await _context.Set<Notification>()
                .AnyAsync(n =>
                    n.RelatedType == "Appointment" &&
                    n.RelatedId == appointment.AppointmentId &&
                    n.TemplateId == rule.TemplateId &&
                    n.CreatedDate >= DateTime.UtcNow.AddHours(-24), // Within last 24 hours
                    cancellationToken);

            if (existingNotification)
            {
                _logger.LogDebug("Notification already exists for appointment {AppointmentId}", appointment.AppointmentId);
                continue;
            }

            var notification = new Notification
            {
                NotificationCode = $"AUTO-APT-{appointment.AppointmentId}-{DateTime.UtcNow:yyyyMMddHHmmss}",
                TemplateId = rule.TemplateId,
                UserId = appointment.Customer?.UserId,
                CustomerId = appointment.CustomerId,
                Channel = rule.Template.Channel,
                Priority = rule.Priority ?? "Medium",
                Subject = ReplaceTemplatePlaceholders(rule.Template.Subject ?? "", appointment),
                Message = ReplaceTemplatePlaceholders(rule.Template.MessageTemplate ?? "", appointment),
                RelatedType = "Appointment",
                RelatedId = appointment.AppointmentId,
                ScheduledDate = rule.Template.SendDelay.HasValue
                    ? DateTime.UtcNow.AddMinutes(rule.Template.SendDelay.Value)
                    : DateTime.UtcNow,
                Status = "Pending",
                CreatedDate = DateTime.UtcNow,
                CreatedBy = rule.CreatedBy
            };

            _context.Set<Notification>().Add(notification);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task ProcessWorkOrderRuleAsync(AutoNotificationRule rule, CancellationToken cancellationToken)
    {
        var query = _context.Set<WorkOrder>()
            .Include(w => w.Customer)
            .Include(w => w.Status)
            .Where(w => w.CustomerId > 0);

        // Apply trigger condition filters
        if (!string.IsNullOrEmpty(rule.TriggerCondition))
        {
            var conditions = rule.TriggerCondition.Split(';');
            foreach (var condition in conditions)
            {
                var parts = condition.Split('=');
                if (parts.Length == 2)
                {
                    var field = parts[0].Trim();
                    var value = parts[1].Trim();

                    if (field.Equals("Status", StringComparison.OrdinalIgnoreCase))
                    {
                        query = query.Where(w => w.Status!.StatusName == value);
                    }
                }
            }
        }

        var workOrders = await query
            .Take(100)
            .ToListAsync(cancellationToken);

        _logger.LogDebug("Found {Count} work orders matching rule {RuleName}",
            workOrders.Count, rule.RuleName);

        foreach (var workOrder in workOrders)
        {
            var existingNotification = await _context.Set<Notification>()
                .AnyAsync(n =>
                    n.RelatedType == "WorkOrder" &&
                    n.RelatedId == workOrder.WorkOrderId &&
                    n.TemplateId == rule.TemplateId &&
                    n.CreatedDate >= DateTime.UtcNow.AddHours(-24),
                    cancellationToken);

            if (existingNotification)
                continue;

            var notification = new Notification
            {
                NotificationCode = $"AUTO-WO-{workOrder.WorkOrderId}-{DateTime.UtcNow:yyyyMMddHHmmss}",
                TemplateId = rule.TemplateId,
                UserId = workOrder.Customer?.UserId,
                CustomerId = workOrder.CustomerId,
                Channel = rule.Template.Channel,
                Priority = rule.Priority ?? "Medium",
                Subject = ReplaceWorkOrderPlaceholders(rule.Template.Subject ?? "", workOrder),
                Message = ReplaceWorkOrderPlaceholders(rule.Template.MessageTemplate ?? "", workOrder),
                RelatedType = "WorkOrder",
                RelatedId = workOrder.WorkOrderId,
                ScheduledDate = rule.Template.SendDelay.HasValue
                    ? DateTime.UtcNow.AddMinutes(rule.Template.SendDelay.Value)
                    : DateTime.UtcNow,
                Status = "Pending",
                CreatedDate = DateTime.UtcNow,
                CreatedBy = rule.CreatedBy
            };

            _context.Set<Notification>().Add(notification);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task ProcessInvoiceRuleAsync(AutoNotificationRule rule, CancellationToken cancellationToken)
    {
        var query = _context.Set<Invoice>()
            .Include(i => i.Customer)
            .Where(i => i.CustomerId > 0);

        // Apply trigger condition filters
        if (!string.IsNullOrEmpty(rule.TriggerCondition))
        {
            var conditions = rule.TriggerCondition.Split(';');
            foreach (var condition in conditions)
            {
                var parts = condition.Split('=');
                if (parts.Length == 2)
                {
                    var field = parts[0].Trim();
                    var value = parts[1].Trim();

                    if (field.Equals("Status", StringComparison.OrdinalIgnoreCase))
                    {
                        query = query.Where(i => i.Status == value);
                    }
                    else if (field.Equals("Overdue", StringComparison.OrdinalIgnoreCase) && value.Equals("True", StringComparison.OrdinalIgnoreCase))
                    {
                        var today = DateOnly.FromDateTime(DateTime.UtcNow);
                        query = query.Where(i => i.DueDate < today && i.Status != "Paid");
                    }
                }
            }
        }

        var invoices = await query
            .Take(100)
            .ToListAsync(cancellationToken);

        _logger.LogDebug("Found {Count} invoices matching rule {RuleName}",
            invoices.Count, rule.RuleName);

        foreach (var invoice in invoices)
        {
            var existingNotification = await _context.Set<Notification>()
                .AnyAsync(n =>
                    n.RelatedType == "Invoice" &&
                    n.RelatedId == invoice.InvoiceId &&
                    n.TemplateId == rule.TemplateId &&
                    n.CreatedDate >= DateTime.UtcNow.AddHours(-24),
                    cancellationToken);

            if (existingNotification)
                continue;

            var notification = new Notification
            {
                NotificationCode = $"AUTO-INV-{invoice.InvoiceId}-{DateTime.UtcNow:yyyyMMddHHmmss}",
                TemplateId = rule.TemplateId,
                UserId = invoice.Customer?.UserId,
                CustomerId = invoice.CustomerId,
                Channel = rule.Template.Channel,
                Priority = rule.Priority ?? "Medium",
                Subject = ReplaceInvoicePlaceholders(rule.Template.Subject ?? "", invoice),
                Message = ReplaceInvoicePlaceholders(rule.Template.MessageTemplate ?? "", invoice),
                RelatedType = "Invoice",
                RelatedId = invoice.InvoiceId,
                ScheduledDate = rule.Template.SendDelay.HasValue
                    ? DateTime.UtcNow.AddMinutes(rule.Template.SendDelay.Value)
                    : DateTime.UtcNow,
                Status = "Pending",
                CreatedDate = DateTime.UtcNow,
                CreatedBy = rule.CreatedBy
            };

            _context.Set<Notification>().Add(notification);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private string ReplaceTemplatePlaceholders(string template, Appointment appointment)
    {
        return template
            .Replace("{CustomerName}", appointment.Customer?.FullName ?? "Customer")
            .Replace("{AppointmentDate}", appointment.Slot?.SlotDate.ToString("dd/MM/yyyy") ?? "")
            .Replace("{AppointmentTime}", appointment.Slot?.StartTime.ToString(@"hh\:mm") ?? "")
            .Replace("{AppointmentId}", appointment.AppointmentId.ToString());
    }

    private string ReplaceWorkOrderPlaceholders(string template, WorkOrder workOrder)
    {
        return template
            .Replace("{CustomerName}", workOrder.Customer?.FullName ?? "Customer")
            .Replace("{WorkOrderCode}", workOrder.WorkOrderCode ?? "")
            .Replace("{Status}", workOrder.Status?.StatusName ?? "")
            .Replace("{WorkOrderId}", workOrder.WorkOrderId.ToString());
    }

    private string ReplaceInvoicePlaceholders(string template, Invoice invoice)
    {
        return template
            .Replace("{CustomerName}", invoice.Customer?.FullName ?? "Customer")
            .Replace("{InvoiceCode}", invoice.InvoiceCode ?? "")
            .Replace("{GrandTotal}", invoice.GrandTotal?.ToString("N0") ?? "0")
            .Replace("{InvoiceId}", invoice.InvoiceId.ToString());
    }

    #endregion
}
