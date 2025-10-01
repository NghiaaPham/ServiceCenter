namespace EVServiceCenter.Core.Domains.CarModels.DTOs.Requests
{
    public class UpdateCarModelRequestDto
    {
        public int ModelId { get; set; }
        public int BrandId { get; set; }
        public string ModelName { get; set; } = string.Empty;
        public int? Year { get; set; }
        public decimal? BatteryCapacity { get; set; }
        public int? MaxRange { get; set; }
        public string? ChargingType { get; set; }
        public decimal? MotorPower { get; set; }
        public decimal? AccelerationTime { get; set; }
        public int? TopSpeed { get; set; }
        public int? ServiceInterval { get; set; }
        public int? ServiceIntervalMonths { get; set; }
        public int? WarrantyPeriod { get; set; }
        public string? ImageUrl { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }
}