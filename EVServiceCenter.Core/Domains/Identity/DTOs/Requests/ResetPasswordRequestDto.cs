using EVServiceCenter.Core.Constants;
using System.ComponentModel.DataAnnotations;

namespace EVServiceCenter.Core.Domains.Identity.DTOs.Requests
{
    public class ResetPasswordRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        [StringLength(SystemConstants.PASSWORD_MAX_LENGTH, MinimumLength = SystemConstants.PASSWORD_MIN_LENGTH)]
        public string NewPassword { get; set; } = string.Empty;
    }
}