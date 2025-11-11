using EVServiceCenter.Core.Domains.Chat.DTOs.Requests;
using FluentValidation;

namespace EVServiceCenter.Core.Domains.Chat.Validators;

/// <summary>
/// Validator for SendMessageRequestDto
/// Validates message content, attachments and related entities
/// </summary>
public class SendMessageValidator : AbstractValidator<SendMessageRequestDto>
{
    private const int MaxMessageLength = 5000;
    private const int MaxAttachmentSize = 10485760; // 10MB in bytes

    public SendMessageValidator()
    {
        // Channel ID validation
        RuleFor(x => x.ChannelId)
            .GreaterThan(0)
            .WithMessage("Channel ID phải lớn hơn 0");

        // Message content validation
        RuleFor(x => x.MessageContent)
            .NotEmpty()
            .WithMessage("Nội dung tin nhắn không được để trống")
            .MaximumLength(MaxMessageLength)
            .WithMessage($"Nội dung tin nhắn không được vượt quá {MaxMessageLength} ký tự");

        // Message type validation
        RuleFor(x => x.MessageType)
            .NotEmpty()
            .WithMessage("Loại tin nhắn không được để trống")
            .Must(mt => new[] { "Text", "Image", "File", "System" }.Contains(mt))
            .WithMessage("Loại tin nhắn phải là Text, Image, File hoặc System");

        // Attachment URL validation
        When(x => !string.IsNullOrEmpty(x.AttachmentUrl), () =>
        {
            RuleFor(x => x.AttachmentUrl)
                .Must(BeAValidUrl)
                .WithMessage("URL đính kèm không hợp lệ")
                .MaximumLength(500)
                .WithMessage("URL đính kèm không được vượt quá 500 ký tự");

            // If attachment URL is provided, require attachment type
            RuleFor(x => x.AttachmentType)
                .NotEmpty()
                .WithMessage("Loại đính kèm bắt buộc khi có URL đính kèm");
        });

        // Attachment type validation
        When(x => !string.IsNullOrEmpty(x.AttachmentType), () =>
        {
            RuleFor(x => x.AttachmentType)
                .MaximumLength(50)
                .WithMessage("Loại đính kèm không được vượt quá 50 ký tự");
        });

        // Attachment size validation
        When(x => x.AttachmentSize.HasValue, () =>
        {
            RuleFor(x => x.AttachmentSize!.Value)
                .GreaterThan(0)
                .WithMessage("Kích thước đính kèm phải lớn hơn 0")
                .LessThanOrEqualTo(MaxAttachmentSize)
                .WithMessage($"Kích thước đính kèm không được vượt quá {MaxAttachmentSize / 1024 / 1024}MB");
        });

        // Reply to message ID validation
        When(x => x.ReplyToMessageId.HasValue, () =>
        {
            RuleFor(x => x.ReplyToMessageId!.Value)
                .GreaterThan(0)
                .WithMessage("ID tin nhắn trả lời phải lớn hơn 0");
        });

        // Related appointment ID validation
        When(x => x.RelatedAppointmentId.HasValue, () =>
        {
            RuleFor(x => x.RelatedAppointmentId!.Value)
                .GreaterThan(0)
                .WithMessage("ID lịch hẹn liên quan phải lớn hơn 0");
        });

        // Related work order ID validation
        When(x => x.RelatedWorkOrderId.HasValue, () =>
        {
            RuleFor(x => x.RelatedWorkOrderId!.Value)
                .GreaterThan(0)
                .WithMessage("ID work order liên quan phải lớn hơn 0");
        });

        // Related invoice ID validation
        When(x => x.RelatedInvoiceId.HasValue, () =>
        {
            RuleFor(x => x.RelatedInvoiceId!.Value)
                .GreaterThan(0)
                .WithMessage("ID hóa đơn liên quan phải lớn hơn 0");
        });

        // Business rule: For Image/File type, require attachment URL
        When(x => x.MessageType == "Image" || x.MessageType == "File", () =>
        {
            RuleFor(x => x.AttachmentUrl)
                .NotEmpty()
                .WithMessage("Tin nhắn loại Image hoặc File bắt buộc phải có URL đính kèm");
        });

        // Business rule: System messages should have specific content format
        When(x => x.MessageType == "System", () =>
        {
            RuleFor(x => x.MessageContent)
                .MinimumLength(10)
                .WithMessage("Tin nhắn hệ thống phải có nội dung rõ ràng (tối thiểu 10 ký tự)");
        });
    }

    private bool BeAValidUrl(string? url)
    {
        if (string.IsNullOrEmpty(url))
            return true;

        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
