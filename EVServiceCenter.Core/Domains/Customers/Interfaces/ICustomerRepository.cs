using EVServiceCenter.Core.Domains.Customers.DTOs.Requests;
using EVServiceCenter.Core.Domains.Customers.DTOs.Responses;
using EVServiceCenter.Core.Domains.Shared.Models;

namespace EVServiceCenter.Core.Domains.Customers.Interfaces
{
    public interface ICustomerRepository
    {
        Task<PagedResult<CustomerResponseDto>> GetAllAsync(CustomerQueryDto query, CancellationToken cancellationToken = default);
        Task<CustomerResponseDto?> GetByIdAsync(int customerId, bool includeStats = true, CancellationToken cancellationToken = default);
        Task<CustomerResponseDto?> GetByCustomerCodeAsync(string customerCode, CancellationToken cancellationToken = default);
        Task<CustomerResponseDto?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default);
        Task<CustomerResponseDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<CustomerResponseDto> CreateAsync(CreateCustomerRequestDto request, int? userId = null, CancellationToken cancellationToken = default);
        Task<CustomerResponseDto> UpdateAsync(UpdateCustomerRequestDto request, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int customerId, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(string phoneNumber, int? excludeCustomerId = null, CancellationToken cancellationToken = default);
        Task<bool> EmailExistsAsync(string email, int? excludeCustomerId = null, CancellationToken cancellationToken = default);
        Task<bool> IdentityNumberExistsAsync(string identityNumber, int? excludeCustomerId = null, CancellationToken cancellationToken = default);
        Task<bool> HasVehiclesAsync(int customerId, CancellationToken cancellationToken = default);
        Task<string> GenerateCustomerCodeAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<CustomerResponseDto>> GetActiveCustomersAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<CustomerResponseDto>> GetCustomersWithMaintenanceDueAsync(CancellationToken cancellationToken = default);
        Task<Dictionary<string, int>> GetCustomerStatsByTypeAsync(CancellationToken cancellationToken = default);
        Task<bool> UpdateLoyaltyPointsAsync(int customerId, int points, CancellationToken cancellationToken = default);
        Task<bool> UpdateTotalSpentAsync(int customerId, decimal amount, CancellationToken cancellationToken = default);
    }
}
