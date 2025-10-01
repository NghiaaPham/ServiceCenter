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
    public class CarBrandQueryController : BaseController
    {
        private readonly ICarBrandQueryService _queryService;
        private readonly IValidator<CarBrandQueryDto> _queryValidator;
        private readonly ILogger<CarBrandQueryController> _logger;

        public CarBrandQueryController(
            ICarBrandQueryService queryService,
            IValidator<CarBrandQueryDto> queryValidator,
            ILogger<CarBrandQueryController> logger)
        {
            _queryService = queryService;
            _queryValidator = queryValidator;
            _logger = logger;
        }

        /// <summary>
        /// Get all car brands with pagination
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] CarBrandQueryDto query,
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
                return BadRequest(ApiResponse<PagedResult<CarBrandResponseDto>>.WithValidationError(errors));
            }

            try
            {
                var result = await _queryService.GetAllAsync(query, ct);
                return Ok(ApiResponse<PagedResult<CarBrandResponseDto>>.WithSuccess(
                    result,
                    $"Tìm thấy {result.TotalCount} thương hiệu"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all brands");
                return StatusCode(500, ApiResponse<PagedResult<CarBrandResponseDto>>.WithError(
                    "Có lỗi xảy ra", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Get active car brands
        /// </summary>
        [HttpGet("active")]
        public async Task<IActionResult> GetActive(CancellationToken ct)
        {
            try
            {
                var result = await _queryService.GetActiveBrandsAsync(ct);
                return Ok(ApiResponse<IEnumerable<CarBrandResponseDto>>.WithSuccess(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active brands");
                return StatusCode(500, ApiResponse<IEnumerable<CarBrandResponseDto>>.WithError(
                    "Có lỗi xảy ra", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Get car brands by country
        /// </summary>
        [HttpGet("by-country/{country}")]
        public async Task<IActionResult> GetByCountry(string country, CancellationToken ct)
        {
            try
            {
                var result = await _queryService.GetBrandsByCountryAsync(country, ct);
                return Ok(ApiResponse<IEnumerable<CarBrandResponseDto>>.WithSuccess(
                    result,
                    $"Tìm thấy {result.Count()} thương hiệu"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting brands by country {Country}", country);
                return StatusCode(500, ApiResponse<IEnumerable<CarBrandResponseDto>>.WithError(
                    "Có lỗi xảy ra", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Search car brands
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> Search(
            [FromQuery] string searchTerm,
            CancellationToken ct)
        {
            try
            {
                var result = await _queryService.SearchBrandsAsync(searchTerm, ct);
                return Ok(ApiResponse<IEnumerable<CarBrandResponseDto>>.WithSuccess(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching brands");
                return StatusCode(500, ApiResponse<IEnumerable<CarBrandResponseDto>>.WithError(
                    "Có lỗi xảy ra", "INTERNAL_ERROR", 500));
            }
        }
    }
}