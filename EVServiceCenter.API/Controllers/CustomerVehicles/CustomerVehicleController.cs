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
    [ApiExplorerSettings(GroupName = "Staff - Vehicles")]
    public class CustomerVehicleController : BaseController
    {
        private readonly ICustomerVehicleService _service;
        private readonly IValidator<CreateCustomerVehicleRequestDto> _createValidator;
        private readonly IValidator<UpdateCustomerVehicleRequestDto> _updateValidator;
        private readonly ILogger<CustomerVehicleController> _logger;

        public CustomerVehicleController(
            ICustomerVehicleService service,
            IValidator<CreateCustomerVehicleRequestDto> createValidator,
            IValidator<UpdateCustomerVehicleRequestDto> updateValidator,
            ILogger<CustomerVehicleController> logger)
        {
            _service = service;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _logger = logger;
        }

        /// <summary>
        /// Get vehicle by ID
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            try
            {
                var result = await _service.GetByIdAsync(id, ct);
                if (result == null)
                    return NotFound(ApiResponse<CustomerVehicleResponseDto>.WithNotFound($"Không tìm thấy xe {id}"));

                return Ok(ApiResponse<CustomerVehicleResponseDto>.WithSuccess(result, "Lấy thông tin xe thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vehicle {Id}", id);
                return StatusCode(500, ApiResponse<CustomerVehicleResponseDto>.WithError("Có lỗi xảy ra", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Create new vehicle
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "AdminOrStaff")]
        public async Task<IActionResult> Create(
            [FromBody] CreateCustomerVehicleRequestDto request,
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
                return BadRequest(ApiResponse<CustomerVehicleResponseDto>.WithValidationError(errors));
            }

            try
            {
                var userId = GetCurrentUserId();
                var result = await _service.CreateAsync(request, userId, ct);

                _logger.LogInformation("Vehicle created by user {UserId}: {LicensePlate}",
                    userId, result.LicensePlate);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = result.VehicleId },
                    ApiResponse<CustomerVehicleResponseDto>.WithSuccess(result, "Tạo xe thành công", 201));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<CustomerVehicleResponseDto>.WithError(ex.Message, "BUSINESS_RULE_VIOLATION"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating vehicle");
                return StatusCode(500, ApiResponse<CustomerVehicleResponseDto>.WithError("Có lỗi xảy ra", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Update vehicle
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize(Policy = "AdminOrStaff")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdateCustomerVehicleRequestDto request,
            CancellationToken ct)
        {
            if (id != request.VehicleId)
                return BadRequest(ApiResponse<CustomerVehicleResponseDto>.WithError("ID không khớp", "ID_MISMATCH"));

            var validation = await _updateValidator.ValidateAsync(request, ct);
            if (!validation.IsValid)
            {
                var errors = validation.Errors.Select(e => new
                {
                    Field = e.PropertyName,
                    Message = e.ErrorMessage
                });
                return BadRequest(ApiResponse<CustomerVehicleResponseDto>.WithValidationError(errors));
            }

            try
            {
                var userId = GetCurrentUserId();
                var result = await _service.UpdateAsync(request, userId, ct);

                _logger.LogInformation("Vehicle updated by user {UserId}: {VehicleId}",
                    userId, id);

                return Ok(ApiResponse<CustomerVehicleResponseDto>.WithSuccess(result, "Cập nhật thành công"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<CustomerVehicleResponseDto>.WithError(ex.Message, "BUSINESS_RULE_VIOLATION"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating vehicle {Id}", id);
                return StatusCode(500, ApiResponse<CustomerVehicleResponseDto>.WithError("Có lỗi xảy ra", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Delete vehicle
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            try
            {
                await _service.DeleteAsync(id, ct);

                _logger.LogInformation("Vehicle deleted by user {UserId}: {VehicleId}",
                    GetCurrentUserId(), id);

                return Ok(ApiResponse<object>.WithSuccess(null, "Xóa xe thành công"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.WithError(ex.Message, "BUSINESS_RULE_VIOLATION"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting vehicle {Id}", id);
                return StatusCode(500, ApiResponse<object>.WithError("Có lỗi xảy ra", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Check if vehicle can be deleted
        /// </summary>
        [HttpGet("{id:int}/can-delete")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> CanDelete(int id, CancellationToken ct)
        {
            try
            {
                var canDelete = await _service.CanDeleteAsync(id, ct);
                var data = new { CanDelete = canDelete, Reason = canDelete ? null : "Xe có lịch hẹn hoặc phiếu công việc liên quan" };
                return Ok(ApiResponse<object>.WithSuccess(data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking can delete {Id}", id);
                return StatusCode(500, ApiResponse<object>.WithError("Có lỗi xảy ra", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Update vehicle mileage
        /// </summary>
        [HttpPatch("{id:int}/mileage")]
        [Authorize(Policy = "AdminOrStaff")]
        public async Task<IActionResult> UpdateMileage(
            int id,
            [FromBody] UpdateMileageDto request,
            CancellationToken ct)
        {
            if (request.Mileage < 0)
                return BadRequest(ApiResponse<object>.WithError("Số km phải >= 0", "INVALID_MILEAGE"));

            try
            {
                var userId = GetCurrentUserId();
                var result = await _service.UpdateMileageAsync(id, request.Mileage, userId, ct);

                if (!result)
                    return NotFound(ApiResponse<object>.WithNotFound($"Không tìm thấy xe {id}"));

                _logger.LogInformation("Vehicle mileage updated by user {UserId}: {VehicleId} -> {Mileage} km",
                    userId, id, request.Mileage);

                return Ok(ApiResponse<object>.WithSuccess(null, "Cập nhật số km thành công"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.WithError(ex.Message, "BUSINESS_RULE_VIOLATION"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating mileage for vehicle {Id}", id);
                return StatusCode(500, ApiResponse<object>.WithError("Có lỗi xảy ra", "INTERNAL_ERROR", 500));
            }
        }
    }

    // Helper DTO
    public class UpdateMileageDto
    {
        public int Mileage { get; set; }
    }
}