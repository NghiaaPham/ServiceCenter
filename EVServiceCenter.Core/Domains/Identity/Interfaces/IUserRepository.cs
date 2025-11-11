using EVServiceCenter.Core.Domains.Identity.Entities;
using EVServiceCenter.Core.Domains.Shared.Interfaces;

namespace EVServiceCenter.Core.Domains.Identity.Interfaces
{
  public interface IUserRepository : IRepository<User>
  {
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
    Task<bool> IsUsernameExistsAsync(string username);
    Task<bool> IsEmailExistsAsync(string email);
    Task<IEnumerable<User>> GetByRoleAsync(int roleId);
    Task<User?> ValidateUserAsync(string username, byte[] passwordHash, byte[] passwordSalt);
    Task UpdateLastLoginAsync(int userId);
    Task UpdatePasswordAsync(int userId, byte[] newPasswordHash, byte[] newPasswordSalt);
    Task IncrementFailedLoginAttemptsAsync(int userId);

    Task<string?> GetMaxEmployeeCodeAsync(string prefix);
    Task<bool> IsEmployeeCodeExistsAsync(string code);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetByRefreshTokenAsync(string refreshToken);
  }
}
