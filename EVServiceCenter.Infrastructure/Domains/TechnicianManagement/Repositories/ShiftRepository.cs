using EVServiceCenter.Core.Domains.TechnicianManagement.Interfaces;
using EVServiceCenter.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace EVServiceCenter.Infrastructure.Domains.TechnicianManagement.Repositories
{
    /// <summary>
    /// Repository implementation for Shift data access
    /// </summary>
    public class ShiftRepository : IShiftRepository
    {
        private readonly EVDbContext _context;

        public ShiftRepository(EVDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Shift?> GetByIdAsync(int shiftId, CancellationToken cancellationToken = default)
        {
            return await _context.Shifts
                .Include(s => s.User)
                .Include(s => s.Center)
                .FirstOrDefaultAsync(s => s.ShiftId == shiftId, cancellationToken);
        }

        public async Task<Shift?> GetByTechnicianAndDateAsync(
            int technicianId,
            DateOnly shiftDate,
            CancellationToken cancellationToken = default)
        {
            return await _context.Shifts
                .Include(s => s.User)
                .Include(s => s.Center)
                .FirstOrDefaultAsync(
                    s => s.UserId == technicianId && s.ShiftDate == shiftDate,
                    cancellationToken);
        }

        public async Task<List<Shift>> GetShiftsByDateRangeAsync(
            int technicianId,
            DateOnly from,
            DateOnly to,
            CancellationToken cancellationToken = default)
        {
            return await _context.Shifts
                .Include(s => s.User)
                .Include(s => s.Center)
                .Where(s => s.UserId == technicianId
                         && s.ShiftDate >= from
                         && s.ShiftDate <= to)
                .OrderByDescending(s => s.ShiftDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<Shift> CreateAsync(Shift shift, CancellationToken cancellationToken = default)
        {
            await _context.Shifts.AddAsync(shift, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            
            // ? Reload entity with navigation properties
            return await GetByIdAsync(shift.ShiftId, cancellationToken) 
                ?? throw new InvalidOperationException("Failed to reload created shift");
        }

        public async Task<Shift> UpdateAsync(Shift shift, CancellationToken cancellationToken = default)
        {
            _context.Shifts.Update(shift);
            await _context.SaveChangesAsync(cancellationToken);
            
            // ? Reload entity with navigation properties
            return await GetByIdAsync(shift.ShiftId, cancellationToken)
                ?? throw new InvalidOperationException("Failed to reload updated shift");
        }

        public async Task<bool> HasActiveShiftAsync(
            int technicianId,
            CancellationToken cancellationToken = default)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            
            return await _context.Shifts
                .AnyAsync(
                    s => s.UserId == technicianId
                      && s.ShiftDate == today
                      && s.CheckInTime != null
                      && s.CheckOutTime == null,
                    cancellationToken);
        }
    }
}
