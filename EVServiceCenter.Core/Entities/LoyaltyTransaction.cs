using EVServiceCenter.Core.Domains.Customers.Entities;
using EVServiceCenter.Core.Domains.Identity.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Entities;

public partial class LoyaltyTransaction
{
    [Key]
    [Column("TransactionID")]
    public int TransactionId { get; set; }

    [Column("CustomerID")]
    public int CustomerId { get; set; }

    [Column("ProgramID")]
    public int ProgramId { get; set; }

    [StringLength(20)]
    public string TransactionType { get; set; } = null!;

    public int Points { get; set; }

    [StringLength(50)]
    public string? ReferenceType { get; set; }

    [Column("ReferenceID")]
    public int? ReferenceId { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    public DateOnly? ExpiryDate { get; set; }

    public DateTime? TransactionDate { get; set; }

    public int? CreatedBy { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("LoyaltyTransactions")]
    public virtual User? CreatedByNavigation { get; set; }

    [ForeignKey("CustomerId")]
    [InverseProperty("LoyaltyTransactions")]
    public virtual Customer Customer { get; set; } = null!;

    [ForeignKey("ProgramId")]
    [InverseProperty("LoyaltyTransactions")]
    public virtual LoyaltyProgram Program { get; set; } = null!;
}
