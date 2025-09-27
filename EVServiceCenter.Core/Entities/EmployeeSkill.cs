using EVServiceCenter.Core.Domains.Identity.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Entities;

public partial class EmployeeSkill
{
    [Key]
    [Column("SkillID")]
    public int SkillId { get; set; }

    [Column("UserID")]
    public int UserId { get; set; }

    [StringLength(100)]
    public string SkillName { get; set; } = null!;

    [StringLength(20)]
    public string? SkillLevel { get; set; }

    public DateOnly? CertificationDate { get; set; }

    public DateOnly? ExpiryDate { get; set; }

    [StringLength(100)]
    public string? CertifyingBody { get; set; }

    [StringLength(50)]
    public string? CertificationNumber { get; set; }

    public bool? IsVerified { get; set; }

    public int? VerifiedBy { get; set; }

    public DateOnly? VerifiedDate { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("EmployeeSkillUsers")]
    public virtual User User { get; set; } = null!;

    [ForeignKey("VerifiedBy")]
    [InverseProperty("EmployeeSkillVerifiedByNavigations")]
    public virtual User? VerifiedByNavigation { get; set; }
}
