using EVServiceCenter.API.Hubs;
using EVServiceCenter.Core.Domains.Chat.DTOs.Responses;
using Microsoft.AspNetCore.SignalR;

namespace EVServiceCenter.API.Realtime;

public class SignalRChatBroadcaster : IChatRealtimeBroadcaster
{
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly IUserConnectionManager _connectionManager;
    private readonly ILogger<SignalRChatBroadcaster> _logger;

    public SignalRChatBroadcaster(
        IHubContext<ChatHub> hubContext,
        IUserConnectionManager connectionManager,
        ILogger<SignalRChatBroadcaster> logger)
    {
        _hubContext = hubContext;
        _connectionManager = connectionManager;
        _logger = logger;
    }

    public async Task BroadcastMessageAsync(ChatMessageResponseDto message, CancellationToken cancellationToken = default)
    {
        var groupName = GetGroupName(message.ChannelId);

        await _hubContext.Clients.Group(groupName)
            .SendAsync("ReceiveMessage", message, cancellationToken);

        _logger.LogDebug("Broadcasted message {MessageId} to group {GroupName}", message.MessageId, groupName);
    }

    public async Task BroadcastMessagesReadAsync(int channelId, int userId, int count, CancellationToken cancellationToken = default)
    {
        var groupName = GetGroupName(channelId);

        await _hubContext.Clients.Group(groupName)
            .SendAsync("MessagesRead", new
            {
                ChannelId = channelId,
                UserId = userId,
                Count = count,
                Timestamp = DateTime.UtcNow
            }, cancellationToken);

        _logger.LogDebug("Broadcasted read-receipt for channel {ChannelId} by user {UserId} ({Count} messages)", channelId, userId, count);
    }

    public async Task NotifyChannelClosedAsync(int channelId, CancellationToken cancellationToken = default)
    {
        var groupName = GetGroupName(channelId);

        await _hubContext.Clients.Group(groupName)
            .SendAsync("ChannelClosed", new
            {
                ChannelId = channelId,
                Timestamp = DateTime.UtcNow
            }, cancellationToken);

        _logger.LogInformation("Notified channel {ChannelId} closed", channelId);
    }

    public async Task NotifyChannelCreatedAsync(ChatChannelResponseDto channel, CancellationToken cancellationToken = default)
    {
        var groupName = GetGroupName(channel.ChannelId);
        var tasks = new List<Task>();

        if (channel.AssignedUserId.HasValue)
        {
            tasks.AddRange(AddUserConnectionsToGroup(channel.AssignedUserId.Value, groupName, cancellationToken));
            tasks.Add(_hubContext.Clients.User(channel.AssignedUserId.Value.ToString())
                .SendAsync("ChannelCreated", channel, cancellationToken));
        }

        if (channel.CustomerUserId.HasValue)
        {
            tasks.AddRange(AddUserConnectionsToGroup(channel.CustomerUserId.Value, groupName, cancellationToken));
            tasks.Add(_hubContext.Clients.User(channel.CustomerUserId.Value.ToString())
                .SendAsync("ChannelCreated", channel, cancellationToken));
        }

        if (tasks.Count > 0)
        {
            await Task.WhenAll(tasks);
        }

        _logger.LogInformation("Notified participants about channel {ChannelId} creation", channel.ChannelId);
    }

    private IEnumerable<Task> AddUserConnectionsToGroup(int userId, string groupName, CancellationToken cancellationToken)
    {
        var connectionIds = _connectionManager.GetUserConnections(userId);

        foreach (var connectionId in connectionIds)
        {
            yield return _hubContext.Groups.AddToGroupAsync(connectionId, groupName, cancellationToken);
        }
    }

    private static string GetGroupName(int channelId) => $"channel_{channelId}";
}
