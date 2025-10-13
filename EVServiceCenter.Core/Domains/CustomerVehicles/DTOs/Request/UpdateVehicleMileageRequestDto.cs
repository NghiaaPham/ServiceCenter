using System.ComponentModel.DataAnnotations;

namespace EVServiceCenter.Core.Domains.CustomerVehicles.DTOs.Request;

/// <summary>
/// DTO để cập nhật km hiện tại của xe (khi khách hàng muốn cập nhật thủ công)
/// </summary>
public class UpdateVehicleMileageRequestDto
{
    [Required]
    [Range(0, 999999)]
    public decimal CurrentMileage { get; set; }

    public string? Notes { get; set; }
}
