namespace EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Response
{
    /// <summary>
    /// ?? Payment Health Metrics - Theo dõi s?c kh?e thanh toán
    /// </summary>
    public class PaymentHealthMetricsDto
    {
        /// <summary>
        /// T?ng s? appointments có payment
        /// </summary>
        public int TotalAppointmentsWithPayment { get; set; }

        /// <summary>
        /// S? appointments ?ã thanh toán ??y ??
        /// </summary>
        public int FullyPaidAppointments { get; set; }

        /// <summary>
        /// S? appointments ch?a thanh toán ho?c thanh toán thi?u
        /// </summary>
        public int UnpaidOrPartialAppointments { get; set; }

        /// <summary>
        /// T? l? thanh toán thành công (%)
        /// </summary>
        public decimal PaymentSuccessRate { get; set; }

        /// <summary>
        /// T? l? ch?a thanh toán (%)
        /// </summary>
        public decimal UnpaidRate { get; set; }

        /// <summary>
        /// T?ng s? ti?n ?ã thu (VN?)
        /// </summary>
        public decimal TotalRevenue { get; set; }

        /// <summary>
        /// T?ng s? ti?n ch?a thu (VN?)
        /// </summary>
        public decimal TotalOutstanding { get; set; }

        /// <summary>
        /// Th?i gian thanh toán trung bình (gi?)
        /// Tính t? lúc t?o appointment ??n lúc completed payment
        /// </summary>
        public double AveragePaymentTimeHours { get; set; }

        /// <summary>
        /// Phân b? theo payment status
        /// </summary>
        public Dictionary<string, int> PaymentStatusDistribution { get; set; } = new();

        /// <summary>
        /// Top 5 appointments có outstanding amount cao nh?t
        /// </summary>
        public List<OutstandingAppointmentDto> TopOutstandingAppointments { get; set; } = new();

        /// <summary>
        /// Th?i gian t?o metrics
        /// </summary>
        public DateTime GeneratedAt { get; set; }
    }

    /// <summary>
    /// Thông tin appointment có outstanding amount
    /// </summary>
    public class OutstandingAppointmentDto
    {
        public int AppointmentId { get; set; }
        public string AppointmentCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public decimal OutstandingAmount { get; set; }
        public DateTime CreatedDate { get; set; }
        public int DaysOverdue { get; set; }
    }
}
