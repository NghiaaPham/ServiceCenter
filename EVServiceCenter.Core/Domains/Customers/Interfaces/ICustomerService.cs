using EVServiceCenter.Core.Domains.Customers.DTOs.Requests;
using EVServiceCenter.Core.Domains.Customers.DTOs.Responses;
using EVServiceCenter.Core.Domains.Shared.Models;
namespace EVServiceCenter.Core.Domains.Customers.Interfaces
{
    public interface ICustomerService
    {
        Task<PagedResult<CustomerResponseDto>> GetAllAsync(CustomerQueryDto query, CancellationToken cancellationToken = default);
        Task<CustomerResponseDto?> GetByIdAsync(int customerId, bool includeStats = true, CancellationToken cancellationToken = default);
        Task<CustomerResponseDto?> GetByCustomerCodeAsync(string customerCode, CancellationToken cancellationToken = default);
        Task<CustomerResponseDto?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default);
        Task<CustomerResponseDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<CustomerResponseDto> CreateWalkInCustomerAsync(
            CreateWalkInCustomerDto request,
            int createdByUserId,
            CancellationToken cancellationToken = default);
        Task<CustomerResponseDto> UpdateAsync(UpdateCustomerRequestDto request, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int customerId, CancellationToken cancellationToken = default);
        Task<IEnumerable<CustomerResponseDto>> GetActiveCustomersAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<CustomerResponseDto>> GetCustomersWithMaintenanceDueAsync(CancellationToken cancellationToken = default);
        Task<Dictionary<string, int>> GetCustomerStatisticsAsync(CancellationToken cancellationToken = default);
        Task<bool> CanDeleteAsync(int customerId, CancellationToken cancellationToken = default);
        Task<bool> AddLoyaltyPointsAsync(int customerId, int points, string reason, CancellationToken cancellationToken = default);
        Task<bool> ProcessPurchaseAsync(int customerId, decimal amount, CancellationToken cancellationToken = default);
    }
}
