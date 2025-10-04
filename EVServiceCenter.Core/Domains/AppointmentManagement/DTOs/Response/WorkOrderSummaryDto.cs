
namespace EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Response
{
    public class WorkOrderSummaryDto
    {
        public int WorkOrderId { get; set; }
        public string WorkOrderNumber { get; set; } = null!;
        public string StatusName { get; set; } = null!;
        public decimal? TotalAmount { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
