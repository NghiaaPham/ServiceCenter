using EVServiceCenter.Core.Domains.Checklists.DTOs.Requests;
using FluentValidation;

namespace EVServiceCenter.Core.Domains.Checklists.Validators;

/// <summary>
/// Validator cho ApplyChecklistTemplateRequestDto
/// </summary>
public class ApplyChecklistTemplateValidator : AbstractValidator<ApplyChecklistTemplateRequestDto>
{
    public ApplyChecklistTemplateValidator()
    {
        RuleFor(x => x.TemplateId)
            .GreaterThan(0)
            .WithMessage("TemplateId ph?i l?n h?n 0");

        // CustomItems optional, nh?ng n?u có thì validate
        When(x => x.CustomItems != null && x.CustomItems.Any(), () =>
        {
            RuleFor(x => x.CustomItems)
                .Must(items => items.All(i => i.Order > 0))
                .WithMessage("T?t c? items ph?i có Order > 0");

            RuleFor(x => x.CustomItems)
                .Must(items => items.Select(i => i.Order).Distinct().Count() == items.Count)
                .WithMessage("Order c?a các items không ???c trùng nhau");

            RuleFor(x => x.CustomItems)
                .Must(items => items.All(i => !string.IsNullOrWhiteSpace(i.Description)))
                .WithMessage("T?t c? items ph?i có Description");
        });
    }
}
