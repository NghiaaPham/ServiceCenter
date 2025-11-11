using EVServiceCenter.Core.Domains.Customers.DTOs.Requests;
using EVServiceCenter.Core.Domains.Customers.DTOs.Responses;
using EVServiceCenter.Core.Domains.Customers.Entities;
using EVServiceCenter.Core.Domains.Customers.Interfaces;
using EVServiceCenter.Core.Domains.CustomerTypes.DTOs.Responses;
using EVServiceCenter.Core.Domains.Identity.Entities;
using EVServiceCenter.Core.Domains.Identity.Interfaces;
using EVServiceCenter.Core.Entities;
using EVServiceCenter.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace EVServiceCenter.Infrastructure.Domains.Customers.Services
{
    public class CustomerAccountService : ICustomerAccountService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IUserService _userService;
        private readonly EVDbContext _context;
        private readonly ILogger<CustomerAccountService> _logger;

        public CustomerAccountService(
            ICustomerRepository customerRepository,
            IUserService userService,
            EVDbContext context,
            ILogger<CustomerAccountService> logger)
        {
            _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<CustomerResponseDto> CreateCustomerWithAccountAsync(
    CreateCustomerRequestDto customerRequest,
    string password)
        {
            // ✅ FIX: Use ExecutionStrategy to wrap transaction (prevent retry strategy conflict)
            var executionStrategy = _context.Database.CreateExecutionStrategy();

            return await executionStrategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // Validate unique constraints first
                    if (!string.IsNullOrEmpty(customerRequest.Email))
                    {
                        var existingUserByEmail = await _context.Users.AnyAsync(u => u.Email == customerRequest.Email);
                        if (existingUserByEmail)
                        {
                            throw new InvalidOperationException($"Email '{customerRequest.Email}' đã được sử dụng.");
                        }

                        var existingCustomerByEmail = await _context.Customers.AnyAsync(c => c.Email == customerRequest.Email);
                        if (existingCustomerByEmail)
                        {
                            throw new InvalidOperationException($"Email '{customerRequest.Email}' đã được sử dụng bởi khách hàng khác.");
                        }
                    }

                    var existingCustomerByPhone = await _context.Customers.AnyAsync(c => c.PhoneNumber == customerRequest.PhoneNumber);
                    if (existingCustomerByPhone)
                    {
                        throw new InvalidOperationException($"Số điện thoại '{customerRequest.PhoneNumber}' đã được sử dụng.");
                    }

                    // ✅ FIX: Get and VALIDATE CustomerType ID
                    int defaultTypeId;
                    
                    if (customerRequest.TypeId.HasValue)
                    {
                        // ✅ VALIDATE: Check if TypeId exists in database
                        var requestedType = await _context.CustomerTypes
                            .Where(ct => ct.TypeId == customerRequest.TypeId.Value && ct.IsActive == true)
                            .FirstOrDefaultAsync();

                        if (requestedType == null)
                        {
                            throw new InvalidOperationException(
                                $"Loại khách hàng với ID {customerRequest.TypeId.Value} không tồn tại hoặc không còn hoạt động. " +
                                "Vui lòng kiểm tra lại hoặc không chỉ định TypeId để sử dụng loại mặc định.");
                        }

                        defaultTypeId = requestedType.TypeId;
                        
                        _logger.LogInformation(
                            "Using requested CustomerType: {TypeId} - {TypeName}",
                            requestedType.TypeId, requestedType.TypeName);
                    }
                    else
                    {
                        // Get "Standard" CustomerType ID from database
                        var standardType = await _context.CustomerTypes
                            .Where(ct => ct.TypeName == "Standard" && ct.IsActive == true)
                            .FirstOrDefaultAsync();

                        if (standardType == null)
                        {
                            // ✅ FALLBACK: If "Standard" not found, use ANY active type
                            var anyActiveType = await _context.CustomerTypes
                                .Where(ct => ct.IsActive == true)
                                .OrderBy(ct => ct.TypeId)
                                .FirstOrDefaultAsync();

                            if (anyActiveType == null)
                            {
                                throw new InvalidOperationException(
                                    "Không tìm thấy bất kỳ loại khách hàng nào đang hoạt động trong hệ thống. " +
                                    "Vui lòng liên hệ quản trị viên để kiểm tra dữ liệu CustomerTypes.");
                            }

                            defaultTypeId = anyActiveType.TypeId;
                            
                            _logger.LogWarning(
                                "CustomerType 'Standard' not found. Using fallback type: {TypeId} - {TypeName}",
                                anyActiveType.TypeId, anyActiveType.TypeName);
                        }
                        else
                        {
                            defaultTypeId = standardType.TypeId;
                            
                            _logger.LogInformation(
                                "Using default CustomerType: {TypeId} - Standard",
                                standardType.TypeId);
                        }
                    }

                    // Create User với role Customer (WITHOUT sending email yet)
                    var user = new User
                    {
                        Username = customerRequest.Username ?? customerRequest.Email ?? customerRequest.PhoneNumber,
                        FullName = customerRequest.FullName,
                        Email = customerRequest.Email,
                        PhoneNumber = customerRequest.PhoneNumber,
                        RoleId = (int)UserRoles.Customer,
                        CreatedDate = DateTime.UtcNow
                    };

                    // ✅ FIX: Create user without sending email first
                    var createdUser = await _userService.RegisterCustomerUserWithoutEmailAsync(user, password);

                    // Generate customer code
                    var customerCode = await GenerateCustomerCodeAsync();

                    // Create Customer record with all registration info
                    var customer = new Customer
                    {
                        UserId = createdUser.UserId,
                        CustomerCode = customerCode,
                        FullName = customerRequest.FullName,
                        PhoneNumber = customerRequest.PhoneNumber,
                        Email = customerRequest.Email,
                        Address = customerRequest.Address,
                        DateOfBirth = customerRequest.DateOfBirth,
                        Gender = customerRequest.Gender,
                        IdentityNumber = !string.IsNullOrEmpty(customerRequest.IdentityNumber)
                            ? EncryptIdentityNumber(customerRequest.IdentityNumber)
                            : null,
                        TypeId = defaultTypeId,
                        PreferredLanguage = customerRequest.PreferredLanguage,
                        MarketingOptIn = customerRequest.MarketingOptIn,
                        LoyaltyPoints = 0,
                        TotalSpent = 0,
                        Notes = customerRequest.Notes,
                        IsActive = customerRequest.IsActive,
                        CreatedDate = DateTime.UtcNow
                    };

                    _context.Customers.Add(customer);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // ✅ FIX: Send email AFTER transaction commit successfully
                    try
                    {
                        await _userService.SendVerificationEmailAsync(createdUser);
                        _logger.LogInformation("Verification email sent to {Email}", customerRequest.Email);
                    }
                    catch (Exception emailEx)
                    {
                        _logger.LogWarning(emailEx, "Customer created successfully but failed to send verification email to {Email}", customerRequest.Email);
                        // Don't throw - customer already created, just log the warning
                    }

                    _logger.LogInformation("Created customer with account: {CustomerCode} - {FullName} - UserID: {UserId} - TypeId: {TypeId}",
                        customer.CustomerCode, customer.FullName, createdUser.UserId, defaultTypeId);

                    // Return full customer info
                    return new CustomerResponseDto
                    {
                        CustomerId = customer.CustomerId,
                        CustomerCode = customer.CustomerCode,
                        FullName = customer.FullName,
                        PhoneNumber = customer.PhoneNumber,
                        Email = customer.Email,
                        Address = customer.Address,
                        DateOfBirth = customer.DateOfBirth,
                        Gender = customer.Gender,
                        PreferredLanguage = customer.PreferredLanguage,
                        MarketingOptIn = customer.MarketingOptIn,
                        LoyaltyPoints = customer.LoyaltyPoints,
                        TotalSpent = customer.TotalSpent,
                        Notes = customer.Notes,
                        IsActive = customer.IsActive,
                        CreatedDate = customer.CreatedDate,
                        DisplayName = customer.CustomerCode + " - " + customer.FullName,
                        ContactInfo = !string.IsNullOrEmpty(customer.Email) ? customer.PhoneNumber + " / " + customer.Email : customer.PhoneNumber
                    };
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error creating customer with account: {FullName}", customerRequest.FullName);
                    throw;
                }
            });
        }


        public async Task<CustomerResponseDto> CreateCustomerProfileForUserAsync(
    int userId,
    CreateCustomerRequestDto customerRequest)
        {
            // ✅ FIX: Use ExecutionStrategy to wrap transaction
            var executionStrategy = _context.Database.CreateExecutionStrategy();

            return await executionStrategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var user = await _context.Users.FindAsync(userId);
                    if (user == null || user.RoleId != (int)UserRoles.Customer)
                    {
                        throw new InvalidOperationException("User không tồn tại hoặc không phải role Customer");
                    }
                    var customer = await _customerRepository.CreateAsync(customerRequest, userId);

                    await transaction.CommitAsync();

                    _logger.LogInformation("Created customer profile for user {UserId}, CustomerCode: {CustomerCode}",
                        userId, customer.CustomerCode);

                    return customer;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error creating customer profile for user {UserId}", userId);
                    throw;
                }
            });
        }

        public async Task<CustomerResponseDto?> GetCustomerByUserIdAsync(int userId)
        {
            // ✅ FIX: Include CustomerType to get full customer info
            var customer = await _context.Customers
                .AsNoTracking()
                .Include(c => c.Type) // ✅ ADDED: Include CustomerType for TypeName
                .Where(c => c.UserId == userId)
                .FirstOrDefaultAsync();

            if (customer == null)
            {
                return null;
            }

            // ✅ FIX: Calculate Age
            var age = customer.DateOfBirth.HasValue
                ? DateTime.Today.Year - customer.DateOfBirth.Value.Year -
                  (DateTime.Today.DayOfYear < customer.DateOfBirth.Value.DayOfYear ? 1 : 0)
                : 0;

            // ✅ FIX: Calculate LoyaltyStatus
            var loyaltyStatus = (customer.LoyaltyPoints ?? 0) switch
            {
                >= 10000 => "VIP",
                >= 5000 => "Gold",
                >= 2000 => "Silver",
                >= 500 => "Bronze",
                _ => ""
            };

            // ✅ FIX: Calculate LastVisitStatus
            var lastVisitStatus = customer.LastVisitDate.HasValue
                ? (DateOnly.FromDateTime(DateTime.Today).DayNumber - customer.LastVisitDate.Value.DayNumber) switch
                {
                    <= 7 => "Vừa ghé thăm",
                    <= 30 => "Ghé thăm gần đây",
                    <= 90 => "Lâu không ghé thăm",
                    _ => "Khách hàng cũ"
                }
                : "";

            // ✅ FIX: Load Vehicle Stats
            var vehicleStats = await _context.CustomerVehicles
                .Where(v => v.CustomerId == customer.CustomerId)
                .GroupBy(v => v.CustomerId)
                .Select(g => new
                {
                    VehicleCount = g.Count(),
                    ActiveVehicleCount = g.Count(v => v.IsActive == true)
                })
                .FirstOrDefaultAsync();

            // ✅ FIX: Map ALL fields with computed properties
            return new CustomerResponseDto
            {
                CustomerId = customer.CustomerId,
                CustomerCode = customer.CustomerCode,
                FullName = customer.FullName,
                PhoneNumber = customer.PhoneNumber,
                Email = customer.Email,
                Address = customer.Address,
                DateOfBirth = customer.DateOfBirth,
                Gender = customer.Gender,
                TypeId = customer.TypeId,
                LastVisitDate = customer.LastVisitDate,
                CustomerType = customer.Type != null ? new CustomerTypeResponseDto
                {
                    TypeId = customer.Type.TypeId,
                    TypeName = customer.Type.TypeName,
                    DiscountPercent = (decimal)(customer.Type.DiscountPercent ?? 0),
                    Description = customer.Type.Description,
                    IsActive = customer.Type.IsActive ?? false
                } : null,
                PreferredLanguage = customer.PreferredLanguage,
                MarketingOptIn = customer.MarketingOptIn,
                LoyaltyPoints = customer.LoyaltyPoints,
                TotalSpent = customer.TotalSpent,
                Notes = customer.Notes,
                IsActive = customer.IsActive,
                CreatedDate = customer.CreatedDate,
                
                // ✅ COMPUTED PROPERTIES
                Age = age,
                DisplayName = customer.CustomerCode + " - " + customer.FullName,
                ContactInfo = !string.IsNullOrEmpty(customer.Email)
                    ? customer.PhoneNumber + " / " + customer.Email
                    : customer.PhoneNumber,
                LoyaltyStatus = loyaltyStatus,
                PotentialDiscount = customer.Type != null ? customer.Type.DiscountPercent ?? 0 : 0,
                LastVisitStatus = lastVisitStatus,
                VehicleCount = vehicleStats?.VehicleCount ?? 0,
                ActiveVehicleCount = vehicleStats?.ActiveVehicleCount ?? 0,
                RecentVehicles = new List<CustomerVehicleSummaryDto>()
            };
        }

        public async Task<bool> LinkCustomerToUserAsync(int customerId, int userId)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(customerId);
                if (customer == null) return false;

                var user = await _context.Users.FindAsync(userId);
                if (user == null || user.RoleId != (int)UserRoles.Customer) return false;

                customer.UserId = userId;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Linked customer {CustomerId} with user {UserId}", customerId, userId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error linking customer {CustomerId} to user {UserId}",
                    customerId, userId);
                return false;
            }
        }

        private static byte[] EncryptIdentityNumber(string identityNumber)
        {
        #if WINDOWS
            var bytes = Encoding.UTF8.GetBytes(identityNumber);
            return ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
        #else
                    throw new PlatformNotSupportedException("Identity number encryption is only supported on Windows.");
        #endif
        }

        public async Task<string> GenerateCustomerCodeAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _context.Database
                    .SqlQueryRaw<string>("EXEC sp_GetNextCustomerCode")
                    .ToListAsync(cancellationToken);

                return result.First();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating customer code with sequence");
                throw;
            }
        }
    }
}
