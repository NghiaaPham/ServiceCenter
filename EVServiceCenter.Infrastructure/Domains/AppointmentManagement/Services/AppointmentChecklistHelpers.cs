using EVServiceCenter.Core.Domains.AppointmentManagement.Entities;
using EVServiceCenter.Core.Domains.Checklists.DTOs.Requests;
using EVServiceCenter.Core.Domains.Checklists.Interfaces;
using EVServiceCenter.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace EVServiceCenter.Infrastructure.Domains.AppointmentManagement.Services
{
    /// <summary>
    /// Helper class ?? t? ??ng t?o checklist t? template khi check-in
    /// </summary>
    public static class AppointmentChecklistHelpers
    {
        /// <summary>
        /// ? AUTO-GENERATE CHECKLIST t? Template khi t?o WorkOrder
        ///
        /// Flow:
        /// 1. L?y danh sách services trong appointment
        /// 2. V?i m?i service, tìm template phù h?p (Priority: Service ? Category ? Generic)
        /// 3. Apply template ? t?o ChecklistItems cho WorkOrder
        /// 4. Update WorkOrder.ChecklistTotal
        ///
        /// Priority:
        /// - Level 1: Service-specific template (ServiceId match)
        /// - Level 2: Category-specific template (CategoryId match)
        /// - Level 3: Generic template (ServiceId = null, CategoryId = null)
        /// </summary>
        public static async Task AutoApplyChecklistTemplateAsync(
            Appointment appointment,
            WorkOrder workOrder,
            IChecklistService checklistService,
            EVDbContext context,
            ILogger logger,
            CancellationToken cancellationToken = default)
        {
            logger.LogInformation(
                "?? AUTO-GENERATE CHECKLIST: WorkOrderId={WorkOrderId}, AppointmentId={AppointmentId}",
                workOrder.WorkOrderId, appointment.AppointmentId);

            try
            {
                // ========== STEP 1: LOAD APPOINTMENT SERVICES ==========
                var appointmentServices = await context.AppointmentServices
                    .Include(aps => aps.Service)
                        .ThenInclude(s => s!.Category)
                    .Where(aps => aps.AppointmentId == appointment.AppointmentId)
                    .ToListAsync(cancellationToken);

                if (!appointmentServices.Any())
                {
                    logger.LogWarning(
                        "?? No services found for appointment {AppointmentId}. " +
                        "No checklist will be created.",
                        appointment.AppointmentId);
                    return;
                }

                logger.LogInformation(
                    "?? Found {Count} services in appointment {AppointmentId}",
                    appointmentServices.Count, appointment.AppointmentId);

                // ========== STEP 2: FIND & APPLY TEMPLATE FOR EACH SERVICE ==========
                var allChecklistItems = new List<ChecklistItem>();
                var processedServices = new HashSet<int>(); // Avoid duplicate templates

                foreach (var appointmentService in appointmentServices)
                {
                    var serviceId = appointmentService.ServiceId;

                    // Skip if already processed (avoid duplicate checklists)
                    if (processedServices.Contains(serviceId))
                    {
                        logger.LogDebug(
                            "Service {ServiceId} already processed, skipping",
                            serviceId);
                        continue;
                    }

                    processedServices.Add(serviceId);

                    var service = appointmentService.Service;
                    if (service == null)
                    {
                        logger.LogWarning(
                            "?? Service {ServiceId} not loaded, skipping",
                            serviceId);
                        continue;
                    }

                    // ?? FIND BEST TEMPLATE (Priority: Service ? Category ? Generic)
                    var template = await FindBestTemplateAsync(
                        serviceId,
                        service.CategoryId,
                        context,
                        logger,
                        cancellationToken);

                    if (template == null)
                    {
                        logger.LogWarning(
                            "?? No template found for ServiceId={ServiceId}, CategoryId={CategoryId}. " +
                            "Skipping checklist creation.",
                            serviceId, service.CategoryId);
                        continue;
                    }

                    // ? APPLY TEMPLATE ? CREATE CHECKLIST ITEMS
                    var items = await CreateChecklistItemsFromTemplateAsync(
                        workOrder.WorkOrderId,
                        template,
                        context,
                        logger,
                        cancellationToken);

                    allChecklistItems.AddRange(items);

                    logger.LogInformation(
                        "? Applied template '{TemplateName}' (ID={TemplateId}) for service '{ServiceName}': {ItemCount} items",
                        template.TemplateName, template.TemplateId,
                        service.ServiceName, items.Count);
                }

                // ========== STEP 3: UPDATE WORKORDER STATS ==========
                if (allChecklistItems.Any())
                {
                    workOrder.ChecklistTotal = allChecklistItems.Count;
                    workOrder.ChecklistCompleted = 0;

                    await context.SaveChangesAsync(cancellationToken);

                    logger.LogInformation(
                        "? AUTO-GENERATE CHECKLIST COMPLETED: WorkOrderId={WorkOrderId}, " +
                        "Total Items={TotalItems}, Services Processed={ServiceCount}",
                        workOrder.WorkOrderId, allChecklistItems.Count, processedServices.Count);
                }
                else
                {
                    logger.LogWarning(
                        "?? No checklist items generated for WorkOrder {WorkOrderId}. " +
                        "Check if templates exist in database.",
                        workOrder.WorkOrderId);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "? Error auto-generating checklist for WorkOrder {WorkOrderId}",
                    workOrder.WorkOrderId);

                // Don't throw - checklist creation failure should not block check-in
                // Staff can manually add items later
            }
        }

        /// <summary>
        /// ?? Tìm template phù h?p nh?t theo Priority:
        /// 1. Service-specific (ServiceId match + IsActive)
        /// 2. Category-specific (CategoryId match + ServiceId NULL + IsActive)
        /// 3. Generic (ServiceId NULL + CategoryId NULL + IsActive)
        /// </summary>
        private static async Task<ChecklistTemplate?> FindBestTemplateAsync(
            int serviceId,
            int? categoryId,
            EVDbContext context,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            // Priority 1: Service-specific template
            var serviceTemplate = await context.ChecklistTemplates
                .FirstOrDefaultAsync(t =>
                    t.ServiceId == serviceId &&
                    t.IsActive == true,
                    cancellationToken);

            if (serviceTemplate != null)
            {
                logger.LogDebug(
                    "Found service-specific template: '{Name}' (ID={Id}) for ServiceId={ServiceId}",
                    serviceTemplate.TemplateName, serviceTemplate.TemplateId, serviceId);
                return serviceTemplate;
            }

            // Priority 2: Category-specific template
            if (categoryId.HasValue)
            {
                var categoryTemplate = await context.ChecklistTemplates
                    .FirstOrDefaultAsync(t =>
                        t.CategoryId == categoryId &&
                        t.ServiceId == null &&
                        t.IsActive == true,
                        cancellationToken);

                if (categoryTemplate != null)
                {
                    logger.LogDebug(
                        "Found category-specific template: '{Name}' (ID={Id}) for CategoryId={CategoryId}",
                        categoryTemplate.TemplateName, categoryTemplate.TemplateId, categoryId);
                    return categoryTemplate;
                }
            }

            // Priority 3: Generic template
            var genericTemplate = await context.ChecklistTemplates
                .FirstOrDefaultAsync(t =>
                    t.ServiceId == null &&
                    t.CategoryId == null &&
                    t.IsActive == true,
                    cancellationToken);

            if (genericTemplate != null)
            {
                logger.LogDebug(
                    "Found generic template: '{Name}' (ID={Id})",
                    genericTemplate.TemplateName, genericTemplate.TemplateId);
            }
            else
            {
                logger.LogWarning(
                    "?? No template found for ServiceId={ServiceId}, CategoryId={CategoryId}",
                    serviceId, categoryId);
            }

            return genericTemplate;
        }

        /// <summary>
        /// ? Parse JSON template Items và t?o ChecklistItems trong database
        /// </summary>
        private static async Task<List<ChecklistItem>> CreateChecklistItemsFromTemplateAsync(
            int workOrderId,
            ChecklistTemplate template,
            EVDbContext context,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            try
            {
                // Deserialize JSON template Items
                var templateItems = JsonSerializer.Deserialize<List<ChecklistTemplateItem>>(
                    template.Items);

                if (templateItems == null || !templateItems.Any())
                {
                    logger.LogWarning(
                        "Template {TemplateId} has no items or invalid JSON format",
                        template.TemplateId);
                    return new List<ChecklistItem>();
                }

                // Create ChecklistItems
                var checklistItems = new List<ChecklistItem>();

                foreach (var templateItem in templateItems.OrderBy(i => i.ItemOrder))
                {
                    var item = new ChecklistItem
                    {
                        WorkOrderId = workOrderId,
                        TemplateId = template.TemplateId,
                        ItemOrder = templateItem.ItemOrder,
                        ItemDescription = templateItem.Description,
                        IsRequired = templateItem.IsRequired,
                        IsCompleted = false,
                        CompletedBy = null,
                        CompletedDate = null,
                        Notes = null
                    };

                    checklistItems.Add(item);
                }

                // Save to database
                await context.ChecklistItems.AddRangeAsync(checklistItems, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);

                logger.LogInformation(
                    "? Created {Count} checklist items from template {TemplateId} for WorkOrder {WorkOrderId}",
                    checklistItems.Count, template.TemplateId, workOrderId);

                return checklistItems;
            }
            catch (JsonException ex)
            {
                logger.LogError(ex,
                    "? Failed to parse template Items JSON for TemplateId={TemplateId}",
                    template.TemplateId);
                return new List<ChecklistItem>();
            }
        }
    }

    /// <summary>
    /// DTO class ?? deserialize JSON template Items
    /// </summary>
    internal class ChecklistTemplateItem
    {
        public string Description { get; set; } = null!;
        public int ItemOrder { get; set; }
        public bool IsRequired { get; set; }
    }
}
