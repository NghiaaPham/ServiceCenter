using EVServiceCenter.Core.Domains.ServiceCenters.DTOs.Responses;
using EVServiceCenter.Core.Domains.ServiceCenters.Interfaces.Services;
using EVServiceCenter.Core.Domains.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVServiceCenter.API.Controllers.ServiceCenters
{
    [ApiController]
    [Route("api/service-centers")]
    [Authorize(Policy = "AllInternal")]
    public class ServiceCenterAvailabilityController : BaseController
    {
        private readonly IServiceCenterAvailabilityService _availabilityService;

        public ServiceCenterAvailabilityController(IServiceCenterAvailabilityService availabilityService)
        {
            _availabilityService = availabilityService;
        }

        [HttpGet("{id:int}/availability")]
        [ProducesResponseType(typeof(ApiResponse<ServiceCenterAvailabilityDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ServiceCenterAvailabilityDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAvailability(
            int id,
            [FromQuery] DateTime? date,
            CancellationToken ct)
        {
            var checkDate = date ?? DateTime.Today;
            var result = await _availabilityService.GetAvailabilityAsync(id, checkDate, ct);

            if (result == null)
            {
                return NotFound(ApiResponse<ServiceCenterAvailabilityDto>.WithNotFound(
                    $"Không tìm thấy trung tâm có ID = {id} trong ngày {checkDate:dd/MM/yyyy}"
                ));
            }

            return Ok(ApiResponse<ServiceCenterAvailabilityDto>.WithSuccess(
                result, $"Trung tâm {id} có dữ liệu khả dụng"
            ));
        }

        [HttpGet("available")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ServiceCenterAvailabilityDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAvailableCenters(
            [FromQuery] DateTime? date,
            CancellationToken ct)
        {
            var checkDate = date ?? DateTime.Today;
            var result = await _availabilityService.GetAvailableCentersAsync(checkDate, ct);

            return Ok(ApiResponse<IEnumerable<ServiceCenterAvailabilityDto>>.WithSuccess(
                result,
                $"Tìm thấy {result.Count()} trung tâm có chỗ trong ngày {checkDate:dd/MM/yyyy}"
            ));
        }

    }
}
