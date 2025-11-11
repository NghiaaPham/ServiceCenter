using EVServiceCenter.Core.Domains.Checklists.DTOs.Requests;
using EVServiceCenter.Core.Domains.Checklists.DTOs.Responses;
using EVServiceCenter.Core.Domains.Shared.Models;

namespace EVServiceCenter.Core.Domains.Checklists.Interfaces;

/// <summary>
/// Service interface for checklist business logic
/// </summary>
public interface IChecklistService
{
    // Template Management
    Task<PagedResult<ChecklistTemplateResponseDto>> GetTemplatesAsync(
        ChecklistTemplateQueryDto query, CancellationToken cancellationToken);

    Task<ChecklistTemplateResponseDto> GetTemplateByIdAsync(
        int templateId, CancellationToken cancellationToken);

    Task<ChecklistTemplateResponseDto> CreateTemplateAsync(
        CreateChecklistTemplateRequestDto request, int createdBy, CancellationToken cancellationToken);

    Task<ChecklistTemplateResponseDto> UpdateTemplateAsync(
        int templateId, UpdateChecklistTemplateRequestDto request, CancellationToken cancellationToken);

    Task<bool> DeleteTemplateAsync(int templateId, CancellationToken cancellationToken);

    // Work Order Checklist Management
    Task<WorkOrderChecklistResponseDto> GetWorkOrderChecklistAsync(
        int workOrderId, CancellationToken cancellationToken);

    Task<WorkOrderChecklistResponseDto> ApplyTemplateToWorkOrderAsync(
        int workOrderId, ApplyChecklistTemplateRequestDto request, CancellationToken cancellationToken);

    // Checklist Item Operations
    Task<ChecklistItemDetailResponseDto> UpdateChecklistItemAsync(
        int itemId, UpdateChecklistItemStatusRequestDto request, int updatedBy,
        CancellationToken cancellationToken);

    Task<ChecklistItemDetailResponseDto> MarkItemCompleteAsync(
        int itemId, int completedBy, string? notes, CancellationToken cancellationToken);

    Task<ChecklistItemDetailResponseDto> MarkItemIncompleteAsync(
        int itemId, CancellationToken cancellationToken);

    // ? NEW: Complete/Skip checklist items
    /// <summary>
    /// Complete m?t checklist item (technician tick ?)
    /// </summary>
    Task<ChecklistItemResponseDto> CompleteChecklistItemAsync(
        CompleteChecklistItemRequestDto request,
        int completedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Skip m?t checklist item v?i lý do (cho optional items)
    /// </summary>
    Task<ChecklistItemResponseDto> SkipChecklistItemAsync(
        SkipChecklistItemRequestDto request,
        int skippedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate xem WorkOrder có th? complete không
    /// (t?t c? required items ?ã completed)
    /// </summary>
    Task<(bool CanComplete, List<string> MissingItems)> ValidateWorkOrderCompletionAsync(
        int workOrderId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Complete T?T C? checklist items c?a WorkOrder trong m?t l?n (bulk operation)
    /// Use case: Auto-complete toàn b? checklist khi test ho?c khi technician hoàn thành nhanh
    /// </summary>
    Task<BulkCompleteChecklistResponseDto> CompleteAllItemsAsync(
        int workOrderId,
        string? notes,
        int completedBy,
        CancellationToken cancellationToken = default);
}
