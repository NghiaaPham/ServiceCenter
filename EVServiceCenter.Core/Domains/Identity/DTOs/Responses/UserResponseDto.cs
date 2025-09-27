namespace EVServiceCenter.Core.Domains.Identity.DTOs.Responses
{
    public class UserResponseDto
    {
        // Basic Information
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; }

        // Role Information  
        public int RoleId { get; set; }
        public string? RoleName { get; set; }

        // Employee Information (for internal users)
        public string? EmployeeCode { get; set; }
        public string? Department { get; set; }
        public DateOnly? HireDate { get; set; }

        // Audit Information
        public DateTime? CreatedDate { get; set; }
        public DateTime? LastLoginDate { get; set; }

        // Security Information (minimal - for admin view)
        public int? FailedLoginAttempts { get; set; }

        // Email Verification Information - THÊM MỚI
        public bool EmailVerified { get; set; }

        // Computed Properties
        public bool IsInternal => RoleId == 1 || RoleId == 2 || RoleId == 3; // Admin, Staff, Technician
        public bool IsLocked => FailedLoginAttempts >= 5;
        public string DisplayName => !string.IsNullOrEmpty(FullName) ? FullName : Username;
    }
}