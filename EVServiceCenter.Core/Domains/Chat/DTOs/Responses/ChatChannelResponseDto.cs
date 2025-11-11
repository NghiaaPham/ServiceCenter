namespace EVServiceCenter.Core.Domains.Chat.DTOs.Responses;

/// <summary>
/// Response DTO for chat channel
/// </summary>
public class ChatChannelResponseDto
{
    public int ChannelId { get; set; }
    public string ChannelName { get; set; } = null!;
    public string ChannelType { get; set; } = null!;
    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public int? CustomerUserId { get; set; }
    public int? AssignedUserId { get; set; }
    public string? AssignedUserName { get; set; }
    public string Status { get; set; } = null!;
    public string Priority { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
    public DateTime? LastMessageDate { get; set; }
    public DateTime? ClosedDate { get; set; }
    public int? Rating { get; set; }
    public string? Tags { get; set; }
    public int UnreadCount { get; set; }
}
