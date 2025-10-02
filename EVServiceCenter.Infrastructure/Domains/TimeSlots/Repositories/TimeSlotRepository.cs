using EVServiceCenter.Core.Domains.TimeSlots.Entities;
using EVServiceCenter.Core.Domains.TimeSlots.Interfaces.Repositories;
using EVServiceCenter.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace EVServiceCenter.Infrastructure.Domains.TimeSlots.Repositories
{
    public class TimeSlotRepository : ITimeSlotRepository
    {
        protected readonly EVDbContext _context;

        public TimeSlotRepository(EVDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<TimeSlot?> GetByIdAsync(int slotId, CancellationToken cancellationToken = default)
        {
            return await _context.TimeSlots
                .FirstOrDefaultAsync(s => s.SlotId == slotId, cancellationToken);
        }

        public async Task<TimeSlot?> GetByIdWithDetailsAsync(int slotId, CancellationToken cancellationToken = default)
        {
            return await _context.TimeSlots
                .Include(s => s.Center)
                .Include(s => s.Appointments)
                .FirstOrDefaultAsync(s => s.SlotId == slotId, cancellationToken);
        }

        public IQueryable<TimeSlot> GetQueryable()
        {
            return _context.TimeSlots.AsQueryable();
        }

        public async Task<TimeSlot> CreateAsync(TimeSlot slot)
        {
            slot.CreatedDate = DateTime.UtcNow;
            _context.TimeSlots.Add(slot);
            await _context.SaveChangesAsync();
            return slot;
        }

        public async Task UpdateAsync(TimeSlot slot)
        {
            slot.UpdatedDate = DateTime.UtcNow;
            _context.TimeSlots.Update(slot);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int slotId)
        {
            var slot = await GetByIdAsync(slotId);
            if (slot == null)
                return false;

            _context.TimeSlots.Remove(slot);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<TimeSlot>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.TimeSlots
                .Include(s => s.Center)
                .ToListAsync(cancellationToken);
        }
    }
}