using EVServiceCenter.Core.Domains.TimeSlots.DTOs.Requests;
using FluentValidation;

namespace EVServiceCenter.Core.Domains.TimeSlots.Validators
{
    public class CreateTimeSlotValidator : AbstractValidator<CreateTimeSlotRequestDto>
    {
        public CreateTimeSlotValidator()
        {
            RuleFor(x => x.CenterId)
                .GreaterThan(0)
                .WithMessage("CenterId phải lớn hơn 0");

            RuleFor(x => x.SlotDate)
                .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
                .WithMessage("Không thể tạo slot cho ngày trong quá khứ");

            RuleFor(x => x.StartTime)
                .NotEmpty()
                .WithMessage("StartTime không được để trống");

            RuleFor(x => x.EndTime)
                .NotEmpty()
                .WithMessage("EndTime không được để trống")
                .GreaterThan(x => x.StartTime)
                .WithMessage("EndTime phải sau StartTime");

            RuleFor(x => x)
                .Must(x => (x.EndTime.ToTimeSpan() - x.StartTime.ToTimeSpan()).TotalMinutes >= 15)
                .WithMessage("Slot phải có độ dài tối thiểu 15 phút");

            RuleFor(x => x.MaxBookings)
                .GreaterThan(0)
                .WithMessage("MaxBookings phải lớn hơn 0")
                .LessThanOrEqualTo(50)
                .WithMessage("MaxBookings không được vượt quá 50");

            RuleFor(x => x.SlotType)
                .MaximumLength(20)
                .WithMessage("SlotType không được vượt quá 20 ký tự");

            RuleFor(x => x.Notes)
                .MaximumLength(200)
                .WithMessage("Notes không được vượt quá 200 ký tự");
        }
    }
}