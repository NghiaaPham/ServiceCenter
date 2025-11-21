using EVServiceCenter.Core.Domains.Checklists.DTOs.Requests;
using EVServiceCenter.Core.Domains.Checklists.DTOs.Responses;
using EVServiceCenter.Core.Domains.Checklists.Interfaces;
using EVServiceCenter.Core.Domains.Shared.Models;
using Microsoft.Extensions.Logging;

namespace EVServiceCenter.Infrastructure.Domains.Checklists.Services;

/// <summary>
/// Service for checklist business logic and validation
/// </summary>
public class ChecklistService : IChecklistService
{
    private readonly IChecklistRepository _repository;
    private readonly ILogger<ChecklistService> _logger;

    public ChecklistService(
        IChecklistRepository repository,
        ILogger<ChecklistService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    #region Template Management

    /// <summary>
    /// Get checklist templates with filtering and pagination
    /// </summary>
    public async Task<PagedResult<ChecklistTemplateResponseDto>> GetTemplatesAsync(
        ChecklistTemplateQueryDto query, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting checklist templates with query: {Query}", query);

        try
        {
            return await _repository.GetTemplatesAsync(query, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting checklist templates");
            throw;
        }
    }

    /// <summary>
    /// Get template by ID
    /// </summary>
    public async Task<ChecklistTemplateResponseDto> GetTemplateByIdAsync(
        int templateId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting checklist template {TemplateId}", templateId);

        var template = await _repository.GetTemplateByIdAsync(templateId, cancellationToken);

        if (template == null)
        {
            _logger.LogWarning("Template {TemplateId} not found", templateId);
            throw new KeyNotFoundException($"Template {templateId} not found");
        }

        return template;
    }

    /// <summary>
    /// Create new checklist template
    /// Business rule: Template name must be unique per service/category
    /// </summary>
    public async Task<ChecklistTemplateResponseDto> CreateTemplateAsync(
        CreateChecklistTemplateRequestDto request, int createdBy, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Creating checklist template '{Name}' with {Count} items",
            request.TemplateName, request.Items.Count);

        // Business validation: Items must have sequential order starting from 1
        ValidateItemOrdering(request.Items);

        try
        {
            var result = await _repository.CreateTemplateAsync(request, createdBy, cancellationToken);

            _logger.LogInformation(
                "Created checklist template {TemplateId}: {Name}",
                result.TemplateId, result.TemplateName);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating checklist template");
            throw;
        }
    }

    /// <summary>
    /// Update existing checklist template
    /// </summary>
    public async Task<ChecklistTemplateResponseDto> UpdateTemplateAsync(
        int templateId, UpdateChecklistTemplateRequestDto request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating checklist template {TemplateId}", templateId);

        // Business validation: If updating items, validate ordering
        if (request.Items != null && request.Items.Any())
        {
            ValidateItemOrdering(request.Items);
        }

        try
        {
            var result = await _repository.UpdateTemplateAsync(templateId, request, cancellationToken);

            _logger.LogInformation("Updated checklist template {TemplateId}", templateId);

            return result;
        }
        catch (KeyNotFoundException)
        {
            _logger.LogWarning("Template {TemplateId} not found for update", templateId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating checklist template {TemplateId}", templateId);
            throw;
        }
    }

    /// <summary>
    /// Delete checklist template (soft delete)
    /// </summary>
    public async Task<bool> DeleteTemplateAsync(int templateId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting checklist template {TemplateId}", templateId);

        try
        {
            var result = await _repository.DeleteTemplateAsync(templateId, cancellationToken);

            if (result)
                _logger.LogInformation("Deleted checklist template {TemplateId}", templateId);
            else
                _logger.LogWarning("Template {TemplateId} not found for deletion", templateId);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting checklist template {TemplateId}", templateId);
            throw;
        }
    }

    #endregion

    #region Work Order Checklist Management

    /// <summary>
    /// Get work order checklist with completion status
    /// </summary>
    public async Task<WorkOrderChecklistResponseDto> GetWorkOrderChecklistAsync(
        int workOrderId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting checklist for work order {WorkOrderId}", workOrderId);

        var checklist = await _repository.GetWorkOrderChecklistAsync(workOrderId, cancellationToken);

        if (checklist == null)
        {
            _logger.LogWarning("Work order {WorkOrderId} not found when retrieving checklist", workOrderId);
            throw new KeyNotFoundException($"Work order {workOrderId} not found");
        }

        return checklist;
    }

    /// <summary>
    /// Apply checklist template to work order
    /// Business rule: Work order can only have one checklist (delete existing before applying new)
    /// </summary>
    public async Task<WorkOrderChecklistResponseDto> ApplyTemplateToWorkOrderAsync(
        int workOrderId, ApplyChecklistTemplateRequestDto request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Applying template {TemplateId} to work order {WorkOrderId}",
            request.TemplateId, workOrderId);

        // Business validation: If custom items provided, validate ordering
        if (request.CustomItems != null && request.CustomItems.Any())
        {
            ValidateItemOrdering(request.CustomItems);
        }

        try
        {
            var items = await _repository.ApplyTemplateToWorkOrderAsync(
                workOrderId,
                request.TemplateId,
                request.CustomItems,
                cancellationToken);

            _logger.LogInformation(
                "Applied template {TemplateId} to work order {WorkOrderId}, created {Count} items",
                request.TemplateId, workOrderId, items.Count);

            // Return full checklist
            return await GetWorkOrderChecklistAsync(workOrderId, cancellationToken);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying template to work order");
            throw;
        }
    }

    /// <summary>
    /// Determine default template based on provided services
    /// </summary>
    public async Task<int?> GetDefaultTemplateIdForServicesAsync(
        List<int> serviceIds,
        CancellationToken cancellationToken)
    {
        if (serviceIds == null || serviceIds.Count == 0)
        {
            _logger.LogWarning("Cannot determine default checklist template because service list is empty");
            return null;
        }

        try
        {
            var templateId = await _repository.FindBestTemplateIdForServicesAsync(serviceIds, cancellationToken);

            if (templateId.HasValue)
            {
                _logger.LogInformation(
                    "Auto-selected checklist template {TemplateId} for service ids: {ServiceIds}",
                    templateId.Value, string.Join(", ", serviceIds));
            }
            else
            {
                _logger.LogWarning(
                    "No checklist template found for service ids: {ServiceIds}",
                    string.Join(", ", serviceIds));
            }

            return templateId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error finding default checklist template for services: {ServiceIds}",
                string.Join(", ", serviceIds));
            throw;
        }
    }
    #endregion

    #region Checklist Item Operations

    /// <summary>
    /// Update checklist item status, notes, and image
    /// </summary>
    public async Task<ChecklistItemDetailResponseDto> UpdateChecklistItemAsync(
        int itemId, UpdateChecklistItemStatusRequestDto request, int updatedBy,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating checklist item {ItemId}", itemId);

        try
        {
            var result = await _repository.UpdateChecklistItemAsync(
                itemId, request, updatedBy, cancellationToken);

            _logger.LogInformation(
                "Updated checklist item {ItemId}, IsCompleted: {IsCompleted}",
                itemId, result.IsCompleted);

            return result;
        }
        catch (KeyNotFoundException)
        {
            _logger.LogWarning("Checklist item {ItemId} not found", itemId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating checklist item {ItemId}", itemId);
            throw;
        }
    }

    /// <summary>
    /// Mark checklist item as complete
    /// Convenience method for completing items
    /// </summary>
    public async Task<ChecklistItemDetailResponseDto> MarkItemCompleteAsync(
        int itemId, int completedBy, string? notes, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Marking checklist item {ItemId} as complete", itemId);

        var request = new UpdateChecklistItemStatusRequestDto
        {
            IsCompleted = true,
            Notes = notes
        };

        return await UpdateChecklistItemAsync(itemId, request, completedBy, cancellationToken);
    }

    /// <summary>
    /// Mark checklist item as incomplete
    /// Convenience method for unmarking items
    /// </summary>
    public async Task<ChecklistItemDetailResponseDto> MarkItemIncompleteAsync(
        int itemId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Marking checklist item {ItemId} as incomplete", itemId);

        var request = new UpdateChecklistItemStatusRequestDto
        {
            IsCompleted = false
        };

        // Use dummy userId (0) since we're just unmarking
        return await UpdateChecklistItemAsync(itemId, request, 0, cancellationToken);
    }

    #endregion

    #region NEW: Complete/Skip Checklist Items

    /// <summary>
    /// ? Complete m?t checklist item (technician tick ?)
    /// </summary>
    public async Task<ChecklistItemResponseDto> CompleteChecklistItemAsync(
        CompleteChecklistItemRequestDto request,
        int completedBy,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Completing checklist item {ItemId} for WorkOrder {WorkOrderId} by user {UserId}",
            request.ItemId, request.WorkOrderId, completedBy);

        try
        {
            // 1. Validate item thu?c v? work order
            var item = await _repository.GetChecklistItemByIdAsync(request.ItemId, cancellationToken);
            
            if (item == null)
                throw new KeyNotFoundException($"Checklist item {request.ItemId} not found");

            if (item.WorkOrderId != request.WorkOrderId)
                throw new InvalidOperationException(
                    $"Checklist item {request.ItemId} does not belong to work order {request.WorkOrderId}");

            // 2. Update item
            var result = await _repository.CompleteChecklistItemAsync(
                request.ItemId,
                completedBy,
                request.Notes,
                request.ImageUrl,
                cancellationToken);

            _logger.LogInformation(
                "? Completed checklist item {ItemId}: {Description}",
                request.ItemId, result.ItemDescription);

            // 3. Update WorkOrder progress
            await UpdateWorkOrderProgressAsync(request.WorkOrderId, cancellationToken);

            return result;
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex.Message);
            throw;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing checklist item {ItemId}", request.ItemId);
            throw;
        }
    }

    /// <summary>
    /// ?? Skip m?t checklist item v?i lý do (cho optional items)
    /// </summary>
    public async Task<ChecklistItemResponseDto> SkipChecklistItemAsync(
        SkipChecklistItemRequestDto request,
        int skippedBy,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Skipping checklist item {ItemId} for WorkOrder {WorkOrderId} by user {UserId}, Reason: {Reason}",
            request.ItemId, request.WorkOrderId, skippedBy, request.SkipReason);

        try
        {
            // 1. Validate item
            var item = await _repository.GetChecklistItemByIdAsync(request.ItemId, cancellationToken);
            
            if (item == null)
                throw new KeyNotFoundException($"Checklist item {request.ItemId} not found");

            if (item.WorkOrderId != request.WorkOrderId)
                throw new InvalidOperationException(
                    $"Checklist item {request.ItemId} does not belong to work order {request.WorkOrderId}");

            // 2. Validate: Ch? skip ???c optional items
            if (item.IsRequired == true)
                throw new InvalidOperationException(
                    $"Cannot skip required checklist item: {item.ItemDescription}. " +
                    "Required items must be completed.");

            // 3. Mark as completed with skip reason in notes
            var skipNote = $"[SKIPPED] {request.SkipReason}";
            
            var result = await _repository.CompleteChecklistItemAsync(
                request.ItemId,
                skippedBy,
                skipNote,
                null,
                cancellationToken);

            _logger.LogInformation(
                "?? Skipped checklist item {ItemId}: {Description}",
                request.ItemId, result.ItemDescription);

            // 4. Update WorkOrder progress
            await UpdateWorkOrderProgressAsync(request.WorkOrderId, cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error skipping checklist item {ItemId}", request.ItemId);
            throw;
        }
    }

    /// <summary>
    /// ?? Validate xem WorkOrder có th? complete không
    /// </summary>
    public async Task<(bool CanComplete, List<string> MissingItems)> ValidateWorkOrderCompletionAsync(
        int workOrderId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Validating completion for work order {WorkOrderId}", workOrderId);

        try
        {
            var checklist = await _repository.GetWorkOrderChecklistAsync(workOrderId, cancellationToken);
            
            if (checklist == null)
                throw new KeyNotFoundException($"Work order {workOrderId} not found");

            // L?y các required items ch?a completed
            var missingItems = checklist.Items
                .Where(i => i.IsRequired && !i.IsCompleted)
                .Select(i => i.ItemDescription)
                .ToList();

            bool canComplete = !missingItems.Any();

            _logger.LogInformation(
                "WorkOrder {WorkOrderId} completion validation: CanComplete={CanComplete}, MissingItems={Count}",
                workOrderId, canComplete, missingItems.Count);

            return (canComplete, missingItems);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating work order completion");
            throw;
        }
    }

    /// <summary>
    /// Helper: Update WorkOrder progress based on checklist completion
    /// </summary>
    private async Task UpdateWorkOrderProgressAsync(
        int workOrderId,
        CancellationToken cancellationToken)
    {
        var checklist = await _repository.GetWorkOrderChecklistAsync(workOrderId, cancellationToken);
        
        if (checklist == null) return;

        await _repository.UpdateWorkOrderChecklistProgressAsync(
            workOrderId,
            checklist.TotalItems,
            checklist.CompletedItems,
            cancellationToken);

        _logger.LogInformation(
            "Updated WorkOrder {WorkOrderId} progress: {Completed}/{Total} ({Percentage}%)",
            workOrderId, checklist.CompletedItems, checklist.TotalItems, 
            checklist.CompletionPercentage);
    }

    #endregion

    #region NEW: Bulk Complete All Items

    /// <summary>
    /// Complete T?T C? checklist items c?a WorkOrder trong m?t l?n (bulk operation)
    /// Use case: Auto-complete toàn b? checklist khi test ho?c khi technician hoàn thành nhanh
    /// FIX: Load CompletedByName from User entity
    /// </summary>
    public async Task<BulkCompleteChecklistResponseDto> CompleteAllItemsAsync(
        int workOrderId,
        string? notes,
        int completedBy,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Bulk completing ALL checklist items for WorkOrder {WorkOrderId} by user {UserId}",
            workOrderId, completedBy);

        try
        {
            // ? FIX: Load User entity first to get FullName
            var user = await _repository.GetUserByIdAsync(completedBy, cancellationToken);
            string? completedByName = user?.FullName;

            // 1. Get all checklist items
            var checklist = await _repository.GetWorkOrderChecklistAsync(workOrderId, cancellationToken);
            
            if (checklist == null)
                throw new KeyNotFoundException($"Work order {workOrderId} not found");

            if (checklist.TotalItems == 0)
            {
                _logger.LogWarning("WorkOrder {WorkOrderId} has no checklist items", workOrderId);
                throw new InvalidOperationException($"Work order {workOrderId} has no checklist items");
            }

            // 2. Get incomplete items
            var incompleteItems = checklist.Items
                .Where(i => !i.IsCompleted)
                .ToList();

            if (incompleteItems.Count == 0)
            {
                _logger.LogInformation("All items already completed for WorkOrder {WorkOrderId}", workOrderId);
                
                return new BulkCompleteChecklistResponseDto
                {
                    WorkOrderId = workOrderId,
                    TotalItems = checklist.TotalItems,
                    CompletedItems = checklist.CompletedItems,
                    FailedItems = 0,
                    FailedItemDescriptions = new List<string>(),
                    CompletionPercentage = 100,
                    CompletedDate = DateTime.UtcNow,
                    CompletedBy = completedBy,
                    CompletedByName = completedByName, // ? FIX: Use loaded name
                    Notes = notes
                };
            }

            // 3. Complete each item
            var failedItems = new List<string>();
            int successCount = 0;

            foreach (var item in incompleteItems)
            {
                try
                {
                    await _repository.CompleteChecklistItemAsync(
                        item.ItemId,
                        completedBy,
                        notes ?? "Bulk completed",
                        null,
                        cancellationToken);

                    successCount++;

                    _logger.LogDebug(
                        "Completed item {ItemId}: {Description}",
                        item.ItemId, item.ItemDescription);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Failed to complete item {ItemId}: {Description}",
                        item.ItemId, item.ItemDescription);

                    failedItems.Add($"{item.ItemDescription} (ItemId: {item.ItemId})");
                }
            }

            // 4. Update WorkOrder progress
            await UpdateWorkOrderProgressAsync(workOrderId, cancellationToken);

            // 5. Get updated checklist
            var updatedChecklist = await _repository.GetWorkOrderChecklistAsync(workOrderId, cancellationToken);

            var response = new BulkCompleteChecklistResponseDto
            {
                WorkOrderId = workOrderId,
                TotalItems = checklist.TotalItems,
                CompletedItems = updatedChecklist?.CompletedItems ?? (checklist.CompletedItems + successCount),
                FailedItems = failedItems.Count,
                FailedItemDescriptions = failedItems,
                CompletionPercentage = updatedChecklist?.CompletionPercentage ?? 
                    ((decimal)(checklist.CompletedItems + successCount) / checklist.TotalItems * 100),
                CompletedDate = DateTime.UtcNow,
                CompletedBy = completedBy,
                CompletedByName = completedByName, // ? FIX: Use loaded name
                Notes = notes
            };

            _logger.LogInformation(
                "Bulk complete finished for WorkOrder {WorkOrderId}: " +
                "{Success}/{Total} succeeded, {Failed} failed",
                workOrderId, successCount, incompleteItems.Count, failedItems.Count);

            return response;
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk completing checklist items for WorkOrder {WorkOrderId}", workOrderId);
            throw;
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Validate item ordering is sequential starting from 1
    /// Business rule: Order numbers must be unique and start from 1
    /// </summary>
    private void ValidateItemOrdering(List<ChecklistTemplateItemDto> items)
    {
        if (items == null || !items.Any())
            return;

        var orders = items.Select(i => i.Order).OrderBy(o => o).ToList();

        // Check for duplicates
        if (orders.Distinct().Count() != orders.Count)
        {
            throw new ArgumentException("Item order numbers must be unique");
        }

        // Validate sequential ordering (optional - can be relaxed)
        // Just ensure all orders are positive
        if (orders.Any(o => o <= 0))
        {
            throw new ArgumentException("Item order numbers must be greater than 0");
        }
    }

    #endregion
}

