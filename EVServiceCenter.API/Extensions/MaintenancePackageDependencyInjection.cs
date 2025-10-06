using EVServiceCenter.Core.Domains.MaintenancePackages.DTOs.Requests;
using EVServiceCenter.Core.Domains.MaintenancePackages.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.MaintenancePackages.Interfaces.Services;
using EVServiceCenter.Core.Domains.MaintenancePackages.Validators;
using EVServiceCenter.Infrastructure.Domains.MaintenancePackages.Repositories;
using EVServiceCenter.Infrastructure.Domains.MaintenancePackages.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace EVServiceCenter.API.Extensions
{
    public static class MaintenancePackageDependencyInjection
    {
        public static IServiceCollection AddMaintenancePackageModule(this IServiceCollection services)
        {
            // Repositories - CQRS Pattern
            services.AddScoped<IMaintenancePackageQueryRepository, MaintenancePackageQueryRepository>();
            services.AddScoped<IMaintenancePackageCommandRepository, MaintenancePackageCommandRepository>();

            // Service - Single service sử dụng cả Query và Command repositories
            services.AddScoped<IMaintenancePackageService, MaintenancePackageService>();

            // Validators
            services.AddScoped<IValidator<CreateMaintenancePackageRequestDto>, CreateMaintenancePackageRequestValidator>();
            services.AddScoped<IValidator<UpdateMaintenancePackageRequestDto>, UpdateMaintenancePackageRequestValidator>();
            services.AddScoped<IValidator<MaintenancePackageQueryDto>, MaintenancePackageQueryValidator>();

            return services;
        }
    }
}
