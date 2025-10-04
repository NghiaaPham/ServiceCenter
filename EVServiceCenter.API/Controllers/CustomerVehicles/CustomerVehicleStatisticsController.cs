using EVServiceCenter.Core.Domains.CustomerVehicles.Interfaces.Services;
using EVServiceCenter.Core.Domains.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVServiceCenter.API.Controllers.CustomerVehicles
{
    [ApiController]
    [Route("api/customer-vehicles")]
    [Authorize(Policy = "AdminOrStaff")]
    [ApiExplorerSettings(GroupName = "Staff - Vehicles")]
    public class CustomerVehicleStatisticsController : BaseController
    {
        private readonly ICustomerVehicleStatisticsService _statsService;
        private readonly ILogger<CustomerVehicleStatisticsController> _logger;

        public CustomerVehicleStatisticsController(
            ICustomerVehicleStatisticsService statsService,
            ILogger<CustomerVehicleStatisticsController> logger)
        {
            _statsService = statsService;
            _logger = logger;
        }

        /// <summary>
        /// Get statistics for specific vehicle
        /// </summary>
        [HttpGet("{id:int}/statistics")]
        public async Task<IActionResult> GetVehicleStats(int id, CancellationToken ct)
        {
            try
            {
                var result = await _statsService.GetVehicleStatisticsAsync(id, ct);
                return Ok(ApiResponse<object>.WithSuccess(result, "Lấy thống kê thành công"));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ApiResponse<object>.WithNotFound(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stats for vehicle {Id}", id);
                return StatusCode(500, ApiResponse<object>.WithError("Có lỗi xảy ra", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Get overall vehicle statistics
        /// </summary>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetAllStats(CancellationToken ct)
        {
            try
            {
                var result = await _statsService.GetAllVehiclesStatisticsAsync(ct);
                return Ok(ApiResponse<object>.WithSuccess(result, "Lấy thống kê tổng quan thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all vehicle stats");
                return StatusCode(500, ApiResponse<object>.WithError("Có lỗi xảy ra", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Get statistics for all vehicles of a customer
        /// </summary>
        [HttpGet("by-customer/{customerId:int}/statistics")]
        public async Task<IActionResult> GetCustomerVehiclesStats(int customerId, CancellationToken ct)
        {
            try
            {
                var result = await _statsService.GetCustomerVehiclesStatisticsAsync(customerId, ct);
                return Ok(ApiResponse<object>.WithSuccess(result, "Lấy thống kê thành công"));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ApiResponse<object>.WithNotFound(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer vehicles stats {CustomerId}", customerId);
                return StatusCode(500, ApiResponse<object>.WithError("Có lỗi xảy ra", "INTERNAL_ERROR", 500));
            }
        }
    }
}