using EVServiceCenter.Core.Domains.TechnicianManagement.Interfaces;
using EVServiceCenter.Infrastructure.Domains.TechnicianManagement.Repositories;
using EVServiceCenter.Infrastructure.Domains.TechnicianManagement.Services;

namespace EVServiceCenter.API.Extensions
{
    /// <summary>
    /// Dependency Injection extension for Shift/Attendance module
    /// </summary>
    public static class ShiftDependencyInjection
    {
        public static IServiceCollection AddShiftManagementModule(this IServiceCollection services)
        {
            // Repositories
            services.AddScoped<IShiftRepository, ShiftRepository>();

            // Services
            services.AddScoped<IShiftService, ShiftService>();

            return services;
        }
    }
}
