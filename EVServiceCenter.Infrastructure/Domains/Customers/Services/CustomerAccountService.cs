using EVServiceCenter.Core.Domains.Customers.DTOs.Requests;
using EVServiceCenter.Core.Domains.Customers.DTOs.Responses;
using EVServiceCenter.Core.Domains.Customers.Entities;
using EVServiceCenter.Core.Domains.Customers.Interfaces;
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
            using var transaction = await _context.Database.BeginTransactionAsync();

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

                // Create User với role Customer (will need email verification)
                var user = new User
                {
                    Username = customerRequest.Email ?? customerRequest.PhoneNumber, // Use email as username, fallback to phone
                    FullName = customerRequest.FullName,
                    Email = customerRequest.Email,
                    PhoneNumber = customerRequest.PhoneNumber,
                    RoleId = (int)UserRoles.Customer,
                    CreatedDate = DateTime.UtcNow
                };

                // Use the new RegisterCustomerUserAsync method (we'll create this)
                var createdUser = await _userService.RegisterCustomerUserAsync(user, password);

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
                    TypeId = customerRequest.TypeId ?? 1, // Default customer type
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

                _logger.LogInformation("Created customer with account: {CustomerCode} - {FullName} - UserID: {UserId}",
                    customer.CustomerCode, customer.FullName, createdUser.UserId);

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
        }


        public async Task<CustomerResponseDto> CreateCustomerProfileForUserAsync(
    int userId,
    CreateCustomerRequestDto customerRequest)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null || user.RoleId != (int)UserRoles.Customer)
                {
                    throw new InvalidOperationException("User không tồn tại hoặc không phải role Customer");
                }
                var customer = await _customerRepository.CreateAsync(customerRequest,userId);

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
        }

        public async Task<CustomerResponseDto?> GetCustomerByUserIdAsync(int userId)
        {
            var customer = await _context.Customers
                .Where(c => c.UserId == userId)
                .Select(c => new CustomerResponseDto
                {
                    CustomerId = c.CustomerId,
                    CustomerCode = c.CustomerCode,
                    FullName = c.FullName,
                    PhoneNumber = c.PhoneNumber,
                    Email = c.Email,
                    IsActive = c.IsActive
                })
                .FirstOrDefaultAsync();

            return customer;
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
            var bytes = Encoding.UTF8.GetBytes(identityNumber);
            return ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
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
