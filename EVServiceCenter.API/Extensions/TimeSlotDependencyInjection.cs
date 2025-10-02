using EVServiceCenter.Core.Domains.TimeSlots.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.TimeSlots.Interfaces.Services;
using EVServiceCenter.Core.Domains.TimeSlots.Validators;
using EVServiceCenter.Infrastructure.Domains.TimeSlots.Repositories;
using EVServiceCenter.Infrastructure.Domains.TimeSlots.Services;

namespace EVServiceCenter.API.Extensions
{
    public static class TimeSlotDependencyInjection
    {
        public static IServiceCollection AddTimeSlotModule(this IServiceCollection services)
        {
            // Repositories
            services.AddScoped<ITimeSlotRepository, TimeSlotRepository>();
            services.AddScoped<ITimeSlotQueryRepository, TimeSlotQueryRepository>();
            services.AddScoped<ITimeSlotCommandRepository, TimeSlotCommandRepository>();

            // Services CQRS
            services.AddScoped<ITimeSlotQueryService, TimeSlotQueryService>();
            services.AddScoped<ITimeSlotCommandService, TimeSlotCommandService>();

            // Validators
            services.AddValidatorsFromAssemblyContaining<CreateTimeSlotValidator>();
            services.AddValidatorsFromAssemblyContaining<UpdateTimeSlotValidator>();
            services.AddValidatorsFromAssemblyContaining<GenerateSlotsValidator>();
            services.AddValidatorsFromAssemblyContaining<TimeSlotQueryValidator>();

            return services;
        }
    }
}