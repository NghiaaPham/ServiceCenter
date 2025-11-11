using EVServiceCenter.Core.Domains.Checklists.DTOs.Requests;
using FluentValidation;

namespace EVServiceCenter.Core.Domains.Checklists.Validators;

public class UpdateChecklistTemplateValidator : AbstractValidator<UpdateChecklistTemplateRequestDto>
{
    public UpdateChecklistTemplateValidator()
    {
        RuleFor(x => x.TemplateName)
            .MaximumLength(100)
            .WithMessage("Template name cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.TemplateName));

        RuleFor(x => x.ServiceId)
            .GreaterThan(0)
            .WithMessage("Service ID must be greater than 0")
            .When(x => x.ServiceId.HasValue);

        RuleFor(x => x.CategoryId)
            .GreaterThan(0)
            .WithMessage("Category ID must be greater than 0")
            .When(x => x.CategoryId.HasValue);

        RuleFor(x => x.Items)
            .Must(items => items != null && items.Count > 0)
            .WithMessage("Checklist must have at least one item")
            .Must(items => items != null && items.Count <= 50)
            .WithMessage("Checklist cannot have more than 50 items")
            .When(x => x.Items != null);

        RuleForEach(x => x.Items)
            .ChildRules(item =>
            {
                item.RuleFor(i => i.Order)
                    .GreaterThan(0)
                    .WithMessage("Item order must be greater than 0");

                item.RuleFor(i => i.Description)
                    .NotEmpty()
                    .WithMessage("Item description is required")
                    .MaximumLength(500)
                    .WithMessage("Item description cannot exceed 500 characters");
            })
            .When(x => x.Items != null);

        // Business rule: Items must have unique order numbers
        RuleFor(x => x.Items)
            .Must(items => items == null || items.Select(i => i.Order).Distinct().Count() == items.Count)
            .WithMessage("Item order numbers must be unique")
            .When(x => x.Items != null);
    }
}
