using EVServiceCenter.Core.Domains.TimeSlots.Entities;
using EVServiceCenter.Core.Domains.TimeSlots.Interfaces.Repositories;
using EVServiceCenter.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace EVServiceCenter.Infrastructure.Domains.TimeSlots.Repositories
{
    public class TimeSlotCommandRepository : ITimeSlotCommandRepository
    {
        private readonly EVDbContext _context;

        public TimeSlotCommandRepository(EVDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<bool> HasConflictAsync(
            int centerId,
            DateOnly date,
            TimeOnly startTime,
            TimeOnly endTime,
            int? excludeSlotId = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.TimeSlots
                .Where(s =>
                    s.CenterId == centerId &&
                    s.SlotDate == date &&
                    ((s.StartTime < endTime && s.EndTime > startTime)));

            if (excludeSlotId.HasValue)
            {
                query = query.Where(s => s.SlotId != excludeSlotId.Value);
            }

            return await query.AnyAsync(cancellationToken);
        }

        public async Task<int> BulkCreateAsync(
            IEnumerable<TimeSlot> slots,
            CancellationToken cancellationToken = default)
        {
            var slotList = slots.ToList();
            foreach (var slot in slotList)
            {
                slot.CreatedDate = DateTime.UtcNow;
            }

            await _context.TimeSlots.AddRangeAsync(slotList, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return slotList.Count;
        }

        public async Task DeleteSlotsByDateRangeAsync(
            int centerId,
            DateOnly startDate,
            DateOnly endDate,
            CancellationToken cancellationToken = default)
        {
            var slots = await _context.TimeSlots
                .Where(s =>
                    s.CenterId == centerId &&
                    s.SlotDate >= startDate &&
                    s.SlotDate <= endDate &&
                    !s.Appointments.Any())  
                .ToListAsync(cancellationToken);

            if (slots.Any())
            {
                _context.TimeSlots.RemoveRange(slots);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<int> DeleteEmptySlotsAsync(
            int centerId,
            DateOnly date,
            CancellationToken cancellationToken = default)
        {
            var slots = await _context.TimeSlots
                .Where(s =>
                    s.CenterId == centerId &&
                    s.SlotDate == date &&
                    !s.Appointments.Any())
                .ToListAsync(cancellationToken);

            if (slots.Any())
            {
                _context.TimeSlots.RemoveRange(slots);
                await _context.SaveChangesAsync(cancellationToken);
            }

            return slots.Count;
        }
    }
}