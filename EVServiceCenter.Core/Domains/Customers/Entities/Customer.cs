
using EVServiceCenter.Core.Domains.CustomerTypes.Entities;
using EVServiceCenter.Core.Domains.Identity.Entities;
using EVServiceCenter.Core.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EVServiceCenter.Core.Domains.AppointmentManagement.Entities;
using EVServiceCenter.Core.Domains.CustomerVehicles.Entities;
namespace EVServiceCenter.Core.Domains.Customers.Entities;

public partial class Customer
{
    [Key]
    [Column("CustomerID")]
    public int CustomerId { get; set; }

    [Column("UserID")]
    public int? UserId { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("Customer")]
    public virtual User? User { get; set; }

    [StringLength(20)]
    public string CustomerCode { get; set; } = null!;

    [StringLength(100)]
    public string FullName { get; set; } = null!;

    [StringLength(20)]
    public string PhoneNumber { get; set; } = null!;

    [StringLength(100)]
    public string? Email { get; set; }

    [StringLength(500)]
    public string? Address { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    [StringLength(10)]
    public string? Gender { get; set; }

    [MaxLength(256)]
    public byte[]? IdentityNumber { get; set; }

    [Column("TypeID")]
    public int? TypeId { get; set; }

    [StringLength(10)]
    public string? PreferredLanguage { get; set; }

    public bool? MarketingOptIn { get; set; }

    public int? LoyaltyPoints { get; set; }

    [Column(TypeName = "decimal(15, 2)")]
    public decimal? TotalSpent { get; set; }

    public DateOnly? LastVisitDate { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    [InverseProperty("Customer")]
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    [InverseProperty("Customer")]
    public virtual ICollection<ChatChannel> ChatChannels { get; set; } = new List<ChatChannel>();

    [ForeignKey("CreatedBy")]
    [InverseProperty("CustomerCreatedByNavigations")]
    public virtual User? CreatedByNavigation { get; set; }

    [InverseProperty("Customer")]
    public virtual ICollection<CustomerCommunicationPreference> CustomerCommunicationPreferences { get; set; } = new List<CustomerCommunicationPreference>();

    [InverseProperty("Customer")]
    public virtual ICollection<CustomerPackageSubscription> CustomerPackageSubscriptions { get; set; } = new List<CustomerPackageSubscription>();

    [InverseProperty("Customer")]
    public virtual ICollection<CustomerVehicle> CustomerVehicles { get; set; } = new List<CustomerVehicle>();

    [InverseProperty("Customer")]
    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    [InverseProperty("Customer")]
    public virtual ICollection<LoyaltyTransaction> LoyaltyTransactions { get; set; } = new List<LoyaltyTransaction>();

    [InverseProperty("Customer")]
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    [InverseProperty("Customer")]
    public virtual ICollection<ServiceRating> ServiceRatings { get; set; } = new List<ServiceRating>();

    [ForeignKey("TypeId")]
    [InverseProperty("Customers")]
    public virtual CustomerType? Type { get; set; }

    [ForeignKey("UpdatedBy")]
    [InverseProperty("CustomerUpdatedByNavigations")]
    public virtual User? UpdatedByNavigation { get; set; }

    [InverseProperty("Customer")]
    public virtual ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();
}
