using EVServiceCenter.Core.Domains.CarBrands.DTOs.Requests;
using EVServiceCenter.Core.Domains.CarBrands.DTOs.Responses;
using EVServiceCenter.Core.Domains.Shared.Models;

namespace EVServiceCenter.Core.Domains.CarBrands.Interfaces.Services
{
    public interface ICarBrandQueryService
    {
        Task<PagedResult<CarBrandResponseDto>> GetAllAsync(
            CarBrandQueryDto query,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<CarBrandResponseDto>> GetActiveBrandsAsync(
            CancellationToken cancellationToken = default);

        Task<IEnumerable<CarBrandResponseDto>> GetBrandsByCountryAsync(
            string country,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<CarBrandResponseDto>> SearchBrandsAsync(
            string searchTerm,
            CancellationToken cancellationToken = default);
    }
}