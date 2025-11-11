using EVServiceCenter.Core.Domains.Invoices.Interfaces;
using EVServiceCenter.Infrastructure.Domains.Invoices.Repositories;
using EVServiceCenter.Infrastructure.Domains.Invoices.Services;

namespace EVServiceCenter.API.Extensions;

/// <summary>
/// Dependency Injection configuration for Invoice Management module
/// </summary>
public static class InvoiceManagementDependencyInjection
{
    /// <summary>
    /// Register Invoice Management services, repositories, and validators
    /// </summary>
    public static IServiceCollection AddInvoiceManagementModule(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();

        // Services
        services.AddScoped<IInvoiceService, InvoiceService>();

        return services;
    }
}
