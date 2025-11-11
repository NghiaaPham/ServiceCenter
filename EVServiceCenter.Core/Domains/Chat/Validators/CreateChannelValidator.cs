using EVServiceCenter.Core.Domains.Chat.DTOs.Requests;
using FluentValidation;

namespace EVServiceCenter.Core.Domains.Chat.Validators;

/// <summary>
/// Validator for CreateChannelRequestDto
/// Validates channel creation parameters
/// </summary>
public class CreateChannelValidator : AbstractValidator<CreateChannelRequestDto>
{
    public CreateChannelValidator()
    {
        // Channel name validation
        RuleFor(x => x.ChannelName)
            .NotEmpty()
            .WithMessage("Tên kênh chat không được để trống")
            .MaximumLength(200)
            .WithMessage("Tên kênh chat không được vượt quá 200 ký tự")
            .MinimumLength(3)
            .WithMessage("Tên kênh chat phải có ít nhất 3 ký tự");

        // Channel type validation
        RuleFor(x => x.ChannelType)
            .NotEmpty()
            .WithMessage("Loại kênh không được để trống")
            .Must(ct => new[] { "Support", "Sales", "Technical", "Feedback", "Emergency" }.Contains(ct))
            .WithMessage("Loại kênh phải là Support, Sales, Technical, Feedback hoặc Emergency");

        // Customer ID validation (optional)
        When(x => x.CustomerId.HasValue, () =>
        {
            RuleFor(x => x.CustomerId!.Value)
                .GreaterThan(0)
                .WithMessage("Customer ID phải lớn hơn 0");
        });

        // Assigned user ID validation (optional)
        When(x => x.AssignedUserId.HasValue, () =>
        {
            RuleFor(x => x.AssignedUserId!.Value)
                .GreaterThan(0)
                .WithMessage("Assigned User ID phải lớn hơn 0");
        });

        // Priority validation
        RuleFor(x => x.Priority)
            .NotEmpty()
            .WithMessage("Độ ưu tiên không được để trống")
            .Must(p => new[] { "High", "Medium", "Low", "Urgent" }.Contains(p))
            .WithMessage("Độ ưu tiên phải là High, Medium, Low hoặc Urgent");

        // Tags validation (optional)
        When(x => !string.IsNullOrEmpty(x.Tags), () =>
        {
            RuleFor(x => x.Tags)
                .MaximumLength(500)
                .WithMessage("Tags không được vượt quá 500 ký tự");
        });

        // Business rule: Emergency channel should have high priority
        When(x => x.ChannelType == "Emergency", () =>
        {
            RuleFor(x => x.Priority)
                .Must(p => p == "High" || p == "Urgent")
                .WithMessage("Kênh Emergency phải có độ ưu tiên High hoặc Urgent");
        });

        // Business rule: Support/Technical channels should have customer or assigned user
        When(x => x.ChannelType == "Support" || x.ChannelType == "Technical", () =>
        {
            RuleFor(x => x)
                .Must(dto => dto.CustomerId.HasValue || dto.AssignedUserId.HasValue)
                .WithMessage("Kênh Support/Technical phải có Customer hoặc Assigned User");
        });
    }
}
