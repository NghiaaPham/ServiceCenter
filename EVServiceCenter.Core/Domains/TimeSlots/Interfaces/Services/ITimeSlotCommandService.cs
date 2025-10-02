using EVServiceCenter.Core.Domains.TimeSlots.DTOs.Requests;
using EVServiceCenter.Core.Domains.TimeSlots.DTOs.Responses;

namespace EVServiceCenter.Core.Domains.TimeSlots.Interfaces.Services
{
    public interface ITimeSlotCommandService
    {
        Task<TimeSlotResponseDto> CreateAsync(
            CreateTimeSlotRequestDto request, CancellationToken cancellationToken = default);

        Task<TimeSlotResponseDto> UpdateAsync(
            UpdateTimeSlotRequestDto request, CancellationToken cancellationToken = default);

        Task<bool> DeleteAsync(
            int slotId, CancellationToken cancellationToken = default);

        Task<int> GenerateSlotsAsync(
            GenerateSlotsRequestDto request, CancellationToken cancellationToken = default);

        Task<int> DeleteEmptySlotsAsync(
            int centerId, DateOnly date, CancellationToken cancellationToken = default);

        Task<TimeSlotResponseDto> ToggleBlockAsync(
            int slotId, bool isBlocked, CancellationToken cancellationToken = default);
    }
}