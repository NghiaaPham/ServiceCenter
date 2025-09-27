using System.ComponentModel.DataAnnotations;

namespace EVServiceCenter.Core.Domains.Identity.DTOs.Requests
{
    public class ForgotPasswordRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
