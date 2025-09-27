using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Entities;

public partial class OnlinePayment
{
    [Key]
    [Column("OnlinePaymentID")]
    public int OnlinePaymentId { get; set; }

    [Column("PaymentID")]
    public int PaymentId { get; set; }

    [Column("GatewayTransactionID")]
    [StringLength(100)]
    public string GatewayTransactionId { get; set; } = null!;

    [StringLength(50)]
    public string? GatewayName { get; set; }

    [StringLength(50)]
    public string? PaymentStatus { get; set; }

    public string? RequestData { get; set; }

    public string? ResponseData { get; set; }

    [StringLength(50)]
    public string? ResponseCode { get; set; }

    [StringLength(500)]
    public string? ResponseMessage { get; set; }

    public int? ProcessingTime { get; set; }

    [Column("IPAddress")]
    [StringLength(45)]
    public string? Ipaddress { get; set; }

    [StringLength(500)]
    public string? UserAgent { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    [ForeignKey("PaymentId")]
    [InverseProperty("OnlinePayments")]
    public virtual Payment Payment { get; set; } = null!;
}
