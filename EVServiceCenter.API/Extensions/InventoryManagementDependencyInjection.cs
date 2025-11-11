using EVServiceCenter.Core.Domains.InventoryManagement.Interfaces;
using EVServiceCenter.Infrastructure.Domains.InventoryManagement.Repositories;
using EVServiceCenter.Infrastructure.Domains.InventoryManagement.Services;

namespace EVServiceCenter.API.Extensions;

/// <summary>
/// Dependency Injection configuration for Inventory Management module
/// </summary>
public static class InventoryManagementDependencyInjection
{
    /// <summary>
    /// Register Inventory Management services, repositories, and validators
    /// </summary>
    public static IServiceCollection AddInventoryManagementModule(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<IPartInventoryRepository, PartInventoryRepository>();
        services.AddScoped<IStockTransactionRepository, StockTransactionRepository>();

        // Services
        services.AddScoped<IInventoryService, InventoryService>();

        return services;
    }
}
