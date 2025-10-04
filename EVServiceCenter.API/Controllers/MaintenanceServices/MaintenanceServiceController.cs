using EVServiceCenter.Core.Domains.MaintenanceServices.DTOs.Requests;
using EVServiceCenter.Core.Domains.MaintenanceServices.DTOs.Responses;
using EVServiceCenter.Core.Domains.MaintenanceServices.Interfaces.Services;
using EVServiceCenter.Core.Domains.Shared.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVServiceCenter.API.Controllers.MaintenanceServices
{
    [ApiController]
    [Route("api/maintenance-services")]
    [Authorize(Policy = "AllInternal")]
    [ApiExplorerSettings(GroupName = "Staff - Services")]
    public class MaintenanceServiceController : BaseController
    {
        private readonly IMaintenanceServiceService _service;
        private readonly IValidator<CreateMaintenanceServiceRequestDto> _createValidator;
        private readonly IValidator<UpdateMaintenanceServiceRequestDto> _updateValidator;
        private readonly IValidator<MaintenanceServiceQueryDto> _queryValidator;
        private readonly ILogger<MaintenanceServiceController> _logger;

        public MaintenanceServiceController(
            IMaintenanceServiceService service,
            IValidator<CreateMaintenanceServiceRequestDto> createValidator,
            IValidator<UpdateMaintenanceServiceRequestDto> updateValidator,
            IValidator<MaintenanceServiceQueryDto> queryValidator,
            ILogger<MaintenanceServiceController> logger)
        {
            _service = service;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _queryValidator = queryValidator;
            _logger = logger;
        }

        /// <summary>
        /// Get all maintenance services with pagination and filters
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] MaintenanceServiceQueryDto query,
            CancellationToken ct)
        {
            var validation = await _queryValidator.ValidateAsync(query, ct);
            if (!validation.IsValid)
            {
                var errors = validation.Errors.Select(e => new
                {
                    Field = e.PropertyName,
                    Message = e.ErrorMessage
                });
                return BadRequest(ApiResponse<PagedResult<MaintenanceServiceResponseDto>>.WithValidationError(errors));
            }

            try
            {
                var result = await _service.GetAllAsync(query, ct);
                return Ok(ApiResponse<PagedResult<MaintenanceServiceResponseDto>>.WithSuccess(
                    result,
                    $"Tìm thấy {result.TotalCount} dịch vụ"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all maintenance services");
                return StatusCode(500, ApiResponse<PagedResult<MaintenanceServiceResponseDto>>.WithError(
                    "Có lỗi xảy ra", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Get maintenance service by ID
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            try
            {
                var result = await _service.GetByIdAsync(id, ct);
                if (result == null)
                    return NotFound(ApiResponse<MaintenanceServiceResponseDto>.WithNotFound($"Không tìm thấy dịch vụ {id}"));

                return Ok(ApiResponse<MaintenanceServiceResponseDto>.WithSuccess(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting maintenance service {Id}", id);
                return StatusCode(500, ApiResponse<MaintenanceServiceResponseDto>.WithError(
                    "Có lỗi xảy ra", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Get all active maintenance services
        /// </summary>
        [HttpGet("active")]
        [AllowAnonymous]
        public async Task<IActionResult> GetActive(CancellationToken ct)
        {
            try
            {
                var result = await _service.GetActiveServicesAsync(ct);
                return Ok(ApiResponse<IEnumerable<MaintenanceServiceResponseDto>>.WithSuccess(
                    result,
                    $"Tìm thấy {result.Count()} dịch vụ"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active maintenance services");
                return StatusCode(500, ApiResponse<IEnumerable<MaintenanceServiceResponseDto>>.WithError(
                    "Có lỗi xảy ra", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Get services by category
        /// </summary>
        [HttpGet("by-category/{categoryId:int}")]
        public async Task<IActionResult> GetByCategory(int categoryId, CancellationToken ct)
        {
            try
            {
                var result = await _service.GetServicesByCategoryAsync(categoryId, ct);
                return Ok(ApiResponse<IEnumerable<MaintenanceServiceResponseDto>>.WithSuccess(
                    result,
                    $"Tìm thấy {result.Count()} dịch vụ"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting services by category {CategoryId}", categoryId);
                return StatusCode(500, ApiResponse<IEnumerable<MaintenanceServiceResponseDto>>.WithError(
                    "Có lỗi xảy ra", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Create new maintenance service
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Create(
            [FromBody] CreateMaintenanceServiceRequestDto request,
            CancellationToken ct)
        {
            var validation = await _createValidator.ValidateAsync(request, ct);
            if (!validation.IsValid)
            {
                var errors = validation.Errors.Select(e => new
                {
                    Field = e.PropertyName,
                    Message = e.ErrorMessage
                });
                return BadRequest(ApiResponse<MaintenanceServiceResponseDto>.WithValidationError(errors));
            }

            try
            {
                var result = await _service.CreateAsync(request, ct);

                _logger.LogInformation("Maintenance service created by user {UserId}: {ServiceCode}",
                    GetCurrentUserId(), result.ServiceCode);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = result.ServiceId },
                    ApiResponse<MaintenanceServiceResponseDto>.WithSuccess(result, "Tạo dịch vụ thành công", 201));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<MaintenanceServiceResponseDto>.WithError(ex.Message, "BUSINESS_RULE_VIOLATION"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating maintenance service");
                return StatusCode(500, ApiResponse<MaintenanceServiceResponseDto>.WithError(
                    "Có lỗi xảy ra", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Update maintenance service
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdateMaintenanceServiceRequestDto request,
            CancellationToken ct)
        {
            if (id != request.ServiceId)
                return BadRequest(ApiResponse<MaintenanceServiceResponseDto>.WithError("ID không khớp", "ID_MISMATCH"));

            var validation = await _updateValidator.ValidateAsync(request, ct);
            if (!validation.IsValid)
            {
                var errors = validation.Errors.Select(e => new
                {
                    Field = e.PropertyName,
                    Message = e.ErrorMessage
                });
                return BadRequest(ApiResponse<MaintenanceServiceResponseDto>.WithValidationError(errors));
            }

            try
            {
                var result = await _service.UpdateAsync(request, ct);

                _logger.LogInformation("Maintenance service updated by user {UserId}: {ServiceId}",
                    GetCurrentUserId(), id);

                return Ok(ApiResponse<MaintenanceServiceResponseDto>.WithSuccess(result, "Cập nhật thành công"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<MaintenanceServiceResponseDto>.WithError(ex.Message, "BUSINESS_RULE_VIOLATION"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating maintenance service {Id}", id);
                return StatusCode(500, ApiResponse<MaintenanceServiceResponseDto>.WithError(
                    "Có lỗi xảy ra", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Delete maintenance service
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            try
            {
                await _service.DeleteAsync(id, ct);

                _logger.LogInformation("Maintenance service deleted by user {UserId}: {ServiceId}",
                    GetCurrentUserId(), id);

                return Ok(ApiResponse<object>.WithSuccess(null, "Xóa dịch vụ thành công"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.WithError(ex.Message, "BUSINESS_RULE_VIOLATION"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting maintenance service {Id}", id);
                return StatusCode(500, ApiResponse<object>.WithError("Có lỗi xảy ra", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Check if maintenance service can be deleted
        /// </summary>
        [HttpGet("{id:int}/can-delete")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> CanDelete(int id, CancellationToken ct)
        {
            try
            {
                var canDelete = await _service.CanDeleteAsync(id, ct);
                var data = new
                {
                    CanDelete = canDelete,
                    Reason = canDelete ? null : "Dịch vụ đang có lịch hẹn, phiếu công việc hoặc gói dịch vụ liên kết"
                };
                return Ok(ApiResponse<object>.WithSuccess(data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking can delete {Id}", id);
                return StatusCode(500, ApiResponse<object>.WithError("Có lỗi xảy ra", "INTERNAL_ERROR", 500));
            }
        }
    }
}