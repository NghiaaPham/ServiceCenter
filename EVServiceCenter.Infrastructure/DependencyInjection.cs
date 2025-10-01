using EVServiceCenter.Core.Domains.Customers.Interfaces;
using EVServiceCenter.Core.Domains.CustomerTypes.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.Identity.Interfaces;
using EVServiceCenter.Core.Domains.Shared.Interfaces;
using EVServiceCenter.Core.Entities;
using EVServiceCenter.Infrastructure.Domains.Customers.Repositories;
using EVServiceCenter.Infrastructure.Domains.Customers.Services;
using EVServiceCenter.Infrastructure.Domains.CustomerTypes;
using EVServiceCenter.Infrastructure.Domains.CustomerTypes.Services;
using EVServiceCenter.Infrastructure.Domains.Identity.Repositories;
using EVServiceCenter.Infrastructure.Domains.Identity.Services;
using EVServiceCenter.Infrastructure.Domains.Shared.Repositories;
using EVServiceCenter.Infrastructure.Domains.Shared.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // DbContext
            services.AddDbContext<EVDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // 🔹 MemoryCache (bắt buộc vì TimeSlotRepository dùng IMemoryCache)
            services.AddMemoryCache();

            // Repositories
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ICustomerTypeRepository, CustomerTypeRepository>();
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            // services.AddScoped<ITimeSlotRepository, TimeSlotRepository>();
            // TODO: Add AppointmentRepository, InvoiceRepository,...

            // Services
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IExternalAuthService, ExternalAuthService>();
            services.AddScoped<IHttpContextService, HttpContextService>();
            services.AddScoped<ICustomerTypeService, CustomerTypeService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<ICustomerAccountService, CustomerAccountService>();
            // TODO: Add AppointmentService, InvoiceService,...

            return services;
        }
    }
