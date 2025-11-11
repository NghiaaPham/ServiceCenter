using EVServiceCenter.Core.Domains.ServiceRatings.DTOs.Requests;
using EVServiceCenter.Core.Domains.ServiceRatings.DTOs.Responses;

namespace EVServiceCenter.Core.Domains.ServiceRatings.Interfaces;

/// <summary>
/// Service rating service interface
/// Handles rating creation and retrieval
/// </summary>
public interface IServiceRatingService
{
    /// <summary>
    /// Create service rating for work order
    /// </summary>
    Task<ServiceRatingResponseDto> CreateRatingAsync(
        CreateServiceRatingRequestDto request,
        int customerId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get rating by ID
    /// </summary>
    Task<ServiceRatingResponseDto?> GetRatingByIdAsync(
        int ratingId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get rating by work order ID
    /// </summary>
    Task<ServiceRatingResponseDto?> GetRatingByWorkOrderIdAsync(
        int workOrderId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get service center ratings summary
    /// </summary>
    Task<ServiceCenterRatingsResponseDto> GetServiceCenterRatingsAsync(
        int serviceCenterId,
        int? top = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if work order can be rated
    /// </summary>
    Task<bool> CanRateWorkOrderAsync(
        int workOrderId,
        int customerId,
        CancellationToken cancellationToken = default);
}
