using EVServiceCenter.Core.Domains.Identity.DTOs.Requests;
using EVServiceCenter.Core.Domains.Identity.DTOs.Responses;
using EVServiceCenter.Core.Domains.Identity.Interfaces;
using EVServiceCenter.Core.Domains.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVServiceCenter.API.Controllers.Auth
{
    [ApiController]
    [Route("api/auth/external")]
    [AllowAnonymous]
    [ApiExplorerSettings(GroupName = "Public - Authentication")]
    public class ExternalAuthController : BaseController
    {
        private readonly IExternalAuthService _externalAuthService;
        private readonly ILogger<ExternalAuthController> _logger;

        public ExternalAuthController(
            IExternalAuthService externalAuthService,
            ILogger<ExternalAuthController> logger)
        {
            _externalAuthService = externalAuthService;
            _logger = logger;
        }

        [HttpPost("google")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequestDto request)
        {
            if (!ModelState.IsValid)
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
                var result = await _externalAuthService.GoogleLoginAsync(request);

                if (!result.Success)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = result.Message,
                        ErrorCode = result.ErrorCode
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = result.IsNewUser ? "Đăng ký thành công" : "Đăng nhập thành công",
                    Data = new
                    {
                        User = result.User,
                        Token = result.AccessToken,
                        RefreshToken = result.RefreshToken,
                        IsNewUser = result.IsNewUser
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Google login");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra trong quá trình đăng nhập",
                    ErrorCode = "INTERNAL_ERROR"
                });
            }
        }

        [HttpPost("facebook")]
        public async Task<IActionResult> FacebookLogin([FromBody] FacebookLoginRequestDto request)
        {
            if (!ModelState.IsValid)
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
                var result = await _externalAuthService.FacebookLoginAsync(request);

                if (!result.Success)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = result.Message,
                        ErrorCode = result.ErrorCode
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = result.IsNewUser ? "Đăng ký thành công" : "Đăng nhập thành công",
                    Data = new
                    {
                        User = result.User,
                        Token = result.AccessToken,
                        RefreshToken = result.RefreshToken,
                        IsNewUser = result.IsNewUser
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Facebook login");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra trong quá trình đăng nhập",
                    ErrorCode = "INTERNAL_ERROR"
                });
            }
        }

        [HttpPost("link")]
        [Authorize]
        public async Task<IActionResult> LinkExternalAccount([FromBody] LinkExternalAccountRequestDto request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _externalAuthService.LinkExternalAccountAsync(
                    userId,
                    request.Provider,
                    request.ExternalId
                );

                if (!result)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Không thể liên kết tài khoản",
                        ErrorCode = "LINK_FAILED"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = $"Đã liên kết tài khoản {request.Provider} thành công"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error linking external account");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra",
                    ErrorCode = "INTERNAL_ERROR"
                });
            }
        }

        [HttpDelete("unlink/{provider}")]
        [Authorize]
        public async Task<IActionResult> UnlinkExternalAccount(string provider)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _externalAuthService.UnlinkExternalAccountAsync(userId, provider);

                if (!result)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Không thể hủy liên kết tài khoản",
                        ErrorCode = "UNLINK_FAILED"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = $"Đã hủy liên kết tài khoản {provider}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unlinking external account");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra",
                    ErrorCode = "INTERNAL_ERROR"
                });
            }
        }
    }
}