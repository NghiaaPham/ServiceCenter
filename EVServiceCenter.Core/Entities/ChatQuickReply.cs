using EVServiceCenter.Core.Domains.Identity.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Entities;

public partial class ChatQuickReply
{
    [Key]
    [Column("QuickReplyID")]
    public int QuickReplyId { get; set; }

    [StringLength(50)]
    public string? Category { get; set; }

    [StringLength(100)]
    public string Title { get; set; } = null!;

    [StringLength(1000)]
    public string MessageText { get; set; } = null!;

    public int? UseCount { get; set; }

    public bool? IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("ChatQuickReplies")]
    public virtual User? CreatedByNavigation { get; set; }
}
