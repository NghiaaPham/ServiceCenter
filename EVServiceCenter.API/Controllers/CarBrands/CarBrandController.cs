using EVServiceCenter.Core.Domains.CarBrands.DTOs.Requests;
using EVServiceCenter.Core.Domains.CarBrands.DTOs.Responses;
using EVServiceCenter.Core.Domains.CarBrands.Interfaces.Services;
using EVServiceCenter.Core.Domains.Shared.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVServiceCenter.API.Controllers.CarBrands
{
    [ApiController]
    [Route("api/car-brands")]
    [Authorize(Policy = "AllInternal")]
    [ApiExplorerSettings(GroupName = "Staff - Car Brands")]
    public class CarBrandController : BaseController
    {
        private readonly ICarBrandService _service;
        private readonly IValidator<CreateCarBrandRequestDto> _createValidator;
        private readonly IValidator<UpdateCarBrandRequestDto> _updateValidator;
        private readonly ILogger<CarBrandController> _logger;

        public CarBrandController(
            ICarBrandService service,
            IValidator<CreateCarBrandRequestDto> createValidator,
            IValidator<UpdateCarBrandRequestDto> updateValidator,
            ILogger<CarBrandController> logger)
        {
            _service = service;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _logger = logger;
        }

        /// <summary>
        /// Get car brand by ID
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            try
            {
                var result = await _service.GetByIdAsync(id, ct);
                if (result == null)
                    return NotFound(ApiResponse<CarBrandResponseDto>.WithNotFound($"Không tìm thấy thương hiệu {id}"));

                return Ok(ApiResponse<CarBrandResponseDto>.WithSuccess(result, "Lấy thông tin thương hiệu thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting brand {Id}", id);
                return StatusCode(500, ApiResponse<CarBrandResponseDto>.WithError("Có lỗi xảy ra", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Get car brand by name
        /// </summary>
        [HttpGet("by-name/{name}")]
        public async Task<IActionResult> GetByName(string name, CancellationToken ct)
        {
            try
            {
                var result = await _service.GetByNameAsync(name, ct);
                if (result == null)
                    return NotFound(ApiResponse<CarBrandResponseDto>.WithNotFound($"Không tìm thấy thương hiệu {name}"));

                return Ok(ApiResponse<CarBrandResponseDto>.WithSuccess(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting brand {Name}", name);
                return StatusCode(500, ApiResponse<CarBrandResponseDto>.WithError("Có lỗi xảy ra", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Create new car brand
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Create(
            [FromBody] CreateCarBrandRequestDto request,
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
                return BadRequest(ApiResponse<CarBrandResponseDto>.WithValidationError(errors));
            }

            try
            {
                var result = await _service.CreateAsync(request, ct);

                _logger.LogInformation("Brand created by user {UserId}: {BrandName}",
                    GetCurrentUserId(), result.BrandName);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = result.BrandId },
                    ApiResponse<CarBrandResponseDto>.WithSuccess(result, "Tạo thương hiệu thành công", 201));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<CarBrandResponseDto>.WithError(ex.Message, "BUSINESS_RULE_VIOLATION"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating brand");
                return StatusCode(500, ApiResponse<CarBrandResponseDto>.WithError("Có lỗi xảy ra", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Update car brand
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdateCarBrandRequestDto request,
            CancellationToken ct)
        {
            if (id != request.BrandId)
                return BadRequest(ApiResponse<CarBrandResponseDto>.WithError("ID không khớp", "ID_MISMATCH"));

            var validation = await _updateValidator.ValidateAsync(request, ct);
            if (!validation.IsValid)
            {
                var errors = validation.Errors.Select(e => new
                {
                    Field = e.PropertyName,
                    Message = e.ErrorMessage
                });
                return BadRequest(ApiResponse<CarBrandResponseDto>.WithValidationError(errors));
            }

            try
            {
                var result = await _service.UpdateAsync(request, ct);

                _logger.LogInformation("Brand updated by user {UserId}: {BrandId}",
                    GetCurrentUserId(), id);

                return Ok(ApiResponse<CarBrandResponseDto>.WithSuccess(result, "Cập nhật thành công"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<CarBrandResponseDto>.WithError(ex.Message, "BUSINESS_RULE_VIOLATION"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating brand {Id}", id);
                return StatusCode(500, ApiResponse<CarBrandResponseDto>.WithError("Có lỗi xảy ra", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Delete car brand
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            try
            {
                await _service.DeleteAsync(id, ct);

                _logger.LogInformation("Brand deleted by user {UserId}: {BrandId}",
                    GetCurrentUserId(), id);

                return Ok(ApiResponse<object>.WithSuccess(null, "Xóa thương hiệu thành công"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.WithError(ex.Message, "BUSINESS_RULE_VIOLATION"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting brand {Id}", id);
                return StatusCode(500, ApiResponse<object>.WithError("Có lỗi xảy ra", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Check if brand can be deleted
        /// </summary>
        [HttpGet("{id:int}/can-delete")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> CanDelete(int id, CancellationToken ct)
        {
            try
            {
                var canDelete = await _service.CanDeleteAsync(id, ct);
                var data = new { CanDelete = canDelete, Reason = canDelete ? null : "Thương hiệu có dòng xe liên quan" };
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