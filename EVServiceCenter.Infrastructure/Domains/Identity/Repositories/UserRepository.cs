using EVServiceCenter.Core.Entities;
using EVServiceCenter.Core.Constants;
using EVServiceCenter.Core.Enums;
using EVServiceCenter.Core.Domains.Identity.Entities;
using EVServiceCenter.Core.Domains.Identity.Interfaces;
using EVServiceCenter.Infrastructure.Domains.Shared.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace EVServiceCenter.Infrastructure.Domains.Identity.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(EVDbContext context) : base(context) { }

    public override async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
      {
        return await _dbSet
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserId == id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty.", nameof(email));

        return await _dbSet
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == email && u.IsActive == true);
    }

    public async Task<IEnumerable<User>> GetByRoleAsync(int roleId)
    {
        if (!Enum.IsDefined(typeof(UserRoles), roleId))
            throw new ArgumentException(ErrorMessages.VALIDATION_ERROR, nameof(roleId));

        return await _dbSet
            .Include(u => u.Role)
            .Where(u => u.RoleId == roleId && u.IsActive == true)
            .ToListAsync();
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be empty.", nameof(username));

        return await _dbSet
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Username == username && u.IsActive == true);
    }

    public async Task<bool> IsUsernameExistsAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be empty.", nameof(username));

        return await _dbSet.AnyAsync(u => u.Username == username);
    }

    public async Task<bool> IsEmailExistsAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty.", nameof(email));

        return await _dbSet.AnyAsync(u => u.Email == email);
    }

    public async Task UpdateLastLoginAsync(int userId)
    {
        var user = await GetByIdAsync(userId);
        if (user is null) return;

        user.LastLoginDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<User?> ValidateUserAsync(string username, byte[] passwordHash, byte[] passwordSalt)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be empty.", nameof(username));

        if (passwordHash is null || passwordSalt is null)
            throw new ArgumentNullException(nameof(passwordHash), "Password data cannot be null.");

        var user = await _dbSet
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Username == username && u.IsActive == true);

        if (user is null) return null;

        // TODO: Replace with secure comparison logic
        string inputHash = BCrypt.Net.BCrypt.HashPassword(
            Encoding.UTF8.GetString(passwordHash),
            Encoding.UTF8.GetString(user.PasswordSalt)
        );

        return inputHash == Encoding.UTF8.GetString(user.PasswordHash) ? user : null;
    }

    public async Task UpdatePasswordAsync(int userId, byte[] newPasswordHash, byte[] newPasswordSalt)
    {
        if (newPasswordHash is null || newPasswordSalt is null)
            throw new ArgumentNullException(nameof(newPasswordHash), "Password data cannot be null.");

        var user = await GetByIdAsync(userId);
        if (user is null) return;

        user.PasswordHash = newPasswordHash;
        user.PasswordSalt = newPasswordSalt;
        await _context.SaveChangesAsync();
    }

    public async Task IncrementFailedLoginAttemptsAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user is null) return;

        user.FailedLoginAttempts = (user.FailedLoginAttempts ?? 0) + 1;
        user.LastFailedLoginAttempt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task<string?> GetMaxEmployeeCodeAsync(string prefix)
    {
        return await _dbSet
            .Where(u => u.EmployeeCode != null && u.EmployeeCode.StartsWith(prefix))
            .OrderByDescending(u => u.EmployeeCode)
            .Select(u => u.EmployeeCode)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> IsEmployeeCodeExistsAsync(string code)
    {
        return await _dbSet.AnyAsync(u => u.EmployeeCode == code);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await GetByEmailAsync(email);
    }
}
