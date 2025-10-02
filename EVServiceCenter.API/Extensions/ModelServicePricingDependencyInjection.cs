using EVServiceCenter.Core.Domains.ModelServicePricings.DTOs.Requests;
using EVServiceCenter.Core.Domains.ModelServicePricings.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.ModelServicePricings.Interfaces.Services;
using EVServiceCenter.Core.Domains.ModelServicePricings.Validators;
using EVServiceCenter.Infrastructure.Domains.ModelServicePricings.Repositories;
using EVServiceCenter.Infrastructure.Domains.ModelServicePricings.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace EVServiceCenter.API.Extensions
{
    public static class ModelServicePricingDependencyInjection
    {
        public static IServiceCollection AddModelServicePricingModule(this IServiceCollection services)
        {
            // Repository
            services.AddScoped<IModelServicePricingRepository, ModelServicePricingRepository>();

            // Service
            services.AddScoped<IModelServicePricingService, ModelServicePricingService>();

            // Validators
            services.AddScoped<IValidator<CreateModelServicePricingRequestDto>, CreateModelServicePricingValidator>();
            services.AddScoped<IValidator<UpdateModelServicePricingRequestDto>, UpdateModelServicePricingValidator>();
            services.AddScoped<IValidator<ModelServicePricingQueryDto>, ModelServicePricingQueryValidator>();

            return services;
        }
    }
}