using EVServiceCenter.Core.Domains.Checklists.DTOs.Requests;
using EVServiceCenter.Core.Domains.Checklists.Interfaces;
using EVServiceCenter.Core.Domains.Checklists.Validators;
using EVServiceCenter.Infrastructure.Domains.Checklists.Repositories;
using EVServiceCenter.Infrastructure.Domains.Checklists.Services;
using FluentValidation;

namespace EVServiceCenter.API.Extensions;

/// <summary>
/// Dependency Injection configuration for Checklist Management module
/// </summary>
public static class ChecklistManagementDependencyInjection
{
    /// <summary>
    /// Register Checklist Management services, repositories, and validators
    /// </summary>
    public static IServiceCollection AddChecklistManagementModule(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<IChecklistRepository, ChecklistRepository>();

        // Services
        services.AddScoped<IChecklistService, ChecklistService>();

        // ? Validators
        services.AddScoped<IValidator<CompleteChecklistItemRequestDto>, CompleteChecklistItemValidator>();
        services.AddScoped<IValidator<SkipChecklistItemRequestDto>, SkipChecklistItemValidator>();

        return services;
    }
}
