using EVServiceCenter.Core.Constants;
using EVServiceCenter.Core.Domains.Customers.DTOs.Requests;
using EVServiceCenter.Core.Domains.Customers.DTOs.Responses;
using EVServiceCenter.Core.Domains.Customers.Interfaces;
using EVServiceCenter.Core.Domains.Shared.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVServiceCenter.API.Controllers.Customers
{
    [ApiController]
    [Route("api/customers")]
    [Authorize(Policy = "AllInternal")] // Base policy: Admin/Staff/Technician
    [ApiExplorerSettings(GroupName = "Staff - Customers")]
    public class CustomersController : BaseController
    {
        private readonly ICustomerService _customerService;
        private readonly ILogger<CustomersController> _logger;
        private readonly IValidator<CreateCustomerRequestDto> _createValidator;
        private readonly IValidator<UpdateCustomerRequestDto> _updateValidator;
        private readonly IValidator<CustomerQueryDto> _queryValidator;

        public CustomersController(
            ICustomerService customerService,
            ILogger<CustomersController> logger,
            IValidator<CreateCustomerRequestDto> createValidator,
            IValidator<UpdateCustomerRequestDto> updateValidator,
            IValidator<CustomerQueryDto> queryValidator)
        {
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _createValidator = createValidator ?? throw new ArgumentNullException(nameof(createValidator));
            _updateValidator = updateValidator ?? throw new ArgumentNullException(nameof(updateValidator));
            _queryValidator = queryValidator ?? throw new ArgumentNullException(nameof(queryValidator));
        }

        /// <summary>
        /// [Xem danh sách] Tất cả khách hàng (có filter/sort/paging)
        /// </summary>
        /// <remarks>
        /// Lấy danh sách tất cả khách hàng trong hệ thống với filter, sort, paging.
        ///
        /// **Có thể filter theo:**
        /// - Loại khách hàng (TypeId: VIP, Regular, New)
        /// - Trạng thái (IsActive: true/false)
        /// - Số điện thoại
        /// - Email
        /// - Ngày tạo (DateFrom, DateTo)
        ///
        /// **Hỗ trợ:**
        /// - Pagination (Page, PageSize)
        /// - Sorting (SortBy, IsDescending)
        /// - Search (tìm theo tên, SĐT, email)
        ///
        /// **Thông tin trả về:**
        /// - Thông tin cơ bản khách hàng
        /// - Số lượng xe đang sở hữu
        /// - Tổng chi tiêu (nếu includeStats = true)
        /// - Điểm thưởng (LoyaltyPoints)
        /// </remarks>
        /// <param name="query">Query parameters for filtering and pagination</param>
        /// <returns>Paginated list of customers</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllCustomers([FromQuery] CustomerQueryDto query)
        {
            // Validate query parameters
            var validationResult = await _queryValidator.ValidateAsync(query);
            if (!validationResult.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Tham số truy vấn không hợp lệ",
                    ErrorCode = "VALIDATION_ERROR",
                    ValidationErrors = validationResult.Errors.Select(e => new
                    {
                        Field = e.PropertyName,
                        Message = e.ErrorMessage,
                        AttemptedValue = e.AttemptedValue
                    }),
                    Timestamp = DateTime.UtcNow
                });
            }

            try
            {
                var result = await _customerService.GetAllAsync(query);

                return Ok(new ApiResponse<PagedResult<CustomerResponseDto>>
                {
                    Success = true,
                    Message = $"Lấy danh sách khách hàng thành công. Tìm thấy {result.TotalCount} khách hàng.",
                    Data = result,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customers with query: {@Query}", query);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi lấy danh sách khách hàng",
                    ErrorCode = "INTERNAL_ERROR",
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// [Xem chi tiết] Thông tin khách hàng theo ID
        /// </summary>
        /// <remarks>
        /// Lấy thông tin chi tiết đầy đủ của một khách hàng.
        ///
        /// **Bao gồm:**
        /// - Thông tin cá nhân (tên, SĐT, email, địa chỉ, CCCD)
        /// - Loại khách hàng (VIP, Regular, New)
        /// - Điểm thưởng (LoyaltyPoints)
        /// - Ngày tạo, ngày cập nhật
        ///
        /// **Nếu includeStats = true:**
        /// - Số lượng xe đang sở hữu
        /// - Tổng số lịch hẹn đã tạo
        /// - Tổng chi tiêu
        /// - Số lần NoShow
        /// - Lịch hẹn gần nhất
        ///
        /// **Use case:**
        /// - Xem profile chi tiết khách hàng
        /// - Phân tích hành vi mua hàng
        /// - Hỗ trợ tư vấn
        /// </remarks>
        /// <param name="id">Customer ID</param>
        /// <param name="includeStats">Include vehicle and transaction statistics</param>
        /// <returns>Customer details</returns>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCustomerById(int id, [FromQuery] bool includeStats = true)
        {
            if (id <= 0)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "ID khách hàng phải lớn hơn 0",
                    ErrorCode = "INVALID_PARAMETER",
                    Timestamp = DateTime.UtcNow
                });
            }

            try
            {
                var result = await _customerService.GetByIdAsync(id, includeStats);

                if (result == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Không tìm thấy khách hàng với ID {id}",
                        ErrorCode = "CUSTOMER_NOT_FOUND",
                        Timestamp = DateTime.UtcNow
                    });
                }

                return Ok(new ApiResponse<CustomerResponseDto>
                {
                    Success = true,
                    Message = "Lấy thông tin khách hàng thành công",
                    Data = result,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer by ID: {CustomerId}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi lấy thông tin khách hàng",
                    ErrorCode = "INTERNAL_ERROR",
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// [Tra cứu] Tìm khách hàng theo mã
        /// </summary>
        /// <remarks>
        /// Tra cứu khách hàng bằng mã khách hàng (Customer Code).
        ///
        /// **Format mã:**
        /// - KH + YYMMDD + 2 số tự tăng
        /// - VD: KH240101, KH241231
        ///
        /// **Use case:**
        /// - Staff tra cứu nhanh khách hàng
        /// - Tìm theo mã được in trên thẻ thành viên
        /// - Lookup từ báo cáo/invoice
        /// </remarks>
        /// <param name="customerCode">Customer code (e.g., KH240101)</param>
        /// <returns>Customer details</returns>
        [HttpGet("by-code/{customerCode}")]
        public async Task<IActionResult> GetCustomerByCode(string customerCode)
        {
            if (string.IsNullOrWhiteSpace(customerCode))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Mã khách hàng không được để trống",
                    ErrorCode = "INVALID_PARAMETER",
                    Timestamp = DateTime.UtcNow
                });
            }

            try
            {
                var result = await _customerService.GetByCustomerCodeAsync(customerCode);

                if (result == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Không tìm thấy khách hàng với mã {customerCode}",
                        ErrorCode = "CUSTOMER_NOT_FOUND",
                        Timestamp = DateTime.UtcNow
                    });
                }

                return Ok(new ApiResponse<CustomerResponseDto>
                {
                    Success = true,
                    Message = "Lấy thông tin khách hàng thành công",
                    Data = result,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer by code: {CustomerCode}", customerCode);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi lấy thông tin khách hàng",
                    ErrorCode = "INTERNAL_ERROR",
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// [Tra cứu] Tìm khách hàng theo SĐT
        /// </summary>
        /// <remarks>
        /// Tra cứu khách hàng bằng số điện thoại (quick lookup).
        ///
        /// **Use case:**
        /// - Khách walk-in, Staff check SĐT trong hệ thống
        /// - Khách gọi điện đặt lịch, tìm thông tin
        /// - Xác minh khách hàng khi check-in
        ///
        /// **Format SĐT:**
        /// - Có thể nhập với hoặc không có +84
        /// - VD: 0901234567, +84901234567
        ///
        /// **Kết quả:**
        /// - Nếu tìm thấy: trả về thông tin khách hàng
        /// - Nếu không tìm thấy: 404 Not Found
        /// </remarks>
        /// <param name="phone">Phone number</param>
        /// <returns>Customer details</returns>
        [HttpGet("by-phone")]
        public async Task<IActionResult> GetCustomerByPhone([FromQuery] string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Số điện thoại không được để trống",
                    ErrorCode = "INVALID_PARAMETER",
                    Timestamp = DateTime.UtcNow
                });
            }

            try
            {
                var result = await _customerService.GetByPhoneNumberAsync(phone);

                if (result == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Không tìm thấy khách hàng với số điện thoại {phone}",
                        ErrorCode = "CUSTOMER_NOT_FOUND",
                        Timestamp = DateTime.UtcNow
                    });
                }

                return Ok(new ApiResponse<CustomerResponseDto>
                {
                    Success = true,
                    Message = "Lấy thông tin khách hàng thành công",
                    Data = result,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer by phone: {Phone}", phone);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi lấy thông tin khách hàng",
                    ErrorCode = "INTERNAL_ERROR",
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// [Thêm mới] Tạo khách hàng walk-in
        /// </summary>
        /// <remarks>
        /// Tạo khách hàng walk-in (khách đến trực tiếp, chưa có tài khoản online).
        ///
        /// **Khi nào dùng:**
        /// - Khách đến trực tiếp trung tâm lần đầu
        /// - Khách chưa đăng ký tài khoản online
        /// - Staff tạo nhanh profile để đặt lịch
        ///
        /// **Thông tin bắt buộc:**
        /// - Họ tên (FullName)
        /// - Số điện thoại (PhoneNumber) - dùng làm username tạm
        ///
        /// **Thông tin tùy chọn:**
        /// - Email
        /// - Địa chỉ
        /// - Ngày sinh, giới tính
        ///
        /// **Quy trình:**
        /// 1. Staff nhập thông tin khách
        /// 2. Hệ thống tạo Customer record
        /// 3. Tự động tạo CustomerCode (KH + YYMMDD + số)
        /// 4. Gán TypeId = "New Customer"
        /// 5. Không tạo User account (chỉ tạo Customer)
        ///
        /// **Phân quyền:**
        /// - Chỉ Staff/Admin mới tạo được
        /// </remarks>
        [HttpPost]
        [Authorize(Policy = "AdminOrStaff")]
        public async Task<IActionResult> CreateCustomer([FromBody] CreateWalkInCustomerDto request) 
        {
            if (!IsValidRequest(request))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Dữ liệu tạo khách hàng không hợp lệ",
                    ErrorCode = "VALIDATION_ERROR",
                    Timestamp = DateTime.UtcNow
                });
            }

            try
            {
                // Gán TypeId mặc định là Standard (20) nếu không được cung cấp
                // Walk-in customers are automatically assigned Standard type
                if (!request.TypeId.HasValue)
                {
                    request.TypeId = CustomerConstants.DefaultCustomerTypeId;
                    _logger.LogInformation("TypeId not provided for walk-in customer, using default: {TypeId}", 
                        CustomerConstants.DefaultCustomerTypeId);
                }

                var createdByUserId = GetCurrentUserId();
                var result = await _customerService.CreateWalkInCustomerAsync(request, createdByUserId);  
                _logger.LogInformation("Walk-in customer created by user {UserId}: {CustomerCode} - {FullName}",
                    createdByUserId, result.CustomerCode, result.FullName);

                return CreatedAtAction(
                    nameof(GetCustomerById),
                    new { id = result.CustomerId },
                    new ApiResponse<CustomerResponseDto>
                    {
                        Success = true,
                        Message = $"Tạo khách hàng thành công. Mã khách hàng: {result.CustomerCode}",
                        Data = result,
                        Timestamp = DateTime.UtcNow
                    });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message,
                    ErrorCode = "BUSINESS_RULE_VIOLATION",
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating walk-in customer: {FullName}", request.FullName);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi tạo khách hàng",
                    ErrorCode = "INTERNAL_ERROR",
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// [Cập nhật] Sửa thông tin khách hàng
        /// </summary>
        /// <remarks>
        /// Staff cập nhật thông tin khách hàng (full access).
        ///
        /// **Staff có thể sửa:**
        /// - Thông tin cá nhân (tên, SĐT, email, địa chỉ)
        /// - Loại khách hàng (TypeId: VIP, Regular, New)
        /// - Trạng thái (IsActive: active/inactive)
        /// - Ghi chú nội bộ (Notes)
        /// - Số CCCD (IdentityNumber)
        ///
        /// **Không thể sửa:**
        /// - CustomerCode (tự động generate)
        /// - LoyaltyPoints (dùng API riêng)
        /// - CreatedAt, UpdatedAt
        ///
        /// **Use case:**
        /// - Cập nhật thông tin sau khi xác minh
        /// - Sửa thông tin sai
        /// - Nâng cấp khách hàng lên VIP
        /// - Thêm ghi chú quan trọng
        ///
        /// **Phân quyền:**
        /// - Chỉ Staff/Admin mới cập nhật được
        /// </remarks>
        /// <param name="id">Customer ID</param>
        /// <param name="request">Customer update data</param>
        /// <returns>Updated customer details</returns>
        [HttpPut("{id:int}")]
        [Authorize(Policy = "AdminOrStaff")]
        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] UpdateCustomerRequestDto request)
        {
            // Ensure ID consistency
            if (id != request.CustomerId)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "ID trong URL không khớp với ID trong dữ liệu",
                    ErrorCode = "ID_MISMATCH",
                    Timestamp = DateTime.UtcNow
                });
            }

            // Validate request using FluentValidation
            var validationResult = await _updateValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Dữ liệu cập nhật khách hàng không hợp lệ",
                    ErrorCode = "VALIDATION_ERROR",
                    ValidationErrors = validationResult.Errors.Select(e => new
                    {
                        Field = e.PropertyName,
                        Message = e.ErrorMessage,
                        AttemptedValue = e.AttemptedValue
                    }),
                    Timestamp = DateTime.UtcNow
                });
            }

            try
            {
                var result = await _customerService.UpdateAsync(request);

                _logger.LogInformation("Customer updated successfully by user {UserId}: {CustomerCode} - {FullName}",
                    GetCurrentUserId(), result.CustomerCode, result.FullName);

                return Ok(new ApiResponse<CustomerResponseDto>
                {
                    Success = true,
                    Message = "Cập nhật thông tin khách hàng thành công",
                    Data = result,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message,
                    ErrorCode = "BUSINESS_RULE_VIOLATION",
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer: {CustomerId}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi cập nhật khách hàng",
                    ErrorCode = "INTERNAL_ERROR",
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// [Xóa] Xóa khách hàng
        /// </summary>
        /// <remarks>
        /// Xóa khách hàng khỏi hệ thống (soft/hard delete tự động).
        ///
        /// **Logic xóa:**
        /// - **Nếu có xe hoặc giao dịch:** Soft delete (IsActive = false, giữ data)
        /// - **Nếu không có gì:** Hard delete (xóa hoàn toàn khỏi DB)
        ///
        /// **Điều kiện:**
        /// - Không có lịch hẹn đang active (Pending/Confirmed/CheckedIn/InProgress)
        /// - Không có WorkOrder đang mở
        /// - Không có Invoice chưa thanh toán
        ///
        /// **Phân quyền:**
        /// - **Chỉ Admin mới xóa được**
        /// - Staff không có quyền xóa (chỉ Deactivate)
        ///
        /// **Khuyến nghị:**
        /// - Nên dùng Deactivate (set IsActive = false) thay vì Delete
        /// - Giữ lại data để phân tích, báo cáo
        /// - Chỉ xóa customer test/spam/duplicate
        ///
        /// **Lưu ý:**
        /// - Kiểm tra CanDelete trước khi gọi API này
        /// - Soft delete vẫn giữ data, có thể restore
        /// - Hard delete không thể phục hồi
        /// </remarks>
        /// <param name="id">Customer ID</param>
        /// <returns>Success confirmation</returns>
        [HttpDelete("{id:int}")]
        [Authorize(Policy = "AdminOnly")] // Only Admin can delete customers
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            try
            {
                var result = await _customerService.DeleteAsync(id);

                if (!result)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Không tìm thấy khách hàng với ID {id}",
                        ErrorCode = "CUSTOMER_NOT_FOUND",
                        Timestamp = DateTime.UtcNow
                    });
                }

                _logger.LogInformation("Customer deleted successfully by user {UserId}: {CustomerId}",
                    GetCurrentUserId(), id);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Xóa khách hàng thành công",
                    Data = null,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message,
                    ErrorCode = "BUSINESS_RULE_VIOLATION",
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message,
                    ErrorCode = "INVALID_PARAMETER",
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer: {CustomerId}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi xóa khách hàng",
                    ErrorCode = "INTERNAL_ERROR",
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// [Xem danh sách] Khách hàng đang hoạt động
        /// </summary>
        /// <remarks>
        /// Lấy danh sách khách hàng đang active (IsActive = true).
        ///
        /// **Use case:**
        /// - Dropdown list khi tạo lịch hẹn
        /// - Dropdown list khi tạo WorkOrder
        /// - Danh sách gửi email marketing
        /// - Danh sách chương trình khuyến mãi
        ///
        /// **Kết quả:**
        /// - Chỉ khách hàng có IsActive = true
        /// - Sắp xếp theo tên A-Z
        /// - Hiển thị: ID, Name, Code, Phone, Email
        ///
        /// **Không pagination:**
        /// - Trả về toàn bộ danh sách (không phân trang)
        /// - Thích hợp cho dropdown/autocomplete
        /// </remarks>
        /// <returns>List of active customers</returns>
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveCustomers()
        {
            try
            {
                var result = await _customerService.GetActiveCustomersAsync();

                return Ok(new ApiResponse<IEnumerable<CustomerResponseDto>>
                {
                    Success = true,
                    Message = "Lấy danh sách khách hàng hoạt động thành công",
                    Data = result,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active customers");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi lấy danh sách khách hàng hoạt động",
                    ErrorCode = "INTERNAL_ERROR",
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// [Xem danh sách] Khách hàng có xe cần bảo dưỡng
        /// </summary>
        /// <remarks>
        /// Lấy danh sách khách hàng có xe cần bảo dưỡng định kỳ.
        ///
        /// **Tiêu chí xe cần bảo dưỡng:**
        /// - Đến kỳ hạn bảo dưỡng theo km
        /// - Đến kỳ hạn bảo dưỡng theo thời gian
        /// - Có cảnh báo từ hệ thống xe
        ///
        /// **Use case:**
        /// - Marketing: gửi SMS/email nhắc bảo dưỡng
        /// - Tele-sales: gọi điện chăm sóc khách hàng
        /// - Retention: giữ chân khách cũ
        /// - Upsell: đề xuất gói bảo dưỡng
        ///
        /// **Kết quả:**
        /// - Thông tin khách hàng
        /// - Thông tin xe cần bảo dưỡng
        /// - Lịch bảo dưỡng dự kiến
        /// - Dịch vụ khuyến nghị
        ///
        /// **Phân quyền:**
        /// - Chỉ Staff/Admin mới xem được
        /// </remarks>
        /// <returns>List of customers with maintenance due</returns>
        [HttpGet("maintenance-due")]
        [Authorize(Policy = "AdminOrStaff")]
        public async Task<IActionResult> GetCustomersWithMaintenanceDue()
        {
            try
            {
                var result = await _customerService.GetCustomersWithMaintenanceDueAsync();

                return Ok(new ApiResponse<IEnumerable<CustomerResponseDto>>
                {
                    Success = true,
                    Message = $"Tìm thấy {result.Count()} khách hàng có xe cần bảo dưỡng",
                    Data = result,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customers with maintenance due");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi lấy danh sách khách hàng cần bảo dưỡng",
                    ErrorCode = "INTERNAL_ERROR",
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// [Tra cứu] Kiểm tra khả năng xóa
        /// </summary>
        /// <remarks>
        /// Kiểm tra xem khách hàng có thể xóa được hay không.
        ///
        /// **Điều kiện để xóa được:**
        /// - Không có xe đang sở hữu
        /// - Không có lịch hẹn đang active
        /// - Không có WorkOrder đang mở
        /// - Không có Invoice chưa thanh toán
        /// - Không có transaction history
        ///
        /// **Use case:**
        /// - Gọi trước khi hiển thị nút Delete
        /// - Disable nút Delete nếu không thể xóa
        /// - Hiển thị lý do không thể xóa
        ///
        /// **Kết quả:**
        /// - CanDelete: true/false
        /// - Reason: Lý do không thể xóa (nếu có)
        ///
        /// **Phân quyền:**
        /// - Chỉ Admin mới check được
        /// </remarks>
        /// <param name="id">Customer ID</param>
        /// <returns>Delete capability status</returns>
        [HttpGet("{id:int}/can-delete")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> CanDeleteCustomer(int id)
        {
            try
            {
                var canDelete = await _customerService.CanDeleteAsync(id);

                var message = canDelete
                    ? "Khách hàng có thể được xóa"
                    : "Khách hàng không thể xóa vì đang có xe hoặc giao dịch trong hệ thống";

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = message,
                    Data = new
                    {
                        CanDelete = canDelete,
                        CustomerId = id,
                        Reason = canDelete ? null : "Khách hàng có xe hoặc giao dịch liên quan"
                    },
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if customer can be deleted: {CustomerId}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi kiểm tra",
                    ErrorCode = "INTERNAL_ERROR",
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// [Cập nhật] Cộng điểm thưởng
        /// </summary>
        /// <remarks>
        /// Cộng điểm thưởng (Loyalty Points) cho khách hàng.
        ///
        /// **Lý do cộng điểm:**
        /// - Hoàn thành dịch vụ (auto)
        /// - Giới thiệu bạn bè
        /// - Sinh nhật khách hàng
        /// - Khuyến mãi đặc biệt
        /// - Bù đắp sự cố
        /// - Manual adjustment
        ///
        /// **Quy tắc:**
        /// - Points phải > 0 (không cho trừ điểm qua API này)
        /// - Phải có lý do (Reason)
        /// - Ghi log ai cộng, khi nào, lý do gì
        ///
        /// **Use case:**
        /// - Staff thưởng điểm cho khách VIP
        /// - Chương trình khuyến mãi
        /// - Bù điểm do lỗi hệ thống
        ///
        /// **Lưu ý:**
        /// - Để trừ điểm: dùng API riêng (DeductLoyaltyPoints)
        /// - Points có thể có hạn sử dụng
        /// - Tích hợp với hệ thống rewards
        ///
        /// **Phân quyền:**
        /// - Chỉ Staff/Admin mới cộng điểm được
        /// </remarks>
        /// <param name="id">Customer ID</param>
        /// <param name="request">Loyalty points addition request</param>
        /// <returns>Success confirmation</returns>
        [HttpPost("{id:int}/loyalty-points")]
        [Authorize(Policy = "AdminOrStaff")]
        public async Task<IActionResult> AddLoyaltyPoints(int id, [FromBody] AddLoyaltyPointsRequestDto request)
        {
            if (request.Points <= 0)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Số điểm thưởng phải lớn hơn 0",
                    ErrorCode = "INVALID_PARAMETER",
                    Timestamp = DateTime.UtcNow
                });
            }

            try
            {
                var result = await _customerService.AddLoyaltyPointsAsync(id, request.Points, request.Reason ?? "Manual adjustment");

                if (!result)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Không tìm thấy khách hàng với ID {id}",
                        ErrorCode = "CUSTOMER_NOT_FOUND",
                        Timestamp = DateTime.UtcNow
                    });
                }

                _logger.LogInformation("Loyalty points added by user {UserId}: {Points} points to customer {CustomerId} for reason: {Reason}",
                    GetCurrentUserId(), request.Points, id, request.Reason);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = $"Đã cộng {request.Points} điểm thưởng cho khách hàng",
                    Data = new
                    {
                        CustomerId = id,
                        PointsAdded = request.Points,
                        Reason = request.Reason,
                        AddedBy = GetCurrentUserName(),
                        AddedAt = DateTime.UtcNow
                    },
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding loyalty points to customer: {CustomerId}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi cộng điểm thưởng",
                    ErrorCode = "INTERNAL_ERROR",
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// [Thống kê] Thống kê khách hàng
        /// </summary>
        /// <remarks>
        /// Lấy thống kê tổng quan về khách hàng trong hệ thống.
        ///
        /// **Bao gồm:**
        /// - Tổng số khách hàng (Total)
        /// - Khách hàng đang active (Active)
        /// - Khách hàng inactive (Inactive)
        /// - Phân loại theo Type (VIP, Regular, New)
        /// - Khách có xe cần bảo dưỡng
        /// - Khách hàng mới trong tháng
        ///
        /// **Thống kê mở rộng (TODO):**
        /// - Top khách hàng theo chi tiêu
        /// - Top khách hàng theo điểm thưởng
        /// - Tỷ lệ giữ chân khách hàng (Retention Rate)
        /// - Giá trị trung bình mỗi khách (Average Customer Value)
        ///
        /// **Use case:**
        /// - Dashboard tổng quan
        /// - Báo cáo quản lý
        /// - Phân tích dữ liệu
        /// - KPI tracking
        ///
        /// **Phân quyền:**
        /// - Chỉ Staff/Admin mới xem được
        /// </remarks>
        /// <returns>Customer statistics summary</returns>
        [HttpGet("statistics")]
        [Authorize(Policy = "AdminOrStaff")]
        public async Task<IActionResult> GetCustomerStatistics()
        {
            try
            {
                var typeStats = await _customerService.GetCustomerStatisticsAsync();

                // Get additional statistics from a query
                var query = new CustomerQueryDto
                {
                    Page = 1,
                    PageSize = 1, // Just need totals
                    IncludeStats = false
                };

                var totalResult = await _customerService.GetAllAsync(query);

                var activeQuery = new CustomerQueryDto
                {
                    Page = 1,
                    PageSize = 1,
                    IsActive = true,
                    IncludeStats = false
                };

                var activeResult = await _customerService.GetAllAsync(activeQuery);

                var maintenanceDueCustomers = await _customerService.GetCustomersWithMaintenanceDueAsync();

                var statistics = new
                {
                    TotalCustomers = totalResult.TotalCount,
                    ActiveCustomers = activeResult.TotalCount,
                    InactiveCustomers = totalResult.TotalCount - activeResult.TotalCount,
                    MaintenanceDueCustomers = maintenanceDueCustomers.Count(),
                    CustomersByType = typeStats,
                    TopCustomersBySpending = new List<object>(), // Would need additional query
                    TopCustomersByLoyaltyPoints = new List<object>(), // Would need additional query
                    NewCustomersThisMonth = 0, // Would need date filter query
                    AverageCustomerValue = 0, // Would need calculation
                    CustomerRetentionRate = 0 // Would need complex calculation
                };

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Lấy thống kê khách hàng thành công",
                    Data = statistics,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer statistics");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi lấy thống kê khách hàng",
                    ErrorCode = "INTERNAL_ERROR",
                    Timestamp = DateTime.UtcNow
                });
            }
        }
    }

    // Helper DTO for loyalty points addition
    public class AddLoyaltyPointsRequestDto
    {
        public int Points { get; set; }
        public string? Reason { get; set; }
    }
}