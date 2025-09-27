using EVServiceCenter.Core.Domains.Identity.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Entities;

public partial class Department
{
    [Key]
    [Column("DepartmentID")]
    public int DepartmentId { get; set; }

    [StringLength(20)]
    public string DepartmentCode { get; set; } = null!;

    [StringLength(100)]
    public string DepartmentName { get; set; } = null!;

    [StringLength(500)]
    public string? Description { get; set; }

    [Column("ManagerID")]
    public int? ManagerId { get; set; }

    [Column("CenterID")]
    public int? CenterId { get; set; }

    public bool? IsActive { get; set; }

    [ForeignKey("CenterId")]
    [InverseProperty("Departments")]
    public virtual ServiceCenter? Center { get; set; }

    [ForeignKey("ManagerId")]
    [InverseProperty("Departments")]
    public virtual User? Manager { get; set; }
}
