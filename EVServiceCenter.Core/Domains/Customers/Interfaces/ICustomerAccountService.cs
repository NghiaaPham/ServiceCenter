using EVServiceCenter.Core.Domains.Customers.DTOs.Requests;
using EVServiceCenter.Core.Domains.Customers.DTOs.Responses;

namespace EVServiceCenter.Core.Domains.Customers.Interfaces
{
    public interface ICustomerAccountService
    {
        Task<CustomerResponseDto> CreateCustomerWithAccountAsync(CreateCustomerRequestDto customerRequest, string password);
        Task<CustomerResponseDto> CreateCustomerProfileForUserAsync(int userId, CreateCustomerRequestDto customerRequest);
        Task<CustomerResponseDto?> GetCustomerByUserIdAsync(int userId);
        Task<bool> LinkCustomerToUserAsync(int customerId, int userId);
    }
}
