namespace EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Response
{
    /// <summary>
    /// ?? Subscription Usage Metrics - Phân tích s? d?ng subscription
    /// </summary>
    public class SubscriptionUsageMetricsDto
    {
        /// <summary>
        /// T?ng s? appointments trong k?
        /// </summary>
        public int TotalAppointments { get; set; }

        /// <summary>
        /// S? appointments s? d?ng subscription
        /// </summary>
        public int SubscriptionAppointments { get; set; }

        /// <summary>
        /// S? appointments không dùng subscription (Extra)
        /// </summary>
        public int ExtraAppointments { get; set; }

        /// <summary>
        /// T? l? s? d?ng subscription (%)
        /// </summary>
        public decimal SubscriptionUsageRate { get; set; }

        /// <summary>
        /// T?ng ti?n ti?t ki?m nh? subscription (VN?)
        /// Tính = Giá g?c - Giá ?ã gi?m
        /// </summary>
        public decimal TotalSavings { get; set; }

        /// <summary>
        /// Ti?n ti?t ki?m trung bình m?i appointment (VN?)
        /// </summary>
        public decimal AverageSavingsPerAppointment { get; set; }

        /// <summary>
        /// Top 5 packages ???c s? d?ng nhi?u nh?t
        /// </summary>
        public List<PackageUsageDto> TopPackages { get; set; } = new();

        /// <summary>
        /// Top 5 services ???c dùng t? subscription
        /// </summary>
        public List<ServiceUsageDto> TopServices { get; set; } = new();

        /// <summary>
        /// Th?i gian t?o metrics
        /// </summary>
        public DateTime GeneratedAt { get; set; }
    }

    /// <summary>
    /// Thông tin usage c?a m?t package
    /// </summary>
    public class PackageUsageDto
    {
        public int PackageId { get; set; }
        public string PackageName { get; set; } = string.Empty;
        public int UsageCount { get; set; }
        public decimal TotalSavings { get; set; }
    }

    /// <summary>
    /// Thông tin usage c?a m?t service
    /// </summary>
    public class ServiceUsageDto
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public int UsageCount { get; set; }
        public decimal TotalSavings { get; set; }
    }
}
