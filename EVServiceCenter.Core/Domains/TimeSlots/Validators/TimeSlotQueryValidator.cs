using EVServiceCenter.Core.Domains.TimeSlots.DTOs.Requests;
using FluentValidation;

namespace EVServiceCenter.Core.Domains.TimeSlots.Validators
{
    public class TimeSlotQueryValidator : AbstractValidator<TimeSlotQueryDto>
    {
        public TimeSlotQueryValidator()
        {
            RuleFor(x => x.Page)
                .GreaterThan(0)
                .WithMessage("Page phải lớn hơn 0");

            RuleFor(x => x.PageSize)
                .GreaterThan(0)
                .WithMessage("PageSize phải lớn hơn 0")
                .LessThanOrEqualTo(100)
                .WithMessage("PageSize không được vượt quá 100");

            RuleFor(x => x.CenterId)
                .GreaterThan(0)
                .When(x => x.CenterId.HasValue)
                .WithMessage("CenterId phải lớn hơn 0");

            RuleFor(x => x.EndDate)
                .GreaterThanOrEqualTo(x => x.StartDate)
                .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
                .WithMessage("EndDate phải sau hoặc bằng StartDate");

            RuleFor(x => x.SortBy)
                .Must(value => new[] { "SlotDate", "StartTime", "CenterName", "MaxBookings", "CurrentBookings" }
                    .Contains(value, StringComparer.OrdinalIgnoreCase))
                .WithMessage("SortBy không hợp lệ");

            RuleFor(x => x.SortOrder)
                .Must(value => new[] { "asc", "desc" }.Contains(value, StringComparer.OrdinalIgnoreCase))
                .WithMessage("SortOrder phải là 'asc' hoặc 'desc'");
        }
    }
}