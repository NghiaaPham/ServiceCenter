using EVServiceCenter.Core.Domains.Chat.DTOs.Requests;
using EVServiceCenter.Core.Domains.Chat.DTOs.Responses;
using EVServiceCenter.Core.Domains.Chat.Interfaces;
using EVServiceCenter.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EVServiceCenter.Infrastructure.Domains.Chat.Services;

/// <summary>
/// Chat service implementation
/// Handles chat operations with performance optimization
/// </summary>
public class ChatService : IChatService
{
    private readonly EVDbContext _context;
    private readonly ILogger<ChatService> _logger;

    public ChatService(
        EVDbContext context,
        ILogger<ChatService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ChatChannelResponseDto> CreateChannelAsync(
        CreateChannelRequestDto request,
        int createdByUserId,
        CancellationToken cancellationToken = default)
    {
        // Normalize optional references to avoid FK violations
        if (request.AssignedUserId.HasValue && request.AssignedUserId.Value <= 0)
        {
            request.AssignedUserId = null;
        }

        if (request.CustomerId.HasValue && request.CustomerId.Value <= 0)
        {
            request.CustomerId = null;
        }

        // Validate foreign keys upfront so we can return 4xx instead of 500
        if (request.CustomerId.HasValue)
        {
            var customerExists = await _context.Customers
                .AsNoTracking()
                .AnyAsync(c => c.CustomerId == request.CustomerId.Value, cancellationToken);

            if (!customerExists)
            {
                throw new ArgumentException(
                    $"Customer {request.CustomerId.Value} does not exist",
                    nameof(request.CustomerId));
            }
        }

        if (request.AssignedUserId.HasValue)
        {
            var userExists = await _context.Users
                .AsNoTracking()
                .AnyAsync(u => u.UserId == request.AssignedUserId.Value, cancellationToken);

            if (!userExists)
            {
                throw new ArgumentException(
                    $"Assigned user {request.AssignedUserId.Value} does not exist",
                    nameof(request.AssignedUserId));
            }
        }

        var channel = new ChatChannel
        {
            ChannelName = request.ChannelName,
            ChannelType = request.ChannelType,
            CustomerId = request.CustomerId,
            AssignedUserId = request.AssignedUserId,
            Status = "Open",
            Priority = request.Priority,
            CreatedDate = DateTime.UtcNow,
            Tags = request.Tags
        };

        _context.Set<ChatChannel>().Add(channel);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created chat channel {ChannelId} by user {UserId}",
            channel.ChannelId, createdByUserId);

        return await GetChannelByIdAsync(channel.ChannelId, cancellationToken)
            ?? throw new InvalidOperationException("Failed to retrieve created channel");
    }

    public async Task<List<ChatChannelResponseDto>> GetUserChannelsAsync(
        int userId,
        int? customerId,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Set<ChatChannel>()
            .AsNoTracking()
            .Include(c => c.Customer)
            .Include(c => c.AssignedUser)
            .Where(c => c.AssignedUserId == userId ||
                       (customerId.HasValue && c.CustomerId == customerId.Value));

        var channels = await query
            .OrderByDescending(c => c.LastMessageDate ?? c.CreatedDate)
            .Select(c => new ChatChannelResponseDto
            {
                ChannelId = c.ChannelId,
                ChannelName = c.ChannelName,
                ChannelType = c.ChannelType ?? "Support",
                CustomerId = c.CustomerId,
                CustomerName = c.Customer != null ? c.Customer.FullName : null,
                CustomerUserId = c.Customer != null ? c.Customer.UserId : null,
                AssignedUserId = c.AssignedUserId,
                AssignedUserName = c.AssignedUser != null ? c.AssignedUser.FullName : null,
                Status = c.Status ?? "Open",
                Priority = c.Priority ?? "Medium",
                CreatedDate = c.CreatedDate ?? DateTime.UtcNow,
                LastMessageDate = c.LastMessageDate,
                ClosedDate = c.ClosedDate,
                Rating = c.Rating,
                Tags = c.Tags,
                UnreadCount = c.ChatMessages.Count(m => m.IsRead != true && m.SenderId != userId)
            })
            .ToListAsync(cancellationToken);

        return channels;
    }

    public async Task<ChatChannelResponseDto?> GetChannelByIdAsync(
        int channelId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<ChatChannel>()
            .AsNoTracking()
            .Include(c => c.Customer)
            .Include(c => c.AssignedUser)
            .Where(c => c.ChannelId == channelId)
            .Select(c => new ChatChannelResponseDto
            {
                ChannelId = c.ChannelId,
                ChannelName = c.ChannelName,
                ChannelType = c.ChannelType ?? "Support",
                CustomerId = c.CustomerId,
                CustomerName = c.Customer != null ? c.Customer.FullName : null,
                CustomerUserId = c.Customer != null ? c.Customer.UserId : null,
                AssignedUserId = c.AssignedUserId,
                AssignedUserName = c.AssignedUser != null ? c.AssignedUser.FullName : null,
                Status = c.Status ?? "Open",
                Priority = c.Priority ?? "Medium",
                CreatedDate = c.CreatedDate ?? DateTime.UtcNow,
                LastMessageDate = c.LastMessageDate,
                ClosedDate = c.ClosedDate,
                Rating = c.Rating,
                Tags = c.Tags,
                UnreadCount = c.ChatMessages.Count(m => m.IsRead != true)
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ChatMessageResponseDto> SendMessageAsync(
        SendMessageRequestDto request,
        int senderId,
        string senderType,
        CancellationToken cancellationToken = default)
    {
        // Verify channel exists
        var channel = await _context.Set<ChatChannel>()
            .FirstOrDefaultAsync(c => c.ChannelId == request.ChannelId, cancellationToken);

        if (channel == null)
        {
            throw new KeyNotFoundException($"Chat channel {request.ChannelId} not found");
        }

        var message = new ChatMessage
        {
            ChannelId = request.ChannelId,
            SenderType = senderType,
            SenderId = senderId,
            MessageType = request.MessageType,
            MessageContent = request.MessageContent,
            AttachmentUrl = request.AttachmentUrl,
            AttachmentType = request.AttachmentType,
            AttachmentSize = request.AttachmentSize,
            ReplyToMessageId = request.ReplyToMessageId,
            RelatedAppointmentId = request.RelatedAppointmentId,
            RelatedWorkOrderId = request.RelatedWorkOrderId,
            RelatedInvoiceId = request.RelatedInvoiceId,
            IsRead = false,
            IsDelivered = true,
            DeliveredDate = DateTime.UtcNow,
            Timestamp = DateTime.UtcNow,
            IsDeleted = false
        };

        _context.Set<ChatMessage>().Add(message);

        // Update channel last message date
        channel.LastMessageDate = DateTime.UtcNow;

        var senderName = await _context.Users
            .AsNoTracking()
            .Where(u => u.UserId == senderId)
            .Select(u => u.FullName)
            .FirstOrDefaultAsync(cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Message {MessageId} sent to channel {ChannelId} by {SenderType} {SenderId}",
            message.MessageId, request.ChannelId, senderType, senderId);

        return new ChatMessageResponseDto
        {
            MessageId = message.MessageId,
            ChannelId = message.ChannelId,
            SenderType = message.SenderType,
            SenderId = message.SenderId,
            SenderName = senderName,
            MessageType = message.MessageType,
            MessageContent = message.MessageContent,
            AttachmentUrl = message.AttachmentUrl,
            AttachmentType = message.AttachmentType,
            AttachmentSize = message.AttachmentSize,
            IsRead = message.IsRead ?? false,
            ReadDate = message.ReadDate,
            IsDelivered = message.IsDelivered ?? false,
            DeliveredDate = message.DeliveredDate,
            ReplyToMessageId = message.ReplyToMessageId,
            RelatedAppointmentId = message.RelatedAppointmentId,
            RelatedWorkOrderId = message.RelatedWorkOrderId,
            RelatedInvoiceId = message.RelatedInvoiceId,
            Timestamp = message.Timestamp ?? DateTime.UtcNow,
            EditedDate = message.EditedDate,
            IsDeleted = message.IsDeleted ?? false
        };
    }

    public async Task<ChatHistoryResponseDto> GetChatHistoryAsync(
        int channelId,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        // Get channel info
        var channel = await GetChannelByIdAsync(channelId, cancellationToken);
        if (channel == null)
        {
            throw new KeyNotFoundException($"Chat channel {channelId} not found");
        }

        // Get total message count
        var totalMessages = await _context.Set<ChatMessage>()
            .AsNoTracking()
            .CountAsync(m => m.ChannelId == channelId && m.IsDeleted != true, cancellationToken);

        // Get messages with pagination
        var messages = await _context.Set<ChatMessage>()
            .AsNoTracking()
            .Where(m => m.ChannelId == channelId && m.IsDeleted != true)
            .OrderByDescending(m => m.Timestamp)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new ChatMessageResponseDto
            {
                MessageId = m.MessageId,
                ChannelId = m.ChannelId,
                SenderType = m.SenderType,
                SenderId = m.SenderId,
                SenderName = _context.Users.Where(u => u.UserId == m.SenderId)
                    .Select(u => u.FullName)
                    .FirstOrDefault(),
                MessageType = m.MessageType ?? "Text",
                MessageContent = m.MessageContent,
                AttachmentUrl = m.AttachmentUrl,
                AttachmentType = m.AttachmentType,
                AttachmentSize = m.AttachmentSize,
                IsRead = m.IsRead ?? false,
                ReadDate = m.ReadDate,
                IsDelivered = m.IsDelivered ?? false,
                DeliveredDate = m.DeliveredDate,
                ReplyToMessageId = m.ReplyToMessageId,
                RelatedAppointmentId = m.RelatedAppointmentId,
                RelatedWorkOrderId = m.RelatedWorkOrderId,
                RelatedInvoiceId = m.RelatedInvoiceId,
                Timestamp = m.Timestamp ?? DateTime.UtcNow,
                EditedDate = m.EditedDate,
                IsDeleted = m.IsDeleted ?? false
            })
            .ToListAsync(cancellationToken);

        // Reverse to show oldest first
        messages.Reverse();

        return new ChatHistoryResponseDto
        {
            Channel = channel,
            Messages = messages,
            TotalMessages = totalMessages,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<int> MarkMessagesAsReadAsync(
        int channelId,
        int userId,
        CancellationToken cancellationToken = default)
    {
        var unreadMessages = await _context.Set<ChatMessage>()
            .Where(m => m.ChannelId == channelId &&
                       m.SenderId != userId &&
                       m.IsRead != true)
            .ToListAsync(cancellationToken);

        if (!unreadMessages.Any())
        {
            return 0;
        }

        var readTime = DateTime.UtcNow;
        foreach (var message in unreadMessages)
        {
            message.IsRead = true;
            message.ReadDate = readTime;
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Marked {Count} messages as read in channel {ChannelId} for user {UserId}",
            unreadMessages.Count, channelId, userId);

        return unreadMessages.Count;
    }

    public async Task<bool> CloseChannelAsync(
        int channelId,
        int closedBy,
        CancellationToken cancellationToken = default)
    {
        var channel = await _context.Set<ChatChannel>()
            .FirstOrDefaultAsync(c => c.ChannelId == channelId, cancellationToken);

        if (channel == null)
        {
            return false;
        }

        channel.Status = "Closed";
        channel.ClosedDate = DateTime.UtcNow;
        channel.ClosedBy = closedBy;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Chat channel {ChannelId} closed by user {UserId}",
            channelId, closedBy);

        return true;
    }
}
