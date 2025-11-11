namespace EVServiceCenter.Core.Domains.TechnicianManagement.DTOs.Responses
{
    /// <summary>
    /// Response DTO for technician's work orders
    /// </summary>
    public class TechnicianWorkOrderResponseDto
    {
        public int WorkOrderId { get; set; }
        public string WorkOrderCode { get; set; } = string.Empty;
        
        // Customer & Vehicle
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerPhone { get; set; }
        
        public int VehicleId { get; set; }
        public string VehicleName { get; set; } = string.Empty;
        public string? LicensePlate { get; set; }
        
        // Status
        public int StatusId { get; set; }
        public string StatusName { get; set; } = string.Empty;
        
        // Assignment
        public int? TechnicianId { get; set; }
        public string? TechnicianName { get; set; }
        public int? SupervisorId { get; set; }
        public string? SupervisorName { get; set; }
        
        // Services
        public int ServicesCount { get; set; }
        public List<WorkOrderServiceDto> Services { get; set; } = new();
        
        // Checklist
        public int ChecklistTotal { get; set; }
        public int ChecklistCompleted { get; set; }
        public decimal ChecklistProgress => ChecklistTotal > 0 
            ? Math.Round((decimal)ChecklistCompleted / ChecklistTotal * 100, 2) 
            : 0;
        
        // Cost & Time
        public decimal? EstimatedCost { get; set; }
        public int? EstimatedDuration { get; set; }
        public int? ActualDuration { get; set; }
        
        // Dates
        public DateTime? StartDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public DateTime CreatedDate { get; set; }
        
        public string? TechnicianNotes { get; set; }
        public string Priority { get; set; } = "Normal";
    }
    
    public class WorkOrderServiceDto
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public int? EstimatedTime { get; set; }
        public int? ActualTime { get; set; }
        public decimal Price { get; set; }
        public string? Status { get; set; }
    }
}
