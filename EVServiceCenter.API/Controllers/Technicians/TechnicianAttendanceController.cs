using EVServiceCenter.Core.Domains.Shared.Models;
using EVServiceCenter.Core.Domains.TechnicianManagement.DTOs.Requests;
using EVServiceCenter.Core.Domains.TechnicianManagement.DTOs.Responses;
using EVServiceCenter.Core.Domains.TechnicianManagement.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVServiceCenter.API.Controllers.Technicians
{
    /// <summary>
    /// Technician Attendance Management API
    /// Handles check-in, check-out, and shift tracking
    /// </summary>
    [ApiController]
    [Route("api/technicians/attendance")]
    [Authorize(Policy = "TechnicianOnly")]
    [ApiExplorerSettings(GroupName = "Technician - Attendance")]
    public class TechnicianAttendanceController : BaseController
    {
        private readonly IShiftService _shiftService;
        private readonly ILogger<TechnicianAttendanceController> _logger;

        public TechnicianAttendanceController(
            IShiftService shiftService,
            ILogger<TechnicianAttendanceController> logger)
        {
            _shiftService = shiftService ?? throw new ArgumentNullException(nameof(shiftService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// [Check-in] Check-in for shift
        /// </summary>
        /// <remarks>
        /// Technician checks in at the start of shift.
        ///
        /// **Business Rules:**
        /// - Can only check-in once per day
        /// - Must have a schedule for today
        /// - System tracks if late (grace period: 15 minutes)
        /// - Cannot check-in if marked unavailable (time-off)
        ///
        /// **Request:**
        /// ```json
        /// {
        ///   "serviceCenterId": 1,
        ///   "shiftType": "Morning",
        ///   "notes": "Arrived early"
        /// }
        /// ```
        ///
        /// **Response:**
        /// - ShiftId
        /// - CheckInTime
        /// - IsLate flag
        /// - Scheduled times for reference
        ///
        /// **Status Codes:**
        /// - 200 OK: Check-in successful
        /// - 400 Bad Request: Already checked in or invalid data
        /// - 404 Not Found: No schedule found
        /// </remarks>
        /// <param name="request">Check-in details</param>
        /// <returns>Shift information with check-in time</returns>
        [HttpPost("check-in")]
        [ProducesResponseType(typeof(ApiResponse<ShiftResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> CheckIn([FromBody] CheckInRequestDto request)
        {
            if (!IsValidRequest(request))
            {
                return ValidationError("Invalid check-in data");
            }

            try
            {
                var technicianId = GetCurrentUserId();
                var result = await _shiftService.CheckInAsync(technicianId, request);

                _logger.LogInformation(
                    "Technician {TechnicianId} checked in successfully. ShiftId={ShiftId}, IsLate={IsLate}",
                    technicianId, result.ShiftId, result.IsLate);

                var message = result.IsLate
                    ? $"Checked in successfully (Late by {result.LateMinutes} minutes)"
                    : "Checked in successfully";

                return Success(result, message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid check-in attempt by user {UserId}", GetCurrentUserId());
                return ValidationError(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during check-in for user {UserId}", GetCurrentUserId());
                return ServerError("An error occurred during check-in");
            }
        }

        /// <summary>
        /// [Check-out] Check-out from shift
        /// </summary>
        /// <remarks>
        /// Technician checks out at the end of shift.
        ///
        /// **Business Rules:**
        /// - Must have checked in first
        /// - Cannot check-out twice
        /// - System tracks if early leave
        /// - Automatically calculates worked hours
        ///
        /// **Request:**
        /// ```json
        /// {
        ///   "notes": "Leaving early for medical appointment",
        ///   "earlyCheckoutReason": "Doctor appointment"
        /// }
        /// ```
        ///
        /// **Response:**
        /// - WorkedHours
        /// - NetWorkingHours (after break deduction)
        /// - IsEarlyLeave flag
        ///
        /// **Status Codes:**
        /// - 200 OK: Check-out successful
        /// - 400 Bad Request: Not checked in or already checked out
        /// </remarks>
        /// <param name="request">Check-out details</param>
        /// <returns>Completed shift information</returns>
        [HttpPost("check-out")]
        [ProducesResponseType(typeof(ApiResponse<ShiftResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> CheckOut([FromBody] CheckOutRequestDto request)
        {
            try
            {
                var technicianId = GetCurrentUserId();
                var result = await _shiftService.CheckOutAsync(technicianId, request);

                _logger.LogInformation(
                    "Technician {TechnicianId} checked out successfully. ShiftId={ShiftId}, WorkedHours={WorkedHours}",
                    technicianId, result.ShiftId, result.WorkedHours);

                var message = result.IsEarlyLeave
                    ? $"Checked out successfully (Early leave: {result.WorkedHours:F2}h worked)"
                    : $"Checked out successfully ({result.WorkedHours:F2}h worked)";

                return Success(result, message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid check-out attempt by user {UserId}", GetCurrentUserId());
                return ValidationError(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during check-out for user {UserId}", GetCurrentUserId());
                return ServerError("An error occurred during check-out");
            }
        }

        /// <summary>
        /// [View] Get today's shift
        /// </summary>
        /// <remarks>
        /// View current shift status.
        ///
        /// **Use cases:**
        /// - Check if already checked in
        /// - View shift details before starting work
        /// - See worked hours so far
        ///
        /// **Response includes:**
        /// - Check-in time (if checked in)
        /// - Check-out time (if completed)
        /// - Scheduled times
        /// - Late/Early flags
        /// - Current status
        /// </remarks>
        /// <returns>Today's shift or 404 if not found</returns>
        [HttpGet("today")]
        [ProducesResponseType(typeof(ApiResponse<ShiftResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetTodayShift()
        {
            try
            {
                var technicianId = GetCurrentUserId();
                var result = await _shiftService.GetTodayShiftAsync(technicianId);

                if (result == null)
                {
                    return NotFoundError("No shift found for today");
                }

                return Success(result, "Today's shift retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving today's shift for user {UserId}", GetCurrentUserId());
                return ServerError("An error occurred");
            }
        }

        /// <summary>
        /// [View] Get my shift history
        /// </summary>
        /// <remarks>
        /// View attendance history for date range.
        ///
        /// **Default range:** Last 30 days
        ///
        /// **Query params:**
        /// - from: Start date (YYYY-MM-DD)
        /// - to: End date (YYYY-MM-DD)
        ///
        /// **Use cases:**
        /// - Review attendance record
        /// - Check worked hours
        /// - Track late/early instances
        /// </remarks>
        /// <param name="from">Start date (default: 30 days ago)</param>
        /// <param name="to">End date (default: today)</param>
        /// <returns>List of shifts in date range</returns>
        [HttpGet("my-shifts")]
        [ProducesResponseType(typeof(ApiResponse<List<ShiftResponseDto>>), 200)]
        public async Task<IActionResult> GetMyShifts(
            [FromQuery] DateOnly? from = null,
            [FromQuery] DateOnly? to = null)
        {
            try
            {
                var technicianId = GetCurrentUserId();
                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                
                var fromDate = from ?? today.AddDays(-30);
                var toDate = to ?? today;

                if (fromDate > toDate)
                {
                    return ValidationError("'from' date must be before 'to' date");
                }

                var result = await _shiftService.GetShiftHistoryAsync(technicianId, fromDate, toDate);

                return Success(result, $"Retrieved {result.Count} shifts");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving shift history for user {UserId}", GetCurrentUserId());
                return ServerError("An error occurred");
            }
        }
    }
}
