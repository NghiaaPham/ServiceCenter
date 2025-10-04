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
    public class CarModelQueryController : BaseController
    {
        private readonly ICarModelQueryService _queryService;
        private readonly IValidator<CarModelQueryDto> _queryValidator;
        private readonly ILogger<CarModelQueryController> _logger;

        public CarModelQueryController(
            ICarModelQueryService queryService,
            IValidator<CarModelQueryDto> queryValidator,
            ILogger<CarModelQueryController> logger)
        {
            _queryService = queryService;
            _queryValidator = queryValidator;
            _logger = logger;
        }

        /// <summary>
        /// Get all car models with pagination
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] CarModelQueryDto query,
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
                return BadRequest(ApiResponse<PagedResult<CarModelResponseDto>>.WithValidationError(errors));
            }

            try
            {
                var result = await _queryService.GetAllAsync(query, ct);
                return Ok(ApiResponse<PagedResult<CarModelResponseDto>>.WithSuccess(
                    result,
                    $"Tìm thấy {result.TotalCount} dòng xe"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all models");
                return StatusCode(500, ApiResponse<PagedResult<CarModelResponseDto>>.WithError(
                    "Có lỗi xảy ra", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Get active car models
        /// </summary>
        [HttpGet("active")]
        public async Task<IActionResult> GetActive(CancellationToken ct)
        {
            try
            {
                var result = await _queryService.GetActiveModelsAsync(ct);
                return Ok(ApiResponse<IEnumerable<CarModelResponseDto>>.WithSuccess(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active models");
                return StatusCode(500, ApiResponse<IEnumerable<CarModelResponseDto>>.WithError(
                    "Có lỗi xảy ra", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Get car models by brand
        /// </summary>
        [HttpGet("by-brand/{brandId:int}")]
        public async Task<IActionResult> GetByBrand(int brandId, CancellationToken ct)
        {
            try
            {
                var result = await _queryService.GetModelsByBrandAsync(brandId, ct);
                return Ok(ApiResponse<IEnumerable<CarModelResponseDto>>.WithSuccess(
                    result,
                    $"Tìm thấy {result.Count()} dòng xe"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting models by brand {BrandId}", brandId);
                return StatusCode(500, ApiResponse<IEnumerable<CarModelResponseDto>>.WithError(
                    "Có lỗi xảy ra", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Search car models
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> Search(
            [FromQuery] string searchTerm,
            CancellationToken ct)
        {
            try
            {
                var result = await _queryService.SearchModelsAsync(searchTerm, ct);
                return Ok(ApiResponse<IEnumerable<CarModelResponseDto>>.WithSuccess(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching models");
                return StatusCode(500, ApiResponse<IEnumerable<CarModelResponseDto>>.WithError(
                    "Có lỗi xảy ra", "INTERNAL_ERROR", 500));
            }
        }
    }
}