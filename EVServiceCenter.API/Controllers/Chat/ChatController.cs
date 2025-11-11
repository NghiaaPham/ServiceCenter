using EVServiceCenter.API.Realtime;
using EVServiceCenter.Core.Domains.Chat.DTOs.Requests;
using EVServiceCenter.Core.Domains.Chat.Interfaces;
using EVServiceCenter.Core.Domains.Shared.Interfaces;
using EVServiceCenter.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVServiceCenter.API.Controllers.Chat;

/// <summary>
/// Chat Management
/// Handles real-time chat operations and history
/// </summary>
[ApiController]
[Route("api/chat")]
[ApiExplorerSettings(GroupName = "Chat")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly IRepository<ChatChannel> _channelRepository;
    private readonly ILogger<ChatController> _logger;
    private readonly IChatRealtimeBroadcaster _realtimeBroadcaster;

    public ChatController(
        IChatService chatService,
        IRepository<ChatChannel> channelRepository,
        ILogger<ChatController> logger,
        IChatRealtimeBroadcaster realtimeBroadcaster)
    {
        _chatService = chatService;
        _channelRepository = channelRepository;
        _logger = logger;
        _realtimeBroadcaster = realtimeBroadcaster;
    }

    /// <summary>
    /// [Create] Create a new chat channel
    /// </summary>
    [HttpPost("channels")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateChannel(
        [FromBody] CreateChannelRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();

            var channel = await _chatService.CreateChannelAsync(
                request,
                userId,
                cancellationToken);

            await _realtimeBroadcaster.NotifyChannelCreatedAsync(channel, cancellationToken);

            return CreatedAtAction(
                nameof(GetChannel),
                new { id = channel.ChannelId },
                new
                {
                    success = true,
                    data = channel,
                    message = "Chat channel created successfully"
                });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex,
                "Validation error while creating chat channel. Field: {Field}",
                ex.ParamName ?? "unknown");

            return BadRequest(new
            {
                success = false,
                message = ex.Message,
                field = ex.ParamName
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating chat channel");
            return StatusCode(500, new
            {
                success = false,
                message = "Error creating chat channel"
            });
        }
    }

    /// <summary>
    /// [List] Get user's chat channels
    /// </summary>
    [HttpGet("channels")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserChannels(CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var customerId = GetCurrentCustomerId();

            var channels = await _chatService.GetUserChannelsAsync(
                userId,
                customerId,
                cancellationToken);

            return Ok(new
            {
                success = true,
                data = channels,
                count = channels.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user channels");
            return StatusCode(500, new
            {
                success = false,
                message = "Error retrieving channels"
            });
        }
    }

    /// <summary>
    /// [Details] Get chat channel by ID
    /// </summary>
    [HttpGet("channels/{id:int}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetChannel(
        int id,
        CancellationToken cancellationToken)
    {
        try
        {
            // SECURITY: Verify channel ownership for customer role
            if (!await VerifyChannelOwnershipAsync(id, cancellationToken))
            {
                _logger.LogWarning("User {UserId} attempted to access chat channel {ChannelId} without ownership",
                    GetCurrentUserId(), id);
                return Forbid();
            }

            var channel = await _chatService.GetChannelByIdAsync(id, cancellationToken);

            if (channel == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = $"Chat channel {id} not found"
                });
            }

            return Ok(new
            {
                success = true,
                data = channel
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving channel {ChannelId}", id);
            return StatusCode(500, new
            {
                success = false,
                message = "Error retrieving channel"
            });
        }
    }

    /// <summary>
    /// [Create] Send a message (REST endpoint for non-SignalR clients)
    /// Original requirement: POST /api/chat/send
    /// </summary>
    [HttpPost("send")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendMessage(
        [FromBody] SendMessageRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var role = GetCurrentUserRole();
            var senderType = role == "Customer" ? "Customer" : "Staff";

            var message = await _chatService.SendMessageAsync(
                request,
                userId,
                senderType,
                cancellationToken);

            await _realtimeBroadcaster.BroadcastMessageAsync(message, cancellationToken);

            return CreatedAtAction(
                nameof(GetChatHistory),
                new { channelId = request.ChannelId },
                new
                {
                    success = true,
                    data = message,
                    message = "Message sent successfully"
                });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message");
            return StatusCode(500, new
            {
                success = false,
                message = "Error sending message"
            });
        }
    }

    /// <summary>
    /// [List] Get chat history for a channel
    /// Original requirement: GET /api/chat/history
    /// </summary>
    [HttpGet("history")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetChatHistory(
        [FromQuery] int channelId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (channelId <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Channel ID is required"
                });
            }

            // SECURITY: Verify channel ownership for customer role
            if (!await VerifyChannelOwnershipAsync(channelId, cancellationToken))
            {
                _logger.LogWarning("User {UserId} attempted to access chat history for channel {ChannelId} without ownership",
                    GetCurrentUserId(), channelId);
                return Forbid();
            }

            var history = await _chatService.GetChatHistoryAsync(
                channelId,
                pageNumber,
                pageSize,
                cancellationToken);

            return Ok(new
            {
                success = true,
                data = history
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving chat history for channel {ChannelId}", channelId);
            return StatusCode(500, new
            {
                success = false,
                message = "Error retrieving chat history"
            });
        }
    }

    /// <summary>
    /// [Update] Mark messages as read in a channel
    /// </summary>
    [HttpPut("channels/{channelId:int}/read")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkAsRead(
        int channelId,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();

            var count = await _chatService.MarkMessagesAsReadAsync(
                channelId,
                userId,
                cancellationToken);

            if (count > 0)
            {
                await _realtimeBroadcaster.BroadcastMessagesReadAsync(channelId, userId, count, cancellationToken);
            }

            return Ok(new
            {
                success = true,
                data = new { markedCount = count },
                message = $"Marked {count} messages as read"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking messages as read in channel {ChannelId}", channelId);
            return StatusCode(500, new
            {
                success = false,
                message = "Error marking messages as read"
            });
        }
    }

    /// <summary>
    /// [Update] Close a chat channel
    /// </summary>
    [HttpPut("channels/{channelId:int}/close")]
    [Authorize(Roles = "Admin,Staff")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CloseChannel(
        int channelId,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();

            var result = await _chatService.CloseChannelAsync(
                channelId,
                userId,
                cancellationToken);

            if (!result)
            {
                return NotFound(new
                {
                    success = false,
                    message = $"Chat channel {channelId} not found"
                });
            }

            await _realtimeBroadcaster.NotifyChannelClosedAsync(channelId, cancellationToken);

            return Ok(new
            {
                success = true,
                message = "Chat channel closed successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error closing channel {ChannelId}", channelId);
            return StatusCode(500, new
            {
                success = false,
                message = "Error closing channel"
            });
        }
    }

    #region Helper Methods

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("UserId");
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
        {
            return userId;
        }
        throw new UnauthorizedAccessException("User ID not found in token");
    }

    private int? GetCurrentCustomerId()
    {
        var customerIdClaim = User.FindFirst("CustomerId");
        if (customerIdClaim != null && int.TryParse(customerIdClaim.Value, out int customerId))
        {
            return customerId;
        }
        return null;
    }

    private string GetCurrentUserRole()
    {
        return User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value ?? "User";
    }

    /// <summary>
    /// Verify that the current user owns the chat channel
    /// Staff/Admin bypass this check (via role)
    /// Customers must own the channel OR be assigned to it
    /// </summary>
    private async Task<bool> VerifyChannelOwnershipAsync(int channelId, CancellationToken cancellationToken)
    {
        // Staff and above can access any channel
        if (User.IsInRole("Admin") || User.IsInRole("Staff"))
        {
            return true;
        }

        // For customers, verify ownership via CustomerId
        var customerIdClaim = User.FindFirst("CustomerId");
        if (customerIdClaim == null || !int.TryParse(customerIdClaim.Value, out int customerId))
        {
            _logger.LogWarning("CustomerId claim not found for user {UserId}", GetCurrentUserId());
            return false;
        }

        // Get channel and verify CustomerId matches
        var channel = await _channelRepository.GetByIdAsync(channelId, cancellationToken);
        if (channel == null)
        {
            throw new KeyNotFoundException($"Chat channel {channelId} not found");
        }

        return channel.CustomerId == customerId;
    }

    #endregion
}
