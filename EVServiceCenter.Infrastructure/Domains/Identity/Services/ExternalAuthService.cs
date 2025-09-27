﻿using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using EVServiceCenter.Core.Domains.Identity.Interfaces;
using EVServiceCenter.Core.Domains.Identity.DTOs.Requests;
using EVServiceCenter.Core.Domains.Identity.DTOs.Responses;
using EVServiceCenter.Core.Domains.Identity.Entities;
using EVServiceCenter.Core.Enums;
using AutoMapper;
using Google.Apis.Auth;
using EVServiceCenter.Core.Helpers;
using System.Text;

namespace EVServiceCenter.Infrastructure.Domains.Identity.Services
{
    public class ExternalAuthService : IExternalAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ExternalAuthService> _logger;
        private readonly IMapper _mapper;
        private readonly HttpClient _httpClient;

        public ExternalAuthService(
            IUserRepository userRepository,
            ITokenService tokenService,
            IEmailService emailService,
            IConfiguration configuration,
            ILogger<ExternalAuthService> logger,
            IMapper mapper,
            IHttpClientFactory httpClientFactory)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
            _mapper = mapper;
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task<ExternalLoginResponseDto> GoogleLoginAsync(GoogleLoginRequestDto request)
        {
            try
            {
                // Verify Google token
                var googleUser = await VerifyGoogleTokenAsync(request.IdToken);
                if (googleUser == null)
                {
                    return new ExternalLoginResponseDto
                    {
                        Success = false,
                        Message = "Invalid Google token",
                        ErrorCode = "INVALID_TOKEN"
                    };
                }

                // Check if user exists with this Google ID
                var existingUser = await _userRepository.FirstOrDefaultAsync(u =>
                    u.ExternalProvider == "Google" &&
                    u.ExternalProviderId == googleUser.Id);

                if (existingUser != null)
                {
                    // Existing user - log them in
                    return await HandleExistingUserLogin(existingUser);
                }

                // Check if user exists with same email
                var userByEmail = await _userRepository.GetByEmailAsync(googleUser.Email);
                if (userByEmail != null)
                {
                    // Link Google account to existing user
                    if (string.IsNullOrEmpty(userByEmail.ExternalProvider))
                    {
                        userByEmail.ExternalProvider = "Google";
                        userByEmail.ExternalProviderId = googleUser.Id;
                        userByEmail.AvatarUrl = googleUser.Picture;
                        await _userRepository.UpdateAsync(userByEmail);

                        return await HandleExistingUserLogin(userByEmail);
                    }
                    else
                    {
                        return new ExternalLoginResponseDto
                        {
                            Success = false,
                            Message = $"Email đã được liên kết với tài khoản {userByEmail.ExternalProvider}",
                            ErrorCode = "EMAIL_ALREADY_LINKED"
                        };
                    }
                }

                // Create new user
                return await CreateNewExternalUser(googleUser, "Google");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Google login");
                return new ExternalLoginResponseDto
                {
                    Success = false,
                    Message = "Đăng nhập Google thất bại",
                    ErrorCode = "EXTERNAL_LOGIN_ERROR"
                };
            }
        }

        public async Task<ExternalLoginResponseDto> FacebookLoginAsync(FacebookLoginRequestDto request)
        {
            try
            {
                // Verify Facebook token
                var facebookUser = await VerifyFacebookTokenAsync(request.AccessToken);
                if (facebookUser == null)
                {
                    return new ExternalLoginResponseDto
                    {
                        Success = false,
                        Message = "Invalid Facebook token",
                        ErrorCode = "INVALID_TOKEN"
                    };
                }

                // Check if user exists with this Facebook ID
                var existingUser = await _userRepository.FirstOrDefaultAsync(u =>
                    u.ExternalProvider == "Facebook" &&
                    u.ExternalProviderId == facebookUser.Id);

                if (existingUser != null)
                {
                    // Existing user - log them in
                    return await HandleExistingUserLogin(existingUser);
                }

                // Check if user exists with same email
                var userByEmail = await _userRepository.GetByEmailAsync(facebookUser.Email);
                if (userByEmail != null)
                {
                    // Link Facebook account to existing user
                    if (string.IsNullOrEmpty(userByEmail.ExternalProvider))
                    {
                        userByEmail.ExternalProvider = "Facebook";
                        userByEmail.ExternalProviderId = facebookUser.Id;
                        userByEmail.AvatarUrl = facebookUser.Picture;
                        await _userRepository.UpdateAsync(userByEmail);

                        return await HandleExistingUserLogin(userByEmail);
                    }
                    else
                    {
                        return new ExternalLoginResponseDto
                        {
                            Success = false,
                            Message = $"Email đã được liên kết với tài khoản {userByEmail.ExternalProvider}",
                            ErrorCode = "EMAIL_ALREADY_LINKED"
                        };
                    }
                }

                // Create new user
                return await CreateNewExternalUser(facebookUser, "Facebook");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Facebook login");
                return new ExternalLoginResponseDto
                {
                    Success = false,
                    Message = "Đăng nhập Facebook thất bại",
                    ErrorCode = "EXTERNAL_LOGIN_ERROR"
                };
            }
        }

        public async Task<ExternalUserInfoDto?> VerifyGoogleTokenAsync(string idToken)
        {
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _configuration["Authentication:Google:ClientId"] }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

                return new ExternalUserInfoDto
                {
                    Id = payload.Subject,
                    Email = payload.Email,
                    Name = payload.Name,
                    Picture = payload.Picture,
                    Provider = "Google"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying Google token");
                return null;
            }
        }

        public async Task<ExternalUserInfoDto?> VerifyFacebookTokenAsync(string accessToken)
        {
            try
            {
                // Verify token with Facebook
                var appId = _configuration["Authentication:Facebook:AppId"];
                var appSecret = _configuration["Authentication:Facebook:AppSecret"];

                // Debug token
                var debugTokenUrl = $"https://graph.facebook.com/debug_token?input_token={accessToken}&access_token={appId}|{appSecret}";
                var debugResponse = await _httpClient.GetAsync(debugTokenUrl);

                if (!debugResponse.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Facebook token validation failed");
                    return null;
                }

                var debugContent = await debugResponse.Content.ReadAsStringAsync();
                var debugData = JsonConvert.DeserializeObject<dynamic>(debugContent);

                if (debugData?.data?.is_valid != true)
                {
                    _logger.LogWarning("Facebook token is not valid");
                    return null;
                }

                // Get user info
                var userInfoUrl = $"https://graph.facebook.com/me?fields=id,name,email,picture&access_token={accessToken}";
                var userResponse = await _httpClient.GetAsync(userInfoUrl);

                if (!userResponse.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to get Facebook user info");
                    return null;
                }

                var userContent = await userResponse.Content.ReadAsStringAsync();
                var userData = JsonConvert.DeserializeObject<dynamic>(userContent);

                return new ExternalUserInfoDto
                {
                    Id = userData.id,
                    Email = userData.email ?? $"{userData.id}@facebook.local",
                    Name = userData.name,
                    Picture = userData.picture?.data?.url,
                    Provider = "Facebook"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying Facebook token");
                return null;
            }
        }

        public async Task<bool> LinkExternalAccountAsync(int userId, string provider, string externalId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return false;

                // Check if already linked
                if (!string.IsNullOrEmpty(user.ExternalProvider))
                {
                    _logger.LogWarning("User {UserId} already has external provider linked", userId);
                    return false;
                }

                // Check if external ID is already used
                var existing = await _userRepository.FirstOrDefaultAsync(u =>
                    u.ExternalProvider == provider &&
                    u.ExternalProviderId == externalId);

                if (existing != null)
                {
                    _logger.LogWarning("External ID {ExternalId} already linked to another user", externalId);
                    return false;
                }

                user.ExternalProvider = provider;
                user.ExternalProviderId = externalId;
                await _userRepository.UpdateAsync(user);

                _logger.LogInformation("Linked {Provider} account to user {UserId}", provider, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error linking external account");
                return false;
            }
        }

        public async Task<bool> UnlinkExternalAccountAsync(int userId, string provider)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return false;

                if (user.ExternalProvider != provider)
                {
                    _logger.LogWarning("User {UserId} does not have {Provider} linked", userId, provider);
                    return false;
                }

                // Check if user has password (can login without external provider)
                if (user.PasswordHash == null || user.PasswordHash.Length == 0)
                {
                    _logger.LogWarning("Cannot unlink external provider for user without password");
                    return false;
                }

                user.ExternalProvider = null;
                user.ExternalProviderId = null;
                await _userRepository.UpdateAsync(user);

                _logger.LogInformation("Unlinked {Provider} account from user {UserId}", provider, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unlinking external account");
                return false;
            }
        }

        // Private helper methods
        private async Task<ExternalLoginResponseDto> HandleExistingUserLogin(User user)
        {
            // Update last login
            user.LastLoginDate = DateTime.UtcNow;
            user.FailedLoginAttempts = 0;
            await _userRepository.UpdateAsync(user);

            // Generate tokens
            var accessToken = _tokenService.GenerateToken(user);
            var refreshToken = GenerateRefreshToken();

            // Save refresh token
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await _userRepository.UpdateAsync(user);

            var userDto = _mapper.Map<UserResponseDto>(user);

            _logger.LogInformation("External login successful for user {UserId} via {Provider}",
                user.UserId, user.ExternalProvider);

            return new ExternalLoginResponseDto
            {
                Success = true,
                Message = "Đăng nhập thành công",
                User = userDto,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                IsNewUser = false
            };
        }

        private async Task<ExternalLoginResponseDto> CreateNewExternalUser(ExternalUserInfoDto externalUser, string provider)
        {
            try
            {
                // Generate username from email or provider ID
                var username = GenerateUsername(externalUser.Email, externalUser.Id);

                // Ensure username is unique
                while (await _userRepository.IsUsernameExistsAsync(username))
                {
                    username = $"{username}_{Guid.NewGuid().ToString().Substring(0, 4)}";
                }

                var user = new User
                {
                    Username = username,
                    FullName = externalUser.Name,
                    Email = externalUser.Email,
                    EmailVerified = true, // External logins are pre-verified
                    ExternalProvider = provider,
                    ExternalProviderId = externalUser.Id,
                    AvatarUrl = externalUser.Picture,
                    RoleId = (int)UserRoles.Customer,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    // No password for external users initially
                    PasswordHash = new byte[1],
                    PasswordSalt = new byte[1]
                };

                var createdUser = await _userRepository.CreateAsync(user);

                // Send welcome email
                if (!string.IsNullOrEmpty(user.Email) && !user.Email.EndsWith("@facebook.local"))
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await _emailService.SendWelcomeEmailAsync(user.Email, user.FullName);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to send welcome email");
                        }
                    });
                }

                // Generate tokens
                var accessToken = _tokenService.GenerateToken(createdUser);
                var refreshToken = GenerateRefreshToken();

                // Save refresh token
                createdUser.RefreshToken = refreshToken;
                createdUser.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
                await _userRepository.UpdateAsync(createdUser);

                var userDto = _mapper.Map<UserResponseDto>(createdUser);

                _logger.LogInformation("New user created via {Provider}: {Username}", provider, username);

                return new ExternalLoginResponseDto
                {
                    Success = true,
                    Message = "Đăng ký thành công",
                    User = userDto,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    IsNewUser = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating new external user");
                throw;
            }
        }

        private string GenerateUsername(string email, string externalId)
        {
            if (!string.IsNullOrEmpty(email) && !email.EndsWith("@facebook.local"))
            {
                return email.Split('@')[0];
            }
            return $"user_{externalId.Substring(0, Math.Min(8, externalId.Length))}";
        }

        private string GenerateRefreshToken()
        {
            return SecurityHelper.GenerateSecureToken(64);
        }
    }
}