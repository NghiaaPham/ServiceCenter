using EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Response;
using EVServiceCenter.Core.Domains.AppointmentManagement.Interfaces.Services;
using EVServiceCenter.Core.Entities;
using EVServiceCenter.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EVServiceCenter.Infrastructure.Domains.AppointmentManagement.Services
{
    /// <summary>
    /// ?? Advanced Appointment Metrics Service
    /// Cung c?p analytics cho business insights
    /// </summary>
    public class AppointmentMetricsService : IAppointmentMetricsService
    {
        private readonly EVDbContext _context;
        private readonly ILogger<AppointmentMetricsService> _logger;

        public AppointmentMetricsService(
            EVDbContext context,
            ILogger<AppointmentMetricsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// ?? Payment Health Metrics
        /// </summary>
        public async Task<PaymentHealthMetricsDto> GetPaymentHealthMetricsAsync(
            DateTime? startDate = null,
            DateTime? endDate = null,
            int? serviceCenterId = null,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting payment health metrics: {StartDate} - {EndDate}, Center: {CenterId}",
                startDate, endDate, serviceCenterId);

            // Default date range: Last 30 days
            startDate ??= DateTime.UtcNow.AddDays(-30);
            endDate ??= DateTime.UtcNow;

            var query = _context.Appointments
                .AsNoTracking()
                .Where(a => a.CreatedDate >= startDate && a.CreatedDate <= endDate);

            if (serviceCenterId.HasValue)
                query = query.Where(a => a.ServiceCenterId == serviceCenterId.Value);

            // ✅ OPTIMIZED: Load with Include to avoid N+1 query in projection
            // Filter appointments with payment (EstimatedCost > 0)
            var appointmentsData = await query
                .Where(a => a.EstimatedCost > 0)
                .Include(a => a.Customer)
                .Include(a => a.PaymentIntents)
                .ToListAsync(cancellationToken);

            // Project in-memory to avoid N+1 queries
            var appointmentsWithPayment = appointmentsData
                .Select(a => new
                {
                    a.AppointmentId,
                    a.AppointmentCode,
                    a.CustomerId,
                    a.EstimatedCost,
                    a.FinalCost,
                    a.PaidAmount,
                    a.PaymentStatus,
                    a.CreatedDate,
                    CustomerName = a.Customer.FullName,
                    // Payment completion time (from completed PaymentIntents)
                    FirstPaymentTime = a.PaymentIntents
                        .Where(pi => pi.Status == PaymentIntentStatusEnum.Completed.ToString())
                        .Min(pi => (DateTime?)pi.UpdatedDate)
                })
                .ToList();

            var totalWithPayment = appointmentsWithPayment.Count;
            var fullyPaid = appointmentsWithPayment
                .Count(a => a.PaymentStatus == PaymentStatusEnum.Completed.ToString());
            var unpaidOrPartial = totalWithPayment - fullyPaid;

            var totalRevenue = appointmentsWithPayment
                .Sum(a => a.PaidAmount ?? 0m);

            var totalOutstanding = appointmentsWithPayment
                .Sum(a => Math.Max((a.FinalCost ?? a.EstimatedCost ?? 0m) - (a.PaidAmount ?? 0m), 0m));

            // Calculate average payment time
            var paymentsWithTime = appointmentsWithPayment
                .Where(a => a.FirstPaymentTime.HasValue)
                .Select(a => (a.FirstPaymentTime!.Value - a.CreatedDate!.Value).TotalHours)
                .ToList();

            var avgPaymentTime = paymentsWithTime.Any() ? paymentsWithTime.Average() : 0;

            // Payment status distribution
            var statusDistribution = appointmentsWithPayment
                .GroupBy(a => a.PaymentStatus)
                .ToDictionary(g => g.Key ?? "Unknown", g => g.Count());

            // Top 5 outstanding appointments
            var topOutstanding = appointmentsWithPayment
                .Select(a => new
                {
                    a.AppointmentId,
                    a.AppointmentCode,
                    a.CustomerName,
                    a.CreatedDate,
                    OutstandingAmount = Math.Max((a.FinalCost ?? a.EstimatedCost ?? 0m) - (a.PaidAmount ?? 0m), 0m)
                })
                .Where(a => a.OutstandingAmount > 0)
                .OrderByDescending(a => a.OutstandingAmount)
                .Take(5)
                .Select(a => new OutstandingAppointmentDto
                {
                    AppointmentId = a.AppointmentId,
                    AppointmentCode = a.AppointmentCode,
                    CustomerName = a.CustomerName,
                    OutstandingAmount = a.OutstandingAmount,
                    CreatedDate = a.CreatedDate!.Value,
                    DaysOverdue = (int)(DateTime.UtcNow - a.CreatedDate!.Value).TotalDays
                })
                .ToList();

            return new PaymentHealthMetricsDto
            {
                TotalAppointmentsWithPayment = totalWithPayment,
                FullyPaidAppointments = fullyPaid,
                UnpaidOrPartialAppointments = unpaidOrPartial,
                PaymentSuccessRate = totalWithPayment > 0 ? (decimal)fullyPaid / totalWithPayment * 100 : 0,
                UnpaidRate = totalWithPayment > 0 ? (decimal)unpaidOrPartial / totalWithPayment * 100 : 0,
                TotalRevenue = totalRevenue,
                TotalOutstanding = totalOutstanding,
                AveragePaymentTimeHours = Math.Round(avgPaymentTime, 2),
                PaymentStatusDistribution = statusDistribution,
                TopOutstandingAppointments = topOutstanding,
                GeneratedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// ?? Subscription Usage Metrics
        /// </summary>
        public async Task<SubscriptionUsageMetricsDto> GetSubscriptionUsageMetricsAsync(
            DateTime? startDate = null,
            DateTime? endDate = null,
            int? serviceCenterId = null,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting subscription usage metrics: {StartDate} - {EndDate}, Center: {CenterId}",
                startDate, endDate, serviceCenterId);

            startDate ??= DateTime.UtcNow.AddDays(-30);
            endDate ??= DateTime.UtcNow;

            var query = _context.Appointments
                .AsNoTracking()
                .Where(a =>
                    a.CreatedDate >= startDate &&
                    a.CreatedDate <= endDate &&
                    a.StatusId == (int)AppointmentStatusEnum.Completed);

            if (serviceCenterId.HasValue)
                query = query.Where(a => a.ServiceCenterId == serviceCenterId.Value);

            var appointments = await query
                .Include(a => a.AppointmentServices)
                    .ThenInclude(aps => aps.Service)
                .Include(a => a.Package)
                .ToListAsync(cancellationToken);

            var totalAppointments = appointments.Count;
            var subscriptionAppointments = appointments.Count(a => a.SubscriptionId.HasValue);
            var extraAppointments = totalAppointments - subscriptionAppointments;

            // Calculate savings
            decimal totalSavings = 0;
            var packageUsage = new Dictionary<int, (string Name, int Count, decimal Savings)>();
            var serviceUsage = new Dictionary<int, (string Name, int Count, decimal Savings)>();

            foreach (var appointment in appointments)
            {
                foreach (var aps in appointment.AppointmentServices)
                {
                    if (aps.ServiceSource == "Subscription")
                    {
                        // Savings = Original price - Subscription price (0)
                        var originalPrice = aps.Service?.BasePrice ?? 0m;
                        var savings = originalPrice - aps.Price;
                        totalSavings += savings;

                        // Track package usage
                        if (appointment.SubscriptionId.HasValue && appointment.PackageId.HasValue)
                        {
                            var packageId = appointment.PackageId.Value;
                            if (!packageUsage.ContainsKey(packageId))
                            {
                                packageUsage[packageId] = (appointment.Package?.PackageName ?? "Unknown", 0, 0m);
                            }
                            var current = packageUsage[packageId];
                            packageUsage[packageId] = (current.Name, current.Count + 1, current.Savings + savings);
                        }

                        // Track service usage
                        if (aps.ServiceId > 0)
                        {
                            var serviceId = aps.ServiceId;
                            if (!serviceUsage.ContainsKey(serviceId))
                            {
                                serviceUsage[serviceId] = (aps.Service?.ServiceName ?? "Unknown", 0, 0m);
                            }
                            var current = serviceUsage[serviceId];
                            serviceUsage[serviceId] = (current.Name, current.Count + 1, current.Savings + savings);
                        }
                    }
                }
            }

            var topPackages = packageUsage
                .OrderByDescending(kv => kv.Value.Count)
                .Take(5)
                .Select(kv => new PackageUsageDto
                {
                    PackageId = kv.Key,
                    PackageName = kv.Value.Name,
                    UsageCount = kv.Value.Count,
                    TotalSavings = kv.Value.Savings
                })
                .ToList();

            var topServices = serviceUsage
                .OrderByDescending(kv => kv.Value.Count)
                .Take(5)
                .Select(kv => new ServiceUsageDto
                {
                    ServiceId = kv.Key,
                    ServiceName = kv.Value.Name,
                    UsageCount = kv.Value.Count,
                    TotalSavings = kv.Value.Savings
                })
                .ToList();

            return new SubscriptionUsageMetricsDto
            {
                TotalAppointments = totalAppointments,
                SubscriptionAppointments = subscriptionAppointments,
                ExtraAppointments = extraAppointments,
                SubscriptionUsageRate = totalAppointments > 0 ? (decimal)subscriptionAppointments / totalAppointments * 100 : 0,
                TotalSavings = totalSavings,
                AverageSavingsPerAppointment = subscriptionAppointments > 0 ? totalSavings / subscriptionAppointments : 0,
                TopPackages = topPackages,
                TopServices = topServices,
                GeneratedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// ?? Degradation Metrics
        /// </summary>
        public async Task<DegradationMetricsDto> GetDegradationMetricsAsync(
            DateTime? startDate = null,
            DateTime? endDate = null,
            int? serviceCenterId = null,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting degradation metrics: {StartDate} - {EndDate}, Center: {CenterId}",
                startDate, endDate, serviceCenterId);

            startDate ??= DateTime.UtcNow.AddDays(-30);
            endDate ??= DateTime.UtcNow;

            // Get audit logs for degradation events
            var degradationLogs = await _context.ServiceSourceAuditLogs
                .AsNoTracking()
                .Include(log => log.AppointmentService)
                    .ThenInclude(aps => aps!.Service)
                .Include(log => log.AppointmentService)
                    .ThenInclude(aps => aps!.Appointment)
                        .ThenInclude(a => a!.ServiceCenter)
                .Where(log =>
                    log.ChangedDate >= startDate &&
                    log.ChangedDate <= endDate &&
                    log.ChangeType == "AUTO_DEGRADE" &&
                    log.NewServiceSource == "Extra")
                .ToListAsync(cancellationToken);

            if (serviceCenterId.HasValue)
            {
                degradationLogs = degradationLogs
                    .Where(log => log.AppointmentService?.Appointment?.ServiceCenterId == serviceCenterId.Value)
                    .ToList();
            }

            // Get total completed services in same period
            var totalCompleted = await _context.AppointmentServices
                .AsNoTracking()
                .Where(aps =>
                    aps.Appointment!.CreatedDate >= startDate &&
                    aps.Appointment.CreatedDate <= endDate &&
                    aps.Appointment.StatusId == (int)AppointmentStatusEnum.Completed)
                .CountAsync(cancellationToken);

            var degradedCount = degradationLogs.Count;
            var revenueImpact = degradationLogs.Sum(log => log.NewPrice - log.OldPrice);

            // Degradation reasons
            var reasons = degradationLogs
                .GroupBy(log => log.ChangeReason ?? "Unknown")
                .ToDictionary(g => g.Key, g => g.Count());

            // Top degraded services
            var topDegraded = degradationLogs
                .Where(log => log.AppointmentService!.ServiceId > 0)
                .GroupBy(log => new { ServiceId = log.AppointmentService!.ServiceId, log.AppointmentService.Service!.ServiceName })
                .Select(g => new DegradedServiceDto
                {
                    ServiceId = g.Key.ServiceId,
                    ServiceName = g.Key.ServiceName,
                    DegradationCount = g.Count(),
                    TotalRevenueImpact = g.Sum(log => log.NewPrice - log.OldPrice)
                })
                .OrderByDescending(s => s.DegradationCount)
                .Take(5)
                .ToList();

            // Degradation by center
            var byCenter = degradationLogs
                .GroupBy(log => new
                {
                    log.AppointmentService!.Appointment!.ServiceCenterId,
                    log.AppointmentService.Appointment.ServiceCenter!.CenterName
                })
                .Select(g => new CenterDegradationDto
                {
                    ServiceCenterId = g.Key.ServiceCenterId,
                    CenterName = g.Key.CenterName,
                    DegradationCount = g.Count(),
                    DegradationRate = 0 // Will calculate below
                })
                .ToList();

            // ✅ OPTIMIZED: Batch query for all centers instead of N+1 queries
            // Get total services count for each center in a single query
            var centerServiceCounts = await _context.AppointmentServices
                .AsNoTracking()
                .Where(aps =>
                    aps.Appointment!.CreatedDate >= startDate &&
                    aps.Appointment.CreatedDate <= endDate &&
                    aps.Appointment.StatusId == (int)AppointmentStatusEnum.Completed)
                .GroupBy(aps => aps.Appointment!.ServiceCenterId)
                .Select(g => new
                {
                    ServiceCenterId = g.Key,
                    TotalCount = g.Count()
                })
                .ToListAsync(cancellationToken);

            // Calculate degradation rate for each center
            foreach (var center in byCenter)
            {
                var centerStats = centerServiceCounts.FirstOrDefault(c => c.ServiceCenterId == center.ServiceCenterId);
                var centerTotalServices = centerStats?.TotalCount ?? 0;
                center.DegradationRate = centerTotalServices > 0
                    ? (decimal)center.DegradationCount / centerTotalServices * 100
                    : 0;
            }

            return new DegradationMetricsDto
            {
                TotalCompletedServices = totalCompleted,
                DegradedServices = degradedCount,
                DegradationRate = totalCompleted > 0 ? (decimal)degradedCount / totalCompleted * 100 : 0,
                RevenueImpact = revenueImpact,
                DegradationReasons = reasons,
                TopDegradedServices = topDegraded,
                DegradationByCenter = byCenter.OrderByDescending(c => c.DegradationCount).ToList(),
                GeneratedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// ?? Cancellation Metrics
        /// </summary>
        public async Task<CancellationMetricsDto> GetCancellationMetricsAsync(
            DateTime? startDate = null,
            DateTime? endDate = null,
            int? serviceCenterId = null,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting cancellation metrics: {StartDate} - {EndDate}, Center: {CenterId}",
                startDate, endDate, serviceCenterId);

            startDate ??= DateTime.UtcNow.AddDays(-30);
            endDate ??= DateTime.UtcNow;

            var query = _context.Appointments
                .AsNoTracking()
                .Where(a => a.CreatedDate >= startDate && a.CreatedDate <= endDate);

            if (serviceCenterId.HasValue)
                query = query.Where(a => a.ServiceCenterId == serviceCenterId.Value);

            var appointments = await query
                .Include(a => a.ServiceCenter)
                .Include(a => a.Slot)
                .ToListAsync(cancellationToken);

            var totalAppointments = appointments.Count;
            var cancelled = appointments.Where(a => a.StatusId == (int)AppointmentStatusEnum.Cancelled).ToList();
            var noShows = appointments.Where(a => a.StatusId == (int)AppointmentStatusEnum.NoShow).ToList();

            var cancelledCount = cancelled.Count;
            var noShowCount = noShows.Count;

            // Total refunded (from Refunds table)
            var cancelledIds = cancelled.Select(a => a.AppointmentId).ToList();
            var totalRefunded = await _context.Refunds
                .Where(r => cancelledIds.Contains(r.AppointmentId))
                .SumAsync(r => r.RefundAmount, cancellationToken);

            // Average notice time
            var noticeHours = cancelled
                .Where(a => a.Slot != null)
                .Select(a =>
                {
                    var appointmentDateTime = a.Slot!.SlotDate.ToDateTime(a.Slot.StartTime);
                    var cancelledAt = a.UpdatedDate ?? DateTime.UtcNow;
                    return (appointmentDateTime - cancelledAt).TotalHours;
                })
                .Where(h => h > 0)
                .ToList();

            var avgNoticeTime = noticeHours.Any() ? noticeHours.Average() : 0;

            // Notice time distribution
            var moreThan24h = noticeHours.Count(h => h >= 24);
            var between2And24h = noticeHours.Count(h => h >= 2 && h < 24);
            var lessThan2h = noticeHours.Count(h => h < 2);

            // Top cancellation reasons
            var topReasons = cancelled
                .Where(a => !string.IsNullOrWhiteSpace(a.CancellationReason))
                .GroupBy(a => a.CancellationReason!)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .ToDictionary(g => g.Key, g => g.Count());

            // Cancellation by center
            var byCenter = appointments
                .GroupBy(a => new { a.ServiceCenterId, a.ServiceCenter!.CenterName })
                .Select(g => new
                {
                    g.Key.ServiceCenterId,
                    g.Key.CenterName,
                    Total = g.Count(),
                    Cancelled = g.Count(a => a.StatusId == (int)AppointmentStatusEnum.Cancelled)
                })
                .Select(x => new CenterCancellationDto
                {
                    ServiceCenterId = x.ServiceCenterId,
                    CenterName = x.CenterName,
                    CancellationCount = x.Cancelled,
                    CancellationRate = x.Total > 0 ? (decimal)x.Cancelled / x.Total * 100 : 0
                })
                .OrderByDescending(c => c.CancellationCount)
                .ToList();

            return new CancellationMetricsDto
            {
                TotalAppointments = totalAppointments,
                CancelledAppointments = cancelledCount,
                CancellationRate = totalAppointments > 0 ? (decimal)cancelledCount / totalAppointments * 100 : 0,
                NoShowAppointments = noShowCount,
                NoShowRate = totalAppointments > 0 ? (decimal)noShowCount / totalAppointments * 100 : 0,
                TotalRefunded = totalRefunded,
                AverageNoticeTimeHours = Math.Round(avgNoticeTime, 2),
                NoticeTimeDistribution = new NoticeTimeDistributionDto
                {
                    MoreThan24Hours = moreThan24h,
                    Between2And24Hours = between2And24h,
                    LessThan2Hours = lessThan2h
                },
                TopCancellationReasons = topReasons,
                CancellationByCenter = byCenter,
                GeneratedAt = DateTime.UtcNow
            };
        }
    }
}
