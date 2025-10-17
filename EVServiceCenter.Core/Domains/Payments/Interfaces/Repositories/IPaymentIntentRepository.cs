using System.Threading;
using System.Threading.Tasks;
using EVServiceCenter.Core.Domains.Payments.Entities;

namespace EVServiceCenter.Core.Domains.Payments.Interfaces.Repositories
{
  public interface IPaymentIntentRepository
  {
    Task AddAsync(PaymentIntent intent, CancellationToken cancellationToken = default);

    Task<PaymentIntent?> GetByIdAsync(
        int intentId,
        CancellationToken cancellationToken = default);

    Task<PaymentIntent?> GetByCodeAsync(
        string intentCode,
        CancellationToken cancellationToken = default);

    Task<PaymentIntent?> GetLatestByAppointmentAsync(
        int appointmentId,
        CancellationToken cancellationToken = default);

    Task<List<PaymentIntent>> GetByAppointmentAsync(
        int appointmentId,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        PaymentIntent intent,
        CancellationToken cancellationToken = default);
  }
}
