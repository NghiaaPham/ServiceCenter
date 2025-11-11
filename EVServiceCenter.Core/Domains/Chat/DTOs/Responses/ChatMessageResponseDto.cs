namespace EVServiceCenter.Core.Domains.Chat.DTOs.Responses;

/// <summary>
/// Response DTO for chat message
/// </summary>
public class ChatMessageResponseDto
{
    public int MessageId { get; set; }
    public int ChannelId { get; set; }
    public string SenderType { get; set; } = null!;
    public int SenderId { get; set; }
    public string? SenderName { get; set; }
    public string MessageType { get; set; } = null!;
    public string MessageContent { get; set; } = null!;
    public string? AttachmentUrl { get; set; }
    public string? AttachmentType { get; set; }
    public int? AttachmentSize { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadDate { get; set; }
    public bool IsDelivered { get; set; }
    public DateTime? DeliveredDate { get; set; }
    public int? ReplyToMessageId { get; set; }
    public int? RelatedAppointmentId { get; set; }
    public int? RelatedWorkOrderId { get; set; }
    public int? RelatedInvoiceId { get; set; }
    public DateTime Timestamp { get; set; }
    public DateTime? EditedDate { get; set; }
    public bool IsDeleted { get; set; }
}
