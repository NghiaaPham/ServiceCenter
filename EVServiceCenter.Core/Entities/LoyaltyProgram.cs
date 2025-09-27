using EVServiceCenter.Core.Domains.Identity.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Entities;

public partial class LoyaltyProgram
{
    [Key]
    [Column("ProgramID")]
    public int ProgramId { get; set; }

    [StringLength(100)]
    public string ProgramName { get; set; } = null!;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Column(TypeName = "decimal(10, 4)")]
    public decimal? PointsPerDollar { get; set; }

    public int? WelcomeBonus { get; set; }

    public int? ReferralBonus { get; set; }

    public int? BirthdayBonus { get; set; }

    public int? MinimumRedemption { get; set; }

    public int? PointsExpiryDays { get; set; }

    public string? TierThresholds { get; set; }

    public bool? IsActive { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("LoyaltyPrograms")]
    public virtual User? CreatedByNavigation { get; set; }

    [InverseProperty("Program")]
    public virtual ICollection<LoyaltyTransaction> LoyaltyTransactions { get; set; } = new List<LoyaltyTransaction>();
}
