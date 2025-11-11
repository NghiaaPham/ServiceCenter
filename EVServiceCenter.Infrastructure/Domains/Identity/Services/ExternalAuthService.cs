using System.Net.Http;
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
using EVServiceCenter.Core.Domains.Customers.Interfaces;
using EVServiceCenter.Core.Domains.Customers.DTOs.Requests;
using EVServiceCenter.Core.Entities;
using EVServiceCenter.Core.Domains.Customers.Entities;
using Microsoft.EntityFrameworkCore;

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
        private readonly ICustomerAccountService _customerAccountService;
        private readonly EVDbContext _context;
        private readonly IUserService _userService;

        public ExternalAuthService(
            IUserRepository userRepository,
            ITokenService tokenService,
            IEmailService emailService,
            IConfiguration configuration,
            ILogger<ExternalAuthService> logger,
            IMapper mapper,
            IHttpClientFactory httpClientFactory,
            ICustomerAccountService customerAccountService,
            EVDbContext context,
            IUserService userService)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
            _mapper = mapper;
            _httpClient = httpClientFactory.CreateClient();
            _customerAccountService = customerAccountService;
            _context = context;
            _userService = userService;
        }
        public async Task<ExternalLoginResponseDto> GoogleLoginAsync(GoogleLoginRequestDto request)
        {
            try
            {
                const string provider = "Google";

                _logger.LogInformation("GoogleLoginAsync called with token length: {Length}",
                    request?.IdToken?.Length ?? 0);

                // Validate request
                if (request == null || string.IsNullOrWhiteSpace(request.IdToken))
                {
                    _logger.LogWarning("Google login request is null or IdToken is empty");
                    return new ExternalLoginResponseDto
                    {
                        Success = false,
                        Message = "Invalid Google token",
                        ErrorCode = "INVALID_TOKEN"
                    };
                }

                // Verify Google token
                ExternalUserInfoDto? googleUser = null;
                try
                {
                    googleUser = await VerifyGoogleTokenAsync(request.IdToken);
                }
                catch (InvalidOperationException ex)
                {
                    // Configuration error
                    _logger.LogError(ex, "Google authentication configuration error");
                    return new ExternalLoginResponseDto
                    {
                        Success = false,
                        Message = ex.Message,
                        ErrorCode = "CONFIG_ERROR"
                    };
                }
                catch (TimeoutException ex)
                {
                    _logger.LogError(ex, "Google token verification timeout");
                    return new ExternalLoginResponseDto
                    {
                        Success = false,
                        Message = "Dịch vụ xác thực Google không phản hồi. Vui lòng thử lại.",
                        ErrorCode = "TIMEOUT"
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error verifying Google token: {ErrorType}", ex.GetType().Name);
                    return new ExternalLoginResponseDto
                    {
                        Success = false,
                        Message = "Token Google không hợp lệ hoặc đã hết hạn",
                        ErrorCode = "INVALID_TOKEN"
                    };
                }

                if (googleUser == null)
                {
                    _logger.LogWarning("Google token verification returned null");
                    return new ExternalLoginResponseDto
                    {
                        Success = false,
                        Message = "Token Google không hợp lệ",
                        ErrorCode = "INVALID_TOKEN"
                    };
                }

                _logger.LogInformation("Google user verified: {Email}", googleUser.Email);

                // Check if user exists with this Google ID
                var existingUser = await _userRepository.FirstOrDefaultAsync(u =>
                    u.ExternalProvider == provider &&
                    u.ExternalProviderId == googleUser.Id);

                if (existingUser != null)
                {
                    // Existing user - log them in
                    _logger.LogInformation("Existing Google user found: {UserId}", existingUser.UserId);
                    return await HandleExistingUserLogin(existingUser, googleUser);
                }

                // Check if user exists with same email
                var userByEmail = await _userRepository.GetByEmailAsync(googleUser.Email);
                if (userByEmail == null)
                {
                    // Create new user
                    _logger.LogInformation("Creating new user for Google email: {Email}", googleUser.Email);
                    return await CreateNewExternalUser(googleUser, provider);
                }

                // ✅ FIX: Cho phép login bằng Google dù email đã được dùng với provider khác
                // Update Google info và cho phép login
                _logger.LogInformation("Linking/Updating Google account for existing user {UserId} (Previous provider: {Provider})",
                    userByEmail.UserId, userByEmail.ExternalProvider ?? "None");

                userByEmail.ExternalProvider = provider;
                userByEmail.ExternalProviderId = googleUser.Id;
                userByEmail.AvatarUrl = googleUser.Picture;

                await _userRepository.UpdateAsync(userByEmail);
                return await HandleExistingUserLogin(userByEmail, googleUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during Google login");
                return new ExternalLoginResponseDto
                {
                    Success = false,
                    Message = "Đăng nhập Google thất bại. Vui lòng thử lại.",
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
                    return await HandleExistingUserLogin(existingUser, facebookUser);
                }

                // Check if user exists with same email
                var userByEmail = await _userRepository.GetByEmailAsync(facebookUser.Email);
                if (userByEmail != null)
                {
                    // ✅ FIX: Cho phép login bằng Facebook dù email đã được dùng với provider khác
                    // Update Facebook info và cho phép login
                    _logger.LogInformation("Linking/Updating Facebook account for existing user {UserId} (Previous provider: {Provider})",
                        userByEmail.UserId, userByEmail.ExternalProvider ?? "None");

                    userByEmail.ExternalProvider = "Facebook";
                    userByEmail.ExternalProviderId = facebookUser.Id;
                    userByEmail.AvatarUrl = facebookUser.Picture;
                    await _userRepository.UpdateAsync(userByEmail);

                    return await HandleExistingUserLogin(userByEmail, facebookUser);
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
                // Validate Google ClientId configuration
                var clientId = _configuration["Authentication:Google:ClientId"];
                if (string.IsNullOrWhiteSpace(clientId) ||
                    clientId.Equals("your-google-client-id", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogError("Google ClientId is not configured in appsettings.json. Please add a valid Google OAuth ClientId.");
                    throw new InvalidOperationException(
                        "Google login is not configured. Please contact the administrator to enable Google authentication.");
                }

                _logger.LogInformation("Verifying Google token with ClientId: {ClientId}, Token length: {Length}",
                    clientId.Substring(0, Math.Min(10, clientId.Length)) + "...", idToken.Length);

                Google.Apis.Auth.GoogleJsonWebSignature.Payload? payload = null;

                try
                {
                    _logger.LogInformation("About to call GoogleJsonWebSignature.ValidateAsync...");

                    var settings = new GoogleJsonWebSignature.ValidationSettings
                    {
                        Audience = new[] { clientId }
                    };

                    // ✅ FIX: Thêm timeout 10 giây để tránh request bị hang
                    var validationTask = GoogleJsonWebSignature.ValidateAsync(idToken, settings);
                    var timeoutTask = Task.Delay(TimeSpan.FromSeconds(10));

                    var completedTask = await Task.WhenAny(validationTask, timeoutTask);

                    if (completedTask == timeoutTask)
                    {
                        _logger.LogError("Google token verification timed out after 10 seconds");
                        throw new TimeoutException("Google authentication service timed out. Please try again.");
                    }

                    payload = await validationTask;

                    _logger.LogInformation("GoogleJsonWebSignature.ValidateAsync completed successfully");
                }
                catch (Google.Apis.Auth.InvalidJwtException jwtEx)
                {
                    _logger.LogError(jwtEx, "Invalid JWT token: {Message}", jwtEx.Message);
                    return null;
                }
                catch (TimeoutException)
                {
                    // Re-throw timeout exceptions
                    throw;
                }
                catch (HttpRequestException httpEx)
                {
                    _logger.LogError(httpEx, "HTTP error while verifying Google token: {Message}", httpEx.Message);
                    return null;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error in GoogleJsonWebSignature.ValidateAsync: Type={Type}, Message={Message}, StackTrace={StackTrace}",
                        ex.GetType().FullName, ex.Message, ex.StackTrace);
                    throw;
                }

                if (payload == null)
                {
                    _logger.LogWarning("Google token validation returned null payload");
                    return null;
                }

                _logger.LogInformation("Google token verified successfully for email: {Email}", payload.Email);

                return new ExternalUserInfoDto
                {
                    Id = payload.Subject,
                    Email = payload.Email,
                    Name = payload.Name,
                    Picture = payload.Picture,
                    Provider = "Google"
                };
            }
            catch (InvalidOperationException)
            {
                // Re-throw configuration errors
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FATAL: Unhandled exception in VerifyGoogleTokenAsync: Type={Type}, Message={Message}",
                    ex.GetType().FullName, ex.Message);
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

                // ✅ IMPROVED: Get user info with additional fields (birthday, gender, location)
                var userInfoUrl = $"https://graph.facebook.com/me?fields=id,name,email,picture,birthday,gender,location&access_token={accessToken}";
                var userResponse = await _httpClient.GetAsync(userInfoUrl);

                if (!userResponse.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to get Facebook user info");
                    return null;
                }

                var userContent = await userResponse.Content.ReadAsStringAsync();
                var userData = JsonConvert.DeserializeObject<dynamic>(userContent);

                // Parse birthday from Facebook format (MM/DD/YYYY)
                DateOnly? birthday = null;
                if (userData.birthday != null)
                {
                    try
                    {
                        var birthdayStr = (string)userData.birthday;
                        var parts = birthdayStr.Split('/');
                        if (parts.Length == 3)
                        {
                            birthday = new DateOnly(int.Parse(parts[2]), int.Parse(parts[0]), int.Parse(parts[1]));
                        }
                    }
                    catch (Exception ex)
                    {
                        string birthdayStr = userData.birthday?.ToString() ?? "null";
                        _logger.LogWarning(ex, "Failed to parse Facebook birthday: {Birthday}", birthdayStr);
                    }
                }

                return new ExternalUserInfoDto
                {
                    Id = userData.id,
                    Email = userData.email ?? $"{userData.id}@facebook.local",
                    Name = userData.name,
                    Picture = userData.picture?.data?.url,
                    Provider = "Facebook",
                    Birthday = birthday,
                    Gender = userData.gender,
                    Location = userData.location?.name
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
        private async Task<ExternalLoginResponseDto> HandleExistingUserLogin(User user, ExternalUserInfoDto? externalUserInfo = null)
        {
            // ✅ FIX: Check if Customer record exists, if not create one (for old users who logged in before Customer creation was implemented)
            if (user.RoleId == (int)UserRoles.Customer)
            {
                var existingCustomer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == user.UserId);
                if (existingCustomer == null && externalUserInfo != null)
                {
                    _logger.LogInformation("Customer record not found for existing user {UserId}. Creating Customer profile directly...", user.UserId);

                    // ✅ Create Customer directly using DbContext to avoid nested transaction issue
                    try
                    {
                        // Generate customer code
                        var customerCodeResult = await _context.Database
                            .SqlQueryRaw<string>("EXEC sp_GetNextCustomerCode")
                            .ToListAsync();
                        var customerCode = customerCodeResult.First();

                        var customer = new Customer
                        {
                            UserId = user.UserId,
                            CustomerCode = customerCode,
                            FullName = externalUserInfo.Name ?? user.FullName,
                            Email = externalUserInfo.Email ?? user.Email,
                            PhoneNumber = externalUserInfo.PhoneNumber ?? user.PhoneNumber ?? "",
                            DateOfBirth = externalUserInfo.Birthday,
                            Gender = MapGender(externalUserInfo.Gender),
                            Address = externalUserInfo.Location,
                            TypeId = 20, // Standard customer type
                            LoyaltyPoints = 0,
                            TotalSpent = 0,
                            IsActive = true,
                            CreatedDate = DateTime.UtcNow
                        };

                        _context.Customers.Add(customer);
                        await _context.SaveChangesAsync();

                        _logger.LogInformation("Successfully created Customer record {CustomerCode} for existing user {UserId}",
                            customerCode, user.UserId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to create Customer record for existing user {UserId}. User can still login.", user.UserId);
                        // Don't throw - user can still login
                    }
                }
            }

            // Update last login
            user.LastLoginDate = DateTime.UtcNow;
            user.FailedLoginAttempts = 0;
            await _userRepository.UpdateAsync(user);

            // Generate tokens
            var accessToken = _tokenService.GenerateToken(user);
            var refreshToken = await _userService.RotateRefreshTokenAsync(user.UserId, null, null);

            // TODO: Re-implement refresh token saving with the new RefreshToken entity model
            // await _userRepository.UpdateAsync(user);


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

                // ✅ IMPROVED: Tạo Customer record với thông tin đầy đủ từ external provider
                try
                {
                    var customerRequest = new CreateCustomerRequestDto
                    {
                        FullName = externalUser.Name,
                        Email = externalUser.Email,
                        PhoneNumber = externalUser.PhoneNumber ?? "", // PhoneNumber từ Facebook (nếu có)
                        DateOfBirth = externalUser.Birthday,  // Birthday từ Facebook/Google
                        Gender = MapGender(externalUser.Gender), // Map gender từ string sang bool
                        Address = externalUser.Location, // Location từ Facebook
                        TypeId = 20, // Standard customer type (TypeID starts from 20 in database)
                        IsActive = true
                    };

                    await _customerAccountService.CreateCustomerProfileForUserAsync(createdUser.UserId, customerRequest);
                    _logger.LogInformation("Created Customer record for user {UserId} with data from {Provider}: Birthday={Birthday}, Gender={Gender}, Location={Location}",
                        createdUser.UserId, provider, externalUser.Birthday, externalUser.Gender, externalUser.Location);
                }
                catch (Exception customerEx)
                {
                    _logger.LogError(customerEx, "Failed to create Customer record for external user {UserId}. Login succeeded but profile may be incomplete.", createdUser.UserId);
                    // Không throw để user vẫn đăng nhập được
                }

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
                var refreshToken = await _userService.RotateRefreshTokenAsync(createdUser.UserId, null, null);

                // TODO: Re-implement refresh token saving with the new RefreshToken entity model
                // await _userRepository.UpdateAsync(createdUser);


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

        /// <summary>
        /// Map gender string từ external provider sang gender string chuẩn
        /// </summary>
        private string? MapGender(string? gender)
        {
            if (string.IsNullOrWhiteSpace(gender))
                return null;

            var genderLower = gender.ToLower();

            // Facebook returns: "male", "female"
            // Google có thể return tương tự
            return genderLower switch
            {
                "male" => "Nam",
                "female" => "Nữ",
                "nam" => "Nam",
                "nữ" => "Nữ",
                "nu" => "Nữ",
                _ => null
            };
        }
    }
}