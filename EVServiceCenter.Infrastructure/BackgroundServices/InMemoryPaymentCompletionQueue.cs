using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using EVServiceCenter.Core.Domains.Payments.DTOs;
using EVServiceCenter.Core.Domains.Payments.Interfaces;

namespace EVServiceCenter.Infrastructure.BackgroundServices
{
    public class InMemoryPaymentCompletionQueue : IPaymentCompletionQueue, IDisposable
    {
        private readonly Channel<PaymentCompletionJob> _channel;
        private bool _disposed;

        public InMemoryPaymentCompletionQueue(int capacity = 1000)
        {
            var options = new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleReader = true,
                SingleWriter = false
            };

            _channel = Channel.CreateBounded<PaymentCompletionJob>(options);
        }

        public void Enqueue(PaymentCompletionJob job)
        {
            if (!_channel.Writer.TryWrite(job))
            {
                // If channel is full, drop the job and optionally log (caller should log)
                // For now throw so caller can handle retry/enqueue fallback
                throw new InvalidOperationException("Payment completion queue is full");
            }
        }

        public async ValueTask<PaymentCompletionJob?> DequeueAsync(CancellationToken cancellationToken)
        {
            try
            {
                var job = await _channel.Reader.ReadAsync(cancellationToken);
                return job;
            }
            catch (OperationCanceledException)
            {
                return null;
            }
            catch (ChannelClosedException)
            {
                return null;
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _channel.Writer.Complete();
            _disposed = true;
        }
    }
}
