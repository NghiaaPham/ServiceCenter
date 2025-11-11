using EVServiceCenter.Core.Domains.Checklists.DTOs.Requests;
using FluentValidation;

namespace EVServiceCenter.Core.Domains.Checklists.Validators
{
    /// <summary>
    /// Validator cho CompleteChecklistItemRequestDto
    /// </summary>
    public class CompleteChecklistItemValidator : AbstractValidator<CompleteChecklistItemRequestDto>
    {
        public CompleteChecklistItemValidator()
        {
            RuleFor(x => x.ItemId)
                .GreaterThan(0).WithMessage("ItemId ph?i l?n h?n 0");

            RuleFor(x => x.WorkOrderId)
                .GreaterThan(0).WithMessage("WorkOrderId ph?i l?n h?n 0");

            RuleFor(x => x.Notes)
                .MaximumLength(500).WithMessage("Notes không ???c v??t quá 500 ký t?");

            RuleFor(x => x.ImageUrl)
                .MaximumLength(500).WithMessage("ImageUrl không ???c v??t quá 500 ký t?")
                .Must(url => string.IsNullOrEmpty(url) || Uri.IsWellFormedUriString(url, UriKind.Absolute))
                .WithMessage("ImageUrl ph?i là URL h?p l?");
        }
    }
}
