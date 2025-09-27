using EVServiceCenter.Core.Domains.AppointmentManagement.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Entities;

public partial class ChatMessage
{
    [Key]
    [Column("MessageID")]
    public int MessageId { get; set; }

    [Column("ChannelID")]
    public int ChannelId { get; set; }

    [StringLength(10)]
    public string SenderType { get; set; } = null!;

    [Column("SenderID")]
    public int SenderId { get; set; }

    [StringLength(20)]
    public string? MessageType { get; set; }

    public string MessageContent { get; set; } = null!;

    [StringLength(500)]
    public string? AttachmentUrl { get; set; }

    [StringLength(50)]
    public string? AttachmentType { get; set; }

    public int? AttachmentSize { get; set; }

    public bool? IsRead { get; set; }

    public DateTime? ReadDate { get; set; }

    public bool? IsDelivered { get; set; }

    public DateTime? DeliveredDate { get; set; }

    [Column("ReplyToMessageID")]
    public int? ReplyToMessageId { get; set; }

    [Column("RelatedAppointmentID")]
    public int? RelatedAppointmentId { get; set; }

    [Column("RelatedWorkOrderID")]
    public int? RelatedWorkOrderId { get; set; }

    [Column("RelatedInvoiceID")]
    public int? RelatedInvoiceId { get; set; }

    [Column("IPAddress")]
    [StringLength(45)]
    public string? Ipaddress { get; set; }

    [StringLength(500)]
    public string? UserAgent { get; set; }

    [StringLength(20)]
    public string? DeviceType { get; set; }

    public DateTime? Timestamp { get; set; }

    public DateTime? EditedDate { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime? DeletedDate { get; set; }

    [ForeignKey("ChannelId")]
    [InverseProperty("ChatMessages")]
    public virtual ChatChannel Channel { get; set; } = null!;

    [InverseProperty("ReplyToMessage")]
    public virtual ICollection<ChatMessage> InverseReplyToMessage { get; set; } = new List<ChatMessage>();

    [ForeignKey("RelatedAppointmentId")]
    [InverseProperty("ChatMessages")]
    public virtual Appointment? RelatedAppointment { get; set; }

    [ForeignKey("RelatedInvoiceId")]
    [InverseProperty("ChatMessages")]
    public virtual Invoice? RelatedInvoice { get; set; }

    [ForeignKey("RelatedWorkOrderId")]
    [InverseProperty("ChatMessages")]
    public virtual WorkOrder? RelatedWorkOrder { get; set; }

    [ForeignKey("ReplyToMessageId")]
    [InverseProperty("InverseReplyToMessage")]
    public virtual ChatMessage? ReplyToMessage { get; set; }
}
