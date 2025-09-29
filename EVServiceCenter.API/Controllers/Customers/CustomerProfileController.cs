using EVServiceCenter.Core.Domains.Customers.DTOs.Requests;
using EVServiceCenter.Core.Domains.Customers.DTOs.Responses;
using EVServiceCenter.Core.Domains.Customers.Interfaces;
using EVServiceCenter.Core.Domains.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVServiceCenter.API.Controllers.Customers
{
    [Route("api/customer/profile")]
    [ApiController]
    [Authorize(Policy = "CustomerOnly")]
    public class CustomerProfileController : BaseController
    {
        private readonly ICustomerAccountService _customerAccountService;
        private readonly ICustomerService _customerService;
        private readonly ILogger<CustomerProfileController> _logger;
        public CustomerProfileController(
            ICustomerAccountService customerAccountService,
            ICustomerService customerService,
            ILogger<CustomerProfileController> logger)
        {
            _customerAccountService = customerAccountService;
            _customerService = customerService;
            _logger = logger;
        }

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
    }
}
