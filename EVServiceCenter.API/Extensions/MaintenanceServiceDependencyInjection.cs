using EVServiceCenter.Core.Domains.MaintenanceServices.DTOs.Requests;
using EVServiceCenter.Core.Domains.MaintenanceServices.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.MaintenanceServices.Interfaces.Services;
using EVServiceCenter.Core.Domains.MaintenanceServices.Validators;
using EVServiceCenter.Infrastructure.Domains.MaintenanceServices.Repositories;
using EVServiceCenter.Infrastructure.Domains.MaintenanceServices.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace EVServiceCenter.API.Extensions
{
    public static class MaintenanceServiceDependencyInjection
    {
        public static IServiceCollection AddMaintenanceServiceModule(this IServiceCollection services)
        {
            // Repository
            services.AddScoped<IMaintenanceServiceRepository, MaintenanceServiceRepository>();

            // Service
            services.AddScoped<IMaintenanceServiceService, MaintenanceServiceService>();

            // Validators
            services.AddScoped<IValidator<CreateMaintenanceServiceRequestDto>, CreateMaintenanceServiceValidator>();
            services.AddScoped<IValidator<UpdateMaintenanceServiceRequestDto>, UpdateMaintenanceServiceValidator>();
            services.AddScoped<IValidator<MaintenanceServiceQueryDto>, MaintenanceServiceQueryValidator>();

            return services;
        }
    }
}