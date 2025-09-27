using EVServiceCenter.Core.Domains.Identity.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Entities;

public partial class Certification
{
    [Key]
    [Column("CertificationID")]
    public int CertificationId { get; set; }

    [Column("UserID")]
    public int UserId { get; set; }

    [StringLength(200)]
    public string CertificationName { get; set; } = null!;

    [StringLength(50)]
    public string? CertificationLevel { get; set; }

    [StringLength(100)]
    public string? Issuer { get; set; }

    public DateOnly? IssueDate { get; set; }

    public DateOnly? ExpirationDate { get; set; }

    [StringLength(50)]
    public string? CertificateNumber { get; set; }

    [StringLength(500)]
    public string? CertificateUrl { get; set; }

    [StringLength(20)]
    public string? Status { get; set; }

    public bool? RenewalRequired { get; set; }

    public bool? RenewalReminderSent { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? Cost { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("CertificationCreatedByNavigations")]
    public virtual User? CreatedByNavigation { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("CertificationUsers")]
    public virtual User User { get; set; } = null!;
}
