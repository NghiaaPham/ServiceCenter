using EVServiceCenter.API.Controllers;
using EVServiceCenter.Core.Domains.Checklists.DTOs.Requests;
using EVServiceCenter.Core.Domains.Checklists.DTOs.Responses;
using EVServiceCenter.Core.Domains.Checklists.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVServiceCenter.API.Controllers.Checklists
{
    /// <summary>
    /// Controller for checklist management - Work Order checklists
    /// </summary>
    [ApiController]
    [Route("api/checklists")]
    [Authorize]
    [ApiExplorerSettings(GroupName = "Staff - Checklists")]
    public class ChecklistController : BaseController
    {
        private readonly IChecklistService _checklistService;
        private readonly ILogger<ChecklistController> _logger;

        public ChecklistController(
            IChecklistService checklistService,
            ILogger<ChecklistController> logger)
        {
            _checklistService = checklistService ?? throw new ArgumentNullException(nameof(checklistService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// [Xem checklist] L?y checklist c?a WorkOrder
        /// </summary>
        /// <remarks>
        /// L?y t?t c? checklist items c?a work order v?i tr?ng thái hoàn thành.
        ///
        /// **Bao g?m:**
        /// - T?t c? checklist items (theo th? t? ItemOrder)
        /// - Tr?ng thái completed c?a t?ng item
        /// - Ng??i complete và th?i gian complete
        /// - Ghi chú và ?nh minh ch?ng
        /// - T?ng s? items và s? l??ng ?ã completed
        /// - Completion percentage
        ///
        /// **Use case:**
        /// - Technician xem checklist c?n làm
        /// - Staff theo dõi ti?n ?? work order
        /// - QC ki?m tra công vi?c ?ã completed
        /// </remarks>
        /// <param name="workOrderId">ID c?a work order</param>
        [HttpGet("work-orders/{workOrderId:int}")]
        [Authorize(Policy = "AllInternal")] // Admin, Staff, Technician
        public async Task<IActionResult> GetWorkOrderChecklist(int workOrderId)
        {
            try
            {
                var checklist = await _checklistService.GetWorkOrderChecklistAsync(
                    workOrderId, CancellationToken.None);

                return Success(checklist, "L?y checklist thành công");
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFoundError(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting work order checklist");
                return ServerError("Có l?i x?y ra khi l?y checklist");
            }
        }

        /// <summary>
        /// [Complete Item] Technician tick ? checklist item
        /// </summary>
        /// <remarks>
        /// ?ánh d?u m?t checklist item là ?ã hoàn thành.
        ///
        /// **Quy trình:**
        /// 1. Technician làm xong m?t công vi?c (vd: Ki?m tra d?u phanh)
        /// 2. Tick ? item trong checklist
        /// 3. Có th? ghi thêm notes và upload ?nh minh ch?ng
        /// 4. System t? ??ng:
        ///    - Update IsCompleted = true
        ///    - Ghi CompletedBy = current user
        ///    - Ghi CompletedDate = now
        ///    - C?p nh?t WorkOrder.ProgressPercentage
        ///    - C?p nh?t WorkOrder.ChecklistCompleted count
        ///
        /// **Phân quy?n:**
        /// - Technician, Staff, Admin
        /// </remarks>
        /// <param name="request">Thông tin complete item</param>
        [HttpPost("items/complete")]
        [Authorize(Policy = "AllInternal")] // Admin, Staff, Technician
        public async Task<IActionResult> CompleteChecklistItem(
            [FromBody] CompleteChecklistItemRequestDto request)
        {
            if (!IsValidRequest(request))
                return ValidationError("D? li?u không h?p l?");

            try
            {
                var currentUserId = GetCurrentUserId();

                var result = await _checklistService.CompleteChecklistItemAsync(
                    request, currentUserId, CancellationToken.None);

                _logger.LogInformation(
                    "? User {UserId} completed checklist item {ItemId}: {Description}",
                    currentUserId, result.ItemId, result.ItemDescription);

                return Success(result, "?ã hoàn thành checklist item");
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFoundError(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex.Message);
                return ValidationError(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing checklist item");
                return ServerError("Có l?i x?y ra khi hoàn thành item");
            }
        }

        /// <summary>
        /// [Skip Item] Skip checklist item v?i lý do (optional items only)
        /// </summary>
        /// <remarks>
        /// Skip m?t checklist item không b?t bu?c v?i lý do.
        ///
        /// **Quy trình:**
        /// 1. Technician g?p item không th?/không c?n làm (vd: "Ki?m tra pin 12V" - xe không có)
        /// 2. Click "Skip" và nh?p lý do
        /// 3. System validate:
        ///    - Item ph?i là optional (IsRequired = false)
        ///    - Required items KHÔNG th? skip
        /// 4. Mark item as completed v?i note "[SKIPPED] {reason}"
        ///
        /// **Use cases:**
        /// - Item không áp d?ng cho xe này (vd: M?t s? xe không có pin 12V ph?)
        /// - Khách hàng t? ch?i làm thêm (vd: không mu?n v? sinh n?i th?t)
        /// - Thi?u ph? tùng t?m th?i
        ///
        /// **Phân quy?n:**
        /// - Technician, Staff, Admin
        /// </remarks>
        /// <param name="request">Thông tin skip item</param>
        [HttpPost("items/skip")]
        [Authorize(Policy = "AllInternal")]
        public async Task<IActionResult> SkipChecklistItem(
            [FromBody] SkipChecklistItemRequestDto request)
        {
            if (!IsValidRequest(request))
                return ValidationError("D? li?u không h?p l?");

            try
            {
                var currentUserId = GetCurrentUserId();

                var result = await _checklistService.SkipChecklistItemAsync(
                    request, currentUserId, CancellationToken.None);

                _logger.LogInformation(
                    "?? User {UserId} skipped checklist item {ItemId}: {Reason}",
                    currentUserId, result.ItemId, request.SkipReason);

                return Success(result, "?ã skip checklist item");
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFoundError(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex.Message);
                return ValidationError(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error skipping checklist item");
                return ServerError("Có l?i x?y ra khi skip item");
            }
        }

        /// <summary>
        /// [Validate] Ki?m tra xem WorkOrder có th? complete không
        /// </summary>
        /// <remarks>
        /// Validate t?t c? required checklist items ?ã ???c completed.
        ///
        /// **Validation rules:**
        /// - T?t c? items có IsRequired = true ph?i IsCompleted = true
        /// - Optional items (IsRequired = false) có th? skip
        ///
        /// **Response:**
        /// - CanComplete: true n?u có th? complete work order
        /// - MissingItems: Danh sách các required items ch?a completed
        ///
        /// **Use case:**
        /// - Frontend disable nút "Complete WorkOrder" n?u CanComplete = false
        /// - Backend validation tr??c khi complete work order
        /// - Hi?n th? warning cho user: "Còn X items b?t bu?c ch?a xong"
        /// </remarks>
        /// <param name="workOrderId">ID c?a work order</param>
        [HttpGet("work-orders/{workOrderId:int}/validate")]
        [Authorize(Policy = "AllInternal")]
        public async Task<IActionResult> ValidateWorkOrderCompletion(int workOrderId)
        {
            try
            {
                var (canComplete, missingItems) = await _checklistService
                    .ValidateWorkOrderCompletionAsync(workOrderId, CancellationToken.None);

                var response = new
                {
                    WorkOrderId = workOrderId,
                    CanComplete = canComplete,
                    MissingItemsCount = missingItems.Count,
                    MissingItems = missingItems
                };

                if (canComplete)
                {
                    return Success(response, "? Work order có th? complete");
                }
                else
                {
                    return Success(response, 
                        $"?? Còn {missingItems.Count} checklist items b?t bu?c ch?a hoàn thành");
                }
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFoundError(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating work order completion");
                return ServerError("Có l?i x?y ra khi validate");
            }
        }
    }
}
