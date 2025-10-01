namespace EVServiceCenter.Core.Domains.CustomerVehicles.DTOs.Responses
{
    public class CustomerVehicleResponseDto
    {
        public int VehicleId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerCode { get; set; } = string.Empty;

        public int ModelId { get; set; }
        public string ModelName { get; set; } = string.Empty;
        public int BrandId { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public string FullModelName { get; set; } = string.Empty;

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
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        public bool IsMaintenanceDue { get; set; }
        public bool IsInsuranceExpiring { get; set; }
        public bool IsRegistrationExpiring { get; set; }
        public int? DaysSinceLastMaintenance { get; set; }
        public int? DaysUntilNextMaintenance { get; set; }
        public string MaintenanceStatus { get; set; } = string.Empty;

        public int? TotalWorkOrders { get; set; }
        public int? TotalMaintenanceRecords { get; set; }
        public decimal? TotalSpentOnVehicle { get; set; }
    }
}