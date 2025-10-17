using EVServiceCenter.Core.Domains.Payments.Entities;
using EVServiceCenter.Core.Domains.Payments.Interfaces.Repositories;
using EVServiceCenter.Core.Entities;
using EVServiceCenter.Core.Constants;
using Microsoft.EntityFrameworkCore;

namespace EVServiceCenter.Infrastructure.Domains.Payments.Repositories
{
    public class RefundRepository : IRefundRepository
    {
        private readonly EVDbContext _context;

        public RefundRepository(EVDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Refund?> GetByIdAsync(int refundId, CancellationToken cancellationToken = default)
        {
            return await _context.Refunds
                .Include(r => r.PaymentIntent)
                .Include(r => r.Appointment)
                .FirstOrDefaultAsync(r => r.RefundId == refundId, cancellationToken);
        }

        public async Task<List<Refund>> GetByAppointmentIdAsync(
            int appointmentId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Refunds
                .Include(r => r.PaymentIntent)
                .Where(r => r.AppointmentId == appointmentId)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Refund>> GetByCustomerIdAsync(
            int customerId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Refunds
                .Include(r => r.PaymentIntent)
                .Include(r => r.Appointment)
                .Where(r => r.CustomerId == customerId)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Refund>> GetPendingRefundsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Refunds
                .Include(r => r.PaymentIntent)
                .Include(r => r.Appointment)
                .Where(r => r.Status == RefundConstants.Status.Pending
                         || r.Status == RefundConstants.Status.Processing)
                .OrderBy(r => r.CreatedDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<Refund> AddAsync(Refund refund, CancellationToken cancellationToken = default)
        {
            await _context.Refunds.AddAsync(refund, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return refund;
        }

        public async Task<Refund> UpdateAsync(Refund refund, CancellationToken cancellationToken = default)
        {
            _context.Refunds.Update(refund);
            await _context.SaveChangesAsync(cancellationToken);
            return refund;
        }

        public async Task<bool> DeleteAsync(int refundId, CancellationToken cancellationToken = default)
        {
            var refund = await _context.Refunds.FindAsync(new object[] { refundId }, cancellationToken);
            if (refund == null)
                return false;

            _context.Refunds.Remove(refund);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
