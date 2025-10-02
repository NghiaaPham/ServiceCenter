using EVServiceCenter.Core.Domains.ModelServicePricings.DTOs.Requests;
using EVServiceCenter.Core.Domains.ModelServicePricings.DTOs.Responses;
using EVServiceCenter.Core.Domains.Shared.Models;

namespace EVServiceCenter.Core.Domains.ModelServicePricings.Interfaces.Services
{
    public interface IModelServicePricingService
    {
        Task<PagedResult<ModelServicePricingResponseDto>> GetAllAsync(
            ModelServicePricingQueryDto query,
            CancellationToken cancellationToken = default);

        Task<ModelServicePricingResponseDto?> GetByIdAsync(
            int pricingId,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<ModelServicePricingResponseDto>> GetByModelIdAsync(
            int modelId,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<ModelServicePricingResponseDto>> GetByServiceIdAsync(
            int serviceId,
            CancellationToken cancellationToken = default);

        Task<ModelServicePricingResponseDto?> GetActivePricingAsync(
            int modelId,
            int serviceId,
            DateOnly? forDate = null,
            CancellationToken cancellationToken = default);

        Task<ModelServicePricingResponseDto> CreateAsync(
            CreateModelServicePricingRequestDto request,
            CancellationToken cancellationToken = default);

        Task<ModelServicePricingResponseDto> UpdateAsync(
            UpdateModelServicePricingRequestDto request,
            CancellationToken cancellationToken = default);

        Task<bool> DeleteAsync(
            int pricingId,
            CancellationToken cancellationToken = default);
    }
}