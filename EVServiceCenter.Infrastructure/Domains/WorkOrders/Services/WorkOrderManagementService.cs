using EVServiceCenter.Core.Domains.AppointmentManagement.Interfaces.Services;
using EVServiceCenter.Core.Domains.Invoices.DTOs.Requests;
using EVServiceCenter.Core.Domains.Invoices.Interfaces;
using EVServiceCenter.Core.Domains.Shared.Models;
using EVServiceCenter.Core.Domains.WorkOrders.DTOs.Requests;
using EVServiceCenter.Core.Domains.WorkOrders.DTOs.Responses;
using EVServiceCenter.Core.Domains.WorkOrders.Interfaces;
using EVServiceCenter.Core.Entities;
using EVServiceCenter.Core.Enums;
using EVServiceCenter.Infrastructure.Domains.WorkOrders.Repositories;
using EVServiceCenter.Core.Domains.TechnicianManagement.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EVServiceCenter.Infrastructure.Domains.WorkOrders.Services;

/// <summary>
/// Service for WorkOrder management operations
/// Handles business logic for work order lifecycle
/// </summary>
public class WorkOrderManagementService : IWorkOrderService
{
    private readonly EVDbContext _context;
    private readonly WorkOrderQueryRepository _queryRepo;
    private readonly IWorkOrderRepository _repository;
    private readonly IWorkOrderTimelineService _timelineService;
    private readonly IAppointmentCommandService _appointmentCommandService;
    private readonly IInvoiceService _invoiceService;
    private readonly IShiftService _shiftService;
    private readonly ILogger<WorkOrderManagementService> _logger;

    public WorkOrderManagementService(
        EVDbContext context,
        WorkOrderQueryRepository queryRepo,
        IWorkOrderRepository repository,
        IWorkOrderTimelineService timelineService,
        IInvoiceService invoiceService,
         IAppointmentCommandService appointmentCommandService,
        IShiftService shiftService,
        ILogger<WorkOrderManagementService> logger)
    {
        _context = context;
        _queryRepo = queryRepo;
        _repository = repository;
        _timelineService = timelineService;
        _invoiceService = invoiceService;
        _appointmentCommandService = appointmentCommandService;
        _shiftService = shiftService;
        _logger = logger;
    }

    #region Create Operations

    public async Task<WorkOrderResponseDto> CreateWorkOrderAsync(
        CreateWorkOrderRequestDto request,
        int createdBy,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating work order for Customer {CustomerId}, Vehicle {VehicleId}",
            request.CustomerId, request.VehicleId);

        try
        {
            // 1. Generate unique work order code
            var workOrderCode = await _queryRepo.GenerateWorkOrderCodeAsync(cancellationToken);

            // 2. Determine initial status (1 = Pending)
            var initialStatusId = 1;

            // 3. Create work order entity
            var workOrder = new WorkOrder
            {
                WorkOrderCode = workOrderCode,
                AppointmentId = request.AppointmentId,
                CustomerId = request.CustomerId,
                VehicleId = request.VehicleId,
                ServiceCenterId = request.ServiceCenterId,
                TechnicianId = request.TechnicianId,
                AdvisorId = request.AdvisorId,
                StatusId = initialStatusId,
                Priority = request.Priority ?? "Normal",
                SourceType = request.AppointmentId.HasValue ? "Scheduled" : "WalkIn", // ✅ NEW: Auto-detect source
                EstimatedCompletionDate = request.EstimatedCompletionDate,
                CustomerNotes = request.CustomerNotes,
                InternalNotes = request.InternalNotes,
                RequiresApproval = request.RequiresApproval,
                QualityCheckRequired = request.QualityCheckRequired,
                CreatedBy = createdBy,
                CreatedDate = DateTime.UtcNow
            };

            _context.WorkOrders.Add(workOrder);
            await _context.SaveChangesAsync(cancellationToken);

            // 4. Add services if provided
            if (request.ServiceIds != null && request.ServiceIds.Any())
            {
                await AddServicesToWorkOrderAsync(workOrder.WorkOrderId, request.ServiceIds, cancellationToken);
            }

            // 5. Create initial timeline event
            await _timelineService.AddTimelineEventAsync(
                workOrder.WorkOrderId,
                new AddWorkOrderTimelineRequestDto
                {
                    EventType = "Created",
                    EventDescription = $"Work order {workOrderCode} created",
                    IsVisible = true
                },
                createdBy,
                cancellationToken);

            _logger.LogInformation("Work order {WorkOrderCode} created successfully", workOrderCode);

            // 6. Return full details
            var result = await _queryRepo.GetWorkOrderDetailAsync(workOrder.WorkOrderId, cancellationToken);
            return result ?? throw new InvalidOperationException("Failed to retrieve created work order");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating work order for Customer {CustomerId}", request.CustomerId);
            throw;
        }
    }

    /// <summary>
    /// ❌ OBSOLETE: Create work order from appointment
    ///
    /// This method is marked as obsolete and should NOT be used.
    ///
    /// **Replacement:**
    /// Use AppointmentCommandService.CheckInAsync() instead.
    /// Check-in automatically creates WorkOrder with proper duplicate prevention.
    ///
    /// **Why obsolete:**
    /// - No duplicate check (can create multiple WorkOrders for same appointment)
    /// - No status validation (can create WorkOrder from cancelled appointment)
    /// - Bypasses check-in business logic
    /// - Creates confusion: "Which API should I use?"
    ///
    /// **This method will be removed in future version.**
    /// </summary>
    [Obsolete("Use AppointmentCommandService.CheckInAsync() instead. This method will be removed in future version.")]
    public Task<WorkOrderResponseDto> CreateWorkOrderFromAppointmentAsync(
        int appointmentId,
        int createdBy,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException(
            "Creating WorkOrder from appointment via this method is no longer supported. " +
            "Use POST /api/appointments/{id}/check-in instead. " +
            "Check-in automatically creates WorkOrder with SourceType='Scheduled'.");
    }

    #endregion

    #region Read Operations

    public async Task<WorkOrderResponseDto> GetWorkOrderAsync(
        int workOrderId,
        CancellationToken cancellationToken = default)
    {
        var result = await _queryRepo.GetWorkOrderDetailAsync(workOrderId, cancellationToken);
        if (result == null)
            throw new KeyNotFoundException($"Work order {workOrderId} not found");

        return result;
    }

    public async Task<WorkOrderResponseDto> GetWorkOrderByCodeAsync(
        string workOrderCode,
        CancellationToken cancellationToken = default)
    {
        var workOrder = await _queryRepo.GetByCodeAsync(workOrderCode, cancellationToken);
        if (workOrder == null)
            throw new KeyNotFoundException($"Work order {workOrderCode} not found");

        return await _queryRepo.GetWorkOrderDetailAsync(workOrder.WorkOrderId, cancellationToken)
            ?? throw new InvalidOperationException("Failed to load work order details");
    }

    public async Task<PagedResult<WorkOrderSummaryDto>> GetWorkOrdersAsync(
        WorkOrderQueryDto query,
        CancellationToken cancellationToken = default)
    {
        return await _queryRepo.GetWorkOrdersAsync(query, cancellationToken);
    }

    #endregion

    #region Update Operations

    public async Task<WorkOrderResponseDto> UpdateWorkOrderAsync(
        int workOrderId,
        UpdateWorkOrderRequestDto request,
        int updatedBy,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating work order {WorkOrderId}", workOrderId);

        var workOrder = await _context.WorkOrders
            .FirstOrDefaultAsync(w => w.WorkOrderId == workOrderId, cancellationToken);

        if (workOrder == null)
            throw new KeyNotFoundException($"Work order {workOrderId} not found");

        // Update fields
        if (request.TechnicianId.HasValue)
            workOrder.TechnicianId = request.TechnicianId.Value;

        if (request.AdvisorId.HasValue)
            workOrder.AdvisorId = request.AdvisorId.Value;

        if (request.SupervisorId.HasValue)
            workOrder.SupervisorId = request.SupervisorId.Value;

        if (!string.IsNullOrWhiteSpace(request.Priority))
            workOrder.Priority = request.Priority;

        if (request.EstimatedCompletionDate.HasValue)
            workOrder.EstimatedCompletionDate = request.EstimatedCompletionDate.Value;

        if (request.InternalNotes != null)
            workOrder.InternalNotes = request.InternalNotes;

        if (request.TechnicianNotes != null)
            workOrder.TechnicianNotes = request.TechnicianNotes;

        workOrder.UpdatedBy = updatedBy;
        workOrder.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Work order {WorkOrderId} updated successfully", workOrderId);

        return await GetWorkOrderAsync(workOrderId, cancellationToken);
    }

    public async Task<WorkOrderResponseDto> UpdateStatusAsync(
        int workOrderId,
        UpdateWorkOrderStatusRequestDto request,
        int updatedBy,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating status of work order {WorkOrderId} to Status {StatusId}",
            workOrderId, request.StatusId);

        var workOrder = await _context.WorkOrders
            .Include(w => w.Status)
            .FirstOrDefaultAsync(w => w.WorkOrderId == workOrderId, cancellationToken);

        if (workOrder == null)
            throw new KeyNotFoundException($"Work order {workOrderId} not found");

        var oldStatusId = workOrder.StatusId;
        var oldStatusName = workOrder.Status?.StatusName ?? "Unknown";
        var newStatus = await _context.WorkOrderStatuses
            .FirstOrDefaultAsync(s => s.StatusId == request.StatusId, cancellationToken);

        if (newStatus == null)
            throw new InvalidOperationException($"Status {request.StatusId} not found");

        // 🔧 FIX GAP #5: Validate status transition
        if (!IsValidStatusTransition(oldStatusId, request.StatusId))
        {
            var allowedTransitions = GetAllowedTransitionsForStatus(oldStatusId);
            var allowedStatusNames = string.Join(", ", allowedTransitions.Select(id =>
            {
                var statusName = GetWorkOrderStatusName(id);
                return $"{statusName} ({id})";
            }));

            throw new InvalidOperationException(
                $"Invalid status transition for WorkOrder {workOrderId}: " +
                $"Cannot change from '{oldStatusName}' (ID={oldStatusId}) to '{newStatus.StatusName}' (ID={request.StatusId}). " +
                $"Allowed transitions from '{oldStatusName}': {allowedStatusNames}");
        }

        _logger.LogInformation(
            "Valid status transition: {OldStatus} ({OldId}) → {NewStatus} ({NewId})",
            oldStatusName, oldStatusId, newStatus.StatusName, request.StatusId);

        // Update status
        workOrder.StatusId = request.StatusId;
        workOrder.UpdatedBy = updatedBy;
        workOrder.UpdatedDate = DateTime.UtcNow;

        // If completing, set completed date and auto-generate invoice
        if (newStatus.StatusName.Equals("Completed", StringComparison.OrdinalIgnoreCase))
        {
            workOrder.CompletedDate = DateTime.UtcNow;

            // ✅ GAP 1 FIX: Auto-generate invoice when WorkOrder is completed
            try
            {
                // Check if invoice already exists for this work order
                var existingInvoice = await _invoiceService.GetInvoiceByWorkOrderIdAsync(
                    workOrderId, cancellationToken);

                if (existingInvoice == null)
                {
                    _logger.LogInformation(
                        "Auto-generating invoice for completed WorkOrder {WorkOrderId}",
                        workOrderId);

                    var generateInvoiceRequest = new GenerateInvoiceRequestDto
                    {
                        WorkOrderId = workOrderId,
                        PaymentTerms = "Due on Receipt",
                        Notes = $"Auto-generated invoice for completed work order {workOrder.WorkOrderCode}"
                    };

                    var invoice = await _invoiceService.GenerateInvoiceFromWorkOrderAsync(
                        generateInvoiceRequest,
                        updatedBy,
                        cancellationToken);

                    _logger.LogInformation(
                        "Successfully auto-generated Invoice {InvoiceCode} for WorkOrder {WorkOrderId}",
                        invoice.InvoiceCode, workOrderId);
                }
                else
                {
                    _logger.LogInformation(
                        "Invoice {InvoiceCode} already exists for WorkOrder {WorkOrderId}, skipping auto-generation",
                        existingInvoice.InvoiceCode, workOrderId);
                }
            }
            catch (Exception ex)
            {
                // Log error but don't fail the status update
                // Invoice can still be generated manually if auto-generation fails
                _logger.LogError(ex,
                    "Failed to auto-generate invoice for WorkOrder {WorkOrderId}. Invoice must be created manually.",
                    workOrderId);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Create timeline event
        await _timelineService.AddStatusChangeEventAsync(
            workOrderId,
            oldStatusName,
            newStatus.StatusName,
            request.Notes,
            updatedBy,
            cancellationToken);

        _logger.LogInformation("Work order {WorkOrderId} status updated to {StatusName}",
            workOrderId, newStatus.StatusName);

        return await GetWorkOrderAsync(workOrderId, cancellationToken);
    }

    #endregion

    // ... TO BE CONTINUED IN PART 2
    // (AssignTechnician, StartWork, CompleteWork, etc.)

    #region Helper Methods

    private async Task AddServicesToWorkOrderAsync(
        int workOrderId,
        List<int> serviceIds,
        CancellationToken cancellationToken)
    {
        foreach (var serviceId in serviceIds)
        {
            var service = await _context.MaintenanceServices
                .FindAsync(new object[] { serviceId }, cancellationToken);

            if (service != null)
            {
                // ✅ FIX: Use TotalCost (BasePrice + LaborCost) instead of BasePrice
                var unitPrice = service.BasePrice + (service.LaborCost ?? 0m);
                
                var workOrderService = new Core.Entities.WorkOrderService
                {
                    WorkOrderId = workOrderId,
                    ServiceId = serviceId,
                    UnitPrice = unitPrice,
                    Quantity = 1,
                    TotalPrice = unitPrice,
                    Status = "Pending"
                };
                _context.WorkOrderServices.Add(workOrderService);

                _logger.LogInformation(
                    "Added service {ServiceId} to WorkOrder {WorkOrderId}: BasePrice={BasePrice}, LaborCost={LaborCost}, UnitPrice={UnitPrice}",
                    serviceId, workOrderId, service.BasePrice, service.LaborCost, unitPrice);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// 🔧 FIX GAP #5: Valid WorkOrder status transition matrix
    /// WorkOrderStatusEnum:
    /// 1 = Created, 2 = Assigned, 3 = InProgress, 4 = AwaitingParts,
    /// 5 = QualityCheck, 6 = Completed, 7 = Cancelled
    /// </summary>
    private static readonly Dictionary<int, List<int>> ValidStatusTransitions = new()
    {
        // Created (1) → Can assign to technician, start work, or cancel
        { 1, new List<int> { 2, 3, 7 } },

        // Assigned (2) → Can start work or cancel
        { 2, new List<int> { 3, 7 } },

        // InProgress (3) → Can wait for parts, go to quality check, complete, or cancel
        { 3, new List<int> { 4, 5, 6, 7 } },

        // AwaitingParts (4) → Can resume work or cancel
        { 4, new List<int> { 3, 7 } },

        // QualityCheck (5) → Can return to work (if issues found) or complete
        { 5, new List<int> { 3, 6 } },

        // Completed (6) → Terminal state, no transitions allowed
        { 6, new List<int>() },

        // Cancelled (7) → Terminal state, no transitions allowed
        { 7, new List<int>() }
    };

    /// <summary>
    /// Status name mapping for better error messages
    /// </summary>
    private static readonly Dictionary<int, string> StatusNames = new()
    {
        { 1, "Created" },
        { 2, "Assigned" },
        { 3, "InProgress" },
        { 4, "AwaitingParts" },
        { 5, "QualityCheck" },
        { 6, "Completed" },
        { 7, "Cancelled" }
    };

    /// <summary>
    /// Check if status transition is valid
    /// </summary>
    private bool IsValidStatusTransition(int fromStatusId, int toStatusId)
    {
        // Allow staying in same status (idempotent)
        if (fromStatusId == toStatusId)
            return true;

        // Check if transition is allowed
        if (ValidStatusTransitions.TryGetValue(fromStatusId, out var allowedTransitions))
        {
            return allowedTransitions.Contains(toStatusId);
        }

        // Unknown status - allow for backward compatibility
        _logger.LogWarning(
            "Unknown WorkOrder status {StatusId}, allowing transition by default",
            fromStatusId);
        return true;
    }

    /// <summary>
    /// Get list of allowed status transitions from current status
    /// </summary>
    private List<int> GetAllowedTransitionsForStatus(int statusId)
    {
        return ValidStatusTransitions.TryGetValue(statusId, out var transitions)
            ? transitions
            : new List<int>();
    }

    /// <summary>
    /// Get status name for display
    /// </summary>
    private string GetWorkOrderStatusName(int statusId)
    {
        return StatusNames.TryGetValue(statusId, out var name)
            ? name
            : $"Unknown({statusId})";
    }

    #endregion

    // Throw NotImplementedException for methods not yet implemented
    public async Task<WorkOrderResponseDto> AssignTechnicianAsync(
        int workOrderId,
        int technicianId,
        int assignedBy,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Assigning technician {TechnicianId} to WorkOrder {WorkOrderId}",
            technicianId, workOrderId);

        var workOrder = await _context.WorkOrders
            .Include(wo => wo.Status)
            .FirstOrDefaultAsync(wo => wo.WorkOrderId == workOrderId, cancellationToken);

        if (workOrder == null)
            throw new KeyNotFoundException($"Work order {workOrderId} not found");

        if (workOrder.StatusId == (int)WorkOrderStatusEnum.Completed ||
            workOrder.StatusId == (int)WorkOrderStatusEnum.Cancelled)
        {
            throw new InvalidOperationException(
                $"Cannot assign technician when work order is {GetWorkOrderStatusName(workOrder.StatusId)}");
        }

        if (workOrder.TechnicianId == technicianId)
        {
            _logger.LogInformation(
                "WorkOrder {WorkOrderId} already assigned to technician {TechnicianId}",
                workOrderId, technicianId);
            return await GetWorkOrderAsync(workOrderId, cancellationToken);
        }

        var technician = await _context.Users
            .FirstOrDefaultAsync(u => u.UserId == technicianId, cancellationToken);

        if (technician == null)
            throw new InvalidOperationException($"Technician {technicianId} not found");

        if (technician.RoleId != (int)UserRoles.Technician)
            throw new InvalidOperationException("Selected user is not a technician");

        if (technician.IsActive.HasValue && technician.IsActive.Value == false)
            throw new InvalidOperationException($"Technician {technician.FullName} is not active");

        var isAvailable = await _repository.IsTechnicianAvailableAsync(
            technicianId,
            cancellationToken: cancellationToken);

        if (!isAvailable)
        {
            throw new InvalidOperationException(
                $"Technician {technician.FullName} is currently at maximum workload");
        }

        var previousStatusId = workOrder.StatusId;
        var previousStatusName = GetWorkOrderStatusName(previousStatusId);

        workOrder.TechnicianId = technicianId;
        workOrder.UpdatedBy = assignedBy;
        workOrder.UpdatedDate = DateTime.UtcNow;

        if (workOrder.StatusId == (int)WorkOrderStatusEnum.Created)
        {
            workOrder.StatusId = (int)WorkOrderStatusEnum.Assigned;
        }

        await _context.SaveChangesAsync(cancellationToken);

        await _timelineService.AddTechnicianAssignmentEventAsync(
            workOrderId,
            technician.FullName,
            assignedBy,
            cancellationToken);

        if (previousStatusId != workOrder.StatusId)
        {
            await _timelineService.AddStatusChangeEventAsync(
                workOrderId,
                previousStatusName,
                GetWorkOrderStatusName(workOrder.StatusId),
                null,
                assignedBy,
                cancellationToken);
        }

        return await GetWorkOrderAsync(workOrderId, cancellationToken);
    }

    public async Task<WorkOrderResponseDto> StartWorkAsync(
        int workOrderId,
        int startedBy,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting work on WorkOrder {WorkOrderId}", workOrderId);

        var workOrder = await _context.WorkOrders
            .Include(wo => wo.Status)
            .FirstOrDefaultAsync(wo => wo.WorkOrderId == workOrderId, cancellationToken);

        if (workOrder == null)
            throw new KeyNotFoundException($"Work order {workOrderId} not found");

        if (workOrder.StatusId == (int)WorkOrderStatusEnum.Completed ||
            workOrder.StatusId == (int)WorkOrderStatusEnum.Cancelled)
        {
            throw new InvalidOperationException(
                $"Cannot start work when work order is {GetWorkOrderStatusName(workOrder.StatusId)}");
        }

        if (!workOrder.TechnicianId.HasValue)
            throw new InvalidOperationException("Cannot start work without an assigned technician");

        // ✅ SPRINT 1 DAY 3: On-Shift Validation
        var isOnShift = await _shiftService.IsOnShiftAsync(
            workOrder.TechnicianId.Value,
            DateTime.UtcNow,
            cancellationToken);

        if (!isOnShift)
        {
            _logger.LogWarning(
                "Technician {TechnicianId} attempted to start WorkOrder {WorkOrderId} without being on-shift",
                workOrder.TechnicianId.Value, workOrderId);

            throw new InvalidOperationException(
                "Technician must check-in for shift before starting work. " +
                "Please use POST /api/technicians/attendance/check-in to check-in first.");
        }

        _logger.LogInformation(
            "Technician {TechnicianId} on-shift validation passed for WorkOrder {WorkOrderId}",
            workOrder.TechnicianId.Value, workOrderId);

        var targetStatus = (int)WorkOrderStatusEnum.InProgress;
        if (!IsValidStatusTransition(workOrder.StatusId, targetStatus))
        {
            throw new InvalidOperationException(
                $"Invalid transition from {GetWorkOrderStatusName(workOrder.StatusId)} to {GetWorkOrderStatusName(targetStatus)}");
        }

        var previousStatusName = GetWorkOrderStatusName(workOrder.StatusId);
        workOrder.StatusId = targetStatus;
        workOrder.StartDate ??= DateTime.UtcNow;
        workOrder.UpdatedBy = startedBy;
        workOrder.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        await _timelineService.AddStatusChangeEventAsync(
            workOrderId,
            previousStatusName,
            GetWorkOrderStatusName(workOrder.StatusId),
            null,
            startedBy,
            cancellationToken);

        await _timelineService.AddTimelineEventAsync(
            workOrderId,
            new AddWorkOrderTimelineRequestDto
            {
                EventType = "WorkStarted",
                EventDescription = "Technician started working on the vehicle",
                IsVisible = true
            },
            startedBy,
            cancellationToken);

        return await GetWorkOrderAsync(workOrderId, cancellationToken);
    }

    /// <summary>
    /// Complete work order with checklist validation and auto invoice generation
    /// ✅ IMPLEMENTED: Complete flow including invoice creation
    /// </summary>
    public async Task<WorkOrderResponseDto> CompleteWorkOrderAsync(
    int workOrderId,
    int completedBy,
    CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Completing WorkOrder {WorkOrderId} by user {UserId}",
            workOrderId, completedBy);

        var executionStrategy = _context.Database.CreateExecutionStrategy();

        return await executionStrategy.ExecuteAsync(async () =>
        {
            var workOrder = await _context.WorkOrders
                .Include(wo => wo.WorkOrderParts).ThenInclude(p => p.Part)
                .Include(wo => wo.WorkOrderServices).ThenInclude(s => s.Service)
                .Include(wo => wo.Customer)
                .Include(wo => wo.Vehicle)
                    .ThenInclude(v => v.Model)
                .FirstOrDefaultAsync(wo => wo.WorkOrderId == workOrderId, cancellationToken);

            if (workOrder == null)
                throw new InvalidOperationException($"Work order {workOrderId} not found");

            if (workOrder.StatusId == (int)WorkOrderStatusEnum.Completed)
            {
                _logger.LogWarning("WorkOrder {WorkOrderId} is already completed", workOrderId);
                throw new InvalidOperationException($"Work order {workOrder.WorkOrderCode} is already completed");
            }

            // ✅ Lưu lại AppointmentId để dùng sau khi commit transaction
            int? linkedAppointmentId = workOrder.AppointmentId;

            // 1) Validate checklist
            var checklistItems = await _context.Set<ChecklistItem>()
                .Where(ci => ci.WorkOrderId == workOrderId)
                .ToListAsync(cancellationToken);

            if (checklistItems.Any())
            {
                var totalItems = checklistItems.Count;
                var completedItems = checklistItems.Count(ci => ci.IsCompleted == true);
                var requiredIncomplete = checklistItems
                    .Where(ci => ci.IsRequired == true && ci.IsCompleted != true)
                    .ToList();

                if (requiredIncomplete.Any())
                {
                    var incompleteNames = string.Join(", ", requiredIncomplete.Select(ci => ci.ItemDescription));
                    _logger.LogWarning(
                        "WorkOrder {WorkOrderId} has {Count} incomplete required checklist items: {Items}",
                        workOrderId, requiredIncomplete.Count, incompleteNames);

                    throw new InvalidOperationException(
                        $"Cannot complete work order. {requiredIncomplete.Count} required checklist item(s) not completed: {incompleteNames}");
                }

                _logger.LogInformation(
                    "Checklist validation passed: {Completed}/{Total} items completed",
                    completedItems, totalItems);
            }

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                // 2) Tính tiền
                var servicesTotal = workOrder.WorkOrderServices?.Sum(s => s.TotalPrice ?? 0) ?? 0;
                var partsTotal = workOrder.WorkOrderParts?.Sum(p => p.TotalPrice ?? 0) ?? 0;
                var subTotal = servicesTotal + partsTotal;
                var taxRate = 0.08m;
                var taxAmount = subTotal * taxRate;
                var grandTotal = subTotal + taxAmount;

                _logger.LogInformation(
                    "Calculated totals: Services={Services}, Parts={Parts}, SubTotal={SubTotal}, Tax={Tax}, GrandTotal={GrandTotal}",
                    servicesTotal, partsTotal, subTotal, taxAmount, grandTotal);

                // ✅ FIX: Update WorkOrderServices status to "Completed"
                if (workOrder.WorkOrderServices != null && workOrder.WorkOrderServices.Any())
                {
                    foreach (var service in workOrder.WorkOrderServices)
                    {
                        service.Status = "Completed";
                    }

                    _logger.LogInformation(
                        "Updated {Count} WorkOrderServices to 'Completed' status",
                        workOrder.WorkOrderServices.Count);
                }

                // ✅ FIX: Update WorkOrderParts status to "Installed" (if any)
                if (workOrder.WorkOrderParts != null && workOrder.WorkOrderParts.Any())
                {
                    foreach (var part in workOrder.WorkOrderParts)
                    {
                        if (part.Status != "Installed")
                        {
                            part.Status = "Installed";
                            part.InstalledDate = DateTime.UtcNow;
                            part.InstalledBy = completedBy;
                        }
                    }

                    _logger.LogInformation(
                        "Updated {Count} WorkOrderParts to 'Installed' status",
                        workOrder.WorkOrderParts.Count);
                }

                // 3) Cập nhật WorkOrder
                workOrder.StatusId = (int)WorkOrderStatusEnum.Completed;
                workOrder.CompletedDate = DateTime.UtcNow;
                workOrder.TotalAmount = subTotal;
                workOrder.TaxAmount = taxAmount;
                workOrder.FinalAmount = grandTotal;
                workOrder.ProgressPercentage = 100;

                if (checklistItems.Any())
                {
                    workOrder.ChecklistCompleted = checklistItems.Count(ci => ci.IsCompleted == true);
                    workOrder.ChecklistTotal = checklistItems.Count;
                }

                // 4) Đảm bảo Invoice
                var invoice = await EnsureInvoiceAsync(
                    workOrder,
                    servicesTotal,
                    partsTotal,
                    subTotal,
                    taxAmount,
                    grandTotal,
                    completedBy,
                    cancellationToken);

                // 5) Đảm bảo MaintenanceHistory
                var serviceDate = DateOnly.FromDateTime(workOrder.CompletedDate ?? DateTime.UtcNow);

                var invoiceServiceTotal = invoice.ServiceTotal ?? invoice.ServiceSubTotal ?? servicesTotal;
                var invoicePartsTotal = invoice.PartsTotal ?? invoice.PartsSubTotal ?? partsTotal;
                var invoiceGrandTotal = invoice.GrandTotal ?? grandTotal;

                _ = await EnsureMaintenanceHistoryAsync(
                    workOrder,
                    serviceDate,
                    invoiceServiceTotal,
                    invoicePartsTotal,
                    invoiceGrandTotal,
                    completedBy,
                    cancellationToken);

                // ⚠️ KHÔNG tự update Appointment ở đây nữa – để CompleteAppointmentAsync xử lý

                // 6) Timeline event
                await _timelineService.AddTimelineEventAsync(
                    workOrderId,
                    new AddWorkOrderTimelineRequestDto
                    {
                        EventType = "Completed",
                        EventDescription = $"Work order completed. Invoice {invoice.InvoiceCode}. Total amount: {grandTotal:C}",
                        IsVisible = true
                    },
                    completedBy,
                    cancellationToken);

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation(
                    "Successfully completed WorkOrder {WorkOrderCode}: Invoice={InvoiceId}, Amount={Amount}",
                    workOrder.WorkOrderCode, invoice.InvoiceId, grandTotal);

                // 7) Sau khi WorkOrder hoàn tất & commit => gọi CompleteAppointmentAsync
                if (linkedAppointmentId.HasValue)
                {
                    var appointmentId = linkedAppointmentId.Value;

                    try
                    {
                        _logger.LogInformation(
                            "Triggering CompleteAppointmentAsync from CompleteWorkOrderAsync. " +
                            "WorkOrderId={WorkOrderId}, AppointmentId={AppointmentId}",
                            workOrderId, appointmentId);

                        var completed = await _appointmentCommandService.CompleteAppointmentAsync(
                            appointmentId,
                            completedBy,
                            cancellationToken);

                        _logger.LogInformation(
                            "CompleteAppointmentAsync finished (Result={Result}) for Appointment {AppointmentId} " +
                            "triggered by WorkOrder {WorkOrderId}",
                            completed, appointmentId, workOrderId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "Error while completing Appointment {AppointmentId} from WorkOrder {WorkOrderId}",
                            appointmentId, workOrderId);
                        // tuỳ business: nếu muốn WO complete phải chắc chắn deduct usage thì throw; 
                        // nếu chấp nhận xử lý tay thì có thể chỉ log warning.
                        throw;
                    }
                }

                // 8) Trả về WorkOrder đã cập nhật đầy đủ
                return await GetWorkOrderAsync(workOrderId, cancellationToken);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex,
                    "Failed to complete WorkOrder {WorkOrderId}",
                    workOrderId);
                throw;
            }
        });
    }


    private async Task<MaintenanceHistory> EnsureMaintenanceHistoryAsync(
        WorkOrder workOrder,
        DateOnly serviceDate,
        decimal servicesTotal,
        decimal partsTotal,
        decimal grandTotal,
        int completedBy,
        CancellationToken cancellationToken)
    {
        var history = await _context.MaintenanceHistories
            .FirstOrDefaultAsync(h => h.WorkOrderId == workOrder.WorkOrderId, cancellationToken);

        var vehicle = workOrder.Vehicle ?? await _context.CustomerVehicles
            .Include(v => v.Model)
            .FirstOrDefaultAsync(v => v.VehicleId == workOrder.VehicleId, cancellationToken)
            ?? throw new InvalidOperationException($"Vehicle {workOrder.VehicleId} not found for work order {workOrder.WorkOrderCode}");

        var vehicleMileage = vehicle.Mileage ?? vehicle.LastMaintenanceMileage;

        var serviceNames = workOrder.WorkOrderServices?
            .Where(s => s.Service != null)
            .Select(s =>
            {
                var label = s.Service!.ServiceName;
                var quantity = s.Quantity.GetValueOrDefault(1);
                return quantity > 1 ? $"{label} x{quantity}" : label;
            })
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .ToList() ?? new List<string>();

        var partsNames = workOrder.WorkOrderParts?
            .Where(p => p.Part != null)
            .Select(p =>
            {
                var label = p.Part!.PartName;
                return p.Quantity > 1 ? $"{label} x{p.Quantity}" : label;
            })
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .ToList() ?? new List<string>();

        var servicesPerformed = serviceNames.Count > 0
            ? string.Join(", ", serviceNames)
            : "Bảo dưỡng tổng hợp";

        var partsReplaced = partsNames.Count > 0
            ? string.Join(", ", partsNames)
            : null;

        if (history == null)
        {
            history = new MaintenanceHistory
            {
                VehicleId = vehicle.VehicleId,
                WorkOrderId = workOrder.WorkOrderId,
                CreatedDate = DateTime.UtcNow
            };

            await _context.MaintenanceHistories.AddAsync(history, cancellationToken);

            _logger.LogInformation(
                "Creating maintenance history entry for WorkOrder {WorkOrderId} (Vehicle {VehicleId})",
                workOrder.WorkOrderId,
                vehicle.VehicleId);
        }
        else
        {
            _logger.LogInformation(
                "Updating maintenance history entry #{HistoryId} for WorkOrder {WorkOrderId}",
                history.HistoryId,
                workOrder.WorkOrderId);
        }

        history.ServiceDate = serviceDate;
        history.Mileage = vehicleMileage;
        history.ServicesPerformed = servicesPerformed;
        history.PartsReplaced = partsReplaced;
        history.TotalServiceCost = servicesTotal;
        history.TotalPartsCost = partsTotal;
        history.TotalCost = grandTotal;
        history.TechnicianNotes = workOrder.TechnicianNotes;
        history.CustomerNotes = workOrder.CustomerNotes;

        var serviceIntervalKm = vehicle.Model?.ServiceInterval;
        var serviceIntervalMonths = vehicle.Model?.ServiceIntervalMonths;

        if (serviceIntervalKm.HasValue && serviceIntervalKm.Value > 0)
        {
            var baselineMileage = history.Mileage ?? 0;
            history.NextServiceMileage = baselineMileage + serviceIntervalKm.Value;
        }

        if (serviceIntervalMonths.HasValue && serviceIntervalMonths.Value > 0)
        {
            history.NextServiceDue = serviceDate.AddMonths(serviceIntervalMonths.Value);
        }

        vehicle.LastMaintenanceDate = serviceDate;
        vehicle.LastMaintenanceMileage = history.Mileage;
        vehicle.NextMaintenanceMileage = history.NextServiceMileage;
        vehicle.NextMaintenanceDate = history.NextServiceDue;
        vehicle.UpdatedDate = DateTime.UtcNow;
        vehicle.UpdatedBy = completedBy;

        return history;
    }

    private async Task<Invoice> EnsureInvoiceAsync(
     WorkOrder workOrder,
     decimal servicesTotal,
     decimal partsTotal,
     decimal subTotal,
     decimal taxAmount,
     decimal grandTotal,
     int completedBy,
     CancellationToken cancellationToken)
    {
        // Nếu đã có invoice cho work order này thì trả về luôn
        var existingInvoice = await _context.Invoices
            .FirstOrDefaultAsync(i => i.WorkOrderId == workOrder.WorkOrderId, cancellationToken);

        if (existingInvoice != null)
        {
            _logger.LogWarning(
                "Invoice {InvoiceCode} already exists for WorkOrder {WorkOrderId}",
                existingInvoice.InvoiceCode, workOrder.WorkOrderId);
            return existingInvoice;
        }

        var invoiceCode = await GenerateInvoiceCodeAsync(cancellationToken);

        decimal paidAmount = 0m;
        string invoiceStatus;

        // ✅ Case 1: WorkOrder miễn phí (mọi thứ được cover từ gói → grandTotal = 0)
        if (grandTotal == 0m)
        {
            invoiceStatus = "Paid";
            paidAmount = 0m;

            _logger.LogInformation(
                "WorkOrder {WorkOrderId} grand total is 0. Creating invoice marked as Paid (no outstanding).",
                workOrder.WorkOrderId);
        }
        else
        {
            // ✅ Case 2: Có phát sinh tiền → xem Appointment đã pre-pay chưa
            invoiceStatus = "Pending";

            if (workOrder.AppointmentId.HasValue)
            {
                var appointment = await _context.Appointments
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.AppointmentId == workOrder.AppointmentId.Value, cancellationToken);

                if (appointment != null &&
                    string.Equals(appointment.PaymentStatus,
                        PaymentStatusEnum.Completed.ToString(),
                        StringComparison.OrdinalIgnoreCase))
                {
                    paidAmount = appointment.PaidAmount ?? 0m;

                    if (paidAmount >= grandTotal)
                    {
                        invoiceStatus = "Paid";
                    }

                    _logger.LogInformation(
                        "Appointment {AppointmentId} pre-payment detected: PaidAmount={PaidAmount}, InvoiceStatus={InvoiceStatus}",
                        appointment.AppointmentId, paidAmount, invoiceStatus);
                }
            }
        }

        var invoice = new Invoice
        {
            InvoiceCode = invoiceCode,
            WorkOrderId = workOrder.WorkOrderId,
            CustomerId = workOrder.CustomerId,

            InvoiceDate = DateTime.UtcNow,
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),

            // Services
            ServiceSubTotal = servicesTotal,
            ServiceTax = servicesTotal * 0.08m,
            ServiceTotal = servicesTotal * 1.08m,

            // Parts
            PartsSubTotal = partsTotal,
            PartsTax = partsTotal * 0.08m,
            PartsTotal = partsTotal * 1.08m,

            // Tổng cộng
            SubTotal = subTotal,
            TotalTax = taxAmount,
            GrandTotal = grandTotal,

            PaidAmount = paidAmount,
            OutstandingAmount = Math.Max(grandTotal - paidAmount, 0m),

            Status = invoiceStatus,
            PaymentTerms = "Due within 7 days",
            Notes = $"Auto-generated from work order {workOrder.WorkOrderCode}",
            SentToCustomer = false,

            CreatedDate = DateTime.UtcNow,
            CreatedBy = completedBy
        };

        await _context.Invoices.AddAsync(invoice, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Created Invoice {InvoiceCode} (ID: {InvoiceId}) for WorkOrder {WorkOrderCode}: Amount={Amount}, Status={Status}, Paid={Paid}, Outstanding={Outstanding}",
            invoiceCode,
            invoice.InvoiceId,
            workOrder.WorkOrderCode,
            grandTotal,
            invoice.Status,
            invoice.PaidAmount,
            invoice.OutstandingAmount);

        return invoice;
    }

    /// <summary>
    /// Generate unique invoice code
    /// Format: INV-YYYYMMDD-XXXX
    /// </summary>
    private async Task<string> GenerateInvoiceCodeAsync(CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow;
        var prefix = $"INV-{today:yyyyMMdd}";

        var lastInvoice = await _context.Invoices
            .Where(i => i.InvoiceCode.StartsWith(prefix))
            .OrderByDescending(i => i.InvoiceCode)
            .FirstOrDefaultAsync(cancellationToken);

        if (lastInvoice == null)
            return $"{prefix}-0001";

        var lastNumber = int.Parse(lastInvoice.InvoiceCode.Substring(prefix.Length + 1));
        return $"{prefix}-{(lastNumber + 1):D4}";
    }

    public async Task<WorkOrderResponseDto> RequestApprovalAsync(
        int workOrderId,
        string approvalNotes,
        int requestedBy,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Requesting approval for WorkOrder {WorkOrderId}",
            workOrderId);

        var workOrder = await _context.WorkOrders
            .FirstOrDefaultAsync(wo => wo.WorkOrderId == workOrderId, cancellationToken);

        if (workOrder == null)
            throw new KeyNotFoundException($"Work order {workOrderId} not found");

        if (workOrder.StatusId == (int)WorkOrderStatusEnum.Completed ||
            workOrder.StatusId == (int)WorkOrderStatusEnum.Cancelled)
        {
            throw new InvalidOperationException(
                $"Cannot request approval when work order is {GetWorkOrderStatusName(workOrder.StatusId)}");
        }

        workOrder.RequiresApproval = true;
        workOrder.ApprovalRequired = true;
        workOrder.ApprovedBy = null;
        workOrder.ApprovedDate = null;
        workOrder.ApprovalNotes = approvalNotes;
        workOrder.UpdatedBy = requestedBy;
        workOrder.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        await _timelineService.AddTimelineEventAsync(
            workOrderId,
            new AddWorkOrderTimelineRequestDto
            {
                EventType = "ApprovalRequested",
                EventDescription = string.IsNullOrWhiteSpace(approvalNotes)
                    ? "Technician requested approval for additional work"
                    : $"Approval requested: {approvalNotes}",
                IsVisible = true
            },
            requestedBy,
            cancellationToken);

        return await GetWorkOrderAsync(workOrderId, cancellationToken);
    }

    public async Task<WorkOrderResponseDto> ApproveWorkOrderAsync(
        int workOrderId,
        string? approvalNotes,
        int approvedBy,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Approving WorkOrder {WorkOrderId}",
            workOrderId);

        var workOrder = await _context.WorkOrders
            .FirstOrDefaultAsync(wo => wo.WorkOrderId == workOrderId, cancellationToken);

        if (workOrder == null)
            throw new KeyNotFoundException($"Work order {workOrderId} not found");

        if (workOrder.StatusId == (int)WorkOrderStatusEnum.Cancelled)
            throw new InvalidOperationException("Cannot approve a cancelled work order");

        workOrder.RequiresApproval = false;
        workOrder.ApprovalRequired = false;
        workOrder.ApprovedBy = approvedBy;
        workOrder.ApprovedDate = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(approvalNotes))
        {
            workOrder.ApprovalNotes = approvalNotes;
        }

        workOrder.UpdatedBy = approvedBy;
        workOrder.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        await _timelineService.AddTimelineEventAsync(
            workOrderId,
            new AddWorkOrderTimelineRequestDto
            {
                EventType = "ApprovalGranted",
                EventDescription = string.IsNullOrWhiteSpace(approvalNotes)
                    ? "Work order approval granted"
                    : $"Approval granted: {approvalNotes}",
                IsVisible = true
            },
            approvedBy,
            cancellationToken);

        return await GetWorkOrderAsync(workOrderId, cancellationToken);
    }

    public async Task<WorkOrderResponseDto> AddServiceAsync(
        int workOrderId,
        int serviceId,
        int addedBy,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Adding service {ServiceId} to WorkOrder {WorkOrderId}",
            serviceId, workOrderId);

        var workOrder = await _context.WorkOrders
            .Include(wo => wo.WorkOrderServices)
            .FirstOrDefaultAsync(wo => wo.WorkOrderId == workOrderId, cancellationToken);

        if (workOrder == null)
            throw new InvalidOperationException($"Work order {workOrderId} not found");

        if (workOrder.StatusId == (int)WorkOrderStatusEnum.Completed ||
            workOrder.StatusId == (int)WorkOrderStatusEnum.Cancelled)
        {
            throw new InvalidOperationException(
                $"Cannot add service when work order is {GetWorkOrderStatusName(workOrder.StatusId)}");
        }

        if (workOrder.WorkOrderServices.Any(ws => ws.ServiceId == serviceId))
            throw new InvalidOperationException("Service already added to this work order");

        var service = await _context.MaintenanceServices
            .FirstOrDefaultAsync(s => s.ServiceId == serviceId, cancellationToken);

        if (service == null)
            throw new InvalidOperationException($"Service {serviceId} not found");

        if (service.IsActive.HasValue && service.IsActive.Value == false)
            throw new InvalidOperationException("Service is not active");

        // ✅ FIX: Wrap transaction with ExecutionStrategy
        var executionStrategy = _context.Database.CreateExecutionStrategy();

        return await executionStrategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var quantity = 1;
                // ✅ FIX: Use TotalCost (BasePrice + LaborCost) instead of BasePrice
                var unitPrice = service.BasePrice + (service.LaborCost ?? 0m);
                var totalPrice = unitPrice * quantity;

                var workOrderService = new WorkOrderService
                {
                    WorkOrderId = workOrderId,
                    ServiceId = serviceId,
                    Quantity = quantity,
                    UnitPrice = unitPrice,
                    TotalPrice = totalPrice,
                    EstimatedTime = service.StandardTime,
                    Status = "Pending",
                    Notes = $"Added by user {addedBy}"
                };

                await _context.WorkOrderServices.AddAsync(workOrderService, cancellationToken);

                workOrder.EstimatedAmount = (workOrder.EstimatedAmount ?? 0) + totalPrice;
                workOrder.TotalAmount = (workOrder.TotalAmount ?? 0) + totalPrice;
                workOrder.UpdatedBy = addedBy;
                workOrder.UpdatedDate = DateTime.UtcNow;

                _logger.LogInformation(
                    "Adding service {ServiceId} to WorkOrder {WorkOrderId}: BasePrice={BasePrice}, LaborCost={LaborCost}, UnitPrice={UnitPrice}, TotalPrice={TotalPrice}",
                    serviceId, workOrderId, service.BasePrice, service.LaborCost, unitPrice, totalPrice);

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                await _timelineService.AddTimelineEventAsync(
                    workOrderId,
                    new AddWorkOrderTimelineRequestDto
                    {
                        EventType = "ServiceAdded",
                        EventDescription = $"Added service: {service.ServiceName} ({service.ServiceCode}) - {unitPrice:C}",
                        IsVisible = true
                    },
                    addedBy,
                    cancellationToken);

                return await GetWorkOrderAsync(workOrderId, cancellationToken);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }

    /// <summary>
    /// Add part to work order with inventory update
    /// ✅ IMPLEMENTED: Parts tracking + inventory management
    /// </summary>
    public async Task<WorkOrderResponseDto> AddPartAsync(
        int workOrderId,
        int partId,
        int quantity,
        int addedBy,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Adding part {PartId} x{Quantity} to WorkOrder {WorkOrderId}",
            partId, quantity, workOrderId);

        // 1. Get WorkOrder with parts
        var workOrder = await _context.WorkOrders
            .Include(wo => wo.WorkOrderParts)
            .FirstOrDefaultAsync(wo => wo.WorkOrderId == workOrderId, cancellationToken);

        if (workOrder == null)
            throw new InvalidOperationException($"Work order {workOrderId} not found");

        // 2. Get Part & check inventory
        var part = await _context.Parts
            .FirstOrDefaultAsync(p => p.PartId == partId, cancellationToken);

        if (part == null)
            throw new InvalidOperationException($"Part {partId} not found");

        // 3. Check stock availability
        var currentStock = part.CurrentStock ?? 0;
        if (currentStock < quantity)
        {
            _logger.LogWarning(
                "Insufficient stock for Part {PartId}: Available={Available}, Required={Required}",
                partId, currentStock, quantity);
            throw new InvalidOperationException(
                $"Insufficient stock for part '{part.PartName}'. Available: {currentStock}, Required: {quantity}");
        }

        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                // 4. Check if part already exists in work order
                var existingPart = workOrder.WorkOrderParts?.FirstOrDefault(p => p.PartId == partId);

                if (existingPart != null)
                {
                    // Update existing part quantity
                    _logger.LogInformation(
                        "Part {PartId} already in WorkOrder. Updating quantity: {OldQty} + {AddQty} = {NewQty}",
                        partId, existingPart.Quantity, quantity, existingPart.Quantity + quantity);

                    existingPart.Quantity += quantity;
                    existingPart.TotalPrice = existingPart.Quantity * (existingPart.UnitPrice ?? 0);
                }
                else
                {
                    // Add new part to work order
                    var workOrderPart = new WorkOrderPart
                    {
                        WorkOrderId = workOrderId,
                        PartId = partId,
                        Quantity = quantity,
                        UnitCost = part.CostPrice,
                        UnitPrice = part.SellingPrice,
                        TotalCost = quantity * (part.CostPrice ?? 0),
                        TotalPrice = quantity * (part.SellingPrice ?? 0),
                        Status = "Requested",
                        RequestedDate = DateTime.UtcNow,
                        Notes = $"Added by user {addedBy}"
                    };

                    await _context.WorkOrderParts.AddAsync(workOrderPart, cancellationToken);

                    _logger.LogInformation(
                        "Created WorkOrderPart: PartId={PartId}, Qty={Qty}, UnitPrice={UnitPrice}, Total={Total}",
                        partId, quantity, part.SellingPrice, workOrderPart.TotalPrice);
                }

                // 5. Update part inventory
                part.CurrentStock -= quantity;

                _logger.LogInformation(
                    "Updated Part {PartId} inventory: {OldStock} - {Qty} = {NewStock}",
                    partId, currentStock, quantity, part.CurrentStock);

                // 6. Update WorkOrder estimated amount
                var partsTotal = await _context.WorkOrderParts
                    .Where(p => p.WorkOrderId == workOrderId)
                    .SumAsync(p => p.TotalPrice ?? 0, cancellationToken);

                workOrder.EstimatedAmount = (workOrder.EstimatedAmount ?? 0) + (quantity * (part.SellingPrice ?? 0));

                _logger.LogInformation(
                    "Updated WorkOrder {WorkOrderId} estimated amount to {Amount}",
                    workOrderId, workOrder.EstimatedAmount);

                // 7. Add timeline event
                await _timelineService.AddTimelineEventAsync(
                    workOrderId,
                    new AddWorkOrderTimelineRequestDto
                    {
                        EventType = "PartAdded",
                        EventDescription = $"Added part: {part.PartName} x{quantity} ({part.SellingPrice:C} each)",
                        IsVisible = true
                    },
                    addedBy,
                    cancellationToken);

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation(
                    "✅ Successfully added part {PartId} to WorkOrder {WorkOrderId}",
                    partId, workOrderId);

                // 8. Return updated work order
                return await GetWorkOrderAsync(workOrderId, cancellationToken);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex,
                    "Failed to add part {PartId} to WorkOrder {WorkOrderId}",
                    partId, workOrderId);
                throw;
            }
        });
    }

    public async Task<bool> DeleteWorkOrderAsync(
        int workOrderId,
        int deletedBy,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Soft deleting WorkOrder {WorkOrderId}", workOrderId);

        var workOrder = await _context.WorkOrders
            .FirstOrDefaultAsync(wo => wo.WorkOrderId == workOrderId, cancellationToken);

        if (workOrder == null)
            return false;

        if (workOrder.StatusId != (int)WorkOrderStatusEnum.Created &&
            workOrder.StatusId != (int)WorkOrderStatusEnum.Assigned)
        {
            throw new InvalidOperationException("Only work orders that have not started can be cancelled");
        }

        var previousStatusName = GetWorkOrderStatusName(workOrder.StatusId);
        workOrder.StatusId = (int)WorkOrderStatusEnum.Cancelled;
        workOrder.UpdatedBy = deletedBy;
        workOrder.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        await _timelineService.AddStatusChangeEventAsync(
            workOrderId,
            previousStatusName,
            GetWorkOrderStatusName(workOrder.StatusId),
            "Work order cancelled before start",
            deletedBy,
            cancellationToken);

        await _timelineService.AddTimelineEventAsync(
            workOrderId,
            new AddWorkOrderTimelineRequestDto
            {
                EventType = "WorkOrderCancelled",
                EventDescription = "Work order cancelled before starting work",
                IsVisible = true
            },
            deletedBy,
            cancellationToken);

        return true;
    }

    public async Task<WorkOrderStatisticsDto> GetWorkOrderStatisticsAsync(
        int? serviceCenterId = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.WorkOrders.AsNoTracking();

        if (serviceCenterId.HasValue)
        {
            query = query.Where(w => w.ServiceCenterId == serviceCenterId.Value);
        }

        if (dateFrom.HasValue)
        {
            query = query.Where(w => w.CreatedDate >= dateFrom.Value);
        }

        if (dateTo.HasValue)
        {
            query = query.Where(w => w.CreatedDate <= dateTo.Value);
        }

        var pendingStatuses = new[]
        {
            (int)WorkOrderStatusEnum.Created,
            (int)WorkOrderStatusEnum.Assigned
        };

        var totalWorkOrders = await query.CountAsync(cancellationToken);
        var pendingWorkOrders = await query
            .Where(w => pendingStatuses.Contains(w.StatusId))
            .CountAsync(cancellationToken);
        var inProgressWorkOrders = await query
            .Where(w => w.StatusId == (int)WorkOrderStatusEnum.InProgress)
            .CountAsync(cancellationToken);
        var completedWorkOrders = await query
            .Where(w => w.StatusId == (int)WorkOrderStatusEnum.Completed)
            .CountAsync(cancellationToken);
        var cancelledWorkOrders = await query
            .Where(w => w.StatusId == (int)WorkOrderStatusEnum.Cancelled)
            .CountAsync(cancellationToken);

        var totalRevenue = await query
            .Where(w => w.StatusId == (int)WorkOrderStatusEnum.Completed)
            .SumAsync(w => w.FinalAmount ?? w.TotalAmount ?? 0m, cancellationToken);

        var completionMinutes = await query
            .Where(w =>
                w.StatusId == (int)WorkOrderStatusEnum.Completed &&
                w.StartDate.HasValue &&
                w.CompletedDate.HasValue)
            .Select(w => EF.Functions.DateDiffMinute(w.StartDate!.Value, w.CompletedDate!.Value))
            .ToListAsync(cancellationToken);

        var averageCompletionHours = 0m;
        if (completionMinutes.Any())
        {
            var averageMinutes = completionMinutes.Average(); // double
            averageCompletionHours = Math.Round((decimal)averageMinutes / 60m, 2);
        }

        return new WorkOrderStatisticsDto
        {
            TotalWorkOrders = totalWorkOrders,
            PendingWorkOrders = pendingWorkOrders,
            InProgressWorkOrders = inProgressWorkOrders,
            CompletedWorkOrders = completedWorkOrders,
            CancelledWorkOrders = cancelledWorkOrders,
            TotalRevenue = decimal.Round(totalRevenue, 2),
            AverageCompletionTimeHours = averageCompletionHours
        };
    }
}

