using EVServiceCenter.API.Realtime;
using EVServiceCenter.Core.Domains.Chat.DTOs.Requests;
using EVServiceCenter.Core.Domains.Chat.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace EVServiceCenter.API.Hubs;

/// <summary>
/// SignalR Hub for real-time chat functionality
/// </summary>
[Authorize]
public class ChatHub : Hub
{
    private readonly IChatService _chatService;
    private readonly ILogger<ChatHub> _logger;
    private readonly IUserConnectionManager _connectionManager;
    private readonly IChatRealtimeBroadcaster _realtimeBroadcaster;

    public ChatHub(
        IChatService chatService,
        ILogger<ChatHub> logger,
        IUserConnectionManager connectionManager,
        IChatRealtimeBroadcaster realtimeBroadcaster)
    {
        _chatService = chatService;
        _logger = logger;
        _connectionManager = connectionManager;
        _realtimeBroadcaster = realtimeBroadcaster;
    }

    /// <summary>
    /// Join a chat channel
    /// </summary>
    public async Task JoinChannel(int channelId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, GetGroupName(channelId));

        _logger.LogInformation("User {UserId} joined channel {ChannelId}",
            Context.User?.FindFirst("UserId")?.Value, channelId);

        await Clients.Group(GetGroupName(channelId))
            .SendAsync("UserJoined", new
            {
                UserId = GetCurrentUserId(),
                UserName = GetCurrentUserName(),
                Timestamp = DateTime.UtcNow
            });
    }

    /// <summary>
    /// Leave a chat channel
    /// </summary>
    public async Task LeaveChannel(int channelId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetGroupName(channelId));

        _logger.LogInformation("User {UserId} left channel {ChannelId}",
            Context.User?.FindFirst("UserId")?.Value, channelId);

        await Clients.Group(GetGroupName(channelId))
            .SendAsync("UserLeft", new
            {
                UserId = GetCurrentUserId(),
                UserName = GetCurrentUserName(),
                Timestamp = DateTime.UtcNow
            });
    }

    /// <summary>
    /// Send a message to a channel
    /// </summary>
    public async Task SendMessage(SendMessageRequestDto request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var senderType = GetCurrentUserRole() == "Customer" ? "Customer" : "Staff";

            var message = await _chatService.SendMessageAsync(
                request,
                userId,
                senderType,
                Context.ConnectionAborted);

            // Broadcast message to all users in the channel
            await _realtimeBroadcaster.BroadcastMessageAsync(message, Context.ConnectionAborted);

            _logger.LogInformation("Message {MessageId} sent to channel {ChannelId}",
                message.MessageId, request.ChannelId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message to channel {ChannelId}", request.ChannelId);
            await Clients.Caller.SendAsync("Error", new { Message = "Failed to send message" });
        }
    }

    /// <summary>
    /// Notify typing status
    /// </summary>
    public async Task NotifyTyping(int channelId, bool isTyping)
    {
        await Clients.OthersInGroup(GetGroupName(channelId))
            .SendAsync("UserTyping", new
            {
                UserId = GetCurrentUserId(),
                UserName = GetCurrentUserName(),
                IsTyping = isTyping,
                Timestamp = DateTime.UtcNow
            });
    }

    /// <summary>
    /// Mark messages as read
    /// </summary>
    public async Task MarkAsRead(int channelId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var count = await _chatService.MarkMessagesAsReadAsync(channelId, userId, Context.ConnectionAborted);

            await _realtimeBroadcaster.BroadcastMessagesReadAsync(channelId, userId, count, Context.ConnectionAborted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking messages as read in channel {ChannelId}", channelId);
        }
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetCurrentUserId();
        _connectionManager.AddConnection(userId, Context.ConnectionId);

        _logger.LogInformation("User {UserId} connected to ChatHub (ConnectionId: {ConnectionId})",
            userId, Context.ConnectionId);

        try
        {
            var customerId = GetCurrentCustomerId();
            var channels = await _chatService.GetUserChannelsAsync(
                userId,
                customerId,
                Context.ConnectionAborted);

            foreach (var channel in channels)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, GetGroupName(channel.ChannelId));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to auto-join channels for user {UserId}", userId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            var userId = GetCurrentUserId();
            _connectionManager.RemoveConnection(userId, Context.ConnectionId);

            _logger.LogInformation("User {UserId} disconnected from ChatHub (ConnectionId: {ConnectionId})",
                userId, Context.ConnectionId);
        }
        catch (UnauthorizedAccessException)
        {
            _logger.LogWarning("Connection {ConnectionId} disconnected without valid user context", Context.ConnectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    #region Helper Methods

    private int GetCurrentUserId()
    {
        var userIdClaim = Context.User?.FindFirst("UserId");
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
        {
            return userId;
        }
        throw new UnauthorizedAccessException("User ID not found in token");
    }

    private int? GetCurrentCustomerId()
    {
        var customerIdClaim = Context.User?.FindFirst("CustomerId");
        if (customerIdClaim != null && int.TryParse(customerIdClaim.Value, out int customerId))
        {
            return customerId;
        }

        return null;
    }

    private string GetCurrentUserName()
    {
        return Context.User?.FindFirst("FullName")?.Value ?? "Unknown";
    }

    private string GetCurrentUserRole()
    {
        return Context.User?.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value ?? "User";
    }

    private static string GetGroupName(int channelId) => $"channel_{channelId}";

    #endregion
}
