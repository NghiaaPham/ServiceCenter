using System.ComponentModel.DataAnnotations;
using EVServiceCenter.Core.Constants;

namespace EVServiceCenter.Core.Domains.Identity.DTOs.Requests
{
  public class LoginRequestDto
  {
    [Required]
    [StringLength(SystemConstants.USERNAME_MAX_LENGTH, MinimumLength = SystemConstants.USERNAME_MIN_LENGTH)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(SystemConstants.PASSWORD_MAX_LENGTH, MinimumLength = SystemConstants.PASSWORD_MIN_LENGTH)]
    public string Password { get; set; } = string.Empty;
  }
}