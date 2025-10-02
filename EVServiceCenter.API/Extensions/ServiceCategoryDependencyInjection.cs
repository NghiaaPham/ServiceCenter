using EVServiceCenter.Core.Domains.ServiceCategories.DTOs.Requests;
using EVServiceCenter.Core.Domains.ServiceCategories.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.ServiceCategories.Interfaces.Services;
using EVServiceCenter.Core.Domains.ServiceCategories.Validators;
using EVServiceCenter.Infrastructure.Domains.ServiceCategories.Repositories;
using EVServiceCenter.Infrastructure.Domains.ServiceCategories.Services;


namespace EVServiceCenter.API.Extensions
{
    public static class ServiceCategoryDependencyInjection
    {
        public static IServiceCollection AddServiceCategoryModule(this IServiceCollection services)
        {
            // Repository
            services.AddScoped<IServiceCategoryRepository, ServiceCategoryRepository>();

            // Service
            services.AddScoped<IServiceCategoryService, ServiceCategoryService>();

            // Validators
            services.AddScoped<IValidator<CreateServiceCategoryRequestDto>, CreateServiceCategoryValidator>();
            services.AddScoped<IValidator<UpdateServiceCategoryRequestDto>, UpdateServiceCategoryValidator>();
            services.AddScoped<IValidator<ServiceCategoryQueryDto>, ServiceCategoryQueryValidator>();

            return services;
        }
    }
}