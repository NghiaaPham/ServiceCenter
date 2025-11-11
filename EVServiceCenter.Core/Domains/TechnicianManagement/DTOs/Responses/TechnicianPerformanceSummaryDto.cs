namespace EVServiceCenter.Core.Domains.TechnicianManagement.DTOs.Responses
{
    /// <summary>
    /// Summary DTO for technician performance metrics
    /// Used in dashboard and overview displays
    /// </summary>
    public class TechnicianPerformanceSummaryDto
    {
        public int TotalWorkOrdersCompleted { get; set; }
        public int WorkOrdersThisMonth { get; set; }
        public decimal? AverageRating { get; set; }
        public decimal? AverageCompletionTimeHours { get; set; }
    }
}
