using EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Query;
using EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Request;
using EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Response;
using EVServiceCenter.Core.Domains.AppointmentManagement.Interfaces.Services;
using EVServiceCenter.Core.Domains.Shared.Models;
using EVServiceCenter.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVServiceCenter.API.Controllers.Appointments
{
    /// <summary>
    /// Appointment Management API - For Staff/Admin to manage all appointments
    /// </summary>
    [ApiController]
    [Route("api/appointment-management")]
    [Authorize(Policy = "AllInternal")] // Admin, Staff, Technician
    [ApiExplorerSettings(GroupName = "Staff - Appointments")]
    public class AppointmentManagementController : BaseController
    {
        private readonly IAppointmentCommandService _commandService;
        private readonly IAppointmentQueryService _queryService;
        private readonly IServiceSourceAuditService _auditService;
        private readonly ILogger<AppointmentManagementController> _logger;

        public AppointmentManagementController(
            IAppointmentCommandService commandService,
            IAppointmentQueryService queryService,
            IServiceSourceAuditService auditService,
            ILogger<AppointmentManagementController> logger)
        {
            _commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
            _queryService = queryService ?? throw new ArgumentNullException(nameof(queryService));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// [Xem danh sách] Tất cả lịch hẹn (có filter/sort/paging)
        /// </summary>
        /// <remarks>
        /// Lấy danh sách tất cả lịch hẹn trong hệ thống với các tùy chọn filter, sort, paging.
        ///
        /// **Có thể filter theo:**
        /// - Trạng thái (Pending, Confirmed, CheckedIn, InProgress, Completed, Cancelled, Rescheduled, NoShow)
        /// - Trung tâm dịch vụ (ServiceCenterId)
        /// - Khách hàng (CustomerId)
        /// - Khoảng thời gian (DateFrom, DateTo)
        /// - Priority (Low, Normal, High, Urgent)
        /// - Source (Online, Phone, WalkIn)
        ///
        /// **Hỗ trợ:**
        /// - Pagination (Page, PageSize)
        /// - Sorting (SortBy, IsDescending)
        /// - Search (tìm theo mã appointment, tên khách, biển số xe)
        ///
        /// **Use case:**
        /// - Dashboard tổng quan
        /// - Lọc lịch theo điều kiện
        /// - Export danh sách
        /// </remarks>
        /// <param name="query">Query parameters</param>
        /// <returns>Danh sách lịch hẹn phân trang</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllAppointments([FromQuery] AppointmentQueryDto query)
        {
            try
            {
                var result = await _queryService.GetPagedAsync(query);

                return Ok(new
                {
                    Success = true,
                    Message = $"Tìm thấy {result.TotalCount} lịch hẹn",
                    Data = result,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving appointments with query {@Query}", query);
                return ServerError("Có lỗi xảy ra khi lấy danh sách lịch hẹn");
            }
        }

        /// <summary>
        /// [Xem chi tiết] Lấy thông tin lịch hẹn theo ID
        /// </summary>
        /// <remarks>
        /// Lấy thông tin chi tiết đầy đủ của lịch hẹn (bao gồm cả WorkOrder nếu có).
        ///
        /// **Bao gồm:**
        /// - Thông tin lịch hẹn đầy đủ
        /// - Thông tin khách hàng (tên, SĐT, email)
        /// - Thông tin xe (biển số, model, hãng)
        /// - Danh sách dịch vụ đã chọn + giá
        /// - Trung tâm dịch vụ và slot thời gian
        /// - WorkOrder (nếu đã check-in)
        /// - Lịch sử thay đổi trạng thái
        ///
        /// **Phân quyền:**
        /// - Staff/Admin xem được tất cả lịch hẹn
        /// </remarks>
        /// <param name="id">ID lịch hẹn</param>
        /// <returns>Chi tiết đầy đủ lịch hẹn</returns>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetAppointmentDetail(int id)
        {
            try
            {
                var result = await _queryService.GetByIdAsync(id);

                if (result == null)
                {
                    return NotFoundError($"Không tìm thấy lịch hẹn với ID {id}");
                }

                return Success(result, "Lấy chi tiết lịch hẹn thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving appointment detail {AppointmentId}", id);
                return ServerError("Có lỗi xảy ra khi lấy chi tiết lịch hẹn");
            }
        }

        /// <summary>
        /// [Xem danh sách] Lịch hẹn theo trung tâm và ngày
        /// </summary>
        /// <remarks>
        /// Lấy danh sách lịch hẹn của một trung tâm trong một ngày cụ thể.
        ///
        /// **Use case:**
        /// - Quản lý slot thời gian
        /// - Xem lịch làm việc hàng ngày
        /// - Phân công kỹ thuật viên
        /// - Kiểm tra tải công việc
        ///
        /// **Format ngày:**
        /// - yyyy-MM-dd (VD: 2025-10-03)
        ///
        /// **Kết quả:**
        /// - Danh sách lịch hẹn trong ngày
        /// - Sắp xếp theo thời gian slot
        /// - Hiển thị trạng thái từng lịch
        /// </remarks>
        /// <param name="serviceCenterId">ID trung tâm</param>
        /// <param name="date">Ngày (yyyy-MM-dd)</param>
        /// <returns>Danh sách lịch hẹn trong ngày</returns>
        [HttpGet("by-service-center/{serviceCenterId:int}/date/{date}")]
        public async Task<IActionResult> GetAppointmentsByServiceCenterAndDate(
            int serviceCenterId,
            string date)
        {
            if (!DateOnly.TryParse(date, out var parsedDate))
            {
                return ValidationError("Định dạng ngày không hợp lệ. Vui lòng dùng yyyy-MM-dd");
            }

            try
            {
                var result = await _queryService.GetByServiceCenterAndDateAsync(serviceCenterId, parsedDate);

                return Success(result, $"Tìm thấy {result.Count()} lịch hẹn ngày {parsedDate:dd/MM/yyyy}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving appointments for service center {ServiceCenterId} on {Date}",
                    serviceCenterId, parsedDate);
                return ServerError("Có lỗi xảy ra khi lấy danh sách lịch hẹn");
            }
        }

        /// <summary>
        /// [Tra cứu] Lịch hẹn theo khách hàng
        /// </summary>
        /// <remarks>
        /// Xem toàn bộ lịch sử đặt lịch của một khách hàng cụ thể.
        ///
        /// **Bao gồm:**
        /// - Tất cả lịch hẹn (mọi trạng thái)
        /// - Sắp xếp theo thời gian mới nhất
        /// - Hiển thị đầy đủ thông tin từng lịch
        ///
        /// **Use case:**
        /// - Staff tra cứu lịch sử khách hàng
        /// - Hỗ trợ tư vấn cho khách quen
        /// - Xem tần suất bảo dưỡng
        /// - Phân tích hành vi khách hàng
        /// </remarks>
        /// <param name="customerId">ID khách hàng</param>
        /// <returns>Danh sách lịch hẹn của khách hàng</returns>
        [HttpGet("by-customer/{customerId:int}")]
        public async Task<IActionResult> GetAppointmentsByCustomer(int customerId)
        {
            try
            {
                var result = await _queryService.GetByCustomerIdAsync(customerId);

                return Success(result, $"Tìm thấy {result.Count()} lịch hẹn của khách hàng");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving appointments for customer {CustomerId}", customerId);
                return ServerError("Có lỗi xảy ra khi lấy lịch hẹn của khách hàng");
            }
        }

        /// <summary>
        /// [Xác nhận] Confirm lịch hẹn (Pending → Confirmed)
        /// </summary>
        /// <remarks>
        /// Xác nhận lịch hẹn sau khi liên hệ với khách hàng.
        ///
        /// **Quy trình:**
        /// 1. Lịch hẹn ở trạng thái Pending
        /// 2. Staff liên hệ khách qua Phone/Email/SMS/In-Person
        /// 3. Khách xác nhận sẽ đến
        /// 4. Staff click Confirm
        /// 5. Trạng thái chuyển → Confirmed
        /// 6. Lock slot (không cho đặt trùng)
        /// 7. Gửi email/SMS xác nhận
        ///
        /// **Confirmation Method:**
        /// - Phone: Gọi điện xác nhận
        /// - Email: Gửi email và khách reply
        /// - SMS: Nhắn tin và nhận phản hồi
        /// - In-Person: Khách đến trực tiếp
        ///
        /// **Phân quyền:**
        /// - Chỉ Admin/Staff mới confirm được
        /// </remarks>
        /// <param name="id">ID lịch hẹn</param>
        /// <param name="request">Thông tin xác nhận</param>
        /// <returns>Kết quả xác nhận</returns>
        [HttpPost("{id:int}/confirm")]
        [Authorize(Policy = "AdminOrStaff")] // Chỉ Admin/Staff mới confirm được
        public async Task<IActionResult> ConfirmAppointment(
            int id,
            [FromBody] ConfirmAppointmentRequestDto request)
        {
            if (!IsValidRequest(request))
            {
                return ValidationError("Dữ liệu xác nhận không hợp lệ");
            }

            if (id != request.AppointmentId)
            {
                return ValidationError("ID trong URL không khớp với ID trong dữ liệu");
            }

            try
            {
                var result = await _commandService.ConfirmAsync(request, GetCurrentUserId());

                _logger.LogInformation(
                    "Staff {UserId} confirmed appointment {AppointmentId} via {Method}",
                    GetCurrentUserId(),
                    id,
                    request.ConfirmationMethod);

                return Success(
                    new { AppointmentId = id, Confirmed = result },
                    "Xác nhận lịch hẹn thành công");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid confirm attempt for appointment {AppointmentId}", id);
                return ValidationError(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming appointment {AppointmentId}", id);
                return ServerError("Có lỗi xảy ra khi xác nhận lịch hẹn");
            }
        }

        /// <summary>
        /// [NoShow] Đánh dấu khách không đến
        /// </summary>
        /// <remarks>
        /// Đánh dấu lịch hẹn là NoShow khi khách không đến đúng hẹn.
        ///
        /// **Khi nào dùng:**
        /// - Khách đã Confirm nhưng không đến
        /// - Đã quá thời gian hẹn 15-30 phút
        /// - Không liên lạc được với khách
        ///
        /// **Quy trình:**
        /// 1. Đợi khách quá 15-30 phút
        /// 2. Gọi điện xác nhận (không nghe máy)
        /// 3. Staff đánh dấu NoShow
        /// 4. Giải phóng slot
        /// 5. Có thể penalty customer (giảm priority)
        ///
        /// **Điều kiện:**
        /// - Trạng thái: Confirmed hoặc CheckedIn
        /// - Quá thời gian appointment ít nhất 15 phút
        ///
        /// **Kết quả:**
        /// - Status → NoShow
        /// - Slot được release
        /// - Ghi log vào lịch sử khách hàng
        /// </remarks>
        /// <param name="id">ID lịch hẹn</param>
        /// <returns>Kết quả</returns>
        [HttpPost("{id:int}/mark-no-show")]
        [Authorize(Policy = "AdminOrStaff")]
        public async Task<IActionResult> MarkAsNoShow(int id)
        {
            try
            {
                var result = await _commandService.MarkAsNoShowAsync(id, GetCurrentUserId());

                _logger.LogInformation(
                    "Staff {UserId} marked appointment {AppointmentId} as NoShow",
                    GetCurrentUserId(),
                    id);

                return Success(
                    new { AppointmentId = id, NoShow = result },
                    "Đã đánh dấu khách không đến");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid NoShow marking for appointment {AppointmentId}", id);
                return ValidationError(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking appointment {AppointmentId} as NoShow", id);
                return ServerError("Có lỗi xảy ra khi đánh dấu NoShow");
            }
        }

        /// <summary>
        /// [Check-in] Check-in khách hàng khi đến
        /// </summary>
        /// <remarks>
        /// Check-in khách hàng khi họ đến trung tâm dịch vụ.
        ///
        /// **Quy trình:**
        /// 1. Khách hàng đến trung tâm
        /// 2. Lễ tân check thông tin (SĐT/mã lịch hẹn)
        /// 3. Click Check-in
        /// 4. Status → CheckedIn
        /// 5. Tạo WorkOrder (phiếu công việc)
        /// 6. Giao xe cho kỹ thuật viên
        ///
        /// **Điều kiện:**
        /// - Trạng thái: Confirmed
        /// - Trong khoảng thời gian hợp lệ (không quá sớm/muộn)
        ///
        /// **Sau khi check-in:**
        /// - Tự động tạo WorkOrder
        /// - Kỹ thuật viên bắt đầu làm việc
        /// - WorkOrder tracking bảo dưỡng/sửa chữa
        ///
        /// **Lưu ý:**
        /// - Tính năng đang phát triển (TODO)
        /// </remarks>
        /// <param name="id">ID lịch hẹn</param>
        /// <returns>Kết quả check-in</returns>
        [HttpPost("{id:int}/check-in")]
        [Authorize(Policy = "AdminOrStaff")]
        public async Task<IActionResult> CheckInAppointment(int id)
        {
            try
            {
                var result = await _commandService.CheckInAsync(id, GetCurrentUserId());

                _logger.LogInformation(
                    "Staff {UserId} checked in appointment {AppointmentId}",
                    GetCurrentUserId(),
                    id);

                return Success(result, "Check-in thành công. WorkOrder đã được tạo.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid check-in attempt for appointment {AppointmentId}", id);
                return ValidationError(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking in appointment {AppointmentId}", id);
                return ServerError("Có lỗi xảy ra khi check-in");
            }
        }

        /// <summary>
        /// [Thêm dịch vụ] Thêm dịch vụ phát sinh khi đang InProgress
        /// </summary>
        /// <remarks>
        /// Thêm dịch vụ phát sinh khi kỹ thuật viên phát hiện vấn đề trong lúc kiểm tra.
        ///
        /// **Quy trình:**
        /// 1. Kỹ thuật viên kiểm tra xe
        /// 2. Phát hiện vấn đề cần sửa (vd: phanh mòn, lốp hư)
        /// 3. Tư vấn khách hàng
        /// 4. Khách đồng ý thêm dịch vụ
        /// 5. Staff thêm dịch vụ vào appointment
        /// 6. Tạo PaymentIntent mới cho phần phát sinh
        /// 7. Khách thanh toán phần phát sinh
        ///
        /// **Điều kiện:**
        /// - Appointment phải ở trạng thái InProgress
        /// - Danh sách serviceIds không được rỗng
        ///
        /// **Kết quả:**
        /// - AppointmentServices được thêm vào
        /// - EstimatedCost và EstimatedDuration được cập nhật
        /// - PaymentIntent mới được tạo
        /// - PaymentStatus chuyển về Pending (vì có thêm tiền cần trả)
        ///
        /// **Use case:**
        /// - Phanh mòn cần thay
        /// - Lốp xe hư cần đổi
        /// - Phát hiện rò rỉ dầu
        /// - Cần thêm bảo dưỡng khác
        /// </remarks>
        /// <param name="id">ID lịch hẹn</param>
        /// <param name="request">Danh sách service IDs cần thêm</param>
        /// <returns>Appointment đã cập nhật</returns>
        [HttpPost("{id:int}/add-services")]
        [Authorize(Policy = "AdminOrStaff")]
        public async Task<IActionResult> AddServices(
            int id,
            [FromBody] AddServicesRequestDto request)
        {
            if (!IsValidRequest(request) || !request.ServiceIds.Any())
            {
                return ValidationError("Danh sách dịch vụ không được rỗng");
            }

            try
            {
                var result = await _commandService.AddServicesAsync(
                    id,
                    request.ServiceIds,
                    GetCurrentUserId());

                _logger.LogInformation(
                    "Staff {UserId} added {Count} services to appointment {AppointmentId}",
                    GetCurrentUserId(),
                    request.ServiceIds.Count,
                    id);

                return Success(result, $"Đã thêm {request.ServiceIds.Count} dịch vụ phát sinh. PaymentIntent mới đã được tạo.");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid service IDs for appointment {AppointmentId}", id);
                return ValidationError(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid add services attempt for appointment {AppointmentId}", id);
                return ValidationError(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding services to appointment {AppointmentId}", id);
                return ServerError("Có lỗi xảy ra khi thêm dịch vụ");
            }
        }

        /// <summary>
        /// [Thanh toán] Tạo PaymentIntent mới cho lịch hẹn
        /// </summary>
        /// <remarks>
        /// Dùng khi cần khởi tạo lại intent thanh toán (ví dụ: khách yêu cầu thanh toán lại,
        /// bổ sung phí sau khi điều chỉnh dịch vụ, hoặc intent trước đó đã hết hạn).
        /// </remarks>
        /// <param name="id">ID lịch hẹn</param>
        /// <param name="request">Thông tin tạo intent</param>
        [HttpPost("{id:int}/payments/create-intent")]
        [Authorize(Policy = "AdminOrStaff")]
        public async Task<IActionResult> CreatePaymentIntent(
            int id,
            [FromBody] CreatePaymentIntentRequestDto request)
        {
            if (!IsValidRequest(request))
            {
                return ValidationError("Thông tin tạo payment intent không hợp lệ");
            }

            if (id != request.AppointmentId)
            {
                return ValidationError("ID lịch hẹn trong URL không khớp với dữ liệu gửi lên");
            }

            try
            {
                var result = await _commandService.CreatePaymentIntentAsync(request, GetCurrentUserId());

                return Success(result, "Đã tạo payment intent mới cho lịch hẹn");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid payment intent creation for appointment {AppointmentId}", id);
                return ValidationError(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment intent for appointment {AppointmentId}", id);
                return ServerError("Có lỗi xảy ra khi tạo payment intent mới");
            }
        }

        /// <summary>
        /// [Thanh toán] Danh sách PaymentIntent của lịch hẹn
        /// </summary>
        /// <param name="id">ID lịch hẹn</param>
        [HttpGet("{id:int}/payments/intents")]
        [Authorize(Policy = "AdminOrStaff")]
        public async Task<IActionResult> GetPaymentIntents(int id)
        {
            try
            {
                var intents = await _queryService.GetPaymentIntentsAsync(id);

                return Success(intents, $"Tìm thấy {intents.Count} payment intent cho lịch hẹn");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment intents for appointment {AppointmentId}", id);
                return ServerError("Có lỗi xảy ra khi lấy danh sách payment intent");
            }
        }

        /// <summary>
        /// [Thanh toán] Ghi nhận kết quả PaymentIntent cho lịch hẹn
        /// </summary>
        /// <remarks>
        /// Dùng khi nhận callback từ cổng thanh toán hoặc khi staff xác nhận khách đã thanh toán thành công/không thành công.
        /// </remarks>
        /// <param name="id">ID lịch hẹn</param>
        /// <param name="request">Thông tin kết quả thanh toán</param>
        [HttpPost("{id:int}/payments/record-result")]
        [Authorize(Policy = "AdminOrStaff")]
        public async Task<IActionResult> RecordPaymentResult(
            int id,
            [FromBody] RecordPaymentResultRequestDto request)
        {
            if (!IsValidRequest(request))
            {
                return ValidationError("Dữ liệu kết quả thanh toán không hợp lệ");
            }

            if (id != request.AppointmentId)
            {
                return ValidationError("ID lịch hẹn trong URL không khớp với dữ liệu gửi lên");
            }

            try
            {
                var result = await _commandService.RecordPaymentResultAsync(request, GetCurrentUserId());

                return Success(result, "Đã cập nhật kết quả thanh toán cho lịch hẹn");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid payment result for appointment {AppointmentId}", id);
                return ValidationError(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording payment result for appointment {AppointmentId}", id);
                return ServerError("Có lỗi xảy ra khi ghi nhận kết quả thanh toán");
            }
        }

        /// <summary>
        /// [Complete] Hoàn thành lịch hẹn và update subscription usage
        /// </summary>
        /// <remarks>
        /// Đánh dấu lịch hẹn hoàn thành (InProgress → Completed).
        /// Tự động update subscription usage nếu lịch hẹn được book bằng subscription.
        ///
        /// **Quy trình:**
        /// 1. Kỹ thuật viên hoàn thành công việc
        /// 2. Staff/Admin click Complete
        /// 3. Status → Completed
        /// 4. Nếu có SubscriptionId:
        ///    - Trừ lượt sử dụng cho từng service (UsedQuantity +1, RemainingQuantity -1)
        ///    - Update LastUsedDate và LastUsedAppointmentId
        ///    - Tự động chuyển subscription sang FullyUsed nếu hết lượt
        ///
        /// **Điều kiện:**
        /// - Trạng thái: InProgress (đang thực hiện)
        /// - Không thể complete lại lịch đã Completed
        ///
        /// **Subscription Update:**
        /// - Mỗi service trong appointment sẽ trừ 1 lượt từ subscription
        /// - Nếu subscription hết lượt, status tự động → FullyUsed
        /// - Ghi log đầy đủ để tracking
        ///
        /// **Phân quyền:**
        /// - Staff/Admin/Technician
        /// </remarks>
        /// <param name="id">ID lịch hẹn</param>
        /// <returns>Kết quả hoàn thành</returns>
        [HttpPost("{id:int}/complete")]
        [Authorize(Policy = "AllInternal")] // Admin, Staff, Technician
        public async Task<IActionResult> CompleteAppointment(int id)
        {
            try
            {
                var result = await _commandService.CompleteAppointmentAsync(id, GetCurrentUserId());

                _logger.LogInformation(
                    "User {UserId} completed appointment {AppointmentId}",
                    GetCurrentUserId(),
                    id);

                return Success(
                    new { AppointmentId = id, Completed = result },
                    "Hoàn thành lịch hẹn thành công. Subscription usage đã được cập nhật.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid complete attempt for appointment {AppointmentId}", id);
                return ValidationError(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing appointment {AppointmentId}", id);
                return ServerError("Có lỗi xảy ra khi hoàn thành lịch hẹn");
            }
        }

        /// <summary>
        /// [Hủy lịch] Hủy lịch hẹn bởi Staff
        /// </summary>
        /// <remarks>
        /// Staff hủy lịch hẹn (có nhiều quyền hơn customer).
        ///
        /// **Khác biệt với Customer Cancel:**
        /// - **Customer:** Chỉ hủy được Pending/Confirmed
        /// - **Staff:** Có thể hủy thêm CheckedIn (trong trường hợp đặc biệt)
        ///
        /// **Lý do hủy bởi Staff:**
        /// - Khách yêu cầu hủy qua điện thoại
        /// - Trung tâm phải đóng cửa đột xuất
        /// - Thiết bị hỏng, không thực hiện được dịch vụ
        /// - Thiếu nhân viên/kỹ thuật viên
        ///
        /// **Quy trình:**
        /// 1. Staff chọn lịch cần hủy
        /// 2. Nhập lý do hủy (bắt buộc)
        /// 3. Xác nhận hủy
        /// 4. Status → Cancelled
        /// 5. Giải phóng slot
        /// 6. Gửi email/SMS thông báo khách
        ///
        /// **Phân quyền:**
        /// - Chỉ Staff/Admin mới hủy được
        /// </remarks>
        /// <param name="id">ID lịch hẹn</param>
        /// <param name="request">Lý do hủy</param>
        /// <returns>Kết quả hủy</returns>
        [HttpPost("{id:int}/cancel")]
        [Authorize(Policy = "AdminOrStaff")]
        public async Task<IActionResult> CancelAppointment(
            int id,
            [FromBody] CancelAppointmentRequestDto request)
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
                var result = await _commandService.CancelAsync(request, GetCurrentUserId());

                _logger.LogInformation(
                    "Staff {UserId} cancelled appointment {AppointmentId}. Reason: {Reason}",
                    GetCurrentUserId(),
                    id,
                    request.CancellationReason);

                return Success(
                    new { AppointmentId = id, Cancelled = result },
                    "Hủy lịch hẹn thành công");
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
        /// [Thống kê] Số lượng lịch hẹn theo trạng thái
        /// </summary>
        /// <remarks>
        /// Lấy thống kê số lượng lịch hẹn theo từng trạng thái.
        ///
        /// **Trạng thái bao gồm:**
        /// 1. Pending - Chờ xác nhận
        /// 2. Confirmed - Đã xác nhận
        /// 3. CheckedIn - Đã check-in
        /// 4. InProgress - Đang thực hiện
        /// 5. Completed - Hoàn thành
        /// 6. Cancelled - Đã hủy
        /// 7. Rescheduled - Đã dời lịch
        /// 8. NoShow - Không đến
        ///
        /// **Kết quả:**
        /// - Total: Tổng số lịch hẹn
        /// - ByStatus: Số lượng từng trạng thái
        /// - ActiveAppointments: Tổng lịch đang active (Pending + Confirmed + CheckedIn + InProgress)
        ///
        /// **Use case:**
        /// - Dashboard tổng quan
        /// - Báo cáo quản lý
        /// - Theo dõi hiệu suất
        /// </remarks>
        /// <returns>Số lượng theo từng trạng thái</returns>
        [HttpGet("statistics/by-status")]
        public async Task<IActionResult> GetStatisticsByStatus()
        {
            try
            {
                // Lấy count cho từng status (1-8)
                var statistics = new Dictionary<string, int>
                {
                    { "Pending", await _queryService.GetCountByStatusAsync(1) },      // Pending
                    { "Confirmed", await _queryService.GetCountByStatusAsync(2) },    // Confirmed
                    { "CheckedIn", await _queryService.GetCountByStatusAsync(3) },    // CheckedIn
                    { "InProgress", await _queryService.GetCountByStatusAsync(4) },   // InProgress
                    { "Completed", await _queryService.GetCountByStatusAsync(5) },    // Completed
                    { "Cancelled", await _queryService.GetCountByStatusAsync(6) },    // Cancelled
                    { "Rescheduled", await _queryService.GetCountByStatusAsync(7) },  // Rescheduled
                    { "NoShow", await _queryService.GetCountByStatusAsync(8) }        // NoShow
                };

                var total = statistics.Values.Sum();

                return Success(new
                {
                    Total = total,
                    ByStatus = statistics,
                    ActiveAppointments = statistics["Pending"] + statistics["Confirmed"] +
                                        statistics["CheckedIn"] + statistics["InProgress"]
                },
                "Lấy thống kê lịch hẹn thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving appointment statistics");
                return ServerError("Có lỗi xảy ra khi lấy thống kê");
            }
        }

        /// <summary>
        /// [Cập nhật] Sửa thông tin lịch hẹn (Staff)
        /// </summary>
        /// <remarks>
        /// Staff cập nhật thông tin lịch hẹn (có nhiều quyền hơn customer).
        ///
        /// **Staff có thể sửa:**
        /// - Tất cả fields mà customer sửa được
        /// - Priority (thay đổi độ ưu tiên)
        /// - Internal notes (ghi chú nội bộ)
        /// - Assigned technician (phân công KTV)
        ///
        /// **Không thể sửa:**
        /// - AppointmentCode (tự động generate)
        /// - Customer (không đổi chủ lịch hẹn)
        /// - Created date
        ///
        /// **Use case:**
        /// - Điều chỉnh thông tin sau khi liên hệ khách
        /// - Thêm ghi chú nội bộ
        /// - Thay đổi priority theo tình hình
        /// - Cập nhật dịch vụ theo yêu cầu mới
        ///
        /// **Phân quyền:**
        /// - Chỉ Staff/Admin mới cập nhật được
        /// </remarks>
        /// <param name="id">ID lịch hẹn</param>
        /// <param name="request">Thông tin cập nhật</param>
        /// <returns>Lịch hẹn đã cập nhật</returns>
        [HttpPut("{id:int}")]
        [Authorize(Policy = "AdminOrStaff")]
        public async Task<IActionResult> UpdateAppointment(
            int id,
            [FromBody] UpdateAppointmentRequestDto request)
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
                var result = await _commandService.UpdateAsync(request, GetCurrentUserId());

                _logger.LogInformation(
                    "Staff {UserId} updated appointment {AppointmentId}",
                    GetCurrentUserId(),
                    id);

                return Success(result, "Cập nhật lịch hẹn thành công");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid update attempt for appointment {AppointmentId}", id);
                return ValidationError(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating appointment {AppointmentId}", id);
                return ServerError("Có lỗi xảy ra khi cập nhật lịch hẹn");
            }
        }

        /// <summary>
        /// [Xóa] Xóa lịch hẹn (Admin only)
        /// </summary>
        /// <remarks>
        /// Xóa hoàn toàn lịch hẹn khỏi hệ thống (hard delete).
        ///
        /// **Điều kiện:**
        /// - Chỉ xóa được lịch ở trạng thái Pending
        /// - Lịch chưa được confirm
        /// - Không có WorkOrder liên kết
        ///
        /// **Phân quyền:**
        /// - **Chỉ Admin mới xóa được**
        /// - Staff không có quyền xóa (chỉ Cancel)
        ///
        /// **Khi nào dùng:**
        /// - Dọn dẹp data test
        /// - Xóa lịch spam/fake
        /// - Lịch tạo nhầm không cần history
        ///
        /// **Lưu ý:**
        /// - Ưu tiên dùng Cancel thay vì Delete để giữ history
        /// - Delete không thể phục hồi
        /// - Nên có confirmation dialog trước khi xóa
        /// </remarks>
        /// <param name="id">ID lịch hẹn</param>
        /// <returns>Kết quả xóa</returns>
        [HttpDelete("{id:int}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteAppointment(int id)
        {
            try
            {
                var result = await _commandService.DeleteAsync(id, GetCurrentUserId());

                _logger.LogInformation(
                    "Admin {UserId} deleted appointment {AppointmentId}",
                    GetCurrentUserId(),
                    id);

                return Success(
                    new { AppointmentId = id, Deleted = result },
                    "Xóa lịch hẹn thành công");
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

        /// <summary>
        /// Tạo lịch hẹn cho khách hàng (Walk-in hoặc qua điện thoại)
        /// </summary>
        /// <param name="request">Thông tin lịch hẹn</param>
        /// <returns>Lịch hẹn đã tạo</returns>
        [HttpPost]
        [Authorize(Policy = "AdminOrStaff")]
        public async Task<IActionResult> CreateAppointmentForCustomer([FromBody] CreateAppointmentRequestDto request)
        {
            if (!IsValidRequest(request))
            {
                return ValidationError("Dữ liệu đặt lịch không hợp lệ");
            }

            try
            {
                var result = await _commandService.CreateAsync(request, GetCurrentUserId());

                _logger.LogInformation(
                    "Staff {UserId} created appointment {AppointmentCode} for customer {CustomerId}",
                    GetCurrentUserId(),
                    result.AppointmentCode,
                    request.CustomerId);

                return CreatedAtAction(
                    nameof(GetAppointmentDetail),
                    new { id = result.AppointmentId },
                    new
                    {
                        Success = true,
                        Message = $"Tạo lịch hẹn thành công! Mã: {result.AppointmentCode}",
                        Data = result,
                        Timestamp = DateTime.UtcNow
                    });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid appointment creation by staff {UserId}", GetCurrentUserId());
                return ValidationError(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating appointment by staff {UserId}", GetCurrentUserId());
                return ServerError("Có lỗi xảy ra khi tạo lịch hẹn");
            }
        }

        #region Admin Tools - ServiceSource Adjustment

        /// <summary>
        /// [ADMIN ONLY] Điều chỉnh ServiceSource và giá của một AppointmentService
        /// </summary>
        /// <remarks>
        /// **Use cases:**
        /// 1. **Sửa lỗi hệ thống**: Customer đã có subscription nhưng bị charge nhầm (Extra → Subscription)
        /// 2. **Hoàn tiền**: Dịch vụ không đạt yêu cầu, giảm giá hoặc miễn phí
        /// 3. **Thu thêm phí**: Customer dùng service ngoài subscription
        ///
        /// **Features:**
        /// - Validate appointment đã Completed
        /// - Update ServiceSource (Subscription/Extra/Regular) và Price
        /// - Tự động deduct subscription usage nếu chuyển Extra → Subscription
        /// - Issue refund nếu giá mới < giá cũ và IssueRefund = true
        /// - Log đầy đủ audit trail (IP, User Agent, Reason)
        ///
        /// **Important:**
        /// - Chỉ áp dụng cho appointment đã Completed/CompletedWithUnpaidBalance
        /// - Reason là bắt buộc (10-500 ký tự) để đảm bảo audit trail
        /// - Refund chỉ được issue nếu giá mới < giá cũ
        /// </remarks>
        /// <param name="appointmentId">ID của appointment</param>
        /// <param name="appointmentServiceId">ID của AppointmentService cần điều chỉnh</param>
        /// <param name="request">Thông tin điều chỉnh</param>
        /// <returns>Thông tin adjustment</returns>
        [HttpPost("appointments/{appointmentId}/services/{appointmentServiceId}/adjust")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(typeof(ApiResponse<AdjustServiceSourceResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> AdjustServiceSource(
            int appointmentId,
            int appointmentServiceId,
            [FromBody] AdjustServiceSourceRequestDto request)
        {
            if (!IsValidRequest(request))
            {
                return ValidationError("Dữ liệu điều chỉnh không hợp lệ");
            }

            try
            {
                var currentUserId = GetCurrentUserId();
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

                var result = await _commandService.AdjustServiceSourceAsync(
                    appointmentId: appointmentId,
                    appointmentServiceId: appointmentServiceId,
                    newServiceSource: request.NewServiceSource,
                    newPrice: request.NewPrice,
                    reason: request.Reason,
                    issueRefund: request.IssueRefund,
                    userId: currentUserId,
                    ipAddress: ipAddress,
                    userAgent: userAgent);

                _logger.LogInformation(
                    "Admin {UserId} adjusted AppointmentService {AppointmentServiceId}: " +
                    "{OldSource}({OldPrice}đ) → {NewSource}({NewPrice}đ), Refund={Refund}",
                    currentUserId, appointmentServiceId,
                    result.OldServiceSource, result.OldPrice,
                    result.NewServiceSource, result.NewPrice,
                    result.RefundIssued);

                return Ok(new
                {
                    Success = true,
                    Message = "Điều chỉnh service source thành công",
                    Data = result,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex,
                    "Validation error adjusting service source: AppointmentId={AppointmentId}, " +
                    "AppointmentServiceId={AppointmentServiceId}",
                    appointmentId, appointmentServiceId);
                return ValidationError(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error adjusting service source: AppointmentId={AppointmentId}, " +
                    "AppointmentServiceId={AppointmentServiceId}",
                    appointmentId, appointmentServiceId);
                return ServerError("Đã xảy ra lỗi khi điều chỉnh service source");
            }
        }

        /// <summary>
        /// [ADMIN ONLY] Lấy audit log của một appointment
        /// </summary>
        /// <remarks>
        /// Xem lịch sử thay đổi ServiceSource của tất cả services trong appointment.
        ///
        /// **Thông tin audit log bao gồm:**
        /// - ServiceName, ServiceSource cũ/mới, Price cũ/mới
        /// - Người thực hiện thay đổi (ChangedBy)
        /// - Thời gian thay đổi
        /// - Loại thay đổi (AUTO_DEGRADE, MANUAL_ADJUST, REFUND)
        /// - Lý do thay đổi
        /// - IP address của request
        /// - Refund/Usage deduction status
        ///
        /// **Use cases:**
        /// - Dispute resolution
        /// - Audit compliance
        /// - Debugging pricing issues
        /// </remarks>
        /// <param name="appointmentId">ID của appointment</param>
        /// <returns>Danh sách audit logs</returns>
        [HttpGet("appointments/{appointmentId}/audit-log")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(typeof(ApiResponse<List<object>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetAuditLog(int appointmentId)
        {
            try
            {
                var logs = await _auditService.GetAuditLogsForAppointmentAsync(appointmentId);

                _logger.LogInformation(
                    "Admin {UserId} retrieved audit log for appointment {AppointmentId}: {Count} records",
                    GetCurrentUserId(), appointmentId, logs.Count);

                return Ok(new
                {
                    Success = true,
                    Message = $"Lấy audit log thành công ({logs.Count} bản ghi)",
                    Data = logs,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error getting audit log for appointment {AppointmentId}",
                    appointmentId);
                return ServerError("Đã xảy ra lỗi khi lấy audit log");
            }
        }

        #endregion
    }
}
