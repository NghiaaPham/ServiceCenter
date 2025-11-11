using EVServiceCenter.Core.Domains.Checklists.DTOs.Requests;
using EVServiceCenter.Core.Domains.Checklists.Interfaces;
using EVServiceCenter.Core.Domains.Checklists.Validators;
using EVServiceCenter.Infrastructure.Domains.Checklists.Repositories;
using EVServiceCenter.Infrastructure.Domains.Checklists.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace EVServiceCenter.API.Extensions;

/// <summary>
/// Dependency Injection configuration for Checklist module
/// </summary>
public static class ChecklistDependencyInjection
{
    /// <summary>
    /// Add checklist module services
    /// </summary>
    public static IServiceCollection AddChecklistModule(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<IChecklistRepository, ChecklistRepository>();

        // Services
        services.AddScoped<IChecklistService, ChecklistService>();

        // Validators
        services.AddScoped<IValidator<CompleteChecklistItemRequestDto>, CompleteChecklistItemValidator>();
        services.AddScoped<IValidator<SkipChecklistItemRequestDto>, SkipChecklistItemValidator>();
        services.AddScoped<IValidator<CreateChecklistTemplateRequestDto>, CreateChecklistTemplateValidator>();
        services.AddScoped<IValidator<UpdateChecklistTemplateRequestDto>, UpdateChecklistTemplateValidator>();
        services.AddScoped<IValidator<ApplyChecklistTemplateRequestDto>, ApplyChecklistTemplateValidator>();

        return services;
    }
}
