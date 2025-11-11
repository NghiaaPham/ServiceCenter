using EVServiceCenter.Core.Domains.Checklists.DTOs.Requests;
using FluentValidation;

namespace EVServiceCenter.Core.Domains.Checklists.Validators
{
    /// <summary>
    /// Validator cho SkipChecklistItemRequestDto
    /// </summary>
    public class SkipChecklistItemValidator : AbstractValidator<SkipChecklistItemRequestDto>
    {
        public SkipChecklistItemValidator()
        {
            RuleFor(x => x.ItemId)
                .GreaterThan(0).WithMessage("ItemId ph?i l?n h?n 0");

            RuleFor(x => x.WorkOrderId)
                .GreaterThan(0).WithMessage("WorkOrderId ph?i l?n h?n 0");

            RuleFor(x => x.SkipReason)
                .NotEmpty().WithMessage("Lý do skip là b?t bu?c")
                .MinimumLength(10).WithMessage("Lý do skip ph?i ít nh?t 10 ký t?")
                .MaximumLength(500).WithMessage("Lý do skip không ???c v??t quá 500 ký t?");
        }
    }
}
