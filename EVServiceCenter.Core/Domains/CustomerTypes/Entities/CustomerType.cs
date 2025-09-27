using EVServiceCenter.Core.Domains.Customers.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Domains.CustomerTypes.Entities;

public partial class CustomerType
{
    [Key]
    [Column("TypeID")]
    public int TypeId { get; set; }

    [StringLength(50)]
    public string TypeName { get; set; } = null!;

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? DiscountPercent { get; set; }

    [StringLength(200)]
    public string? Description { get; set; }

    public bool? IsActive { get; set; }

    [InverseProperty("Type")]
    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();
}
