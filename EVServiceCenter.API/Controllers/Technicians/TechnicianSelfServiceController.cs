using EVServiceCenter.API.Controllers;
using EVServiceCenter.Core.Domains.Shared.Models;
using EVServiceCenter.Core.Domains.TechnicianManagement.DTOs.Requests;
using EVServiceCenter.Core.Domains.TechnicianManagement.DTOs.Responses;
using EVServiceCenter.Core.Domains.TechnicianManagement.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVServiceCenter.API.Controllers.Technicians
{
    /// <summary>
    /// Technician Self-Service Portal
    /// Technicians can view their schedule, work orders, performance, and ratings
    /// </summary>
    [ApiController]
    [Route("api/technicians")]
    [Authorize(Policy = "TechnicianOnly")] // Only technicians can access
    [ApiExplorerSettings(GroupName = "Technician - Self Service")]
    public class TechnicianSelfServiceController : BaseController
    {
        private readonly ITechnicianService _technicianService;
        private readonly ILogger<TechnicianSelfServiceController> _logger;

        public TechnicianSelfServiceController(
            ITechnicianService technicianService,
            ILogger<TechnicianSelfServiceController> logger)
        {
            _technicianService = technicianService ?? throw new ArgumentNullException(nameof(technicianService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// [My Schedule] Xem l?ch làm vi?c c?a tôi
        /// </summary>
        /// <remarks>
        /// Technician xem l?ch làm vi?c c?a mình trong kho?ng th?i gian.
        /// 
        /// **Default:** Next 7 days from today
        /// 
        /// **Hi?n th?:**
        /// - Work date, shift time (start/end)
        /// - Break time
        /// - Capacity (max, booked, available minutes)
        /// - Shift type (Morning/Afternoon/FullDay)
        /// - Service center assignment
        /// 
        /// **Use cases:**
        /// - Xem l?ch tu?n này
        /// - Plan công vi?c cá nhân
        /// - Check availability
        /// </remarks>
        /// <param name="startDate">Start date (yyyy-MM-dd), default: today</param>
        /// <param name="endDate">End date (yyyy-MM-dd), default: today + 7 days</param>
        /// <returns>List of schedules</returns>
        [HttpGet("my-schedule")]
        public async Task<IActionResult> GetMySchedule(
            [FromQuery] DateOnly? startDate = null,
            [FromQuery] DateOnly? endDate = null)
        {
            try
            {
                var technicianId = GetCurrentUserId();

                var schedules = await _technicianService.GetMyScheduleAsync(
                    technicianId, startDate, endDate);

                return Success(schedules, $"Found {schedules.Count} schedule(s)");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error getting my schedule");
                return ValidationError(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting my schedule for user {UserId}", GetCurrentUserId());
                return ServerError("Có l?i x?y ra khi l?y l?ch làm vi?c");
            }
        }

        /// <summary>
        /// [My Work Orders] Xem công vi?c c?a tôi
        /// </summary>
        /// <remarks>
        /// Technician xem danh sách work orders ???c assign cho mình.
        /// 
        /// **Default:** Last 30 days to next 7 days
        /// 
        /// **Filter options:**
        /// - Status ID (Pending, Assigned, InProgress, Completed, etc.)
        /// - Date range
        /// 
        /// **Hi?n th?:**
        /// - Work order code, customer info, vehicle info
        /// - Status, services list
        /// - Checklist progress
        /// - Estimated vs actual duration
        /// - Notes
        /// 
        /// **Use cases:**
        /// - Xem công vi?c hôm nay
        /// - Check công vi?c ?ang làm
        /// - Review l?ch s? hoàn thành
        /// </remarks>
        /// <param name="statusId">Filter by status (optional)</param>
        /// <param name="startDate">Start date (optional)</param>
        /// <param name="endDate">End date (optional)</param>
        /// <returns>List of work orders</returns>
        [HttpGet("my-work-orders")]
        public async Task<IActionResult> GetMyWorkOrders(
            [FromQuery] int? statusId = null,
            [FromQuery] DateOnly? startDate = null,
            [FromQuery] DateOnly? endDate = null)
        {
            try
            {
                var technicianId = GetCurrentUserId();

                var workOrders = await _technicianService.GetMyWorkOrdersAsync(
                    technicianId, statusId, startDate, endDate);

                return Success(workOrders, $"Found {workOrders.Count} work order(s)");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting my work orders for user {UserId}", GetCurrentUserId());
                return ServerError("Có l?i x?y ra khi l?y danh sách công vi?c");
            }
        }

        /// <summary>
        /// [My Performance] Xem hi?u su?t công vi?c c?a tôi
        /// </summary>
        /// <remarks>
        /// Technician xem performance metrics c?a mình.
        /// 
        /// **Default period:** Last 30 days
        /// 
        /// **Metrics:**
        /// - Total work orders completed
        /// - Average completion time
        /// - Customer ratings
        /// - Skill utilization
        /// - Time management (on-time completion rate)
        /// - Quality metrics
        /// 
        /// **Use cases:**
        /// - Self-review
        /// - Track progress
        /// - Identify improvement areas
        /// </remarks>
        /// <param name="periodStart">Period start date (optional)</param>
        /// <param name="periodEnd">Period end date (optional)</param>
        /// <returns>Performance metrics</returns>
        [HttpGet("my-performance")]
        public async Task<IActionResult> GetMyPerformance(
            [FromQuery] DateTime? periodStart = null,
            [FromQuery] DateTime? periodEnd = null)
        {
            try
            {
                var technicianId = GetCurrentUserId();

                var performance = await _technicianService.GetMyPerformanceAsync(
                    technicianId, periodStart, periodEnd);

                return Success(performance, "Performance metrics retrieved successfully");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error getting my performance");
                return ValidationError(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting my performance for user {UserId}", GetCurrentUserId());
                return ServerError("Có l?i x?y ra khi l?y thông tin hi?u su?t");
            }
        }

        /// <summary>
        /// [My Ratings] Xem ?ánh giá t? khách hàng
        /// </summary>
        /// <remarks>
        /// Technician xem customer ratings v? mình.
        /// 
        /// **Default:** Last 90 days
        /// 
        /// **Filter options:**
        /// - Minimum rating (1-5 stars)
        /// - Date range
        /// 
        /// **Hi?n th?:**
        /// - Overall rating, service quality, professionalism
        /// - Positive/negative feedback
        /// - Customer suggestions
        /// - Work order reference
        /// 
        /// **Use cases:**
        /// - Xem feedback t? customers
        /// - Identify strengths/weaknesses
        /// - Improve service quality
        /// </remarks>
        /// <param name="minRating">Minimum rating filter (1-5)</param>
        /// <param name="startDate">Start date (optional)</param>
        /// <param name="endDate">End date (optional)</param>
        /// <returns>List of customer ratings</returns>
        [HttpGet("my-ratings")]
        public async Task<IActionResult> GetMyRatings(
            [FromQuery] int? minRating = null,
            [FromQuery] DateOnly? startDate = null,
            [FromQuery] DateOnly? endDate = null)
        {
            try
            {
                var technicianId = GetCurrentUserId();

                var ratings = await _technicianService.GetMyRatingsAsync(
                    technicianId, minRating, startDate, endDate);

                return Success(ratings, $"Found {ratings.Count} rating(s)");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error getting my ratings");
                return ValidationError(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting my ratings for user {UserId}", GetCurrentUserId());
                return ServerError("Có l?i x?y ra khi l?y ?ánh giá");
            }
        }

        /// <summary>
        /// [Request Time Off] Yêu c?u ngh? phép
        /// </summary>
        /// <remarks>
        /// Technician request time off (vacation, sick leave, personal, emergency).
        /// 
        /// **Validation rules:**
        /// - End date must be after start date
        /// - Cannot request for past dates
        /// - Max 30 days continuous time off
        /// - Minimum 3 days notice (except emergency)
        /// 
        /// **Time off types:**
        /// - Vacation: Ngh? phép th??ng niên
        /// - Sick: Ngh? ?m
        /// - Personal: Ngh? vi?c riêng
        /// - Emergency: Ngh? kh?n c?p (không c?n báo tr??c 3 ngày)
        /// 
        /// **Process:**
        /// 1. Technician submits request
        /// 2. System marks schedule as unavailable
        /// 3. Manager reviews (future feature)
        /// 4. Notification sent
        /// 
        /// **Use cases:**
        /// - Ngh? phép
        /// - Ngh? ?m
        /// - Ngh? vi?c riêng
        /// </remarks>
        /// <param name="request">Time off request details</param>
        /// <returns>Request status</returns>
        [HttpPost("request-time-off")]
        public async Task<IActionResult> RequestTimeOff([FromBody] RequestTimeOffDto request)
        {
            if (!IsValidRequest(request))
            {
                return ValidationError("Invalid time off request data");
            }

            try
            {
                // Validate technician ID matches current user
                var currentUserId = GetCurrentUserId();
                if (request.TechnicianId != currentUserId)
                {
                    return Forbid("You can only request time off for yourself");
                }

                var result = await _technicianService.RequestTimeOffAsync(request);

                if (result)
                {
                    _logger.LogInformation(
                        "Technician {TechnicianId} requested time off from {StartDate} to {EndDate}",
                        currentUserId, request.StartDate, request.EndDate);

                    return Success(
                        new { Requested = true, StartDate = request.StartDate, EndDate = request.EndDate },
                        "Time off request submitted successfully");
                }

                return ValidationError("Failed to submit time off request");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error requesting time off");
                return ValidationError(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error requesting time off for user {UserId}", GetCurrentUserId());
                return ServerError("Có l?i x?y ra khi g?i yêu c?u ngh? phép");
            }
        }
    }
}
