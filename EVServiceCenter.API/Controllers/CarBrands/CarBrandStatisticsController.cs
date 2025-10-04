using EVServiceCenter.Core.Domains.CarBrands.Interfaces.Services;
using EVServiceCenter.Core.Domains.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVServiceCenter.API.Controllers.CarBrands
{
    [ApiController]
    [Route("api/car-brands")]
    [Authorize(Policy = "AdminOrStaff")]
    [ApiExplorerSettings(GroupName = "Staff - Car Brands")]
    public class CarBrandStatisticsController : BaseController
    {
        private readonly ICarBrandStatisticsService _statsService;
        private readonly ILogger<CarBrandStatisticsController> _logger;

        public CarBrandStatisticsController(
            ICarBrandStatisticsService statsService,
            ILogger<CarBrandStatisticsController> logger)
        {
            _statsService = statsService;
            _logger = logger;
        }

        /// <summary>
        /// Get statistics for specific brand
        /// </summary>
        [HttpGet("{id:int}/statistics")]
        public async Task<IActionResult> GetBrandStats(int id, CancellationToken ct)
        {
            try
            {
                var result = await _statsService.GetBrandStatisticsAsync(id, ct);
                return Ok(ApiResponse<object>.WithSuccess(result, "Lấy thống kê thành công"));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ApiResponse<object>.WithNotFound(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stats for brand {Id}", id);
                return StatusCode(500, ApiResponse<object>.WithError("Có lỗi xảy ra", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Get overall brand statistics
        /// </summary>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetAllStats(CancellationToken ct)
        {
            try
            {
                var result = await _statsService.GetAllBrandsStatisticsAsync(ct);
                return Ok(ApiResponse<object>.WithSuccess(result, "Lấy thống kê tổng quan thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all brand stats");
                return StatusCode(500, ApiResponse<object>.WithError("Có lỗi xảy ra", "INTERNAL_ERROR", 500));
            }
        }
    }
}