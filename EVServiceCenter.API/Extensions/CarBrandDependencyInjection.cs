using EVServiceCenter.Application.Domains.CarBrands.Services;
using EVServiceCenter.Core.Domains.CarBrands.DTOs.Requests;
using EVServiceCenter.Core.Domains.CarBrands.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.CarBrands.Interfaces.Services;
using EVServiceCenter.Core.Domains.CarBrands.Validators;
using EVServiceCenter.Infrastructure.Domains.CarBrands.Repositories;

namespace EVServiceCenter.API.Extensions
{
    public static class CarBrandDependencyInjection
    {
        public static IServiceCollection AddCarBrandModule(this IServiceCollection services)
        {
            // Repositories
            services.AddScoped<ICarBrandRepository, CarBrandRepository>();
            services.AddScoped<ICarBrandStatisticsRepository, CarBrandStatisticsRepository>();

            // Services
            services.AddScoped<ICarBrandService, CarBrandService>();
            services.AddScoped<ICarBrandQueryService, CarBrandQueryService>();
            services.AddScoped<ICarBrandStatisticsService, CarBrandStatisticsService>();

            // Validators
            services.AddScoped<IValidator<CreateCarBrandRequestDto>, CreateCarBrandValidator>();
            services.AddScoped<IValidator<UpdateCarBrandRequestDto>, UpdateCarBrandValidator>();
            services.AddScoped<IValidator<CarBrandQueryDto>, CarBrandQueryValidator>();

            return services;
        }
    }
}