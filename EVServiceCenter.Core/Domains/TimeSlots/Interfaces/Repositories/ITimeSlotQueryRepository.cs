using EVServiceCenter.Core.Domains.AppointmentManagement.Entities;
using EVServiceCenter.Core.Domains.TimeSlots.Entities;

namespace EVServiceCenter.Core.Domains.TimeSlots.Interfaces.Repositories
{
    public interface ITimeSlotQueryRepository
    {
        Task<IEnumerable<TimeSlot>> GetSlotsByCenterAndDateAsync(
            int centerId, DateOnly date, CancellationToken cancellationToken = default);

        Task<IEnumerable<TimeSlot>> GetAvailableSlotsAsync(
            int centerId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default);

        Task<IEnumerable<TimeSlot>> GetSlotsByDateRangeAsync(
            int centerId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default);

        Task<bool> IsSlotAvailableAsync(int slotId, CancellationToken cancellationToken = default);

        Task<int> GetBookingCountAsync(int slotId, CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(int centerId, DateOnly date, TimeOnly startTime, CancellationToken cancellationToken = default);
    }
}