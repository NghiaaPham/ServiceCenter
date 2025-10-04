using EVServiceCenter.Core.Domains.CarModels.DTOs.Requests;
using EVServiceCenter.Core.Domains.CarModels.DTOs.Responses;
using EVServiceCenter.Core.Domains.CarModels.Interfaces.Services;
using EVServiceCenter.Core.Domains.Shared.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVServiceCenter.API.Controllers.CarModels
{
    [ApiController]
    [Route("api/car-models")]
    [Authorize(Policy = "AllInternal")]
    [ApiExplorerSettings(GroupName = "Staff - Car Models")]
    public class CarModelController : BaseController
    {
        private readonly ICarModelService _service;
        private readonly IValidator<CreateCarModelRequestDto> _createValidator;
        private readonly IValidator<UpdateCarModelRequestDto> _updateValidator;
        private readonly ILogger<CarModelController> _logger;

        public CarModelController(
            ICarModelService service,
            IValidator<CreateCarModelRequestDto> createValidator,
            IValidator<UpdateCarModelRequestDto> updateValidator,
            ILogger<CarModelController> logger)
        {
            _service = service;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _logger = logger;
        }

        /// <summary>
        /// Get car model by ID
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            try
            {
                var result = await _service.GetByIdAsync(id, ct);
                if (result == null)
                    return NotFound(ApiResponse<CarModelResponseDto>.WithNotFound($"Không tìm thấy dòng xe {id}"));

                return Ok(ApiResponse<CarModelResponseDto>.WithSuccess(result, "Lấy thông tin dòng xe thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting model {Id}", id);
                return StatusCode(500, ApiResponse<CarModelResponseDto>.WithError("Có lỗi xảy ra", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Create new car model
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Create(
            [FromBody] CreateCarModelRequestDto request,
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
                return BadRequest(ApiResponse<CarModelResponseDto>.WithValidationError(errors));
            }

            try
            {
                var result = await _service.CreateAsync(request, ct);

                _logger.LogInformation("Car model created by user {UserId}: {BrandId} {ModelName}",
                    GetCurrentUserId(), result.BrandId, result.ModelName);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = result.ModelId },
                    ApiResponse<CarModelResponseDto>.WithSuccess(result, "Tạo dòng xe thành công", 201));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<CarModelResponseDto>.WithError(ex.Message, "BUSINESS_RULE_VIOLATION"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating car model");
                return StatusCode(500, ApiResponse<CarModelResponseDto>.WithError("Có lỗi xảy ra", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Update car model
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdateCarModelRequestDto request,
            CancellationToken ct)
        {
            if (id != request.ModelId)
                return BadRequest(ApiResponse<CarModelResponseDto>.WithError("ID không khớp", "ID_MISMATCH"));

            var validation = await _updateValidator.ValidateAsync(request, ct);
            if (!validation.IsValid)
            {
                var errors = validation.Errors.Select(e => new
                {
                    Field = e.PropertyName,
                    Message = e.ErrorMessage
                });
                return BadRequest(ApiResponse<CarModelResponseDto>.WithValidationError(errors));
            }

            try
            {
                var result = await _service.UpdateAsync(request, ct);

                _logger.LogInformation("Car model updated by user {UserId}: {ModelId}",
                    GetCurrentUserId(), id);

                return Ok(ApiResponse<CarModelResponseDto>.WithSuccess(result, "Cập nhật thành công"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<CarModelResponseDto>.WithError(ex.Message, "BUSINESS_RULE_VIOLATION"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating model {Id}", id);
                return StatusCode(500, ApiResponse<CarModelResponseDto>.WithError("Có lỗi xảy ra", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Delete car model
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            try
            {
                await _service.DeleteAsync(id, ct);

                _logger.LogInformation("Car model deleted by user {UserId}: {ModelId}",
                    GetCurrentUserId(), id);

                return Ok(ApiResponse<object>.WithSuccess(null, "Xóa dòng xe thành công"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.WithError(ex.Message, "BUSINESS_RULE_VIOLATION"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting model {Id}", id);
                return StatusCode(500, ApiResponse<object>.WithError("Có lỗi xảy ra", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Check if model can be deleted
        /// </summary>
        [HttpGet("{id:int}/can-delete")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> CanDelete(int id, CancellationToken ct)
        {
            try
            {
                var canDelete = await _service.CanDeleteAsync(id, ct);
                var data = new { CanDelete = canDelete, Reason = canDelete ? null : "Dòng xe có xe hoặc bảng giá liên quan" };
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