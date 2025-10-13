using EVServiceCenter.API.Controllers;
using EVServiceCenter.Core.Domains.CustomerVehicles.DTOs.Request;
using EVServiceCenter.Core.Domains.CustomerVehicles.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVServiceCenter.API.Controllers.CustomerVehicles;

/// <summary>
/// API endpoints cho Smart Maintenance Reminder
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "CustomerOnly")]
public class VehicleMaintenanceController : BaseController
{
    private readonly IVehicleMaintenanceService _maintenanceService;

    public VehicleMaintenanceController(IVehicleMaintenanceService maintenanceService)
    {
        _maintenanceService = maintenanceService;
    }

    /// <summary>
    /// Lấy trạng thái bảo dưỡng của xe (với ước tính thông minh)
    /// </summary>
    /// <param name="vehicleId">ID của xe</param>
    /// <response code="200">Trả về trạng thái bảo dưỡng với km ước tính</response>
    /// <response code="404">Không tìm thấy xe</response>
    [HttpGet("{vehicleId}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVehicleMaintenanceStatus(int vehicleId)
    {
        try
        {
            var status = await _maintenanceService.GetVehicleMaintenanceStatusAsync(vehicleId);

            return Ok(new
            {
                success = true,
                message = "Lấy trạng thái bảo dưỡng thành công",
                data = status
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                success = false,
                message = ex.Message
            });
        }
    }

    /// <summary>
    /// Lấy trạng thái bảo dưỡng cho tất cả xe của khách hàng đang đăng nhập
    /// </summary>
    /// <response code="200">Trả về danh sách trạng thái bảo dưỡng các xe</response>
    [HttpGet("my-vehicles/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyVehiclesMaintenanceStatus()
    {
        var customerId = GetCurrentCustomerId();

        if (customerId == 0)
        {
            return BadRequest(new
            {
                success = false,
                message = "Không tìm thấy thông tin khách hàng"
            });
        }

        var statusList = await _maintenanceService.GetCustomerVehiclesMaintenanceStatusAsync(
            customerId);

        return Ok(new
        {
            success = true,
            message = $"Lấy trạng thái bảo dưỡng thành công cho {statusList.Count} xe",
            data = statusList
        });
    }

    /// <summary>
    /// Lấy lịch sử bảo dưỡng của xe
    /// </summary>
    /// <param name="vehicleId">ID của xe</param>
    /// <response code="200">Trả về lịch sử bảo dưỡng</response>
    [HttpGet("{vehicleId}/history")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVehicleMaintenanceHistory(int vehicleId)
    {
        var history = await _maintenanceService.GetVehicleMaintenanceHistoryAsync(vehicleId);

        return Ok(new
        {
            success = true,
            message = $"Lấy lịch sử bảo dưỡng thành công ({history.Count} lần)",
            data = history
        });
    }

    /// <summary>
    /// Cập nhật km hiện tại của xe (thủ công)
    /// </summary>
    /// <param name="vehicleId">ID của xe</param>
    /// <param name="request">Dữ liệu km cập nhật</param>
    /// <response code="200">Cập nhật thành công</response>
    /// <response code="400">Dữ liệu không hợp lệ</response>
    [HttpPut("{vehicleId}/mileage")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateVehicleMileage(
        int vehicleId,
        [FromBody] UpdateVehicleMileageRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            await _maintenanceService.UpdateVehicleMileageAsync(
                vehicleId,
                request.CurrentMileage,
                request.Notes);

            return Ok(new
            {
                success = true,
                message = "Cập nhật km thành công",
                data = new
                {
                    vehicleId,
                    currentMileage = request.CurrentMileage,
                    updatedAt = DateTime.Now
                }
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                success = false,
                message = ex.Message
            });
        }
    }

    /// <summary>
    /// Lấy thông báo nhắc nhở cho khách hàng (những xe sắp đến hạn bảo dưỡng)
    /// </summary>
    /// <response code="200">Trả về danh sách xe cần bảo dưỡng</response>
    [HttpGet("reminders")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMaintenanceReminders()
    {
        var customerId = GetCurrentCustomerId();

        if (customerId == 0)
        {
            return BadRequest(new
            {
                success = false,
                message = "Không tìm thấy thông tin khách hàng"
            });
        }

        var statusList = await _maintenanceService.GetCustomerVehiclesMaintenanceStatusAsync(
            customerId);

        // Lọc ra những xe cần chú ý hoặc urgent
        var reminders = statusList
            .Where(s => s.Status == "NeedAttention" || s.Status == "Urgent")
            .OrderBy(s => s.RemainingKm)
            .ToList();

        return Ok(new
        {
            success = true,
            message = reminders.Count > 0
                ? $"Bạn có {reminders.Count} xe cần bảo dưỡng sớm"
                : "Tất cả xe của bạn đều trong tình trạng tốt",
            data = reminders,
            summary = new
            {
                totalVehicles = statusList.Count,
                needsAttention = statusList.Count(s => s.Status == "NeedAttention"),
                urgent = statusList.Count(s => s.Status == "Urgent"),
                normal = statusList.Count(s => s.Status == "Normal")
            }
        });
    }
}
