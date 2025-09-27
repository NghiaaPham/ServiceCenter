using System.ComponentModel.DataAnnotations;
using EVServiceCenter.Core.Constants;

namespace EVServiceCenter.Core.Domains.Identity.DTOs.Requests
{
    public class ChangePasswordRequestDto
    {
        [Required]
        [StringLength(SystemConstants.PASSWORD_MAX_LENGTH, MinimumLength = SystemConstants.PASSWORD_MIN_LENGTH)]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        [StringLength(SystemConstants.PASSWORD_MAX_LENGTH, MinimumLength = SystemConstants.PASSWORD_MIN_LENGTH)]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [Compare(nameof(NewPassword), ErrorMessage = "Password confirmation does not match")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}