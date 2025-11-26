using EVServiceCenter.API.Extensions;
using EVServiceCenter.API.HostedServices;
using EVServiceCenter.API.Mappings;
using EVServiceCenter.API.Middleware;
using EVServiceCenter.API.Validators;
using EVServiceCenter.API.Realtime;
using EVServiceCenter.Core.Domains.CustomerTypes.Validators; // Added for FluentValidation integration
using EVServiceCenter.Core.Entities;
using EVServiceCenter.Core.Enums;
using EVServiceCenter.Core.Interfaces.Services;
using EVServiceCenter.Infrastructure.BackgroundServices;
using EVServiceCenter.Infrastructure.JsonConverters;
using EVServiceCenter.Infrastructure.Options;
using EVServiceCenter.Infrastructure.Persistence.Seeders; // Added for Seeders
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore; // Added for Database.Migrate()
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Linq;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

var builder = WebApplication.CreateBuilder(args);

// Enable EF Core SQL + timing logs for debugging (development only)
// This will make EF Core log executed SQL and durations to the configured logger (Serilog)
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", Microsoft.Extensions.Logging.LogLevel.Information);
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Infrastructure", Microsoft.Extensions.Logging.LogLevel.Information);

// Add SignalR for real-time chat
builder.Services.AddSignalR();
builder.Services.AddSingleton<IUserConnectionManager, InMemoryUserConnectionManager>();
builder.Services.AddSingleton<IChatRealtimeBroadcaster, SignalRChatBroadcaster>();

// Add services
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new TimeOnlyJsonConverter());
        // Fix Vietnamese characters encoding
        options.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
    });

// Use recommended FluentValidation registration
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateCustomerTypeRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateCustomerTypeRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CustomerTypeQueryValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UserValidator>();

// ✅ Customer Vehicle Update Validator
builder.Services.AddValidatorsFromAssemblyContaining<EVServiceCenter.Infrastructure.Domains.Customers.Validators.UpdateMyVehicleValidator>();

// ✅ Inventory Management Validators
builder.Services.AddValidatorsFromAssemblyContaining<EVServiceCenter.Core.Domains.InventoryManagement.Validators.PartInventoryQueryValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<EVServiceCenter.Core.Domains.InventoryManagement.Validators.StockAdjustmentValidator>();

// ✅ Checklist Management Validators
builder.Services.AddValidatorsFromAssemblyContaining<EVServiceCenter.Core.Domains.Checklists.Validators.CreateChecklistTemplateValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<EVServiceCenter.Core.Domains.Checklists.Validators.UpdateChecklistTemplateValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<EVServiceCenter.Core.Domains.Checklists.Validators.ChecklistTemplateQueryValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<EVServiceCenter.Core.Domains.Checklists.Validators.UpdateChecklistItemStatusValidator>();

// ✅ Invoice Management Validators
builder.Services.AddValidatorsFromAssemblyContaining<EVServiceCenter.Core.Domains.Invoices.Validators.GenerateInvoiceValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<EVServiceCenter.Core.Domains.Invoices.Validators.InvoiceQueryValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<EVServiceCenter.Core.Domains.Invoices.Validators.SendInvoiceValidator>();

// ✅ Payment Management Validators
builder.Services.AddValidatorsFromAssemblyContaining<EVServiceCenter.Core.Domains.Payments.Validators.CreatePaymentValidator>();

// ✅ Financial Reports Validators
builder.Services.AddValidatorsFromAssemblyContaining<EVServiceCenter.Core.Domains.FinancialReports.Validators.RevenueReportQueryValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<EVServiceCenter.Core.Domains.FinancialReports.Validators.PaymentReportQueryValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<EVServiceCenter.Core.Domains.FinancialReports.Validators.InvoiceReportQueryValidator>();

// ✅ Phase 4: Customer Experience Validators
// Notification Validators
builder.Services.AddValidatorsFromAssemblyContaining<EVServiceCenter.Core.Domains.Notifications.Validators.NotificationQueryValidator>();

// Service Rating Validators
builder.Services.AddValidatorsFromAssemblyContaining<EVServiceCenter.Core.Domains.ServiceRatings.Validators.CreateServiceRatingValidator>();

// Chat Validators
builder.Services.AddValidatorsFromAssemblyContaining<EVServiceCenter.Core.Domains.Chat.Validators.SendMessageValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<EVServiceCenter.Core.Domains.Chat.Validators.CreateChannelValidator>();

builder.Services.Configure<SubscriptionRenewalReminderOptions>(
    builder.Configuration.GetSection("SubscriptionRenewalReminders"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "EV Service Center API", Version = "v1" });

    // Group endpoints by tags for better organization
    c.TagActionsBy(api =>
    {
        if (api.GroupName != null)
        {
            return new[] { api.GroupName };
        }

        var controllerName = api.ActionDescriptor.DisplayName;
        if (controllerName != null)
        {
            return new[] { controllerName.Split('.')[^1].Replace("Controller", "") };
        }

        return new[] { "Default" };
    });

    c.DocInclusionPredicate((name, api) => true);
    c.OrderActionsBy(api => api.GroupName);

    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new()
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new()
    {
        {
            new()
            {
                Reference = new()
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add HttpContextAccessor (CẦN THIẾT cho HttpContextService)
builder.Services.AddHttpContextAccessor();

// ✅ SCHOOL PROJECT: Simple MemoryCache (Redis removed - not needed for small data)
builder.Services.AddMemoryCache();
Console.WriteLine("✅ MemoryCache configured for application (Redis removed for simplicity)");

/// Add Google package
builder.Services.AddHttpClient();

// AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Warm-up DB & giữ connection pool nóng để giảm độ trễ đăng nhập lần đầu
builder.Services.AddHostedService<DatabaseWarmupHostedService>();

// Logging
builder.Host.UseSerilog((context, services, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.File($"logs/userlog-{DateTime.Now:yyyyMMdd}.txt", rollingInterval: RollingInterval.Day)
        .WriteTo.Console());

// Authentication and Authorization
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"] ?? string.Empty)),
        ClockSkew = TimeSpan.Zero
    };
});
// Comment out OAuth providers - we're verifying tokens manually
// .AddGoogle(options =>
// {
//     options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
//     options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
// })
// .AddFacebook(options =>
// {
//     options.AppId = builder.Configuration["Authentication:Facebook:AppId"]!;
//     options.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"]!;
// });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole(UserRoles.Admin.ToString()));
    options.AddPolicy("StaffOnly", policy => policy.RequireRole(UserRoles.Staff.ToString()));
    options.AddPolicy("TechnicianOnly", policy => policy.RequireRole(UserRoles.Technician.ToString()));
    options.AddPolicy("CustomerOnly", policy => policy.RequireRole(UserRoles.Customer.ToString()));
    options.AddPolicy("AdminOrStaff", policy => policy.RequireRole(UserRoles.Admin.ToString(), UserRoles.Staff.ToString()));
    options.AddPolicy("AdminOrTechnician", policy => policy.RequireRole(UserRoles.Admin.ToString(), UserRoles.Technician.ToString()));
    options.AddPolicy("StaffOrTechnician", policy => policy.RequireRole(UserRoles.Staff.ToString(), UserRoles.Technician.ToString()));
    options.AddPolicy("AllInternal", policy => policy.RequireRole(UserRoles.Admin.ToString(), UserRoles.Staff.ToString(), UserRoles.Technician.ToString()));
    options.AddPolicy("Authenticated", policy => policy.RequireAuthenticatedUser());
});

builder.Services.AddCors(options =>
{
    var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
        ?? new[] { "http://localhost:3000", "http://localhost:4200", "http://localhost:5000" };

    options.AddPolicy("DefaultCors", policy =>
        policy.SetIsOriginAllowed(origin =>
        {
            var uri = new Uri(origin);
            var host = uri.Host;
            var normalizedOrigin = origin.TrimEnd('/');
            return allowedOrigins.Any(allowed => string.Equals(allowed.TrimEnd('/'), normalizedOrigin, StringComparison.OrdinalIgnoreCase))
                   || host == "localhost"
                   || host.EndsWith("ngrok.app", StringComparison.OrdinalIgnoreCase)
                   || host.EndsWith("ngrok-free.app", StringComparison.OrdinalIgnoreCase)
                   || host.EndsWith("ngrok-free.dev", StringComparison.OrdinalIgnoreCase)
                   || host.EndsWith("ngrok.io", StringComparison.OrdinalIgnoreCase);
        })
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials()
        .SetPreflightMaxAge(TimeSpan.FromMinutes(30)));
});

// 🔥 ĐĂNG KÝ INFRASTRUCTURE (DbContext, Repositories, Services)
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddServiceCenterModule();
builder.Services.AddCarBrandModule();
builder.Services.AddCarModelModule();
builder.Services.AddCustomerVehicleModule();
builder.Services.AddServiceCategoryModule();
builder.Services.AddMaintenanceServiceModule();
builder.Services.AddMaintenancePackageModule();
builder.Services.AddPackageSubscriptionModule();
builder.Services.AddModelServicePricingModule();
builder.Services.AddTimeSlotModule();
builder.Services.AddAppointmentModule(); // Appointment booking & management
builder.Services.AddPricingModule(); // ✅ Discount calculation & promotion services
builder.Services.AddIdentityModule();
builder.Services.AddTechnicianManagementModule(); // ✅ PHASE 2: Technician management & auto-assignment
builder.Services.AddShiftManagementModule(); // ✅ SPRINT 1: Attendance check-in/check-out
builder.Services.AddInventoryManagementModule(); // ✅ PHASE 1: Inventory & Parts management
builder.Services.AddChecklistManagementModule(); // ✅ PHASE 1: Checklist management
builder.Services.AddChecklistModule(); // ✅ NEW: Complete/Skip/Validate/BulkComplete APIs
builder.Services.AddInvoiceManagementModule(); // ✅ PHASE 3: Invoice management
builder.Services.AddPaymentManagementModule(builder.Configuration); // ✅ HYBRID: Mock/Sandbox/Production
builder.Services.AddFinancialReportModule(); // ✅ PHASE 3: Financial reports & analytics
builder.Services.AddCustomerExperienceModule(); // ✅ PHASE 4: Notifications & Ratings

// 🔒 JWT Token Blacklist Service (for logout)
builder.Services.AddScoped<EVServiceCenter.Core.Domains.Identity.Interfaces.ITokenBlacklistService,
    EVServiceCenter.Infrastructure.Domains.Identity.Services.TokenBlacklistService>();

// ⚙️ BACKGROUND SERVICES
builder.Services.AddSingleton<EVServiceCenter.Infrastructure.BackgroundServices.AppointmentReconciliationService>();
builder.Services.AddHostedService<EVServiceCenter.Infrastructure.BackgroundServices.AutoNotificationBackgroundService>();

// ✅ SMART SUBSCRIPTION: Service Source Audit Service
// Try to load real implementation, fallback to stub if needed
builder.Services.AddScoped<IServiceSourceAuditService>(sp =>
{
    var dbContext = sp.GetRequiredService<EVDbContext>();
    var logger = sp.GetRequiredService<ILogger<Program>>();

    try
    {
        var infrastructureAssembly = System.Reflection.Assembly.Load("EVServiceCenter.Infrastructure");
        var serviceType = infrastructureAssembly.GetType("EVServiceCenter.Infrastructure.Services.ServiceSourceAuditService");
        if (serviceType != null)
        {
            return (IServiceSourceAuditService)Activator.CreateInstance(serviceType, dbContext, logger)!;
        }
    }
    catch (Exception ex)
    {
        logger.LogWarning("Could not load ServiceSourceAuditService: {Message}. Using stub.", ex.Message);
    }

    // Fallback to stub implementation
    return new EVServiceCenter.API.Services.StubServiceSourceAuditService(dbContext, logger);
});

// Payment completion queue and worker
builder.Services.AddSingleton<EVServiceCenter.Core.Domains.Payments.Interfaces.IPaymentCompletionQueue, EVServiceCenter.Infrastructure.BackgroundServices.InMemoryPaymentCompletionQueue>();
builder.Services.AddHostedService<PaymentCompletionWorker>();

var app = builder.Build();

var httpsPortConfig = builder.Configuration.GetValue<int?>("HttpsRedirection:HttpsPort");
var httpsPortEnv = Environment.GetEnvironmentVariable("ASPNETCORE_HTTPS_PORT");
var urlsConfig = builder.Configuration["ASPNETCORE_URLS"] ?? builder.Configuration["urls"];
var httpsUrlConfigured = !string.IsNullOrWhiteSpace(urlsConfig)
    && urlsConfig.Split(';', StringSplitOptions.RemoveEmptyEntries)
        .Any(url => url.StartsWith("https://", StringComparison.OrdinalIgnoreCase));
var httpsRedirectionEnabled = httpsPortConfig.HasValue
    || !string.IsNullOrEmpty(httpsPortEnv)
    || httpsUrlConfigured;

// Seed data only in Development environment
if (app.Environment.IsDevelopment())
{
    var seedEnabled = app.Configuration.GetValue("SeedData:Enabled", false);
    var inventorySeedDisabled = app.Configuration.GetValue("SeedInventory:Disabled", false);

    if (seedEnabled)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<EVDbContext>();
            var loggerFactory = services.GetRequiredService<ILoggerFactory>();

            try
            {
                context.Database.SetCommandTimeout(TimeSpan.FromMinutes(5));
                context.Database.Migrate();
            }
            catch (Exception migEx)
            {
                var migLogger = loggerFactory.CreateLogger("DatabaseMigration");
                migLogger.LogWarning(migEx, "Database migration in development failed or skipped.");
            }

            ServiceCenterSeeder.SeedData(context);
            CarBrandSeeder.SeedCarBrands(context);
            CarModelSeeder.SeedCarModels(context);


            // var customerTypeLogger = loggerFactory.CreateLogger("CustomerTypeSeeder");
            // CustomerTypeSeeder.SeedAsync(context, customerTypeLogger).Wait();

            CustomerSeeder.SeedCustomers(context);
            CustomerVehicleSeeder.SeedCustomerVehicles(context);
            ServiceCategorySeeder.SeedServiceCategories(context);
            MaintenanceServiceSeeder.SeedMaintenanceServices(context);
            ModelServicePricingSeeder.SeedModelServicePricings(context);
            AppointmentStatusSeeder.SeedAppointmentStatuses(context);
            WorkOrderStatusSeeder.SeedWorkOrderStatuses(context);
            TimeSlotSeeder.SeedTimeSlots(context);

            ChecklistTemplateSeeder.SeedChecklistTemplates(context);

            TechnicianSeeder.SeedTechnicians(context);
            EmployeeSkillSeeder.SeedEmployeeSkills(context);
            TechnicianScheduleSeeder.SeedTechnicianSchedules(context);

            await TechnicianTestDataSeeder.SeedAsync(context);

            var packageLogger = loggerFactory.CreateLogger<MaintenancePackageSeeder>();
            var packageSeeder = new MaintenancePackageSeeder(context, packageLogger);
            packageSeeder.SeedAsync().Wait();

            var subscriptionLogger = loggerFactory.CreateLogger<CustomerPackageSubscriptionSeeder>();
            var subscriptionSeeder = new CustomerPackageSubscriptionSeeder(context, subscriptionLogger);
            subscriptionSeeder.SeedAsync().Wait();

            var testimonialLogger = loggerFactory.CreateLogger("ServiceRatingSeeder");
            EVServiceCenter.Infrastructure.Persistence.Seeders.ServiceRatingSeeder.SeedDemoTestimonials(context, testimonialLogger);

            var mainflowLogger = loggerFactory.CreateLogger("MainflowDemoSeeder");
            MainflowDemoSeeder.Seed(context, mainflowLogger);

            if (!inventorySeedDisabled)
            {
                var inventoryLogger = loggerFactory.CreateLogger("InventoryDemoSeeder");
                InventoryDemoSeeder.Seed(context, inventoryLogger);
            }
            else
            {
                var inventoryLogger = loggerFactory.CreateLogger("InventoryDemoSeeder");
                inventoryLogger.LogInformation("SeedInventory:Disabled = true. Skipping inventory seeding.");
            }

            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Development seeding completed.");
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while migrating or seeding the database.");
        }
    }
    else
    {
        using var scope = app.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("SeedData:Enabled = false. Skipping development seeding.");
    }
}

app.UseMiddleware<GlobalExceptionHandler>();

// Middleware pipeline
if (httpsRedirectionEnabled)
{
    app.UseHttpsRedirection();
}
else
{
    app.Logger.LogInformation("Skipping HTTPS redirection because no HTTPS endpoint is configured.");
}
app.UseResponseCaching();

// ✅ CORS MUST BE CALLED BEFORE Authentication/Authorization
// Apply single policy for all environments (allows localhost + ngrok)
app.UseCors("DefaultCors");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "EV Service Center API V1");
        c.RoutePrefix = "swagger";
    });
}

app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Request: {Method} {Path}", context.Request.Method, context.Request.Path);
    await next();
});

app.UseAuthentication();
app.UseMiddleware<EVServiceCenter.API.Middlewares.TokenBlacklistMiddleware>(); // 🔒 Check revoked tokens
app.UseMiddleware<PasswordResetRateLimitMiddleware>();
app.UseAuthorization();


app.MapHub<EVServiceCenter.API.Hubs.ChatHub>("/hubs/chat");

app.MapControllers();


var reconciliationService = app.Services.GetRequiredService<EVServiceCenter.Infrastructure.BackgroundServices.AppointmentReconciliationService>();
reconciliationService.Start();
app.Run();
