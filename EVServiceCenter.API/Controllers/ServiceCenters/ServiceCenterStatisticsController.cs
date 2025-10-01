using EVServiceCenter.Core.Domains.ServiceCenters.Interfaces.Services;
using EVServiceCenter.Core.Domains.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVServiceCenter.API.Controllers.ServiceCenters
{
    [ApiController]
    [Route("api/service-centers")]
    [Authorize(Policy = "AdminOrStaff")]
    public class ServiceCenterStatisticsController : BaseController
    {
        private readonly IServiceCenterStatisticsService _statsService;

        public ServiceCenterStatisticsController(IServiceCenterStatisticsService statsService)
        {
            _statsService = statsService;
        }

        /// <summary>
        /// Get statistics for a specific center
        /// </summary>
        [HttpGet("{id:int}/statistics")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCenterStats(int id, CancellationToken ct)
        {
            var result = await _statsService.GetCenterStatisticsAsync(id, ct);

            if (result == null)
                return NotFound(ApiResponse<object>.WithNotFound($"Không tìm thấy thống kê cho trung tâm {id}"));

            return Ok(ApiResponse<object>.WithSuccess(result, $"Thống kê cho trung tâm {id}"));
        }

        /// <summary>
        /// Get overall statistics across all centers
        /// </summary>
        [HttpGet("statistics")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllStats(CancellationToken ct)
        {
            var result = await _statsService.GetAllCentersStatisticsAsync(ct);

            return Ok(ApiResponse<object>.WithSuccess(result, "Thống kê toàn bộ hệ thống"));
        }
    }
}
