using EVServiceCenter.Core.Domains.CarModels.DTOs.Requests;
using EVServiceCenter.Core.Domains.CarModels.DTOs.Responses;
using EVServiceCenter.Core.Domains.Shared.Models;

namespace EVServiceCenter.Core.Domains.CarModels.Interfaces.Services
{
    public interface ICarModelQueryService
    {
        Task<PagedResult<CarModelResponseDto>> GetAllAsync(
            CarModelQueryDto query,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<CarModelResponseDto>> GetActiveModelsAsync(
            CancellationToken cancellationToken = default);

        Task<IEnumerable<CarModelResponseDto>> GetModelsByBrandAsync(
            int brandId,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<CarModelResponseDto>> SearchModelsAsync(
            string searchTerm,
            CancellationToken cancellationToken = default);
    }
}