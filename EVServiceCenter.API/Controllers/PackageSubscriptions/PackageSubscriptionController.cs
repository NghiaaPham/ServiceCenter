using EVServiceCenter.Core.Domains.PackageSubscriptions.DTOs.Requests;
using EVServiceCenter.Core.Domains.PackageSubscriptions.DTOs.Responses;
using EVServiceCenter.Core.Domains.PackageSubscriptions.Interfaces.Services;
using EVServiceCenter.Core.Domains.Shared.Models;
using EVServiceCenter.Core.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EVServiceCenter.Core.Domains.Payments.Interfaces;
using EVServiceCenter.Core.Domains.Payments.DTOs.Requests;
using EVServiceCenter.Core.Domains.MaintenancePackages.Interfaces.Repositories;

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
        private readonly IPaymentService _paymentService;
        private readonly IMaintenancePackageQueryRepository _packageQueryRepository;

        public PackageSubscriptionController(
            IPackageSubscriptionService service,
            IValidator<PurchasePackageRequestDto> purchaseValidator,
            ILogger<PackageSubscriptionController> logger,
            IPaymentService paymentService,
            IMaintenancePackageQueryRepository packageQueryRepository)
        {
            _service = service;
            _purchaseValidator = purchaseValidator;
            _logger = logger;
            _paymentService = paymentService;
            _packageQueryRepository = packageQueryRepository;
        }

        /// <summary>
        /// Lay danh sach dich vu dang duoc bao phu boi cac goi active cua xe.
        /// Có th�f filter theo status (Active, Expired, Cancelled,...)
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
                    "Có l�-i xảy ra khi lấy danh sách subscriptions", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Lay danh sach dich vu dang duoc bao phu boi cac goi active cua xe.
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
                        $"Không tìm thấy subscription v�>i ID: {id}"));
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
                    "Có l�-i xảy ra", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Lay danh sach dich vu dang duoc bao phu boi cac goi active cua xe.
        /// Xem �'ã dùng bao nhiêu, còn lại bao nhiêu lượt cho từng service
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
                    "Có l�-i xảy ra", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Lay subscriptions active cho 1 vehicle
        /// Dang khi customer book appointment, chon xe xong se hien subscriptions available
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
                    "Có l�-i xảy ra", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Lay danh sach dich vu duoc bao phu boi cac goi active cua xe.
        /// </summary>
        [HttpGet("vehicle/{vehicleId:int}/applicable-services")]
        public async Task<IActionResult> GetApplicableServicesForVehicle(
            int vehicleId,
            CancellationToken ct)
        {
            try
            {
                var customerId = GetCurrentCustomerId();
                var services = await _service.GetApplicableServicesForVehicleAsync(vehicleId, customerId, ct);

                var message = services.Count == 0
                    ? "Khong co dich vu nao trong goi cho xe nay"
                    : $"Tim thay {services.Count} dich vu da duoc bao gom trong goi";

                return Ok(ApiResponse<List<ApplicableServiceDto>>.WithSuccess(services, message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error getting applicable services for vehicle {VehicleId}",
                    vehicleId);

                return StatusCode(500, ApiResponse<List<ApplicableServiceDto>>.WithError(
                    "Co loi xay ra khi kiem tra dich vu mien phi", "INTERNAL_ERROR", 500));
            }
        }


        /// <summary>
        /// Mua/Subscribe vào 1 package
        /// Customer chọn package, �'iền thông tin thanh toán �?' tạo subscription
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
                var result = await _service.PurchasePackageAsync(
                    request,
                    customerId,
                    GetCurrentUserId(),
                    ct);

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
                    "Có l�-i xảy ra khi mua gói", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// �YOY MUA G�"I D�SCH VỤ V�sI THANH TOÁN (OPTION 3 - HYBRID)
        /// </summary>
        /// <remarks>
        /// **USE CASES:**
        /// 
        /// **Case 1: Online Payment (VNPay/MoMo)**
        /// ```json
        /// {
        ///   "packageId": 1,
        ///   "vehicleId": 10,
        ///   "paymentMethod": "VNPay",
        ///   "returnUrl": "https://frontend.com/subscription/payment-result"
        /// }
        /// ```
        /// �?' Response contains `paymentUrl`
        /// �?' Customer redirect to VNPay
        /// �?' After payment, callback updates subscription status
        /// �?' Customer redirects back to returnUrl
        /// 
        /// **Case 2: Cash Payment (Pay at counter)**
        /// ```json
        /// {
        ///   "packageId": 1,
        ///   "vehicleId": 10,
        ///   "paymentMethod": "Cash"
        /// }
        /// ```
        /// �?' Subscription created with Status = PendingPayment
        /// �?' Customer pays at service center
        /// �?' Staff confirms payment �?' Status = Active
        /// 
        /// **Case 3: Bank Transfer**
        /// ```json
        /// {
        ///   "packageId": 1,
        ///   "vehicleId": 10,
        ///   "paymentMethod": "BankTransfer"
        /// }
        /// ```
        /// �?' Subscription created with Status = PendingPayment
        /// �?' Customer transfers money
        /// �?' Staff verifies transaction �?' Status = Active
        /// 
        /// **FLOW:**
        /// 1. Get package details and validate
        /// 2. Create subscription with Status = PendingPayment (using package price)
        /// 3. If Online: Create payment URL �?' return to customer
        /// 4. If Cash/BankTransfer: Return subscription + invoice code
        /// 5. After payment confirmed: Update subscription Status = Active
        /// </remarks>
        /// <param name="request">Purchase request with payment method</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Subscription + Payment URL (if online) or Invoice code (if cash)</returns>
                [HttpPost("purchase-with-payment")]
        [ProducesResponseType(typeof(ApiResponse<PurchaseWithPaymentResponseDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> PurchaseWithPayment(
            [FromBody] PurchaseWithPaymentRequestDto request,
            CancellationToken ct)
        {
            try
            {
                var validMethods = new[] { "VNPay", "MoMo", "Cash", "BankTransfer" };
                if (!validMethods.Contains(request.PaymentMethod, StringComparer.OrdinalIgnoreCase))
                {
                    return BadRequest(ApiResponse<PurchaseWithPaymentResponseDto>.WithError(
                        $"Payment method is invalid. Supported: {string.Join(", ", validMethods)}",
                        "INVALID_PAYMENT_METHOD"));
                }

                var isOnlinePayment = request.PaymentMethod.Equals("VNPay", StringComparison.OrdinalIgnoreCase) ||
                                      request.PaymentMethod.Equals("MoMo", StringComparison.OrdinalIgnoreCase);

                if (isOnlinePayment && string.IsNullOrWhiteSpace(request.ReturnUrl))
                {
                    return BadRequest(ApiResponse<PurchaseWithPaymentResponseDto>.WithError(
                        "ReturnUrl is required for online payment",
                        "MISSING_RETURN_URL"));
                }

                var customerId = GetCurrentCustomerId();

                var package = await _packageQueryRepository.GetPackageByIdAsync(request.PackageId, ct);
                if (package == null)
                {
                    return NotFound(ApiResponse<PurchaseWithPaymentResponseDto>.WithError(
                        $"Kh�ng t�m th?y g�i v?i ID: {request.PackageId}",
                        "PACKAGE_NOT_FOUND"));
                }

                if (package.Status != PackageStatusEnum.Active)
                {
                    return BadRequest(ApiResponse<PurchaseWithPaymentResponseDto>.WithError(
                        "G�i d?ch v? d� ng?ng kinh doanh",
                        "PACKAGE_INACTIVE"));
                }

                var purchaseRequest = new PurchasePackageRequestDto
                {
                    PackageId = request.PackageId,
                    VehicleId = request.VehicleId,
                    CustomerNotes = request.CustomerNotes,
                    PaymentMethod = request.PaymentMethod,
                    PaymentTransactionId = null,
                    AmountPaid = package.TotalPriceAfterDiscount
                };

                var subscription = await _service.PurchasePackageAsync(
                    purchaseRequest,
                    customerId,
                    GetCurrentUserId(),
                    ct);

                if (isOnlinePayment && !subscription.InvoiceId.HasValue)
                {
                    _logger.LogError("Subscription {SubscriptionId} missing invoice reference", subscription.SubscriptionId);
                    return StatusCode(500, ApiResponse<PurchaseWithPaymentResponseDto>.WithError(
                        "Kh�ng th? t?o h�a don cho giao d?ch n�y. Vui l�ng th? l?i.",
                        "INVOICE_NOT_FOUND",
                        500));
                }

                var response = new PurchaseWithPaymentResponseDto
                {
                    Subscription = subscription,
                    PaymentMethod = request.PaymentMethod,
                    PaymentStatus = "Pending",
                    InvoiceId = subscription.InvoiceId,
                    InvoiceCode = subscription.InvoiceCode
                };

                if (isOnlinePayment)
                {
                    try
                    {
                        var paymentRequest = new CreatePaymentRequestDto
                        {
                            InvoiceId = subscription.InvoiceId!.Value,
                            Amount = subscription.PricePaid,
                            PaymentMethod = request.PaymentMethod,
                            ReturnUrl = request.ReturnUrl!,
                            CustomerName = subscription.CustomerName ?? "Customer",
                            CustomerEmail = $"customer{customerId}@evsc.com",
                            CustomerPhone = "0901234567"
                        };

                        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
                        var paymentResponse = await _paymentService.CreatePaymentAsync(
                            paymentRequest,
                            GetCurrentUserId(),
                            clientIp,
                            ct);

                        response.PaymentUrl = paymentResponse.PaymentUrl;
                        response.PaymentCode = paymentResponse.PaymentCode;
                        response.QrCodeUrl = paymentResponse.QrCodeUrl;
                        response.DeepLink = paymentResponse.DeepLink;
                        response.PaymentExpiresAt = paymentResponse.ExpiryTime;
                        response.Message = $"Vui l�ng thanh to�n {subscription.PricePaid:N0}d qua {request.PaymentMethod}.";
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to create payment URL for subscription {SubscriptionId}", subscription.SubscriptionId);
                        response.PaymentStatus = "Failed";
                        response.Message = "Kh�ng th? t?o link thanh to�n. Vui l�ng th? l?i ho?c ch?n phuong th?c kh�c.";
                        return StatusCode(500, ApiResponse<PurchaseWithPaymentResponseDto>.WithSuccess(response, response.Message));
                    }
                }
                else
                {
                    if (request.PaymentMethod.Equals("Cash", StringComparison.OrdinalIgnoreCase))
                    {
                        response.Message = $"�� t?o subscription #{subscription.SubscriptionId}. Vui l�ng thanh to�n {subscription.PricePaid:N0}d t?i qu?y (H�a don {subscription.InvoiceCode ?? "N/A"}).";
                    }
                    else
                    {
                        response.Message = $"�� t?o subscription #{subscription.SubscriptionId}. Vui l�ng chuy?n kho?n {subscription.PricePaid:N0}d. N?i dung: SUB{subscription.SubscriptionId}. H�a don {subscription.InvoiceCode ?? "N/A"}.";
                    }
                }

                return CreatedAtAction(
                    nameof(GetSubscriptionDetails),
                    new { id = subscription.SubscriptionId },
                    ApiResponse<PurchaseWithPaymentResponseDto>.WithSuccess(response, response.Message, 201));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Business rule violation while purchasing package");
                return BadRequest(ApiResponse<PurchaseWithPaymentResponseDto>.WithError(
                    ex.Message, "BUSINESS_RULE_VIOLATION"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error purchasing package with payment");
                return StatusCode(500, ApiResponse<PurchaseWithPaymentResponseDto>.WithError(
                    "C� l?i x?y ra khi mua g�i", "INTERNAL_ERROR", 500));
            }
        }        /// <summary>
        /// Hủy subscription
        /// Customer có th�f hủy subscription của mình
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
                        "Lý do hủy không �'ược �'�f tr�'ng", "VALIDATION_ERROR"));
                }

                var result = await _service.CancelSubscriptionAsync(
                    id, request.CancellationReason, customerId, ct);

                if (!result)
                {
                    return NotFound(ApiResponse<object>.WithNotFound(
                        $"Không tìm thấy subscription v�>i ID: {id}"));
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
                    "Có l�-i xảy ra khi hủy subscription", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Tạm dừng subscription
        /// Customer có th�f tạm dừng khi: xe �'ang sửa chữa l�>n, �'i công tác dài hạn
        /// Staff có th�f tạm dừng khi: phát hi�?n gian lận, vi phạm chính sách
        /// </summary>
        /// <param name="id">ID của subscription cần tạm dừng</param>
        /// <param name="request">Lý do tạm dừng (bắt bu�Tc)</param>
        /// <param name="ct">Cancellation token</param>
        [HttpPost("{id:int}/suspend")]
        [Authorize(Policy = "CustomerOrStaff")] // Customer hoặc Staff �'ều có th�f suspend
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
                            $"Không tìm thấy subscription v�>i ID: {id}"));
                    }

                    if (subscription.CustomerId != customerId)
                    {
                        return StatusCode(403, ApiResponse<object>.WithError(
                            "Bạn không có quyền tạm dừng subscription này", "FORBIDDEN", 403));
                    }
                }

                // Gọi service �'�f suspend
                var result = await _service.SuspendSubscriptionAsync(id, request.Reason, ct);

                if (!result)
                {
                    return BadRequest(ApiResponse<object>.WithError(
                        "Không th�f tạm dừng subscription (có th�f �'ã b�< hủy hoặc hết hạn)", 
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
                    "Có l�-i xảy ra khi tạm dừng subscription", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Kích hoạt lại subscription �'ã b�< tạm dừng
        /// Ch�? reactivate �'ược subscription �'ang �Y trạng thái Suspended
        /// </summary>
        /// <param name="id">ID của subscription cần kích hoạt lại</param>
        /// <param name="ct">Cancellation token</param>
        [HttpPost("{id:int}/reactivate")]
        [Authorize(Policy = "CustomerOrStaff")] // Customer hoặc Staff �'ều có th�f reactivate
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
                            $"Không tìm thấy subscription v�>i ID: {id}"));
                    }

                    if (subscription.CustomerId != customerId)
                    {
                        return StatusCode(403, ApiResponse<object>.WithError(
                            "Bạn không có quyền kích hoạt lại subscription này", "FORBIDDEN", 403));
                    }

                    // Check expiry date (customer không th�f reactivate subscription �'ã hết hạn)
                    if (subscription.ExpiryDate.HasValue && 
                        subscription.ExpiryDate.Value < DateTime.UtcNow)
                    {
                        return BadRequest(ApiResponse<object>.WithError(
                            $"Không th�f kích hoạt lại subscription �'ã hết hạn vào {subscription.ExpiryDate.Value:dd/MM/yyyy}. " +
                            "Vui lòng mua gói m�>i.",
                            "SUBSCRIPTION_EXPIRED"));
                    }
                }

                // Gọi service �'�f reactivate
                var result = await _service.ReactivateSubscriptionAsync(id, ct);

                if (!result)
                {
                    return BadRequest(ApiResponse<object>.WithError(
                        "Không th�f kích hoạt lại subscription", 
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
                    "Có l�-i xảy ra khi kích hoạt lại subscription", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Thanh toán cho package subscription qua VNPay/MoMo
        /// </summary>
        /// <remarks>
        /// Tạo payment URL �'�f customer thanh toán cho subscription �'ã mua�?,
        ///
        /// **Flow:**
        /// 1. Customer mua package �?' subscription �'ược tạo (POST /purchase)
        /// 2. Customer gọi endpoint này v�>i subscriptionId
        /// 3. H�? th�'ng tạo payment URL (VNPay hoặc MoMo)
        /// 4. Customer redirect �'ến gateway thanh toán
        /// 5. Sau khi thanh toán, callback về /api/payments/vnpay-callback
        /// 6. Payment status �'ược cập nhật
        ///
        /// **Payment Methods:**
        /// - VNPay: Ví �'i�?n tử VNPay
        /// - MoMo: Ví �'i�?n tử MoMo
        /// - (Cash/BankTransfer: Thanh toán tại quầy, không qua endpoint này)
        ///
        /// **Return URL:**
        /// - Sau khi thanh toán, customer sẽ redirect về returnUrl
        /// - Frontend check payment status và hi�fn th�< kết quả
        /// </remarks>
        /// <param name="subscriptionId">ID của subscription cần thanh toán</param>
        /// <param name="request">Payment method và return URL</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Payment URL �'�f redirect customer</returns>
        [HttpPost("{subscriptionId:int}/pay")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> CreatePaymentForSubscription(
            int subscriptionId,
            [FromBody] CreateSubscriptionPaymentRequestDto request,
            CancellationToken ct)
        {
            try
            {
                var customerId = GetCurrentCustomerId();

                // 1. Validate subscription exists and belongs to customer
                var subscription = await _service.GetSubscriptionDetailsAsync(subscriptionId, customerId, ct);

                if (subscription == null)
                {
                    return NotFound(ApiResponse<object>.WithNotFound(
                        $"Không tìm thấy subscription v�>i ID: {subscriptionId}"));
                }

                if (subscription.CustomerId != customerId)
                {
                    return StatusCode(403, ApiResponse<object>.WithError(
                        "Bạn không có quyền thanh toán cho subscription này", "FORBIDDEN", 403));
                }

                // 2. Validate payment method
                var validMethods = new[] { "VNPay", "MoMo" };
                if (!validMethods.Contains(request.PaymentMethod, StringComparer.OrdinalIgnoreCase))
                {
                    return BadRequest(ApiResponse<object>.WithError(
                        $"Phương thức thanh toán không hợp l�?. Ch�? h�- trợ: {string.Join(", ", validMethods)}",
                        "INVALID_PAYMENT_METHOD"));
                }

                // 3. Get customer info for payment
                // Note: Subscription should include customer details in SubscriptionResponseDto
                var customerName = subscription.CustomerName ?? "Customer";
                var customerEmail = $"customer{customerId}@evservicecenter.com"; // Fallback email
                var customerPhone = "0901234567"; // Fallback phone

                // 4. Create payment request
                var paymentRequest = new CreatePaymentRequestDto
                {
                    // For subscription, we don't have InvoiceId yet
                    // We'll use a special reference to subscription
                    Amount = subscription.PricePaid,
                    PaymentMethod = request.PaymentMethod,
                    ReturnUrl = request.ReturnUrl,
                    CustomerName = customerName,
                    CustomerEmail = customerEmail,
                    CustomerPhone = customerPhone
                };

                // 5. Get client IP
                var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";

                // 6. Create payment via PaymentService
                var paymentResponse = await _paymentService.CreatePaymentAsync(
                    paymentRequest,
                    GetCurrentUserId(),
                    clientIp,
                    ct);
                _logger.LogInformation(
                    "Created payment for subscription {SubscriptionId}: Amount={Amount}, Method={Method}, PaymentCode={PaymentCode}",
                    subscriptionId, subscription.PricePaid, request.PaymentMethod, paymentResponse.PaymentCode);

                // 7. Return payment URL
                return Ok(ApiResponse<object>.WithSuccess(new
                {
                    subscriptionId,
                    paymentCode = paymentResponse.PaymentCode,
                    paymentUrl = paymentResponse.PaymentUrl,
                    amount = subscription.PricePaid,
                    gateway = paymentResponse.Gateway,
                    qrCodeUrl = paymentResponse.QrCodeUrl,
                    deepLink = paymentResponse.DeepLink,
                    expiryTime = paymentResponse.ExpiryTime
                }, "Tạo payment URL thành công. Redirect customer �'ến paymentUrl"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.WithError(ex.Message, "BUSINESS_RULE_VIOLATION"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment for subscription {SubscriptionId}", subscriptionId);
                return StatusCode(500, ApiResponse<object>.WithError(
                    "Có l�-i xảy ra khi tạo thanh toán", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Request body cho cancel subscription
        /// </summary>
        public class CancelSubscriptionRequest
        {
            public string CancellationReason { get; set; } = null!;
        }

        /// <summary>
        /// �o. NEW: Request DTO cho thanh toán subscription
        /// </summary>
        public class CreateSubscriptionPaymentRequestDto
        {
            /// <summary>
            /// Phương thức thanh toán: VNPay, MoMo
            /// </summary>
            public string PaymentMethod { get; set; } = "VNPay";

            /// <summary>
            /// URL �'�f redirect sau khi thanh toán
            /// Frontend sẽ check payment status tại URL này
            /// </summary>
            public string ReturnUrl { get; set; } = null!;
        }
    }
}

