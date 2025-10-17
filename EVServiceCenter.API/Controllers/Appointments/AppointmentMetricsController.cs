using EVServiceCenter.Core.Domains.AppointmentManagement.Interfaces.Services;
using EVServiceCenter.Core.Domains.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVServiceCenter.API.Controllers.Appointments
{
    /// <summary>
    /// ?? Appointment Metrics API - Advanced Analytics
    /// </summary>
    [ApiController]
    [Route("api/appointment-management/metrics")]
    [Authorize(Policy = "AdminOrStaff")]
    [ApiExplorerSettings(GroupName = "Staff - Appointment Metrics")]
    public class AppointmentMetricsController : BaseController
    {
        private readonly IAppointmentMetricsService _metricsService;
        private readonly ILogger<AppointmentMetricsController> _logger;

        public AppointmentMetricsController(
            IAppointmentMetricsService metricsService,
            ILogger<AppointmentMetricsController> logger)
        {
            _metricsService = metricsService;
            _logger = logger;
        }

        /// <summary>
        /// ?? Payment Health Metrics
        /// </summary>
        /// <remarks>
        /// Ph�n t�ch s?c kh?e thanh to�n:
        /// - T? l? thanh to�n th�nh c�ng / ch?a thanh to�n
        /// - T?ng revenue / outstanding amount
        /// - Th?i gian thanh to�n trung b�nh
        /// - Top appointments c� outstanding cao
        ///
        /// **Use cases:**
        /// - Dashboard t�i ch�nh
        /// - Theo d�i c�ng n?
        /// - Ph�t hi?n appointments c?n thu n?
        /// - ?�nh gi� cash flow
        ///
        /// **Default:** Last 30 days, all centers
        /// </remarks>
        /// <param name="startDate">Ng�y b?t ??u (yyyy-MM-dd)</param>
        /// <param name="endDate">Ng�y k?t th�c (yyyy-MM-dd)</param>
        /// <param name="serviceCenterId">Filter theo service center</param>
        [HttpGet("payment-health")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPaymentHealthMetrics(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int? serviceCenterId = null)
        {
            try
            {
                var metrics = await _metricsService.GetPaymentHealthMetricsAsync(
                    startDate, endDate, serviceCenterId);

                _logger.LogInformation(
                    "Staff {UserId} retrieved payment health metrics: {StartDate} - {EndDate}",
                    GetCurrentUserId(), startDate, endDate);

                return Success(metrics, "L?y payment health metrics th�nh c�ng");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment health metrics");
                return ServerError("C� l?i x?y ra khi l?y payment health metrics");
            }
        }

        /// <summary>
        /// ?? Subscription Usage Metrics
        /// </summary>
        /// <remarks>
        /// Ph�n t�ch s? d?ng subscription:
        /// - T? l? appointments d�ng subscription
        /// - T?ng ti?n ti?t ki?m nh? subscription
        /// - Top packages & services ???c s? d?ng
        ///
        /// **Use cases:**
        /// - ?�nh gi� hi?u qu? subscription program
        /// - Ph�n t�ch ROI c?a packages
        /// - Marketing insights (packages n�o hot)
        /// - Customer value analysis
        ///
        /// **Default:** Last 30 days, all centers
        /// </remarks>
        /// <param name="startDate">Ng�y b?t ??u (yyyy-MM-dd)</param>
        /// <param name="endDate">Ng�y k?t th�c (yyyy-MM-dd)</param>
        /// <param name="serviceCenterId">Filter theo service center</param>
        [HttpGet("subscription-usage")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSubscriptionUsageMetrics(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int? serviceCenterId = null)
        {
            try
            {
                var metrics = await _metricsService.GetSubscriptionUsageMetricsAsync(
                    startDate, endDate, serviceCenterId);

                _logger.LogInformation(
                    "Staff {UserId} retrieved subscription usage metrics: {StartDate} - {EndDate}",
                    GetCurrentUserId(), startDate, endDate);

                return Success(metrics, "L?y subscription usage metrics th�nh c�ng");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subscription usage metrics");
                return ServerError("C� l?i x?y ra khi l?y subscription usage metrics");
            }
        }

        /// <summary>
        /// ?? Degradation Metrics
        /// </summary>
        /// <remarks>
        /// Ph�n t�ch service degradation (Subscription ? Extra):
        /// - T? l? degradation
        /// - Revenue impact t? degradation
        /// - L� do degradation ph? bi?n
        /// - Top services b? degraded
        ///
        /// **Use cases:**
        /// - Monitoring subscription capacity
        /// - Ph�t hi?n race conditions
        /// - Capacity planning
        /// - Customer experience improvement
        ///
        /// **Default:** Last 30 days, all centers
        /// </remarks>
        /// <param name="startDate">Ng�y b?t ??u (yyyy-MM-dd)</param>
        /// <param name="endDate">Ng�y k?t th�c (yyyy-MM-dd)</param>
        /// <param name="serviceCenterId">Filter theo service center</param>
        [HttpGet("degradation")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDegradationMetrics(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int? serviceCenterId = null)
        {
            try
            {
                var metrics = await _metricsService.GetDegradationMetricsAsync(
                    startDate, endDate, serviceCenterId);

                _logger.LogInformation(
                    "Staff {UserId} retrieved degradation metrics: {StartDate} - {EndDate}",
                    GetCurrentUserId(), startDate, endDate);

                return Success(metrics, "L?y degradation metrics th�nh c�ng");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting degradation metrics");
                return ServerError("C� l?i x?y ra khi l?y degradation metrics");
            }
        }

        /// <summary>
        /// ?? Cancellation Metrics
        /// </summary>
        /// <remarks>
        /// Ph�n t�ch h?y l?ch & no-show:
        /// - T? l? cancel & no-show
        /// - T?ng ti?n refunded
        /// - Th?i gian th�ng b�o h?y trung b�nh
        /// - L� do h?y ph? bi?n
        ///
        /// **Use cases:**
        /// - Gi?m t? l? cancel/no-show
        /// - T?i ?u refund policy
        /// - Customer retention analysis
        /// - Capacity planning
        ///
        /// **Default:** Last 30 days, all centers
        /// </remarks>
        /// <param name="startDate">Ng�y b?t ??u (yyyy-MM-dd)</param>
        /// <param name="endDate">Ng�y k?t th�c (yyyy-MM-dd)</param>
        /// <param name="serviceCenterId">Filter theo service center</param>
        [HttpGet("cancellation")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCancellationMetrics(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int? serviceCenterId = null)
        {
            try
            {
                var metrics = await _metricsService.GetCancellationMetricsAsync(
                    startDate, endDate, serviceCenterId);

                _logger.LogInformation(
                    "Staff {UserId} retrieved cancellation metrics: {StartDate} - {EndDate}",
                    GetCurrentUserId(), startDate, endDate);

                return Success(metrics, "L?y cancellation metrics th�nh c�ng");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cancellation metrics");
                return ServerError("C� l?i x?y ra khi l?y cancellation metrics");
            }
        }
    }
}
