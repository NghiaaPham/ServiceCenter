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
    [Authorize(Policy = "AdminOrStaff")]
    [ApiExplorerSettings(GroupName = "Staff - Service Centers")]
    public class ServiceCenterController : BaseController
    {
        private readonly IServiceCenterService _service;
        private readonly IValidator<CreateServiceCenterRequestDto> _createValidator;
        private readonly IValidator<UpdateServiceCenterRequestDto> _updateValidator;

        public ServiceCenterController(
            IServiceCenterService service,
            IValidator<CreateServiceCenterRequestDto> createValidator,
            IValidator<UpdateServiceCenterRequestDto> updateValidator)
        {
            _service = service;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        /// <summary>
        /// Get service center by ID
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<ServiceCenterResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ServiceCenterResponseDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            var result = await _service.GetByIdAsync(id, ct);
            if (result == null)
                return NotFound(ApiResponse<ServiceCenterResponseDto>.WithNotFound($"Không tìm thấy trung tâm {id}"));

            return Ok(ApiResponse<ServiceCenterResponseDto>.WithSuccess(result));
        }

        /// <summary>
        /// Get service center by code
        /// </summary>
        [HttpGet("by-code/{code}")]
        public async Task<IActionResult> GetByCode(string code, CancellationToken ct)
        {
            var result = await _service.GetByCenterCodeAsync(code, ct);
            if (result == null)
                return NotFound(ApiResponse<ServiceCenterResponseDto>.WithNotFound($"Không tìm thấy trung tâm {code}"));

            return Ok(ApiResponse<ServiceCenterResponseDto>.WithSuccess(result));
        }

        /// <summary>
        /// Create new service center
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Create(
            [FromBody] CreateServiceCenterRequestDto request,
            CancellationToken ct)
        {
            var validation = await _createValidator.ValidateAsync(request, ct);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<ServiceCenterResponseDto>.WithValidationError(validation.Errors));

            var result = await _service.CreateAsync(request, ct);
            return CreatedAtAction(nameof(GetById),
                new { id = result.CenterId },
                ApiResponse<ServiceCenterResponseDto>.WithSuccess(result, "Tạo mới thành công"));
        }

        /// <summary>
        /// Update service center
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdateServiceCenterRequestDto request,
            CancellationToken ct)
        {
            if (id != request.CenterId)
                return BadRequest(ApiResponse<ServiceCenterResponseDto>.WithError("ID không khớp"));

            var validation = await _updateValidator.ValidateAsync(request, ct);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<ServiceCenterResponseDto>.WithValidationError(validation.Errors));

            var result = await _service.UpdateAsync(request, ct);
            return Ok(ApiResponse<ServiceCenterResponseDto>.WithSuccess(result, "Cập nhật thành công"));
        }

        /// <summary>
        /// Delete service center
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var deleted = await _service.DeleteAsync(id, ct);
            if (!deleted)
                return NotFound(ApiResponse<ServiceCenterResponseDto>.WithNotFound($"Không tìm thấy trung tâm {id}"));

            return Ok(ApiResponse<string>.WithSuccess("Xóa thành công"));
        }

        /// <summary>
        /// Check if center can be deleted
        /// </summary>
        [HttpGet("{id:int}/can-delete")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> CanDelete(int id, CancellationToken ct)
        {
            var canDelete = await _service.CanDeleteAsync(id, ct);
            return Ok(ApiResponse<object>.WithSuccess(new { CanDelete = canDelete }));
        }
    }

}