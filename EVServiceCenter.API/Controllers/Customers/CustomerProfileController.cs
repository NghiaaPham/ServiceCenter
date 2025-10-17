using EVServiceCenter.Core.Domains.Customers.DTOs.Requests;
using EVServiceCenter.Core.Domains.Customers.DTOs.Responses;
using EVServiceCenter.Core.Domains.Customers.Interfaces;
using EVServiceCenter.Core.Domains.CustomerVehicles.DTOs.Requests;
using EVServiceCenter.Core.Domains.CustomerVehicles.DTOs.Responses;
using EVServiceCenter.Core.Domains.CustomerVehicles.Interfaces.Services;
using EVServiceCenter.Core.Domains.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;

namespace EVServiceCenter.API.Controllers.Customers
{
    [Route("api/customer/profile")]
    [ApiController]
    [Authorize(Policy = "CustomerOnly")]
    [ApiExplorerSettings(GroupName = "Customer - Profile")]     
    public class CustomerProfileController : BaseController
    {
        private readonly ICustomerAccountService _customerAccountService;
        private readonly ICustomerService _customerService;
        private readonly ICustomerVehicleService _vehicleService;
        private readonly ICustomerVehicleQueryService _vehicleQueryService;
        private readonly IValidator<UpdateMyVehicleRequestDto> _updateVehicleValidator;
        private readonly ILogger<CustomerProfileController> _logger;

        public CustomerProfileController(
            ICustomerAccountService customerAccountService,
            ICustomerService customerService,
            ICustomerVehicleService vehicleService,
            ICustomerVehicleQueryService vehicleQueryService,
            IValidator<UpdateMyVehicleRequestDto> updateVehicleValidator,
            ILogger<CustomerProfileController> logger)
        {
            _customerAccountService = customerAccountService;
            _customerService = customerService;
            _vehicleService = vehicleService;
            _vehicleQueryService = vehicleQueryService;
            _updateVehicleValidator = updateVehicleValidator;
            _logger = logger;
        }

        /// <summary>
        /// [Xem chi tiết] Thông tin hồ sơ của tôi
        /// </summary>
        /// <remarks>
        /// Customer xem thông tin hồ sơ cá nhân của mình.
        ///
        /// **Bao gồm:**
        /// - Thông tin cá nhân (tên, SĐT, email, địa chỉ)
        /// - Mã khách hàng (CustomerCode)
        /// - Loại khách hàng (Type: VIP, Regular, New)
        /// - Điểm thưởng hiện tại (LoyaltyPoints)
        /// - Ngày sinh, giới tính
        /// - Preferences (ngôn ngữ, marketing opt-in)
        ///
        /// **Không bao gồm:**
        /// - Danh sách xe (dùng API /vehicles)
        /// - Lịch sử lịch hẹn (dùng API /appointments/my-appointments)
        /// - Ghi chú nội bộ (Notes) - chỉ staff xem được
        ///
        /// **Use case:**
        /// - Hiển thị profile trên app/website
        /// - Trang "Tài khoản của tôi"
        /// - Check điểm thưởng
        ///
        /// **Phân quyền:**
        /// - Chỉ customer đăng nhập mới xem được hồ sơ của mình
        /// </remarks>
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = GetCurrentUserId();
            var customer = await _customerAccountService.GetCustomerByUserIdAsync(userId);

            if (customer == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Thông tin khách hàng không tồn tại",
                    ErrorCode = "CUSTOMER_NOT_FOUND"
                });
            }

            return Ok(new ApiResponse<CustomerResponseDto>
            {
                Success = true,
                Message = "Lấy thông tin thành công",
                Data = customer
            });
        }

        /// <summary>
        /// [Cập nhật] Sửa thông tin hồ sơ của tôi
        /// </summary>
        /// <remarks>
        /// Customer tự cập nhật thông tin hồ sơ cá nhân.
        ///
        /// **Customer có thể sửa:**
        /// - Họ tên (FullName)
        /// - Số điện thoại (PhoneNumber)
        /// - Địa chỉ (Address)
        /// - Ngày sinh (DateOfBirth)
        /// - Giới tính (Gender)
        /// - Ngôn ngữ ưa thích (PreferredLanguage)
        /// - Marketing opt-in (MarketingOptIn)
        ///
        /// **Customer KHÔNG thể sửa:**
        /// - Email (tied to User account, phải dùng API riêng)
        /// - CustomerCode (auto-generated)
        /// - TypeId (chỉ staff/admin mới đổi được)
        /// - LoyaltyPoints (chỉ qua giao dịch)
        /// - IsActive (chỉ staff/admin mới đổi được)
        /// - Notes (internal, chỉ staff/admin)
        ///
        /// **Validation:**
        /// - FullName: required, 2-100 ký tự
        /// - PhoneNumber: format VN, unique
        /// - Email: unique (nếu có)
        ///
        /// **Use case:**
        /// - Customer tự update profile trên app/website
        /// - Sửa thông tin sai
        /// - Cập nhật địa chỉ mới
        ///
        /// **Phân quyền:**
        /// - Chỉ customer đăng nhập mới sửa được hồ sơ của mình
        /// </remarks>
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateCustomerProfileDto request)
        {
            if (!IsValidRequest(request))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Dữ liệu không hợp lệ",
                    ErrorCode = "VALIDATION_ERROR"
                });
            }

            try
            {
                var userId = GetCurrentUserId();
                var customerId = GetCurrentCustomerId();

                if (customerId == 0)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy thông tin khách hàng",
                        ErrorCode = "CUSTOMER_NOT_FOUND"
                    });
                }

                // Get current customer data
                var currentCustomer = await _customerService.GetByIdAsync(customerId, includeStats: false);
                if (currentCustomer == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy thông tin khách hàng",
                        ErrorCode = "CUSTOMER_NOT_FOUND"
                    });
                }

                // Map to UpdateCustomerRequestDto
                var updateRequest = new UpdateCustomerRequestDto
                {
                    CustomerId = customerId,
                    FullName = request.FullName,
                    PhoneNumber = request.PhoneNumber,
                    Email = currentCustomer.Email,  // ✅ Giữ nguyên email (tied to User)
                    Address = request.Address,
                    DateOfBirth = request.DateOfBirth,
                    Gender = request.Gender,
                    PreferredLanguage = request.PreferredLanguage,
                    MarketingOptIn = request.MarketingOptIn,

                    // ✅ Keep server-controlled fields unchanged
                    TypeId = currentCustomer.TypeId,
                    IsActive = currentCustomer.IsActive ?? true,
                    Notes = currentCustomer.Notes,
                    IdentityNumber = null  // Don't update identity number via this endpoint
                };

                var updated = await _customerService.UpdateAsync(updateRequest);

                _logger.LogInformation("Customer {CustomerId} updated own profile", customerId);

                return Ok(new ApiResponse<CustomerResponseDto>
                {
                    Success = true,
                    Message = "Cập nhật thông tin thành công",
                    Data = updated
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message,
                    ErrorCode = "BUSINESS_RULE_VIOLATION"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer profile for userId {UserId}", GetCurrentUserId());
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi cập nhật thông tin",
                    ErrorCode = "INTERNAL_ERROR"
                });
            }
        }

        #region My Vehicles - Quản lý xe của tôi

        /// <summary>
        /// [Xem danh sách] Danh sách xe của tôi
        /// </summary>
        /// <remarks>
        /// Customer xem tất cả xe đã đăng ký dưới tên mình.
        ///
        /// **Bao gồm:**
        /// - Thông tin xe (biển số, VIN, màu, model)
        /// - Thông tin hãng xe và model
        /// - Số km hiện tại
        /// - Ngày bảo dưỡng tiếp theo
        /// - Tình trạng pin (BatteryHealthPercent)
        /// - Thông tin bảo hiểm và đăng kiểm
        ///
        /// **Use case:**
        /// - Xem danh sách xe của tôi
        /// - Chọn xe để đặt lịch bảo dưỡng
        /// - Kiểm tra xe nào cần bảo dưỡng
        ///
        /// **Phân quyền:**
        /// - Chỉ customer đăng nhập mới xem được xe của mình
        /// </remarks>
        [HttpGet("my-vehicles")]
        public async Task<IActionResult> GetMyVehicles()
        {
            try
            {
                var customerId = GetCurrentCustomerId();

                if (customerId == 0)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy thông tin khách hàng",
                        ErrorCode = "CUSTOMER_NOT_FOUND"
                    });
                }

                var vehicles = await _vehicleQueryService.GetVehiclesByCustomerAsync(customerId, CancellationToken.None);

                return Ok(new ApiResponse<IEnumerable<CustomerVehicleResponseDto>>
                {
                    Success = true,
                    Message = $"Tìm thấy {vehicles.Count()} xe",
                    Data = vehicles
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vehicles for customer {CustomerId}", GetCurrentCustomerId());
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi lấy danh sách xe",
                    ErrorCode = "INTERNAL_ERROR"
                });
            }
        }

        /// <summary>
        /// [Đăng ký xe] Đăng ký xe mới của tôi
        /// </summary>
        /// <remarks>
        /// Customer tự đăng ký xe mới dưới tên mình.
        ///
        /// **Thông tin bắt buộc:**
        /// - ModelId (ID model xe - lấy từ API /api/car-models/by-brand/{brandId})
        /// - LicensePlate (Biển số xe - unique)
        ///
        /// **Thông tin tùy chọn:**
        /// - VIN (Số khung)
        /// - Color (Màu xe)
        /// - PurchaseDate (Ngày mua)
        /// - Mileage (Số km hiện tại)
        /// - InsuranceNumber (Số bảo hiểm)
        /// - InsuranceExpiry (Hạn bảo hiểm)
        /// - RegistrationExpiry (Hạn đăng kiểm)
        ///
        /// **Quy trình:**
        /// 1. Validate thông tin đầu vào
        /// 2. Kiểm tra biển số không trùng
        /// 3. Tự động gán CustomerId từ token (không cho customer truyền)
        /// 4. Tạo xe với IsActive = true
        /// 5. Trả về thông tin xe đã tạo
        ///
        /// **Use case:**
        /// - Customer vừa mua xe mới
        /// - Thêm xe thứ 2, 3... vào tài khoản
        /// - Đăng ký để đặt lịch bảo dưỡng
        ///
        /// **Phân quyền:**
        /// - Chỉ customer đăng nhập mới đăng ký được
        /// - Xe tự động gắn với customer hiện tại
        /// </remarks>
        [HttpPost("my-vehicles")]
        public async Task<IActionResult> RegisterMyVehicle([FromBody] RegisterMyVehicleRequestDto request)
        {
            if (!IsValidRequest(request))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Dữ liệu không hợp lệ",
                    ErrorCode = "VALIDATION_ERROR"
                });
            }

            try
            {
                var customerId = GetCurrentCustomerId();

                if (customerId == 0)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy thông tin khách hàng",
                        ErrorCode = "CUSTOMER_NOT_FOUND"
                    });
                }

                // Map to CreateCustomerVehicleRequestDto with auto-assigned CustomerId
                var createRequest = new CreateCustomerVehicleRequestDto
                {
                    CustomerId = customerId,  // ✅ Auto-assigned from token
                    ModelId = request.ModelId,
                    LicensePlate = request.LicensePlate,
                    Vin = request.Vin,
                    Color = request.Color,
                    PurchaseDate = request.PurchaseDate,
                    Mileage = request.Mileage,
                    InsuranceNumber = request.InsuranceNumber,
                    InsuranceExpiry = request.InsuranceExpiry,
                    RegistrationExpiry = request.RegistrationExpiry,
                    IsActive = true
                };

                var userId = GetCurrentUserId();
                var result = await _vehicleService.CreateAsync(createRequest, userId, CancellationToken.None);

                _logger.LogInformation(
                    "Customer {CustomerId} registered new vehicle: {LicensePlate}",
                    customerId,
                    result.LicensePlate);

                return CreatedAtAction(
                    nameof(GetMyVehicles),
                    new { id = result.VehicleId },
                    new ApiResponse<CustomerVehicleResponseDto>
                    {
                        Success = true,
                        Message = "Đăng ký xe thành công",
                        Data = result
                    });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid vehicle registration attempt by customer {CustomerId}", GetCurrentCustomerId());
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message,
                    ErrorCode = "BUSINESS_RULE_VIOLATION"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering vehicle for customer {CustomerId}", GetCurrentCustomerId());
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi đăng ký xe",
                    ErrorCode = "INTERNAL_ERROR"
                });
            }
        }

        /// <summary>
        /// [Xem chi tiết] Thông tin chi tiết xe của tôi
        /// </summary>
        /// <remarks>
        /// Customer xem thông tin chi tiết 1 xe cụ thể của mình.
        ///
        /// **Phân quyền:**
        /// - Customer chỉ xem được xe của mình
        /// - Không xem được xe của người khác
        /// </remarks>
        [HttpGet("my-vehicles/{vehicleId:int}")]
        public async Task<IActionResult> GetMyVehicleById(int vehicleId)
        {
            try
            {
                var customerId = GetCurrentCustomerId();

                if (customerId == 0)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy thông tin khách hàng",
                        ErrorCode = "CUSTOMER_NOT_FOUND"
                    });
                }

                var vehicle = await _vehicleService.GetByIdAsync(vehicleId, CancellationToken.None);

                if (vehicle == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Không tìm thấy xe với ID {vehicleId}",
                        ErrorCode = "VEHICLE_NOT_FOUND"
                    });
                }

                // Kiểm tra xe có thuộc customer này không
                if (vehicle.CustomerId != customerId)
                {
                    return Forbid(); // 403 Forbidden
                }

                return Ok(new ApiResponse<CustomerVehicleResponseDto>
                {
                    Success = true,
                    Message = "Lấy thông tin xe thành công",
                    Data = vehicle
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vehicle {VehicleId} for customer {CustomerId}", vehicleId, GetCurrentCustomerId());
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi lấy thông tin xe",
                    ErrorCode = "INTERNAL_ERROR"
                });
            }
        }

        /// <summary>
        /// [Xóa] Xóa xe của tôi
        /// </summary>
        /// <remarks>
        /// Customer tự xóa xe của mình khỏi danh sách.
        ///
        /// **Điều kiện để xóa:**
        /// - Xe phải thuộc về customer hiện tại
        /// - Xe không có lịch hẹn đang active (Pending, Confirmed, CheckedIn, InProgress)
        /// - Xe không có work order đang mở
        /// - Xe không có subscription đang active
        ///
        /// **Use cases:**
        /// - Khách hàng đã bán xe → Xóa khỏi danh sách
        /// - Khách hàng đăng ký nhầm xe → Xóa để đăng ký lại
        /// - Khách hàng không muốn quản lý xe cũ nữa
        ///
        /// **Logic xóa:**
        /// - Nếu xe chưa có giao dịch nào: **Hard delete** (xóa hoàn toàn)
        /// - Nếu xe đã có lịch hẹn/work order cũ: **Soft delete** (IsActive = false)
        ///
        /// **Lưu ý:**
        /// - Nếu xe có lịch hẹn đang active → Không cho xóa
        /// - Khuyến nghị: Hủy lịch hẹn trước khi xóa xe
        /// - Xe đã soft delete có thể restore bởi Admin
        ///
        /// **Phân quyền:**
        /// - Customer chỉ xóa được xe của mình
        /// - Không xóa được xe của người khác
        /// </remarks>
        /// <param name="vehicleId">ID của xe cần xóa</param>
        /// <returns>Kết quả xóa xe</returns>
        [HttpDelete("my-vehicles/{vehicleId:int}")]
        public async Task<IActionResult> DeleteMyVehicle(int vehicleId)
        {
            try
            {
                var customerId = GetCurrentCustomerId();

                if (customerId == 0)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy thông tin khách hàng",
                        ErrorCode = "CUSTOMER_NOT_FOUND"
                    });
                }

                // 1. Kiểm tra xe có tồn tại không
                var vehicle = await _vehicleService.GetByIdAsync(vehicleId, CancellationToken.None);

                if (vehicle == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Không tìm thấy xe với ID {vehicleId}",
                        ErrorCode = "VEHICLE_NOT_FOUND"
                    });
                }

                // 2. Kiểm tra xe có thuộc customer này không (QUAN TRỌNG - Security check)
                if (vehicle.CustomerId != customerId)
                {
                    _logger.LogWarning(
                        "Customer {CustomerId} attempted to delete vehicle {VehicleId} belonging to customer {OwnerId}",
                        customerId, vehicleId, vehicle.CustomerId);

                    return Forbid(); // 403 Forbidden - Không cho xóa xe của người khác
                }

                // 3. Kiểm tra xe có thể xóa không (có lịch hẹn active, work order, subscription không)
                var canDelete = await _vehicleService.CanDeleteAsync(vehicleId, CancellationToken.None);

                if (!canDelete)
                {
                    _logger.LogInformation(
                        "Customer {CustomerId} cannot delete vehicle {VehicleId} - has active appointments/work orders/subscriptions",
                        customerId, vehicleId);

                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Không thể xóa xe này vì đang có lịch hẹn, phiếu công việc hoặc gói dịch vụ đang hoạt động. " +
                                  "Vui lòng hủy lịch hẹn/hoàn thành công việc trước khi xóa xe.",
                        ErrorCode = "VEHICLE_HAS_ACTIVE_RELATIONS",
                        Data = new
                        {
                            VehicleId = vehicleId,
                            LicensePlate = vehicle.LicensePlate,
                            CanDelete = false,
                            Reason = "Xe đang có lịch hẹn, phiếu công việc hoặc gói dịch vụ đang hoạt động"
                        }
                    });
                }

                // 4. Thực hiện xóa (service sẽ tự quyết định soft/hard delete)
                await _vehicleService.DeleteAsync(vehicleId, CancellationToken.None);

                _logger.LogInformation(
                    "Customer {CustomerId} successfully deleted their vehicle {VehicleId} ({LicensePlate})",
                    customerId, vehicleId, vehicle.LicensePlate);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = $"Đã xóa xe {vehicle.LicensePlate} khỏi danh sách của bạn",
                    Data = new
                    {
                        VehicleId = vehicleId,
                        LicensePlate = vehicle.LicensePlate,
                        DeletedAt = DateTime.UtcNow
                    }
                });
            }
            catch (InvalidOperationException ex)
            {
                // Business rule violation (e.g., xe có lịch hẹn active)
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message,
                    ErrorCode = "BUSINESS_RULE_VIOLATION"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting vehicle {VehicleId} for customer {CustomerId}", 
                    vehicleId, GetCurrentCustomerId());

                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi xóa xe. Vui lòng thử lại sau hoặc liên hệ hỗ trợ.",
                    ErrorCode = "INTERNAL_ERROR"
                });
            }
        }

        /// <summary>
        /// [Kiểm tra] Xe của tôi có thể xóa không
        /// </summary>
        /// <remarks>
        /// Kiểm tra xem xe có thể xóa được hay không trước khi thực hiện xóa.
        ///
        /// **Điều kiện để xóa được:**
        /// - Xe không có lịch hẹn đang active
        /// - Xe không có work order đang mở
        /// - Xe không có subscription đang active
        ///
        /// **Use case:**
        /// - UI gọi trước khi hiển thị nút "Xóa xe"
        /// - Disable nút xóa nếu `canDelete = false`
        /// - Hiển thị lý do không thể xóa để hướng dẫn customer
        ///
        /// **Response:**
        /// ```json
        /// {
        ///   "canDelete": true,
        ///   "vehicleId": 123,
        ///   "licensePlate": "30A-12345",
        ///   "reason": null
        /// }
        /// ```
        ///
        /// Hoặc nếu không xóa được:
        /// ```json
        /// {
        ///   "canDelete": false,
        ///   "vehicleId": 123,
        ///   "licensePlate": "30A-12345",
        ///   "reason": "Xe đang có 2 lịch hẹn active"
        /// }
        /// ```
        ///
        /// **Phân quyền:**
        /// - Customer chỉ check được xe của mình
        /// </remarks>
        /// <param name="vehicleId">ID của xe cần kiểm tra</param>
        /// <returns>Trạng thái có thể xóa hay không</returns>
        [HttpGet("my-vehicles/{vehicleId:int}/can-delete")]
        public async Task<IActionResult> CanDeleteMyVehicle(int vehicleId)
        {
            try
            {
                var customerId = GetCurrentCustomerId();

                if (customerId == 0)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy thông tin khách hàng",
                        ErrorCode = "CUSTOMER_NOT_FOUND"
                    });
                }

                // 1. Kiểm tra xe có tồn tại không
                var vehicle = await _vehicleService.GetByIdAsync(vehicleId, CancellationToken.None);

                if (vehicle == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Không tìm thấy xe với ID {vehicleId}",
                        ErrorCode = "VEHICLE_NOT_FOUND"
                    });
                }

                // 2. Security check: Xe có thuộc customer này không
                if (vehicle.CustomerId != customerId)
                {
                    _logger.LogWarning(
                        "Customer {CustomerId} attempted to check delete status of vehicle {VehicleId} belonging to customer {OwnerId}",
                        customerId, vehicleId, vehicle.CustomerId);

                    return Forbid(); // 403 Forbidden
                }

                // 3. Kiểm tra có thể xóa không
                var canDelete = await _vehicleService.CanDeleteAsync(vehicleId, CancellationToken.None);

                var message = canDelete
                    ? "Xe có thể được xóa"
                    : "Xe không thể xóa vì đang có lịch hẹn, phiếu công việc hoặc gói dịch vụ đang hoạt động";

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = message,
                    Data = new
                    {
                        CanDelete = canDelete,
                        VehicleId = vehicleId,
                        LicensePlate = vehicle.LicensePlate,
                        Reason = canDelete 
                            ? null 
                            : "Xe đang có lịch hẹn, phiếu công việc hoặc gói dịch vụ đang hoạt động"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if vehicle {VehicleId} can be deleted for customer {CustomerId}",
                    vehicleId, GetCurrentCustomerId());

                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi kiểm tra trạng thái xe",
                    ErrorCode = "INTERNAL_ERROR"
                });
            }
        }

        /// <summary>
        /// [Cập nhật] Cập nhật thông tin xe của tôi
        /// </summary>
        /// <remarks>
        /// Customer tự cập nhật một số thông tin cơ bản của xe.
        ///
        /// **🚀 PERFORMANCE OPTIMIZATIONS:**
        /// - Sử dụng GetByIdAsync với caching (5 phút)
        /// - Chỉ update fields thay đổi (partial update)
        /// - Invalidate cache sau khi update
        /// - Minimal database queries
        /// - Fast-fail validation
        ///
        /// **Customer có thể sửa:**
        /// - Số km hiện tại (Mileage) - phải >= số km cũ
        /// - Màu xe (Color)
        /// - Thông tin bảo hiểm (InsuranceNumber, InsuranceExpiry)
        /// - Thông tin đăng kiểm (RegistrationExpiry)
        /// - Sức khỏe pin (BatteryHealthPercent)
        /// - Tình trạng xe (VehicleCondition: Good, Fair, Poor, Excellent)
        ///
        /// **Customer KHÔNG thể sửa:**
        /// - Biển số xe (LicensePlate) - phải liên hệ admin
        /// - Số khung (VIN) - phải liên hệ admin
        /// - Model xe (ModelId) - phải liên hệ admin
        /// - Ngày mua (PurchaseDate) - không thay đổi
        /// - Thông tin bảo dưỡng - do hệ thống tự động
        ///
        /// **Validation:**
        /// - Mileage: Phải >= số km hiện tại
        /// - BatteryHealthPercent: 0-100%
        /// - VehicleCondition: Good, Fair, Poor, Excellent
        ///
        /// **Use case:**
        /// - Cập nhật km sau khi đi xa
        /// - Gia hạn bảo hiểm/đăng kiểm
        /// - Sơn lại xe → đổi màu
        /// - Cập nhật tình trạng pin
        ///
        /// **Phân quyền:**
        /// - Chỉ customer đăng nhập mới sửa được xe của mình
        /// - Không sửa được xe của người khác
        /// </remarks>
        /// <param name="vehicleId">ID của xe cần cập nhật</param>
        /// <param name="request">Thông tin cần cập nhật (partial update supported)</param>
        /// <returns>Thông tin xe sau khi cập nhật</returns>
        [HttpPut("my-vehicles/{vehicleId:int}")]
        [ProducesResponseType(typeof(ApiResponse<CustomerVehicleResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateMyVehicle(
            int vehicleId,
            [FromBody] UpdateMyVehicleRequestDto request,
            CancellationToken ct = default)
        {
            // ⚡ PERFORMANCE: Fast-fail validation (no DB query)
            var validation = await _updateVehicleValidator.ValidateAsync(request, ct);
            if (!validation.IsValid)
            {
                var errors = validation.Errors.Select(e => new
                {
                    Field = e.PropertyName,
                    Message = e.ErrorMessage
                });
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Dữ liệu không hợp lệ",
                    ErrorCode = "VALIDATION_ERROR",
                    ValidationErrors = errors
                });
            }

            try
            {
                // ⚡ PERFORMANCE: Get customerId from claims (no DB query)
                var customerId = GetCurrentCustomerId();

                if (customerId == 0)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy thông tin khách hàng",
                        ErrorCode = "CUSTOMER_NOT_FOUND"
                    });
                }

                // ⚡ PERFORMANCE: Single query with caching (5 min cache in service layer)
                var vehicle = await _vehicleService.GetByIdAsync(vehicleId, ct);

                if (vehicle == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Không tìm thấy xe với ID {vehicleId}",
                        ErrorCode = "VEHICLE_NOT_FOUND"
                    });
                }

                // ⚡ PERFORMANCE: Security check (no extra DB query)
                if (vehicle.CustomerId != customerId)
                {
                    _logger.LogWarning(
                        "SECURITY: Customer {CustomerId} attempted to update vehicle {VehicleId} of customer {OwnerId}",
                        customerId, vehicleId, vehicle.CustomerId);

                    return StatusCode(403, new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Bạn không có quyền cập nhật xe này",
                        ErrorCode = "FORBIDDEN"
                    });
                }

                // ⚡ PERFORMANCE: Validate mileage BEFORE creating update object
                if (request.Mileage.HasValue && request.Mileage < vehicle.Mileage)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Số km mới ({request.Mileage:N0}) không thể nhỏ hơn số km hiện tại ({vehicle.Mileage:N0})",
                        ErrorCode = "INVALID_MILEAGE"
                    });
                }

                // ⚡ PERFORMANCE: Build change log BEFORE update (for efficient logging)
                var changes = new List<string>();
                if (request.Mileage.HasValue && request.Mileage != vehicle.Mileage)
                    changes.Add($"Mileage: {vehicle.Mileage:N0} → {request.Mileage:N0} km");
                if (request.Color != null && request.Color != vehicle.Color)
                    changes.Add($"Color: {vehicle.Color} → {request.Color}");
                if (request.BatteryHealthPercent.HasValue && request.BatteryHealthPercent != vehicle.BatteryHealthPercent)
                    changes.Add($"Battery: {vehicle.BatteryHealthPercent}% → {request.BatteryHealthPercent}%");
                if (request.VehicleCondition != null && request.VehicleCondition != vehicle.VehicleCondition)
                    changes.Add($"Condition: {vehicle.VehicleCondition} → {request.VehicleCondition}");
                if (request.InsuranceNumber != null && request.InsuranceNumber != vehicle.InsuranceNumber)
                    changes.Add($"Insurance: {vehicle.InsuranceNumber} → {request.InsuranceNumber}");
                if (request.InsuranceExpiry.HasValue && request.InsuranceExpiry != vehicle.InsuranceExpiry)
                    changes.Add($"InsuranceExpiry: {vehicle.InsuranceExpiry} → {request.InsuranceExpiry}");
                if (request.RegistrationExpiry.HasValue && request.RegistrationExpiry != vehicle.RegistrationExpiry)
                    changes.Add($"RegistrationExpiry: {vehicle.RegistrationExpiry} → {request.RegistrationExpiry}");

                // ⚡ PERFORMANCE: Skip update if no changes
                if (changes.Count == 0)
                {
                    return Ok(new ApiResponse<CustomerVehicleResponseDto>
                    {
                        Success = true,
                        Message = "Không có thay đổi nào để cập nhật",
                        Data = vehicle
                    });
                }

                // ⚡ PERFORMANCE: Map only changed fields (partial update)
                var updateRequest = new UpdateCustomerVehicleRequestDto
                {
                    VehicleId = vehicleId,
                    
                    // ❌ CRITICAL FIELDS - Không cho customer đổi
                    CustomerId = vehicle.CustomerId,
                    ModelId = vehicle.ModelId,
                    LicensePlate = vehicle.LicensePlate,
                    Vin = vehicle.Vin,
                    PurchaseDate = vehicle.PurchaseDate,
                    IsActive = vehicle.IsActive,
                    
                    // ❌ MAINTENANCE FIELDS - Do hệ thống quản lý
                    LastMaintenanceDate = vehicle.LastMaintenanceDate,
                    NextMaintenanceDate = vehicle.NextMaintenanceDate,
                    LastMaintenanceMileage = vehicle.LastMaintenanceMileage,
                    NextMaintenanceMileage = vehicle.NextMaintenanceMileage,
                    Notes = vehicle.Notes,
                    
                    // ✅ UPDATABLE FIELDS - Cho phép customer update (null-coalescing for partial update)
                    Mileage = request.Mileage ?? vehicle.Mileage,
                    Color = request.Color ?? vehicle.Color,
                    BatteryHealthPercent = request.BatteryHealthPercent ?? vehicle.BatteryHealthPercent,
                    VehicleCondition = request.VehicleCondition ?? vehicle.VehicleCondition,
                    InsuranceNumber = request.InsuranceNumber ?? vehicle.InsuranceNumber,
                    InsuranceExpiry = request.InsuranceExpiry ?? vehicle.InsuranceExpiry,
                    RegistrationExpiry = request.RegistrationExpiry ?? vehicle.RegistrationExpiry
                };

                // ⚡ PERFORMANCE: Single update query, cache will be invalidated in service
                var userId = GetCurrentUserId();
                var result = await _vehicleService.UpdateAsync(updateRequest, userId, ct);

                // ⚡ PERFORMANCE: Structured logging (efficient)
                _logger.LogInformation(
                    "Customer {CustomerId} updated vehicle {VehicleId} ({LicensePlate}). Changes: {ChangeCount} - {Changes}",
                    customerId, vehicleId, vehicle.LicensePlate, changes.Count, string.Join("; ", changes));

                return Ok(new ApiResponse<CustomerVehicleResponseDto>
                {
                    Success = true,
                    Message = $"Cập nhật thông tin xe thành công ({changes.Count} thay đổi)",
                    Data = result
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Business rule violation when updating vehicle {VehicleId}", vehicleId);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message,
                    ErrorCode = "BUSINESS_RULE_VIOLATION"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating vehicle {VehicleId} for customer {CustomerId}",
                    vehicleId, GetCurrentCustomerId());

                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi cập nhật thông tin xe. Vui lòng thử lại sau.",
                    ErrorCode = "INTERNAL_ERROR"
                });
            }
        }

        #endregion
    }
}
