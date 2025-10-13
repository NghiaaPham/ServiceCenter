using EVServiceCenter.Core.Entities;
using EVServiceCenter.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace EVServiceCenter.API.Services
{
    /// <summary>
    /// Stub implementation for IServiceSourceAuditService
    /// Used as fallback when full implementation cannot be loaded
    /// </summary>
    public class StubServiceSourceAuditService : IServiceSourceAuditService
    {
        private readonly EVDbContext _context;
        private readonly ILogger _logger;

        public StubServiceSourceAuditService(EVDbContext context, ILogger logger)
        {
            _context = context;
            _logger = logger;
        }

        public Task<ServiceSourceAuditLog> LogServiceSourceChangeAsync(
            int appointmentServiceId, string? oldServiceSource, string newServiceSource,
            decimal oldPrice, decimal newPrice, string changeReason, string changeType,
            int changedBy, string? ipAddress = null, string? userAgent = null,
            decimal? refundAmount = null, bool usageDeducted = false)
        {
            _logger.LogInformation("Audit log stub: {Reason}", changeReason);
            return Task.FromResult(new ServiceSourceAuditLog());
        }

        public Task<List<object>> GetAuditLogsForAppointmentAsync(int appointmentId)
        {
            return Task.FromResult(new List<object>());
        }

        public Task<List<ServiceSourceAuditLog>> GetAuditLogsForServiceAsync(int appointmentServiceId)
        {
            return Task.FromResult(new List<ServiceSourceAuditLog>());
        }
    }
}
