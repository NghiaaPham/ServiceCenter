using AutoMapper;
using EVServiceCenter.Core.Constants;
using EVServiceCenter.Core.Domains.Identity.DTOs.Responses;
using EVServiceCenter.Core.Domains.Identity.Entities;
using EVServiceCenter.Core.Domains.Identity.Interfaces;
using EVServiceCenter.Core.Enums;
using EVServiceCenter.Core.Helpers;
using EVServiceCenter.Core.Domains.Shared.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text;

namespace EVServiceCenter.Infrastructure.Domains.Identity.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;
        private readonly IEmailService _emailService;
        private readonly IHttpContextService _httpContextService;

        public UserService(
            IUserRepository userRepository,
            IMapper mapper,
            ILogger<UserService> logger,
            IEmailService emailService,
            IHttpContextService httpContextService)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _httpContextService = httpContextService ?? throw new ArgumentNullException(nameof(httpContextService));
        }

        public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<UserResponseDto>>(users);
        }

        public async Task<UserResponseDto?> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            return user == null ? null : _mapper.Map<UserResponseDto>(user);
        }

        public async Task<UserResponseDto> RegisterCustomerUserAsync(User user, string plainPassword)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            // Validate this is customer user
            if (user.RoleId != (int)UserRoles.Customer)
            {
                throw new ArgumentException("This method is only for customer registration", nameof(user.RoleId));
            }

            // Validate password strength
            var (isValid, errorMessage) = PasswordValidator.ValidatePassword(plainPassword);
            if (!isValid)
            {
                throw new ArgumentException(errorMessage, nameof(plainPassword));
            }

            // Check for duplicates
            if (await _userRepository.IsUsernameExistsAsync(user.Username))
                throw new InvalidOperationException(ErrorMessages.DUPLICATE_USERNAME);

            if (!string.IsNullOrEmpty(user.Email) && await _userRepository.IsEmailExistsAsync(user.Email))
                throw new InvalidOperationException(ErrorMessages.DUPLICATE_EMAIL);

            // Password hashing
            var salt = SecurityHelper.GenerateSalt();
            var hash = SecurityHelper.HashPassword(plainPassword, salt);
            var hashBytes = Encoding.UTF8.GetBytes(hash);
            var saltBytes = Encoding.UTF8.GetBytes(salt);

            if (hashBytes.Length > 64 || saltBytes.Length > 32)
                throw new InvalidOperationException("Password hash/salt exceeds database limits.");

            user.PasswordHash = hashBytes;
            user.PasswordSalt = saltBytes;
            user.IsActive = true;
            user.CreatedDate = DateTime.UtcNow;

            // Customer: Email verification required
            user.EmailVerified = false;
            var emailVerificationToken = SecurityHelper.GenerateSecureToken();
            user.EmailVerificationToken = Encoding.UTF8.GetBytes(emailVerificationToken);
            user.EmailVerificationExpiry = DateTime.UtcNow.AddHours(24);

            var createdUser = await _userRepository.CreateAsync(user);

            // Send verification email for customers
            if (!string.IsNullOrEmpty(user.Email))
            {
                try
                {
                    await _emailService.SendCustomerEmailVerificationAsync(
                        user.Email,
                        user.FullName,
                        emailVerificationToken
                    );
                    _logger.LogInformation("Verification email sent to customer {Email} for user {Username}", user.Email, user.Username);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send verification email for customer {Username}", user.Username);
                    // Don't fail registration if email fails
                }
            }

            _logger.LogInformation("Customer user registered successfully: {Username}, Email: {Email}",
                user.Username, user.Email);

            return _mapper.Map<UserResponseDto>(createdUser);
        }

        public async Task<UserResponseDto> UpdateUserAsync(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            var existingUser = await _userRepository.GetByIdAsync(user.UserId);
            if (existingUser == null)
                throw new InvalidOperationException(ErrorMessages.USER_NOT_FOUND);

            // Check for duplicate username (if changed)
            if (existingUser.Username != user.Username)
            {
                if (await _userRepository.IsUsernameExistsAsync(user.Username))
                    throw new InvalidOperationException(ErrorMessages.DUPLICATE_USERNAME);
            }

            // Check for duplicate email (if changed and not null)
            if (!string.IsNullOrEmpty(user.Email) && existingUser.Email != user.Email)
            {
                if (await _userRepository.IsEmailExistsAsync(user.Email))
                    throw new InvalidOperationException(ErrorMessages.DUPLICATE_EMAIL);
            }

            // Update allowed fields only
            existingUser.Username = user.Username;
            existingUser.FullName = user.FullName;
            existingUser.Email = user.Email;
            existingUser.PhoneNumber = user.PhoneNumber;
            existingUser.Department = user.Department;
            existingUser.IsActive = user.IsActive;

            // Only update role if it's different (for admin changes)
            if (existingUser.RoleId != user.RoleId)
            {
                if (!Enum.IsDefined(typeof(UserRoles), user.RoleId))
                    throw new ArgumentException($"{ErrorMessages.VALIDATION_ERROR}: Invalid role", nameof(user.RoleId));

                existingUser.RoleId = user.RoleId;

                // Update employee code if role changed to/from internal roles
                if (user.RoleId == (int)UserRoles.Admin ||
                    user.RoleId == (int)UserRoles.Staff ||
                    user.RoleId == (int)UserRoles.Technician)
                {
                    if (string.IsNullOrEmpty(existingUser.EmployeeCode))
                    {
                        existingUser.EmployeeCode = await GenerateUniqueEmployeeCodeAsync();
                    }
                }
                else if (user.RoleId == (int)UserRoles.Customer)
                {
                    existingUser.EmployeeCode = null;
                }
            }

            var updatedUser = await _userRepository.UpdateAsync(existingUser);
            _logger.LogInformation("User updated: {UserId} by system", user.UserId);

            return _mapper.Map<UserResponseDto>(updatedUser);
        }

        public async Task<(UserResponseDto? User, string? ErrorCode, string? ErrorMessage)> LoginAsync(string username, string plainPassword)
        {
            try
            {
                // 1. Validate input
                if (string.IsNullOrWhiteSpace(username))
                {
                    _logger.LogWarning("Login attempt with empty username");
                    return (null, ErrorCodes.VALIDATION_ERROR, "Tên đăng nhập không được để trống.");
                }

                if (string.IsNullOrWhiteSpace(plainPassword))
                {
                    _logger.LogWarning("Login attempt with empty password for username: {Username}", username);
                    return (null, ErrorCodes.VALIDATION_ERROR, "Mật khẩu không được để trống.");
                }

                // 2. Get user from database
                var user = await _userRepository.GetByUsernameAsync(username);

                if (user == null)
                {
                    _logger.LogWarning("Login failed: Username {Username} not found", username);
                    // Delay response to prevent timing attacks
                    await Task.Delay(Random.Shared.Next(100, 500));
                    return (null, ErrorCodes.INVALID_CREDENTIALS, ErrorMessages.INVALID_USERNAME_PASSWORD);
                }

                // 3. Check if account is inactive
                if (user.IsActive == false)
                {
                    _logger.LogWarning("Login failed: Account {Username} is inactive", username);
                    return (null, ErrorCodes.ACCOUNT_LOCKED, "Tài khoản của bạn đã bị vô hiệu hóa. Vui lòng liên hệ quản trị viên.");
                }

                // 4. Check account lockout status
                if (user.IsAccountLocked && user.AccountLockedUntil.HasValue)
                {
                    if (user.AccountLockedUntil > DateTime.UtcNow)
                    {
                        // Still locked
                        var remainingTime = (user.AccountLockedUntil.Value - DateTime.UtcNow).TotalMinutes;
                        _logger.LogWarning("Login failed: Account {Username} is locked until {LockoutEnd}",
                            username, user.AccountLockedUntil);

                        return (null, ErrorCodes.ACCOUNT_LOCKED,
                            $"Tài khoản đã bị khóa do đăng nhập sai quá nhiều lần. Vui lòng thử lại sau {Math.Ceiling(remainingTime)} phút.");
                    }
                    else
                    {
                        // Lockout period has expired, unlock the account
                        _logger.LogInformation("Auto-unlocking account {Username} after lockout expiry", username);
                        user.IsAccountLocked = false;
                        user.AccountLockedUntil = null;
                        user.FailedLoginAttempts = 0;
                        user.UnlockAttempts = 0;
                        user.LockoutReason = null;
                        await _userRepository.UpdateAsync(user);
                    }
                }

                // 5. Verify password using SecurityHelper
                bool isPasswordValid = false;
                try
                {
                    isPasswordValid = SecurityHelper.VerifyPassword(
                        plainPassword,
                        Encoding.UTF8.GetString(user.PasswordHash)
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error verifying password for user {Username}", username);
                    return (null, ErrorCodes.INTERNAL_ERROR, "Có lỗi xảy ra trong quá trình xác thực.");
                }

                if (!isPasswordValid)
                {
                    // Increment failed login attempts
                    user.FailedLoginAttempts = (user.FailedLoginAttempts ?? 0) + 1;
                    user.LastFailedLoginAttempt = DateTime.UtcNow;

                    _logger.LogWarning("Invalid password for user {Username}. Failed attempts: {FailedAttempts}",
                        username, user.FailedLoginAttempts);

                    // Check if we need to lock the account
                    if (user.FailedLoginAttempts >= 5)
                    {
                        user.IsAccountLocked = true;
                        user.AccountLockedUntil = DateTime.UtcNow.AddMinutes(30);
                        user.LockoutReason = "Đăng nhập sai quá 5 lần liên tiếp";

                        await _userRepository.UpdateAsync(user);

                        _logger.LogWarning("Account {Username} has been locked due to {Attempts} failed login attempts",
                            username, user.FailedLoginAttempts);

                        // Send email notification about account lockout
                        if (!string.IsNullOrEmpty(user.Email))
                        {
                            _ = Task.Run(async () =>
                            {
                                try
                                {
                                    await _emailService.SendNotificationAsync(
                                        user.Email,
                                        "Tài khoản đã bị khóa tạm thời",
                                        $"Tài khoản của bạn đã bị khóa tạm thời do có 5 lần đăng nhập không thành công. " +
                                        $"Tài khoản sẽ được tự động mở khóa sau 30 phút. " +
                                        $"Nếu đây không phải là bạn, vui lòng liên hệ với bộ phận hỗ trợ ngay lập tức."
                                    );
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, "Failed to send lockout notification email to {Email}", user.Email);
                                }
                            });
                        }

                        return (null, ErrorCodes.ACCOUNT_LOCKED,
                            "Tài khoản đã bị khóa do đăng nhập sai quá nhiều lần. Vui lòng thử lại sau 30 phút.");
                    }
                    else
                    {
                        await _userRepository.UpdateAsync(user);
                        var remainingAttempts = 5 - user.FailedLoginAttempts;

                        return (null, ErrorCodes.INVALID_CREDENTIALS,
                            $"{ErrorMessages.INVALID_USERNAME_PASSWORD}. Bạn còn {remainingAttempts} lần thử.");
                    }
                }

                // 6. Check email verification (only for users with email)
                if (!string.IsNullOrEmpty(user.Email) && !user.EmailVerified)
                {
                    _logger.LogWarning("Login blocked: Email not verified for {Username}", username);

                    // Check if we need to resend verification email
                    bool shouldResendEmail = user.EmailVerificationToken == null ||
                                           user.EmailVerificationExpiry == null ||
                                           user.EmailVerificationExpiry < DateTime.UtcNow;

                    if (shouldResendEmail)
                    {
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                // Generate new verification token
                                var verificationToken = SecurityHelper.GenerateSecureToken();
                                user.EmailVerificationToken = Encoding.UTF8.GetBytes(verificationToken);
                                user.EmailVerificationExpiry = DateTime.UtcNow.AddHours(24);
                                await _userRepository.UpdateAsync(user);

                                // Send verification email
                                await _emailService.SendCustomerEmailVerificationAsync(user.Email, user.FullName, verificationToken);
                                _logger.LogInformation("New verification email sent for {Username}", username);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Failed to send verification email for {Username}", username);
                            }
                        });
                    }

                    return (null, ErrorCodes.EMAIL_NOT_VERIFIED, ErrorMessages.EMAIL_NOT_VERIFIED);
                }

                // 7. Check password expiry (optional)
                if (user.PasswordExpiryDate.HasValue && user.PasswordExpiryDate < DateOnly.FromDateTime(DateTime.UtcNow))
                {
                    _logger.LogWarning("Login blocked: Password expired for {Username}", username);
                    return (null, ErrorCodes.PASSWORD_EXPIRED,
                        "Mật khẩu của bạn đã hết hạn. Vui lòng đặt lại mật khẩu mới.");
                }

                // 8. Reset failed login attempts on successful authentication
                if (user.FailedLoginAttempts > 0)
                {
                    user.FailedLoginAttempts = 0;
                    user.LastFailedLoginAttempt = null;
                    user.UnlockAttempts = 0;
                }

                // 9. Update last login time
                user.LastLoginDate = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);

                // 10. Log successful login with IP and UserAgent
                var clientIp = _httpContextService.GetClientIpAddress();
                var userAgent = _httpContextService.GetUserAgent();

                _logger.LogInformation("User {Username} (ID: {UserId}, Role: {Role}) logged in successfully from IP: {IP}, UserAgent: {UserAgent}",
                    username, user.UserId, (UserRoles)user.RoleId, clientIp, userAgent);

                // 12. Send login notification for sensitive accounts (async, non-blocking)
                if (user.RoleId == (int)UserRoles.Admin && !string.IsNullOrEmpty(user.Email))
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await _emailService.SendNotificationAsync(
                                user.Email,
                                "Thông báo đăng nhập",
                                $"Tài khoản Admin của bạn vừa đăng nhập lúc {DateTime.Now:dd/MM/yyyy HH:mm:ss} " +
                                $"từ địa chỉ IP: {clientIp}. " +
                                $"Nếu đây không phải là bạn, vui lòng đổi mật khẩu ngay lập tức."
                            );
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to send login notification to admin {Username}", username);
                        }
                    });
                }

                // 13. Map to response DTO
                var userResponse = _mapper.Map<UserResponseDto>(user);

                // 14. Return successful result
                return (userResponse, null, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during login for username: {Username}", username);
                return (null, ErrorCodes.INTERNAL_ERROR, "Có lỗi xảy ra trong quá trình đăng nhập. Vui lòng thử lại sau.");
            }
        }

        public async Task UpdateUserLastLoginAsync(int userId)
        {
            await _userRepository.UpdateLastLoginAsync(userId);
            _logger.LogInformation("Last login updated for UserId: {UserId}", userId);
        }

        public async Task UpdateUserPasswordAsync(int userId, string currentPassword, string newPlainPassword)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException(ErrorMessages.USER_NOT_FOUND);

            // Verify current password using SecurityHelper
            var isCurrentPasswordValid = SecurityHelper.VerifyPassword(
                currentPassword,
                Encoding.UTF8.GetString(user.PasswordHash)
            );

            if (!isCurrentPasswordValid)
            {
                _logger.LogWarning("Password change failed: incorrect current password for UserId: {UserId}", userId);
                throw new InvalidOperationException("Current password is incorrect");
            }

            // Validate new password
            var (isValid, errorMessage) = PasswordValidator.ValidatePassword(newPlainPassword);
            if (!isValid)
            {
                throw new ArgumentException(errorMessage, nameof(newPlainPassword));
            }

            // Generate new hash using SecurityHelper
            var salt = SecurityHelper.GenerateSalt();
            var hash = SecurityHelper.HashPassword(newPlainPassword, salt);
            var hashBytes = Encoding.UTF8.GetBytes(hash);
            var saltBytes = Encoding.UTF8.GetBytes(salt);

            if (hashBytes.Length > 64)
                throw new InvalidOperationException("Password hash exceeds VARBINARY(64) limit.");
            if (saltBytes.Length > 32)
                throw new InvalidOperationException("Password salt exceeds VARBINARY(32) limit.");

            await _userRepository.UpdatePasswordAsync(userId, hashBytes, saltBytes);
            _logger.LogInformation("Password updated successfully for UserId {UserId}", userId);
        }

        //public async Task<bool> DeleteUserAsync(int id)
        //{
        //    if (!await _userRepository.ExistsAsync(id))
        //    {
        //        _logger.LogWarning("Delete failed: User not found with ID: {UserId}", id);
        //        throw new InvalidOperationException(ErrorMessages.USER_NOT_FOUND);
        //    }

        //    var result = await _userRepository.DeleteAsync(id);
        //    _logger.LogInformation("User deleted: {UserId}", id);
        //    return result;
        //}

        public async Task<bool> DeleteUserAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            // Check if user has customer
            if (user?.Customer != null)
            {
                throw new InvalidOperationException(
                    "Cannot delete user with linked customer. " +
                    "Please unlink or delete customer first.");
            }

            return await _userRepository.DeleteAsync(userId);
        }

        public async Task<bool> ForgotPasswordAsync(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                    throw new ArgumentException("Email is required", nameof(email));

                var user = await _userRepository.GetByEmailAsync(email);
                if (user == null)
                {
                    // Return true for security (prevent email enumeration)
                    _logger.LogWarning("Password reset requested for non-existent email: {Email} from IP: {IP}",
                        email, _httpContextService.GetClientIpAddress());

                    // Add artificial delay to prevent timing attacks
                    await Task.Delay(Random.Shared.Next(500, 1500));
                    return true;
                }

                // Check if user recently requested reset (rate limiting)
                if (user.ResetTokenExpiry.HasValue && user.ResetTokenExpiry > DateTime.UtcNow.AddMinutes(-5))
                {
                    _logger.LogWarning("Rate limit: Password reset requested too soon for email: {Email}", email);

                    // Still return true but don't send email
                    return true;
                }

                // Check account status
                if (user.IsActive == false)
                {
                    _logger.LogWarning("Password reset requested for inactive account: {Email}", email);
                    return true; // Still return true for security
                }

                // Generate reset token using SecurityHelper
                var resetToken = SecurityHelper.GenerateSecureToken();
                user.ResetToken = Encoding.UTF8.GetBytes(resetToken);
                user.ResetTokenExpiry = DateTime.UtcNow.AddHours(1);

                await _userRepository.UpdateAsync(user);

                // Send reset email
                await _emailService.SendPasswordResetAsync(email, user.FullName, resetToken);

                _logger.LogInformation("Password reset token generated for email: {Email} from IP: {IP}",
                    email, _httpContextService.GetClientIpAddress());

                return true;
            }
            catch (ArgumentException)
            {
                throw; // Re-throw validation errors
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during forgot password for email: {Email}", email);
                throw new InvalidOperationException("Có lỗi xảy ra khi gửi email đặt lại mật khẩu. Vui lòng thử lại sau.");
            }
        }
        public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrWhiteSpace(email))
                    throw new ArgumentException("Email is required", nameof(email));

                if (string.IsNullOrWhiteSpace(token))
                    throw new ArgumentException("Reset token is required", nameof(token));

                if (string.IsNullOrWhiteSpace(newPassword))
                    throw new ArgumentException("New password is required", nameof(newPassword));

                var user = await _userRepository.GetByEmailAsync(email);
                if (user == null || user.ResetToken == null || user.ResetTokenExpiry == null)
                {
                    _logger.LogWarning("Reset password failed: User not found or no reset token for email: {Email}", email);
                    return false;
                }

                // Check if account is active
                if (user.IsActive == false)
                {
                    _logger.LogWarning("Reset password failed: Account inactive for email: {Email}", email);
                    return false;
                }

                // Verify token and expiry
                var storedToken = Encoding.UTF8.GetString(user.ResetToken);
                if (storedToken != token)
                {
                    _logger.LogWarning("Reset password failed: Invalid token for email: {Email}", email);
                    return false;
                }

                if (user.ResetTokenExpiry < DateTime.UtcNow)
                {
                    _logger.LogWarning("Reset password failed: Expired token for email: {Email}. Token expired at: {ExpiredAt}",
                        email, user.ResetTokenExpiry);
                    return false;
                }

                // Validate new password strength
                var (isValid, errorMessage) = PasswordValidator.ValidatePassword(newPassword);
                if (!isValid)
                {
                    _logger.LogWarning("Reset password failed: Invalid password for email: {Email}. Error: {Error}",
                        email, errorMessage);
                    throw new ArgumentException(errorMessage, nameof(newPassword));
                }

                // Check if new password is same as current (optional security measure)
                var currentPasswordValid = SecurityHelper.VerifyPassword(newPassword, Encoding.UTF8.GetString(user.PasswordHash));
                if (currentPasswordValid)
                {
                    _logger.LogWarning("Reset password failed: New password same as current for email: {Email}", email);
                    throw new ArgumentException("Mật khẩu mới không được trùng với mật khẩu cũ", nameof(newPassword));
                }

                // Update password using SecurityHelper
                var salt = SecurityHelper.GenerateSalt();
                var hash = SecurityHelper.HashPassword(newPassword, salt);

                // Validate hash lengths
                var hashBytes = Encoding.UTF8.GetBytes(hash);
                var saltBytes = Encoding.UTF8.GetBytes(salt);

                if (hashBytes.Length > 64)
                    throw new InvalidOperationException("Password hash exceeds VARBINARY(64) limit");
                if (saltBytes.Length > 32)
                    throw new InvalidOperationException("Password salt exceeds VARBINARY(32) limit");

                user.PasswordHash = hashBytes;
                user.PasswordSalt = saltBytes;

                // Clear reset token and unlock account
                user.ResetToken = null;
                user.ResetTokenExpiry = null;
                user.FailedLoginAttempts = 0;
                user.IsAccountLocked = false;
                user.AccountLockedUntil = null;
                user.LockoutReason = null;

                await _userRepository.UpdateAsync(user);

                // Send confirmation email (non-blocking)
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _emailService.SendNotificationAsync(
                            email,
                            "Mật khẩu đã được đặt lại thành công",
                            $"Mật khẩu cho tài khoản {email} đã được đặt lại thành công lúc {DateTime.Now:dd/MM/yyyy HH:mm:ss}. " +
                            $"Nếu đây không phải là bạn, vui lòng liên hệ hỗ trợ ngay lập tức."
                        );
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send password reset confirmation email to {Email}", email);
                    }
                });

                _logger.LogInformation("Password reset successful for email: {Email}", email);
                return true;
            }
            catch (ArgumentException)
            {
                throw; // Re-throw validation errors
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during password reset for email: {Email}", email);
                throw new InvalidOperationException("Có lỗi xảy ra khi đặt lại mật khẩu. Vui lòng thử lại sau.");
            }
        }

        public async Task<bool> VerifyEmailAsync(string email, string token)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null || user.EmailVerificationToken == null || user.EmailVerificationExpiry == null)
            {
                _logger.LogWarning("Invalid email verification attempt for: {Email}", email);
                return false;
            }

            // Verify token and expiry
            var storedToken = Encoding.UTF8.GetString(user.EmailVerificationToken);
            if (storedToken != token || user.EmailVerificationExpiry < DateTime.UtcNow)
            {
                _logger.LogWarning("Invalid or expired verification token for: {Email}", email);
                return false;
            }

            // Mark email as verified
            user.EmailVerified = true;
            user.EmailVerificationToken = null;
            user.EmailVerificationExpiry = null;

            await _userRepository.UpdateAsync(user);

            // Send welcome email
            await _emailService.SendWelcomeEmailAsync(email, user.FullName);

            _logger.LogInformation("Email verified successfully for user: {Email}", email);
            return true;
        }

        public async Task<bool> ResendEmailVerificationAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                _logger.LogWarning("Resend verification requested for non-existent email: {Email}", email);
                return false;
            }

            if (user.EmailVerified)
            {
                _logger.LogInformation("Email already verified for: {Email}", email);
                return true;
            }

            // Generate new verification token using SecurityHelper
            var verificationToken = SecurityHelper.GenerateSecureToken();
            user.EmailVerificationToken = Encoding.UTF8.GetBytes(verificationToken);
            user.EmailVerificationExpiry = DateTime.UtcNow.AddHours(24);

            await _userRepository.UpdateAsync(user);

            // Send verification email
            await _emailService.SendCustomerEmailVerificationAsync(email, user.FullName, verificationToken);

            _logger.LogInformation("Verification email resent for user: {Email}", email);
            return true;
        }

        public async Task<bool> IsEmailVerifiedAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            return user?.EmailVerified ?? false;
        }

        public async Task<UserResponseDto?> GetUserByEmailAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            return user != null ? _mapper.Map<UserResponseDto>(user) : null;
        }

        public async Task<bool> ValidateResetTokenAsync(string email, string token)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null || user.ResetToken == null || user.ResetTokenExpiry == null)
            {
                return false;
            }

            var storedToken = Encoding.UTF8.GetString(user.ResetToken);
            return storedToken == token && user.ResetTokenExpiry >= DateTime.UtcNow;
        }

        // Private helper methods
        private async Task<string> GenerateUniqueEmployeeCodeAsync()
        {
            var year = DateTime.UtcNow.Year.ToString().Substring(2);
            var prefix = $"EMP-{year}-";

            var maxCode = await _userRepository.GetMaxEmployeeCodeAsync(prefix);
            int nextNum = (maxCode != null) ? int.Parse(maxCode.Substring(prefix.Length)) + 1 : 1;

            var newCode = $"{prefix}{nextNum:D4}";

            while (await _userRepository.IsEmployeeCodeExistsAsync(newCode))
            {
                nextNum++;
                newCode = $"{prefix}{nextNum:D4}";
            }

            return newCode;
        }

        public Task<bool> IsUsernameExistsAsync(string username)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsEmailExistsAsync(string email)
        {
            throw new NotImplementedException();
        }

        public async Task<UserResponseDto> RegisterInternalUserAsync(User user, string plainPassword)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            // Validate this is internal user
            if (user.RoleId != (int)UserRoles.Admin &&
                user.RoleId != (int)UserRoles.Staff &&
                user.RoleId != (int)UserRoles.Technician)
            {
                throw new ArgumentException("This method is only for internal staff registration", nameof(user.RoleId));
            }

            // Validate password strength
            var (isValid, errorMessage) = PasswordValidator.ValidatePassword(plainPassword);
            if (!isValid)
            {
                throw new ArgumentException(errorMessage, nameof(plainPassword));
            }

            // Check for duplicates
            if (await _userRepository.IsUsernameExistsAsync(user.Username))
                throw new InvalidOperationException(ErrorMessages.DUPLICATE_USERNAME);

            if (!string.IsNullOrEmpty(user.Email) && await _userRepository.IsEmailExistsAsync(user.Email))
                throw new InvalidOperationException(ErrorMessages.DUPLICATE_EMAIL);

            // Generate employee code for internal roles
            user.EmployeeCode = await GenerateUniqueEmployeeCodeAsync();

            // Password hashing
            var salt = SecurityHelper.GenerateSalt();
            var hash = SecurityHelper.HashPassword(plainPassword, salt);
            var hashBytes = Encoding.UTF8.GetBytes(hash);
            var saltBytes = Encoding.UTF8.GetBytes(salt);

            if (hashBytes.Length > 64 || saltBytes.Length > 32)
                throw new InvalidOperationException("Password hash/salt exceeds database limits.");

            user.PasswordHash = hashBytes;
            user.PasswordSalt = saltBytes;
            user.IsActive = true;
            user.CreatedDate = DateTime.UtcNow;

            // Internal staff: Email verified by default, no verification needed
            user.EmailVerified = true;
            user.EmailVerificationToken = null;
            user.EmailVerificationExpiry = null;

            var createdUser = await _userRepository.CreateAsync(user);

            // Send welcome email for internal staff
            if (!string.IsNullOrEmpty(user.Email))
            {
                try
                {
                    await _emailService.SendInternalStaffWelcomeEmailAsync(
                        user.Email,
                        user.FullName,
                        user.Username,
                        ((UserRoles)user.RoleId).ToString(),
                        user.Department
                    );
                    _logger.LogInformation("Welcome email sent to internal staff {Email} for user {Username}", user.Email, user.Username);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send welcome email for internal staff {Username}", user.Username);
                    // Don't fail registration if email fails
                }
            }

            _logger.LogInformation("Internal staff registered successfully: {Username}, Role: {Role}, EmployeeCode: {EmployeeCode}",
                user.Username, (UserRoles)user.RoleId, user.EmployeeCode);

            return _mapper.Map<UserResponseDto>(createdUser);
        }
       
    }
}