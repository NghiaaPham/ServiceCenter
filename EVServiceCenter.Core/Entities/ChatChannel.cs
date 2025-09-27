using EVServiceCenter.Core.Domains.Customers.Entities;
using EVServiceCenter.Core.Domains.Identity.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Entities;

public partial class ChatChannel
{
    [Key]
    [Column("ChannelID")]
    public int ChannelId { get; set; }

    [StringLength(100)]
    public string ChannelName { get; set; } = null!;

    [StringLength(20)]
    public string? ChannelType { get; set; }

    [Column("CustomerID")]
    public int? CustomerId { get; set; }

    [Column("AssignedUserID")]
    public int? AssignedUserId { get; set; }

    [StringLength(20)]
    public string? Status { get; set; }

    [StringLength(10)]
    public string? Priority { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? LastMessageDate { get; set; }

    public DateTime? ClosedDate { get; set; }

    public int? ClosedBy { get; set; }

    public int? Rating { get; set; }

    [StringLength(500)]
    public string? Tags { get; set; }

    [ForeignKey("AssignedUserId")]
    [InverseProperty("ChatChannelAssignedUsers")]
    public virtual User? AssignedUser { get; set; }

    [InverseProperty("Channel")]
    public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();

    [ForeignKey("ClosedBy")]
    [InverseProperty("ChatChannelClosedByNavigations")]
    public virtual User? ClosedByNavigation { get; set; }

    [ForeignKey("CustomerId")]
    [InverseProperty("ChatChannels")]
    public virtual Customer? Customer { get; set; }
}
