using System.ComponentModel.DataAnnotations;
using EVServiceCenter.Core.Constants;

namespace EVServiceCenter.Core.Domains.Identity.DTOs.Requests
{
  public class RegisterRequestDto
  {
    [Required]
    [StringLength(SystemConstants.USERNAME_MAX_LENGTH, MinimumLength = SystemConstants.USERNAME_MIN_LENGTH)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(SystemConstants.PASSWORD_MAX_LENGTH, MinimumLength = SystemConstants.PASSWORD_MIN_LENGTH)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [StringLength(SystemConstants.USERNAME_MAX_LENGTH)]
    public string FullName { get; set; } = string.Empty;

    [StringLength(SystemConstants.USERNAME_MAX_LENGTH)]
    public string? Email { get; set; }

    [StringLength(SystemConstants.USERNAME_MAX_LENGTH)]
    public string? PhoneNumber { get; set; }

    [Required]
    public int RoleId { get; set; }

    public string? Department { get; set; }
    public string? EmployeeCode { get; set; }
    public DateOnly? HireDate { get; set; }
    public decimal? Salary { get; set; }
    }
}