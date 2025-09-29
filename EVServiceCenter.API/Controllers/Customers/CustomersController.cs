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
        /// Get all customers with filtering, sorting and pagination
        /// </summary>
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
        /// Get customer by ID
        /// </summary>
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
        /// Get customer by customer code
        /// </summary>
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
        /// Get customer by phone number (for quick lookup)
        /// </summary>
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
        /// Create new walk-in customer (by Staff at counter)
        /// </summary>
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
        /// Update existing customer
        /// </summary>
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
        /// Delete customer (soft delete if has vehicles, hard delete if no vehicles)
        /// </summary>
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
        /// Get active customers for dropdown lists
        /// </summary>
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
        /// Get customers with vehicles due for maintenance
        /// </summary>
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
        /// Check if customer can be deleted
        /// </summary>
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
        /// Add loyalty points to customer
        /// </summary>
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
        /// Get customer statistics and analytics
        /// </summary>
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