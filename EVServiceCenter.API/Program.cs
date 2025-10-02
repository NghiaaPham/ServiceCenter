using EVServiceCenter.API.Extensions;
using EVServiceCenter.API.Mappings;
using EVServiceCenter.API.Middleware;
using EVServiceCenter.API.Validators;
using EVServiceCenter.Core.Domains.CustomerTypes.Validators; // Added for FluentValidation integration
using EVServiceCenter.Core.Entities;
using EVServiceCenter.Core.Enums;
using EVServiceCenter.Infrastructure.JsonConverters;
using EVServiceCenter.Infrastructure.Persistence.Seeders; // Added for ServiceCenterSeeder
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore; // Added for Database.Migrate()
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new TimeOnlyJsonConverter());
    });

// Use recommended FluentValidation registration
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateCustomerTypeRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateCustomerTypeRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CustomerTypeQueryValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UserValidator>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "EV Service Center API", Version = "v1" });

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

// MemoryCache
builder.Services.AddMemoryCache();

// Add Google package
builder.Services.AddHttpClient();

// AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

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
})
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
})
.AddFacebook(options =>
{
    options.AppId = builder.Configuration["Authentication:Facebook:AppId"]!;
    options.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"]!;
});

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
    options.AddPolicy("AllowSpecificOrigin", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
            ?? new[] { "http://localhost:3000", "http://localhost:4200", "http://localhost:5000" };

        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });

    options.AddPolicy("AllowNgrok", policy =>
    {
        policy.SetIsOriginAllowed(origin =>
        {
            var uri = new Uri(origin);
            return uri.Host == "localhost" ||
                   uri.Host.EndsWith("ngrok.app") ||
                   uri.Host.EndsWith("ngrok-free.app") ||
                   uri.Host.EndsWith("ngrok.io");
        })
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });
});

// 🔥 ĐĂNG KÝ INFRASTRUCTURE (DbContext, Repositories, Services)
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddServiceCenterModule();
builder.Services.AddCarBrandModule();
builder.Services.AddCarModelModule();
builder.Services.AddCustomerVehicleModule();
builder.Services.AddServiceCategoryModule();
builder.Services.AddMaintenanceServiceModule();
builder.Services.AddModelServicePricingModule();
builder.Services.AddTimeSlotModule();

var app = builder.Build();

// Seed data only in Development environment
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<EVDbContext>();
        context.Database.Migrate(); // Apply any pending migrations
        ServiceCenterSeeder.SeedData(context); // Run the seeder
        CarBrandSeeder.SeedCarBrands(context);
        CarModelSeeder.SeedCarModels(context);
        CustomerSeeder.SeedCustomers(context);
        CustomerVehicleSeeder.SeedCustomerVehicles(context);
        ServiceCategorySeeder.SeedServiceCategories(context);
        MaintenanceServiceSeeder.SeedMaintenanceServices(context);
        ModelServicePricingSeeder.SeedModelServicePricings(context);
        AppointmentStatusSeeder.SeedAppointmentStatuses(context);
        TimeSlotSeeder.SeedTimeSlots(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    }
}
app.UseMiddleware<GlobalExceptionHandler>();

// Middleware pipeline
app.UseHttpsRedirection();
app.UseResponseCaching();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "EV Service Center API V1");
        c.RoutePrefix = "swagger";
    });
    app.UseCors("AllowNgrok");
}
else
{
    app.UseCors("AllowSpecificOrigin");
}

app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Request: {Method} {Path}", context.Request.Method, context.Request.Path);
    await next();
});

app.UseAuthentication();
app.UseMiddleware<PasswordResetRateLimitMiddleware>();
app.UseAuthorization();
app.MapControllers();

app.Run();