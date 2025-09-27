using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Entities;

[Table("APIRequestLogs")]
public partial class ApirequestLog
{
    [Key]
    [Column("LogID")]
    public int LogId { get; set; }

    [Column("KeyID")]
    public int? KeyId { get; set; }

    [StringLength(10)]
    public string? RequestMethod { get; set; }

    [Column("RequestURL")]
    [StringLength(500)]
    public string? RequestUrl { get; set; }

    public string? RequestHeaders { get; set; }

    public string? RequestBody { get; set; }

    public int? ResponseStatus { get; set; }

    public string? ResponseHeaders { get; set; }

    public string? ResponseBody { get; set; }

    public int? ResponseTime { get; set; }

    [Column("IPAddress")]
    [StringLength(45)]
    public string? Ipaddress { get; set; }

    [StringLength(500)]
    public string? UserAgent { get; set; }

    public DateTime? RequestDate { get; set; }

    [ForeignKey("KeyId")]
    [InverseProperty("ApirequestLogs")]
    public virtual Apikey? Key { get; set; }
}
