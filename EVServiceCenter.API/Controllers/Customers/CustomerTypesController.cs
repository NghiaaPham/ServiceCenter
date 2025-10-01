using EVServiceCenter.Core.Domains.CustomerTypes.DTOs.Requests;
using EVServiceCenter.Core.Domains.CustomerTypes.DTOs.Responses;
using EVServiceCenter.Core.Domains.CustomerTypes.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVServiceCenter.API.Controllers.Customers
{
    [ApiController]
    [Route("api/customer-types")]
    [Authorize(Policy = "AllInternal")] // Base policy: Admin/Staff/Technician
    public class CustomerTypesController : BaseController
    {
        private readonly ICustomerTypeService _customerTypeService;
        private readonly ILogger<CustomerTypesController> _logger;
        private readonly IValidator<CreateCustomerTypeRequestDto> _createValidator;
        private readonly IValidator<UpdateCustomerTypeRequestDto> _updateValidator;
        private readonly IValidator<CustomerTypeQueryDto> _queryValidator;
        public CustomerTypesController(
           ICustomerTypeService customerTypeService,
           ILogger<CustomerTypesController> logger,
           IValidator<CreateCustomerTypeRequestDto> createValidator,
           IValidator<UpdateCustomerTypeRequestDto> updateValidator,
           IValidator<CustomerTypeQueryDto> queryValidator)
        {
            _customerTypeService = customerTypeService ?? throw new ArgumentNullException(nameof(customerTypeService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _createValidator = createValidator ?? throw new ArgumentNullException(nameof(createValidator));
            _updateValidator = updateValidator ?? throw new ArgumentNullException(nameof(updateValidator));
            _queryValidator = queryValidator ?? throw new ArgumentNullException(nameof(queryValidator));
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCustomerTypes([FromQuery] CustomerTypeQueryDto query)
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
                        Message = e.ErrorMessage
                    }),
                    Timestamp = DateTime.UtcNow
                });
            }

            try
            {
                var result = await _customerTypeService.GetAllAsync(query);

                return Ok(new ApiResponse<PagedResult<CustomerTypeResponseDto>>
                {
                    Success = true,
                    Message = "Danh sách loại khách hàng đã được lấy thành công",
                    Data = result,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer types with query: {@Query}", query);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi lấy danh sách loại khách hàng",
                    ErrorCode = "INTERNAL_ERROR",
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCustomerTypeById(int id, [FromQuery] bool includeStats = true)
        {
            if (id <= 0)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "ID loại khách hàng phải lớn hơn 0",
                    ErrorCode = "INVALID_PARAMETER",
                    Timestamp = DateTime.UtcNow
                });
            }

            try
            {
                var result = await _customerTypeService.GetByIdAsync(id, includeStats);

                if (result == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Không tìm thấy loại khách hàng với ID {id}",
                        ErrorCode = "CUSTOMER_TYPE_NOT_FOUND",
                        Timestamp = DateTime.UtcNow
                    });
                }

                return Ok(new ApiResponse<CustomerTypeResponseDto>
                {
                    Success = true,
                    Message = "Thông tin loại khách hàng đã được lấy thành công",
                    Data = result,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer type by ID: {Id}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi lấy thông tin loại khách hàng",
                    ErrorCode = "INTERNAL_ERROR",
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        [HttpPost]
        [Authorize(Policy = "AdminOrStaff")]
        public async Task<IActionResult> CreateCustomerType([FromBody] CreateCustomerTypeRequestDto request)
        {
            // Validate request using FluentValidation
            var validationResult = await _createValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Dữ liệu tạo không hợp lệ",
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
                var result = await _customerTypeService.CreateAsync(request);

                return CreatedAtAction(
                    nameof(GetCustomerTypeById),
                    new { id = result.TypeId },
                    new ApiResponse<CustomerTypeResponseDto>
                    {
                        Success = true,
                        Message = "Loại khách hàng đã được tạo thành công",
                        Data = result,
                        Timestamp = DateTime.UtcNow
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer type: {TypeName}", request.TypeName);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi tạo loại khách hàng",
                    ErrorCode = "INTERNAL_ERROR",
                    Timestamp = DateTime.UtcNow
                });
            }
        }


        [HttpPut("{id:int}")]
        [Authorize(Policy = "AdminOrStaff")]
        public async Task<IActionResult> UpdateCustomerType(int id, [FromBody] UpdateCustomerTypeRequestDto request)
        {
            // Ensure ID consistency
            if (id != request.TypeId)
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
                    Message = "Dữ liệu cập nhật không hợp lệ",
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
                var result = await _customerTypeService.UpdateAsync(request);

                return Ok(new ApiResponse<CustomerTypeResponseDto>
                {
                    Success = true,
                    Message = "Loại khách hàng đã được cập nhật thành công",
                    Data = result,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer type: {TypeId}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi cập nhật loại khách hàng",
                    ErrorCode = "INTERNAL_ERROR",
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Delete customer type
        /// </summary>
        /// <param name="id">Customer type ID</param>
        /// <returns>Success confirmation</returns>
        [HttpDelete("{id:int}")]
        [Authorize(Policy = "AdminOnly")] // Only Admin can delete
        public async Task<IActionResult> DeleteCustomerType(int id)
        {
            try
            {
                var result = await _customerTypeService.DeleteAsync(id);

                if (!result)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Không tìm thấy loại khách hàng với ID {id}",
                        ErrorCode = "CUSTOMER_TYPE_NOT_FOUND",
                        Timestamp = DateTime.UtcNow
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Loại khách hàng đã được xóa thành công",
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
                _logger.LogError(ex, "Error deleting customer type: {TypeId}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi xóa loại khách hàng",
                    ErrorCode = "INTERNAL_ERROR",
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Get active customer types for dropdown lists
        /// </summary>
        /// <returns>List of active customer types</returns>
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveCustomerTypes()
        {
            try
            {
                var result = await _customerTypeService.GetActiveTypesAsync();

                return Ok(new ApiResponse<IEnumerable<CustomerTypeResponseDto>>
                {
                    Success = true,
                    Message = "Danh sách loại khách hàng hoạt động đã được lấy thành công",
                    Data = result,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active customer types");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi lấy danh sách loại khách hàng hoạt động",
                    ErrorCode = "INTERNAL_ERROR",
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Check if customer type can be deleted
        /// </summary>
        /// <param name="id">Customer type ID</param>
        /// <returns>Delete capability status</returns>
        [HttpGet("{id:int}/can-delete")]
        [Authorize(Policy = "AdminOnly")] // Only Admin needs this check
        public async Task<IActionResult> CanDeleteCustomerType(int id)
        {
            try
            {
                var canDelete = await _customerTypeService.CanDeleteAsync(id);

                var message = canDelete
                    ? "Loại khách hàng có thể được xóa"
                    : "Loại khách hàng không thể xóa vì đang có khách hàng sử dụng";

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = message,
                    Data = new
                    {
                        CanDelete = canDelete,
                        TypeId = id,
                        Reason = canDelete ? null : "Có khách hàng đang sử dụng loại này"
                    },
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if customer type can be deleted: {TypeId}", id);
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
        /// Get customer type statistics summary
        /// </summary>
        /// <returns>Statistics about customer types usage</returns>
        [HttpGet("statistics")]
        [Authorize(Policy = "AdminOrStaff")]
        public async Task<IActionResult> GetCustomerTypeStatistics()
        {
            try
            {
                var query = new CustomerTypeQueryDto
                {
                    Page = 1,
                    PageSize = 100,
                    IncludeStats = true
                };

                var result = await _customerTypeService.GetAllAsync(query);

                var statistics = new
                {
                    TotalCustomerTypes = result.TotalCount,
                    ActiveCustomerTypes = result.Items.Count(ct => ct.IsActive),
                    InactiveCustomerTypes = result.Items.Count(ct => !ct.IsActive),
                    TotalCustomers = result.Items.Sum(ct => ct.CustomerCount),
                    TotalRevenue = result.Items.Sum(ct => ct.TotalRevenueFromType),
                    AverageDiscountPercent = result.Items.Where(ct => ct.IsActive)
                        .Average(ct => ct.DiscountPercent),
                    TypesWithMostCustomers = result.Items
                        .OrderByDescending(ct => ct.CustomerCount)
                        .Take(3)
                        .Select(ct => new { ct.TypeName, ct.CustomerCount })
                        .ToList(),
                    TypesWithHighestRevenue = result.Items
                        .OrderByDescending(ct => ct.TotalRevenueFromType)
                        .Take(3)
                        .Select(ct => new { ct.TypeName, Revenue = ct.TotalRevenueFromType })
                        .ToList()
                };

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Thống kê loại khách hàng đã được lấy thành công",
                    Data = statistics,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer type statistics");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi lấy thống kê loại khách hàng",
                    ErrorCode = "INTERNAL_ERROR",
                    Timestamp = DateTime.UtcNow
                });
            }
        }
    }
}
