using EVServiceCenter.Core.Domains.ModelServicePricings.DTOs.Requests;
using EVServiceCenter.Core.Domains.ModelServicePricings.DTOs.Responses;
using EVServiceCenter.Core.Domains.ModelServicePricings.Interfaces.Services;
using EVServiceCenter.Core.Domains.Shared.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVServiceCenter.API.Controllers.ModelServicePricings
{
    [ApiController]
    [Route("api/model-service-pricings")]
    [Authorize(Policy = "AllInternal")]
    public class ModelServicePricingController : BaseController
    {
        private readonly IModelServicePricingService _service;
        private readonly IValidator<CreateModelServicePricingRequestDto> _createValidator;
        private readonly IValidator<UpdateModelServicePricingRequestDto> _updateValidator;
        private readonly IValidator<ModelServicePricingQueryDto> _queryValidator;
        private readonly ILogger<ModelServicePricingController> _logger;

        public ModelServicePricingController(
            IModelServicePricingService service,
            IValidator<CreateModelServicePricingRequestDto> createValidator,
            IValidator<UpdateModelServicePricingRequestDto> updateValidator,
            IValidator<ModelServicePricingQueryDto> queryValidator,
            ILogger<ModelServicePricingController> logger)
        {
            _service = service;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _queryValidator = queryValidator;
            _logger = logger;
        }

        /// <summary>
        /// Get all model service pricings with pagination and filters
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] ModelServicePricingQueryDto query,
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
                return BadRequest(ApiResponse<PagedResult<ModelServicePricingResponseDto>>.WithValidationError(errors));
            }

            try
            {
                var result = await _service.GetAllAsync(query, ct);
                return Ok(ApiResponse<PagedResult<ModelServicePricingResponseDto>>.WithSuccess(
                    result,
                    $"Tìm thấy {result.TotalCount} bản ghi giá"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all model service pricings");
                return StatusCode(500, ApiResponse<PagedResult<ModelServicePricingResponseDto>>.WithError(
                    "Có lỗi xảy ra", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Get model service pricing by ID
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            try
            {
                var result = await _service.GetByIdAsync(id, ct);
                if (result == null)
                    return NotFound(ApiResponse<ModelServicePricingResponseDto>.WithNotFound($"Không tìm thấy bản ghi giá {id}"));

                return Ok(ApiResponse<ModelServicePricingResponseDto>.WithSuccess(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting model service pricing {Id}", id);
                return StatusCode(500, ApiResponse<ModelServicePricingResponseDto>.WithError(
                    "Có lỗi xảy ra", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Get pricings by model
        /// </summary>
        [HttpGet("by-model/{modelId:int}")]
        public async Task<IActionResult> GetByModel(int modelId, CancellationToken ct)
        {
            try
            {
                var result = await _service.GetByModelIdAsync(modelId, ct);
                return Ok(ApiResponse<IEnumerable<ModelServicePricingResponseDto>>.WithSuccess(
                    result,
                    $"Tìm thấy {result.Count()} bản ghi giá"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pricings by model {ModelId}", modelId);
                return StatusCode(500, ApiResponse<IEnumerable<ModelServicePricingResponseDto>>.WithError(
                    "Có lỗi xảy ra", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Get pricings by service
        /// </summary>
        [HttpGet("by-service/{serviceId:int}")]
        public async Task<IActionResult> GetByService(int serviceId, CancellationToken ct)
        {
            try
            {
                var result = await _service.GetByServiceIdAsync(serviceId, ct);
                return Ok(ApiResponse<IEnumerable<ModelServicePricingResponseDto>>.WithSuccess(
                    result,
                    $"Tìm thấy {result.Count()} bản ghi giá"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pricings by service {ServiceId}", serviceId);
                return StatusCode(500, ApiResponse<IEnumerable<ModelServicePricingResponseDto>>.WithError(
                    "Có lỗi xảy ra", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Get active pricing for a model and service
        /// </summary>
        [HttpGet("active")]
        [AllowAnonymous]
        public async Task<IActionResult> GetActivePricing(
            [FromQuery] int modelId,
            [FromQuery] int serviceId,
            [FromQuery] DateOnly? forDate,
            CancellationToken ct)
        {
            if (modelId <= 0)
                return BadRequest(ApiResponse<ModelServicePricingResponseDto>.WithError("Model ID không hợp lệ", "INVALID_MODEL_ID"));

            if (serviceId <= 0)
                return BadRequest(ApiResponse<ModelServicePricingResponseDto>.WithError("Service ID không hợp lệ", "INVALID_SERVICE_ID"));

            try
            {
                var result = await _service.GetActivePricingAsync(modelId, serviceId, forDate, ct);
                if (result == null)
                    return NotFound(ApiResponse<ModelServicePricingResponseDto>.WithNotFound(
                        "Không tìm thấy giá tùy chỉnh, sẽ sử dụng giá cơ bản"));

                return Ok(ApiResponse<ModelServicePricingResponseDto>.WithSuccess(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active pricing for model {ModelId} service {ServiceId}", modelId, serviceId);
                return StatusCode(500, ApiResponse<ModelServicePricingResponseDto>.WithError(
                    "Có lỗi xảy ra", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Create new model service pricing
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Create(
            [FromBody] CreateModelServicePricingRequestDto request,
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
                return BadRequest(ApiResponse<ModelServicePricingResponseDto>.WithValidationError(errors));
            }

            try
            {
                var result = await _service.CreateAsync(request, ct);

                _logger.LogInformation("Model service pricing created by user {UserId}: Model {ModelId} - Service {ServiceId}",
                    GetCurrentUserId(), result.ModelId, result.ServiceId);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = result.PricingId },
                    ApiResponse<ModelServicePricingResponseDto>.WithSuccess(result, "Tạo bản ghi giá thành công", 201));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<ModelServicePricingResponseDto>.WithError(ex.Message, "BUSINESS_RULE_VIOLATION"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating model service pricing");
                return StatusCode(500, ApiResponse<ModelServicePricingResponseDto>.WithError(
                    "Có lỗi xảy ra", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Update model service pricing
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdateModelServicePricingRequestDto request,
            CancellationToken ct)
        {
            if (id != request.PricingId)
                return BadRequest(ApiResponse<ModelServicePricingResponseDto>.WithError("ID không khớp", "ID_MISMATCH"));

            var validation = await _updateValidator.ValidateAsync(request, ct);
            if (!validation.IsValid)
            {
                var errors = validation.Errors.Select(e => new
                {
                    Field = e.PropertyName,
                    Message = e.ErrorMessage
                });
                return BadRequest(ApiResponse<ModelServicePricingResponseDto>.WithValidationError(errors));
            }

            try
            {
                var result = await _service.UpdateAsync(request, ct);

                _logger.LogInformation("Model service pricing updated by user {UserId}: {PricingId}",
                    GetCurrentUserId(), id);

                return Ok(ApiResponse<ModelServicePricingResponseDto>.WithSuccess(result, "Cập nhật thành công"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<ModelServicePricingResponseDto>.WithError(ex.Message, "BUSINESS_RULE_VIOLATION"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating model service pricing {Id}", id);
                return StatusCode(500, ApiResponse<ModelServicePricingResponseDto>.WithError(
                    "Có lỗi xảy ra", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Delete model service pricing
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            try
            {
                await _service.DeleteAsync(id, ct);

                _logger.LogInformation("Model service pricing deleted by user {UserId}: {PricingId}",
                    GetCurrentUserId(), id);

                return Ok(ApiResponse<object>.WithSuccess(null, "Xóa bản ghi giá thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting model service pricing {Id}", id);
                return StatusCode(500, ApiResponse<object>.WithError("Có lỗi xảy ra", "INTERNAL_ERROR", 500));
            }
        }
    }
}