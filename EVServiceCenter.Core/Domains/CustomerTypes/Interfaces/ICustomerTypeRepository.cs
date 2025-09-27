using EVServiceCenter.Core.Domains.CustomerTypes.DTOs.Requests;
using EVServiceCenter.Core.Domains.CustomerTypes.DTOs.Responses;
using EVServiceCenter.Core.Domains.Shared.Models;

namespace EVServiceCenter.Core.Domains.CustomerTypes.Interfaces
{
    public interface ICustomerTypeRepository
    {
        Task<PagedResult<CustomerTypeResponseDto>> GetAllAsync(CustomerTypeQueryDto query, CancellationToken cancellationToken = default);
        Task<CustomerTypeResponseDto?> GetByIdAsync(int typeId, bool includeStats = true, CancellationToken cancellationToken = default);
        Task<CustomerTypeResponseDto> CreateAsync(CreateCustomerTypeRequestDto request, CancellationToken cancellationToken = default);
        Task<CustomerTypeResponseDto> UpdateAsync(UpdateCustomerTypeRequestDto request, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int typeId, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(string typeName, int? excludeTypeId = null, CancellationToken cancellationToken = default);
        Task<bool> HasCustomersAsync(int typeId, CancellationToken cancellationToken = default);
        Task<IEnumerable<CustomerTypeResponseDto>> GetActiveAsync(CancellationToken cancellationToken = default);
    }
}
