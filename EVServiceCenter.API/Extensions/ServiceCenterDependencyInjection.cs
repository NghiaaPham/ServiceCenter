using EVServiceCenter.Core.Domains.ServiceCenters.DTOs.Requests;
using EVServiceCenter.Core.Domains.ServiceCenters.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.ServiceCenters.Interfaces.Services;
using EVServiceCenter.Core.Domains.ServiceCenters.Validators;
using EVServiceCenter.Infrastructure.Domains.ServiceCenters.Repositories;
using EVServiceCenter.Infrastructure.Domains.ServiceCenters.Services;

public static class ServiceCenterDependencyInjection
{
    public static IServiceCollection AddServiceCenterModule(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<IServiceCenterRepository, ServiceCenterRepository>();
        services.AddScoped<IServiceCenterStatisticsRepository, ServiceCenterStatisticsRepository>();
        services.AddScoped<IServiceCenterAvailabilityRepository, ServiceCenterAvailabilityRepository>();

        // Services
        services.AddScoped<IServiceCenterService, ServiceCenterService>();
        services.AddScoped<IServiceCenterQueryService, ServiceCenterQueryService>();
        services.AddScoped<IServiceCenterStatisticsService, ServiceCenterStatisticsService>();
        services.AddScoped<IServiceCenterAvailabilityService, ServiceCenterAvailabilityService>();

        // Validators
        services.AddScoped<IValidator<CreateServiceCenterRequestDto>, CreateServiceCenterValidator>();
        services.AddScoped<IValidator<UpdateServiceCenterRequestDto>, UpdateServiceCenterValidator>();
        services.AddScoped<IValidator<ServiceCenterQueryDto>, ServiceCenterQueryValidator>();

        return services;
    }
}
