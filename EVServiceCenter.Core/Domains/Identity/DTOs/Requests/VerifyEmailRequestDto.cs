using System.ComponentModel.DataAnnotations;

namespace EVServiceCenter.Core.Domains.Identity.DTOs.Requests
{
    public class VerifyEmailRequestDto
    {
        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
