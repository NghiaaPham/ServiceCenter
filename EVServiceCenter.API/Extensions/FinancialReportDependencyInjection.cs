using EVServiceCenter.Core.Domains.FinancialReports.Interfaces;
using EVServiceCenter.Infrastructure.Domains.FinancialReports.Services;

namespace EVServiceCenter.API.Extensions;

/// <summary>
/// Dependency Injection configuration for Financial Reports module
/// ✅ SCHOOL PROJECT: Simplified - No caching layer needed for small data
/// </summary>
public static class FinancialReportDependencyInjection
{
    /// <summary>
    /// Register Financial Report services (caching removed for simplicity)
    /// </summary>
    public static IServiceCollection AddFinancialReportModule(this IServiceCollection services)
    {
        // ✅ Register core financial reporting service only
        // Caching removed - not needed for school project with small dataset
        services.AddScoped<IFinancialReportService, FinancialReportService>();

        return services;
    }
}
