namespace EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Response
{
    /// <summary>
    /// ?? Payment Health Metrics - Theo d�i s?c kh?e thanh to�n
    /// </summary>
    public class PaymentHealthMetricsDto
    {
        /// <summary>
        /// T?ng s? appointments c� payment
        /// </summary>
        public int TotalAppointmentsWithPayment { get; set; }

        /// <summary>
        /// S? appointments ?� thanh to�n ??y ??
        /// </summary>
        public int FullyPaidAppointments { get; set; }

        /// <summary>
        /// S? appointments ch?a thanh to�n ho?c thanh to�n thi?u
        /// </summary>
        public int UnpaidOrPartialAppointments { get; set; }

        /// <summary>
        /// T? l? thanh to�n th�nh c�ng (%)
        /// </summary>
        public decimal PaymentSuccessRate { get; set; }

        /// <summary>
        /// T? l? ch?a thanh to�n (%)
        /// </summary>
        public decimal UnpaidRate { get; set; }

        /// <summary>
        /// T?ng s? ti?n ?� thu (VN?)
        /// </summary>
        public decimal TotalRevenue { get; set; }

        /// <summary>
        /// T?ng s? ti?n ch?a thu (VN?)
        /// </summary>
        public decimal TotalOutstanding { get; set; }

        /// <summary>
        /// Th?i gian thanh to�n trung b�nh (gi?)
        /// T�nh t? l�c t?o appointment ??n l�c completed payment
        /// </summary>
        public double AveragePaymentTimeHours { get; set; }

        /// <summary>
        /// Ph�n b? theo payment status
        /// </summary>
        public Dictionary<string, int> PaymentStatusDistribution { get; set; } = new();

        /// <summary>
        /// Top 5 appointments c� outstanding amount cao nh?t
        /// </summary>
        public List<OutstandingAppointmentDto> TopOutstandingAppointments { get; set; } = new();

        /// <summary>
        /// Th?i gian t?o metrics
        /// </summary>
        public DateTime GeneratedAt { get; set; }
    }

    /// <summary>
    /// Th�ng tin appointment c� outstanding amount
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
