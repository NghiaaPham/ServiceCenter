using EVServiceCenter.Core.Domains.ServiceCategories.DTOs.Requests;
using EVServiceCenter.Core.Domains.ServiceCategories.DTOs.Responses;
using EVServiceCenter.Core.Domains.Shared.Models;

namespace EVServiceCenter.Core.Domains.ServiceCategories.Interfaces.Services
{
    public interface IServiceCategoryService
    {
        Task<PagedResult<ServiceCategoryResponseDto>> GetAllAsync(
            ServiceCategoryQueryDto query,
            CancellationToken cancellationToken = default);

        Task<ServiceCategoryResponseDto?> GetByIdAsync(
            int categoryId,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<ServiceCategoryResponseDto>> GetActiveCategoriesAsync(
            CancellationToken cancellationToken = default);

        Task<ServiceCategoryResponseDto> CreateAsync(
            CreateServiceCategoryRequestDto request,
            CancellationToken cancellationToken = default);

        Task<ServiceCategoryResponseDto> UpdateAsync(
            UpdateServiceCategoryRequestDto request,
            CancellationToken cancellationToken = default);

        Task<bool> DeleteAsync(
            int categoryId,
            CancellationToken cancellationToken = default);

        Task<bool> CanDeleteAsync(
            int categoryId,
            CancellationToken cancellationToken = default);
    }
}