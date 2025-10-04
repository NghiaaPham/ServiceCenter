using EVServiceCenter.Core.Domains.ServiceCategories.DTOs.Requests;
using EVServiceCenter.Core.Domains.ServiceCategories.DTOs.Responses;
using EVServiceCenter.Core.Domains.ServiceCategories.Interfaces.Services;
using EVServiceCenter.Core.Domains.Shared.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVServiceCenter.API.Controllers.ServiceCategories
{
    [ApiController]
    [Route("api/service-categories")]
    [Authorize(Policy = "AllInternal")]
    [ApiExplorerSettings(GroupName = "Staff - Service Categories")]
    public class ServiceCategoryController : BaseController
    {
        private readonly IServiceCategoryService _service;
        private readonly IValidator<CreateServiceCategoryRequestDto> _createValidator;
        private readonly IValidator<UpdateServiceCategoryRequestDto> _updateValidator;
        private readonly IValidator<ServiceCategoryQueryDto> _queryValidator;
        private readonly ILogger<ServiceCategoryController> _logger;

        public ServiceCategoryController(
            IServiceCategoryService service,
            IValidator<CreateServiceCategoryRequestDto> createValidator,
            IValidator<UpdateServiceCategoryRequestDto> updateValidator,
            IValidator<ServiceCategoryQueryDto> queryValidator,
            ILogger<ServiceCategoryController> logger)
        {
            _service = service;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _queryValidator = queryValidator;
            _logger = logger;
        }

        /// <summary>
        /// Get all service categories with pagination
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] ServiceCategoryQueryDto query,
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
                return BadRequest(ApiResponse<PagedResult<ServiceCategoryResponseDto>>.WithValidationError(errors));
            }

            try
            {
                var result = await _service.GetAllAsync(query, ct);
                return Ok(ApiResponse<PagedResult<ServiceCategoryResponseDto>>.WithSuccess(
                    result,
                    $"Tìm thấy {result.TotalCount} loại dịch vụ"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all service categories");
                return StatusCode(500, ApiResponse<PagedResult<ServiceCategoryResponseDto>>.WithError(
                    "Có lỗi xảy ra", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Get service category by ID
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            try
            {
                var result = await _service.GetByIdAsync(id, ct);
                if (result == null)
                    return NotFound(ApiResponse<ServiceCategoryResponseDto>.WithNotFound($"Không tìm thấy loại dịch vụ {id}"));

                return Ok(ApiResponse<ServiceCategoryResponseDto>.WithSuccess(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting service category {Id}", id);
                return StatusCode(500, ApiResponse<ServiceCategoryResponseDto>.WithError(
                    "Có lỗi xảy ra", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Get all active service categories
        /// </summary>
        [HttpGet("active")]
        [AllowAnonymous]
        public async Task<IActionResult> GetActive(CancellationToken ct)
        {
            try
            {
                var result = await _service.GetActiveCategoriesAsync(ct);
                return Ok(ApiResponse<IEnumerable<ServiceCategoryResponseDto>>.WithSuccess(
                    result,
                    $"Tìm thấy {result.Count()} loại dịch vụ"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active service categories");
                return StatusCode(500, ApiResponse<IEnumerable<ServiceCategoryResponseDto>>.WithError(
                    "Có lỗi xảy ra", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Create new service category
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Create(
            [FromBody] CreateServiceCategoryRequestDto request,
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
                return BadRequest(ApiResponse<ServiceCategoryResponseDto>.WithValidationError(errors));
            }

            try
            {
                var result = await _service.CreateAsync(request, ct);

                _logger.LogInformation("Service category created by user {UserId}: {CategoryName}",
                    GetCurrentUserId(), result.CategoryName);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = result.CategoryId },
                    ApiResponse<ServiceCategoryResponseDto>.WithSuccess(result, "Tạo loại dịch vụ thành công", 201));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<ServiceCategoryResponseDto>.WithError(ex.Message, "BUSINESS_RULE_VIOLATION"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating service category");
                return StatusCode(500, ApiResponse<ServiceCategoryResponseDto>.WithError(
                    "Có lỗi xảy ra", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Update service category
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdateServiceCategoryRequestDto request,
            CancellationToken ct)
        {
            if (id != request.CategoryId)
                return BadRequest(ApiResponse<ServiceCategoryResponseDto>.WithError("ID không khớp", "ID_MISMATCH"));

            var validation = await _updateValidator.ValidateAsync(request, ct);
            if (!validation.IsValid)
            {
                var errors = validation.Errors.Select(e => new
                {
                    Field = e.PropertyName,
                    Message = e.ErrorMessage
                });
                return BadRequest(ApiResponse<ServiceCategoryResponseDto>.WithValidationError(errors));
            }

            try
            {
                var result = await _service.UpdateAsync(request, ct);

                _logger.LogInformation("Service category updated by user {UserId}: {CategoryId}",
                    GetCurrentUserId(), id);

                return Ok(ApiResponse<ServiceCategoryResponseDto>.WithSuccess(result, "Cập nhật thành công"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<ServiceCategoryResponseDto>.WithError(ex.Message, "BUSINESS_RULE_VIOLATION"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating service category {Id}", id);
                return StatusCode(500, ApiResponse<ServiceCategoryResponseDto>.WithError(
                    "Có lỗi xảy ra", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Delete service category
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            try
            {
                await _service.DeleteAsync(id, ct);

                _logger.LogInformation("Service category deleted by user {UserId}: {CategoryId}",
                    GetCurrentUserId(), id);

                return Ok(ApiResponse<object>.WithSuccess(null, "Xóa loại dịch vụ thành công"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.WithError(ex.Message, "BUSINESS_RULE_VIOLATION"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting service category {Id}", id);
                return StatusCode(500, ApiResponse<object>.WithError("Có lỗi xảy ra", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Check if service category can be deleted
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
                    Reason = canDelete ? null : "Loại dịch vụ đang có dịch vụ liên kết"
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