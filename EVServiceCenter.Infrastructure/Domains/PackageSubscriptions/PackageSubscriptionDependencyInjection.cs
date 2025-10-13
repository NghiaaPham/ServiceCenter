using EVServiceCenter.Core.Domains.PackageSubscriptions.DTOs.Requests;
using EVServiceCenter.Core.Domains.PackageSubscriptions.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.PackageSubscriptions.Interfaces.Services;
using EVServiceCenter.Infrastructure.Domains.PackageSubscriptions.Repositories;
using EVServiceCenter.Infrastructure.Domains.PackageSubscriptions.Services;
using EVServiceCenter.Infrastructure.Domains.PackageSubscriptions.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace EVServiceCenter.Infrastructure.Domains.PackageSubscriptions
{
    /// <summary>
    /// Dependency Injection cho Package Subscription Module
    /// ??ng ký t?t c? services, repositories, validators
    /// </summary>
    public static class PackageSubscriptionDependencyInjection
    {
        public static IServiceCollection AddPackageSubscriptionModule(this IServiceCollection services)
        {
            // ========== REPOSITORIES ==========
            services.AddScoped<IPackageSubscriptionCommandRepository, PackageSubscriptionCommandRepository>();
            services.AddScoped<IPackageSubscriptionQueryRepository, PackageSubscriptionQueryRepository>();

            // ========== SERVICES ==========
            services.AddScoped<IPackageSubscriptionService, PackageSubscriptionService>();

            // ========== VALIDATORS ==========
            services.AddScoped<IValidator<PurchasePackageRequestDto>, PurchasePackageValidator>();

            return services;
        }
    }
}
