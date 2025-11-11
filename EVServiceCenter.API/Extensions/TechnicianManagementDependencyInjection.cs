using EVServiceCenter.Core.Domains.TechnicianManagement.Interfaces;
using EVServiceCenter.Infrastructure.Domains.TechnicianManagement.Repositories;
using EVServiceCenter.Infrastructure.Domains.TechnicianManagement.Services;

namespace EVServiceCenter.API.Extensions;

/// <summary>
/// Dependency Injection configuration for Technician Management module
/// </summary>
public static class TechnicianManagementDependencyInjection
{
    /// <summary>
    /// Register Technician Management services
    /// </summary>
    public static IServiceCollection AddTechnicianManagementModule(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<ITechnicianRepository, TechnicianRepository>();
        services.AddScoped<TechnicianPerformanceRepository>();

        // Services
        services.AddScoped<ITechnicianService, TechnicianService>();
        services.AddScoped<ITechnicianAutoAssignmentService, TechnicianAutoAssignmentService>();

        return services;
    }
}
