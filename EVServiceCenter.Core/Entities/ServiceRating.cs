using EVServiceCenter.Core.Domains.Customers.Entities;
using EVServiceCenter.Core.Domains.Identity.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Entities;

public partial class ServiceRating
{
    [Key]
    [Column("RatingID")]
    public int RatingId { get; set; }

    [Column("WorkOrderID")]
    public int WorkOrderId { get; set; }

    [Column("CustomerID")]
    public int CustomerId { get; set; }

    [Column("TechnicianID")]
    public int? TechnicianId { get; set; }

    [Column("AdvisorID")]
    public int? AdvisorId { get; set; }

    public int? OverallRating { get; set; }

    public int? ServiceQuality { get; set; }

    public int? StaffProfessionalism { get; set; }

    public int? FacilityQuality { get; set; }

    public int? WaitingTime { get; set; }

    public int? PriceValue { get; set; }

    public int? CommunicationQuality { get; set; }

    [StringLength(1000)]
    public string? PositiveFeedback { get; set; }

    [StringLength(1000)]
    public string? NegativeFeedback { get; set; }

    [StringLength(1000)]
    public string? Suggestions { get; set; }

    public bool? WouldRecommend { get; set; }

    public bool? WouldReturn { get; set; }

    public DateTime? RatingDate { get; set; }

    [StringLength(20)]
    public string? ResponseMethod { get; set; }

    public int? RespondedBy { get; set; }

    public bool? IsVerified { get; set; }

    [ForeignKey("AdvisorId")]
    [InverseProperty("ServiceRatingAdvisors")]
    public virtual User? Advisor { get; set; }

    [ForeignKey("CustomerId")]
    [InverseProperty("ServiceRatings")]
    public virtual Customer Customer { get; set; } = null!;

    [ForeignKey("RespondedBy")]
    [InverseProperty("ServiceRatingRespondedByNavigations")]
    public virtual User? RespondedByNavigation { get; set; }

    [ForeignKey("TechnicianId")]
    [InverseProperty("ServiceRatingTechnicians")]
    public virtual User? Technician { get; set; }

    [ForeignKey("WorkOrderId")]
    [InverseProperty("ServiceRatings")]
    public virtual WorkOrder WorkOrder { get; set; } = null!;
}
