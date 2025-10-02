using EVServiceCenter.Core.Domains.TimeSlots.Entities;
using EVServiceCenter.Core.Domains.TimeSlots.Interfaces.Repositories;
using EVServiceCenter.Core.Entities;
using EVServiceCenter.Core.Helpers;
using Microsoft.EntityFrameworkCore;

namespace EVServiceCenter.Infrastructure.Domains.TimeSlots.Repositories
{
    public class TimeSlotQueryRepository : ITimeSlotQueryRepository
    {
        private readonly EVDbContext _context;

        public TimeSlotQueryRepository(EVDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<TimeSlot>> GetSlotsByCenterAndDateAsync(
            int centerId,
            DateOnly date,
            CancellationToken cancellationToken = default)
        {
            return await _context.TimeSlots
                .Include(s => s.Center)
                .Include(s => s.Appointments)
                .Where(s => s.CenterId == centerId && s.SlotDate == date)
                .OrderBy(s => s.StartTime)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<TimeSlot>> GetAvailableSlotsAsync(
            int centerId,
            DateOnly startDate,
            DateOnly endDate,
            CancellationToken cancellationToken = default)
        {
            var now = DateTime.Now;
            var today = DateOnly.FromDateTime(now);
            var currentTime = TimeOnly.FromDateTime(now);

            return await _context.TimeSlots
                .Include(s => s.Center)
                .Include(s => s.Appointments)
                .Where(s =>
                    s.CenterId == centerId &&
                    s.SlotDate >= startDate &&
                    s.SlotDate <= endDate &&
                    !s.IsBlocked &&
                    (s.SlotDate > today || (s.SlotDate == today && s.StartTime > currentTime)))
                .OrderBy(s => s.SlotDate)
                .ThenBy(s => s.StartTime)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<TimeSlot>> GetSlotsByDateRangeAsync(
            int centerId,
            DateOnly startDate,
            DateOnly endDate,
            CancellationToken cancellationToken = default)
        {
            return await _context.TimeSlots
                .Include(s => s.Center)
                .Where(s =>
                    s.CenterId == centerId &&
                    s.SlotDate >= startDate &&
                    s.SlotDate <= endDate)
                .OrderBy(s => s.SlotDate)
                .ThenBy(s => s.StartTime)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> IsSlotAvailableAsync(
            int slotId,
            CancellationToken cancellationToken = default)
        {
            var slot = await _context.TimeSlots
                .Include(s => s.Appointments)
                .FirstOrDefaultAsync(s => s.SlotId == slotId, cancellationToken);

            return slot?.IsAvailable ?? false;
        }

        public async Task<int> GetBookingCountAsync(
     int slotId,
     CancellationToken cancellationToken = default)
        {
            var activeStatuses = AppointmentStatusHelper.ActiveBookings;

            return await _context.Appointments
                .Where(a =>
                    a.SlotId == slotId &&
                    activeStatuses.Contains(a.StatusId))  
                .CountAsync(cancellationToken);
        }

        public async Task<bool> ExistsAsync(
            int centerId,
            DateOnly date,
            TimeOnly startTime,
            CancellationToken cancellationToken = default)
        {
            return await _context.TimeSlots
                .AnyAsync(s =>
                    s.CenterId == centerId &&
                    s.SlotDate == date &&
                    s.StartTime == startTime,
                    cancellationToken);
        }
    }
}