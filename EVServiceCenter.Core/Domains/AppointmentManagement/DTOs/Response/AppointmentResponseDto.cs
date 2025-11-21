namespace EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Response
{
    public class AppointmentResponseDto
    {
        public int AppointmentId { get; set; }
        public string AppointmentCode { get; set; } = null!;

        // Customer & Vehicle
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = null!;
        public string? CustomerPhone { get; set; }
        public string? CustomerEmail { get; set; }

        public int VehicleId { get; set; }
        public string VehicleName { get; set; } = null!;
        public string? LicensePlate { get; set; }
        public string? VIN { get; set; }

        // Service Center & Slot
        public int ServiceCenterId { get; set; }
        public string ServiceCenterName { get; set; } = null!;
        public string? ServiceCenterAddress { get; set; }

        public int? SlotId { get; set; }
        public DateOnly? SlotDate { get; set; }
        public TimeOnly? SlotStartTime { get; set; }
        public TimeOnly? SlotEndTime { get; set; }

        // Package (if any)
        public int? PackageId { get; set; }
        public string? PackageName { get; set; }
        public decimal? PackagePrice { get; set; }

        // Services
        public List<AppointmentServiceDto> Services { get; set; } = new();

        // Status
        public int StatusId { get; set; }
        public string StatusName { get; set; } = null!;
        public string StatusColor { get; set; } = null!;

        // Cost & Time
        public int? EstimatedDuration { get; set; }
        public decimal? EstimatedCost { get; set; }
        public decimal? FinalCost { get; set; }

        /// <summary>
        /// ✅ THÊM MỚI: Breakdown discount chi tiết
        /// Hiển thị cho customer thấy rõ được giảm bao nhiêu từ đâu
        /// NULL nếu không có discount (all subscription services hoặc no discount applied)
        /// </summary>
        public DiscountSummaryDto? DiscountSummary { get; set; }

        // Payment
        public string PaymentStatus { get; set; } = null!;
        public decimal? PaidAmount { get; set; }
        public int PaymentIntentCount { get; set; }
        public int? LatestPaymentIntentId { get; set; }
        public decimal OutstandingAmount { get; set; }

        // Other
        public string? CustomerNotes { get; set; }
        public string Priority { get; set; } = null!;
        public string Source { get; set; } = null!;

        // Preferred Technician
        public int? PreferredTechnicianId { get; set; }
        public string? PreferredTechnicianName { get; set; }

        // Dates
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public DateTime? ConfirmationDate { get; set; }

        // Cancellation/Reschedule
        public string? CancellationReason { get; set; }
        public int? RescheduledFromId { get; set; }

        /// <summary>
        /// ✅ ISSUE #1 FIX: Payment warning message for frontend
        /// Displayed when check-in with outstanding payment
        /// Example: "⚠️ Khách hàng còn công nợ 500,000đ. Vui lòng thu tiền trước khi giao xe."
        /// </summary>
        public string? Message { get; set; }
    }
}