using EVServiceCenter.Core.Domains.Customers.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Entities;

public partial class CustomerCommunicationPreference
{
    [Key]
    [Column("PreferenceID")]
    public int PreferenceId { get; set; }

    [Column("CustomerID")]
    public int CustomerId { get; set; }

    [Column("SMSNotifications")]
    public bool? Smsnotifications { get; set; }

    public bool? EmailNotifications { get; set; }

    public bool? PushNotifications { get; set; }

    public bool? MarketingCommunications { get; set; }

    public bool? ServiceReminders { get; set; }

    public bool? PromotionalOffers { get; set; }

    public DateTime? UpdatedDate { get; set; }

    [ForeignKey("CustomerId")]
    [InverseProperty("CustomerCommunicationPreferences")]
    public virtual Customer Customer { get; set; } = null!;
}
