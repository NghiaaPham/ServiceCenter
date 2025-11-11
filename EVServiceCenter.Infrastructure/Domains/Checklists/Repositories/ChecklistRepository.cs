using EVServiceCenter.Core.Domains.Checklists.DTOs.Requests;
using EVServiceCenter.Core.Domains.Checklists.DTOs.Responses;
using EVServiceCenter.Core.Domains.Checklists.Interfaces;
using EVServiceCenter.Core.Domains.Identity.Entities;
using EVServiceCenter.Core.Domains.Shared.Models;
using EVServiceCenter.Core.Entities;
using EVServiceCenter.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace EVServiceCenter.Infrastructure.Domains.Checklists.Repositories;

/// <summary>
/// Repository for checklist template and item data access
/// Performance: AsNoTracking, database projection, efficient filtering
/// </summary>
public class ChecklistRepository : IChecklistRepository
{
    private readonly EVDbContext _context;
    private readonly ILogger<ChecklistRepository> _logger; // Add logger

    public ChecklistRepository(EVDbContext context, ILogger<ChecklistRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Template Operations

    /// <summary>
    /// Get checklist templates with filtering and pagination
    /// Performance: AsNoTracking + database projection
    /// </summary>
    public async Task<PagedResult<ChecklistTemplateResponseDto>> GetTemplatesAsync(
        ChecklistTemplateQueryDto query, CancellationToken cancellationToken)
    {
        // Base query with AsNoTracking for read-only
        var baseQuery = _context.Set<ChecklistTemplate>()
            .AsNoTracking()
            .Where(t => true);

        // Apply filters in index-aware order
        if (query.IsActive.HasValue)
            baseQuery = baseQuery.Where(t => t.IsActive == query.IsActive.Value);

        if (query.ServiceId.HasValue)
            baseQuery = baseQuery.Where(t => t.ServiceId == query.ServiceId.Value);

        if (query.CategoryId.HasValue)
            baseQuery = baseQuery.Where(t => t.CategoryId == query.CategoryId.Value);

        if (!string.IsNullOrEmpty(query.SearchTerm))
        {
            var searchLower = query.SearchTerm.ToLower();
            baseQuery = baseQuery.Where(t => t.TemplateName.ToLower().Contains(searchLower));
        }

        // Get total count before pagination
        var totalCount = await baseQuery.CountAsync(cancellationToken);

        // Apply sorting
        baseQuery = query.SortBy.ToLowerInvariant() switch
        {
            "createddate" => query.SortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase)
                ? baseQuery.OrderByDescending(t => t.CreatedDate)
                : baseQuery.OrderBy(t => t.CreatedDate),
            _ => query.SortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase)
                ? baseQuery.OrderByDescending(t => t.TemplateName)
                : baseQuery.OrderBy(t => t.TemplateName)
        };

        // Project to DTO IN DATABASE (performance optimization)
        var templates = await baseQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(t => new ChecklistTemplateResponseDto
            {
                TemplateId = t.TemplateId,
                TemplateName = t.TemplateName,
                ServiceId = t.ServiceId,
                ServiceName = t.Service != null ? t.Service.ServiceName : null,
                CategoryId = t.CategoryId,
                CategoryName = t.Category != null ? t.Category.CategoryName : null,
                IsActive = t.IsActive ?? true,
                CreatedDate = t.CreatedDate ?? DateTime.UtcNow,
                CreatedBy = t.CreatedBy,
                CreatedByName = t.CreatedByNavigation != null ? t.CreatedByNavigation.FullName : null,
                TotalItems = 0, // Will be calculated after parsing JSON
                Items = new List<ChecklistTemplateItemResponseDto>()
            })
            .ToListAsync(cancellationToken);

        // Parse JSON items for each template (done in memory after fetch)
        foreach (var template in templates)
        {
            var originalTemplate = await _context.Set<ChecklistTemplate>()
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TemplateId == template.TemplateId, cancellationToken);

            if (originalTemplate != null && !string.IsNullOrEmpty(originalTemplate.Items))
            {
                try
                {
                    var items = JsonSerializer.Deserialize<List<ChecklistTemplateItemDto>>(originalTemplate.Items);
                    if (items != null)
                    {
                        template.Items = items.Select(i => new ChecklistTemplateItemResponseDto
                        {
                            Order = i.Order,
                            Description = i.Description,
                            IsRequired = i.IsRequired
                        }).OrderBy(i => i.Order).ToList();
                        template.TotalItems = items.Count;
                    }
                }
                catch
                {
                    // If JSON parsing fails, leave empty
                    template.Items = new List<ChecklistTemplateItemResponseDto>();
                    template.TotalItems = 0;
                }
            }
        }

        return new PagedResult<ChecklistTemplateResponseDto>
        {
            Items = templates,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
        };
    }

    /// <summary>
    /// Get template by ID with full details
    /// </summary>
    public async Task<ChecklistTemplateResponseDto?> GetTemplateByIdAsync(
        int templateId, CancellationToken cancellationToken)
    {
        var template = await _context.Set<ChecklistTemplate>()
            .AsNoTracking()
            .Include(t => t.Service)
            .Include(t => t.Category)
            .Include(t => t.CreatedByNavigation)
            .Where(t => t.TemplateId == templateId)
            .Select(t => new ChecklistTemplateResponseDto
            {
                TemplateId = t.TemplateId,
                TemplateName = t.TemplateName,
                ServiceId = t.ServiceId,
                ServiceName = t.Service != null ? t.Service.ServiceName : null,
                CategoryId = t.CategoryId,
                CategoryName = t.Category != null ? t.Category.CategoryName : null,
                IsActive = t.IsActive ?? true,
                CreatedDate = t.CreatedDate ?? DateTime.UtcNow,
                CreatedBy = t.CreatedBy,
                CreatedByName = t.CreatedByNavigation != null ? t.CreatedByNavigation.FullName : null,
                TotalItems = 0,
                Items = new List<ChecklistTemplateItemResponseDto>()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (template == null)
            return null;

        // Parse JSON items
        var originalTemplate = await _context.Set<ChecklistTemplate>()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TemplateId == templateId, cancellationToken);

        if (originalTemplate != null && !string.IsNullOrEmpty(originalTemplate.Items))
        {
            try
            {
                var items = JsonSerializer.Deserialize<List<ChecklistTemplateItemDto>>(originalTemplate.Items);
                if (items != null)
                {
                    template.Items = items.Select(i => new ChecklistTemplateItemResponseDto
                    {
                        Order = i.Order,
                        Description = i.Description,
                        IsRequired = i.IsRequired
                    }).OrderBy(i => i.Order).ToList();
                    template.TotalItems = items.Count;
                }
            }
            catch
            {
                template.Items = new List<ChecklistTemplateItemResponseDto>();
                template.TotalItems = 0;
            }
        }

        return template;
    }

    /// <summary>
    /// Create new checklist template
    /// </summary>
    public async Task<ChecklistTemplateResponseDto> CreateTemplateAsync(
        CreateChecklistTemplateRequestDto request, int createdBy, CancellationToken cancellationToken)
    {
        // Serialize items to JSON
        var itemsJson = JsonSerializer.Serialize(request.Items);

        var template = new ChecklistTemplate
        {
            TemplateName = request.TemplateName,
            ServiceId = request.ServiceId,
            CategoryId = request.CategoryId,
            Items = itemsJson,
            IsActive = request.IsActive,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        _context.Set<ChecklistTemplate>().Add(template);
        await _context.SaveChangesAsync(cancellationToken);

        return (await GetTemplateByIdAsync(template.TemplateId, cancellationToken))!;
    }

    /// <summary>
    /// Update existing checklist template
    /// </summary>
    public async Task<ChecklistTemplateResponseDto> UpdateTemplateAsync(
        int templateId, UpdateChecklistTemplateRequestDto request, CancellationToken cancellationToken)
    {
        var template = await _context.Set<ChecklistTemplate>()
            .FirstOrDefaultAsync(t => t.TemplateId == templateId, cancellationToken);

        if (template == null)
            throw new KeyNotFoundException($"Template {templateId} not found");

        // Update fields if provided
        if (!string.IsNullOrEmpty(request.TemplateName))
            template.TemplateName = request.TemplateName;

        if (request.ServiceId.HasValue)
            template.ServiceId = request.ServiceId.Value;

        if (request.CategoryId.HasValue)
            template.CategoryId = request.CategoryId.Value;

        if (request.Items != null && request.Items.Any())
            template.Items = JsonSerializer.Serialize(request.Items);

        if (request.IsActive.HasValue)
            template.IsActive = request.IsActive.Value;

        await _context.SaveChangesAsync(cancellationToken);

        return (await GetTemplateByIdAsync(templateId, cancellationToken))!;
    }

    /// <summary>
    /// Delete checklist template (soft delete)
    /// </summary>
    public async Task<bool> DeleteTemplateAsync(int templateId, CancellationToken cancellationToken)
    {
        var template = await _context.Set<ChecklistTemplate>()
            .FirstOrDefaultAsync(t => t.TemplateId == templateId, cancellationToken);

        if (template == null)
            return false;

        template.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    #endregion

    #region Work Order Checklist Operations

    /// <summary>
    /// Get work order checklist with completion status
    /// Performance: Single query with includes, calculated in memory
    /// FIX: Ensure navigation properties are loaded correctly
    /// </summary>
    public async Task<WorkOrderChecklistResponseDto?> GetWorkOrderChecklistAsync(
        int workOrderId, CancellationToken cancellationToken)
    {
        var workOrder = await _context.Set<WorkOrder>()
            .AsNoTracking()
            .AsSplitQuery() // ? FIX: Use split query to properly load navigation properties
            .Include(w => w.ChecklistItems)
                .ThenInclude(c => c.CompletedByNavigation)
            .FirstOrDefaultAsync(w => w.WorkOrderId == workOrderId, cancellationToken);

        if (workOrder == null)
            return null;

        var items = workOrder.ChecklistItems
            .OrderBy(c => c.ItemOrder)
            .Select(c => new ChecklistItemDetailResponseDto
            {
                ItemId = c.ItemId,
                ItemOrder = c.ItemOrder ?? 0,
                ItemDescription = c.ItemDescription,
                IsRequired = c.IsRequired ?? true,
                IsCompleted = c.IsCompleted ?? false,
                CompletedBy = c.CompletedBy,
                CompletedByName = c.CompletedByNavigation != null ? c.CompletedByNavigation.FullName : null,
                CompletedDate = c.CompletedDate,
                Notes = c.Notes,
                ImageUrl = c.ImageUrl
            })
            .ToList();

        var totalItems = items.Count;
        var completedItems = items.Count(i => i.IsCompleted);

        return new WorkOrderChecklistResponseDto
        {
            WorkOrderId = workOrder.WorkOrderId,
            WorkOrderCode = workOrder.WorkOrderCode,
            TotalItems = totalItems,
            CompletedItems = completedItems,
            CompletionPercentage = totalItems > 0 ? Math.Round((completedItems / (decimal)totalItems) * 100, 2) : 0,
            Items = items
        };
    }

    /// <summary>
    /// Apply checklist template to work order
    /// Creates ChecklistItem records from template
    /// ?? FIX: Use ExecutionStrategy to wrap transaction (avoid retry strategy conflict)
    /// </summary>
    public async Task<List<ChecklistItemDetailResponseDto>> ApplyTemplateToWorkOrderAsync(
        int workOrderId, int templateId, List<ChecklistTemplateItemDto>? customItems,
        CancellationToken cancellationToken)
    {
        // ?? FIX: Create execution strategy
        var executionStrategy = _context.Database.CreateExecutionStrategy();

        return await executionStrategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var workOrder = await _context.Set<WorkOrder>()
                    .Include(w => w.ChecklistItems)
                    .FirstOrDefaultAsync(w => w.WorkOrderId == workOrderId, cancellationToken);

                if (workOrder == null)
                    throw new KeyNotFoundException($"Work order {workOrderId} not found");

                var template = await _context.Set<ChecklistTemplate>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.TemplateId == templateId, cancellationToken);

                if (template == null)
                    throw new KeyNotFoundException($"Template {templateId} not found");

                List<ChecklistTemplateItemDto> itemsToCreate;
                if (customItems != null && customItems.Any())
                {
                    itemsToCreate = customItems;
                }
                else
                {
                    try
                    {
                        itemsToCreate = JsonSerializer.Deserialize<List<ChecklistTemplateItemDto>>(template.Items)
                            ?? new List<ChecklistTemplateItemDto>();
                    }
                    catch
                    {
                        throw new InvalidOperationException("Failed to parse template items");
                    }
                }

                // Remove existing checklist items (enforce single checklist per work order)
                if (workOrder.ChecklistItems.Any())
                {
                    _context.Set<ChecklistItem>().RemoveRange(workOrder.ChecklistItems);
                }

                var orderedItems = itemsToCreate.OrderBy(i => i.Order).ToList();
                var checklistItems = orderedItems
                    .Select(i => new ChecklistItem
                    {
                        WorkOrderId = workOrderId,
                        TemplateId = templateId,
                        ItemOrder = i.Order,
                        ItemDescription = i.Description,
                        IsRequired = i.IsRequired,
                        IsCompleted = false
                    })
                    .ToList();

                _context.Set<ChecklistItem>().AddRange(checklistItems);

                workOrder.ChecklistCompleted = 0;
                workOrder.ChecklistTotal = checklistItems.Count;
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return checklistItems.Select(c => new ChecklistItemDetailResponseDto
                {
                    ItemId = c.ItemId,
                    ItemOrder = c.ItemOrder ?? 0,
                    ItemDescription = c.ItemDescription,
                    IsRequired = c.IsRequired ?? true,
                    IsCompleted = c.IsCompleted ?? false,
                    CompletedBy = null,
                    CompletedByName = null,
                    CompletedDate = null,
                    Notes = null,
                    ImageUrl = null
                }).ToList();
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }

    #endregion

    #region Checklist Item Operations

    /// <summary>
    /// Update checklist item status, notes, and image
    /// FIX: Ensure navigation property is properly loaded
    /// </summary>
    public async Task<ChecklistItemDetailResponseDto> UpdateChecklistItemAsync(
        int itemId, UpdateChecklistItemStatusRequestDto request, int updatedBy,
        CancellationToken cancellationToken)
    {
        var item = await _context.Set<ChecklistItem>()
            .FirstOrDefaultAsync(c => c.ItemId == itemId, cancellationToken);

        if (item == null)
            throw new KeyNotFoundException($"Checklist item {itemId} not found");

        // Update fields if provided
        if (request.IsCompleted.HasValue)
        {
            item.IsCompleted = request.IsCompleted.Value;

            if (request.IsCompleted.Value)
            {
                item.CompletedBy = updatedBy;
                item.CompletedDate = DateTime.UtcNow;
            }
            else
            {
                item.CompletedBy = null;
                item.CompletedDate = null;
            }
        }

        if (request.Notes != null)
            item.Notes = request.Notes;

        if (request.ImageUrl != null)
            item.ImageUrl = request.ImageUrl;

        await _context.SaveChangesAsync(cancellationToken);

        // ? FIX: Reload the entity with navigation properties using a fresh query
        var updatedItem = await _context.Set<ChecklistItem>()
            .AsNoTracking()
            .Include(c => c.CompletedByNavigation)
            .FirstOrDefaultAsync(c => c.ItemId == itemId, cancellationToken);

        if (updatedItem == null)
            throw new InvalidOperationException($"Failed to reload checklist item {itemId}");

        return new ChecklistItemDetailResponseDto
        {
            ItemId = updatedItem.ItemId,
            ItemOrder = updatedItem.ItemOrder ?? 0,
            ItemDescription = updatedItem.ItemDescription,
            IsRequired = updatedItem.IsRequired ?? true,
            IsCompleted = updatedItem.IsCompleted ?? false,
            CompletedBy = updatedItem.CompletedBy,
            CompletedByName = updatedItem.CompletedByNavigation?.FullName,
            CompletedDate = updatedItem.CompletedDate,
            Notes = updatedItem.Notes,
            ImageUrl = updatedItem.ImageUrl
        };
    }

    /// <summary>
    /// Check if checklist item exists
    /// </summary>
    public async Task<bool> ChecklistItemExistsAsync(int itemId, CancellationToken cancellationToken)
    {
        return await _context.Set<ChecklistItem>()
            .AnyAsync(c => c.ItemId == itemId, cancellationToken);
    }

    #endregion

    #region NEW: Complete/Skip Operations

    /// <summary>
    /// Get checklist item by ID
    /// </summary>
    public async Task<ChecklistItemResponseDto?> GetChecklistItemByIdAsync(
        int itemId,
        CancellationToken cancellationToken = default)
    {
        var item = await _context.Set<ChecklistItem>()
            .AsNoTracking()
            .Include(c => c.CompletedByNavigation)
            .FirstOrDefaultAsync(c => c.ItemId == itemId, cancellationToken);

        if (item == null)
            return null;

        return new ChecklistItemResponseDto
        {
            ItemId = item.ItemId,
            WorkOrderId = item.WorkOrderId,
            TemplateId = item.TemplateId,
            ItemOrder = item.ItemOrder,
            ItemDescription = item.ItemDescription,
            IsRequired = item.IsRequired,
            IsCompleted = item.IsCompleted,
            CompletedBy = item.CompletedBy,
            CompletedByName = item.CompletedByNavigation?.FullName,
            CompletedDate = item.CompletedDate,
            Notes = item.Notes,
            ImageUrl = item.ImageUrl
        };
    }

    /// <summary>
    /// Complete checklist item with notes and image
    /// FIX: Ensure navigation property is properly loaded
    /// </summary>
    public async Task<ChecklistItemResponseDto> CompleteChecklistItemAsync(
        int itemId,
        int completedBy,
        string? notes,
        string? imageUrl,
        CancellationToken cancellationToken = default)
    {
        var item = await _context.Set<ChecklistItem>()
            .FirstOrDefaultAsync(c => c.ItemId == itemId, cancellationToken);

        if (item == null)
            throw new KeyNotFoundException($"Checklist item {itemId} not found");

        // Mark as completed
        item.IsCompleted = true;
        item.CompletedBy = completedBy;
        item.CompletedDate = DateTime.UtcNow;
        item.Notes = notes;
        item.ImageUrl = imageUrl;

        await _context.SaveChangesAsync(cancellationToken);

        // ? FIX: Reload with navigation using a fresh query instead of .Entry()
        var updatedItem = await _context.Set<ChecklistItem>()
            .AsNoTracking()
            .Include(c => c.CompletedByNavigation)
            .FirstOrDefaultAsync(c => c.ItemId == itemId, cancellationToken);

        if (updatedItem == null)
            throw new InvalidOperationException($"Failed to reload checklist item {itemId}");

        return new ChecklistItemResponseDto
        {
            ItemId = updatedItem.ItemId,
            WorkOrderId = updatedItem.WorkOrderId,
            TemplateId = updatedItem.TemplateId,
            ItemOrder = updatedItem.ItemOrder,
            ItemDescription = updatedItem.ItemDescription,
            IsRequired = updatedItem.IsRequired,
            IsCompleted = updatedItem.IsCompleted,
            CompletedBy = updatedItem.CompletedBy,
            CompletedByName = updatedItem.CompletedByNavigation?.FullName,
            CompletedDate = updatedItem.CompletedDate,
            Notes = updatedItem.Notes,
            ImageUrl = updatedItem.ImageUrl
        };
    }

    /// <summary>
    /// Update WorkOrder checklist progress
    /// </summary>
    public async Task UpdateWorkOrderChecklistProgressAsync(
        int workOrderId,
        int totalItems,
        int completedItems,
        CancellationToken cancellationToken = default)
    {
        var workOrder = await _context.Set<WorkOrder>()
            .FirstOrDefaultAsync(w => w.WorkOrderId == workOrderId, cancellationToken);

        if (workOrder == null)
            throw new KeyNotFoundException($"Work order {workOrderId} not found");

        workOrder.ChecklistTotal = totalItems;
        workOrder.ChecklistCompleted = completedItems;

        // Calculate progress percentage
        if (totalItems > 0)
        {
            workOrder.ProgressPercentage = Math.Round((completedItems / (decimal)totalItems) * 100, 2);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Get user by ID to retrieve FullName for completed by
    /// </summary>
    public async Task<User?> GetUserByIdAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);
    }

    #endregion
}
