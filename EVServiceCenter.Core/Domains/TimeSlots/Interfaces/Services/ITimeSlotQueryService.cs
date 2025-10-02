using EVServiceCenter.Core.Domains.Shared.Models;
using EVServiceCenter.Core.Domains.TimeSlots.DTOs.Requests;
using EVServiceCenter.Core.Domains.TimeSlots.DTOs.Responses;

namespace EVServiceCenter.Core.Domains.TimeSlots.Interfaces.Services
{
    public interface ITimeSlotQueryService
    {
        Task<PagedResult<TimeSlotResponseDto>> GetAllAsync(
            TimeSlotQueryDto query, CancellationToken cancellationToken = default);

        Task<TimeSlotResponseDto?> GetByIdAsync(
            int slotId, CancellationToken cancellationToken = default);

        Task<IEnumerable<TimeSlotResponseDto>> GetAvailableSlotsAsync(
            int centerId, DateOnly date, CancellationToken cancellationToken = default);

        Task<IEnumerable<TimeSlotResponseDto>> GetAvailableSlotsByDateRangeAsync(
            int centerId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default);

        Task<IEnumerable<TimeSlotResponseDto>> GetSlotsByCenterAndDateAsync(
            int centerId, DateOnly date, CancellationToken cancellationToken = default);

        Task<bool> IsSlotAvailableAsync(
            int slotId, CancellationToken cancellationToken = default);

        Task<int> GetBookingCountAsync(
            int slotId, CancellationToken cancellationToken = default);
    }
}