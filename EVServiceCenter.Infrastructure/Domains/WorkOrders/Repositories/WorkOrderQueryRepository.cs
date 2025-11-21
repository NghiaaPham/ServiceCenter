using EVServiceCenter.Core.Domains.Shared.Models;
using EVServiceCenter.Core.Domains.WorkOrders.DTOs.Requests;
using EVServiceCenter.Core.Domains.WorkOrders.DTOs.Responses;
using EVServiceCenter.Core.Entities;
using EVServiceCenter.Core.Extensions;
using Microsoft.EntityFrameworkCore;

namespace EVServiceCenter.Infrastructure.Domains.WorkOrders.Repositories;

/// <summary>
/// Query repository for WorkOrder with optimized read operations
/// Separated for better performance and code organization
/// </summary>
public class WorkOrderQueryRepository
{
    private readonly EVDbContext _context;

    public WorkOrderQueryRepository(EVDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get filtered, sorted, and paginated work orders
    /// PERFORMANCE: Uses compiled query and minimal select for list view
    /// </summary>
    public async Task<PagedResult<WorkOrderSummaryDto>> GetWorkOrdersAsync(
        WorkOrderQueryDto query,
        CancellationToken cancellationToken = default)
    {
        // Build query
        var queryable = _context.WorkOrders
            .AsNoTracking()
            .Include(w => w.Customer)
            .Include(w => w.Vehicle).ThenInclude(v => v.Model)
            .Include(w => w.ServiceCenter)
            .Include(w => w.Status)
            .Include(w => w.Technician)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(query.WorkOrderCode))
        {
            queryable = queryable.Where(w => w.WorkOrderCode.Contains(query.WorkOrderCode));
        }

        if (query.CustomerId.HasValue)
        {
            queryable = queryable.Where(w => w.CustomerId == query.CustomerId.Value);
        }

        if (query.VehicleId.HasValue)
        {
            queryable = queryable.Where(w => w.VehicleId == query.VehicleId.Value);
        }

        if (query.ServiceCenterId.HasValue)
        {
            queryable = queryable.Where(w => w.ServiceCenterId == query.ServiceCenterId.Value);
        }

        if (query.TechnicianId.HasValue)
        {
            queryable = queryable.Where(w => w.TechnicianId == query.TechnicianId.Value);
        }

        if (query.StatusId.HasValue)
        {
            queryable = queryable.Where(w => w.StatusId == query.StatusId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Priority))
        {
            queryable = queryable.Where(w => w.Priority == query.Priority);
        }

        if (query.StartDateFrom.HasValue)
        {
            queryable = queryable.Where(w => w.StartDate >= query.StartDateFrom.Value);
        }

        if (query.StartDateTo.HasValue)
        {
            queryable = queryable.Where(w => w.StartDate <= query.StartDateTo.Value);
        }

        if (query.CompletedDateFrom.HasValue)
        {
            queryable = queryable.Where(w => w.CompletedDate >= query.CompletedDateFrom.Value);
        }

        if (query.CompletedDateTo.HasValue)
        {
            queryable = queryable.Where(w => w.CompletedDate <= query.CompletedDateTo.Value);
        }

        if (query.RequiresApproval.HasValue)
        {
            queryable = queryable.Where(w => w.RequiresApproval == query.RequiresApproval.Value);
        }

        if (query.QualityCheckRequired.HasValue)
        {
            queryable = queryable.Where(w => w.QualityCheckRequired == query.QualityCheckRequired.Value);
        }

        // Search term (customer name or vehicle plate)
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var searchTerm = query.SearchTerm.ToLower();
            queryable = queryable.Where(w =>
                w.Customer.FullName!.ToLower().Contains(searchTerm) ||
                w.Vehicle.LicensePlate.ToLower().Contains(searchTerm));
        }

        // Get total count before paging
        var totalRecords = await queryable.CountAsync(cancellationToken);

        // Apply sorting
        queryable = ApplySorting(queryable, query.SortBy, query.SortDirection);

        // Apply paging
        var items = await queryable
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(w => new WorkOrderSummaryDto
            {
                WorkOrderId = w.WorkOrderId,
                WorkOrderCode = w.WorkOrderCode,
                CustomerName = w.Customer.FullName ?? "",
                VehiclePlate = w.Vehicle.LicensePlate,
                VehicleModel = w.Vehicle.Model != null ? w.Vehicle.Model.ModelName : "",
                ServiceCenterName = w.ServiceCenter.CenterName,
                StatusId = w.StatusId,
                StatusName = w.Status.StatusName,
                StatusColor = w.Status.StatusColor,
                Priority = w.Priority,
                SourceType = w.SourceType, // ✅ NEW: Include source type
                StartDate = w.StartDate,
                EstimatedCompletionDate = w.EstimatedCompletionDate,
                CreatedDate = w.CreatedDate ?? DateTime.UtcNow,
                TechnicianName = w.Technician != null ? w.Technician.FullName : null,
                ProgressPercentage = w.ProgressPercentage,
                FinalAmount = w.FinalAmount,
                RequiresApproval = w.RequiresApproval ?? false,
                QualityCheckRequired = w.QualityCheckRequired ?? false
            })
            .ToListAsync(cancellationToken);

        return PagedResultFactory.Create(items, totalRecords, query.PageNumber, query.PageSize);
    }

    /// <summary>
    /// Get detailed work order by ID
    /// PERFORMANCE: Single query with all necessary includes
    /// </summary>
    public async Task<WorkOrderResponseDto?> GetWorkOrderDetailAsync(
        int workOrderId,
        CancellationToken cancellationToken = default)
    {
        var workOrder = await _context.WorkOrders
            .AsNoTracking()
            .Include(w => w.Customer)
            .Include(w => w.Vehicle).ThenInclude(v => v.Model)
            .Include(w => w.ServiceCenter)
            .Include(w => w.Appointment)
            .Include(w => w.Status)
            .Include(w => w.Technician)
            .Include(w => w.Advisor)
            .Include(w => w.Supervisor)
            .Include(w => w.ApprovedByNavigation)
            .Include(w => w.QualityCheckedByNavigation)
            .Include(w => w.WorkOrderServices).ThenInclude(ws => ws.Service)
            .Include(w => w.WorkOrderParts).ThenInclude(wp => wp.Part)
            .FirstOrDefaultAsync(w => w.WorkOrderId == workOrderId, cancellationToken);

        if (workOrder == null)
            return null;

        return MapToResponseDto(workOrder);
    }

    /// <summary>
    /// Get work order by code
    /// </summary>
    public async Task<WorkOrder?> GetByCodeAsync(
        string workOrderCode,
        CancellationToken cancellationToken = default)
    {
        return await _context.WorkOrders
            .FirstOrDefaultAsync(w => w.WorkOrderCode == workOrderCode, cancellationToken);
    }

    /// <summary>
    /// Generate unique work order code
    /// Format: WO-YYYYMMDD-####
    /// </summary>
    public async Task<string> GenerateWorkOrderCodeAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow;
        var datePrefix = $"WO-{today:yyyyMMdd}";

        // Get last work order code for today
        var lastCode = await _context.WorkOrders
            .Where(w => w.WorkOrderCode.StartsWith(datePrefix))
            .OrderByDescending(w => w.WorkOrderCode)
            .Select(w => w.WorkOrderCode)
            .FirstOrDefaultAsync(cancellationToken);

        int sequence = 1;
        if (lastCode != null)
        {
            var lastSequence = lastCode.Substring(lastCode.Length - 4);
            if (int.TryParse(lastSequence, out int parsedSequence))
            {
                sequence = parsedSequence + 1;
            }
        }

        return $"{datePrefix}-{sequence:D4}";
    }

    /// <summary>
    /// Get work orders by technician
    /// PERFORMANCE: Optimized query with minimal includes
    /// </summary>
    public async Task<List<WorkOrderSummaryDto>> GetWorkOrdersByTechnicianAsync(
        int technicianId,
        int? statusId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.WorkOrders
            .AsNoTracking()
            .Where(w => w.TechnicianId == technicianId);

        if (statusId.HasValue)
        {
            query = query.Where(w => w.StatusId == statusId.Value);
        }

        return await query
            .Include(w => w.Customer)
            .Include(w => w.Vehicle).ThenInclude(v => v.Model)
            .Include(w => w.ServiceCenter)
            .Include(w => w.Status)
            .OrderByDescending(w => w.CreatedDate)
            .Select(w => new WorkOrderSummaryDto
            {
                WorkOrderId = w.WorkOrderId,
                WorkOrderCode = w.WorkOrderCode,
                CustomerName = w.Customer.FullName ?? "",
                VehiclePlate = w.Vehicle.LicensePlate,
                VehicleModel = w.Vehicle.Model != null ? w.Vehicle.Model.ModelName : "",
                ServiceCenterName = w.ServiceCenter.CenterName,
                StatusId = w.StatusId,
                StatusName = w.Status.StatusName,
                StatusColor = w.Status.StatusColor,
                Priority = w.Priority,
                SourceType = w.SourceType,
                StartDate = w.StartDate,
                EstimatedCompletionDate = w.EstimatedCompletionDate,
                CreatedDate = w.CreatedDate ?? DateTime.UtcNow,
                TechnicianName = w.Technician != null ? w.Technician.FullName : null,
                ProgressPercentage = w.ProgressPercentage,
                FinalAmount = w.FinalAmount,
                RequiresApproval = w.RequiresApproval ?? false,
                QualityCheckRequired = w.QualityCheckRequired ?? false
            })
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get work orders by vehicle
    /// Used for vehicle maintenance history
    /// </summary>
    public async Task<List<WorkOrderSummaryDto>> GetWorkOrdersByVehicleAsync(
        int vehicleId,
        CancellationToken cancellationToken = default)
    {
        return await _context.WorkOrders
            .AsNoTracking()
            .Where(w => w.VehicleId == vehicleId)
            .Include(w => w.Customer)
            .Include(w => w.Vehicle).ThenInclude(v => v.Model)
            .Include(w => w.ServiceCenter)
            .Include(w => w.Status)
            .Include(w => w.Technician)
            .OrderByDescending(w => w.CreatedDate)
            .Select(w => new WorkOrderSummaryDto
            {
                WorkOrderId = w.WorkOrderId,
                WorkOrderCode = w.WorkOrderCode,
                CustomerName = w.Customer.FullName ?? "",
                VehiclePlate = w.Vehicle.LicensePlate,
                VehicleModel = w.Vehicle.Model != null ? w.Vehicle.Model.ModelName : "",
                ServiceCenterName = w.ServiceCenter.CenterName,
                StatusId = w.StatusId,
                StatusName = w.Status.StatusName,
                StatusColor = w.Status.StatusColor,
                Priority = w.Priority,
                SourceType = w.SourceType,
                StartDate = w.StartDate,
                EstimatedCompletionDate = w.EstimatedCompletionDate,
                CreatedDate = w.CreatedDate ?? DateTime.UtcNow,
                TechnicianName = w.Technician != null ? w.Technician.FullName : null,
                ProgressPercentage = w.ProgressPercentage,
                FinalAmount = w.FinalAmount,
                RequiresApproval = w.RequiresApproval ?? false,
                QualityCheckRequired = w.QualityCheckRequired ?? false
            })
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get work orders by customer
    /// </summary>
    public async Task<List<WorkOrderSummaryDto>> GetWorkOrdersByCustomerAsync(
        int customerId,
        CancellationToken cancellationToken = default)
    {
        return await _context.WorkOrders
            .AsNoTracking()
            .Where(w => w.CustomerId == customerId)
            .Include(w => w.Customer)
            .Include(w => w.Vehicle).ThenInclude(v => v.Model)
            .Include(w => w.ServiceCenter)
            .Include(w => w.Status)
            .Include(w => w.Technician)
            .OrderByDescending(w => w.CreatedDate)
            .Select(w => new WorkOrderSummaryDto
            {
                WorkOrderId = w.WorkOrderId,
                WorkOrderCode = w.WorkOrderCode,
                CustomerName = w.Customer.FullName ?? "",
                VehiclePlate = w.Vehicle.LicensePlate,
                VehicleModel = w.Vehicle.Model != null ? w.Vehicle.Model.ModelName : "",
                ServiceCenterName = w.ServiceCenter.CenterName,
                StatusId = w.StatusId,
                StatusName = w.Status.StatusName,
                StatusColor = w.Status.StatusColor,
                Priority = w.Priority,
                SourceType = w.SourceType,
                StartDate = w.StartDate,
                EstimatedCompletionDate = w.EstimatedCompletionDate,
                CreatedDate = w.CreatedDate ?? DateTime.UtcNow,
                TechnicianName = w.Technician != null ? w.Technician.FullName : null,
                ProgressPercentage = w.ProgressPercentage,
                FinalAmount = w.FinalAmount,
                RequiresApproval = w.RequiresApproval ?? false,
                QualityCheckRequired = w.QualityCheckRequired ?? false
            })
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Check if technician is available for new work orders
    /// PERFORMANCE: Count query only, no entity loading
    /// </summary>
    public async Task<bool> IsTechnicianAvailableAsync(
        int technicianId,
        int maxConcurrentWorkOrders = 5,
        CancellationToken cancellationToken = default)
    {
        // Status IDs: 2=InProgress, 3=WaitingForParts, 4=WaitingForApproval
        var activeStatusIds = new[] { 2, 3, 4 };

        var currentWorkOrders = await _context.WorkOrders
            .CountAsync(w =>
                w.TechnicianId == technicianId &&
                activeStatusIds.Contains(w.StatusId),
                cancellationToken);

        return currentWorkOrders < maxConcurrentWorkOrders;
    }

    /// <summary>
    /// Update work order status
    /// </summary>
    public async Task<bool> UpdateStatusAsync(
        int workOrderId,
        int newStatusId,
        int updatedBy,
        CancellationToken cancellationToken = default)
    {
        var workOrder = await _context.WorkOrders
            .FirstOrDefaultAsync(w => w.WorkOrderId == workOrderId, cancellationToken);

        if (workOrder == null)
            return false;

        workOrder.StatusId = newStatusId;
        workOrder.UpdatedBy = updatedBy;
        workOrder.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <summary>
    /// Calculate progress percentage based on checklist completion
    /// PERFORMANCE: Single query with Include
    /// </summary>
    public async Task<decimal> CalculateProgressAsync(
        int workOrderId,
        CancellationToken cancellationToken = default)
    {
        var workOrder = await _context.WorkOrders
            .Include(w => w.ChecklistItems)
            .FirstOrDefaultAsync(w => w.WorkOrderId == workOrderId, cancellationToken);

        if (workOrder == null || !workOrder.ChecklistItems.Any())
            return 0;

        var totalItems = workOrder.ChecklistItems.Count;
        var completedItems = workOrder.ChecklistItems.Count(c => c.IsCompleted == true);

        var progress = (decimal)completedItems / totalItems * 100;

        // Update work order progress
        workOrder.ChecklistCompleted = completedItems;
        workOrder.ChecklistTotal = totalItems;
        workOrder.ProgressPercentage = progress;
        await _context.SaveChangesAsync(cancellationToken);

        return progress;
    }

    #region Helper Methods

    private IQueryable<WorkOrder> ApplySorting(
        IQueryable<WorkOrder> query,
        string sortBy,
        string sortDirection)
    {
        var isDescending = sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);

        return sortBy.ToLower() switch
        {
            "startdate" => isDescending
                ? query.OrderByDescending(w => w.StartDate)
                : query.OrderBy(w => w.StartDate),
            "estimatedcompletiondate" => isDescending
                ? query.OrderByDescending(w => w.EstimatedCompletionDate)
                : query.OrderBy(w => w.EstimatedCompletionDate),
            "priority" => isDescending
                ? query.OrderByDescending(w => w.Priority)
                : query.OrderBy(w => w.Priority),
            "status" => isDescending
                ? query.OrderByDescending(w => w.Status.StatusName)
                : query.OrderBy(w => w.Status.StatusName),
            "workordercode" => isDescending
                ? query.OrderByDescending(w => w.WorkOrderCode)
                : query.OrderBy(w => w.WorkOrderCode),
            _ => isDescending
                ? query.OrderByDescending(w => w.CreatedDate)
                : query.OrderBy(w => w.CreatedDate)
        };
    }

    private WorkOrderResponseDto MapToResponseDto(WorkOrder workOrder)
    {
        return new WorkOrderResponseDto
        {
            WorkOrderId = workOrder.WorkOrderId,
            WorkOrderCode = workOrder.WorkOrderCode,
            AppointmentId = workOrder.AppointmentId,
            CustomerId = workOrder.CustomerId,
            CustomerName = workOrder.Customer?.FullName ?? "",
            CustomerPhone = workOrder.Customer?.PhoneNumber,
            VehicleId = workOrder.VehicleId,
            VehiclePlate = workOrder.Vehicle?.LicensePlate ?? "",
            VehicleModel = workOrder.Vehicle?.Model?.ModelName ?? "",
            ServiceCenterId = workOrder.ServiceCenterId,
            ServiceCenterName = workOrder.ServiceCenter?.CenterName ?? "",
            StatusId = workOrder.StatusId,
            StatusName = workOrder.Status?.StatusName ?? "",
            StatusColor = workOrder.Status?.StatusColor,
            Priority = workOrder.Priority,
            SourceType = workOrder.SourceType, // ✅ NEW: Include source type
            StartDate = workOrder.StartDate,
            EstimatedCompletionDate = workOrder.EstimatedCompletionDate,
            CompletedDate = workOrder.CompletedDate,
            CreatedDate = workOrder.CreatedDate ?? DateTime.UtcNow,
            TechnicianId = workOrder.TechnicianId,
            TechnicianName = workOrder.Technician?.FullName,
            AdvisorId = workOrder.AdvisorId,
            AdvisorName = workOrder.Advisor?.FullName,
            SupervisorId = workOrder.SupervisorId,
            SupervisorName = workOrder.Supervisor?.FullName,
            EstimatedAmount = workOrder.EstimatedAmount,
            TotalAmount = workOrder.TotalAmount,
            DiscountAmount = workOrder.DiscountAmount,
            TaxAmount = workOrder.TaxAmount,
            FinalAmount = workOrder.FinalAmount,

            // Appointment-derived view-only fields
            AppointmentEstimatedCost = workOrder.Appointment?.EstimatedCost,
            AppointmentFinalCost = workOrder.Appointment?.FinalCost,
            AppointmentCode = workOrder.AppointmentCode ?? workOrder.Appointment?.AppointmentCode,
            // Compute outstanding amount deterministically from appointment values (final if present, else estimated) minus paid
            AppointmentOutstandingAmount = workOrder.Appointment != null
                ? Math.Max((workOrder.Appointment.FinalCost ?? workOrder.Appointment.EstimatedCost ?? 0m) - (workOrder.Appointment.PaidAmount ?? 0m), 0m)
                : 0m,
            HasOutstandingAppointmentPayment = workOrder.Appointment != null &&
                (Math.Max((workOrder.Appointment.FinalCost ?? workOrder.Appointment.EstimatedCost ?? 0m) - (workOrder.Appointment.PaidAmount ?? 0m), 0m) > 0m),

            ProgressPercentage = workOrder.ProgressPercentage,
            ChecklistCompleted = workOrder.ChecklistCompleted,
            ChecklistTotal = workOrder.ChecklistTotal,
            RequiresApproval = workOrder.RequiresApproval,
            ApprovalRequired = workOrder.ApprovalRequired,
            ApprovedBy = workOrder.ApprovedBy,
            ApprovedByName = workOrder.ApprovedByNavigation?.FullName,
            ApprovedDate = workOrder.ApprovedDate,
            ApprovalNotes = workOrder.ApprovalNotes,
            QualityCheckRequired = workOrder.QualityCheckRequired,
            QualityCheckedBy = workOrder.QualityCheckedBy,
            QualityCheckedByName = workOrder.QualityCheckedByNavigation?.FullName,
            QualityCheckDate = workOrder.QualityCheckDate,
            QualityRating = workOrder.QualityRating,
            CustomerNotes = workOrder.CustomerNotes,
            InternalNotes = workOrder.InternalNotes,
            TechnicianNotes = workOrder.TechnicianNotes,
            Services = workOrder.WorkOrderServices?.Select(ws => new WorkOrderServiceItemDto
            {
                ServiceId = ws.ServiceId,
                ServiceName = ws.Service?.ServiceName ?? "",
                ServiceDescription = ws.Service?.Description,
                Price = ws.UnitPrice ?? 0,
                Quantity = ws.Quantity ?? 0,
                TotalPrice = ws.TotalPrice ?? 0,
                Status = ws.Status,
                Notes = ws.Notes
            }).ToList() ?? new List<WorkOrderServiceItemDto>(),
            Parts = workOrder.WorkOrderParts?.Select(wp => new WorkOrderPartItemDto
            {
                PartId = wp.PartId,
                PartName = wp.Part?.PartName ?? "",
                PartNumber = wp.Part?.PartCode,
                UnitPrice = wp.UnitPrice ?? 0,
                QuantityUsed = wp.Quantity,
                TotalCost = wp.TotalCost ?? 0,
                Status = wp.Status,
                Notes = wp.Notes
            }).ToList() ?? new List<WorkOrderPartItemDto>()
        };
    }

    #endregion
}
