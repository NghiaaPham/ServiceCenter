using EVServiceCenter.Core.Entities;
using EVServiceCenter.Core.Interfaces.Services;
using EVServiceCenter.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EVServiceCenter.Infrastructure.Services
{
    /// <summary>
    /// Service implementation cho audit trail của ServiceSource changes
    /// </summary>
    public class ServiceSourceAuditService : IServiceSourceAuditService
    {
        private readonly EVDbContext _context;
        private readonly ILogger<ServiceSourceAuditService> _logger;

        public ServiceSourceAuditService(
            EVDbContext context,
            ILogger<ServiceSourceAuditService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Log một thay đổi ServiceSource của AppointmentService
        /// </summary>
        public async Task<ServiceSourceAuditLog> LogServiceSourceChangeAsync(
            int appointmentServiceId,
            string? oldServiceSource,
            string newServiceSource,
            decimal oldPrice,
            decimal newPrice,
            string changeReason,
            string changeType,
            int changedBy,
            string? ipAddress = null,
            string? userAgent = null,
            decimal? refundAmount = null,
            bool usageDeducted = false)
        {
            try
            {
                // Lấy thông tin AppointmentService để có AppointmentId, ServiceId
                var appointmentService = await _context.AppointmentServices
                    .AsNoTracking()
                    .FirstOrDefaultAsync(aps => aps.AppointmentServiceId == appointmentServiceId);

                if (appointmentService == null)
                {
                    throw new InvalidOperationException(
                        $"AppointmentService với ID {appointmentServiceId} không tồn tại");
                }

                // Tạo audit log record
                var auditLog = new ServiceSourceAuditLog
                {
                    AppointmentServiceId = appointmentServiceId,
                    AppointmentId = appointmentService.AppointmentId,
                    ServiceId = appointmentService.ServiceId,
                    OldServiceSource = oldServiceSource,
                    NewServiceSource = newServiceSource,
                    OldPrice = oldPrice,
                    NewPrice = newPrice,
                    // PriceDifference sẽ được compute bởi database (NewPrice - OldPrice)
                    ChangeReason = changeReason,
                    ChangeType = changeType,
                    ChangedBy = changedBy,
                    ChangedDate = DateTime.UtcNow,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    RefundAmount = refundAmount,
                    RefundIssued = refundAmount.HasValue && refundAmount.Value > 0,
                    UsageDeducted = usageDeducted
                };

                _context.ServiceSourceAuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Audit log created: AppointmentServiceId={AppointmentServiceId}, " +
                    "Change={OldSource}→{NewSource}, Type={ChangeType}, By={ChangedBy}",
                    appointmentServiceId, oldServiceSource, newServiceSource, changeType, changedBy);

                return auditLog;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Lỗi khi tạo audit log cho AppointmentServiceId={AppointmentServiceId}",
                    appointmentServiceId);
                throw;
            }
        }

        /// <summary>
        /// Lấy danh sách audit logs cho một Appointment
        /// Trả về dạng anonymous object để dễ serialize
        /// </summary>
        public async Task<List<object>> GetAuditLogsForAppointmentAsync(int appointmentId)
        {
            try
            {
                var logs = await _context.ServiceSourceAuditLogs
                    .Include(log => log.AppointmentService)
                        .ThenInclude(aps => aps.Service)
                    .Include(log => log.ChangedByNavigation)
                    .Where(log => log.AppointmentId == appointmentId)
                    .OrderBy(log => log.ChangedDate)
                    .Select(log => new
                    {
                        log.AuditId,
                        log.AppointmentServiceId,
                        ServiceName = log.Service != null ? log.Service.ServiceName : "N/A",
                        log.OldServiceSource,
                        log.NewServiceSource,
                        log.OldPrice,
                        log.NewPrice,
                        log.PriceDifference,
                        log.ChangeReason,
                        log.ChangeType,
                        ChangedByName = log.ChangedByNavigation != null
                            ? log.ChangedByNavigation.FullName
                            : "System",
                        log.ChangedDate,
                        log.RefundAmount,
                        log.RefundIssued,
                        log.UsageDeducted,
                        log.IpAddress
                    })
                    .ToListAsync();

                return logs.Cast<object>().ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Lỗi khi lấy audit logs cho AppointmentId={AppointmentId}",
                    appointmentId);
                throw;
            }
        }

        /// <summary>
        /// Lấy audit logs cho một AppointmentService cụ thể
        /// </summary>
        public async Task<List<ServiceSourceAuditLog>> GetAuditLogsForServiceAsync(
            int appointmentServiceId)
        {
            try
            {
                var logs = await _context.ServiceSourceAuditLogs
                    .Include(log => log.ChangedByNavigation)
                    .Include(log => log.Service)
                    .Where(log => log.AppointmentServiceId == appointmentServiceId)
                    .OrderBy(log => log.ChangedDate)
                    .ToListAsync();

                return logs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Lỗi khi lấy audit logs cho AppointmentServiceId={AppointmentServiceId}",
                    appointmentServiceId);
                throw;
            }
        }
    }
}
