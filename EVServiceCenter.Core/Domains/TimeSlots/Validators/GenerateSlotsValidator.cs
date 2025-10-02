using EVServiceCenter.Core.Domains.TimeSlots.DTOs.Requests;
using FluentValidation;

namespace EVServiceCenter.Core.Domains.TimeSlots.Validators
{
    public class GenerateSlotsValidator : AbstractValidator<GenerateSlotsRequestDto>
    {
        public GenerateSlotsValidator()
        {
            RuleFor(x => x.CenterId)
                .GreaterThan(0)
                .WithMessage("CenterId phải lớn hơn 0");

            RuleFor(x => x.StartDate)
                .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
                .WithMessage("StartDate không được trong quá khứ");

            RuleFor(x => x.EndDate)
                .GreaterThanOrEqualTo(x => x.StartDate)
                .WithMessage("EndDate phải sau hoặc bằng StartDate");

            RuleFor(x => x)
                .Must(x => x.EndDate.ToDateTime(TimeOnly.MinValue) - x.StartDate.ToDateTime(TimeOnly.MinValue) <= TimeSpan.FromDays(90))
                .WithMessage("Chỉ có thể generate slots tối đa 90 ngày");

            RuleFor(x => x.SlotDurationMinutes)
                .GreaterThanOrEqualTo(15)
                .WithMessage("SlotDuration phải tối thiểu 15 phút")
                .LessThanOrEqualTo(480)
                .WithMessage("SlotDuration không được vượt quá 8 giờ (480 phút)");

            RuleFor(x => x.MaxBookingsPerSlot)
                .GreaterThan(0)
                .WithMessage("MaxBookingsPerSlot phải lớn hơn 0")
                .LessThanOrEqualTo(50)
                .WithMessage("MaxBookingsPerSlot không được vượt quá 50");

            RuleFor(x => x.SlotType)
                .MaximumLength(20)
                .WithMessage("SlotType không được vượt quá 20 ký tự");
        }
    }
}