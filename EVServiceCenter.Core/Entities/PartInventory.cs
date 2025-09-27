using EVServiceCenter.Core.Domains.Identity.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Entities;

[Table("PartInventory")]
public partial class PartInventory
{
    [Key]
    [Column("InventoryID")]
    public int InventoryId { get; set; }

    [Column("PartID")]
    public int PartId { get; set; }

    [Column("CenterID")]
    public int CenterId { get; set; }

    public int? CurrentStock { get; set; }

    public int? ReservedStock { get; set; }

    public int? AvailableStock { get; set; }

    [StringLength(100)]
    public string? Location { get; set; }

    public DateOnly? LastStockTakeDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    [ForeignKey("CenterId")]
    [InverseProperty("PartInventories")]
    public virtual ServiceCenter Center { get; set; } = null!;

    [ForeignKey("PartId")]
    [InverseProperty("PartInventories")]
    public virtual Part Part { get; set; } = null!;

    [ForeignKey("UpdatedBy")]
    [InverseProperty("PartInventories")]
    public virtual User? UpdatedByNavigation { get; set; }
}
