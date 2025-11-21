using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using EVServiceCenter.Core.Domains.Payments.Interfaces;
using EVServiceCenter.Core.Domains.Payments.DTOs;
using EVServiceCenter.Core.Domains.WorkOrders.Interfaces;

namespace EVServiceCenter.Infrastructure.BackgroundServices
{
    public class PaymentCompletionWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PaymentCompletionWorker> _logger;
        private readonly IPaymentCompletionQueue _queue;
        private readonly int _systemUserId;

        public PaymentCompletionWorker(IServiceProvider serviceProvider, ILogger<PaymentCompletionWorker> logger, IPaymentCompletionQueue queue)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _queue = queue;
            _systemUserId = 0; // default system user, can be configured
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("PaymentCompletionWorker started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var job = await _queue.DequeueAsync(stoppingToken);
                    if (job == null)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                        continue;
                    }

                    using var scope = _serviceProvider.CreateScope();
                    var workOrderService = scope.ServiceProvider.GetRequiredService<IWorkOrderService>();

                    try
                    {
                        _logger.LogInformation("Processing payment completion job: Appointment={AppointmentId}, Invoice={InvoiceId}, Intent={IntentId}", job.AppointmentId, job.InvoiceId, job.PaymentIntentId);

                        await workOrderService.HandleAppointmentPaymentCompletedAsync(job.AppointmentId, job.ProcessedBy != 0 ? job.ProcessedBy : _systemUserId, stoppingToken);

                        _logger.LogInformation("Finished processing payment completion job for Appointment {AppointmentId}", job.AppointmentId);
                    }
                    catch (Exception ex)
                    {
                        job.RetryCount++;
                        _logger.LogError(ex, "Error handling payment completion job for Appointment {AppointmentId}. Retry {Retry}", job.AppointmentId, job.RetryCount);

                        if (job.RetryCount < 5)
                        {
                            try
                            {
                                _queue.Enqueue(job);
                            }
                            catch (Exception enqueueEx)
                            {
                                _logger.LogError(enqueueEx, "Failed to re-enqueue payment completion job for Appointment {AppointmentId}", job.AppointmentId);
                            }

                            await Task.Delay(TimeSpan.FromSeconds(2 * job.RetryCount), stoppingToken);
                        }
                        else
                        {
                            _logger.LogError("Dropping job for Appointment {AppointmentId} after {Retry} retries", job.AppointmentId, job.RetryCount);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // shutdown
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error in PaymentCompletionWorker loop");
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }

            _logger.LogInformation("PaymentCompletionWorker stopping");
        }
    }
}
