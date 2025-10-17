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
        private readonly ITokenBlacklistService _blacklistService;
        private readonly ILogger<AuthController> _logger;
        private readonly EVDbContext _context;

        public AuthController(
            IUserService userService,
            ITokenService tokenService,
            ITokenBlacklistService blacklistService,
            ILogger<AuthController> logger,
            EVDbContext context)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _blacklistService = blacklistService ?? throw new ArgumentNullException(nameof(blacklistService));
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

        /// <summary>
        /// [Đăng xuất] Logout và revoke JWT token
        /// </summary>
        /// <remarks>
        /// Logout user và thêm JWT token vào blacklist.
        ///
        /// **🔒 SECURITY FEATURES:**
        /// - Revoke JWT token (thêm vào blacklist)
        /// - Token không thể sử dụng được nữa
        /// - Log logout activity với IP và User Agent
        /// - Invalidate user session
        ///
        /// **Process Flow:**
        /// 1. Extract JWT token từ Authorization header
        /// 2. Get userId từ claims
        /// 3. Revoke token (thêm vào RevokedTokens table)
        /// 4. Invalidate UserSession (nếu có)
        /// 5. Return success
        ///
        /// **Use Cases:**
        /// - User click "Logout" button
        /// - Force logout all sessions (security)
        /// - Logout sau khi đổi password
        ///
        /// **Security:**
        /// - Token blacklist với expiry time
        /// - Background job cleanup expired tokens
        /// - IP và User Agent tracking
        ///
        /// **Response:**
        /// - 200 OK: Logout thành công
        /// - 401 Unauthorized: Token không hợp lệ
        /// - 500 Internal Error: Server error
        /// </remarks>
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                // 1. Get current user ID from claims
                var userId = GetCurrentUserId();

                if (userId == 0)
                {
                    _logger.LogWarning("Logout attempted with invalid user ID");
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Không thể xác định người dùng",
                        ErrorCode = "UNAUTHORIZED"
                    });
                }

                // 2. Extract JWT token from Authorization header
                var authHeader = Request.Headers.Authorization.FirstOrDefault();

                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Logout attempted without valid Authorization header");
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Token không hợp lệ",
                        ErrorCode = "INVALID_TOKEN"
                    });
                }

                var token = authHeader.Substring("Bearer ".Length).Trim();

                // 3. Get client IP and User Agent
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var userAgent = Request.Headers.UserAgent.FirstOrDefault();

                // 4. Revoke token (add to blacklist)
                var revoked = await _blacklistService.RevokeTokenAsync(
                    token: token,
                    userId: userId,
                    reason: "Logout",
                    ipAddress: ipAddress,
                    userAgent: userAgent,
                    cancellationToken: HttpContext.RequestAborted);

                if (!revoked)
                {
                    _logger.LogWarning("Failed to revoke token for user {UserId}", userId);
                }

                // 5. Invalidate UserSession (optional - for tracking)
                var activeSession = await _context.UserSessions
                    .Where(s => s.UserId == userId && s.IsActive == true)
                    .OrderByDescending(s => s.LoginTime)
                    .FirstOrDefaultAsync(HttpContext.RequestAborted);

                if (activeSession != null)
                {
                    activeSession.IsActive = false;
                    activeSession.LogoutTime = DateTime.UtcNow;
                    await _context.SaveChangesAsync(HttpContext.RequestAborted);
                }

                // 6. Log logout event
                _logger.LogInformation(
                    "User {UserId} logged out successfully from IP: {IP}, UserAgent: {UserAgent}",
                    userId, ipAddress, userAgent);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Đăng xuất thành công",
                    Data = new
                    {
                        UserId = userId,
                        LogoutTime = DateTime.UtcNow,
                        Message = "Token đã được thu hồi. Vui lòng đăng nhập lại để tiếp tục."
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout for user {UserId}", GetCurrentUserId());
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi đăng xuất",
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