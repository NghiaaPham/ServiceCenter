using EVServiceCenter.Core.Domains.Identity.DTOs.Requests;
using EVServiceCenter.Core.Domains.Identity.DTOs.Responses;
using EVServiceCenter.Core.Domains.Identity.Interfaces;
using EVServiceCenter.Core.Domains.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace EVServiceCenter.API.Controllers.Account
{
    [Route("api/account")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "🔐 Xác thực & Tài khoản (Public)")]
    [AllowAnonymous]
    public class AccountRecoveryController : BaseController
    {
        private readonly IUserService _userService;
        private readonly ILogger<AccountRecoveryController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IValidator<ForgotPasswordRequestDto> _forgotPasswordValidator;
        private readonly IValidator<ResetPasswordSubmitRequestDto> _resetPasswordValidator;

        public AccountRecoveryController(
            IUserService userService,
            ILogger<AccountRecoveryController> logger,
            IConfiguration configuration,
            IValidator<ForgotPasswordRequestDto> forgotPasswordValidator,
            IValidator<ResetPasswordSubmitRequestDto> resetPasswordValidator)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _forgotPasswordValidator = forgotPasswordValidator ?? throw new ArgumentNullException(nameof(forgotPasswordValidator));
            _resetPasswordValidator = resetPasswordValidator ?? throw new ArgumentNullException(nameof(resetPasswordValidator));
        }

        /// <summary>
        /// [Quên mật khẩu] Gửi email reset password
        /// </summary>
        /// <remarks>Nhận email và gửi link reset password nếu email tồn tại. Link có hiệu lực trong 1 giờ.</remarks>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
        {
            var validationResult = await _forgotPasswordValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Dữ liệu không hợp lệ.",
                    ErrorCode = "VALIDATION_ERROR",
                    ValidationErrors = validationResult.Errors.Select(e => new { Field = e.PropertyName, Message = e.ErrorMessage })
                });
            }

            try
            {
                await _userService.ForgotPasswordAsync(request.Email);

                // Log the request for security monitoring
                _logger.LogInformation("Password reset requested for {Email} from IP: {IP}, UserAgent: {UserAgent}",
                    request.Email,
                    HttpContext.Connection?.RemoteIpAddress,
                    Request.Headers["User-Agent"].ToString()
                );

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Nếu email tồn tại trong hệ thống, bạn sẽ nhận được hướng dẫn đặt lại mật khẩu trong vòng vài phút.",
                    Data = new
                    {
                        Email = request.Email,
                        RequestedAt = DateTime.UtcNow,
                        ExpiryMinutes = 60,
                        Instructions = new[]
                        {
                    "Kiểm tra hộp thư đến của bạn",
                    "Nếu không thấy email, hãy kiểm tra thư mục spam",
                    "Link đặt lại mật khẩu có hiệu lực trong 1 giờ",
                    "Nếu vẫn không nhận được email, vui lòng liên hệ hỗ trợ"
                }
                    }
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message,
                    ErrorCode = "VALIDATION_ERROR"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing forgot password request for {Email}", request.Email);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra. Vui lòng thử lại sau hoặc liên hệ hỗ trợ.",
                    ErrorCode = "INTERNAL_ERROR"
                });
            }
        }

        /// <summary>
        /// [Xác thực] Kiểm tra token reset password
        /// </summary>
        /// <remarks>Kiểm tra xem token và email có hợp lệ để reset password hay không.</remarks>
        [HttpGet("validate-reset-token")]
        public async Task<IActionResult> ValidateResetToken([FromQuery] string token, [FromQuery] string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Token và email không được để trống.",
                    ErrorCode = "INVALID_PARAMETERS"
                });
            }

            try
            {
                var decodedEmail = Uri.UnescapeDataString(email);
                _logger.LogInformation("Validating password reset token for {Email}", decodedEmail);

                var isValidToken = await _userService.ValidateResetTokenAsync(decodedEmail, token);
                if (!isValidToken)
                {
                    // This message is intentionally generic to prevent email enumeration.
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Token không hợp lệ hoặc đã hết hạn.",
                        ErrorCode = "INVALID_TOKEN"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Token hợp lệ. Có thể đặt lại mật khẩu.",
                    Data = new { Email = decodedEmail, TokenValid = true }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating reset token for {Email}", email);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra.",
                    ErrorCode = "INTERNAL_ERROR"
                });
            }
        }


        /// <summary>
        /// [Đặt lại mật khẩu] Gửi thông tin để đặt lại mật khẩu mới
        /// </summary>
        /// <remarks>Nhận token, email và mật khẩu mới để hoàn tất quá trình reset.</remarks>
        [HttpPost("reset-password")]
        public async Task<IActionResult> SubmitPasswordReset([FromBody] ResetPasswordSubmitRequestDto request)
        {
            var validationResult = await _resetPasswordValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại thông tin.",
                    ErrorCode = "VALIDATION_ERROR",
                    ValidationErrors = validationResult.Errors.Select(e => new { Field = e.PropertyName, Message = e.ErrorMessage })
                });
            }

            try
            {
                var result = await _userService.ResetPasswordAsync(request.Email, request.Token, request.NewPassword);

                if (!result)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Link đặt lại mật khẩu không hợp lệ hoặc đã hết hạn. Vui lòng yêu cầu link mới.",
                        ErrorCode = "INVALID_TOKEN"
                    });
                }

                _logger.LogInformation("Password reset successful for {Email} from IP: {IP}",
                    request.Email, HttpContext.Connection?.RemoteIpAddress);

                var frontendUrl = _configuration["AppSettings:WebsiteUrl"];
                return Ok(new ApiResponse<PasswordResetResponse>
                {
                    Success = true,
                    Message = "Mật khẩu đã được đặt lại thành công. Bạn có thể đăng nhập với mật khẩu mới.",
                    Data = new PasswordResetResponse
                    {
                        Email = request.Email,
                        ResetAt = DateTime.UtcNow,
                        LoginUrl = $"{frontendUrl}/login"
                    }
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message,
                    ErrorCode = "VALIDATION_ERROR"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for {Email}", request.Email);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi đặt lại mật khẩu. Vui lòng thử lại sau.",
                    ErrorCode = "INTERNAL_ERROR"
                });
            }
        }

    }
}