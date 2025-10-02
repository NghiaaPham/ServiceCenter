using EVServiceCenter.Core.Domains.Shared.Models;
using EVServiceCenter.Core.Domains.TimeSlots.DTOs.Requests;
using EVServiceCenter.Core.Domains.TimeSlots.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVServiceCenter.API.Controllers.TimeSlots
{
    [ApiController]
    [Route("api/timeslots")]
    [Authorize(Roles = "Admin,Staff")]
    public class TimeSlotCommandController : ControllerBase
    {
        private readonly ITimeSlotCommandService _commandService;
        private readonly IValidator<CreateTimeSlotRequestDto> _createValidator;
        private readonly IValidator<UpdateTimeSlotRequestDto> _updateValidator;
        private readonly IValidator<GenerateSlotsRequestDto> _generateValidator;

        public TimeSlotCommandController(
            ITimeSlotCommandService commandService,
            IValidator<CreateTimeSlotRequestDto> createValidator,
            IValidator<UpdateTimeSlotRequestDto> updateValidator,
            IValidator<GenerateSlotsRequestDto> generateValidator)
        {
            _commandService = commandService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _generateValidator = generateValidator;
        }

        /// <summary>
        /// Create a new timeslot
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateTimeSlotRequestDto request,
            CancellationToken ct)
        {
            var validation = await _createValidator.ValidateAsync(request, ct);
            if (!validation.IsValid)
            {
                var errors = validation.Errors.Select(e => new
                {
                    field = e.PropertyName,
                    message = e.ErrorMessage
                });

                return BadRequest(ApiResponse<object>.WithValidationError(
                    errors,
                    "Dữ liệu không hợp lệ"
                ));
            }

            try
            {
                var result = await _commandService.CreateAsync(request, ct);

                return CreatedAtAction(
                    nameof(TimeSlotQueryController.GetById),
                    "TimeSlotQuery",
                    new { slotId = result.SlotId },
                    ApiResponse<object>.WithSuccess(result, "Tạo timeslot thành công", 201)
                );
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.WithError(
                    ex.Message,
                    "BUSINESS_RULE_VIOLATION"
                ));
            }
        }

        /// <summary>
        /// Update an existing timeslot
        /// </summary>
        [HttpPut("{slotId:int}")]
        public async Task<IActionResult> Update(
            int slotId,
            [FromBody] UpdateTimeSlotRequestDto request,
            CancellationToken ct)
        {
            if (slotId != request.SlotId)
            {
                return BadRequest(ApiResponse<object>.WithError(
                    "SlotId không khớp",
                    "INVALID_PARAMETER"
                ));
            }

            var validation = await _updateValidator.ValidateAsync(request, ct);
            if (!validation.IsValid)
            {
                var errors = validation.Errors.Select(e => new
                {
                    field = e.PropertyName,
                    message = e.ErrorMessage
                });

                return BadRequest(ApiResponse<object>.WithValidationError(
                    errors,
                    "Dữ liệu không hợp lệ"
                ));
            }

            try
            {
                var result = await _commandService.UpdateAsync(request, ct);

                return Ok(ApiResponse<object>.WithSuccess(
                    result,
                    "Cập nhật timeslot thành công"
                ));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.WithError(
                    ex.Message,
                    "BUSINESS_RULE_VIOLATION"
                ));
            }
        }

        /// <summary>
        /// Delete a timeslot (only if no bookings)
        /// </summary>
        [HttpDelete("{slotId:int}")]
        public async Task<IActionResult> Delete(int slotId, CancellationToken ct)
        {
            try
            {
                var result = await _commandService.DeleteAsync(slotId, ct);

                if (!result)
                {
                    return NotFound(ApiResponse<object>.WithNotFound(
                        $"Không tìm thấy timeslot {slotId}"
                    ));
                }

                return Ok(ApiResponse<object>.WithSuccess(
                    new { slotId },
                    "Xóa timeslot thành công"
                ));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.WithError(
                    ex.Message,
                    "BUSINESS_RULE_VIOLATION"
                ));
            }
        }

        /// <summary>
        /// Generate timeslots automatically for a date range
        /// </summary>
        [HttpPost("generate")]
        public async Task<IActionResult> GenerateSlots(
            [FromBody] GenerateSlotsRequestDto request,
            CancellationToken ct)
        {
            var validation = await _generateValidator.ValidateAsync(request, ct);
            if (!validation.IsValid)
            {
                var errors = validation.Errors.Select(e => new
                {
                    field = e.PropertyName,
                    message = e.ErrorMessage
                });

                return BadRequest(ApiResponse<object>.WithValidationError(
                    errors,
                    "Dữ liệu không hợp lệ"
                ));
            }

            try
            {
                var count = await _commandService.GenerateSlotsAsync(request, ct);

                return Ok(ApiResponse<object>.WithSuccess(
                    new { generatedCount = count },
                    $"Đã tạo {count} timeslots"
                ));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.WithError(
                    ex.Message,
                    "BUSINESS_RULE_VIOLATION"
                ));
            }
        }

        /// <summary>
        /// Delete all empty slots for a specific date
        /// </summary>
        [HttpDelete("center/{centerId:int}/date/{date}/empty")]
        public async Task<IActionResult> DeleteEmptySlots(
            int centerId,
            DateOnly date,
            CancellationToken ct)
        {
            var count = await _commandService.DeleteEmptySlotsAsync(centerId, date, ct);

            return Ok(ApiResponse<object>.WithSuccess(
                new { deletedCount = count },
                $"Đã xóa {count} slots trống"
            ));
        }

        /// <summary>
        /// Block/Unblock a timeslot
        /// </summary>
        [HttpPatch("{slotId:int}/block")]
        public async Task<IActionResult> ToggleBlock(
    int slotId,
    [FromBody] BlockSlotRequest request,
    CancellationToken ct)
        {
            try
            {
                var result = await _commandService.ToggleBlockAsync(slotId, request.IsBlocked, ct);

                var message = request.IsBlocked ? "Đã block slot" : "Đã unblock slot";

                return Ok(ApiResponse<object>.WithSuccess(result, message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.WithError(
                    ex.Message,
                    "BUSINESS_RULE_VIOLATION"
                ));
            }
        }
    }
}