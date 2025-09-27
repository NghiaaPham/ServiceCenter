namespace EVServiceCenter.Core.Domains.Customers.DTOs.Responses
{
    public class CustomerVehicleSummaryDto
    {
        public int VehicleId { get; set; }
        public string LicensePlate { get; set; } = string.Empty;
        public string ModelName { get; set; } = string.Empty;
        public string BrandName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsMaintenanceDue { get; set; }
    }
}
