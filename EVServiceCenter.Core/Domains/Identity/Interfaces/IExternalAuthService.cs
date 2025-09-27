using EVServiceCenter.Core.Domains.Identity.DTOs.Requests;
using EVServiceCenter.Core.Domains.Identity.DTOs.Responses;

namespace EVServiceCenter.Core.Domains.Identity.Interfaces
{
    public interface IExternalAuthService
    {
        Task<ExternalLoginResponseDto> GoogleLoginAsync(GoogleLoginRequestDto request);
        Task<ExternalLoginResponseDto> FacebookLoginAsync(FacebookLoginRequestDto request);
        Task<ExternalUserInfoDto?> VerifyGoogleTokenAsync(string idToken);
        Task<ExternalUserInfoDto?> VerifyFacebookTokenAsync(string accessToken);
        Task<bool> LinkExternalAccountAsync(int userId, string provider, string externalId);
        Task<bool> UnlinkExternalAccountAsync(int userId, string provider);
    }
}
