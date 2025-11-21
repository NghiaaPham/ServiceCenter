using System.Threading;
using System.Threading.Tasks;
using EVServiceCenter.Core.Domains.Payments.DTOs;

namespace EVServiceCenter.Core.Domains.Payments.Interfaces
{
    public interface IPaymentCompletionQueue
    {
        void Enqueue(PaymentCompletionJob job);
        ValueTask<PaymentCompletionJob?> DequeueAsync(CancellationToken cancellationToken);
    }
}
                                               