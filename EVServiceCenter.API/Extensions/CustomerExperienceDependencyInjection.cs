using EVServiceCenter.Core.Domains.Chat.Interfaces;
using EVServiceCenter.Core.Domains.Notifications.Interfaces;
using EVServiceCenter.Core.Domains.ServiceRatings.Interfaces;
using EVServiceCenter.Infrastructure.Domains.Chat.Services;
using EVServiceCenter.Infrastructure.Domains.Notifications.Services;
using EVServiceCenter.Infrastructure.Domains.ServiceRatings.Services;

namespace EVServiceCenter.API.Extensions;

/// <summary>
/// Customer Experience module dependency injection
/// Registers notification, rating, and chat services
/// </summary>
public static class CustomerExperienceDependencyInjection
{
    public static IServiceCollection AddCustomerExperienceModule(this IServiceCollection services)
    {
        // Notification services
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IAutoNotificationService, AutoNotificationService>();

        // Rating services
        services.AddScoped<IServiceRatingService, ServiceRatingService>();

        // Chat services
        services.AddScoped<IChatService, ChatService>();

        return services;
    }
}
