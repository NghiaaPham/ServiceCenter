using EVServiceCenter.Core.Domains.WorkOrders.DTOs.Requests;
using EVServiceCenter.Core.Domains.WorkOrders.DTOs.Responses;
using EVServiceCenter.Core.Domains.WorkOrders.Interfaces;
using EVServiceCenter.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EVServiceCenter.Infrastructure.Domains.WorkOrders.Services;

/// <summary>
/// Service for managing work order timeline events
/// Tracks all changes and activities on work orders
/// </summary>
public class WorkOrderTimelineService : IWorkOrderTimelineService
{
    private readonly EVDbContext _context;
    private readonly ILogger<WorkOrderTimelineService> _logger;

    public WorkOrderTimelineService(
        EVDbContext context,
        ILogger<WorkOrderTimelineService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<WorkOrderTimelineResponseDto> AddTimelineEventAsync(
        int workOrderId,
        AddWorkOrderTimelineRequestDto request,
        int performedBy,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adding timeline event for WorkOrder {WorkOrderId}: {EventType}",
            workOrderId, request.EventType);

        var timeline = new WorkOrderTimeline
        {
            WorkOrderId = workOrderId,
            EventType = request.EventType,
            EventDescription = request.EventDescription,
            EventData = request.EventData,
            IsVisible = request.IsVisible,
            PerformedBy = performedBy,
            EventDate = DateTime.UtcNow
        };

        _context.WorkOrderTimelines.Add(timeline);
        await _context.SaveChangesAsync(cancellationToken);

        // Load related data for response
        var workOrder = await _context.WorkOrders.FindAsync(new object[] { workOrderId }, cancellationToken);
        var user = await _context.Users.FindAsync(new object[] { performedBy }, cancellationToken);

        return new WorkOrderTimelineResponseDto
        {
            TimelineId = timeline.TimelineId,
            WorkOrderId = timeline.WorkOrderId,
            WorkOrderCode = workOrder?.WorkOrderCode ?? "",
            EventType = timeline.EventType,
            EventDescription = timeline.EventDescription,
            EventData = timeline.EventData,
            EventDate = timeline.EventDate ?? DateTime.UtcNow,
            PerformedBy = performedBy,
            PerformedByName = user?.FullName,
            IsVisible = timeline.IsVisible ?? true
        };
    }

    public async Task<List<WorkOrderTimelineResponseDto>> GetTimelineAsync(
        int workOrderId,
        bool includeHiddenEvents = false,
        CancellationToken cancellationToken = default)
    {
        var query = _context.WorkOrderTimelines
            .Include(t => t.WorkOrder)
            .Include(t => t.PerformedByNavigation)
            .Where(t => t.WorkOrderId == workOrderId);

        if (!includeHiddenEvents)
        {
            query = query.Where(t => t.IsVisible == true);
        }

        var timelines = await query
            .OrderByDescending(t => t.EventDate)
            .Select(t => new WorkOrderTimelineResponseDto
            {
                TimelineId = t.TimelineId,
                WorkOrderId = t.WorkOrderId,
                WorkOrderCode = t.WorkOrder != null ? t.WorkOrder.WorkOrderCode : "",
                EventType = t.EventType,
                EventDescription = t.EventDescription,
                EventData = t.EventData,
                EventDate = t.EventDate ?? DateTime.UtcNow,
                PerformedBy = t.PerformedBy,
                PerformedByName = t.PerformedByNavigation != null ? t.PerformedByNavigation.FullName : null,
                IsVisible = t.IsVisible ?? true
            })
            .ToListAsync(cancellationToken);

        return timelines;
    }

    public async Task<List<WorkOrderTimelineResponseDto>> GetCustomerTimelineAsync(
        int workOrderId,
        CancellationToken cancellationToken = default)
    {
        // Customer timeline only shows visible events
        return await GetTimelineAsync(workOrderId, includeHiddenEvents: false, cancellationToken);
    }

    public async Task AddStatusChangeEventAsync(
        int workOrderId,
        string fromStatus,
        string toStatus,
        string? notes,
        int performedBy,
        CancellationToken cancellationToken = default)
    {
        var request = new AddWorkOrderTimelineRequestDto
        {
            EventType = "StatusChange",
            EventDescription = $"Status changed from '{fromStatus}' to '{toStatus}'",
            EventData = notes,
            IsVisible = true
        };

        await AddTimelineEventAsync(workOrderId, request, performedBy, cancellationToken);
    }

    public async Task AddTechnicianAssignmentEventAsync(
        int workOrderId,
        string technicianName,
        int performedBy,
        CancellationToken cancellationToken = default)
    {
        var request = new AddWorkOrderTimelineRequestDto
        {
            EventType = "TechnicianAssignment",
            EventDescription = $"Technician assigned: {technicianName}",
            IsVisible = true
        };

        await AddTimelineEventAsync(workOrderId, request, performedBy, cancellationToken);
    }
}
