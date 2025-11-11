using EVServiceCenter.Core.Domains.Checklists.DTOs.Requests;
using FluentValidation;

namespace EVServiceCenter.Core.Domains.Checklists.Validators;

public class UpdateChecklistItemStatusValidator : AbstractValidator<UpdateChecklistItemStatusRequestDto>
{
    public UpdateChecklistItemStatusValidator()
    {
        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .WithMessage("Notes cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));

        RuleFor(x => x.ImageUrl)
            .MaximumLength(500)
            .WithMessage("ImageUrl cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.ImageUrl));

        // At least one field must be provided
        RuleFor(x => x)
            .Must(x => x.IsCompleted.HasValue || !string.IsNullOrEmpty(x.Notes) || !string.IsNullOrEmpty(x.ImageUrl))
            .WithMessage("At least one field (IsCompleted, Notes, or ImageUrl) must be provided");
    }
}
