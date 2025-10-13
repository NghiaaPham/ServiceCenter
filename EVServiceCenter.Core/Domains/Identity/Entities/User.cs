using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EVServiceCenter.Core.Entities;
using EVServiceCenter.Core.Domains.Customers.Entities;
using EVServiceCenter.Core.Domains.AppointmentManagement.Entities;
using EVServiceCenter.Core.Domains.ServiceCenters.Entities;
using EVServiceCenter.Core.Domains.CustomerVehicles.Entities;

namespace EVServiceCenter.Core.Domains.Identity.Entities;

public partial class User
{
    [Key]
    [Column("UserID")]
    public int UserId { get; set; }

    [StringLength(50)]
    public string Username { get; set; } = null!;

    [MaxLength(64)]
    public byte[] PasswordHash { get; set; } = null!;

    [MaxLength(32)]
    public byte[] PasswordSalt { get; set; } = null!;

    [StringLength(100)]
    public string FullName { get; set; } = null!;

    [StringLength(100)]
    public string? Email { get; set; }

    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    [Column("RoleID")]
    public int RoleId { get; set; }

    [StringLength(20)]
    public string? EmployeeCode { get; set; }

    [StringLength(50)]
    public string? Department { get; set; }

    public DateOnly? HireDate { get; set; }

    [Column(TypeName = "decimal(15, 2)")]
    public decimal? Salary { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? LastLoginDate { get; set; }

    public int? FailedLoginAttempts { get; set; }

    public DateOnly? PasswordExpiryDate { get; set; }

    [StringLength(500)]
    public string? ProfilePicture { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    public byte[]? ResetToken { get; set; }
    public DateTime? ResetTokenExpiry { get; set; }
    public bool EmailVerified { get; set; } = false;
    public byte[]? EmailVerificationToken { get; set; }
    public DateTime? EmailVerificationExpiry { get; set; }

    public DateTime? AccountLockedUntil { get; set; }
    public bool IsAccountLocked { get; set; } = false;
    public DateTime? LastFailedLoginAttempt { get; set; }
    public string? LockoutReason { get; set; }
    public int? UnlockAttempts { get; set; } = 0;

    [StringLength(50)]
    public string? ExternalProvider { get; set; }

    [StringLength(200)]
    public string? ExternalProviderId { get; set; }

    [StringLength(500)]
    public string? AvatarUrl { get; set; }

    [StringLength(500)]
    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiry { get; set; }

    // Computed property
    public bool IsExternalLogin => !string.IsNullOrEmpty(ExternalProvider);

    [InverseProperty("User")]
    public virtual ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();

    [InverseProperty("User")]
    public virtual Customer? Customer { get; set; }

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<Apikey> Apikeys { get; set; } = new List<Apikey>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<Appointment> AppointmentCreatedByNavigations { get; set; } = new List<Appointment>();

    [InverseProperty("PreferredTechnician")]
    public virtual ICollection<Appointment> AppointmentPreferredTechnicians { get; set; } = new List<Appointment>();

    [InverseProperty("UpdatedByNavigation")]
    public virtual ICollection<Appointment> AppointmentUpdatedByNavigations { get; set; } = new List<Appointment>();

    /// <summary>
    /// Danh sách appointments đã được complete bởi user này (Staff/Technician)
    /// </summary>
    [InverseProperty("CompletedByNavigation")]
    public virtual ICollection<Appointment> AppointmentCompletedByNavigations { get; set; } = new List<Appointment>();

    /// <summary>
    /// Danh sách audit logs đã được thực hiện bởi user này (Admin/Staff)
    /// Track mọi thay đổi ServiceSource
    /// </summary>
    [InverseProperty("ChangedByUser")]
    public virtual ICollection<EVServiceCenter.Core.Entities.ServiceSourceAuditLog> ServiceSourceAuditLogs { get; set; } = new List<EVServiceCenter.Core.Entities.ServiceSourceAuditLog>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<AutoNotificationRule> AutoNotificationRules { get; set; } = new List<AutoNotificationRule>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<BusinessRule> BusinessRules { get; set; } = new List<BusinessRule>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<Certification> CertificationCreatedByNavigations { get; set; } = new List<Certification>();

    [InverseProperty("User")]
    public virtual ICollection<Certification> CertificationUsers { get; set; } = new List<Certification>();

    [InverseProperty("AssignedUser")]
    public virtual ICollection<ChatChannel> ChatChannelAssignedUsers { get; set; } = new List<ChatChannel>();

    [InverseProperty("ClosedByNavigation")]
    public virtual ICollection<ChatChannel> ChatChannelClosedByNavigations { get; set; } = new List<ChatChannel>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<ChatQuickReply> ChatQuickReplies { get; set; } = new List<ChatQuickReply>();

    [InverseProperty("CompletedByNavigation")]
    public virtual ICollection<ChecklistItem> ChecklistItems { get; set; } = new List<ChecklistItem>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<ChecklistTemplate> ChecklistTemplates { get; set; } = new List<ChecklistTemplate>();

    [ForeignKey("CreatedBy")]
    [InverseProperty("InverseCreatedByNavigation")]
    public virtual User? CreatedByNavigation { get; set; }

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<Customer> CustomerCreatedByNavigations { get; set; } = new List<Customer>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<CustomerPackageSubscription> CustomerPackageSubscriptions { get; set; } = new List<CustomerPackageSubscription>();

    [InverseProperty("UpdatedByNavigation")]
    public virtual ICollection<Customer> CustomerUpdatedByNavigations { get; set; } = new List<Customer>();

    [InverseProperty("UpdatedByNavigation")]
    public virtual ICollection<CustomerVehicle> CustomerVehicles { get; set; } = new List<CustomerVehicle>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<DataRetentionPolicy> DataRetentionPolicies { get; set; } = new List<DataRetentionPolicy>();

    [InverseProperty("Manager")]
    public virtual ICollection<Department> Departments { get; set; } = new List<Department>();

    [InverseProperty("User")]
    public virtual ICollection<EmployeeSkill> EmployeeSkillUsers { get; set; } = new List<EmployeeSkill>();

    [InverseProperty("VerifiedByNavigation")]
    public virtual ICollection<EmployeeSkill> EmployeeSkillVerifiedByNavigations { get; set; } = new List<EmployeeSkill>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<User> InverseCreatedByNavigation { get; set; } = new List<User>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<Invoice> InvoiceCreatedByNavigations { get; set; } = new List<Invoice>();

    [InverseProperty("UpdatedByNavigation")]
    public virtual ICollection<Invoice> InvoiceUpdatedByNavigations { get; set; } = new List<Invoice>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<LoyaltyProgram> LoyaltyPrograms { get; set; } = new List<LoyaltyProgram>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<LoyaltyTransaction> LoyaltyTransactions { get; set; } = new List<LoyaltyTransaction>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<Notification> NotificationCreatedByNavigations { get; set; } = new List<Notification>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<NotificationTemplate> NotificationTemplates { get; set; } = new List<NotificationTemplate>();

    [InverseProperty("User")]
    public virtual ICollection<Notification> NotificationUsers { get; set; } = new List<Notification>();

    [InverseProperty("UpdatedByNavigation")]
    public virtual ICollection<PartInventory> PartInventories { get; set; } = new List<PartInventory>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<Part> Parts { get; set; } = new List<Part>();

    [InverseProperty("ProcessedByNavigation")]
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    [InverseProperty("User")]
    public virtual ICollection<PerformanceMetric> PerformanceMetrics { get; set; } = new List<PerformanceMetric>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<Promotion> Promotions { get; set; } = new List<Promotion>();

    [InverseProperty("ApprovedByNavigation")]
    public virtual ICollection<PurchaseOrder> PurchaseOrderApprovedByNavigations { get; set; } = new List<PurchaseOrder>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<PurchaseOrder> PurchaseOrderCreatedByNavigations { get; set; } = new List<PurchaseOrder>();

    [InverseProperty("GeneratedByNavigation")]
    public virtual ICollection<Report> Reports { get; set; } = new List<Report>();

    [ForeignKey("RoleId")]
    [InverseProperty("Users")]
    public virtual UserRole Role { get; set; } = null!;

    [InverseProperty("ResolvedByNavigation")]
    public virtual ICollection<SecurityEvent> SecurityEventResolvedByNavigations { get; set; } = new List<SecurityEvent>();

    [InverseProperty("User")]
    public virtual ICollection<SecurityEvent> SecurityEventUsers { get; set; } = new List<SecurityEvent>();

    [InverseProperty("Manager")]
    public virtual ICollection<ServiceCenter> ServiceCenters { get; set; } = new List<ServiceCenter>();

    [InverseProperty("Advisor")]
    public virtual ICollection<ServiceRating> ServiceRatingAdvisors { get; set; } = new List<ServiceRating>();

    [InverseProperty("RespondedByNavigation")]
    public virtual ICollection<ServiceRating> ServiceRatingRespondedByNavigations { get; set; } = new List<ServiceRating>();

    [InverseProperty("Technician")]
    public virtual ICollection<ServiceRating> ServiceRatingTechnicians { get; set; } = new List<ServiceRating>();

    [InverseProperty("ApprovedByNavigation")]
    public virtual ICollection<Shift> ShiftApprovedByNavigations { get; set; } = new List<Shift>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<Shift> ShiftCreatedByNavigations { get; set; } = new List<Shift>();

    [InverseProperty("User")]
    public virtual ICollection<Shift> ShiftUsers { get; set; } = new List<Shift>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<StockTransaction> StockTransactions { get; set; } = new List<StockTransaction>();

    [InverseProperty("UpdatedByNavigation")]
    public virtual ICollection<SystemConfiguration> SystemConfigurations { get; set; } = new List<SystemConfiguration>();

    [InverseProperty("Technician")]
    public virtual ICollection<TechnicianSchedule> TechnicianSchedules { get; set; } = new List<TechnicianSchedule>();

    [InverseProperty("User")]
    public virtual ICollection<UserSession> UserSessions { get; set; } = new List<UserSession>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<VehicleCustomService> VehicleCustomServices { get; set; } = new List<VehicleCustomService>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<VehicleHealthMetric> VehicleHealthMetrics { get; set; } = new List<VehicleHealthMetric>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<Warranty> Warranties { get; set; } = new List<Warranty>();

    [InverseProperty("Advisor")]
    public virtual ICollection<WorkOrder> WorkOrderAdvisors { get; set; } = new List<WorkOrder>();

    [InverseProperty("ApprovedByNavigation")]
    public virtual ICollection<WorkOrder> WorkOrderApprovedByNavigations { get; set; } = new List<WorkOrder>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<WorkOrder> WorkOrderCreatedByNavigations { get; set; } = new List<WorkOrder>();

    [InverseProperty("InstalledByNavigation")]
    public virtual ICollection<WorkOrderPart> WorkOrderParts { get; set; } = new List<WorkOrderPart>();

    [InverseProperty("QualityCheckedByNavigation")]
    public virtual ICollection<WorkOrder> WorkOrderQualityCheckedByNavigations { get; set; } = new List<WorkOrder>();

    [InverseProperty("Technician")]
    public virtual ICollection<WorkOrderService> WorkOrderServices { get; set; } = new List<WorkOrderService>();

    [InverseProperty("Supervisor")]
    public virtual ICollection<WorkOrder> WorkOrderSupervisors { get; set; } = new List<WorkOrder>();

    [InverseProperty("Technician")]
    public virtual ICollection<WorkOrder> WorkOrderTechnicians { get; set; } = new List<WorkOrder>();

    [InverseProperty("PerformedByNavigation")]
    public virtual ICollection<WorkOrderTimeline> WorkOrderTimelines { get; set; } = new List<WorkOrderTimeline>();

    [InverseProperty("UpdatedByNavigation")]
    public virtual ICollection<WorkOrder> WorkOrderUpdatedByNavigations { get; set; } = new List<WorkOrder>();
}
