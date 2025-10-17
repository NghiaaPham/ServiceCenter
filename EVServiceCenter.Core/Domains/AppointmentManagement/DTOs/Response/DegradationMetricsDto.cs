namespace EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Response
{
    /// <summary>
    /// ?? Degradation Metrics - Phân tích service degradation
    /// </summary>
    public class DegradationMetricsDto
    {
        /// <summary>
        /// T?ng s? services ???c complete
        /// </summary>
        public int TotalCompletedServices { get; set; }

        /// <summary>
        /// S? services b? degraded (Subscription ? Extra)
        /// </summary>
        public int DegradedServices { get; set; }

        /// <summary>
        /// T? l? degradation (%)
        /// </summary>
        public decimal DegradationRate { get; set; }

        /// <summary>
        /// T?ng revenue impact t? degradation (VN?)
        /// = Sum of degraded service prices
        /// </summary>
        public decimal RevenueImpact { get; set; }

        /// <summary>
        /// Lý do degradation ph? bi?n
        /// </summary>
        public Dictionary<string, int> DegradationReasons { get; set; } = new();

        /// <summary>
        /// Top 5 services b? degraded nhi?u nh?t
        /// </summary>
        public List<DegradedServiceDto> TopDegradedServices { get; set; } = new();

        /// <summary>
        /// Degradation theo service center
        /// </summary>
        public List<CenterDegradationDto> DegradationByCenter { get; set; } = new();

        /// <summary>
        /// Th?i gian t?o metrics
        /// </summary>
        public DateTime GeneratedAt { get; set; }
    }

    /// <summary>
    /// Thông tin service b? degraded
    /// </summary>
    public class DegradedServiceDto
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public int DegradationCount { get; set; }
        public decimal TotalRevenueImpact { get; set; }
    }

    /// <summary>
    /// Degradation theo center
    /// </summary>
    public class CenterDegradationDto
    {
        public int ServiceCenterId { get; set; }
        public string CenterName { get; set; } = string.Empty;
        public int DegradationCount { get; set; }
        public decimal DegradationRate { get; set; }
    }
}
