using EVServiceCenter.Core.Domains.Identity.DTOs.Requests;
using EVServiceCenter.Core.Domains.Identity.DTOs.Responses;
using EVServiceCenter.Core.Domains.Identity.Interfaces;
using EVServiceCenter.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EVServiceCenter.Core.Domains.Shared.Models;

namespace EVServiceCenter.API.Controllers.Auth
{
    [ApiController]
    [Route("api/verification")]
    [AllowAnonymous]
    [ApiExplorerSettings(GroupName = "Public - Verification")]
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

        /// <summary>
        /// Verify email via POST request (for API calls from frontend)
        /// </summary>
        [HttpPost("verify-email")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
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
                    _logger.LogWarning("Email verification failed for {Email} - Invalid or expired token", request.Email);
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Link xác thực không hợp lệ hoặc đã hết hạn",
                        ErrorCode = ErrorCodes.INVALID_TOKEN,
                        Data = new
                        {
                            CanResend = true,
                            ResendUrl = "/resend-verification"
                        }
                    });
                }

                // Gửi welcome email (không throw exception nếu fail)
                await SendWelcomeEmailSafelyAsync(request.Email);

                _logger.LogInformation("Email verification successful for {Email}", request.Email);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Email đã được xác thực thành công! Chào mừng đến với EV Service Center.",
                    Data = new
                    {
                        EmailVerified = true,
                        LoginUrl = $"{GetFrontendUrl()}/login",
                        Message = "Bạn có thể đăng nhập ngay bây giờ!",
                        WelcomeEmailSent = true
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying email for {Email}", request.Email);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra trong quá trình xác thực. Vui lòng thử lại sau.",
                    ErrorCode = ErrorCodes.INTERNAL_ERROR
                });
            }
        }

        /// <summary>
        /// Verify email via GET request (for direct link clicks from email)
        /// Redirects to frontend with result
        /// </summary>
        [HttpGet("verify-email")]
        [ProducesResponseType(302)]
        public async Task<IActionResult> VerifyEmailFromLink([FromQuery] string token, [FromQuery] string email)
        {
            var frontendUrl = GetFrontendUrl();

            // Validate parameters
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(email))
            {
                _logger.LogWarning("Email verification attempted with missing parameters");
                return Redirect($"{frontendUrl}/verify-email?error=invalid-params&message={EncodeMessage("Thiếu thông tin xác thực. Vui lòng kiểm tra lại link trong email.")}");
            }

            try
            {
                var decodedEmail = Uri.UnescapeDataString(email);
                _logger.LogInformation("Processing email verification from link for {Email}", decodedEmail);

                // Verify email
                var result = await _userService.VerifyEmailAsync(decodedEmail, token);

                if (!result)
                {
                    _logger.LogWarning("Email verification failed for {Email} - Invalid or expired token", decodedEmail);
                    return Redirect($"{frontendUrl}/verify-email?error=invalid-token&email={Uri.EscapeDataString(decodedEmail)}&message={EncodeMessage("Link xác thực đã hết hạn hoặc không hợp lệ. Vui lòng yêu cầu gửi lại link mới.")}");
                }

                // Send welcome email asynchronously (don't wait, don't fail the verification if email fails)
                _ = SendWelcomeEmailSafelyAsync(decodedEmail);

                _logger.LogInformation("Email verification successful for {Email}", decodedEmail);

                // Redirect to frontend with success
                return Redirect($"{frontendUrl}/verify-email?success=true&email={Uri.EscapeDataString(decodedEmail)}&message={EncodeMessage("Xác thực thành công! Chào mừng bạn đến với EV Service Center.")}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during email verification for {Email}", email);
                return Redirect($"{frontendUrl}/verify-email?error=server-error&message={EncodeMessage("Có lỗi hệ thống xảy ra. Vui lòng thử lại sau hoặc liên hệ hỗ trợ.")}");
            }
        }

        /// <summary>
        /// Resend email verification link
        /// </summary>
        [HttpPost("resend-verification")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 429)] // Too Many Requests
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
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
                    _logger.LogWarning("Resend verification attempted for non-existent email: {Email}", request.Email);
                    // Fake success response để tránh email enumeration
                    await Task.Delay(Random.Shared.Next(500, 1500));

                    return Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Nếu email tồn tại trong hệ thống, link xác thực đã được gửi. Vui lòng kiểm tra hộp thư của bạn.",
                        Data = new { EmailSent = true }
                    });
                }

                // Check if already verified
                if (user.EmailVerified)
                {
                    _logger.LogInformation("Resend verification attempted for already verified email: {Email}", request.Email);
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Email đã được xác thực trước đó",
                        ErrorCode = ErrorCodes.EMAIL_ALREADY_VERIFIED,
                        Data = new
                        {
                            AlreadyVerified = true,
                            LoginUrl = $"{GetFrontendUrl()}/login",
                            Message = "Tài khoản của bạn đã được xác thực. Bạn có thể đăng nhập ngay."
                        }
                    });
                }

                // Resend verification email
                var result = await _userService.ResendEmailVerificationAsync(request.Email);

                if (!result)
                {
                    _logger.LogError("Failed to resend verification email for {Email}", request.Email);
                    return StatusCode(500, new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Không thể gửi email xác thực lúc này. Vui lòng thử lại sau ít phút.",
                        ErrorCode = ErrorCodes.INTERNAL_ERROR
                    });
                }

                _logger.LogInformation("Verification email resent successfully to {Email}", request.Email);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Link xác thực mới đã được gửi đến email của bạn. Vui lòng kiểm tra hộp thư.",
                    Data = new
                    {
                        EmailSent = true,
                        Email = request.Email,
                        ExpiryHours = 24,
                        SentAt = DateTime.UtcNow,
                        Instructions = new[]
                        {
                            "Kiểm tra hộp thư đến (Inbox)",
                            "Kiểm tra cả thư mục spam/junk mail",
                            "Click vào nút 'XÁC THỰC EMAIL' trong email",
                            "Link sẽ tự động chuyển bạn đến trang xác nhận",
                            "Sau khi xác thực thành công, quay lại để đăng nhập"
                        },
                        Tips = new[]
                        {
                            "Link xác thực có hiệu lực trong 24 giờ",
                            "Nếu không thấy email sau 5 phút, hãy kiểm tra spam",
                            "Bạn chỉ cần xác thực 1 lần duy nhất"
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
                    Message = "Có lỗi xảy ra khi gửi email xác thực. Vui lòng thử lại sau.",
                    ErrorCode = ErrorCodes.INTERNAL_ERROR
                });
            }
        }

        /// <summary>
        /// Check email verification status
        /// </summary>
        [HttpGet("email-status/{email}")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
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

                _logger.LogInformation("Email verification status checked for {Email}: {Status}", decodedEmail, isVerified);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Trạng thái xác thực email được lấy thành công",
                    Data = new
                    {
                        Email = decodedEmail,
                        IsVerified = isVerified,
                        CheckedAt = DateTime.UtcNow,
                        CanLogin = isVerified,
                        Status = isVerified ? "verified" : "pending",
                        NextStep = isVerified
                            ? "Bạn có thể đăng nhập ngay bây giờ"
                            : "Vui lòng xác thực email trước khi đăng nhập",
                        Actions = isVerified
                            ? new[] { "Đăng nhập", "Truy cập trang chủ" }
                            : new[] { "Kiểm tra email", "Gửi lại link xác thực" }
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

        #region Private Helper Methods

        /// <summary>
        /// Get frontend URL from configuration with validation
        /// </summary>
        private string GetFrontendUrl()
        {
            var url = _configuration["AppSettings:WebsiteUrl"];

            if (string.IsNullOrWhiteSpace(url))
            {
                _logger.LogError("WebsiteUrl configuration is missing or empty");
                throw new InvalidOperationException("WebsiteUrl configuration is required but not found in appsettings");
            }

            return url.TrimEnd('/'); // Remove trailing slash for consistency
        }

        /// <summary>
        /// Send welcome email safely without throwing exceptions
        /// </summary>
        private async Task SendWelcomeEmailSafelyAsync(string email)
        {
            try
            {
                var user = await _userService.GetUserByEmailAsync(email);

                if (user != null)
                {
                    await _emailService.SendWelcomeEmailAsync(user.Email, user.FullName);
                    _logger.LogInformation("Welcome email sent successfully to {Email}", email);
                }
                else
                {
                    _logger.LogWarning("Could not send welcome email - user not found for {Email}", email);
                }
            }
            catch (Exception ex)
            {
                // Log but don't throw - welcome email is nice-to-have, not critical
                _logger.LogWarning(ex, "Failed to send welcome email to {Email}. Verification still successful.", email);
            }
        }

        /// <summary>
        /// Encode message for URL parameter
        /// </summary>
        private string EncodeMessage(string message)
        {
            return Uri.EscapeDataString(message);
        }

        #endregion
    }
}