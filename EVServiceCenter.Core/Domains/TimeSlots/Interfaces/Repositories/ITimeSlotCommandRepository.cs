using EVServiceCenter.Core.Domains.AppointmentManagement.Entities;
using EVServiceCenter.Core.Domains.TimeSlots.Entities;

namespace EVServiceCenter.Core.Domains.TimeSlots.Interfaces.Repositories
{
    public interface ITimeSlotCommandRepository
    {
        Task<bool> HasConflictAsync(
            int centerId, DateOnly date, TimeOnly startTime, TimeOnly endTime,
            int? excludeSlotId = null, CancellationToken cancellationToken = default);

        Task<int> BulkCreateAsync(IEnumerable<TimeSlot> slots, CancellationToken cancellationToken = default);

        Task DeleteSlotsByDateRangeAsync(
            int centerId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default);

        Task<int> DeleteEmptySlotsAsync(int centerId, DateOnly date, CancellationToken cancellationToken = default);
    }
}