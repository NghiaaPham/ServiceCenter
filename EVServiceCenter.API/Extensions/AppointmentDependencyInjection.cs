using EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Query;
using EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Request;
using EVServiceCenter.Core.Domains.AppointmentManagement.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.AppointmentManagement.Interfaces.Services;
using EVServiceCenter.Core.Domains.AppointmentManagement.Validators;
using EVServiceCenter.Infrastructure.Domains.AppointmentManagement.Repositories;
using EVServiceCenter.Infrastructure.Domains.AppointmentManagement.Services;
using FluentValidation;

namespace EVServiceCenter.API.Extensions
{
    public static class AppointmentDependencyInjection
    {
        public static IServiceCollection AddAppointmentModule(this IServiceCollection services)
        {
            // Repositories
            services.AddScoped<IAppointmentRepository, AppointmentRepository>();
            services.AddScoped<IAppointmentCommandRepository, AppointmentCommandRepository>();
            services.AddScoped<IAppointmentQueryRepository, AppointmentQueryRepository>();

            // Services
            services.AddScoped<IAppointmentCommandService, AppointmentCommandService>();
            services.AddScoped<IAppointmentQueryService, AppointmentQueryService>();

            // Validators
            services.AddScoped<IValidator<CreateAppointmentRequestDto>, CreateAppointmentValidator>();
            services.AddScoped<IValidator<UpdateAppointmentRequestDto>, UpdateAppointmentValidator>();
            services.AddScoped<IValidator<RescheduleAppointmentRequestDto>, RescheduleAppointmentValidator>();
            services.AddScoped<IValidator<CancelAppointmentRequestDto>, CancelAppointmentValidator>();
            services.AddScoped<IValidator<ConfirmAppointmentRequestDto>, ConfirmAppointmentValidator>();
            services.AddScoped<IValidator<AppointmentQueryDto>, AppointmentQueryValidator>();

            return services;
        }
    }
}
