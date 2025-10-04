using EVServiceCenter.Core.Domains.Customers.DTOs.Requests;
using EVServiceCenter.Core.Domains.Customers.Interfaces;
using EVServiceCenter.Core.Domains.Identity.DTOs.Requests;
using EVServiceCenter.Core.Domains.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVServiceCenter.API.Controllers.Customers
{
    [ApiController]
    [Route("api/customer-registration")]
    [AllowAnonymous]
    [ApiExplorerSettings(GroupName = "Public - Registration")]
    public class CustomerRegistrationController : BaseController
    {
        private readonly ICustomerAccountService _customerAccountService;
        private readonly ILogger<CustomerRegistrationController> _logger;

        public CustomerRegistrationController(
            ICustomerAccountService customerAccountService,
            ILogger<CustomerRegistrationController> logger)
        {
            _customerAccountService = customerAccountService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterCustomer([FromBody] CustomerRegistrationDto request)
        {
            if (!IsValidRequest(request))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Dữ liệu đăng ký không hợp lệ.",
                    ErrorCode = "VALIDATION_ERROR"
                });
            }

            if (!request.AcceptTerms)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Bạn phải đồng ý với điều khoản sử dụng để tiếp tục.",
                    ErrorCode = "TERMS_NOT_ACCEPTED"
                });
            }

            try
            {
                // Convert CustomerRegistrationDto to CreateCustomerRequestDto
                var customerRequest = new CreateCustomerRequestDto
                {
                    FullName = request.FullName,
                    PhoneNumber = request.PhoneNumber,
                    Email = request.Email,
                    PreferredLanguage = "vi-VN",
                    MarketingOptIn = request.MarketingOptIn,
                    TypeId = 1, // Default customer type
                    IsActive = true,
                    Notes = "Đăng ký trực tuyến"
                };

                // Create both User and Customer in one transaction
                var customer = await _customerAccountService.CreateCustomerWithAccountAsync(
                    customerRequest,
                    request.Password
                );

                _logger.LogInformation("Customer self-registered: {CustomerCode} - {FullName} - {Email}",
                    customer.CustomerCode, customer.FullName, customer.Email);

                return CreatedAtAction(
                    "GetMyProfile",
                    "CustomerProfile",
                    null,
                    new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Đăng ký tài khoản thành công! Vui lòng kiểm tra email để xác thực tài khoản.",
                        Data = new
                        {
                            RequireEmailVerification = true,
                            NextSteps = new[]
                            {
                                "Kiểm tra hộp thư email của bạn",
                                "Click vào link xác thực trong email",
                                "Hoàn tất xác thực để có thể đăng nhập",
                                "Cập nhật thông tin chi tiết trong trang cá nhân"
                            },
                            LoginUrl = "/login",
                            ResendVerificationUrl = "/api/verification/resend-verification"
                        }
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
                _logger.LogError(ex, "Error during customer self-registration: {Email}", request.Email);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra trong quá trình đăng ký. Vui lòng thử lại sau.",
                    ErrorCode = "REGISTRATION_ERROR"
                });
            }
        }
    }
}
