using EVServiceCenter.Core.Domains.CarModels.DTOs.Requests;
using EVServiceCenter.Core.Domains.CarModels.DTOs.Responses;

namespace EVServiceCenter.Core.Domains.CarModels.Interfaces.Services
{
    public interface ICarModelService
    {
        Task<CarModelResponseDto?> GetByIdAsync(
            int modelId,
            CancellationToken cancellationToken = default);

        Task<CarModelResponseDto> CreateAsync(
            CreateCarModelRequestDto request,
            CancellationToken cancellationToken = default);

        Task<CarModelResponseDto> UpdateAsync(
            UpdateCarModelRequestDto request,
            CancellationToken cancellationToken = default);

        Task<bool> DeleteAsync(
            int modelId,
            CancellationToken cancellationToken = default);

        Task<bool> CanDeleteAsync(
            int modelId,
            CancellationToken cancellationToken = default);
    }
}