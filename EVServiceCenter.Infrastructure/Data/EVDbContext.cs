using System;
using System.Collections.Generic;
using EVServiceCenter.Core.Domains.AppointmentManagement.Entities;
using EVServiceCenter.Core.Domains.CarBrands.Entities;
using EVServiceCenter.Core.Domains.CarModels.Entities;
using EVServiceCenter.Core.Domains.Customers.Entities;
using EVServiceCenter.Core.Domains.CustomerTypes.Entities;
using EVServiceCenter.Core.Domains.CustomerVehicles.Entities;
using EVServiceCenter.Core.Domains.Identity.Entities;
using EVServiceCenter.Core.Domains.MaintenanceServices.Entities;
using EVServiceCenter.Core.Domains.ServiceCategories.Entities;
using EVServiceCenter.Core.Domains.ServiceCenters.Entities;
using Microsoft.EntityFrameworkCore;

namespace EVServiceCenter.Core.Entities;

public partial class EVDbContext : DbContext
{
    public EVDbContext()
    {
    }

    public EVDbContext(DbContextOptions<EVDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ActivityLog> ActivityLogs { get; set; }

    public virtual DbSet<Apikey> Apikeys { get; set; }

    public virtual DbSet<ApirequestLog> ApirequestLogs { get; set; }

    public virtual DbSet<Appointment> Appointments { get; set; }

    public virtual DbSet<AppointmentStatus> AppointmentStatuses { get; set; }

    public virtual DbSet<AutoNotificationRule> AutoNotificationRules { get; set; }

    public virtual DbSet<BusinessRule> BusinessRules { get; set; }

    public virtual DbSet<CarBrand> CarBrands { get; set; }

    public virtual DbSet<CarModel> CarModels { get; set; }

    public virtual DbSet<Certification> Certifications { get; set; }

    public virtual DbSet<ChatChannel> ChatChannels { get; set; }

    public virtual DbSet<ChatMessage> ChatMessages { get; set; }

    public virtual DbSet<ChatQuickReply> ChatQuickReplies { get; set; }

    public virtual DbSet<ChecklistItem> ChecklistItems { get; set; }

    public virtual DbSet<ChecklistTemplate> ChecklistTemplates { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<CustomerCommunicationPreference> CustomerCommunicationPreferences { get; set; }

    public virtual DbSet<CustomerPackageSubscription> CustomerPackageSubscriptions { get; set; }

    public virtual DbSet<CustomerType> CustomerTypes { get; set; }

    public virtual DbSet<CustomerVehicle> CustomerVehicles { get; set; }

    public virtual DbSet<DailyMetric> DailyMetrics { get; set; }

    public virtual DbSet<DataRetentionPolicy> DataRetentionPolicies { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<EmployeeSkill> EmployeeSkills { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<Kpimetric> Kpimetrics { get; set; }

    public virtual DbSet<LoyaltyProgram> LoyaltyPrograms { get; set; }

    public virtual DbSet<LoyaltyTransaction> LoyaltyTransactions { get; set; }

    public virtual DbSet<MaintenanceHistory> MaintenanceHistories { get; set; }

    public virtual DbSet<MaintenancePackage> MaintenancePackages { get; set; }

    public virtual DbSet<MaintenanceService> MaintenanceServices { get; set; }

    public virtual DbSet<ModelServicePricing> ModelServicePricings { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<NotificationDeliveryLog> NotificationDeliveryLogs { get; set; }

    public virtual DbSet<NotificationTemplate> NotificationTemplates { get; set; }

    public virtual DbSet<NotificationType> NotificationTypes { get; set; }

    public virtual DbSet<OnlinePayment> OnlinePayments { get; set; }

    public virtual DbSet<PackageService> PackageServices { get; set; }

    public virtual DbSet<Part> Parts { get; set; }

    public virtual DbSet<PartCategory> PartCategories { get; set; }

    public virtual DbSet<PartInventory> PartInventories { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<PaymentMethod> PaymentMethods { get; set; }

    public virtual DbSet<PerformanceMetric> PerformanceMetrics { get; set; }

    public virtual DbSet<Promotion> Promotions { get; set; }

    public virtual DbSet<PurchaseOrder> PurchaseOrders { get; set; }

    public virtual DbSet<PurchaseOrderDetail> PurchaseOrderDetails { get; set; }

    public virtual DbSet<Report> Reports { get; set; }

    public virtual DbSet<SecurityEvent> SecurityEvents { get; set; }

    public virtual DbSet<ServiceCategory> ServiceCategories { get; set; }

    public virtual DbSet<ServiceCenter> ServiceCenters { get; set; }

    public virtual DbSet<ServiceRating> ServiceRatings { get; set; }

    public virtual DbSet<Shift> Shifts { get; set; }

    public virtual DbSet<StockTransaction> StockTransactions { get; set; }

    public virtual DbSet<Supplier> Suppliers { get; set; }

    public virtual DbSet<SystemConfiguration> SystemConfigurations { get; set; }

    public virtual DbSet<TechnicianSchedule> TechnicianSchedules { get; set; }

    public virtual DbSet<TimeSlot> TimeSlots { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    public virtual DbSet<UserSession> UserSessions { get; set; }

    public virtual DbSet<VehicleCustomService> VehicleCustomServices { get; set; }

    public virtual DbSet<VehicleHealthMetric> VehicleHealthMetrics { get; set; }

    public virtual DbSet<Warranty> Warranties { get; set; }

    public virtual DbSet<WarrantyType> WarrantyTypes { get; set; }

    public virtual DbSet<WorkOrder> WorkOrders { get; set; }

    public virtual DbSet<WorkOrderPart> WorkOrderParts { get; set; }

    public virtual DbSet<WorkOrderService> WorkOrderServices { get; set; }

    public virtual DbSet<WorkOrderStatus> WorkOrderStatuses { get; set; }

    public virtual DbSet<WorkOrderTimeline> WorkOrderTimelines { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=LAPTOP-774TME8F;Initial Catalog=EVServiceCenterV2;Persist Security Info=True;User ID=sa;Password=12345;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=true");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ActivityLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__Activity__5E5499A89BA8FBB4");

            entity.Property(e => e.ChangeDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Severity).HasDefaultValue("Info");
            entity.Property(e => e.Success).HasDefaultValue(true);

            entity.HasOne(d => d.Session).WithMany(p => p.ActivityLogs).HasConstraintName("FK__ActivityL__Sessi__4336F4B9");

            entity.HasOne(d => d.User).WithMany(p => p.ActivityLogs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ActivityL__UserI__4242D080");
        });

        modelBuilder.Entity<Apikey>(entity =>
        {
            entity.HasKey(e => e.KeyId).HasName("PK__APIKeys__21F5BE2711270D11");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.CurrentUsage).HasDefaultValue(0);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.KeyType).HasDefaultValue("Internal");
            entity.Property(e => e.RateLimit).HasDefaultValue(1000);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Apikeys).HasConstraintName("FK__APIKeys__Created__0504B816");
        });

        modelBuilder.Entity<ApirequestLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__APIReque__5E5499A8CDACA85C");

            entity.Property(e => e.RequestDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Key).WithMany(p => p.ApirequestLogs).HasConstraintName("FK__APIReques__KeyID__08D548FA");
        });

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.AppointmentId).HasName("PK__Appointm__8ECDFCA2E3758D0D");

            entity.Property(e => e.ConfirmationStatus).HasDefaultValue("Pending");
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.NoShowFlag).HasDefaultValue(false);
            entity.Property(e => e.Priority).HasDefaultValue("Normal");
            entity.Property(e => e.ReminderSent).HasDefaultValue(false);
            entity.Property(e => e.Source).HasDefaultValue("Walk-in");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.AppointmentCreatedByNavigations).HasConstraintName("FK__Appointme__Creat__56E8E7AB");

            entity.HasOne(d => d.Customer).WithMany(p => p.Appointments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Appointme__Custo__4E53A1AA");

            entity.HasOne(d => d.Package).WithMany(p => p.Appointments).HasConstraintName("FK__Appointme__Packa__5224328E");

            entity.HasOne(d => d.PreferredTechnician).WithMany(p => p.AppointmentPreferredTechnicians).HasConstraintName("FK__Appointme__Prefe__540C7B00");

            entity.HasOne(d => d.RescheduledFrom).WithMany(p => p.InverseRescheduledFrom).HasConstraintName("FK__Appointme__Resch__55F4C372");

            entity.HasOne(d => d.ServiceCenter).WithMany(p => p.Appointments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Appointme__Servi__503BEA1C");

            entity.HasOne(d => d.Service).WithMany(p => p.Appointments).HasConstraintName("FK__Appointme__Servi__51300E55");

            entity.HasOne(d => d.Slot).WithMany(p => p.Appointments).HasConstraintName("FK__Appointme__SlotI__531856C7");

            entity.HasOne(d => d.Status).WithMany(p => p.Appointments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Appointme__Statu__55009F39");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.AppointmentUpdatedByNavigations).HasConstraintName("FK__Appointme__Updat__57DD0BE4");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.Appointments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Appointme__Vehic__4F47C5E3");
        });

        modelBuilder.Entity<AppointmentStatus>(entity =>
        {
            entity.HasKey(e => e.StatusId).HasName("PK__Appointm__C8EE204358988DD1");

            entity.Property(e => e.DisplayOrder).HasDefaultValue(0);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<AutoNotificationRule>(entity =>
        {
            entity.HasKey(e => e.RuleId).HasName("PK__AutoNoti__110458C29FBAE080");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MaxRetries).HasDefaultValue(3);
            entity.Property(e => e.Priority).HasDefaultValue("Normal");
            entity.Property(e => e.RetryInterval).HasDefaultValue(60);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.AutoNotificationRules).HasConstraintName("FK__AutoNotif__Creat__5C37ACAD");

            entity.HasOne(d => d.Template).WithMany(p => p.AutoNotificationRules)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__AutoNotif__Templ__5B438874");
        });

        modelBuilder.Entity<BusinessRule>(entity =>
        {
            entity.HasKey(e => e.RuleId).HasName("PK__Business__110458C2A7189762");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.EffectiveDate).HasDefaultValueSql("(CONVERT([date],getdate()))");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Priority).HasDefaultValue(0);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.BusinessRules).HasConstraintName("FK__BusinessR__Creat__5EDF0F2E");
        });

        modelBuilder.Entity<CarBrand>(entity =>
        {
            entity.HasKey(e => e.BrandId).HasName("PK__CarBrand__DAD4F3BEFB91F09A");

            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<CarModel>(entity =>
        {
            entity.HasKey(e => e.ModelId).HasName("PK__CarModel__E8D7A1CC320085B4");

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ServiceInterval).HasDefaultValue(10000);
            entity.Property(e => e.ServiceIntervalMonths).HasDefaultValue(6);
            entity.Property(e => e.WarrantyPeriod).HasDefaultValue(24);

            entity.HasOne(d => d.Brand).WithMany(p => p.CarModels)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CarModels__Brand__00200768");
        });

        modelBuilder.Entity<Certification>(entity =>
        {
            entity.HasKey(e => e.CertificationId).HasName("PK__Certific__1237E5AA18E73C54");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.RenewalReminderSent).HasDefaultValue(false);
            entity.Property(e => e.RenewalRequired).HasDefaultValue(false);
            entity.Property(e => e.Status).HasDefaultValue("Active");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.CertificationCreatedByNavigations).HasConstraintName("FK__Certifica__Creat__3C89F72A");

            entity.HasOne(d => d.User).WithMany(p => p.CertificationUsers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Certifica__UserI__3B95D2F1");
        });

        modelBuilder.Entity<ChatChannel>(entity =>
        {
            entity.HasKey(e => e.ChannelId).HasName("PK__ChatChan__38C3E8F48AF9D1A3");

            entity.Property(e => e.ChannelType).HasDefaultValue("Support");
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Priority).HasDefaultValue("Normal");
            entity.Property(e => e.Status).HasDefaultValue("Active");

            entity.HasOne(d => d.AssignedUser).WithMany(p => p.ChatChannelAssignedUsers).HasConstraintName("FK__ChatChann__Assig__00750D23");

            entity.HasOne(d => d.ClosedByNavigation).WithMany(p => p.ChatChannelClosedByNavigations).HasConstraintName("FK__ChatChann__Close__0169315C");

            entity.HasOne(d => d.Customer).WithMany(p => p.ChatChannels).HasConstraintName("FK__ChatChann__Custo__7F80E8EA");
        });

        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasKey(e => e.MessageId).HasName("PK__ChatMess__C87C037C9BFABD56");

            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.IsDelivered).HasDefaultValue(false);
            entity.Property(e => e.IsRead).HasDefaultValue(false);
            entity.Property(e => e.MessageType).HasDefaultValue("Text");
            entity.Property(e => e.Timestamp).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Channel).WithMany(p => p.ChatMessages)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChatMessa__Chann__090A5324");

            entity.HasOne(d => d.RelatedAppointment).WithMany(p => p.ChatMessages).HasConstraintName("FK__ChatMessa__Relat__0AF29B96");

            entity.HasOne(d => d.RelatedInvoice).WithMany(p => p.ChatMessages).HasConstraintName("FK__ChatMessa__Relat__0CDAE408");

            entity.HasOne(d => d.RelatedWorkOrder).WithMany(p => p.ChatMessages).HasConstraintName("FK__ChatMessa__Relat__0BE6BFCF");

            entity.HasOne(d => d.ReplyToMessage).WithMany(p => p.InverseReplyToMessage).HasConstraintName("FK__ChatMessa__Reply__09FE775D");
        });

        modelBuilder.Entity<ChatQuickReply>(entity =>
        {
            entity.HasKey(e => e.QuickReplyId).HasName("PK__ChatQuic__C682A55EBE5F3B85");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.UseCount).HasDefaultValue(0);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.ChatQuickReplies).HasConstraintName("FK__ChatQuick__Creat__1293BD5E");
        });

        modelBuilder.Entity<ChecklistItem>(entity =>
        {
            entity.HasKey(e => e.ItemId).HasName("PK__Checklis__727E83EBA695EE5D");

            entity.Property(e => e.IsCompleted).HasDefaultValue(false);
            entity.Property(e => e.IsRequired).HasDefaultValue(true);
            entity.Property(e => e.ItemOrder).HasDefaultValue(1);

            entity.HasOne(d => d.CompletedByNavigation).WithMany(p => p.ChecklistItems).HasConstraintName("FK__Checklist__Compl__14E61A24");

            entity.HasOne(d => d.Template).WithMany(p => p.ChecklistItems).HasConstraintName("FK__Checklist__Templ__13F1F5EB");

            entity.HasOne(d => d.WorkOrder).WithMany(p => p.ChecklistItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Checklist__WorkO__12FDD1B2");
        });

        modelBuilder.Entity<ChecklistTemplate>(entity =>
        {
            entity.HasKey(e => e.TemplateId).HasName("PK__Checklis__F87ADD07FE6D782B");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.Category).WithMany(p => p.ChecklistTemplates).HasConstraintName("FK__Checklist__Categ__0C50D423");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.ChecklistTemplates).HasConstraintName("FK__Checklist__Creat__0D44F85C");

            entity.HasOne(d => d.Service).WithMany(p => p.ChecklistTemplates).HasConstraintName("FK__Checklist__Servi__0B5CAFEA");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId).HasName("PK__Customer__A4AE64B8632A69D9");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LoyaltyPoints).HasDefaultValue(0);
            entity.Property(e => e.MarketingOptIn).HasDefaultValue(true);
            entity.Property(e => e.PreferredLanguage).HasDefaultValue("vi-VN");
            entity.Property(e => e.TotalSpent).HasDefaultValue(0m);
            entity.Property(e => e.TypeId).HasDefaultValue(1);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.CustomerCreatedByNavigations).HasConstraintName("FK__Customers__Creat__6B24EA82");

            entity.HasOne(d => d.Type).WithMany(p => p.Customers).HasConstraintName("FK__Customers__TypeI__6A30C649");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.CustomerUpdatedByNavigations).HasConstraintName("FK__Customers__Updat__6C190EBB");

            entity.HasOne(d => d.User)
             .WithOne(u => u.Customer)
             .HasForeignKey<Customer>(d => d.UserId)
             .IsRequired(false)  // UserId nullable
             .OnDelete(DeleteBehavior.Restrict)  
             .HasConstraintName("FK_Customers_Users");

            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("IX_Customers_UserID")
                .IsUnique()
                .HasFilter("[UserID] IS NOT NULL"); 
        });

        modelBuilder.Entity<CustomerCommunicationPreference>(entity =>
        {
            entity.HasKey(e => e.PreferenceId).HasName("PK__Customer__E228490FEC3F25E4");

            entity.Property(e => e.EmailNotifications).HasDefaultValue(true);
            entity.Property(e => e.MarketingCommunications).HasDefaultValue(false);
            entity.Property(e => e.PromotionalOffers).HasDefaultValue(false);
            entity.Property(e => e.PushNotifications).HasDefaultValue(true);
            entity.Property(e => e.ServiceReminders).HasDefaultValue(true);
            entity.Property(e => e.Smsnotifications).HasDefaultValue(true);
            entity.Property(e => e.UpdatedDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Customer).WithMany(p => p.CustomerCommunicationPreferences)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CustomerC__Custo__75A278F5");
        });

        modelBuilder.Entity<CustomerPackageSubscription>(entity =>
        {
            entity.HasKey(e => e.SubscriptionId).HasName("PK__Customer__9A2B24BD49D761F8");

            entity.Property(e => e.AutoRenew).HasDefaultValue(false);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.DiscountPercent).HasDefaultValue(0m);
            entity.Property(e => e.RemainingServices).HasDefaultValue(0);
            entity.Property(e => e.Status).HasDefaultValue("Active");
            entity.Property(e => e.UsedServices).HasDefaultValue(0);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.CustomerPackageSubscriptions).HasConstraintName("FK__CustomerP__Creat__6991A7CB");

            entity.HasOne(d => d.Customer).WithMany(p => p.CustomerPackageSubscriptions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CustomerP__Custo__65C116E7");

            entity.HasOne(d => d.Package).WithMany(p => p.CustomerPackageSubscriptions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CustomerP__Packa__66B53B20");

            entity.HasOne(d => d.PaymentMethod).WithMany(p => p.CustomerPackageSubscriptions).HasConstraintName("FK__CustomerP__Payme__689D8392");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.CustomerPackageSubscriptions).HasConstraintName("FK__CustomerP__Vehic__67A95F59");
        });

        modelBuilder.Entity<CustomerType>(entity =>
        {
            entity.HasKey(e => e.TypeId).HasName("PK__Customer__516F03952CD0AA46");

            entity.Property(e => e.DiscountPercent).HasDefaultValue(0m);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<CustomerVehicle>(entity =>
        {
            entity.HasKey(e => e.VehicleId).HasName("PK__Customer__476B54B26D8CEE26");

            entity.Property(e => e.BatteryHealthPercent).HasDefaultValue(100m);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LastMaintenanceMileage).HasDefaultValue(0);
            entity.Property(e => e.Mileage).HasDefaultValue(0);
            entity.Property(e => e.VehicleCondition).HasDefaultValue("Good");

            entity.HasOne(d => d.Customer).WithMany(p => p.CustomerVehicles)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CustomerV__Custo__0A9D95DB");

            entity.HasOne(d => d.Model).WithMany(p => p.CustomerVehicles)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CustomerV__Model__0B91BA14");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.CustomerVehicles).HasConstraintName("FK__CustomerV__Updat__0C85DE4D");
        });

        modelBuilder.Entity<DailyMetric>(entity =>
        {
            entity.HasKey(e => e.DailyMetricId).HasName("PK__DailyMet__D09962A318FC9D36");

            entity.Property(e => e.AppointmentsCancelled).HasDefaultValue(0);
            entity.Property(e => e.AppointmentsCompleted).HasDefaultValue(0);
            entity.Property(e => e.AppointmentsScheduled).HasDefaultValue(0);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.DailyRevenue).HasDefaultValue(0m);
            entity.Property(e => e.NewCustomers).HasDefaultValue(0);
            entity.Property(e => e.PartsRevenue).HasDefaultValue(0m);
            entity.Property(e => e.ReworkCount).HasDefaultValue(0);
            entity.Property(e => e.ServiceRevenue).HasDefaultValue(0m);
            entity.Property(e => e.WarrantyClaimsCount).HasDefaultValue(0);
            entity.Property(e => e.WorkOrdersCompleted).HasDefaultValue(0);
            entity.Property(e => e.WorkOrdersCreated).HasDefaultValue(0);

            entity.HasOne(d => d.Center).WithMany(p => p.DailyMetrics).HasConstraintName("FK__DailyMetr__Cente__2F650636");
        });

        modelBuilder.Entity<DataRetentionPolicy>(entity =>
        {
            entity.HasKey(e => e.PolicyId).HasName("PK__DataRete__2E133944D99AE100");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.DeleteAfterArchive).HasDefaultValue(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.DataRetentionPolicies).HasConstraintName("FK__DataReten__Creat__7C6F7215");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.DepartmentId).HasName("PK__Departme__B2079BCD7A1C4AC9");

            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.Center).WithMany(p => p.Departments).HasConstraintName("FK__Departmen__Cente__184C96B4");

            entity.HasOne(d => d.Manager).WithMany(p => p.Departments).HasConstraintName("FK__Departmen__Manag__1758727B");
        });

        modelBuilder.Entity<EmployeeSkill>(entity =>
        {
            entity.HasKey(e => e.SkillId).HasName("PK__Employee__DFA091E7A81A406B");

            entity.Property(e => e.IsVerified).HasDefaultValue(false);

            entity.HasOne(d => d.User).WithMany(p => p.EmployeeSkillUsers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__EmployeeS__UserI__1C1D2798");

            entity.HasOne(d => d.VerifiedByNavigation).WithMany(p => p.EmployeeSkillVerifiedByNavigations).HasConstraintName("FK__EmployeeS__Verif__1D114BD1");
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.InvoiceId).HasName("PK__Invoices__D796AAD526195D8C");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.GrandTotal).HasComputedColumnSql("((([ServiceSubTotal]-[ServiceDiscount])+[ServiceTax])+(([PartsSubTotal]-[PartsDiscount])+[PartsTax]))", true);
            entity.Property(e => e.InvoiceDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.OutstandingAmount).HasComputedColumnSql("(((([ServiceSubTotal]-[ServiceDiscount])+[ServiceTax])+(([PartsSubTotal]-[PartsDiscount])+[PartsTax]))-[PaidAmount])", true);
            entity.Property(e => e.PaidAmount).HasDefaultValue(0m);
            entity.Property(e => e.PartsDiscount).HasDefaultValue(0m);
            entity.Property(e => e.PartsSubTotal).HasDefaultValue(0m);
            entity.Property(e => e.PartsTax).HasDefaultValue(0m);
            entity.Property(e => e.PartsTotal).HasComputedColumnSql("(([PartsSubTotal]-[PartsDiscount])+[PartsTax])", true);
            entity.Property(e => e.SentToCustomer).HasDefaultValue(false);
            entity.Property(e => e.ServiceDiscount).HasDefaultValue(0m);
            entity.Property(e => e.ServiceSubTotal).HasDefaultValue(0m);
            entity.Property(e => e.ServiceTax).HasDefaultValue(0m);
            entity.Property(e => e.ServiceTotal).HasComputedColumnSql("(([ServiceSubTotal]-[ServiceDiscount])+[ServiceTax])", true);
            entity.Property(e => e.Status).HasDefaultValue("Draft");
            entity.Property(e => e.SubTotal).HasComputedColumnSql("([ServiceSubTotal]+[PartsSubTotal])", true);
            entity.Property(e => e.TotalDiscount).HasComputedColumnSql("([ServiceDiscount]+[PartsDiscount])", true);
            entity.Property(e => e.TotalTax).HasComputedColumnSql("([ServiceTax]+[PartsTax])", true);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.InvoiceCreatedByNavigations).HasConstraintName("FK__Invoices__Create__725BF7F6");

            entity.HasOne(d => d.Customer).WithMany(p => p.Invoices)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Invoices__Custom__7167D3BD");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.InvoiceUpdatedByNavigations).HasConstraintName("FK__Invoices__Update__73501C2F");

            entity.HasOne(d => d.WorkOrder).WithMany(p => p.Invoices)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Invoices__WorkOr__7073AF84");
        });

        modelBuilder.Entity<Kpimetric>(entity =>
        {
            entity.HasKey(e => e.MetricId).HasName("PK__KPIMetri__561056459F678B59");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<LoyaltyProgram>(entity =>
        {
            entity.HasKey(e => e.ProgramId).HasName("PK__LoyaltyP__752560385C07EBF9");

            entity.Property(e => e.BirthdayBonus).HasDefaultValue(0);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MinimumRedemption).HasDefaultValue(100);
            entity.Property(e => e.PointsExpiryDays).HasDefaultValue(365);
            entity.Property(e => e.PointsPerDollar).HasDefaultValue(1.0m);
            entity.Property(e => e.ReferralBonus).HasDefaultValue(0);
            entity.Property(e => e.StartDate).HasDefaultValueSql("(CONVERT([date],getdate()))");
            entity.Property(e => e.WelcomeBonus).HasDefaultValue(0);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.LoyaltyPrograms).HasConstraintName("FK__LoyaltyPr__Creat__6A50C1DA");
        });

        modelBuilder.Entity<LoyaltyTransaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("PK__LoyaltyT__55433A4B990C52E0");

            entity.Property(e => e.TransactionDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.LoyaltyTransactions).HasConstraintName("FK__LoyaltyTr__Creat__70099B30");

            entity.HasOne(d => d.Customer).WithMany(p => p.LoyaltyTransactions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LoyaltyTr__Custo__6E2152BE");

            entity.HasOne(d => d.Program).WithMany(p => p.LoyaltyTransactions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LoyaltyTr__Progr__6F1576F7");
        });

        modelBuilder.Entity<MaintenanceHistory>(entity =>
        {
            entity.HasKey(e => e.HistoryId).HasName("PK__Maintena__4D7B4ADDD775C4B8");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.TotalCost).HasComputedColumnSql("([TotalServiceCost]+[TotalPartsCost])", true);

            entity.HasOne(d => d.Vehicle).WithMany(p => p.MaintenanceHistories)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Maintenan__Vehic__15A53433");

            entity.HasOne(d => d.WorkOrder).WithMany(p => p.MaintenanceHistories)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Maintenan__WorkO__1699586C");
        });

        modelBuilder.Entity<MaintenancePackage>(entity =>
        {
            entity.HasKey(e => e.PackageId).HasName("PK__Maintena__322035EC3FB048A9");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsPopular).HasDefaultValue(false);
        });

        modelBuilder.Entity<MaintenanceService>(entity =>
        {
            entity.HasKey(e => e.ServiceId).HasName("PK__Maintena__C51BB0EA01301206");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsWarrantyService).HasDefaultValue(false);

            entity.HasOne(d => d.Category).WithMany(p => p.MaintenanceServices)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Maintenan__Categ__17036CC0");
        });

        modelBuilder.Entity<ModelServicePricing>(entity =>
        {
            entity.HasKey(e => e.PricingId).HasName("PK__ModelSer__EC306B7289B90AA9");

            entity.Property(e => e.EffectiveDate).HasDefaultValueSql("(CONVERT([date],getdate()))");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.Model).WithMany(p => p.ModelServicePricings)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ModelServ__Model__282DF8C2");

            entity.HasOne(d => d.Service).WithMany(p => p.ModelServicePricings)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ModelServ__Servi__29221CFB");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__20CF2E321CBA13BB");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Priority).HasDefaultValue("Normal");
            entity.Property(e => e.RetryCount).HasDefaultValue(0);
            entity.Property(e => e.Status).HasDefaultValue("Pending");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.NotificationCreatedByNavigations).HasConstraintName("FK__Notificat__Creat__740F363E");

            entity.HasOne(d => d.Customer).WithMany(p => p.Notifications).HasConstraintName("FK__Notificat__Custo__731B1205");

            entity.HasOne(d => d.Template).WithMany(p => p.Notifications).HasConstraintName("FK__Notificat__Templ__7132C993");

            entity.HasOne(d => d.User).WithMany(p => p.NotificationUsers).HasConstraintName("FK__Notificat__UserI__7226EDCC");
        });

        modelBuilder.Entity<NotificationDeliveryLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__Notifica__5E5499A87E1E7382");

            entity.Property(e => e.AttemptDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Cost).HasDefaultValue(0m);

            entity.HasOne(d => d.Notification).WithMany(p => p.NotificationDeliveryLogs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Notificat__Notif__78D3EB5B");
        });

        modelBuilder.Entity<NotificationTemplate>(entity =>
        {
            entity.HasKey(e => e.TemplateId).HasName("PK__Notifica__F87ADD07A2668407");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsAutomatic).HasDefaultValue(true);
            entity.Property(e => e.SendDelay).HasDefaultValue(0);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.NotificationTemplates).HasConstraintName("FK__Notificat__Creat__53A266AC");

            entity.HasOne(d => d.Type).WithMany(p => p.NotificationTemplates)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Notificat__TypeI__52AE4273");
        });

        modelBuilder.Entity<NotificationType>(entity =>
        {
            entity.HasKey(e => e.TypeId).HasName("PK__Notifica__516F03953D44EDA0");

            entity.Property(e => e.DefaultEnabled).HasDefaultValue(true);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<OnlinePayment>(entity =>
        {
            entity.HasKey(e => e.OnlinePaymentId).HasName("PK__OnlinePa__029C2CE0D6C97D07");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.PaymentStatus).HasDefaultValue("Pending");

            entity.HasOne(d => d.Payment).WithMany(p => p.OnlinePayments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OnlinePay__Payme__02925FBF");
        });

        modelBuilder.Entity<PackageService>(entity =>
        {
            entity.HasKey(e => e.PackageServiceId).HasName("PK__PackageS__5EAFC2105FEBAB58");

            entity.Property(e => e.AdditionalCost).HasDefaultValue(0m);
            entity.Property(e => e.IncludedInPackage).HasDefaultValue(true);
            entity.Property(e => e.Quantity).HasDefaultValue(1);

            entity.HasOne(d => d.Package).WithMany(p => p.PackageServices)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PackageSe__Packa__22751F6C");

            entity.HasOne(d => d.Service).WithMany(p => p.PackageServices)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PackageSe__Servi__236943A5");
        });

        modelBuilder.Entity<Part>(entity =>
        {
            entity.HasKey(e => e.PartId).HasName("PK__Parts__7C3F0D3053AF215E");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.CurrentStock).HasDefaultValue(0);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsConsumable).HasDefaultValue(false);
            entity.Property(e => e.LastStockUpdateDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.MaxStock).HasDefaultValue(1000);
            entity.Property(e => e.MinStock).HasDefaultValue(0);
            entity.Property(e => e.PartCondition).HasDefaultValue("New");
            entity.Property(e => e.ReorderLevel).HasDefaultValue(0);
            entity.Property(e => e.Unit).HasDefaultValue("PCS");
            entity.Property(e => e.WarrantyPeriod).HasDefaultValue(12);

            entity.HasOne(d => d.Brand).WithMany(p => p.Parts).HasConstraintName("FK__Parts__BrandID__2F9A1060");

            entity.HasOne(d => d.Category).WithMany(p => p.Parts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Parts__CategoryI__2EA5EC27");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Parts).HasConstraintName("FK__Parts__CreatedBy__318258D2");

            entity.HasOne(d => d.Supplier).WithMany(p => p.Parts).HasConstraintName("FK__Parts__SupplierI__308E3499");
        });

        modelBuilder.Entity<PartCategory>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__PartCate__19093A2B4A988338");

            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.ParentCategory).WithMany(p => p.InverseParentCategory).HasConstraintName("FK__PartCateg__Paren__18B6AB08");
        });

        modelBuilder.Entity<PartInventory>(entity =>
        {
            entity.HasKey(e => e.InventoryId).HasName("PK__PartInve__F5FDE6D3649D35C9");

            entity.Property(e => e.AvailableStock).HasComputedColumnSql("([CurrentStock]-[ReservedStock])", true);
            entity.Property(e => e.CurrentStock).HasDefaultValue(0);
            entity.Property(e => e.ReservedStock).HasDefaultValue(0);
            entity.Property(e => e.UpdatedDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Center).WithMany(p => p.PartInventories)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PartInven__Cente__382F5661");

            entity.HasOne(d => d.Part).WithMany(p => p.PartInventories)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PartInven__PartI__373B3228");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.PartInventories).HasConstraintName("FK__PartInven__Updat__39237A9A");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__Payments__9B556A587DB8A83F");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.NetAmount).HasComputedColumnSql("([Amount]-[ProcessingFee])", true);
            entity.Property(e => e.PaymentDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.ProcessingFee).HasDefaultValue(0m);
            entity.Property(e => e.RefundAmount).HasDefaultValue(0m);
            entity.Property(e => e.Status).HasDefaultValue("Completed");

            entity.HasOne(d => d.Invoice).WithMany(p => p.Payments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Payments__Invoic__7BE56230");

            entity.HasOne(d => d.Method).WithMany(p => p.Payments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Payments__Method__7CD98669");

            entity.HasOne(d => d.ProcessedByNavigation).WithMany(p => p.Payments).HasConstraintName("FK__Payments__Proces__7DCDAAA2");
        });

        modelBuilder.Entity<PaymentMethod>(entity =>
        {
            entity.HasKey(e => e.MethodId).HasName("PK__PaymentM__FC681FB15BC7676A");

            entity.Property(e => e.DisplayOrder).HasDefaultValue(0);
            entity.Property(e => e.FixedFee).HasDefaultValue(0m);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsOnline).HasDefaultValue(false);
            entity.Property(e => e.ProcessingFee).HasDefaultValue(0m);
            entity.Property(e => e.RequiresApproval).HasDefaultValue(false);
        });

        modelBuilder.Entity<PerformanceMetric>(entity =>
        {
            entity.HasKey(e => e.MetricId).HasName("PK__Performa__561056451E964448");

            entity.Property(e => e.AbsentCount).HasDefaultValue(0);
            entity.Property(e => e.CertificationsEarned).HasDefaultValue(0);
            entity.Property(e => e.ComplaintCount).HasDefaultValue(0);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.LateCount).HasDefaultValue(0);
            entity.Property(e => e.RevenueGenerated).HasDefaultValue(0m);
            entity.Property(e => e.ReworkCount).HasDefaultValue(0);
            entity.Property(e => e.ServicesCompleted).HasDefaultValue(0);
            entity.Property(e => e.TrainingHoursCompleted).HasDefaultValue(0m);
            entity.Property(e => e.WorkOrdersCompleted).HasDefaultValue(0);

            entity.HasOne(d => d.User).WithMany(p => p.PerformanceMetrics)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Performan__UserI__33F4B129");
        });

        modelBuilder.Entity<Promotion>(entity =>
        {
            entity.HasKey(e => e.PromotionId).HasName("PK__Promotio__52C42F2F955E70A8");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.UsageCount).HasDefaultValue(0);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Promotions).HasConstraintName("FK__Promotion__Creat__76B698BF");
        });

        modelBuilder.Entity<PurchaseOrder>(entity =>
        {
            entity.HasKey(e => e.PurchaseOrderId).HasName("PK__Purchase__036BAC449534989B");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.OrderDate).HasDefaultValueSql("(CONVERT([date],getdate()))");
            entity.Property(e => e.ShippingCost).HasDefaultValue(0m);
            entity.Property(e => e.Status).HasDefaultValue("Pending");
            entity.Property(e => e.SubTotal).HasDefaultValue(0m);
            entity.Property(e => e.TaxAmount).HasDefaultValue(0m);
            entity.Property(e => e.TotalAmount).HasComputedColumnSql("(([SubTotal]+[TaxAmount])+[ShippingCost])", true);

            entity.HasOne(d => d.ApprovedByNavigation).WithMany(p => p.PurchaseOrderApprovedByNavigations).HasConstraintName("FK__PurchaseO__Appro__52E34C9D");

            entity.HasOne(d => d.Center).WithMany(p => p.PurchaseOrders)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PurchaseO__Cente__51EF2864");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.PurchaseOrderCreatedByNavigations).HasConstraintName("FK__PurchaseO__Creat__53D770D6");

            entity.HasOne(d => d.Supplier).WithMany(p => p.PurchaseOrders)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PurchaseO__Suppl__50FB042B");
        });

        modelBuilder.Entity<PurchaseOrderDetail>(entity =>
        {
            entity.HasKey(e => e.PodetailId).HasName("PK__Purchase__4EB47B5EA0AA979C");

            entity.Property(e => e.ReceivedQuantity).HasDefaultValue(0);
            entity.Property(e => e.RemainingQuantity).HasComputedColumnSql("([Quantity]-[ReceivedQuantity])", true);
            entity.Property(e => e.TotalCost).HasComputedColumnSql("([Quantity]*[UnitCost])", true);

            entity.HasOne(d => d.Part).WithMany(p => p.PurchaseOrderDetails)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PurchaseO__PartI__589C25F3");

            entity.HasOne(d => d.PurchaseOrder).WithMany(p => p.PurchaseOrderDetails)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PurchaseO__Purch__57A801BA");
        });

        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasKey(e => e.ReportId).HasName("PK__Reports__D5BD48E5F07D7347");

            entity.Property(e => e.AccessCount).HasDefaultValue(0);
            entity.Property(e => e.GeneratedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsScheduled).HasDefaultValue(false);

            entity.HasOne(d => d.Center).WithMany(p => p.Reports).HasConstraintName("FK__Reports__CenterI__45544755");

            entity.HasOne(d => d.GeneratedByNavigation).WithMany(p => p.Reports).HasConstraintName("FK__Reports__Generat__46486B8E");
        });

        modelBuilder.Entity<SecurityEvent>(entity =>
        {
            entity.HasKey(e => e.EventId).HasName("PK__Security__7944C870A4DE4C35");

            entity.Property(e => e.EventDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsBlocked).HasDefaultValue(false);
            entity.Property(e => e.RiskLevel).HasDefaultValue("Low");

            entity.HasOne(d => d.ResolvedByNavigation).WithMany(p => p.SecurityEventResolvedByNavigations).HasConstraintName("FK__SecurityE__Resol__49E3F248");

            entity.HasOne(d => d.User).WithMany(p => p.SecurityEventUsers).HasConstraintName("FK__SecurityE__UserI__48EFCE0F");
        });

        modelBuilder.Entity<ServiceCategory>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__ServiceC__19093A2BFAC23C48");

            entity.Property(e => e.DisplayOrder).HasDefaultValue(0);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<ServiceCenter>(entity =>
        {
            entity.HasKey(e => e.CenterId).HasName("PK__ServiceC__398FC7D75D87FF4A");

            entity.Property(e => e.Capacity).HasDefaultValue(10);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.Manager).WithMany(p => p.ServiceCenters).HasConstraintName("FK__ServiceCe__Manag__2FCF1A8A");
        });

        modelBuilder.Entity<ServiceRating>(entity =>
        {
            entity.HasKey(e => e.RatingId).HasName("PK__ServiceR__FCCDF85CC2260A6C");

            entity.Property(e => e.IsVerified).HasDefaultValue(false);
            entity.Property(e => e.RatingDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Advisor).WithMany(p => p.ServiceRatingAdvisors).HasConstraintName("FK__ServiceRa__Advis__3DB3258D");

            entity.HasOne(d => d.Customer).WithMany(p => p.ServiceRatings)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ServiceRa__Custo__3BCADD1B");

            entity.HasOne(d => d.RespondedByNavigation).WithMany(p => p.ServiceRatingRespondedByNavigations).HasConstraintName("FK__ServiceRa__Respo__3EA749C6");

            entity.HasOne(d => d.Technician).WithMany(p => p.ServiceRatingTechnicians).HasConstraintName("FK__ServiceRa__Techn__3CBF0154");

            entity.HasOne(d => d.WorkOrder).WithMany(p => p.ServiceRatings)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ServiceRa__WorkO__3AD6B8E2");
        });

        modelBuilder.Entity<Shift>(entity =>
        {
            entity.HasKey(e => e.ShiftId).HasName("PK__Shifts__C0A838E1D7590458");

            entity.Property(e => e.BreakMinutes).HasComputedColumnSql("(case when [ActualBreakStart] IS NOT NULL AND [ActualBreakEnd] IS NOT NULL then datediff(minute,[ActualBreakStart],[ActualBreakEnd]) else (0) end)", true);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsEarlyLeave).HasDefaultValue(false);
            entity.Property(e => e.IsLate).HasDefaultValue(false);
            entity.Property(e => e.NetWorkingHours).HasComputedColumnSql("(case when [ActualStartTime] IS NOT NULL AND [ActualEndTime] IS NOT NULL then datediff(minute,[ActualStartTime],[ActualEndTime])/(60.0)-case when [ActualBreakStart] IS NOT NULL AND [ActualBreakEnd] IS NOT NULL then datediff(minute,[ActualBreakStart],[ActualBreakEnd])/(60.0) else (0) end  end)", true);
            entity.Property(e => e.ShiftType).HasDefaultValue("Regular");
            entity.Property(e => e.Status).HasDefaultValue("Scheduled");
            entity.Property(e => e.WorkedHours).HasComputedColumnSql("(case when [ActualStartTime] IS NOT NULL AND [ActualEndTime] IS NOT NULL then datediff(minute,[ActualStartTime],[ActualEndTime])/(60.0)  end)", true);

            entity.HasOne(d => d.ApprovedByNavigation).WithMany(p => p.ShiftApprovedByNavigations).HasConstraintName("FK__Shifts__Approved__269AB60B");

            entity.HasOne(d => d.Center).WithMany(p => p.Shifts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Shifts__CenterID__25A691D2");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.ShiftCreatedByNavigations).HasConstraintName("FK__Shifts__CreatedB__278EDA44");

            entity.HasOne(d => d.User).WithMany(p => p.ShiftUsers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Shifts__UserID__24B26D99");
        });

        modelBuilder.Entity<StockTransaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("PK__StockTra__55433A4B44E46CCE");

            entity.Property(e => e.TotalCost).HasComputedColumnSql("([Quantity]*[UnitCost])", true);
            entity.Property(e => e.TransactionDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Center).WithMany(p => p.StockTransactions).HasConstraintName("FK__StockTran__Cente__4589517F");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.StockTransactions).HasConstraintName("FK__StockTran__Creat__477199F1");

            entity.HasOne(d => d.Part).WithMany(p => p.StockTransactions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StockTran__PartI__44952D46");

            entity.HasOne(d => d.Supplier).WithMany(p => p.StockTransactions).HasConstraintName("FK__StockTran__Suppl__467D75B8");
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(e => e.SupplierId).HasName("PK__Supplier__4BE666946869E259");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.CreditLimit).HasDefaultValue(0m);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsPreferred).HasDefaultValue(false);
            entity.Property(e => e.Rating).HasDefaultValue(3);
        });

        modelBuilder.Entity<SystemConfiguration>(entity =>
        {
            entity.HasKey(e => e.ConfigId).HasName("PK__SystemCo__C3BC333CA8449B51");

            entity.Property(e => e.DataType).HasDefaultValue("String");
            entity.Property(e => e.IsEditable).HasDefaultValue(true);
            entity.Property(e => e.RequiresRestart).HasDefaultValue(false);
            entity.Property(e => e.UpdatedDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.SystemConfigurations).HasConstraintName("FK__SystemCon__Updat__5832119F");
        });

        modelBuilder.Entity<TechnicianSchedule>(entity =>
        {
            entity.HasKey(e => e.ScheduleId).HasName("PK__Technici__9C8A5B6984F36596");

            entity.Property(e => e.AvailableMinutes).HasComputedColumnSql("([MaxCapacityMinutes]-[BookedMinutes])", true);
            entity.Property(e => e.BookedMinutes).HasDefaultValue(0);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsAvailable).HasDefaultValue(true);
            entity.Property(e => e.MaxCapacityMinutes).HasDefaultValue(480);
            entity.Property(e => e.ShiftType).HasDefaultValue("Regular");

            entity.HasOne(d => d.Center).WithMany(p => p.TechnicianSchedules)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Technicia__Cente__3864608B");

            entity.HasOne(d => d.Technician).WithMany(p => p.TechnicianSchedules)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Technicia__Techn__37703C52");
        });

        modelBuilder.Entity<TimeSlot>(entity =>
        {
            entity.HasKey(e => e.SlotId).HasName("PK__TimeSlot__0A124A4F359B1C4C");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.CurrentBookings).HasDefaultValue(0);
            entity.Property(e => e.IsAvailable).HasDefaultValue(true);
            entity.Property(e => e.MaxBookings).HasDefaultValue(1);
            entity.Property(e => e.SlotType).HasDefaultValue("Regular");

            entity.HasOne(d => d.Center).WithMany(p => p.TimeSlots)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TimeSlots__Cente__40058253");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CCAC6BBF0671");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.FailedLoginAttempts).HasDefaultValue(0);
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.InverseCreatedByNavigation).HasConstraintName("FK__Users__CreatedBy__5441852A");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Users__RoleID__534D60F1");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__UserRole__8AFACE3A7DD86565");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<UserSession>(entity =>
        {
            entity.HasKey(e => e.SessionId).HasName("PK__UserSess__C9F49270CCD65102");

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LastActivityTime).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.LoginTime).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.User).WithMany(p => p.UserSessions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserSessi__UserI__5AEE82B9");
        });

        modelBuilder.Entity<VehicleCustomService>(entity =>
        {
            entity.HasKey(e => e.CustomServiceId).HasName("PK__VehicleC__D736BE74D6FB7216");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsRecurring).HasDefaultValue(false);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.VehicleCustomServices).HasConstraintName("FK__VehicleCu__Creat__5090EFD7");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.VehicleCustomServices)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__VehicleCu__Vehic__4F9CCB9E");
        });

        modelBuilder.Entity<VehicleHealthMetric>(entity =>
        {
            entity.HasKey(e => e.MetricId).HasName("PK__VehicleH__56105645DC9176E3");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.VehicleHealthMetrics).HasConstraintName("FK__VehicleHe__Creat__1C5231C2");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.VehicleHealthMetrics)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__VehicleHe__Vehic__1A69E950");

            entity.HasOne(d => d.WorkOrder).WithMany(p => p.VehicleHealthMetrics).HasConstraintName("FK__VehicleHe__WorkO__1B5E0D89");
        });

        modelBuilder.Entity<Warranty>(entity =>
        {
            entity.HasKey(e => e.WarrantyId).HasName("PK__Warranti__2ED318F39F393928");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Status).HasDefaultValue("Active");

            entity.HasOne(d => d.ClaimedWorkOrder).WithMany(p => p.WarrantyClaimedWorkOrders).HasConstraintName("FK__Warrantie__Claim__10E07F16");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Warranties).HasConstraintName("FK__Warrantie__Creat__11D4A34F");

            entity.HasOne(d => d.Part).WithMany(p => p.Warranties).HasConstraintName("FK__Warrantie__PartI__0EF836A4");

            entity.HasOne(d => d.Service).WithMany(p => p.Warranties).HasConstraintName("FK__Warrantie__Servi__0E04126B");

            entity.HasOne(d => d.WarrantyType).WithMany(p => p.Warranties)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Warrantie__Warra__0FEC5ADD");

            entity.HasOne(d => d.WorkOrder).WithMany(p => p.WarrantyWorkOrders)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Warrantie__WorkO__0D0FEE32");
        });

        modelBuilder.Entity<WarrantyType>(entity =>
        {
            entity.HasKey(e => e.WarrantyTypeId).HasName("PK__Warranty__EDA140F3DF34F290");

            entity.Property(e => e.DefaultPeriod).HasDefaultValue(12);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<WorkOrder>(entity =>
        {
            entity.HasKey(e => e.WorkOrderId).HasName("PK__WorkOrde__AE755175BEE4870A");

            entity.Property(e => e.ApprovalRequired).HasDefaultValue(false);
            entity.Property(e => e.ChecklistCompleted).HasDefaultValue(0);
            entity.Property(e => e.ChecklistTotal).HasDefaultValue(0);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.DiscountAmount).HasDefaultValue(0m);
            entity.Property(e => e.EstimatedAmount).HasDefaultValue(0m);
            entity.Property(e => e.FinalAmount).HasComputedColumnSql("(([TotalAmount]-[DiscountAmount])+[TaxAmount])", true);
            entity.Property(e => e.Priority).HasDefaultValue("Normal");
            entity.Property(e => e.ProgressPercentage).HasDefaultValue(0m);
            entity.Property(e => e.QualityCheckRequired).HasDefaultValue(true);
            entity.Property(e => e.RequiresApproval).HasDefaultValue(false);
            entity.Property(e => e.TaxAmount).HasDefaultValue(0m);
            entity.Property(e => e.TotalAmount).HasDefaultValue(0m);

            entity.HasOne(d => d.Advisor).WithMany(p => p.WorkOrderAdvisors).HasConstraintName("FK__WorkOrder__Advis__73852659");

            entity.HasOne(d => d.Appointment).WithMany(p => p.WorkOrders).HasConstraintName("FK__WorkOrder__Appoi__6DCC4D03");

            entity.HasOne(d => d.ApprovedByNavigation).WithMany(p => p.WorkOrderApprovedByNavigations).HasConstraintName("FK__WorkOrder__Appro__756D6ECB");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.WorkOrderCreatedByNavigations).HasConstraintName("FK__WorkOrder__Creat__7755B73D");

            entity.HasOne(d => d.Customer).WithMany(p => p.WorkOrders)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__WorkOrder__Custo__6EC0713C");

            entity.HasOne(d => d.QualityCheckedByNavigation).WithMany(p => p.WorkOrderQualityCheckedByNavigations).HasConstraintName("FK__WorkOrder__Quali__76619304");

            entity.HasOne(d => d.ServiceCenter).WithMany(p => p.WorkOrders)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__WorkOrder__Servi__70A8B9AE");

            entity.HasOne(d => d.Status).WithMany(p => p.WorkOrders)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__WorkOrder__Statu__719CDDE7");

            entity.HasOne(d => d.Supervisor).WithMany(p => p.WorkOrderSupervisors).HasConstraintName("FK__WorkOrder__Super__74794A92");

            entity.HasOne(d => d.Technician).WithMany(p => p.WorkOrderTechnicians).HasConstraintName("FK__WorkOrder__Techn__72910220");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.WorkOrderUpdatedByNavigations).HasConstraintName("FK__WorkOrder__Updat__7849DB76");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.WorkOrders)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__WorkOrder__Vehic__6FB49575");
        });

        modelBuilder.Entity<WorkOrderPart>(entity =>
        {
            entity.HasKey(e => e.WorkOrderPartId).HasName("PK__WorkOrde__53622A6F9D3258B6");

            entity.Property(e => e.DiscountAmount).HasDefaultValue(0m);
            entity.Property(e => e.RequestedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Status).HasDefaultValue("Ordered");
            entity.Property(e => e.TotalCost).HasComputedColumnSql("([Quantity]*[UnitCost])", true);
            entity.Property(e => e.TotalPrice).HasComputedColumnSql("([Quantity]*[UnitPrice]-[DiscountAmount])", true);

            entity.HasOne(d => d.InstalledByNavigation).WithMany(p => p.WorkOrderParts).HasConstraintName("FK__WorkOrder__Insta__40C49C62");

            entity.HasOne(d => d.Part).WithMany(p => p.WorkOrderParts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__WorkOrder__PartI__3FD07829");

            entity.HasOne(d => d.WorkOrder).WithMany(p => p.WorkOrderParts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__WorkOrder__WorkO__3EDC53F0");
        });

        modelBuilder.Entity<WorkOrderService>(entity =>
        {
            entity.HasKey(e => e.WorkOrderServiceId).HasName("PK__WorkOrde__EF7C24AA066AE7B2");

            entity.Property(e => e.DiscountAmount).HasDefaultValue(0m);
            entity.Property(e => e.ProgressPercentage).HasDefaultValue(0m);
            entity.Property(e => e.Quantity).HasDefaultValue(1);
            entity.Property(e => e.Status).HasDefaultValue("Pending");
            entity.Property(e => e.TotalPrice).HasComputedColumnSql("([Quantity]*[UnitPrice]-[DiscountAmount])", true);

            entity.HasOne(d => d.Service).WithMany(p => p.WorkOrderServices)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__WorkOrder__Servi__7FEAFD3E");

            entity.HasOne(d => d.Technician).WithMany(p => p.WorkOrderServices).HasConstraintName("FK__WorkOrder__Techn__00DF2177");

            entity.HasOne(d => d.WorkOrder).WithMany(p => p.WorkOrderServices)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__WorkOrder__WorkO__7EF6D905");
        });

        modelBuilder.Entity<WorkOrderStatus>(entity =>
        {
            entity.HasKey(e => e.StatusId).HasName("PK__WorkOrde__C8EE20434E20B889");

            entity.Property(e => e.AllowEdit).HasDefaultValue(true);
            entity.Property(e => e.DisplayOrder).HasDefaultValue(0);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.RequireApproval).HasDefaultValue(false);
        });

        modelBuilder.Entity<WorkOrderTimeline>(entity =>
        {
            entity.HasKey(e => e.TimelineId).HasName("PK__WorkOrde__1DE4F0E52CD568A0");

            entity.Property(e => e.EventDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsVisible).HasDefaultValue(true);

            entity.HasOne(d => d.PerformedByNavigation).WithMany(p => p.WorkOrderTimelines).HasConstraintName("FK__WorkOrder__Perfo__0697FACD");

            entity.HasOne(d => d.WorkOrder).WithMany(p => p.WorkOrderTimelines)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__WorkOrder__WorkO__05A3D694");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
