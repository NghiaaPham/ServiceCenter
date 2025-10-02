using EVServiceCenter.Core.Domains.Shared.Models;
using EVServiceCenter.Core.Domains.TimeSlots.DTOs.Requests;
using EVServiceCenter.Core.Domains.TimeSlots.Interfaces.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVServiceCenter.API.Controllers.TimeSlots
{
    [ApiController]
    [Route("api/timeslots")]
    public class TimeSlotQueryController : ControllerBase
    {
        private readonly ITimeSlotQueryService _queryService;
        private readonly IValidator<TimeSlotQueryDto> _queryValidator;

        public TimeSlotQueryController(
            ITimeSlotQueryService queryService,
            IValidator<TimeSlotQueryDto> queryValidator)
        {
            _queryService = queryService;
            _queryValidator = queryValidator;
        }

        /// <summary>
        /// Get all timeslots with filtering and pagination
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll(
            [FromQuery] TimeSlotQueryDto query,
            CancellationToken ct)
        {
            var validation = await _queryValidator.ValidateAsync(query, ct);
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

            var result = await _queryService.GetAllAsync(query, ct);

            return Ok(ApiResponse<object>.WithSuccess(
                result,
                $"Tìm thấy {result.TotalCount} timeslots"
            ));
        }

        /// <summary>
        /// Get timeslot by ID
        /// </summary>
        [HttpGet("{slotId:int}")]
        [Authorize]
        public async Task<IActionResult> GetById(int slotId, CancellationToken ct)
        {
            var result = await _queryService.GetByIdAsync(slotId, ct);

            if (result == null)
            {
                return NotFound(ApiResponse<object>.WithNotFound(
                    $"Không tìm thấy timeslot {slotId}"
                ));
            }

            return Ok(ApiResponse<object>.WithSuccess(result));
        }

        /// <summary>
        /// Get available slots for a specific center and date
        /// </summary>
        [HttpGet("available")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvailableSlots(
            [FromQuery] int centerId,
            [FromQuery] DateOnly date,
            CancellationToken ct)
        {
            if (centerId <= 0)
            {
                return BadRequest(ApiResponse<object>.WithError(
                    "CenterId không hợp lệ",
                    "INVALID_PARAMETER"
                ));
            }

            var result = await _queryService.GetAvailableSlotsAsync(centerId, date, ct);
            var resultList = result.ToList();

            return Ok(ApiResponse<object>.WithSuccess(
                resultList,
                $"Tìm thấy {resultList.Count} slots khả dụng"
            ));
        }

        /// <summary>
        /// Get available slots for a date range
        /// </summary>
        [HttpGet("available/range")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvailableSlotsByDateRange(
            [FromQuery] int centerId,
            [FromQuery] DateOnly startDate,
            [FromQuery] DateOnly endDate,
            CancellationToken ct)
        {
            if (centerId <= 0)
            {
                return BadRequest(ApiResponse<object>.WithError(
                    "CenterId không hợp lệ",
                    "INVALID_PARAMETER"
                ));
            }

            if (endDate < startDate)
            {
                return BadRequest(ApiResponse<object>.WithError(
                    "EndDate phải sau StartDate",
                    "INVALID_DATE_RANGE"
                ));
            }

            var result = await _queryService.GetAvailableSlotsByDateRangeAsync(
                centerId, startDate, endDate, ct);
            var resultList = result.ToList();

            return Ok(ApiResponse<object>.WithSuccess(
                resultList,
                $"Tìm thấy {resultList.Count} slots khả dụng"
            ));
        }

        /// <summary>
        /// Check if a specific slot is available
        /// </summary>
        [HttpGet("{slotId:int}/availability")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckAvailability(int slotId, CancellationToken ct)
        {
            var isAvailable = await _queryService.IsSlotAvailableAsync(slotId, ct);

            return Ok(ApiResponse<object>.WithSuccess(
                new { slotId, isAvailable },
                isAvailable ? "Slot khả dụng" : "Slot không khả dụng"
            ));
        }

        /// <summary>
        /// Get slots by center and date
        /// </summary>
        [HttpGet("center/{centerId:int}/date/{date}")]
        [Authorize]
        public async Task<IActionResult> GetSlotsByCenterAndDate(
            int centerId,
            DateOnly date,
            CancellationToken ct)
        {
            var result = await _queryService.GetSlotsByCenterAndDateAsync(centerId, date, ct);
            var resultList = result.ToList();

            return Ok(ApiResponse<object>.WithSuccess(
                resultList,
                $"Tìm thấy {resultList.Count} slots"
            ));
        }

        /// <summary>
        /// Get booking count for a slot
        /// </summary>
        [HttpGet("{slotId:int}/bookings/count")]
        [Authorize]
        public async Task<IActionResult> GetBookingCount(int slotId, CancellationToken ct)
        {
            var count = await _queryService.GetBookingCountAsync(slotId, ct);

            return Ok(ApiResponse<object>.WithSuccess(
                new { slotId, bookingCount = count }
            ));
        }
    }
}