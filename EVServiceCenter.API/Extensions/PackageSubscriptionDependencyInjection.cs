using EVServiceCenter.Core.Domains.PackageSubscriptions.DTOs.Requests;
using EVServiceCenter.Core.Domains.PackageSubscriptions.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.PackageSubscriptions.Interfaces.Services;
using EVServiceCenter.Core.Domains.PackageSubscriptions.Validators;
using EVServiceCenter.Infrastructure.Domains.PackageSubscriptions.Repositories;
using EVServiceCenter.Infrastructure.Domains.PackageSubscriptions.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace EVServiceCenter.API.Extensions
{
    public static class PackageSubscriptionDependencyInjection
    {
        public static IServiceCollection AddPackageSubscriptionModule(this IServiceCollection services)
        {
            // Repositories - CQRS Pattern
            services.AddScoped<IPackageSubscriptionQueryRepository, PackageSubscriptionQueryRepository>();
            services.AddScoped<IPackageSubscriptionCommandRepository, PackageSubscriptionCommandRepository>();

            // Service - Single service sử dụng cả Query và Command repositories
            services.AddScoped<IPackageSubscriptionService, PackageSubscriptionService>();

            // Validators
            services.AddScoped<IValidator<PurchasePackageRequestDto>, PurchasePackageRequestValidator>();

            return services;
        }
    }
}
