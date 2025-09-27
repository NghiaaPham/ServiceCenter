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
        // User Context Helper Methods
        protected int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        protected string GetCurrentUserName()
        {
            return User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
        }

        protected string GetCurrentRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }

        // Role Check Helper Methods
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
            return role == UserRoles.Admin.ToString() ||
                   role == UserRoles.Staff.ToString() ||
                   role == UserRoles.Technician.ToString();
        }

        // Authorization Helper Methods
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

        // Standard API Response Methods
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

        protected IActionResult ValidationError(string message = null)
        {
            return BadRequest(new
            {
                Success = false,
                Error = message ?? ErrorMessages.VALIDATION_ERROR,
                Timestamp = DateTime.UtcNow
            });
        }

        protected IActionResult NotFoundError(string message = null)
        {
            return NotFound(new
            {
                Success = false,
                Error = message ?? "Resource not found",
                Timestamp = DateTime.UtcNow
            });
        }

        protected IActionResult UnauthorizedError(string message = null)
        {
            return Unauthorized(new
            {
                Success = false,
                Error = message ?? "Unauthorized access",
                Timestamp = DateTime.UtcNow
            });
        }

        protected IActionResult ForbiddenError(string message = null)
        {
            return StatusCode(403, new
            {
                Success = false,
                Error = message ?? ErrorMessages.ACCESS_DENIED,
                Timestamp = DateTime.UtcNow
            });
        }

        protected IActionResult ServerError(string message = null)
        {
            return StatusCode(500, new
            {
                Success = false,
                Error = message ?? ErrorMessages.OPERATION_FAILED,
                Timestamp = DateTime.UtcNow
            });
        }

        // Exception handling helper
        protected IActionResult HandleException(Exception ex)
        {
            // Log exception here if needed
            return ServerError(ex.Message);
        }

        // Validation helper
        protected bool IsValidRequest<T>(T request) where T : class
        {
            return request != null && ModelState.IsValid;
        }
    }
}