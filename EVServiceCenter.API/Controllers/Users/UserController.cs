using EVServiceCenter.Core.Domains.Identity.DTOs.Requests;
using EVServiceCenter.Core.Domains.Identity.DTOs.Responses;
using EVServiceCenter.Core.Domains.Identity.Entities;
using EVServiceCenter.Core.Domains.Identity.Interfaces;
using EVServiceCenter.Core.Domains.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVServiceCenter.API.Controllers.Users
{
    [Authorize]
    [Route("api/users")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Admin - Users")]
    public class UserController : BaseController
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(
            IUserService userService,
            ILogger<UserController> logger)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                return Ok(new ApiResponse<IEnumerable<UserResponseDto>>
                {
                    Success = true,
                    Message = "Users retrieved successfully",
                    Data = users
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all users");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Internal server error",
                    ErrorCode = "INTERNAL_ERROR"
                });
            }
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "Authenticated")]
        public async Task<IActionResult> GetUserById(int id)
        {
            try
            {
                if (!CanAccessUser(id))
                {
                    return StatusCode(403, new ApiResponse<object>
                    {
                        Success = false,
                        Message = "You can only access your own information",
                        ErrorCode = "ACCESS_DENIED"
                    });
                }

                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {  
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "User not found",
                        ErrorCode = "USER_NOT_FOUND"
                    });
                }

                return Ok(new ApiResponse<UserResponseDto>
                {
                    Success = true,
                    Message = "User retrieved successfully",
                    Data = user
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user {UserId}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Internal server error",
                    ErrorCode = "INTERNAL_ERROR"
                });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "Authenticated")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequestDto updateRequest)
        {
            if (!IsValidRequest(updateRequest) || id != updateRequest.UserId)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid user data or ID mismatch",
                    ErrorCode = "VALIDATION_ERROR"
                });
            }

            try
            {
                if (!CanModifyUser(id))
                {
                    return StatusCode(403, new ApiResponse<object>
                    {
                        Success = false,
                        Message = "You can only update your own information",
                        ErrorCode = "ACCESS_DENIED"
                    });
                }

                if (updateRequest.RoleId.HasValue && !IsAdmin())
                {
                    return StatusCode(403, new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Only administrators can change user roles",
                        ErrorCode = "ROLE_CHANGE_DENIED"
                    });
                }

                var existingUser = await _userService.GetUserByIdAsync(id);
                if (existingUser == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "User not found",
                        ErrorCode = "USER_NOT_FOUND"
                    });
                }

                var userToUpdate = new User
                {
                    UserId = updateRequest.UserId,
                    Username = updateRequest.Username,
                    FullName = updateRequest.FullName,
                    Email = updateRequest.Email,
                    PhoneNumber = updateRequest.PhoneNumber,
                    Department = updateRequest.Department,
                    IsActive = updateRequest.IsActive,
                    RoleId = updateRequest.RoleId ?? existingUser.RoleId,
                    PasswordHash = new byte[1],
                    PasswordSalt = new byte[1]
                };

                var updatedUser = await _userService.UpdateUserAsync(userToUpdate);
                return Ok(new ApiResponse<UserResponseDto>
                {
                    Success = true,
                    Message = "User updated successfully",
                    Data = updatedUser
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Internal server error",
                    ErrorCode = "INTERNAL_ERROR"
                });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var result = await _userService.DeleteUserAsync(id);
                if (!result)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "User not found",
                        ErrorCode = "USER_NOT_FOUND"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "User deleted successfully",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Internal server error",
                    ErrorCode = "INTERNAL_ERROR"
                });
            }
        } 
    }
}