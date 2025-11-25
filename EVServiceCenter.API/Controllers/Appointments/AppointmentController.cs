using EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Request;
using EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Query;
using EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Response;
using EVServiceCenter.Core.Domains.AppointmentManagement.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Sockets;

namespace EVServiceCenter.API.Controllers.Appointments
{
    /// <summary>
    /// Customer Appointment API - For customers to manage their appointments
    /// </summary>
    [ApiController]
    [Route("api/appointments")]
    [Authorize(Policy = "CustomerOnly")] // Chỉ customer mới truy cập được
    [ApiExplorerSettings(GroupName = "Customer - Appointments")]
    public class AppointmentController : BaseController
    {
        private readonly IAppointmentCommandService _commandService;
        private readonly IAppointmentQueryService _queryService;
        private readonly ILogger<AppointmentController> _logger;

        public AppointmentController(
            IAppointmentCommandService commandService,
            IAppointmentQueryService queryService,
            ILogger<AppointmentController> logger)
        {
            _commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
            _queryService = queryService ?? throw new ArgumentNullException(nameof(queryService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// [Đặt lịch] Tạo lịch hẹn mới
        /// </summary>
        /// <remarks>
        /// Cho phép customer tạo lịch hẹn bảo dưỡng/sửa chữa xe điện.
        ///
        /// **Yêu cầu:**
        /// - Customer phải đăng nhập
        /// - Xe phải thuộc sở hữu của customer
        /// - Time slot phải còn trống và hợp lệ
        ///
        /// **Quy trình:**
        /// 1. Validate thông tin đầu vào
        /// 2. Kiểm tra xe thuộc về customer
        /// 3. Kiểm tra slot available
        /// 4. Tính giá dịch vụ theo model xe
        /// 5. Tạo appointment (status: Pending)
        /// 6. Tự động gửi email xác nhận
        /// </remarks>
        /// <param name="request">Thông tin đặt lịch</param>
        /// <returns>Thông tin lịch hẹn đã tạo</returns>
        [HttpPost]
        public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentRequestDto request)
        {
            if (!IsValidRequest(request))
            {
                return ValidationError("Dữ liệu đặt lịch không hợp lệ");
            }

            // Kiểm tra customer có quyền đặt lịch cho customerId này không
            var currentCustomerId = GetCurrentCustomerId();
            if (request.CustomerId != currentCustomerId)
            {
                return ForbiddenError("Bạn không có quyền đặt lịch cho khách hàng khác");
            }

            try
            {
                var result = await _commandService.CreateAsync(request, GetCurrentUserId());

                _logger.LogInformation(
                    "Customer {CustomerId} created appointment {AppointmentCode}",
                    currentCustomerId,
                    result.AppointmentCode);

                return CreatedAtAction(
                    nameof(GetAppointmentById),
                    new { id = result.AppointmentId },
                    new
                    {
                        Success = true,
                        Message = $"Đặt lịch thành công! Mã lịch hẹn: {result.AppointmentCode}",
                        Data = result,
                        Timestamp = DateTime.UtcNow
                    });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid appointment creation attempt by customer {CustomerId}", currentCustomerId);
                return ValidationError(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating appointment for customer {CustomerId}", currentCustomerId);
                return ServerError("Có lỗi xảy ra khi đặt lịch. Vui lòng thử lại sau.");
            }
        }

        /// <summary>
        /// [Xem chi tiết] Lấy thông tin lịch hẹn theo ID
        /// </summary>
        /// <remarks>
        /// Lấy thông tin chi tiết của một lịch hẹn cụ thể.
        ///
        /// **Bao gồm:**
        /// - Thông tin lịch hẹn (ngày giờ, trạng thái, mã code)
        /// - Thông tin xe và customer
        /// - Danh sách dịch vụ đã chọn
        /// - Giá tiền ước tính
        /// - Trung tâm dịch vụ
        ///
        /// **Phân quyền:**
        /// - Customer chỉ xem được lịch hẹn của mình
        /// </remarks>
        /// <param name="id">ID lịch hẹn</param>
        /// <returns>Chi tiết lịch hẹn</returns>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetAppointmentById(int id)
        {
            try
            {
                var result = await _queryService.GetByIdAsync(id);

                if (result == null)
                {
                    return NotFoundError($"Không tìm thấy lịch hẹn với ID {id}");
                }

                // Kiểm tra quyền truy cập: customer chỉ xem được lịch của mình
                var currentCustomerId = GetCurrentCustomerId();
                if (result.CustomerId != currentCustomerId)
                {
                    return ForbiddenError("Bạn không có quyền xem lịch hẹn này");
                }

                return Success(result, "Lấy thông tin lịch hẹn thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving appointment {AppointmentId}", id);
                return ServerError("Có lỗi xảy ra khi lấy thông tin lịch hẹn");
            }
        }

        /// <summary>
        /// [Xem danh sách] Lịch hẹn của tôi
        /// </summary>
        /// <remarks>
        /// Lấy tất cả lịch hẹn của customer đang đăng nhập.
        ///
        /// **Bao gồm:**
        /// - Tất cả lịch hẹn (mọi trạng thái)
        /// - Sắp xếp theo thời gian mới nhất
        /// - Hiển thị đầy đủ thông tin: mã code, trạng thái, thời gian, xe, dịch vụ
        ///
        /// **Use case:**
        /// - Xem lịch sử đặt lịch
        /// - Theo dõi trạng thái lịch hẹn
        /// - Quản lý các lịch đã đặt
        /// </remarks>
        /// <returns>Danh sách lịch hẹn</returns>
        [HttpGet("my-appointments")]
        public async Task<IActionResult> GetMyAppointments([FromQuery] AppointmentQueryDto query)
        {
            try
            {
                var customerId = GetCurrentCustomerId();

                // Force customer scope and sane defaults
                query ??= new AppointmentQueryDto();
                query.CustomerId = customerId;
                if (query.Page <= 0) query.Page = 1;
                if (query.PageSize <= 0) query.PageSize = 10;
                if (query.PageSize > 100) query.PageSize = 100; // Prevent fetching too much

                var result = await _queryService.GetPagedAsync(query);

                return Success(result, $"Tìm thấy {result.TotalCount} lịch hẹn");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving appointments for customer {CustomerId}", GetCurrentCustomerId());
                return ServerError("Có lỗi xảy ra khi lấy danh sách lịch hẹn");
            }
        }

        /// <summary>
        /// [Xem danh sách] Lịch hẹn sắp tới
        /// </summary>
        /// <remarks>
        /// Lấy danh sách các lịch hẹn sắp tới của customer (chưa diễn ra).
        ///
        /// **Tiêu chí:**
        /// - Chỉ lịch hẹn có thời gian >= hiện tại
        /// - Trạng thái: Pending, Confirmed, hoặc CheckedIn
        /// - Sắp xếp theo thời gian gần nhất trước
        ///
        /// **Use case:**
        /// - Hiển thị trên dashboard/home
        /// - Nhắc nhở lịch sắp tới
        /// - Quick view cho customer
        /// </remarks>
        /// <param name="limit">Số lượng lịch tối đa (mặc định 5)</param>
        /// <returns>Danh sách lịch hẹn sắp tới</returns>
        [HttpGet("my-appointments/upcoming")]
        public async Task<IActionResult> GetMyUpcomingAppointments([FromQuery] int limit = 5)
        {
            try
            {
                var customerId = GetCurrentCustomerId();
                // Use optimized DTO projection to improve performance
                var result = await _queryService.GetUpcomingByCustomerDtosAsync(customerId, limit);

                return Success(result, $"Tìm thấy {result.Count()} lịch hẹn sắp tới");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving upcoming appointments for customer {CustomerId}", GetCurrentCustomerId());
                return ServerError("Có lỗi xảy ra khi lấy danh sách lịch hẹn sắp tới");
            }
        }

        /// <summary>
        /// [Tra cứu] Tìm lịch hẹn theo mã
        /// </summary>
        /// <remarks>
        /// Tìm kiếm lịch hẹn bằng mã appointment code.
        ///
        /// **Format mã:**
        /// - APT + YYYYMMDD + 4 số tự tăng
        /// - VD: APT202510031234
        ///
        /// **Use case:**
        /// - Customer tra cứu lịch bằng mã đã nhận qua email/SMS
        /// - Tìm nhanh lịch hẹn cụ thể
        ///
        /// **Phân quyền:**
        /// - Customer chỉ tra được lịch của mình
        /// </remarks>
        /// <param name="code">Mã lịch hẹn (VD: APT202510031234)</param>
        /// <returns>Thông tin lịch hẹn</returns>
        [HttpGet("by-code/{code}")]
        public async Task<IActionResult> GetAppointmentByCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return ValidationError("Mã lịch hẹn không được để trống");
            }

            try
            {
                var result = await _queryService.GetByCodeAsync(code);

                if (result == null)
                {
                    return NotFoundError($"Không tìm thấy lịch hẹn với mã {code}");
                }

                // Kiểm tra quyền truy cập
                var currentCustomerId = GetCurrentCustomerId();
                if (result.CustomerId != currentCustomerId)
                {
                    return ForbiddenError("Bạn không có quyền xem lịch hẹn này");
                }

                return Success(result, "Lấy thông tin lịch hẹn thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving appointment by code {Code}", code);
                return ServerError("Có lỗi xảy ra khi lấy thông tin lịch hẹn");
            }
        }

        /// <summary>
        /// [Cập nhật] Sửa thông tin lịch hẹn
        /// </summary>
        /// <remarks>
        /// Cập nhật thông tin lịch hẹn đã tạo.
        ///
        /// **Có thể sửa:**
        /// - Ghi chú/yêu cầu đặc biệt
        /// - Danh sách dịch vụ
        /// - Priority
        ///
        /// **Không thể sửa:**
        /// - Thời gian (dùng API Reschedule thay vì)
        /// - Xe (phải hủy và tạo lịch mới)
        /// - Trung tâm (phải hủy và tạo lịch mới)
        ///
        /// **Điều kiện:**
        /// - Trạng thái: Pending hoặc Confirmed
        /// - Customer chỉ sửa được lịch của mình
        /// </remarks>
        /// <param name="id">ID lịch hẹn</param>
        /// <param name="request">Thông tin cập nhật</param>
        /// <returns>Lịch hẹn đã cập nhật</returns>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateAppointment(int id, [FromBody] UpdateAppointmentRequestDto request)
        {
            if (!IsValidRequest(request))
            {
                return ValidationError("Dữ liệu cập nhật không hợp lệ");
            }

            if (id != request.AppointmentId)
            {
                return ValidationError("ID trong URL không khớp với ID trong dữ liệu");
            }

            try
            {
                // Kiểm tra quyền: customer chỉ cập nhật được lịch của mình
                var existing = await _queryService.GetByIdAsync(id);
                if (existing == null)
                {
                    return NotFoundError($"Không tìm thấy lịch hẹn với ID {id}");
                }

                var currentCustomerId = GetCurrentCustomerId();
                if (existing.CustomerId != currentCustomerId)
                {
                    return ForbiddenError("Bạn không có quyền cập nhật lịch hẹn này");
                }

                var result = await _commandService.UpdateAsync(request, GetCurrentUserId());

                _logger.LogInformation(
                    "Customer {CustomerId} updated appointment {AppointmentId}",
                    currentCustomerId,
                    id);

                return Success(result, "Cập nhật lịch hẹn thành công");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid appointment update attempt for {AppointmentId}", id);
                return ValidationError(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating appointment {AppointmentId}", id);
                return ServerError("Có lỗi xảy ra khi cập nhật lịch hẹn");
            }
        }

        /// <summary>
        /// [Dời lịch] Đổi sang thời gian khác
        /// </summary>
        /// <remarks>
        /// Dời lịch hẹn sang slot thời gian mới.
        ///
        /// **Quy trình:**
        /// 1. Hủy lịch cũ (status → Rescheduled)
        /// 2. Giải phóng slot cũ
        /// 3. Tạo lịch mới với slot mới
        /// 4. Copy toàn bộ thông tin (xe, dịch vụ, ghi chú)
        /// 5. Gửi email thông báo
        ///
        /// **Yêu cầu:**
        /// - Lịch cũ phải ở trạng thái Pending/Confirmed
        /// - Slot mới phải còn trống
        /// - Phải cung cấp lý do dời lịch
        ///
        /// **Kết quả:**
        /// - Trả về appointment mới (với AppointmentCode mới)
        /// </remarks>
        /// <param name="id">ID lịch hẹn cũ</param>
        /// <param name="request">Thông tin dời lịch</param>
        /// <returns>Lịch hẹn mới</returns>
        [HttpPost("{id:int}/reschedule")]
        public async Task<IActionResult> RescheduleAppointment(int id, [FromBody] RescheduleAppointmentRequestDto request)
        {
            if (!IsValidRequest(request))
            {
                return ValidationError("Dữ liệu dời lịch không hợp lệ");
            }

            if (id != request.AppointmentId)
            {
                return ValidationError("ID trong URL không khớp với ID trong dữ liệu");
            }

            try
            {
                // Kiểm tra quyền
                var existing = await _queryService.GetByIdAsync(id);
                if (existing == null)
                {
                    return NotFoundError($"Không tìm thấy lịch hẹn với ID {id}");
                }

                var currentCustomerId = GetCurrentCustomerId();
                if (existing.CustomerId != currentCustomerId)
                {
                    return ForbiddenError("Bạn không có quyền dời lịch hẹn này");
                }

                var result = await _commandService.RescheduleAsync(request, GetCurrentUserId());

                _logger.LogInformation(
                    "Customer {CustomerId} rescheduled appointment {OldAppointmentId} to new appointment {NewAppointmentId}",
                    currentCustomerId,
                    id,
                    result.AppointmentId);

                return Success(result, $"Dời lịch thành công! Mã lịch mới: {result.AppointmentCode}");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid reschedule attempt for appointment {AppointmentId}", id);
                return ValidationError(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rescheduling appointment {AppointmentId}", id);
                return ServerError("Có lỗi xảy ra khi dời lịch hẹn");
            }
        }

        /// <summary>
        /// [Hủy lịch] Hủy lịch hẹn đã đặt
        /// </summary>
        /// <remarks>
        /// Hủy lịch hẹn đã đặt trước.
        ///
        /// **Quy trình:**
        /// 1. Kiểm tra điều kiện hủy
        /// 2. Cập nhật trạng thái → Cancelled
        /// 3. Giải phóng time slot
        /// 4. Lưu lý do hủy và thời gian hủy
        /// 5. Gửi email xác nhận hủy
        ///
        /// **Điều kiện:**
        /// - Chỉ hủy được lịch ở trạng thái Pending/Confirmed
        /// - Phải cung cấp lý do hủy
        ///
        /// **Lưu ý:**
        /// - Không thể undo sau khi hủy
        /// - Muốn đặt lại phải tạo appointment mới
        /// </remarks>
        /// <param name="id">ID lịch hẹn</param>
        /// <param name="request">Lý do hủy</param>
        /// <returns>Kết quả hủy lịch</returns>
        [HttpPost("{id:int}/cancel")]
        public async Task<IActionResult> CancelAppointment(int id, [FromBody] CancelAppointmentRequestDto request)
        {
            if (!IsValidRequest(request))
            {
                return ValidationError("Dữ liệu hủy lịch không hợp lệ");
            }

            if (id != request.AppointmentId)
            {
                return ValidationError("ID trong URL không khớp với ID trong dữ liệu");
            }

            try
            {
                // Kiểm tra quyền
                var existing = await _queryService.GetByIdAsync(id);
                if (existing == null)
                {
                    return NotFoundError($"Không tìm thấy lịch hẹn với ID {id}");
                }

                var currentCustomerId = GetCurrentCustomerId();
                if (existing.CustomerId != currentCustomerId)
                {
                    return ForbiddenError("Bạn không có quyền hủy lịch hẹn này");
                }

                var result = await _commandService.CancelAsync(request, GetCurrentUserId());

                _logger.LogInformation(
                    "Customer {CustomerId} cancelled appointment {AppointmentId}. Reason: {Reason}",
                    currentCustomerId,
                    id,
                    request.CancellationReason);

                return Success(new { AppointmentId = id, Cancelled = result }, "Hủy lịch hẹn thành công");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid cancel attempt for appointment {AppointmentId}", id);
                return ValidationError(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling appointment {AppointmentId}", id);
                return ServerError("Có lỗi xảy ra khi hủy lịch hẹn");
            }
        }

        /// <summary>
        /// [Thanh toán trước] Thanh toán cho lịch hẹn trước khi check-in
        /// </summary>
        /// <remarks>
        /// Tạo URL thanh toán cho lịch hẹn, cho phép khách hàng thanh toán trước khi đến trung tâm.
        ///
        /// **Quy trình:**
        /// 1. Kiểm tra lịch hẹn tồn tại và ở trạng thái Pending/Confirmed
        /// 2. Kiểm tra EstimatedCost > 0 (không thanh toán nếu miễn phí)
        /// 3. Kiểm tra chưa thanh toán
        /// 4. Tạo hoặc lấy PaymentIntent
        /// 5. Tạo Invoice tạm (pre-payment)
        /// 6. Tạo URL thanh toán từ VNPay/MoMo
        /// 7. Trả về URL để redirect khách hàng
        ///
        /// **Điều kiện:**
        /// - Lịch hẹn phải ở trạng thái Pending hoặc Confirmed
        /// - EstimatedCost > 0
        /// - PaymentStatus != "Completed"
        ///
        /// **Lợi ích:**
        /// - Khách hàng thanh toán trước, không cần chờ khi check-in
        /// - Giảm thời gian xử lý tại quầy
        /// - Đảm bảo thanh toán trước khi nhận dịch vụ
        /// </remarks>
        /// <param name="id">ID lịch hẹn</param>
        /// <param name="request">Thông tin thanh toán (ReturnUrl, PaymentMethod)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Payment URL và thông tin thanh toán</returns>
        [HttpPost("{id:int}/pay")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PayAppointment(
            int id,
            [FromBody] PayAppointmentRequestDto request,
            CancellationToken cancellationToken)
        {
            if (!IsValidRequest(request))
            {
                return ValidationError("Dữ liệu thanh toán không hợp lệ");
            }

            try
            {
                // Kiểm tra quyền
                var existing = await _queryService.GetByIdAsync(id);
                if (existing == null)
                {
                    return NotFoundError($"Không tìm thấy lịch hẹn với ID {id}");
                }

                var currentCustomerId = GetCurrentCustomerId();
                if (existing.CustomerId != currentCustomerId)
                {
                    return ForbiddenError("Bạn không có quyền thanh toán cho lịch hẹn này");
                }

                var clientIp = GetClientIpAddress();

                var result = await _commandService.CreatePrePaymentAsync(
                    id,
                    request.PaymentMethod,
                    request.ReturnUrl,
                    clientIp,
                    GetCurrentUserId(),
                    cancellationToken);

                _logger.LogInformation(
                    "Customer {CustomerId} initiated pre-payment for appointment {AppointmentId}. Invoice: {InvoiceCode}, Amount: {Amount}",
                    currentCustomerId,
                    id,
                    result.InvoiceCode,
                    result.Amount);

                return Success(result, "Tạo URL thanh toán thành công. Vui lòng chuyển đến cổng thanh toán.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid pre-payment attempt for appointment {AppointmentId}", id);
                return ValidationError(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating pre-payment for appointment {AppointmentId}", id);
                return ServerError("Có lỗi xảy ra khi tạo thanh toán");
            }
        }

        /// <summary>
        /// [Xóa] Xóa lịch hẹn (chỉ khi Pending)
        /// </summary>
        /// <remarks>
        /// Xóa hoàn toàn lịch hẹn khỏi hệ thống (hard delete).
        ///
        /// **Điều kiện:**
        /// - Chỉ xóa được lịch ở trạng thái Pending
        /// - Lịch chưa được confirm bởi Staff
        /// - Customer chỉ xóa được lịch của mình
        ///
        /// **Khác biệt Cancel vs Delete:**
        /// - **Cancel:** Giữ lại record, đánh dấu là Cancelled (có history)
        /// - **Delete:** Xóa hoàn toàn khỏi database (không có history)
        ///
        /// **Use case:**
        /// - Khách đặt nhầm ngay sau khi tạo
        /// - Test/demo cần xóa data
        /// </remarks>
        /// <param name="id">ID lịch hẹn</param>
        /// <returns>Kết quả xóa</returns>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteAppointment(int id)
        {
            try
            {
                // Kiểm tra quyền
                var existing = await _queryService.GetByIdAsync(id);
                if (existing == null)
                {
                    return NotFoundError($"Không tìm thấy lịch hẹn với ID {id}");
                }

                var currentCustomerId = GetCurrentCustomerId();
                if (existing.CustomerId != currentCustomerId)
                {
                    return ForbiddenError("Bạn không có quyền xóa lịch hẹn này");
                }

                var result = await _commandService.DeleteAsync(id, GetCurrentUserId());

                _logger.LogInformation(
                    "Customer {CustomerId} deleted appointment {AppointmentId}",
                    currentCustomerId,
                    id);

                return Success(new { AppointmentId = id, Deleted = result }, "Xóa lịch hẹn thành công");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid delete attempt for appointment {AppointmentId}", id);
                return ValidationError(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting appointment {AppointmentId}", id);
                return ServerError("Có lỗi xảy ra khi xóa lịch hẹn");
            }
        }

        private string GetClientIpAddress()
        {
            if (Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedValues))
            {
                var forwardedFor = forwardedValues.ToString();
                if (!string.IsNullOrWhiteSpace(forwardedFor))
                {
                    var firstCandidate = forwardedFor.Split(",")[0].Trim();
                    if (IPAddress.TryParse(firstCandidate, out var forwardedIp))
                    {
                        var normalizedForwarded = NormalizeIpAddress(forwardedIp);
                        // ✅ FIX: Don't fallback to 1.1.1.1, use valid Vietnam IP
                        if (!string.Equals(normalizedForwarded, "118.69.182.149", StringComparison.Ordinal))
                        {
                            return normalizedForwarded;
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(firstCandidate))
                    {
                        return firstCandidate;
                    }
                }
            }

            var remoteIp = HttpContext.Connection.RemoteIpAddress;
            return remoteIp != null
                ? NormalizeIpAddress(remoteIp)
                : "118.69.182.149"; // ✅ FIX: Use valid Vietnam IP instead of 1.1.1.1
        }

                private static string NormalizeIpAddress(IPAddress ipAddress)
        {
            if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
            {
                ipAddress = ipAddress.IsIPv4MappedToIPv6
                    ? ipAddress.MapToIPv4()
                    : null;
            }

            if (ipAddress == null || IPAddress.IsLoopback(ipAddress))
            {
                return "118.69.182.149";
            }

            return ipAddress.ToString();
        }

                private static string NormalizeIpAddress(string? ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
            {
                return "118.69.182.149";
            }

            return IPAddress.TryParse(ipAddress, out var parsed)
                ? NormalizeIpAddress(parsed)
                : ipAddress;
        }
    }
}


