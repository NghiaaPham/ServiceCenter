using EVServiceCenter.Core.Domains.Chat.DTOs.Responses;

namespace EVServiceCenter.API.Realtime;

public interface IChatRealtimeBroadcaster
{
    Task NotifyChannelCreatedAsync(ChatChannelResponseDto channel, CancellationToken cancellationToken = default);
    Task NotifyChannelClosedAsync(int channelId, CancellationToken cancellationToken = default);
    Task BroadcastMessageAsync(ChatMessageResponseDto message, CancellationToken cancellationToken = default);
    Task BroadcastMessagesReadAsync(int channelId, int userId, int count, CancellationToken cancellationToken = default);
}
