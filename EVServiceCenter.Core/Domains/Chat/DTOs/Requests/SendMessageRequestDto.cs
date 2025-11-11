namespace EVServiceCenter.Core.Domains.Chat.DTOs.Requests;

/// <summary>
/// Request DTO for sending a chat message
/// </summary>
public class SendMessageRequestDto
{
    public int ChannelId { get; set; }
    public string MessageContent { get; set; } = null!;
    public string MessageType { get; set; } = "Text";
    public string? AttachmentUrl { get; set; }
    public string? AttachmentType { get; set; }
    public int? AttachmentSize { get; set; }
    public int? ReplyToMessageId { get; set; }
    public int? RelatedAppointmentId { get; set; }
    public int? RelatedWorkOrderId { get; set; }
    public int? RelatedInvoiceId { get; set; }
}
