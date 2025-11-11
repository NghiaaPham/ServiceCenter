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
using System.Diagnostics;
using EVServiceCenter.Core.Domains.Identity.DTOs;

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
                    Message = "D? li?u dang k� kh�ng h?p l?.",
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
                        Message = "Ch? c� th? t?o t�i kho?n n?i b? (Admin, Staff, Technician).",
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
                        Message = "T?o t�i kho?n nh�n vi�n th�nh c�ng! Email ch�o m?ng d� du?c g?i.",
                        Data = new
                        {
                            User = createdUser,
                            UserType = "Internal",
                            CreatedBy = GetCurrentUserName(),
                            NextStep = "Nh�n vi�n c� th? dang nh?p ngay b?ng username/password d� t?o",
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
                    Message = "T�n dang nh?p v� m?t kh?u kh�ng du?c d? tr?ng",
                    ErrorCode = ErrorCodes.VALIDATION_ERROR
                });
            }

            try
            {
                // Measure login stages for performance debugging
                var sw = Stopwatch.StartNew();
                var loginResult = await _userService.LoginAsync(loginRequest.Username, loginRequest.Password);
                sw.Stop();
                _logger.LogInformation("LoginAsync completed in {ElapsedMs} ms for username={Username}", sw.ElapsedMilliseconds, loginRequest.Username);

                var (user, errorCode, errorMessage) = loginResult;

                // N?u c� l?i
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
                            "Ki?m tra h?p thu email c?a b?n",
                            "T�m email t? EV Service Center (ki?m tra c? thu m?c spam)",
                            "Click v�o link x�c th?c trong email",
                            "Quay l?i trang n�y d? dang nh?p"
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

                // ? N?u l� Customer role ? load Customer data
                object? customerData = null;
                TokenCustomerInfo? customerInfo = null;
                int? customerId = null;

                if (user.RoleId == (int)UserRoles.Customer)
                {
                    sw.Restart();
                    var customer = await _context.Customers
                        .AsNoTracking()
                        .Where(c => c.UserId == user.UserId)
                        .Select(c => new
                        {
                            c.CustomerId,
                            c.CustomerCode,
                            c.LoyaltyPoints,
                            c.TotalSpent,
                            c.TypeId,
                            CustomerTypeName = c.Type != null ? c.Type.TypeName : null,
                            CustomerTypeDiscount = c.Type != null ? c.Type.DiscountPercent : (decimal?)null
                        })
                        .FirstOrDefaultAsync();
                    sw.Stop();
                    _logger.LogInformation("Customer lookup completed in {ElapsedMs} ms for userId={UserId}", sw.ElapsedMilliseconds, user.UserId);

                    if (customer != null)
                    {
                        customerId = customer.CustomerId;
                        customerData = new
                        {
                            customer.CustomerId,
                            customer.CustomerCode,
                            customer.LoyaltyPoints,
                            customer.TotalSpent,
                            customer.CustomerTypeName,
                            CustomerTypeDiscount = customer.CustomerTypeDiscount ?? 0
                        };

                        customerInfo = new TokenCustomerInfo
                        {
                            CustomerId = customer.CustomerId,
                            CustomerCode = customer.CustomerCode,
                            CustomerTypeId = customer.TypeId,
                            LoyaltyPoints = customer.LoyaltyPoints ?? 0
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

                sw.Restart();
                var accessToken = _tokenService.GenerateToken(tokenUser, customerInfo);
                sw.Stop();
                _logger.LogInformation("GenerateToken completed in {ElapsedMs} ms for userId={UserId}", sw.ElapsedMilliseconds, user.UserId);

                sw.Restart();
                var refreshToken = await _userService.RotateRefreshTokenAsync(user.UserId, Request.Headers.UserAgent.FirstOrDefault(), HttpContext.Connection.RemoteIpAddress?.ToString());
                sw.Stop();
                _logger.LogInformation("RotateRefreshTokenAsync completed in {ElapsedMs} ms for userId={UserId}", sw.ElapsedMilliseconds, user.UserId);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = SuccessMessages.LOGIN_SUCCESS,
                    Data = new
                    {
                        User = user,
                        Customer = customerData,
                        AccessToken = accessToken,
                        RefreshToken = refreshToken
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user {Username}", loginRequest.Username);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "C� l?i x?y ra trong qu� tr�nh dang nh?p",
                    ErrorCode = ErrorCodes.INTERNAL_ERROR
                });
            }
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return Unauthorized(new ApiResponse<object> { Success = false, Message = "Invalid token", ErrorCode = "INVALID_TOKEN" });
            }

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = Request.Headers.UserAgent.FirstOrDefault();

            var user = await _userService.ValidateRefreshTokenAsync(request.RefreshToken, userAgent, ipAddress);

            if (user == null)
            {
                return Unauthorized(new ApiResponse<object> { Success = false, Message = "Invalid or expired refresh token", ErrorCode = "INVALID_REFRESH_TOKEN" });
            }

            // Load customer info (nếu là customer) để nhúng vào token mới
            TokenCustomerInfo? customerInfo = null;
            if (user.RoleId == (int)UserRoles.Customer)
            {
                customerInfo = await _context.Customers
                    .AsNoTracking()
                    .Where(c => c.UserId == user.UserId)
                    .Select(c => new TokenCustomerInfo
                    {
                        CustomerId = c.CustomerId,
                        CustomerCode = c.CustomerCode,
                        CustomerTypeId = c.TypeId,
                        LoyaltyPoints = c.LoyaltyPoints ?? 0
                    })
                    .FirstOrDefaultAsync();
            }
            var newAccessToken = _tokenService.GenerateToken(user, customerInfo);
            var newRefreshToken = await _userService.RotateRefreshTokenAsync(user.UserId, userAgent, ipAddress);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Token refreshed successfully",
                Data = new
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken
                }
            });
        }

        /// <summary>
        /// [�ang xu?t] Logout v� revoke JWT token
        /// </summary>
        /// <remarks>
        /// Logout user v� th�m JWT token v�o blacklist.
        ///
        /// **?? SECURITY FEATURES:**
        /// - Revoke JWT token (th�m v�o blacklist)
        /// - Token kh�ng th? s? d?ng du?c n?a
        /// - Log logout activity v?i IP v� User Agent
        /// - Invalidate user session
        ///
        /// **Process Flow:**
        /// 1. Extract JWT token t? Authorization header
        /// 2. Get userId t? claims
        /// 3. Revoke token (th�m v�o RevokedTokens table)
        /// 4. Invalidate UserSession (n?u c�)
        /// 5. Return success
        ///
        /// **Use Cases:**
        /// - User click "Logout" button
        /// - Force logout all sessions (security)
        /// - Logout sau khi d?i password
        ///
        /// **Security:**
        /// - Token blacklist v?i expiry time
        /// - Background job cleanup expired tokens
        /// - IP v� User Agent tracking
        ///
        /// **Response:**
        /// - 200 OK: Logout th�nh c�ng
        /// - 401 Unauthorized: Token kh�ng h?p l?
        /// - 500 Internal Error: Server error
        /// </remarks>
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] LogoutRequestDto request)
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
                        Message = "Kh�ng th? x�c d?nh ngu?i d�ng",
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
                        Message = "Token kh�ng h?p l?",
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

                // Revoke the refresh token
                if (!string.IsNullOrEmpty(request.RefreshToken))
                {
                    await _userService.RevokeRefreshTokenAsync(request.RefreshToken, ipAddress);
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
                    Message = "�ang xu?t th�nh c�ng",
                    Data = new
                    {
                        UserId = userId,
                        LogoutTime = DateTime.UtcNow,
                        Message = "Token d� du?c thu h?i. Vui l�ng dang nh?p l?i d? ti?p t?c."
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout for user {UserId}", GetCurrentUserId());
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "C� l?i x?y ra khi dang xu?t",
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

