using EVServiceCenter.Core.Domains.Customers.DTOs.Requests;
using EVServiceCenter.Core.Domains.Customers.DTOs.Responses;
using EVServiceCenter.Core.Domains.Customers.Interfaces;
using EVServiceCenter.Core.Domains.Shared.Models;
using Microsoft.Extensions.Logging;

namespace EVServiceCenter.Infrastructure.Domains.Customers.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _repository;
        private readonly ILogger<CustomerService> _logger;

        public CustomerService(
            ICustomerRepository repository,
            ILogger<CustomerService> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<PagedResult<CustomerResponseDto>> GetAllAsync(
            CustomerQueryDto query,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Retrieving customers with query: {@Query}", query);
                return await _repository.GetAllAsync(query, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in service layer retrieving customers");
                throw;
            }
        }

        public async Task<CustomerResponseDto?> GetByIdAsync(
            int customerId,
            bool includeStats = true,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (customerId <= 0)
                {
                    throw new ArgumentException("Customer ID must be greater than 0", nameof(customerId));
                }

                _logger.LogDebug("Retrieving customer by ID: {CustomerId}, includeStats: {IncludeStats}", customerId, includeStats);
                return await _repository.GetByIdAsync(customerId, includeStats, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in service layer retrieving customer by ID: {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<CustomerResponseDto?> GetByCustomerCodeAsync(string customerCode, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(customerCode))
                {
                    throw new ArgumentException("Customer code cannot be empty", nameof(customerCode));
                }

                _logger.LogDebug("Retrieving customer by code: {CustomerCode}", customerCode);
                return await _repository.GetByCustomerCodeAsync(customerCode, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in service layer retrieving customer by code: {CustomerCode}", customerCode);
                throw;
            }
        }

        public async Task<CustomerResponseDto?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(phoneNumber))
                {
                    throw new ArgumentException("Phone number cannot be empty", nameof(phoneNumber));
                }

                _logger.LogDebug("Retrieving customer by phone: {PhoneNumber}", phoneNumber);
                return await _repository.GetByPhoneNumberAsync(phoneNumber, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in service layer retrieving customer by phone: {PhoneNumber}", phoneNumber);
                throw;
            }
        }

        public async Task<CustomerResponseDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    throw new ArgumentException("Email cannot be empty", nameof(email));
                }

                _logger.LogDebug("Retrieving customer by email: {Email}", email);
                return await _repository.GetByEmailAsync(email, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in service layer retrieving customer by email: {Email}", email);
                throw;
            }
        }

        public async Task<CustomerResponseDto> UpdateAsync(
            UpdateCustomerRequestDto request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Updating customer: {CustomerId}", request.CustomerId);
                var result = await _repository.UpdateAsync(request, cancellationToken);

                _logger.LogInformation("Successfully updated customer: {CustomerCode} - {FullName}",
                    result.CustomerCode, result.FullName);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in service layer updating customer: {CustomerId}", request.CustomerId);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int customerId, CancellationToken cancellationToken = default)
        {
            try
            {
                if (customerId <= 0)
                {
                    throw new ArgumentException("Customer ID must be greater than 0", nameof(customerId));
                }

                // Check if customer can be deleted
                var canDelete = await CanDeleteAsync(customerId, cancellationToken);
                if (!canDelete)
                {
                    throw new InvalidOperationException("Cannot delete customer that has associated vehicles or transactions");
                }

                _logger.LogDebug("Deleting customer: {CustomerId}", customerId);
                var result = await _repository.DeleteAsync(customerId, cancellationToken);

                if (result)
                {
                    _logger.LogInformation("Successfully deleted customer: {CustomerId}", customerId);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in service layer deleting customer: {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<IEnumerable<CustomerResponseDto>> GetActiveCustomersAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Retrieving active customers");
                return await _repository.GetActiveCustomersAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in service layer retrieving active customers");
                throw;
            }
        }

        public async Task<IEnumerable<CustomerResponseDto>> GetCustomersWithMaintenanceDueAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Retrieving customers with maintenance due");
                return await _repository.GetCustomersWithMaintenanceDueAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in service layer retrieving customers with maintenance due");
                throw;
            }
        }

        public async Task<Dictionary<string, int>> GetCustomerStatisticsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Retrieving customer statistics");
                return await _repository.GetCustomerStatsByTypeAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in service layer retrieving customer statistics");
                throw;
            }
        }

        public async Task<bool> CanDeleteAsync(int customerId, CancellationToken cancellationToken = default)
        {
            try
            {
                // Customer can only be deleted if it has no associated vehicles
                return !await _repository.HasVehiclesAsync(customerId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if customer can be deleted: {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<bool> AddLoyaltyPointsAsync(int customerId, int points, string reason, CancellationToken cancellationToken = default)
        {
            try
            {
                if (customerId <= 0)
                {
                    throw new ArgumentException("Customer ID must be greater than 0", nameof(customerId));
                }

                if (points <= 0)
                {
                    throw new ArgumentException("Points must be greater than 0", nameof(points));
                }

                _logger.LogDebug("Adding {Points} loyalty points to customer {CustomerId} for reason: {Reason}",
                    points, customerId, reason);

                var result = await _repository.UpdateLoyaltyPointsAsync(customerId, points, cancellationToken);

                if (result)
                {
                    _logger.LogInformation("Successfully added {Points} loyalty points to customer {CustomerId}",
                        points, customerId);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding loyalty points to customer: {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<bool> ProcessPurchaseAsync(int customerId, decimal amount, CancellationToken cancellationToken = default)
        {
            try
            {
                if (customerId <= 0)
                {
                    throw new ArgumentException("Customer ID must be greater than 0", nameof(customerId));
                }

                if (amount <= 0)
                {
                    throw new ArgumentException("Amount must be greater than 0", nameof(amount));
                }

                _logger.LogDebug("Processing purchase of {Amount} VND for customer {CustomerId}", amount, customerId);

                var result = await _repository.UpdateTotalSpentAsync(customerId, amount, cancellationToken);

                if (result)
                {
                    _logger.LogInformation("Successfully processed purchase of {Amount} VND for customer {CustomerId}",
                        amount, customerId);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing purchase for customer: {CustomerId}", customerId);
                throw;
            }
        }
        public async Task<CustomerResponseDto> CreateWalkInCustomerAsync(
            CreateWalkInCustomerDto request,
            int createdByUserId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Creating walk-in customer: {FullName} by user {UserId}",
                    request.FullName, createdByUserId);

                // Map CreateWalkInCustomerDto → CreateCustomerRequestDto (cho Repository)
                var repositoryRequest = new CreateCustomerRequestDto
                {
                    FullName = request.FullName,
                    PhoneNumber = request.PhoneNumber,
                    Email = request.Email,
                    Address = request.Address,
                    DateOfBirth = request.DateOfBirth,
                    Gender = request.Gender,
                    TypeId = request.TypeId ?? 1,  // Default type
                    PreferredLanguage = "vi-VN",
                    MarketingOptIn = false,  // Walk-in customers mặc định false
                    Notes = request.Notes,
                    IsActive = true
                };

                var result = await _repository.CreateAsync(repositoryRequest, userId: null, cancellationToken);

                _logger.LogInformation("Walk-in customer created: {CustomerCode} - {FullName} by user {UserId}",
                    result.CustomerCode, result.FullName, createdByUserId);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating walk-in customer: {FullName}", request.FullName);
                throw;
            }
        }
    }
}
