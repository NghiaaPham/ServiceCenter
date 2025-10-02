using EVServiceCenter.Core.Domains.AppointmentManagement.Entities;
using EVServiceCenter.Core.Domains.TimeSlots.Entities;

namespace EVServiceCenter.Core.Domains.TimeSlots.Interfaces.Repositories
{
    public interface ITimeSlotRepository
    {
        Task<TimeSlot?> GetByIdAsync(int slotId, CancellationToken cancellationToken = default);
        Task<TimeSlot?> GetByIdWithDetailsAsync(int slotId, CancellationToken cancellationToken = default);
        Task<IEnumerable<TimeSlot>> GetAllAsync(CancellationToken cancellationToken = default);
        IQueryable<TimeSlot> GetQueryable();
        Task<TimeSlot> CreateAsync(TimeSlot slot);
        Task UpdateAsync(TimeSlot slot);
        Task<bool> DeleteAsync(int slotId);
    }
}