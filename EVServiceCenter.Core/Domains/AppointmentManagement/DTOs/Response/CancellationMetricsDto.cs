namespace EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Response
{
    /// <summary>
    /// ?? Cancellation Metrics - Phân tích h?y l?ch
    /// </summary>
    public class CancellationMetricsDto
    {
        /// <summary>
        /// T?ng s? appointments trong k?
        /// </summary>
        public int TotalAppointments { get; set; }

        /// <summary>
        /// S? appointments b? cancel
        /// </summary>
        public int CancelledAppointments { get; set; }

        /// <summary>
        /// T? l? cancel (%)
        /// </summary>
        public decimal CancellationRate { get; set; }

        /// <summary>
        /// S? appointments b? NoShow
        /// </summary>
        public int NoShowAppointments { get; set; }

        /// <summary>
        /// T? l? NoShow (%)
        /// </summary>
        public decimal NoShowRate { get; set; }

        /// <summary>
        /// T?ng ti?n ?ã refund (VN?)
        /// </summary>
        public decimal TotalRefunded { get; set; }

        /// <summary>
        /// Th?i gian thông báo h?y trung bình (gi?)
        /// Tính t? lúc h?y ??n th?i gian h?n
        /// </summary>
        public double AverageNoticeTimeHours { get; set; }

        /// <summary>
        /// Phân b? th?i gian thông báo h?y
        /// </summary>
        public NoticeTimeDistributionDto NoticeTimeDistribution { get; set; } = new();

        /// <summary>
        /// Top 5 lý do h?y ph? bi?n
        /// </summary>
        public Dictionary<string, int> TopCancellationReasons { get; set; } = new();

        /// <summary>
        /// Cancellation theo service center
        /// </summary>
        public List<CenterCancellationDto> CancellationByCenter { get; set; } = new();

        /// <summary>
        /// Th?i gian t?o metrics
        /// </summary>
        public DateTime GeneratedAt { get; set; }
    }

    /// <summary>
    /// Phân b? th?i gian thông báo h?y
    /// </summary>
    public class NoticeTimeDistributionDto
    {
        public int MoreThan24Hours { get; set; } // H?y >= 24h tr??c (100% refund)
        public int Between2And24Hours { get; set; } // H?y 2-24h tr??c (50% refund)
        public int LessThan2Hours { get; set; } // H?y < 2h tr??c (0% refund)
    }

    /// <summary>
    /// Cancellation theo center
    /// </summary>
    public class CenterCancellationDto
    {
        public int ServiceCenterId { get; set; }
        public string CenterName { get; set; } = string.Empty;
        public int CancellationCount { get; set; }
        public decimal CancellationRate { get; set; }
    }
}
