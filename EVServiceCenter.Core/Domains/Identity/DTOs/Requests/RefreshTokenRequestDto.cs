using System.ComponentModel.DataAnnotations;

namespace EVServiceCenter.Core.Domains.Identity.DTOs.Requests
{
    public class RefreshTokenRequestDto
    {
        [Required(ErrorMessage = "Refresh token is required.")]
        public string RefreshToken { get; set; } = null!;
    }
}