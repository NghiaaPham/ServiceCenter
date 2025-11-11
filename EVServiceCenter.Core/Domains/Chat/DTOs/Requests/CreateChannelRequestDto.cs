namespace EVServiceCenter.Core.Domains.Chat.DTOs.Requests;

/// <summary>
/// Request DTO for creating a chat channel
/// </summary>
public class CreateChannelRequestDto
{
    public string ChannelName { get; set; } = null!;
    public string ChannelType { get; set; } = "Support";
    public int? CustomerId { get; set; }
    public int? AssignedUserId { get; set; }
    public string Priority { get; set; } = "Medium";
    public string? Tags { get; set; }
}
