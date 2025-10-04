using EVServiceCenter.Core.Constants;
using EVServiceCenter.Core.Domains.Identity.DTOs.Requests;
using EVServiceCenter.Core.Domains.Identity.DTOs.Responses;
using EVServiceCenter.Core.Domains.Identity.Entities;
using EVServiceCenter.Core.Domains.Identity.Interfaces;
using EVServiceCenter.Core.Domains.Shared.Models;
using EVServiceCenter.Core.Entities;
using EVServiceCenter.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EVServiceCenter.API.Controllers.Auth
{
    [Route("api/auth")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Public - Authentication")]
    public class AuthController : BaseController
    {
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthController> _logger;
        private readonly EVDbContext _context;

        public AuthController(
            IUserService userService,
            ITokenService tokenService,
            ILogger<AuthController> logger,
            EVDbContext context)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        [HttpPost("register")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto registerRequest)
        {
            if (!IsValidRequest(registerRequest))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Dữ liệu đăng ký không hợp lệ.",
                    ErrorCode = "VALIDATION_ERROR"
                });
            }

            try
            {
                // Only allow internal roles (Admin, Staff, Technician)
                if (!CanRegisterWithRole(registerRequest.RoleId))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Chỉ có thể tạo tài khoản nội bộ (Admin, Staff, Technician).",
                        ErrorCode = "ROLE_NOT_ALLOWED"
                    });
                }

                var user = new User
                {
                    Username = registerRequest.Username,
                    FullName = registerRequest.FullName,
                    Email = registerRequest.Email,
                    PhoneNumber = registerRequest.PhoneNumber,
                    RoleId = registerRequest.RoleId,
                    Department = registerRequest.Department,
                    HireDate = registerRequest.HireDate,
                    Salary = registerRequest.Salary,
                    CreatedBy = GetCurrentUserId()
                };

                var createdUser = await _userService.RegisterInternalUserAsync(user, registerRequest.Password);

                return CreatedAtAction(nameof(GetProfile), new { id = createdUser.UserId },
                    new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Tạo tài khoản nhân viên thành công! Email chào mừng đã được gửi.",
                        Data = new
                        {
                            User = createdUser,
                            UserType = "Internal",
                            CreatedBy = GetCurrentUserName(),
                            NextStep = "Nhân viên có thể đăng nhập ngay bằng username/password đã tạo",
                            LoginUrl = "/login"
                        }
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during internal user registration");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message,
                    ErrorCode = "REGISTRATION_ERROR"
                });
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequest)
        {
            if (!IsValidRequest(loginRequest) ||
                string.IsNullOrWhiteSpace(loginRequest.Username) ||
                string.IsNullOrWhiteSpace(loginRequest.Password))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Tên đăng nhập và mật khẩu không được để trống",
                    ErrorCode = ErrorCodes.VALIDATION_ERROR
                });
            }

            try
            {
                var loginResult = await _userService.LoginAsync(loginRequest.Username, loginRequest.Password);
                var (user, errorCode, errorMessage) = loginResult;

                // Nếu có lỗi
                if (user == null)
                {
                    if (errorCode == ErrorCodes.EMAIL_NOT_VERIFIED)
                    {
                        return BadRequest(new ApiResponse<object>
                        {
                            Success = false,
                            Message = errorMessage,
                            ErrorCode = errorCode,
                            Data = new
                            {
                                RequireEmailVerification = true,
                                Username = loginRequest.Username,
                                ResendVerificationUrl = "/api/verification/resend-verification",
                                CheckStatusUrl = "/api/verification/email-status",
                                Instructions = new[]
                                {
                            "Kiểm tra hộp thư email của bạn",
                            "Tìm email từ EV Service Center (kiểm tra cả thư mục spam)",
                            "Click vào link xác thực trong email",
                            "Quay lại trang này để đăng nhập"
                        }
                            }
                        });
                    }

                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = errorMessage,
                        ErrorCode = errorCode
                    });
                }

                // ✅ Nếu là Customer role → load Customer data
                object? customerData = null;
                int? customerId = null;

                if (user.RoleId == (int)UserRoles.Customer)
                {
                    // Load Customer từ database
                    var customer = await _context.Customers
                        .Include(c => c.Type)
                        .FirstOrDefaultAsync(c => c.UserId == user.UserId);

                    if (customer != null)
                    {
                        customerId = customer.CustomerId;
                        customerData = new
                        {
                            customer.CustomerId,
                            customer.CustomerCode,
                            customer.LoyaltyPoints,
                            customer.TotalSpent,
                            CustomerTypeName = customer.Type?.TypeName,
                            CustomerTypeDiscount = customer.Type?.DiscountPercent ?? 0
                        };
                    }
                }

                var tokenUser = new User
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    RoleId = user.RoleId,
                    Email = user.Email,
                    FullName = user.FullName
                };

                var token = _tokenService.GenerateToken(tokenUser, customerId);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = SuccessMessages.LOGIN_SUCCESS,
                    Data = new
                    {
                        User = user,
                        Customer = customerData,  
                        Token = token
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user {Username}", loginRequest.Username);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra trong quá trình đăng nhập",
                    ErrorCode = ErrorCodes.INTERNAL_ERROR
                });
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Logout successful",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Internal server error",
                    ErrorCode = "INTERNAL_ERROR"
                });
            }
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Unable to identify user",
                        ErrorCode = "UNAUTHORIZED"
                    });
                }

                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "User profile not found",
                        ErrorCode = "USER_NOT_FOUND"
                    });
                }

                return Ok(new ApiResponse<UserResponseDto>
                {
                    Success = true,
                    Message = "Profile retrieved successfully",
                    Data = user
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user profile");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Internal server error",
                    ErrorCode = "INTERNAL_ERROR"
                });
            }
        }

        [HttpPut("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
        {
            if (!IsValidRequest(request))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid request data",
                    ErrorCode = "VALIDATION_ERROR"
                });
            }

            try
            {
                var userId = GetCurrentUserId();
                await _userService.UpdateUserPasswordAsync(userId, request.CurrentPassword, request.NewPassword);
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Password changed successfully",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user {UserId}", GetCurrentUserId());
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message.Contains("Current password") ? ex.Message : "Internal server error",
                    ErrorCode = ex.Message.Contains("Current password") ? "INVALID_CURRENT_PASSWORD" : "INTERNAL_ERROR"
                });
            }
        }

        // PRIVATE HELPER METHODS
        private bool CanRegisterWithRole(int roleId)
        {
            // For internal registration, only allow Admin, Staff, Technician
            if (!IsAdmin())
            {
                return false; // Only admins can create internal accounts
            }

            // Admin can create Admin, Staff, or Technician accounts
            return roleId == (int)UserRoles.Admin ||
                   roleId == (int)UserRoles.Staff ||
                   roleId == (int)UserRoles.Technician;
        }
    }
}