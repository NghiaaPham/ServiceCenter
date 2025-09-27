using EVServiceCenter.Core.Domains.Identity.Entities;
using EVServiceCenter.Core.Domains.Identity.DTOs.Responses;

namespace EVServiceCenter.Core.Domains.Identity.Interfaces
{
  public interface IUserService
  {
    Task<IEnumerable<UserResponseDto>> GetAllUsersAsync();
    Task<UserResponseDto?> GetUserByIdAsync(int id);
    Task<UserResponseDto> RegisterUserAsync(User user, string plainPassword);
    Task<UserResponseDto> UpdateUserAsync(User user);
    Task<bool> DeleteUserAsync(int id);
    Task<(UserResponseDto? User, string? ErrorCode, string? ErrorMessage)> LoginAsync(string username, string plainPassword);
    Task UpdateUserLastLoginAsync(int userId);
    Task UpdateUserPasswordAsync(int userId, string cucurrentPassword, string newPlainPassword);

    Task<bool> ForgotPasswordAsync(string email);
    Task<bool> ResetPasswordAsync(string email, string token, string newPassword);
    Task<bool> VerifyEmailAsync(string email, string token);
    Task<bool> ResendEmailVerificationAsync(string email);
    Task<bool> IsEmailVerifiedAsync(string email);
    Task<UserResponseDto?> GetUserByEmailAsync(string email);
    Task<bool> ValidateResetTokenAsync(string email, string token);
  }
}