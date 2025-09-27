using EVServiceCenter.Core.Domains.Identity.DTOs.Requests;
using EVServiceCenter.Core.Domains.Identity.DTOs.Responses;
using EVServiceCenter.Core.Domains.Identity.Interfaces;
using EVServiceCenter.Core.Domains.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVServiceCenter.API.Controllers.Account
{
    [Route("api/account")]
    [ApiController]
    [AllowAnonymous]
    public class AccountRecoveryController : BaseController
    {
        private readonly IUserService _userService;
        private readonly ILogger<AccountRecoveryController> _logger;
        private readonly IConfiguration _configuration;

        public AccountRecoveryController(
            IUserService userService,
            ILogger<AccountRecoveryController> logger,
            IConfiguration configuration)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        //[HttpPost("forgot-password")]
        //public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
        //{
        //    if (!IsValidRequest(request))
        //    {
        //        return BadRequest(new ApiResponse<object>
        //        {
        //            Success = false,
        //            Message = "Email is required",
        //            ErrorCode = "VALIDATION_ERROR"
        //        });
        //    }

        //    try
        //    {
        //        await _userService.ForgotPasswordAsync(request.Email);
        //        return Ok(new ApiResponse<object>
        //        {
        //            Success = true,
        //            Message = "If the email exists, a password reset link has been sent",
        //            Data = null
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error processing forgot password request for {Email}", request.Email);
        //        return StatusCode(500, new ApiResponse<object>
        //        {
        //            Success = false,
        //            Message = "Internal server error",
        //            ErrorCode = "INTERNAL_ERROR"
        //        });
        //    }
        //}

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
        {
            if (!IsValidRequest(request))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Vui lòng nhập địa chỉ email hợp lệ.",
                    ErrorCode = "VALIDATION_ERROR"
                });
            }

            // Additional email format validation
            if (!IsValidEmail(request.Email))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Định dạng email không hợp lệ.",
                    ErrorCode = "INVALID_EMAIL_FORMAT"
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

                var user = await _userService.GetUserByEmailAsync(decodedEmail);
                if (user == null)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Email không tồn tại.",
                        ErrorCode = "EMAIL_NOT_FOUND"
                    });
                }

                var isValidToken = await _userService.ValidateResetTokenAsync(decodedEmail, token);
                if (!isValidToken)
                {
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

        //[HttpPost("reset-password-submit")]
        //public async Task<IActionResult> SubmitPasswordReset([FromBody] ResetPasswordSubmitRequestDto request)
        //{
        //    if (!IsValidRequest(request))
        //    {
        //        return BadRequest(new ApiResponse<object>
        //        {
        //            Success = false,
        //            Message = "Dữ liệu không hợp lệ.",
        //            ErrorCode = "VALIDATION_ERROR"
        //        });
        //    }

        //    if (request.NewPassword != request.ConfirmPassword)
        //    {
        //        return BadRequest(new ApiResponse<object>
        //        {
        //            Success = false,
        //            Message = "Mật khẩu xác nhận không khớp.",
        //            ErrorCode = "PASSWORD_MISMATCH"
        //        });
        //    }

        //    if (request.NewPassword.Length < 6)
        //    {
        //        return BadRequest(new ApiResponse<object>
        //        {
        //            Success = false,
        //            Message = "Mật khẩu phải có ít nhất 6 ký tự.",
        //            ErrorCode = "PASSWORD_TOO_SHORT"
        //        });
        //    }

        //    try
        //    {
        //        var result = await _userService.ResetPasswordAsync(request.Email, request.Token, request.NewPassword);

        //        if (!result)
        //        {
        //            return BadRequest(new ApiResponse<object>
        //            {
        //                Success = false,
        //                Message = "Token không hợp lệ hoặc đã hết hạn.",
        //                ErrorCode = "INVALID_TOKEN"
        //            });
        //        }

        //        _logger.LogInformation("Password reset successful for {Email}", request.Email);

        //        var frontendUrl = _configuration["AppSettings:WebsiteUrl"];
        //        return Ok(new ApiResponse<PasswordResetResponse>
        //        {
        //            Success = true,
        //            Message = "Mật khẩu đã được đặt lại thành công.",
        //            Data = new PasswordResetResponse
        //            {
        //                Email = request.Email,
        //                ResetAt = DateTime.UtcNow,
        //                LoginUrl = $"{frontendUrl}/login"
        //            }
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error resetting password for {Email}", request.Email);
        //        return StatusCode(500, new ApiResponse<object>
        //        {
        //            Success = false,
        //            Message = "Có lỗi xảy ra khi đặt lại mật khẩu.",
        //            ErrorCode = "INTERNAL_ERROR"
        //        });
        //    }
        //}

        [HttpPost("reset-password-submit")]
        public async Task<IActionResult> SubmitPasswordReset([FromBody] ResetPasswordSubmitRequestDto request)
        {
            if (!IsValidRequest(request))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại thông tin.",
                    ErrorCode = "VALIDATION_ERROR"
                });
            }

            if (request.NewPassword != request.ConfirmPassword)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Mật khẩu xác nhận không khớp. Vui lòng nhập lại.",
                    ErrorCode = "PASSWORD_MISMATCH"
                });
            }

            if (request.NewPassword.Length < 6)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Mật khẩu phải có ít nhất 6 ký tự và chứa cả chữ và số.",
                    ErrorCode = "PASSWORD_TOO_SHORT"
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

        // Legacy endpoint for backward compatibility
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
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
                var result = await _userService.ResetPasswordAsync(request.Email, request.Token, request.NewPassword);
                if (!result)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid or expired reset token",
                        ErrorCode = "INVALID_TOKEN"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Password has been reset successfully",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Internal server error",
                    ErrorCode = "INTERNAL_ERROR"
                });
            }
        }
        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}