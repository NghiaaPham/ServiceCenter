using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EVServiceCenter.Core.Domains.Payments.Entities;
using EVServiceCenter.Core.Domains.Payments.Interfaces.Repositories;
using EVServiceCenter.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace EVServiceCenter.Infrastructure.Domains.Payments.Repositories
{
  public class PaymentIntentRepository : IPaymentIntentRepository
  {
    private readonly EVDbContext _context;

    public PaymentIntentRepository(EVDbContext context)
    {
      _context = context;
    }

    public async Task AddAsync(
        PaymentIntent intent,
        CancellationToken cancellationToken = default)
    {
      await _context.PaymentIntents.AddAsync(intent, cancellationToken);
      await _context.SaveChangesAsync(cancellationToken);
    }

    public Task<PaymentIntent?> GetByIdAsync(
        int intentId,
        CancellationToken cancellationToken = default)
    {
      return _context.PaymentIntents
          .Include(pi => pi.PaymentTransactions)
          .FirstOrDefaultAsync(pi => pi.PaymentIntentId == intentId, cancellationToken);
    }

    public Task<PaymentIntent?> GetByCodeAsync(
        string intentCode,
        CancellationToken cancellationToken = default)
    {
      return _context.PaymentIntents
          .Include(pi => pi.PaymentTransactions)
          .FirstOrDefaultAsync(pi => pi.IntentCode == intentCode, cancellationToken);
    }

    public Task<PaymentIntent?> GetLatestByAppointmentAsync(
        int appointmentId,
        CancellationToken cancellationToken = default)
    {
      return _context.PaymentIntents
          .Where(pi => pi.AppointmentId == appointmentId)
          .OrderByDescending(pi => pi.PaymentIntentId)
          .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<List<PaymentIntent>> GetByAppointmentAsync(
        int appointmentId,
        CancellationToken cancellationToken = default)
    {
      return _context.PaymentIntents
          .Where(pi => pi.AppointmentId == appointmentId)
          .OrderByDescending(pi => pi.PaymentIntentId)
          .ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        PaymentIntent intent,
        CancellationToken cancellationToken = default)
    {
      _context.PaymentIntents.Update(intent);
      await _context.SaveChangesAsync(cancellationToken);
    }
  }
}
