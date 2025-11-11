namespace EVServiceCenter.Core.Domains.WorkOrders.DTOs.Responses;

/// <summary>
/// Response DTO for vehicle health metrics
/// </summary>
public class VehicleHealthMetricResponseDto
{
    public int MetricId { get; set; }
    public int VehicleId { get; set; }
    public string VehiclePlate { get; set; } = null!;
    public string VehicleModel { get; set; } = null!;

    public int? WorkOrderId { get; set; }
    public string? WorkOrderCode { get; set; }

    public DateOnly MetricDate { get; set; }

    // Health metrics (0-100 scale)
    public decimal? BatteryHealth { get; set; }
    public decimal? MotorEfficiency { get; set; }
    public decimal? BrakeWear { get; set; }
    public decimal? TireWear { get; set; }
    public decimal? OverallCondition { get; set; }

    // Diagnostics
    public string? DiagnosticCodes { get; set; }
    public string? Recommendations { get; set; }
    public DateOnly? NextCheckDue { get; set; }

    // Audit
    public int? CreatedBy { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Health status indicator based on overall condition
    /// </summary>
    public string HealthStatus =>
        OverallCondition switch
        {
            >= 80 => "Excellent",
            >= 60 => "Good",
            >= 40 => "Fair",
            >= 20 => "Poor",
            _ => "Critical"
        };

    /// <summary>
    /// List of individual diagnostics if any codes exist
    /// </summary>
    public List<string> DiagnosticCodeList =>
        string.IsNullOrWhiteSpace(DiagnosticCodes)
            ? new List<string>()
            : DiagnosticCodes.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
}
