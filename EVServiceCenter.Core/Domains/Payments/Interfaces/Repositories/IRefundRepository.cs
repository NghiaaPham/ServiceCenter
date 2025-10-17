using EVServiceCenter.Core.Domains.Payments.Entities;

namespace EVServiceCenter.Core.Domains.Payments.Interfaces.Repositories
{
    public interface IRefundRepository
    {
        Task<Refund?> GetByIdAsync(int refundId, CancellationToken cancellationToken = default);

        Task<List<Refund>> GetByAppointmentIdAsync(int appointmentId, CancellationToken cancellationToken = default);

        Task<List<Refund>> GetByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default);

        Task<List<Refund>> GetPendingRefundsAsync(CancellationToken cancellationToken = default);

        Task<Refund> AddAsync(Refund refund, CancellationToken cancellationToken = default);

        Task<Refund> UpdateAsync(Refund refund, CancellationToken cancellationToken = default);

        Task<bool> DeleteAsync(int refundId, CancellationToken cancellationToken = default);
    }
}
