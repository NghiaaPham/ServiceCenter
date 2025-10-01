namespace EVServiceCenter.Core.Domains.CustomerVehicles.DTOs
{
    public class VehicleStatistics
    {
        public int TotalWorkOrders { get; set; }
        public int TotalMaintenanceRecords { get; set; }
        public decimal TotalSpentOnVehicle { get; set; }
    }
}