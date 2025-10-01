using EVServiceCenter.Core.Domains.CustomerVehicles.DTOs.Requests;
using EVServiceCenter.Core.Domains.CustomerVehicles.DTOs.Responses;
using EVServiceCenter.Core.Domains.CustomerVehicles.Interfaces.Services;
using EVServiceCenter.Core.Domains.Shared.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVServiceCenter.API.Controllers.CustomerVehicles
{
    [ApiController]
    [Route("api/customer-vehicles")]
    [Authorize(Policy = "AllInternal")]
    public class CustomerVehicleQueryController : BaseController
    {
        private readonly ICustomerVehicleQueryService _queryService;
        private readonly IValidator<CustomerVehicleQueryDto> _queryValidator;
        private readonly ILogger<CustomerVehicleQueryController> _logger;

        public CustomerVehicleQueryController(
            ICustomerVehicleQueryService queryService,
            IValidator<CustomerVehicleQueryDto> queryValidator,
            ILogger<CustomerVehicleQueryController> logger)
        {
            _queryService = queryService;
            _queryValidator = queryValidator;
            _logger = logger;
        }

        /// <summary>
        /// Get all vehicles with pagination
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] CustomerVehicleQueryDto query,
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
                return BadRequest(ApiResponse<PagedResult<CustomerVehicleResponseDto>>.WithValidationError(errors));
            }

            try
            {
                var result = await _queryService.GetAllAsync(query, ct);
                return Ok(ApiResponse<PagedResult<CustomerVehicleResponseDto>>.WithSuccess(
                    result,
                    $"Tìm thấy {result.TotalCount} xe"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all vehicles");
                return StatusCode(500, ApiResponse<PagedResult<CustomerVehicleResponseDto>>.WithError(
                    "Có lỗi xảy ra", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Get vehicles by customer
        /// </summary>
        [HttpGet("by-customer/{customerId:int}")]
        public async Task<IActionResult> GetByCustomer(int customerId, CancellationToken ct)
        {
            try
            {
                var result = await _queryService.GetVehiclesByCustomerAsync(customerId, ct);
                return Ok(ApiResponse<IEnumerable<CustomerVehicleResponseDto>>.WithSuccess(
                    result,
                    $"Tìm thấy {result.Count()} xe"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vehicles by customer {CustomerId}", customerId);
                return StatusCode(500, ApiResponse<IEnumerable<CustomerVehicleResponseDto>>.WithError(
                    "Có lỗi xảy ra", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Get vehicles by model
        /// </summary>
        [HttpGet("by-model/{modelId:int}")]
        public async Task<IActionResult> GetByModel(int modelId, CancellationToken ct)
        {
            try
            {
                var result = await _queryService.GetVehiclesByModelAsync(modelId, ct);
                return Ok(ApiResponse<IEnumerable<CustomerVehicleResponseDto>>.WithSuccess(
                    result,
                    $"Tìm thấy {result.Count()} xe"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vehicles by model {ModelId}", modelId);
                return StatusCode(500, ApiResponse<IEnumerable<CustomerVehicleResponseDto>>.WithError(
                    "Có lỗi xảy ra", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Get vehicles due for maintenance
        /// </summary>
        [HttpGet("maintenance-due")]
        public async Task<IActionResult> GetMaintenanceDue(CancellationToken ct)
        {
            try
            {
                var result = await _queryService.GetMaintenanceDueVehiclesAsync(ct);
                return Ok(ApiResponse<IEnumerable<CustomerVehicleResponseDto>>.WithSuccess(
                    result,
                    $"Tìm thấy {result.Count()} xe cần bảo dưỡng"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting maintenance due vehicles");
                return StatusCode(500, ApiResponse<IEnumerable<CustomerVehicleResponseDto>>.WithError(
                    "Có lỗi xảy ra", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Get vehicle by license plate
        /// </summary>
        [HttpGet("by-license-plate/{licensePlate}")]
        public async Task<IActionResult> GetByLicensePlate(string licensePlate, CancellationToken ct)
        {
            try
            {
                var result = await _queryService.GetByLicensePlateAsync(licensePlate, ct);
                if (result == null)
                    return NotFound(ApiResponse<CustomerVehicleResponseDto>.WithNotFound($"Không tìm thấy xe {licensePlate}"));

                return Ok(ApiResponse<CustomerVehicleResponseDto>.WithSuccess(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vehicle by license plate {LicensePlate}", licensePlate);
                return StatusCode(500, ApiResponse<CustomerVehicleResponseDto>.WithError(
                    "Có lỗi xảy ra", "INTERNAL_ERROR", 500));
            }
        }
    }
}