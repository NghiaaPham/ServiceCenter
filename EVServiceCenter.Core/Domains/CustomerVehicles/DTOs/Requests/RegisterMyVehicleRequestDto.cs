namespace EVServiceCenter.Core.Domains.CustomerVehicles.DTOs.Requests
{
    /// <summary>
    /// DTO for customer to register their own vehicle
    /// CustomerId will be auto-assigned from authenticated user token
    /// </summary>
    public class RegisterMyVehicleRequestDto
    {
        public int ModelId { get; set; }
        public string LicensePlate { get; set; } = string.Empty;
        public string? Vin { get; set; }
        public string? Color { get; set; }
        public DateOnly? PurchaseDate { get; set; }
        public int? Mileage { get; set; }
        public string? InsuranceNumber { get; set; }
        public DateOnly? InsuranceExpiry { get; set; }
        public DateOnly? RegistrationExpiry { get; set; }
    }
}
