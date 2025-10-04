namespace EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Response
{
    public class AppointmentDetailResponseDto : AppointmentResponseDto 
    {
        // Service Description
        public string? ServiceDescription { get; set; }

        // Confirmation Info
        public string? ConfirmationMethod { get; set; }
        public string? ConfirmationStatus { get; set; }

        // Reminder Info
        public bool? ReminderSent { get; set; }
        public DateTime? ReminderSentDate { get; set; }

        // No Show Flag
        public bool? NoShowFlag { get; set; }

        // Created/Updated By
        public int? CreatedBy { get; set; }
        public string? CreatedByName { get; set; }
        public int? UpdatedBy { get; set; }
        public string? UpdatedByName { get; set; }

        // Related Work Orders (if any)
        public List<WorkOrderSummaryDto>? WorkOrders { get; set; }
    }
}
