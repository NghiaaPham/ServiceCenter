using EVServiceCenter.Core.Domains.Customers.Interfaces;
using EVServiceCenter.Core.Domains.CustomerTypes.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.Identity.Interfaces;
using EVServiceCenter.Core.Domains.Shared.Interfaces;
using EVServiceCenter.Core.Domains.Payments.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.Payments.Interfaces.Services;
using EVServiceCenter.Core.Domains.WorkOrders.Interfaces;
using EVServiceCenter.Core.Entities;
using EVServiceCenter.Infrastructure.Domains.Customers.Repositories;
using EVServiceCenter.Infrastructure.Domains.Customers.Services;
using EVServiceCenter.Infrastructure.Domains.CustomerTypes;
using EVServiceCenter.Infrastructure.Domains.CustomerTypes.Services;
using EVServiceCenter.Infrastructure.Domains.Identity.Repositories;
using EVServiceCenter.Infrastructure.Domains.Identity.Services;
using EVServiceCenter.Infrastructure.Domains.Shared.Repositories;
using EVServiceCenter.Infrastructure.Domains.Shared.Services;
using EVServiceCenter.Infrastructure.Domains.Payments.Repositories;
using EVServiceCenter.Infrastructure.Domains.Payments.Services;
using EVServiceCenter.Infrastructure.Domains.WorkOrders.Repositories;
using EVServiceCenter.Infrastructure.Domains.WorkOrders.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using EVServiceCenter.Infrastructure.Domains.Testimonials.Repositories;
using EVServiceCenter.Core.Domains.Testimonials.Interfaces;
using EVServiceCenter.Infrastructure.Domains.Testimonials.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // DbContext - increase command timeout and enable resilient retries
        services.AddDbContext<EVDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions => sqlOptions
                    .CommandTimeout(60) // increase default timeout to 60 seconds
                    .EnableRetryOnFailure() // enable transient retries
            ));


        services.AddMemoryCache();

        // Repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICustomerTypeRepository, CustomerTypeRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IPaymentIntentRepository, PaymentIntentRepository>();

        // WorkOrder Repositories
        services.AddScoped<WorkOrderQueryRepository>();
        services.AddScoped<IWorkOrderRepository, WorkOrderRepository>();

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
        services.AddScoped<IPaymentIntentService, PaymentIntentService>();

        // Register token blacklist service for fast revoked token checks
        services.AddScoped<ITokenBlacklistService, TokenBlacklistService>();

        // WorkOrder Services
        services.AddScoped<IWorkOrderService, WorkOrderManagementService>();
        services.AddScoped<IWorkOrderTimelineService, WorkOrderTimelineService>();
        services.AddScoped<IVehicleHealthService, VehicleHealthService>();

        // TODO: Add AppointmentService, InvoiceService,...

        // Testimonials
        services.AddScoped<TestimonialQueryRepository>();
        services.AddScoped<ITestimonialService, TestimonialService>();

        return services;
    }
}
