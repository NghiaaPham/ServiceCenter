using EVServiceCenter.Core.Domains.Checklists.DTOs.Requests;
using EVServiceCenter.Core.Domains.Checklists.DTOs.Responses;
using EVServiceCenter.Core.Domains.Identity.Entities;
using EVServiceCenter.Core.Domains.Shared.Models;

namespace EVServiceCenter.Core.Domains.Checklists.Interfaces;

/// <summary>
/// Repository interface for checklist template and item data access
/// </summary>
public interface IChecklistRepository
{
    // Template Operations
    Task<PagedResult<ChecklistTemplateResponseDto>> GetTemplatesAsync(
        ChecklistTemplateQueryDto query, CancellationToken cancellationToken);

    Task<ChecklistTemplateResponseDto?> GetTemplateByIdAsync(
        int templateId, CancellationToken cancellationToken);

    Task<ChecklistTemplateResponseDto> CreateTemplateAsync(
        CreateChecklistTemplateRequestDto request, int createdBy, CancellationToken cancellationToken);

    Task<ChecklistTemplateResponseDto> UpdateTemplateAsync(
        int templateId, UpdateChecklistTemplateRequestDto request, CancellationToken cancellationToken);

    Task<bool> DeleteTemplateAsync(int templateId, CancellationToken cancellationToken);

    // Work Order Checklist Operations
    Task<WorkOrderChecklistResponseDto?> GetWorkOrderChecklistAsync(
        int workOrderId, CancellationToken cancellationToken);

    Task<List<ChecklistItemDetailResponseDto>> ApplyTemplateToWorkOrderAsync(
        int workOrderId, int templateId, List<ChecklistTemplateItemDto>? customItems,
        CancellationToken cancellationToken);

    // Checklist Item Operations
    Task<ChecklistItemDetailResponseDto> UpdateChecklistItemAsync(
        int itemId, UpdateChecklistItemStatusRequestDto request, int updatedBy,
        CancellationToken cancellationToken);

    // New methods for complete/skip items
    Task<ChecklistItemResponseDto?> GetChecklistItemByIdAsync(
        int itemId, CancellationToken cancellationToken = default);

    Task<ChecklistItemResponseDto> CompleteChecklistItemAsync(
        int itemId, int completedBy, string? notes, string? imageUrl,
        CancellationToken cancellationToken = default);

    Task UpdateWorkOrderChecklistProgressAsync(
        int workOrderId, int totalItems, int completedItems,
        CancellationToken cancellationToken = default);

    // Helper method to get user info
    Task<User?> GetUserByIdAsync(
        int userId, CancellationToken cancellationToken = default);
}
