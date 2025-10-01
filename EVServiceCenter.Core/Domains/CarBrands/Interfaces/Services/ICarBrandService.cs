using EVServiceCenter.Core.Domains.CarBrands.DTOs.Requests;
using EVServiceCenter.Core.Domains.CarBrands.DTOs.Responses;

namespace EVServiceCenter.Core.Domains.CarBrands.Interfaces.Services
{
    public interface ICarBrandService
    {
        Task<CarBrandResponseDto?> GetByIdAsync(
            int brandId,
            CancellationToken cancellationToken = default);

        Task<CarBrandResponseDto?> GetByNameAsync(
            string brandName,
            CancellationToken cancellationToken = default);

        Task<CarBrandResponseDto> CreateAsync(
            CreateCarBrandRequestDto request,
            CancellationToken cancellationToken = default);

        Task<CarBrandResponseDto> UpdateAsync(
            UpdateCarBrandRequestDto request,
            CancellationToken cancellationToken = default);

        Task<bool> DeleteAsync(
            int brandId,
            CancellationToken cancellationToken = default);

        Task<bool> CanDeleteAsync(
            int brandId,
            CancellationToken cancellationToken = default);

        Task<bool> IsBrandNameExistsAsync(
            string brandName,
            int? excludeBrandId = null,
            CancellationToken cancellationToken = default);
    }
}