# 📘 SMART SUBSCRIPTION - IMPLEMENTATION GUIDE

> **Hướng dẫn chi tiết triển khai từng file**
>
> Tài liệu này chứa toàn bộ code cần thêm/sửa để hoàn thành Smart Subscription logic.

---

## 🎯 CÁC FILE ĐÃ HOÀN THÀNH

✅ **Migration:** `20251009064115_AddRowVersionAndAuditTables.cs`
✅ **Appointment Entity:** `Appointment.cs` - Đã thêm RowVersion, CompletedDate, CompletedBy
✅ **ServiceSourceAuditLog Entity:** `ServiceSourceAuditLog.cs`
✅ **PaymentTransaction Entity:** `PaymentTransaction.cs`

---

## 📝 CÁC FILE CẦN TRIỂN KHAI

### 1️⃣ **AppointmentStatusEnum.cs**

**Đường dẫn:** `EVServiceCenter.Core/Enums/AppointmentStatusEnum.cs`

**Action:** Thêm status mới `CompletedWithUnpaid = 5`

```csharp
namespace EVServiceCenter.Core.Enums
{
    public enum AppointmentStatusEnum
    {
        Pending = 1,
        Confirmed = 2,
        InProgress = 3,
        Completed = 4,
        CompletedWithUnpaid = 5,  // ← ADD THIS
        Cancelled = 6,
        NoShow = 7,
        Rescheduled = 8
    }
}
```

---

### 2️⃣ **EVDbContext.cs**

**Đường dẫn:** `EVServiceCenter.Infrastructure/Data/EVDbContext.cs`

**Action:** Thêm DbSets cho entities mới

```csharp
// Tìm vị trí khai báo DbSets và thêm:

public virtual DbSet<ServiceSourceAuditLog> ServiceSourceAuditLogs { get; set; }
public virtual DbSet<PaymentTransaction> PaymentTransactions { get; set; }
```

---

### 3️⃣ **AppointmentService.cs** (Entity)

**Đường dẫn:** `EVServiceCenter.Core/Domains/AppointmentManagement/Entities/AppointmentService.cs`

**Action:** Thêm navigation property cho ServiceSourceAuditLogs

```csharp
// Thêm vào cuối class, trước closing brace:

/// <summary>
/// Navigation: Audit logs for this service
/// </summary>
[InverseProperty("AppointmentService")]
public virtual ICollection<ServiceSourceAuditLog> ServiceSourceAuditLogs { get; set; }
    = new List<ServiceSourceAuditLog>();
```

---

### 4️⃣ **Customer.cs** (Entity)

**Đường dẫn:** `EVServiceCenter.Core/Domains/Customers/Entities/Customer.cs`

**Action:** Thêm navigation properties

```csharp
// Thêm vào cuối class:

[InverseProperty("Customer")]
public virtual ICollection<ServiceSourceAuditLog> ServiceSourceAuditLogs { get; set; }
    = new List<ServiceSourceAuditLog>();

[InverseProperty("Customer")]
public virtual ICollection<PaymentTransaction> PaymentTransactions { get; set; }
    = new List<PaymentTransaction>();
```

---

### 5️⃣ **Appointment.cs** (Entity) - Thêm navigation

**Action:** Thêm navigation properties cho audit logs và payment transactions

```csharp
// Thêm vào cuối class, sau WorkOrders:

[InverseProperty("Appointment")]
public virtual ICollection<ServiceSourceAuditLog> ServiceSourceAuditLogs { get; set; }
    = new List<ServiceSourceAuditLog>();

[InverseProperty("Appointment")]
public virtual ICollection<PaymentTransaction> PaymentTransactions { get; set; }
    = new List<PaymentTransaction>();
```

---

### 6️⃣ **User.cs** (Entity)

**Đường dẫn:** `EVServiceCenter.Core/Domains/Identity/Entities/User.cs`

**Action:** Thêm navigation properties

```csharp
// Thêm vào cuối class:

[InverseProperty("CompletedByNavigation")]
public virtual ICollection<Appointment> AppointmentCompletedByNavigations { get; set; }
    = new List<Appointment>();

[InverseProperty("ChangedByUser")]
public virtual ICollection<ServiceSourceAuditLog> ServiceSourceAuditLogs { get; set; }
    = new List<ServiceSourceAuditLog>();
```

---

### 7️⃣ **MaintenanceService.cs** (Entity)

**Đường dẫn:** `EVServiceCenter.Core/Domains/MaintenanceServices/Entities/MaintenanceService.cs`

**Action:** Thêm navigation

```csharp
// Thêm:

[InverseProperty("Service")]
public virtual ICollection<ServiceSourceAuditLog> ServiceSourceAuditLogs { get; set; }
    = new List<ServiceSourceAuditLog>();
```

---

### 8️⃣ **CreateAppointmentValidator.cs**

**Đường dẫn:** `EVServiceCenter.Core/Domains/AppointmentManagement/Validators/CreateAppointmentValidator.cs`

**Action:** Sửa validation logic để cho phép ServiceIds empty nếu có SubscriptionId

```csharp
public class CreateAppointmentValidator : AbstractValidator<CreateAppointmentRequestDto>
{
    public CreateAppointmentValidator()
    {
        RuleFor(x => x.CustomerId)
            .GreaterThan(0).WithMessage("Khách hàng không hợp lệ");

        RuleFor(x => x.VehicleId)
            .GreaterThan(0).WithMessage("Xe không hợp lệ");

        RuleFor(x => x.ServiceCenterId)
            .GreaterThan(0).WithMessage("Trung tâm dịch vụ không hợp lệ");

        RuleFor(x => x.SlotId)
            .GreaterThan(0).WithMessage("Slot thời gian không hợp lệ");

        // ✅ SỬA: Cho phép ServiceIds empty nếu có SubscriptionId
        RuleFor(x => x)
            .Must(x => x.SubscriptionId.HasValue ||
                      (x.ServiceIds != null && x.ServiceIds.Any()))
            .WithMessage("Phải chọn ít nhất một gói dịch vụ hoặc dịch vụ đơn lẻ")
            .Must(x => x.ServiceIds == null || x.ServiceIds.All(id => id > 0))
            .WithMessage("ID dịch vụ không hợp lệ");

        When(x => x.PackageId.HasValue, () =>
        {
            RuleFor(x => x.PackageId)
                .GreaterThan(0).WithMessage("Gói dịch vụ không hợp lệ");
        });

        When(x => !string.IsNullOrEmpty(x.CustomerNotes), () =>
        {
            RuleFor(x => x.CustomerNotes)
                .MaximumLength(1000).WithMessage("Ghi chú không được vượt quá 1000 ký tự");
        });

        When(x => x.PreferredTechnicianId.HasValue, () =>
        {
            RuleFor(x => x.PreferredTechnicianId)
                .GreaterThan(0).WithMessage("Kỹ thuật viên không hợp lệ");
        });

        RuleFor(x => x.Priority)
            .NotEmpty().WithMessage("Độ ưu tiên không được để trống")
            .Must(p => new[] { "Normal", "High", "Urgent" }.Contains(p))
            .WithMessage("Độ ưu tiên phải là Normal, High hoặc Urgent");

        RuleFor(x => x.Source)
            .NotEmpty().WithMessage("Nguồn đặt lịch không được để trống")
            .Must(s => new[] { "Online", "Walk-in", "Phone" }.Contains(s))
            .WithMessage("Nguồn phải là Online, Walk-in hoặc Phone");
    }
}
```

---

### 9️⃣ **IServiceSourceAuditService.cs** (Interface)

**Tạo file mới:** `EVServiceCenter.Core/Interfaces/Services/IServiceSourceAuditService.cs`

```csharp
using EVServiceCenter.Core.Domains.AppointmentManagement.Entities;

namespace EVServiceCenter.Core.Interfaces.Services
{
    public interface IServiceSourceAuditService
    {
        Task LogServiceSourceChangeAsync(
            AppointmentService appointmentService,
            string oldSource,
            string newSource,
            decimal oldPrice,
            decimal newPrice,
            string reason,
            string changeType,
            int? changedBy = null,
            string? ipAddress = null,
            string? userAgent = null,
            CancellationToken cancellationToken = default);

        Task<List<object>> GetAuditLogsForAppointmentAsync(
            int appointmentId,
            CancellationToken cancellationToken = default);
    }
}
```

---

### 🔟 **ServiceSourceAuditService.cs** (Implementation)

**Tạo file mới:** `EVServiceCenter.Infrastructure/Services/ServiceSourceAuditService.cs`

```csharp
using EVServiceCenter.Core.Domains.AppointmentManagement.Entities;
using EVServiceCenter.Core.Entities;
using EVServiceCenter.Core.Interfaces.Services;
using EVServiceCenter.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EVServiceCenter.Infrastructure.Services
{
    public class ServiceSourceAuditService : IServiceSourceAuditService
    {
        private readonly EVDbContext _context;
        private readonly ILogger<ServiceSourceAuditService> _logger;

        public ServiceSourceAuditService(
            EVDbContext context,
            ILogger<ServiceSourceAuditService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task LogServiceSourceChangeAsync(
            AppointmentService appointmentService,
            string oldSource,
            string newSource,
            decimal oldPrice,
            decimal newPrice,
            string reason,
            string changeType,
            int? changedBy = null,
            string? ipAddress = null,
            string? userAgent = null,
            CancellationToken cancellationToken = default)
        {
            var auditLog = new ServiceSourceAuditLog
            {
                AppointmentServiceId = appointmentService.AppointmentServiceId,
                AppointmentId = appointmentService.AppointmentId,
                ServiceId = appointmentService.ServiceId,
                CustomerID = appointmentService.Appointment?.CustomerId ?? 0,
                OldServiceSource = oldSource,
                NewServiceSource = newSource,
                OldPrice = oldPrice,
                NewPrice = newPrice,
                ChangeReason = reason,
                ChangeType = changeType,
                ChangedBy = changedBy,
                ChangedDate = DateTime.UtcNow,
                IpAddress = ipAddress,
                UserAgent = userAgent
            };

            _context.ServiceSourceAuditLogs.Add(auditLog);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Audit logged: AppointmentService {AppointmentServiceId}, " +
                "{OldSource}({OldPrice}đ) → {NewSource}({NewPrice}đ), " +
                "Reason: {Reason}, Type: {Type}",
                appointmentService.AppointmentServiceId,
                oldSource, oldPrice, newSource, newPrice,
                reason, changeType);
        }

        public async Task<List<object>> GetAuditLogsForAppointmentAsync(
            int appointmentId,
            CancellationToken cancellationToken = default)
        {
            return await _context.ServiceSourceAuditLogs
                .Where(a => a.AppointmentId == appointmentId)
                .OrderByDescending(a => a.ChangedDate)
                .Select(a => new
                {
                    a.AuditId,
                    a.ServiceId,
                    ServiceName = a.Service.ServiceName,
                    a.OldServiceSource,
                    a.NewServiceSource,
                    a.OldPrice,
                    a.NewPrice,
                    a.PriceDifference,
                    a.ChangeReason,
                    a.ChangeType,
                    ChangedBy = a.ChangedByUser != null ? a.ChangedByUser.FullName : "System",
                    a.ChangedDate,
                    a.IpAddress
                })
                .Cast<object>()
                .ToListAsync(cancellationToken);
        }
    }
}
```

---

Do độ dài quá lớn, tôi sẽ tạo thêm file chứa phần còn lại (AppointmentCommandService logic). Bạn muốn tôi tiếp tục không?

**Đã hoàn thành:**
- ✅ Migration
- ✅ 3 Entities mới (Appointment updated, ServiceSourceAuditLog, PaymentTransaction)
- ✅ File tracking progress
- ✅ Validator
- ✅ Audit Service

**Còn lại:**
- AppointmentCommandService (logic phức tạp nhất)
- Repository updates
- DTOs
- Controller APIs

Bạn muốn tôi:
1. **Tiếp tục tạo file IMPLEMENTATION_GUIDE_PART2.md** với phần còn lại?
2. Hay **trực tiếp sửa code** trong AppointmentCommandService?