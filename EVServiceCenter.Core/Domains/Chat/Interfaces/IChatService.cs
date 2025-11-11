using EVServiceCenter.Core.Domains.Chat.DTOs.Requests;
using EVServiceCenter.Core.Domains.Chat.DTOs.Responses;

namespace EVServiceCenter.Core.Domains.Chat.Interfaces;

/// <summary>
/// Chat service interface
/// Handles chat channel and message operations
/// </summary>
public interface IChatService
{
    /// <summary>
    /// Create a new chat channel
    /// </summary>
    Task<ChatChannelResponseDto> CreateChannelAsync(
        CreateChannelRequestDto request,
        int createdByUserId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get chat channels for a user
    /// </summary>
    Task<List<ChatChannelResponseDto>> GetUserChannelsAsync(
        int userId,
        int? customerId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get chat channel by ID
    /// </summary>
    Task<ChatChannelResponseDto?> GetChannelByIdAsync(
        int channelId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send a message to a channel
    /// </summary>
    Task<ChatMessageResponseDto> SendMessageAsync(
        SendMessageRequestDto request,
        int senderId,
        string senderType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get chat history for a channel
    /// </summary>
    Task<ChatHistoryResponseDto> GetChatHistoryAsync(
        int channelId,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Mark messages as read
    /// </summary>
    Task<int> MarkMessagesAsReadAsync(
        int channelId,
        int userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Close a chat channel
    /// </summary>
    Task<bool> CloseChannelAsync(
        int channelId,
        int closedBy,
        CancellationToken cancellationToken = default);
}
