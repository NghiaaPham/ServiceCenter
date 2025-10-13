using EVServiceCenter.Core.Domains.CustomerVehicles.DTOs.Requests;
using EVServiceCenter.Core.Domains.CustomerVehicles.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.CustomerVehicles.Interfaces.Services;
using EVServiceCenter.Core.Domains.CustomerVehicles.Validators;
using EVServiceCenter.Infrastructure.Domains.CustomerVehicles.Repositories;
using EVServiceCenter.Infrastructure.Domains.CustomerVehicles.Services;

namespace EVServiceCenter.API.Extensions
{
    public static class CustomerVehicleDependencyInjection
    {
        public static IServiceCollection AddCustomerVehicleModule(this IServiceCollection services)
        {
            // Repositories
            services.AddScoped<ICustomerVehicleRepository, CustomerVehicleRepository>();
            services.AddScoped<ICustomerVehicleStatisticsRepository, CustomerVehicleStatisticsRepository>();

            // Services
            services.AddScoped<ICustomerVehicleService, CustomerVehicleService>();
            services.AddScoped<ICustomerVehicleQueryService, CustomerVehicleQueryService>();
            services.AddScoped<ICustomerVehicleStatisticsService, CustomerVehicleStatisticsService>();
            services.AddScoped<IVehicleMaintenanceService, VehicleMaintenanceService>(); // Smart Maintenance Reminder

            // Validators
            services.AddScoped<IValidator<CreateCustomerVehicleRequestDto>, CreateCustomerVehicleValidator>();
            services.AddScoped<IValidator<UpdateCustomerVehicleRequestDto>, UpdateCustomerVehicleValidator>();
            services.AddScoped<IValidator<CustomerVehicleQueryDto>, CustomerVehicleQueryValidator>();

            return services;
        }
    }
}