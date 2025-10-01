namespace EVServiceCenter.Core.Domains.CustomerVehicles.DTOs.Requests
{
    public class CreateCustomerVehicleRequestDto
    {
        public int CustomerId { get; set; }
        public int ModelId { get; set; }
        public string LicensePlate { get; set; } = string.Empty;
        public string? Vin { get; set; }
        public string? Color { get; set; }
        public DateOnly? PurchaseDate { get; set; }
        public int? Mileage { get; set; }
        public DateOnly? LastMaintenanceDate { get; set; }
        public DateOnly? NextMaintenanceDate { get; set; }
        public int? LastMaintenanceMileage { get; set; }
        public int? NextMaintenanceMileage { get; set; }
        public decimal? BatteryHealthPercent { get; set; }
        public string? VehicleCondition { get; set; }
        public string? InsuranceNumber { get; set; }
        public DateOnly? InsuranceExpiry { get; set; }
        public DateOnly? RegistrationExpiry { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; } = true;
    }
}