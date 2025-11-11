using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using EVServiceCenter.Core.Enums;
using EVServiceCenter.Core.Constants;

namespace EVServiceCenter.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseController : ControllerBase
    {
        // =========================
        // USER CONTEXT HELPERS
        // =========================

        /// <summary>
        /// Get current user ID from JWT token claims
        /// Checks multiple claim types for compatibility
        /// </summary>
        /// <returns>User ID</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when user ID not found in token</exception>
        protected int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                       ?? User.FindFirst("UserId")
                       ?? User.FindFirst("sub");

            if (claim != null && int.TryParse(claim.Value, out var userId))
            {
                return userId;
            }

            // Quan điểm: đã vào được controller có [Authorize] mà vẫn không có UserId trong token
            // => coi như request không hợp lệ / chưa auth đúng
            throw new UnauthorizedAccessException("User ID not found in token");
        }

        protected string GetCurrentUserName()
        {
            return User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
        }

        protected string GetCurrentRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }

        // =========================
        // ROLE CHECK HELPERS
        // =========================

        protected bool IsAdmin()
        {
            return GetCurrentRole() == UserRoles.Admin.ToString();
        }

        protected bool IsStaff()
        {
            return GetCurrentRole() == UserRoles.Staff.ToString();
        }

        protected bool IsTechnician()
        {
            return GetCurrentRole() == UserRoles.Technician.ToString();
        }

        protected bool IsCustomer()
        {
            return GetCurrentRole() == UserRoles.Customer.ToString();
        }

        protected bool IsInternal()
        {
            var role = GetCurrentRole();
            return role == UserRoles.Admin.ToString()
                   || role == UserRoles.Staff.ToString()
                   || role == UserRoles.Technician.ToString();
        }

        // =========================
        // AUTHZ HELPERS
        // =========================

        protected bool CanAccessUser(int targetUserId)
        {
            if (IsInternal()) return true;
            return GetCurrentUserId() == targetUserId;
        }

        protected bool CanModifyUser(int targetUserId)
        {
            if (IsAdmin()) return true;
            return GetCurrentUserId() == targetUserId;
        }

        // =========================
        // STANDARD API RESPONSES
        // =========================

        protected IActionResult Success<T>(T data, string message = "Success")
        {
            return Ok(new
            {
                Success = true,
                Message = message,
                Data = data,
                Timestamp = DateTime.UtcNow
            });
        }

        protected IActionResult Success(string message = "Success")
        {
            return Ok(new
            {
                Success = true,
                Message = message,
                Timestamp = DateTime.UtcNow
            });
        }

        protected IActionResult Created<T>(string actionName, object routeValues, T data, string message = "Created successfully")
        {
            return CreatedAtAction(actionName, routeValues, new
            {
                Success = true,
                Message = message,
                Data = data,
                Timestamp = DateTime.UtcNow
            });
        }

        protected IActionResult ValidationError(string? message = null)
        {
            return BadRequest(new
            {
                Success = false,
                Error = message ?? ErrorMessages.VALIDATION_ERROR,
                Timestamp = DateTime.UtcNow
            });
        }

        protected IActionResult NotFoundError(string? message = null)
        {
            return NotFound(new
            {
                Success = false,
                Error = message ?? "Resource not found",
                Timestamp = DateTime.UtcNow
            });
        }

        protected IActionResult UnauthorizedError(string? message = null)
        {
            return Unauthorized(new
            {
                Success = false,
                Error = message ?? "Unauthorized access",
                Timestamp = DateTime.UtcNow
            });
        }

        protected IActionResult ForbiddenError(string? message = null)
        {
            return StatusCode(403, new
            {
                Success = false,
                Error = message ?? ErrorMessages.ACCESS_DENIED,
                Timestamp = DateTime.UtcNow
            });
        }

        protected IActionResult ServerError(string? message = null)
        {
            return StatusCode(500, new
            {
                Success = false,
                Error = message ?? ErrorMessages.OPERATION_FAILED,
                Timestamp = DateTime.UtcNow
            });
        }


        protected IActionResult HandleException(Exception ex)
        {
            return ex switch
            {
                UnauthorizedAccessException uaEx => UnauthorizedError(uaEx.Message),
                InvalidOperationException ioEx => ValidationError(ioEx.Message),
                KeyNotFoundException knfEx => NotFoundError(knfEx.Message),
                _ => ServerError(ex.Message)
            };
        }

        // =========================
        // VALIDATION HELPER
        // =========================

        protected bool IsValidRequest<T>(T request) where T : class
        {
            return request != null && ModelState.IsValid;
        }

        // =========================
        // CUSTOMER HELPERS
        // =========================

        protected int GetCurrentCustomerId()
        {
            var customerIdClaim = User.FindFirst("CustomerId")?.Value;
            return int.TryParse(customerIdClaim, out var customerId) ? customerId : 0;
        }

        protected string GetCustomerCode()
        {
            return User.FindFirst("CustomerCode")?.Value ?? string.Empty;
        }

        protected int GetCustomerLoyaltyPoints()
        {
            var pointsClaim = User.FindFirst("LoyaltyPoints")?.Value;
            return int.TryParse(pointsClaim, out var points) ? points : 0;
        }

        protected bool IsRegisteredCustomer()
        {
            return IsCustomer() && GetCurrentCustomerId() > 0;
        }
    }
}
