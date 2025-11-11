using EVServiceCenter.Core.Domains.Notifications.DTOs.Requests;
using FluentValidation;

namespace EVServiceCenter.Core.Domains.Notifications.Validators;

/// <summary>
/// Validator for NotificationQueryDto
/// Validates pagination, filtering and sorting parameters
/// </summary>
public class NotificationQueryValidator : AbstractValidator<NotificationQueryDto>
{
    public NotificationQueryValidator()
    {
        // Page number validation
        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("Số trang phải lớn hơn 0");

        // Page size validation
        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Kích thước trang phải từ 1 đến 100");

        // Performance warning for large page sizes
        RuleFor(x => x.PageSize)
            .LessThanOrEqualTo(50)
            .WithMessage("Để tối ưu performance, kích thước trang không nên vượt quá 50");

        // Status validation
        When(x => !string.IsNullOrEmpty(x.Status), () =>
        {
            RuleFor(x => x.Status)
                .Must(s => new[] { "Pending", "Sent", "Delivered", "Failed" }.Contains(s))
                .WithMessage("Trạng thái phải là Pending, Sent, Delivered hoặc Failed");
        });

        // Channel validation
        When(x => !string.IsNullOrEmpty(x.Channel), () =>
        {
            RuleFor(x => x.Channel)
                .Must(c => new[] { "Email", "SMS", "InApp", "Push" }.Contains(c))
                .WithMessage("Kênh thông báo phải là Email, SMS, InApp hoặc Push");
        });

        // Priority validation
        When(x => !string.IsNullOrEmpty(x.Priority), () =>
        {
            RuleFor(x => x.Priority)
                .Must(p => new[] { "High", "Medium", "Low" }.Contains(p))
                .WithMessage("Độ ưu tiên phải là High, Medium hoặc Low");
        });

        // Related type validation
        When(x => !string.IsNullOrEmpty(x.RelatedType), () =>
        {
            RuleFor(x => x.RelatedType)
                .Must(rt => new[] { "Appointment", "WorkOrder", "Invoice", "Payment" }.Contains(rt))
                .WithMessage("Loại liên quan phải là Appointment, WorkOrder, Invoice hoặc Payment");
        });

        // Date range validation
        When(x => x.StartDate.HasValue && x.EndDate.HasValue, () =>
        {
            RuleFor(x => x.EndDate)
                .GreaterThanOrEqualTo(x => x.StartDate!.Value)
                .WithMessage("Ngày kết thúc phải sau hoặc bằng ngày bắt đầu");
        });

        // Sort by validation
        RuleFor(x => x.SortBy)
            .Must(sb => new[] { "CreatedDate", "SendDate", "ReadDate" }.Contains(sb))
            .WithMessage("Trường sắp xếp phải là CreatedDate, SendDate hoặc ReadDate");

        // Sort direction validation
        RuleFor(x => x.SortDirection)
            .Must(sd => new[] { "Asc", "Desc" }.Contains(sd))
            .WithMessage("Thứ tự sắp xếp phải là Asc hoặc Desc");
    }
}
