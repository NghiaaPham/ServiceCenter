using EVServiceCenter.Core.Domains.Notifications.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EVServiceCenter.Infrastructure.BackgroundServices;

/// <summary>
/// Background service for automatic notification processing
/// Runs periodically to process notification rules and send scheduled notifications
/// </summary>
public class AutoNotificationBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AutoNotificationBackgroundService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(5); // Run every 5 minutes

    public AutoNotificationBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<AutoNotificationBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Auto Notification Background Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessNotificationsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Auto Notification Background Service");
            }

            // Wait for the next interval
            await Task.Delay(_interval, stoppingToken);
        }

        _logger.LogInformation("Auto Notification Background Service stopped");
    }

    private async Task ProcessNotificationsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var autoNotificationService = scope.ServiceProvider
            .GetRequiredService<IAutoNotificationService>();

        _logger.LogDebug("Processing auto notifications cycle started");

        // Process auto notification rules
        await autoNotificationService.ProcessAutoNotificationsAsync(cancellationToken);

        // Send scheduled notifications
        await autoNotificationService.SendScheduledNotificationsAsync(cancellationToken);

        _logger.LogDebug("Processing auto notifications cycle completed");
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Auto Notification Background Service is stopping");
        await base.StopAsync(cancellationToken);
    }
}
