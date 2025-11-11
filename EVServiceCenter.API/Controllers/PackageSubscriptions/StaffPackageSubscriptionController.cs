using EVServiceCenter.Core.Domains.PackageSubscriptions.DTOs.Requests;
using EVServiceCenter.Core.Domains.PackageSubscriptions.Interfaces.Services;
using EVServiceCenter.Core.Domains.Shared.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVServiceCenter.API.Controllers.PackageSubscriptions
{
    /// <summary>
    /// ?? [STAFF ONLY] Staff Package Subscription Management
    /// Endpoints cho staff qu?n lý subscription payments
    /// </summary>
    [ApiController]
    [Route("api/staff/package-subscriptions")]
    [Authorize(Policy = "AdminOrStaff")]
    [ApiExplorerSettings(GroupName = "Staff - Package Subscriptions")]
    public class StaffPackageSubscriptionController : BaseController
    {
        private readonly IPackageSubscriptionService _service;
        private readonly IValidator<ConfirmPaymentRequestDto> _confirmPaymentValidator;
        private readonly ILogger<StaffPackageSubscriptionController> _logger;

        public StaffPackageSubscriptionController(
            IPackageSubscriptionService service,
            IValidator<ConfirmPaymentRequestDto> confirmPaymentValidator,
            ILogger<StaffPackageSubscriptionController> logger)
        {
            _service = service;
            _confirmPaymentValidator = confirmPaymentValidator;
            _logger = logger;
        }

        /// <summary>
        /// ?? [PHASE 1] Xác nh?n thanh toán Cash/BankTransfer
        /// </summary>
        /// <remarks>
        /// **USE CASES:**
        ///
        /// **Case 1: Cash Payment**
        /// ```json
        /// {
        ///   "subscriptionId": 123,
        ///   "paymentMethod": "Cash",
        ///   "paidAmount": 3000000,
        ///   "notes": "Thanh toán t?i qu?y lúc 14:50"
        /// }
        /// ```
        /// ? Subscription Status: PendingPayment ? Active
        ///
        /// **Case 2: Bank Transfer**
        /// ```json
        /// {
        ///   "subscriptionId": 123,
        ///   "paymentMethod": "BankTransfer",
        ///   "paidAmount": 3000000,
        ///   "bankTransactionId": "FT25100312345",
        ///   "transferDate": "2025-10-03T14:50:00Z",
        ///   "notes": "Verified from bank statement"
        /// }
        /// ```
        /// ? Subscription Status: PendingPayment ? Active
        ///
        /// **FLOW:**
        /// 1. Customer mua subscription ? Status = PendingPayment
        /// 2. Customer thanh toán cash t?i qu?y HO?C chuy?n kho?n
        /// 3. Staff verify payment (check ti?n, check bank statement)
        /// 4. Staff g?i endpoint này ?? confirm
        /// 5. Subscription Status ? Active
        /// 6. Customer có th? s? d?ng subscription
        ///
        /// **VALIDATIONS:**
        /// - Subscription ph?i ? tr?ng thái PendingPayment
        /// - PaidAmount >= Subscription TotalPrice
        /// - PaymentMethod = "Cash" ho?c "BankTransfer"
        /// - N?u BankTransfer: BankTransactionId và TransferDate là b?t bu?c
        /// </remarks>
        /// <param name="request">Thông tin xác nh?n thanh toán</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Success response</returns>
        [HttpPost("confirm-payment")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> ConfirmPayment(
            [FromBody] ConfirmPaymentRequestDto request,
            CancellationToken ct)
        {
            // Validate request
            var validation = await _confirmPaymentValidator.ValidateAsync(request, ct);
            if (!validation.IsValid)
            {
                var errors = validation.Errors.Select(e => new
                {
                    Field = e.PropertyName,
                    Message = e.ErrorMessage
                });
                return BadRequest(ApiResponse<object>.WithValidationError(errors));
            }

            try
            {
                var staffUserId = GetCurrentUserId();

                _logger.LogInformation(
                    "?? Staff {StaffId} confirming payment for subscription {SubscriptionId} via {PaymentMethod}",
                    staffUserId, request.SubscriptionId, request.PaymentMethod);

                var result = await _service.ConfirmPaymentAsync(request, staffUserId, ct);

                if (!result)
                {
                    return NotFound(ApiResponse<object>.WithNotFound(
                        $"Không tìm th?y subscription v?i ID: {request.SubscriptionId}"));
                }

                _logger.LogInformation(
                    "? Payment confirmed successfully: Subscription {SubscriptionId} ? Active",
                    request.SubscriptionId);

                return Ok(ApiResponse<object>.WithSuccess(new
                {
                    subscriptionId = request.SubscriptionId,
                    confirmed = true,
                    confirmedDate = DateTime.UtcNow,
                    confirmedBy = staffUserId,
                    paymentMethod = request.PaymentMethod,
                    paidAmount = request.PaidAmount
                }, $"Xác nh?n thanh toán thành công. Subscription ?ã ???c kích ho?t."));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, 
                    "Business rule violation while confirming payment for subscription {SubscriptionId}",
                    request.SubscriptionId);
                return BadRequest(ApiResponse<object>.WithError(
                    ex.Message, "BUSINESS_RULE_VIOLATION"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Error confirming payment for subscription {SubscriptionId}",
                    request.SubscriptionId);
                return StatusCode(500, ApiResponse<object>.WithError(
                    "Có l?i x?y ra khi xác nh?n thanh toán", "INTERNAL_ERROR", 500));
            }
        }
    }
}
