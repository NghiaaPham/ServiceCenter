using System.ComponentModel.DataAnnotations;

namespace EVServiceCenter.Core.Domains.WorkOrders.DTOs.Requests;

/// <summary>
/// Request DTO for recording vehicle health metrics
/// Captures EV-specific health indicators during service
/// </summary>
public class RecordVehicleHealthMetricRequestDto
{
    /// <summary>
    /// Vehicle ID
    /// </summary>
    [Required(ErrorMessage = "Vehicle ID is required")]
    public int VehicleId { get; set; }

    /// <summary>
    /// Related work order ID (optional)
    /// </summary>
    public int? WorkOrderId { get; set; }

    /// <summary>
    /// Battery health percentage (0-100)
    /// </summary>
    [Range(0, 100, ErrorMessage = "Battery health must be between 0 and 100")]
    public decimal? BatteryHealth { get; set; }

    /// <summary>
    /// Motor efficiency percentage (0-100)
    /// </summary>
    [Range(0, 100, ErrorMessage = "Motor efficiency must be between 0 and 100")]
    public decimal? MotorEfficiency { get; set; }

    /// <summary>
    /// Brake wear percentage (0-100)
    /// </summary>
    [Range(0, 100, ErrorMessage = "Brake wear must be between 0 and 100")]
    public decimal? BrakeWear { get; set; }

    /// <summary>
    /// Tire wear percentage (0-100)
    /// </summary>
    [Range(0, 100, ErrorMessage = "Tire wear must be between 0 and 100")]
    public decimal? TireWear { get; set; }

    /// <summary>
    /// Overall vehicle condition percentage (0-100)
    /// Calculated or manually entered
    /// </summary>
    [Range(0, 100, ErrorMessage = "Overall condition must be between 0 and 100")]
    public decimal? OverallCondition { get; set; }

    /// <summary>
    /// Diagnostic trouble codes (DTCs) found during inspection
    /// Format: comma-separated codes (e.g., "P0A0F,B1234")
    /// </summary>
    [StringLength(500)]
    public string? DiagnosticCodes { get; set; }

    /// <summary>
    /// Technician recommendations based on metrics
    /// </summary>
    [StringLength(1000)]
    public string? Recommendations { get; set; }

    /// <summary>
    /// When the next health check is due
    /// </summary>
    public DateOnly? NextCheckDue { get; set; }
}
