using EVServiceCenter.Core.Domains.Identity.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Entities;

[Table("APIKeys")]
public partial class Apikey
{
    [Key]
    [Column("KeyID")]
    public int KeyId { get; set; }

    [StringLength(100)]
    public string KeyName { get; set; } = null!;

    [Column("APIKey")]
    [StringLength(128)]
    public string Apikey1 { get; set; } = null!;

    [StringLength(128)]
    public string SecretKey { get; set; } = null!;

    [StringLength(20)]
    public string? KeyType { get; set; }

    [Column("AllowedIPs")]
    [StringLength(500)]
    public string? AllowedIps { get; set; }

    public string? Permissions { get; set; }

    public int? RateLimit { get; set; }

    public int? CurrentUsage { get; set; }

    public bool? IsActive { get; set; }

    public DateOnly? ExpiryDate { get; set; }

    public DateTime? LastUsed { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    [InverseProperty("Key")]
    public virtual ICollection<ApirequestLog> ApirequestLogs { get; set; } = new List<ApirequestLog>();

    [ForeignKey("CreatedBy")]
    [InverseProperty("Apikeys")]
    public virtual User? CreatedByNavigation { get; set; }
}
