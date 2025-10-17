using EVServiceCenter.Core.Domains.PackageSubscriptions.DTOs.Requests;
using EVServiceCenter.Core.Domains.PackageSubscriptions.DTOs.Responses;
using EVServiceCenter.Core.Domains.PackageSubscriptions.Interfaces.Services;
using EVServiceCenter.Core.Domains.Shared.Models;
using EVServiceCenter.Core.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVServiceCenter.API.Controllers.PackageSubscriptions
{
    /// <summary>
    /// Controller quản lý Package Subscriptions
    /// Customer mua gói, xem danh sách, hủy subscription
    /// </summary>
    [ApiController]
    [Route("api/package-subscriptions")]
    [Authorize(Policy = "CustomerOnly")]
    [ApiExplorerSettings(GroupName = "Customer - Package Subscriptions")]
    public class PackageSubscriptionController : BaseController
    {
        private readonly IPackageSubscriptionService _service;
        private readonly IValidator<PurchasePackageRequestDto> _purchaseValidator;
        private readonly ILogger<PackageSubscriptionController> _logger;

        public PackageSubscriptionController(
            IPackageSubscriptionService service,
            IValidator<PurchasePackageRequestDto> purchaseValidator,
            ILogger<PackageSubscriptionController> logger)
        {
            _service = service;
            _purchaseValidator = purchaseValidator;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách subscriptions của customer hiện tại
        /// Có thể filter theo status (Active, Expired, Cancelled,...)
        /// </summary>
        [HttpGet("my-subscriptions")]
        public async Task<IActionResult> GetMySubscriptions(
            [FromQuery] SubscriptionStatusEnum? statusFilter,
            CancellationToken ct)
        {
            try
            {
                var customerId = GetCurrentCustomerId();
                var result = await _service.GetMySubscriptionsAsync(customerId, statusFilter, ct);

                return Ok(ApiResponse<List<PackageSubscriptionSummaryDto>>.WithSuccess(
                    result,
                    $"Tìm thấy {result.Count} subscriptions"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subscriptions for customer");
                return StatusCode(500, ApiResponse<List<PackageSubscriptionSummaryDto>>.WithError(
                    "Có lỗi xảy ra khi lấy danh sách subscriptions", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Lấy chi tiết đầy đủ của 1 subscription
        /// Include tất cả service usages
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetSubscriptionDetails(int id, CancellationToken ct)
        {
            try
            {
                var customerId = GetCurrentCustomerId();
                var result = await _service.GetSubscriptionDetailsAsync(id, customerId, ct);

                if (result == null)
                {
                    return NotFound(ApiResponse<PackageSubscriptionResponseDto>.WithNotFound(
                        $"Không tìm thấy subscription với ID: {id}"));
                }

                return Ok(ApiResponse<PackageSubscriptionResponseDto>.WithSuccess(result));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ApiResponse<PackageSubscriptionResponseDto>.WithError(
                    ex.Message, "FORBIDDEN", 403));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subscription details {Id}", id);
                return StatusCode(500, ApiResponse<PackageSubscriptionResponseDto>.WithError(
                    "Có lỗi xảy ra", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Lấy usage details của subscription
        /// Xem đã dùng bao nhiêu, còn lại bao nhiêu lượt cho từng service
        /// </summary>
        [HttpGet("{id:int}/usage")]
        public async Task<IActionResult> GetSubscriptionUsage(int id, CancellationToken ct)
        {
            try
            {
                var customerId = GetCurrentCustomerId();
                var result = await _service.GetSubscriptionUsageDetailsAsync(id, customerId, ct);

                return Ok(ApiResponse<List<PackageServiceUsageDto>>.WithSuccess(
                    result,
                    $"Tìm thấy {result.Count} services trong subscription"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ApiResponse<List<PackageServiceUsageDto>>.WithError(
                    ex.Message, "FORBIDDEN", 403));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting usage for subscription {Id}", id);
                return StatusCode(500, ApiResponse<List<PackageServiceUsageDto>>.WithError(
                    "Có lỗi xảy ra", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Lấy subscriptions active cho 1 vehicle
        /// Dùng khi customer book appointment, chọn xe xong sẽ hiện subscriptions available
        /// </summary>
        [HttpGet("vehicle/{vehicleId:int}/active")]
        public async Task<IActionResult> GetActiveSubscriptionsForVehicle(
            int vehicleId,
            CancellationToken ct)
        {
            try
            {
                var customerId = GetCurrentCustomerId();
                var result = await _service.GetActiveSubscriptionsForVehicleAsync(
                    vehicleId, customerId, ct);

                return Ok(ApiResponse<List<PackageSubscriptionSummaryDto>>.WithSuccess(
                    result,
                    $"Tìm thấy {result.Count} subscriptions active"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active subscriptions for vehicle {VehicleId}", vehicleId);
                return StatusCode(500, ApiResponse<List<PackageSubscriptionSummaryDto>>.WithError(
                    "Có lỗi xảy ra", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Mua/Subscribe vào 1 package
        /// Customer chọn package, điền thông tin thanh toán → tạo subscription
        /// </summary>
        [HttpPost("purchase")]
        public async Task<IActionResult> PurchasePackage(
            [FromBody] PurchasePackageRequestDto request,
            CancellationToken ct)
        {
            var validation = await _purchaseValidator.ValidateAsync(request, ct);
            if (!validation.IsValid)
            {
                var errors = validation.Errors.Select(e => new
                {
                    Field = e.PropertyName,
                    Message = e.ErrorMessage
                });
                return BadRequest(ApiResponse<PackageSubscriptionResponseDto>.WithValidationError(errors));
            }

            try
            {
                var customerId = GetCurrentCustomerId();
                var result = await _service.PurchasePackageAsync(request, customerId, ct);

                _logger.LogInformation(
                    "Customer {CustomerId} purchased package {PackageId}, created subscription {SubscriptionId}",
                    customerId, request.PackageId, result.SubscriptionId);

                return CreatedAtAction(
                    nameof(GetSubscriptionDetails),
                    new { id = result.SubscriptionId },
                    ApiResponse<PackageSubscriptionResponseDto>.WithSuccess(
                        result,
                        "Mua gói thành công",
                        201));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<PackageSubscriptionResponseDto>.WithError(
                    ex.Message, "BUSINESS_RULE_VIOLATION"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error purchasing package");
                return StatusCode(500, ApiResponse<PackageSubscriptionResponseDto>.WithError(
                    "Có lỗi xảy ra khi mua gói", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Hủy subscription
        /// Customer có thể hủy subscription của mình
        /// </summary>
        [HttpPost("{id:int}/cancel")]
        public async Task<IActionResult> CancelSubscription(
            int id,
            [FromBody] CancelSubscriptionRequest request,
            CancellationToken ct)
        {
            try
            {
                var customerId = GetCurrentCustomerId();

                if (string.IsNullOrWhiteSpace(request.CancellationReason))
                {
                    return BadRequest(ApiResponse<object>.WithError(
                        "Lý do hủy không được để trống", "VALIDATION_ERROR"));
                }

                var result = await _service.CancelSubscriptionAsync(
                    id, request.CancellationReason, customerId, ct);

                if (!result)
                {
                    return NotFound(ApiResponse<object>.WithNotFound(
                        $"Không tìm thấy subscription với ID: {id}"));
                }

                _logger.LogInformation("Customer {CustomerId} cancelled subscription {SubscriptionId}",
                    customerId, id);

                return Ok(ApiResponse<object>.WithSuccess(null, "Hủy subscription thành công"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ApiResponse<object>.WithError(ex.Message, "FORBIDDEN", 403));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.WithError(ex.Message, "BUSINESS_RULE_VIOLATION"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling subscription {Id}", id);
                return StatusCode(500, ApiResponse<object>.WithError(
                    "Có lỗi xảy ra khi hủy subscription", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Tạm dừng subscription
        /// Customer có thể tạm dừng khi: xe đang sửa chữa lớn, đi công tác dài hạn
        /// Staff có thể tạm dừng khi: phát hiện gian lận, vi phạm chính sách
        /// </summary>
        /// <param name="id">ID của subscription cần tạm dừng</param>
        /// <param name="request">Lý do tạm dừng (bắt buộc)</param>
        /// <param name="ct">Cancellation token</param>
        [HttpPost("{id:int}/suspend")]
        [Authorize(Policy = "CustomerOrStaff")] // Customer hoặc Staff đều có thể suspend
        public async Task<IActionResult> SuspendSubscription(
            int id,
            [FromBody] SuspendSubscriptionRequestDto request,
            CancellationToken ct)
        {
            try
            {
                var customerId = GetCurrentCustomerId();
                bool isStaff = User.IsInRole("Staff") || User.IsInRole("Admin");

                // Nếu là customer, validate ownership
                if (!isStaff)
                {
                    var subscription = await _service.GetSubscriptionDetailsAsync(id, customerId, ct);
                    if (subscription == null)
                    {
                        return NotFound(ApiResponse<object>.WithNotFound(
                            $"Không tìm thấy subscription với ID: {id}"));
                    }

                    if (subscription.CustomerId != customerId)
                    {
                        return StatusCode(403, ApiResponse<object>.WithError(
                            "Bạn không có quyền tạm dừng subscription này", "FORBIDDEN", 403));
                    }
                }

                // Gọi service để suspend
                var result = await _service.SuspendSubscriptionAsync(id, request.Reason, ct);

                if (!result)
                {
                    return BadRequest(ApiResponse<object>.WithError(
                        "Không thể tạm dừng subscription (có thể đã bị hủy hoặc hết hạn)", 
                        "BUSINESS_RULE_VIOLATION"));
                }

                _logger.LogInformation(
                    "Subscription {SubscriptionId} suspended by user {UserId} (Staff: {IsStaff}). Reason: {Reason}",
                    id, customerId, isStaff, request.Reason);

                return Ok(ApiResponse<object>.WithSuccess(new
                {
                    subscriptionId = id,
                    suspended = true,
                    suspendedDate = DateTime.UtcNow,
                    reason = request.Reason
                }, "Tạm dừng subscription thành công"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.WithError(ex.Message, "BUSINESS_RULE_VIOLATION"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ApiResponse<object>.WithError(ex.Message, "FORBIDDEN", 403));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error suspending subscription {Id}", id);
                return StatusCode(500, ApiResponse<object>.WithError(
                    "Có lỗi xảy ra khi tạm dừng subscription", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Kích hoạt lại subscription đã bị tạm dừng
        /// Chỉ reactivate được subscription đang ở trạng thái Suspended
        /// </summary>
        /// <param name="id">ID của subscription cần kích hoạt lại</param>
        /// <param name="ct">Cancellation token</param>
        [HttpPost("{id:int}/reactivate")]
        [Authorize(Policy = "CustomerOrStaff")] // Customer hoặc Staff đều có thể reactivate
        public async Task<IActionResult> ReactivateSubscription(int id, CancellationToken ct)
        {
            try
            {
                var customerId = GetCurrentCustomerId();
                bool isStaff = User.IsInRole("Staff") || User.IsInRole("Admin");

                // Nếu là customer, validate ownership và expiry
                if (!isStaff)
                {
                    var subscription = await _service.GetSubscriptionDetailsAsync(id, customerId, ct);
                    if (subscription == null)
                    {
                        return NotFound(ApiResponse<object>.WithNotFound(
                            $"Không tìm thấy subscription với ID: {id}"));
                    }

                    if (subscription.CustomerId != customerId)
                    {
                        return StatusCode(403, ApiResponse<object>.WithError(
                            "Bạn không có quyền kích hoạt lại subscription này", "FORBIDDEN", 403));
                    }

                    // Check expiry date (customer không thể reactivate subscription đã hết hạn)
                    if (subscription.ExpiryDate.HasValue && 
                        subscription.ExpiryDate.Value < DateTime.UtcNow)
                    {
                        return BadRequest(ApiResponse<object>.WithError(
                            $"Không thể kích hoạt lại subscription đã hết hạn vào {subscription.ExpiryDate.Value:dd/MM/yyyy}. " +
                            "Vui lòng mua gói mới.",
                            "SUBSCRIPTION_EXPIRED"));
                    }
                }

                // Gọi service để reactivate
                var result = await _service.ReactivateSubscriptionAsync(id, ct);

                if (!result)
                {
                    return BadRequest(ApiResponse<object>.WithError(
                        "Không thể kích hoạt lại subscription", 
                        "BUSINESS_RULE_VIOLATION"));
                }

                _logger.LogInformation(
                    "Subscription {SubscriptionId} reactivated by user {UserId} (Staff: {IsStaff})",
                    id, customerId, isStaff);

                return Ok(ApiResponse<object>.WithSuccess(new
                {
                    subscriptionId = id,
                    reactivated = true,
                    reactivatedDate = DateTime.UtcNow
                }, "Kích hoạt lại subscription thành công"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.WithError(ex.Message, "BUSINESS_RULE_VIOLATION"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ApiResponse<object>.WithError(ex.Message, "FORBIDDEN", 403));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reactivating subscription {Id}", id);
                return StatusCode(500, ApiResponse<object>.WithError(
                    "Có lỗi xảy ra khi kích hoạt lại subscription", "INTERNAL_ERROR", 500));
            }
        }
    }

    /// <summary>
    /// Request body cho cancel subscription
    /// </summary>
    public class CancelSubscriptionRequest
    {
        public string CancellationReason { get; set; } = null!;
    }
}
