using System.ComponentModel.DataAnnotations;
using EVServiceCenter.Core.Constants;

namespace EVServiceCenter.Core.Domains.Identity.DTOs.Requests
{
    public class UpdateUserRequestDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(SystemConstants.USERNAME_MAX_LENGTH, MinimumLength = SystemConstants.USERNAME_MIN_LENGTH)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(50)]
        public string? Department { get; set; }

        public bool IsActive { get; set; } = true;

        public int? RoleId { get; set; }
    }
}