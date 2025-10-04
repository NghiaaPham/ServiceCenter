using EVServiceCenter.Core.Domains.CarModels.Interfaces.Services;
using EVServiceCenter.Core.Domains.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVServiceCenter.API.Controllers.CarModels
{
    [ApiController]
    [Route("api/car-models")]
    [Authorize(Policy = "AdminOrStaff")]
    [ApiExplorerSettings(GroupName = "Staff - Car Models")]
    public class CarModelStatisticsController : BaseController
    {
        private readonly ICarModelStatisticsService _statsService;
        private readonly ILogger<CarModelStatisticsController> _logger;

        public CarModelStatisticsController(
            ICarModelStatisticsService statsService,
            ILogger<CarModelStatisticsController> logger)
        {
            _statsService = statsService;
            _logger = logger;
        }

        /// <summary>
        /// Get statistics for specific model
        /// </summary>
        [HttpGet("{id:int}/statistics")]
        public async Task<IActionResult> GetModelStats(int id, CancellationToken ct)
        {
            try
            {
                var result = await _statsService.GetModelStatisticsAsync(id, ct);
                return Ok(ApiResponse<object>.WithSuccess(result, "Lấy thống kê thành công"));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ApiResponse<object>.WithNotFound(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stats for model {Id}", id);
                return StatusCode(500, ApiResponse<object>.WithError("Có lỗi xảy ra", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Get overall model statistics
        /// </summary>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetAllStats(CancellationToken ct)
        {
            try
            {
                var result = await _statsService.GetAllModelsStatisticsAsync(ct);
                return Ok(ApiResponse<object>.WithSuccess(result, "Lấy thống kê tổng quan thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all model stats");
                return StatusCode(500, ApiResponse<object>.WithError("Có lỗi xảy ra", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Get statistics for all models of a brand
        /// </summary>
        [HttpGet("by-brand/{brandId:int}/statistics")]
        public async Task<IActionResult> GetBrandModelsStats(int brandId, CancellationToken ct)
        {
            try
            {
                var result = await _statsService.GetBrandModelsStatisticsAsync(brandId, ct);
                return Ok(ApiResponse<object>.WithSuccess(result, "Lấy thống kê thành công"));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ApiResponse<object>.WithNotFound(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting brand models stats {BrandId}", brandId);
                return StatusCode(500, ApiResponse<object>.WithError("Có lỗi xảy ra", "INTERNAL_ERROR", 500));
            }
        }
    }
}