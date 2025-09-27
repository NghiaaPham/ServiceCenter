using EVServiceCenter.Core.Constants;
using EVServiceCenter.Core.Domains.Identity.DTOs.Requests;
using EVServiceCenter.Core.Domains.Identity.DTOs.Responses;
using EVServiceCenter.Core.Domains.Identity.Entities;
using EVServiceCenter.Core.Domains.Identity.Interfaces;
using EVServiceCenter.Core.Domains.Shared.Models;
using EVServiceCenter.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVServiceCenter.API.Controllers.Auth
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : BaseController
    {
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IUserService userService,
            ITokenService tokenService,
            ILogger<AuthController> logger)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost("register")]
        [AllowAnonymous]
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
                if (!CanRegisterWithRole(registerRequest.RoleId))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Bạn không thể đăng ký với vai trò này.",
                        ErrorCode = "ROLE_NOT_ALLOWED"
                    });
                }

                var user = new User
                {
                    Username = registerRequest.Username,
                    FullName = registerRequest.FullName,
                    Email = registerRequest.Email,
                    PhoneNumber = registerRequest.PhoneNumber,
                    RoleId = registerRequest.RoleId
                };

                var createdUser = await _userService.RegisterUserAsync(user, registerRequest.Password);

                var tokenUser = new User
                {
                    UserId = createdUser.UserId,
                    Username = createdUser.Username,
                    RoleId = registerRequest.RoleId
                };
                var token = _tokenService.GenerateToken(tokenUser);

                return CreatedAtAction(nameof(GetProfile), new { id = createdUser.UserId },
                    new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Đăng ký thành công! Vui lòng kiểm tra email để xác thực tài khoản.",
                        Data = new
                        {
                            User = createdUser,
                            Token = token,
                            RequireEmailVerification = true
                        }
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration");
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
                var (user, errorCode, errorMessage) = loginResult; // Destructure tuple

                // Nếu có lỗi (user là null)
                if (user == null)
                {
                    // Xử lý riêng cho email not verified
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

                    // Các lỗi khác (sai password, tài khoản bị khóa, v.v.)
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = errorMessage,
                        ErrorCode = errorCode
                    });
                }

                // Login thành công - tạo token
                var tokenUser = new User
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    RoleId = user.RoleId
                };

                var token = _tokenService.GenerateToken(tokenUser);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = SuccessMessages.LOGIN_SUCCESS,
                    Data = new
                    {
                        User = user,
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
            if (!User.Identity?.IsAuthenticated == true)
            {
                return roleId == (int)UserRoles.Customer;
            }

            if (IsAdmin())
            {
                return true;
            }

            return roleId == (int)UserRoles.Customer;
        }
    }
}