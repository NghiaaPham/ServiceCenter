using EVServiceCenter.Application.Domains.CarModels.Services;
using EVServiceCenter.Core.Domains.CarModels.DTOs.Requests;
using EVServiceCenter.Core.Domains.CarModels.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.CarModels.Interfaces.Services;
using EVServiceCenter.Core.Domains.CarModels.Validators;
using EVServiceCenter.Infrastructure.Domains.CarModels.Repositories;


namespace EVServiceCenter.API.Extensions
{
    public static class CarModelDependencyInjection
    {
        public static IServiceCollection AddCarModelModule(this IServiceCollection services)
        {
            // Repositories
            services.AddScoped<ICarModelRepository, CarModelRepository>();
            services.AddScoped<ICarModelStatisticsRepository, CarModelStatisticsRepository>();

            // Services
            services.AddScoped<ICarModelService, CarModelService>();
            services.AddScoped<ICarModelQueryService, CarModelQueryService>();
            services.AddScoped<ICarModelStatisticsService, CarModelStatisticsService>();

            // Validators
            services.AddScoped<IValidator<CreateCarModelRequestDto>, CreateCarModelValidator>();
            services.AddScoped<IValidator<UpdateCarModelRequestDto>, UpdateCarModelValidator>();
            services.AddScoped<IValidator<CarModelQueryDto>, CarModelQueryValidator>();

            return services;
        }
    }
}