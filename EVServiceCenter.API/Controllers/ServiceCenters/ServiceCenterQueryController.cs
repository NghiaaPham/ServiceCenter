namespace EVServiceCenter.API.Controllers.ServiceCenters
{
    using EVServiceCenter.API.Controllers;
    using EVServiceCenter.Core.Domains.ServiceCenters.DTOs.Requests;
    using EVServiceCenter.Core.Domains.ServiceCenters.DTOs.Responses;
    using EVServiceCenter.Core.Domains.ServiceCenters.Interfaces.Services;
    using EVServiceCenter.Core.Domains.Shared.Models;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("api/service-centers")]
    [AllowAnonymous]
    [ApiExplorerSettings(GroupName = "Staff - Service Centers")]
    public class ServiceCenterQueryController : BaseController
    {
        private readonly IServiceCenterQueryService _queryService;
        private readonly IValidator<ServiceCenterQueryDto> _queryValidator;

        public ServiceCenterQueryController(
            IServiceCenterQueryService queryService,
            IValidator<ServiceCenterQueryDto> queryValidator)
        {
            _queryService = queryService;
            _queryValidator = queryValidator;
        }

        /// <summary>
        /// Get all service centers with pagination
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<ServiceCenterResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAll(
            [FromQuery] ServiceCenterQueryDto query,
            CancellationToken ct)
        {
            var validation = await _queryValidator.ValidateAsync(query, ct);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.WithValidationError(validation.Errors));

            var result = await _queryService.GetAllAsync(query, ct);
            return Ok(ApiResponse<PagedResult<ServiceCenterResponseDto>>
                .WithSuccess(result, $"Tìm thấy {result.TotalCount} trung tâm"));
        }

        /// <summary>
        /// Get active centers
        /// </summary>
        [HttpGet("active")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ServiceCenterResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetActive(CancellationToken ct)
        {
            var result = await _queryService.GetActiveCentersAsync(ct);
            return Ok(ApiResponse<IEnumerable<ServiceCenterResponseDto>>.WithSuccess(result));
        }

        /// <summary>
        /// Get centers by province
        /// </summary>
        [HttpGet("by-province/{province}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ServiceCenterResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByProvince(string province, CancellationToken ct)
        {
            var result = await _queryService.GetCentersByProvinceAsync(province, ct);
            return Ok(ApiResponse<IEnumerable<ServiceCenterResponseDto>>.WithSuccess(result));
        }

        /// <summary>
        /// Search centers by keyword
        /// </summary>
        [HttpGet("search")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ServiceCenterResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Search(
            [FromQuery] string searchTerm,
            CancellationToken ct)
        {
            var result = await _queryService.SearchCentersAsync(searchTerm, ct);
            return Ok(ApiResponse<IEnumerable<ServiceCenterResponseDto>>.WithSuccess(result));
        }
    }
}
