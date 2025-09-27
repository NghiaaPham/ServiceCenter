using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Entities;

public partial class PartCategory
{
    [Key]
    [Column("CategoryID")]
    public int CategoryId { get; set; }

    [StringLength(100)]
    public string CategoryName { get; set; } = null!;

    [Column("ParentCategoryID")]
    public int? ParentCategoryId { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(500)]
    public string? ImageUrl { get; set; }

    public bool? IsActive { get; set; }

    [InverseProperty("ParentCategory")]
    public virtual ICollection<PartCategory> InverseParentCategory { get; set; } = new List<PartCategory>();

    [ForeignKey("ParentCategoryId")]
    [InverseProperty("InverseParentCategory")]
    public virtual PartCategory? ParentCategory { get; set; }

    [InverseProperty("Category")]
    public virtual ICollection<Part> Parts { get; set; } = new List<Part>();
}
