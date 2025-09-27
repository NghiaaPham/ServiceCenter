using EVServiceCenter.Core.Domains.Identity.DTOs.Requests;
using EVServiceCenter.Core.Domains.Identity.DTOs.Responses;
using EVServiceCenter.Core.Domains.Identity.Interfaces;
using EVServiceCenter.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EVServiceCenter.Core.Domains.Shared.Models;

namespace EVServiceCenter.API.Controllers.Auth
{
    [Route("api/verification")]
    [ApiController]
    [AllowAnonymous]
    public class VerificationController : BaseController
    {
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;
        private readonly ILogger<VerificationController> _logger;
        private readonly IConfiguration _configuration;

        public VerificationController(
            IUserService userService,
            IEmailService emailService,
            ILogger<VerificationController> logger,
            IConfiguration configuration)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequestDto request)
        {
            if (!IsValidRequest(request))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Dữ liệu không hợp lệ",
                    ErrorCode = ErrorCodes.VALIDATION_ERROR
                });
            }

            try
            {
                var result = await _userService.VerifyEmailAsync(request.Email, request.Token);
                if (!result)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Link xác thực không hợp lệ hoặc đã hết hạn",
                        ErrorCode = ErrorCodes.INVALID_TOKEN
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Email verified successfully! Welcome to EV Service Center.",
                    Data = new
                    {
                        EmailVerified = true,
                        LoginUrl = $"{_configuration["AppSettings:WebsiteUrl"]}/login",
                        Message = "Bạn có thể đăng nhập ngay bây giờ!"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying email for {Email}", request.Email);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra trong quá trình xác thực",
                    ErrorCode = ErrorCodes.INTERNAL_ERROR
                });
            }
        }

        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmailFromLink([FromQuery] string token, [FromQuery] string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                var frontendUrl = _configuration["AppSettings:WebsiteUrl"];
                return Redirect($"{frontendUrl}/verify-email?error=invalid-params");
            }
            try
            {
                var decodedEmail = Uri.UnescapeDataString(email);
                _logger.LogInformation("Processing email verification for {Email}", decodedEmail);

                var result = await _userService.VerifyEmailAsync(decodedEmail, token);

                var frontendUrl = _configuration["AppSettings:WebsiteUrl"];

                if (!result)
                {
                    _logger.LogWarning("Email verification failed for {Email}", decodedEmail);
                    // Redirect về frontend với error
                    return Redirect($"{frontendUrl}/verify-email?error=invalid-token");
                }

                _logger.LogInformation("Email verification successful for {Email}", decodedEmail);

                // Redirect về frontend với success
                return Redirect($"{frontendUrl}/verify-email?success=true&email={Uri.EscapeDataString(decodedEmail)}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during email verification for {Email}", email);
                var frontendUrl = _configuration["AppSettings:WebsiteUrl"];
                return Redirect($"{frontendUrl}/verify-email?error=server-error");
            }
        }

        [HttpPost("resend-verification")]
        public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationRequestDto request)
        {
            if (!IsValidRequest(request))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Email không được để trống",
                    ErrorCode = ErrorCodes.VALIDATION_ERROR
                });
            }

            try
            {
                var user = await _userService.GetUserByEmailAsync(request.Email);
                if (user == null)
                {
                    // Security: không tiết lộ thông tin về email có tồn tại hay không
                    return Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Nếu email tồn tại trong hệ thống, link xác thực đã được gửi.",
                        Data = new { EmailSent = true }
                    });
                }

                if (user.EmailVerified)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = ErrorMessages.EMAIL_ALREADY_VERIFIED,
                        ErrorCode = ErrorCodes.EMAIL_ALREADY_VERIFIED,
                        Data = new
                        {
                            AlreadyVerified = true,
                            LoginUrl = "/login",
                            Message = "Tài khoản đã được xác thực. Bạn có thể đăng nhập ngay."
                        }
                    });
                }

                var result = await _userService.ResendEmailVerificationAsync(request.Email);
                if (!result)
                {
                    return StatusCode(500, new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Không thể gửi email xác thực lúc này. Vui lòng thử lại sau.",
                        ErrorCode = ErrorCodes.INTERNAL_ERROR
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Link xác thực mới đã được gửi đến email của bạn. Vui lòng kiểm tra hộp thư.",
                    Data = new
                    {
                        EmailSent = true,
                        Email = request.Email,
                        ExpiryHours = 24,
                        Instructions = new[]
                        {
                            "Kiểm tra hộp thư đến của bạn",
                            "Kiểm tra cả thư mục spam/junk mail",
                            "Click vào nút 'XÁC THỰC EMAIL' trong email",
                            "Sau khi xác thực, quay lại để đăng nhập"
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending verification email for {Email}", request.Email);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi gửi email xác thực",
                    ErrorCode = ErrorCodes.INTERNAL_ERROR
                });
            }
        }

        [HttpGet("email-status/{email}")]
        public async Task<IActionResult> CheckEmailVerificationStatus(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Email không được để trống",
                    ErrorCode = ErrorCodes.VALIDATION_ERROR
                });
            }

            try
            {
                var decodedEmail = Uri.UnescapeDataString(email);
                var isVerified = await _userService.IsEmailVerifiedAsync(decodedEmail);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Email status retrieved successfully",
                    Data = new
                    {
                        Email = decodedEmail,
                        IsVerified = isVerified,
                        CheckedAt = DateTime.UtcNow,
                        CanLogin = isVerified,
                        NextStep = isVerified ? "Bạn có thể đăng nhập ngay" : "Vui lòng xác thực email trước khi đăng nhập"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking email verification status for {Email}", email);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi kiểm tra trạng thái email",
                    ErrorCode = ErrorCodes.INTERNAL_ERROR
                });
            }
        }
    }
}