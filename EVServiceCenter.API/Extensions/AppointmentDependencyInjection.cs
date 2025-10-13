using EVServiceCenter.API.Services;
using EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Query;
using EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Request;
using EVServiceCenter.Core.Domains.AppointmentManagement.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.AppointmentManagement.Interfaces.Services;
using EVServiceCenter.Core.Domains.AppointmentManagement.Validators;
using EVServiceCenter.Core.Interfaces.Services;
using EVServiceCenter.Infrastructure.Domains.AppointmentManagement.Repositories;
using EVServiceCenter.Infrastructure.Domains.AppointmentManagement.Services;
using FluentValidation;
using Microsoft.Extensions.Logging;

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

            // ✅ SMART SUBSCRIPTION: Audit Service (using Stub implementation)
            services.AddScoped<IServiceSourceAuditService>(provider =>
            {
                var context = provider.GetRequiredService<EVServiceCenter.Core.Entities.EVDbContext>();
                var logger = provider.GetRequiredService<ILogger<StubServiceSourceAuditService>>();
                return new StubServiceSourceAuditService(context, logger);
            });

            // Validators
            services.AddScoped<IValidator<CreateAppointmentRequestDto>, CreateAppointmentValidator>();
            services.AddScoped<IValidator<UpdateAppointmentRequestDto>, UpdateAppointmentValidator>();
            services.AddScoped<IValidator<RescheduleAppointmentRequestDto>, RescheduleAppointmentValidator>();
            services.AddScoped<IValidator<CancelAppointmentRequestDto>, CancelAppointmentValidator>();
            services.AddScoped<IValidator<ConfirmAppointmentRequestDto>, ConfirmAppointmentValidator>();
            services.AddScoped<IValidator<AppointmentQueryDto>, AppointmentQueryValidator>();

            // ✅ SMART SUBSCRIPTION: Adjust ServiceSource Validator
            services.AddScoped<IValidator<AdjustServiceSourceRequestDto>, AdjustServiceSourceValidator>();

            return services;
        }
    }
}
