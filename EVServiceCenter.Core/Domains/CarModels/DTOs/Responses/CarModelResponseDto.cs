namespace EVServiceCenter.Core.Domains.CarModels.DTOs.Responses
{
    public class CarModelResponseDto
    {
        public int ModelId { get; set; }
        public int BrandId { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public string ModelName { get; set; } = string.Empty;
        public string FullModelName { get; set; } = string.Empty; // Brand + Model

        // Specifications
        public int? Year { get; set; }
        public decimal? BatteryCapacity { get; set; }
        public int? MaxRange { get; set; }
        public string? ChargingType { get; set; }
        public decimal? MotorPower { get; set; }
        public decimal? AccelerationTime { get; set; }
        public int? TopSpeed { get; set; }

        // Service info
        public int? ServiceInterval { get; set; }
        public int? ServiceIntervalMonths { get; set; }
        public int? WarrantyPeriod { get; set; }

        // Media & Status
        public string? ImageUrl { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }

        // Audit
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        // Statistics (optional)
        public int? TotalVehicles { get; set; }
        public int? ActiveVehicles { get; set; }
        public int? TotalServicesPerformed { get; set; }
    }
}