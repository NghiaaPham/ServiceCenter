namespace EVServiceCenter.Core.Domains.Chat.DTOs.Responses;

/// <summary>
/// Response DTO for chat history
/// </summary>
public class ChatHistoryResponseDto
{
    public ChatChannelResponseDto Channel { get; set; } = null!;
    public List<ChatMessageResponseDto> Messages { get; set; } = new();
    public int TotalMessages { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalMessages / PageSize);
}
