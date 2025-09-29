using EVServiceCenter.Core.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVServiceCenter.Core.Domains.Identity.DTOs.Requests
{
    public class CustomerRegistrationDto
    {
        [Required]
        [StringLength(SystemConstants.USERNAME_MAX_LENGTH, MinimumLength = SystemConstants.USERNAME_MIN_LENGTH)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(SystemConstants.PASSWORD_MAX_LENGTH, MinimumLength = SystemConstants.PASSWORD_MIN_LENGTH)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        // ONLY ESSENTIAL FIELDS - Other info can be updated later
        [Required]
        public bool AcceptTerms { get; set; } // Required for legal compliance

        // Optional marketing consent
        public bool MarketingOptIn { get; set; } = false; // Default to false for privacy
    }
}
