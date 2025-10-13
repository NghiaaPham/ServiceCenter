# 📘 IMPLEMENTATION GUIDE - PART 4: DTOs, CONTROLLERS & DI

> **Phần cuối: DTOs, Controller endpoints, và Dependency Injection setup**

---

## 5️⃣ DTOs - REQUEST & RESPONSE

### 📁 Tạo file: `AdjustServiceSourceRequestDto.cs`

**Đường dẫn:** `EVServiceCenter.Core/Domains/AppointmentManagement/DTOs/Request/AdjustServiceSourceRequestDto.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Request
{
    /// <summary>
    /// DTO để admin điều chỉnh ServiceSource của AppointmentService
    /// </summary>
    public class AdjustServiceSourceRequestDto
    {
        /// <summary>
        /// ServiceSource mới
        /// Values: Subscription, Extra, Regular
        /// </summary>
        [Required(ErrorMessage = "NewServiceSource là bắt buộc")]
        [StringLength(20)]
        public string NewServiceSource { get; set; } = null!;

        /// <summary>
        /// Giá mới
        /// </summary>
        [Required(ErrorMessage = "NewPrice là bắt buộc")]
        [Range(0, double.MaxValue, ErrorMessage = "NewPrice phải >= 0")]
        public decimal NewPrice { get; set; }

        /// <summary>
        /// Lý do điều chỉnh (bắt buộc)
        /// </summary>
        [Required(ErrorMessage = "Reason là bắt buộc")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Reason phải từ 10-500 ký tự")]
        public string Reason { get; set; } = null!;

        /// <summary>
        /// Có hoàn tiền không?
        /// </summary>
        public bool IssueRefund { get; set; } = false;
    }
}
```

### 📁 Tạo file: `AdjustServiceSourceResponseDto.cs`

**Đường dẫn:** `EVServiceCenter.Core/Domains/AppointmentManagement/DTOs/Response/AdjustServiceSourceResponseDto.cs`

```csharp
namespace EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Response
{
    /// <summary>
    /// Response sau khi adjust service source
    /// </summary>
    public class AdjustServiceSourceResponseDto
    {
        public int AppointmentServiceId { get; set; }
        public string OldServiceSource { get; set; } = null!;
        public string NewServiceSource { get; set; } = null!;
        public decimal OldPrice { get; set; }
        public decimal NewPrice { get; set; }
        public decimal PriceDifference { get; set; }
        public bool RefundIssued { get; set; }
        public bool UsageDeducted { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
```

### 📁 Tạo validator: `AdjustServiceSourceValidator.cs`

**Đường dẫn:** `EVServiceCenter.Core/Domains/AppointmentManagement/Validators/AdjustServiceSourceValidator.cs`

```csharp
using EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Request;
using FluentValidation;

namespace EVServiceCenter.Core.Domains.AppointmentManagement.Validators
{
    public class AdjustServiceSourceValidator : AbstractValidator<AdjustServiceSourceRequestDto>
    {
        public AdjustServiceSourceValidator()
        {
            RuleFor(x => x.NewServiceSource)
                .NotEmpty().WithMessage("NewServiceSource không được để trống")
                .Must(s => new[] { "Subscription", "Extra", "Regular" }.Contains(s))
                .WithMessage("NewServiceSource phải là: Subscription, Extra, hoặc Regular");

            RuleFor(x => x.NewPrice)
                .GreaterThanOrEqualTo(0).WithMessage("NewPrice phải >= 0");

            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Reason không được để trống")
                .MinimumLength(10).WithMessage("Reason phải ít nhất 10 ký tự")
                .MaximumLength(500).WithMessage("Reason không được vượt quá 500 ký tự");
        }
    }
}
```

---

## 6️⃣ CONTROLLER ENDPOINTS

### File: `AppointmentManagementController.cs`

**Đường dẫn:** `EVServiceCenter.API/Controllers/Appointments/AppointmentManagementController.cs`

**Thêm vào cuối class (trước closing brace):**

```csharp
/// <summary>
/// [ADMIN] Điều chỉnh ServiceSource và giá của một AppointmentService
/// Dùng để sửa lỗi hoặc hoàn tiền cho khách hàng
/// </summary>
/// <param name="appointmentId">Appointment ID</param>
/// <param name="appointmentServiceId">AppointmentService ID</param>
/// <param name="request">Thông tin điều chỉnh</param>
[HttpPost("appointments/{appointmentId}/services/{appointmentServiceId}/adjust")]
[Authorize(Policy = "AdminOnly")]
[ProducesResponseType(typeof(ApiResponse<AdjustServiceSourceResponseDto>), 200)]
[ProducesResponseType(typeof(ApiResponse<object>), 400)]
[ProducesResponseType(typeof(ApiResponse<object>), 404)]
public async Task<IActionResult> AdjustServiceSource(
    int appointmentId,
    int appointmentServiceId,
    [FromBody] AdjustServiceSourceRequestDto request)
{
    try
    {
        var currentUserId = GetCurrentUserId();

        var result = await _commandService.AdjustServiceSourceAsync(
            appointmentId,
            appointmentServiceId,
            request,
            currentUserId);

        _logger.LogInformation(
            "Admin {UserId} adjusted AppointmentService {AppointmentServiceId}: " +
            "{OldSource} → {NewSource}, Price: {OldPrice} → {NewPrice}",
            currentUserId, appointmentServiceId,
            result.OldServiceSource, result.NewServiceSource,
            result.OldPrice, result.NewPrice);

        return Ok(ApiResponse<AdjustServiceSourceResponseDto>.WithSuccess(
            result,
            "Điều chỉnh service source thành công"));
    }
    catch (InvalidOperationException ex)
    {
        _logger.LogWarning(ex, "Validation error adjusting service source");
        return BadRequest(ApiResponse<object>.WithError(ex.Message));
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error adjusting service source");
        return StatusCode(500, ApiResponse<object>.WithError(
            "Đã xảy ra lỗi khi điều chỉnh service source"));
    }
}

/// <summary>
/// [ADMIN] Lấy audit log của một appointment
/// Xem lịch sử thay đổi ServiceSource
/// </summary>
/// <param name="appointmentId">Appointment ID</param>
[HttpGet("appointments/{appointmentId}/audit-log")]
[Authorize(Policy = "AdminOnly")]
[ProducesResponseType(typeof(ApiResponse<List<object>>), 200)]
[ProducesResponseType(typeof(ApiResponse<object>), 404)]
public async Task<IActionResult> GetAuditLog(int appointmentId)
{
    try
    {
        var logs = await _auditService.GetAuditLogsForAppointmentAsync(appointmentId);

        return Ok(ApiResponse<List<object>>.WithSuccess(
            logs,
            $"Lấy audit log thành công ({logs.Count} records)"));
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error getting audit log for appointment {AppointmentId}", appointmentId);
        return StatusCode(500, ApiResponse<object>.WithError(
            "Đã xảy ra lỗi khi lấy audit log"));
    }
}
```

**Thêm dependency vào constructor:**

```csharp
private readonly IServiceSourceAuditService _auditService;

public AppointmentManagementController(
    // ... existing dependencies ...
    IServiceSourceAuditService auditService  // ← ADD THIS
)
{
    // ... existing assignments ...
    _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
}
```

---

## 7️⃣ DEPENDENCY INJECTION SETUP

### File: `AppointmentDependencyInjection.cs`

**Đường dẫn:** `EVServiceCenter.API/Extensions/AppointmentDependencyInjection.cs`

**Tìm method `AddAppointmentModule` và thêm:**

```csharp
public static IServiceCollection AddAppointmentModule(this IServiceCollection services)
{
    // ... existing registrations ...

    // ✅ ADD: Audit Service
    services.AddScoped<IServiceSourceAuditService, ServiceSourceAuditService>();

    // ✅ ADD: Validators
    services.AddScoped<IValidator<AdjustServiceSourceRequestDto>, AdjustServiceSourceValidator>();

    return services;
}
```

### File: `Program.cs`

**Đảm bảo đã add validator vào DI:**

```csharp
// Trong section FluentValidation (khoảng line 26-32)
builder.Services.AddValidatorsFromAssemblyContaining<CreateAppointmentValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<AdjustServiceSourceValidator>();  // ← ADD
```

---

## 8️⃣ APPLY MIGRATION

Sau khi hoàn thành tất cả code, chạy migration:

```bash
cd EVServiceCenter.Infrastructure
dotnet ef database update
```

**Kiểm tra database:**
- Bảng `Appointments` có thêm: `RowVersion`, `CompletedDate`, `CompletedBy`
- Bảng `ServiceSourceAuditLog` đã tạo
- Bảng `PaymentTransactions` đã tạo
- Indexes đã tạo

---

## 9️⃣ TESTING CHECKLIST

### Test 1: Create Appointment với Subscription
```json
POST /api/appointments
{
  "customerId": 1,
  "vehicleId": 2,
  "serviceCenterId": 1,
  "slotId": 10,
  "subscriptionId": 5,
  "serviceIds": [],  // Empty - dùng tất cả services từ gói
  "priority": "Normal",
  "source": "Online"
}
```

**Expected:**
- AppointmentServices có `ServiceSource = "Subscription"`
- `Price = 0`
- `EstimatedCost = 0` (không tính tiền)

### Test 2: Create Appointment - Gói + Extra
```json
{
  "subscriptionId": 5,
  "serviceIds": [1, 2, 99],  // 1,2 có trong gói, 99 không có
  // ...
}
```

**Expected:**
- Service 1, 2: `ServiceSource = "Subscription"`, `Price = 0`
- Service 99: `ServiceSource = "Extra"`, `Price = giá thực tế`

### Test 3: Complete Appointment - Race Condition
1. Customer A và B cùng có subscription với "Thay dầu" còn 1 lượt
2. Cả hai đặt lịch
3. A complete trước → OK
4. B complete sau → Service bị degrade to "Extra"

**Expected:**
- B's appointment: Service "Thay dầu" có `ServiceSource = "Extra"`
- ServiceSourceAuditLog có record với `ChangeType = "AUTO_DEGRADE"`
- Notification được gửi cho customer B

### Test 4: Admin Adjust
```json
POST /appointments/123/services/456/adjust
{
  "newServiceSource": "Subscription",
  "newPrice": 0,
  "reason": "Lỗi hệ thống, customer đã có gói",
  "issueRefund": true
}
```

**Expected:**
- AppointmentService updated
- Audit log có record `ChangeType = "REFUND"`
- EstimatedCost được điều chỉnh

### Test 5: Idempotency
1. Complete appointment
2. Call complete lại (retry hoặc double-click)

**Expected:**
- Lần 2 return success ngay (không throw error)
- Không trừ lượt 2 lần

---

## 🎯 FINAL CHECKLIST

### Phase 1: Database ✅
- [x] Migration created
- [x] Entities updated
- [ ] Migration applied (`dotnet ef database update`)

### Phase 2: Services ✅
- [x] ServiceSourceAuditService
- [x] CreateAppointmentValidator updated
- [x] BuildAppointmentServicesAsync
- [x] CompleteAppointmentAsync with race handling
- [x] UpdateServiceUsageAsync with pessimistic lock
- [x] AdjustServiceSourceAsync

### Phase 3: DTOs & Validators ✅
- [x] AdjustServiceSourceRequestDto
- [x] AdjustServiceSourceResponseDto
- [x] AdjustServiceSourceValidator

### Phase 4: Controllers ✅
- [x] Adjust API endpoint
- [x] GetAuditLog endpoint

### Phase 5: DI ✅
- [x] ServiceSourceAuditService registered
- [x] Validators registered
- [x] HttpContextAccessor registered (should already exist)

### Phase 6: Testing ⏳
- [ ] Unit tests
- [ ] Integration tests
- [ ] Manual testing with Swagger

---

## 📝 NOTES

### Phần chưa implement (TODO sau):
1. **Payment Service integration** - Hiện tại chỉ log, chưa thực sự charge/refund
2. **Notification Service** - Email/SMS cho customer khi degrade
3. **Retry Payment API** - Cho customer thanh toán lại nếu CompletedWithUnpaid
4. **Dashboard/Reports** - Thống kê audit logs, degraded services

### Performance Considerations:
- Pessimistic locks có thể ảnh hưởng performance khi traffic cao
- Nên có monitoring cho lock contention
- Xem xét add cache cho subscription lookup nếu cần

### Security:
- Audit log có IP address và User Agent để security tracking
- Admin-only endpoints cần thêm rate limiting
- Xem xét thêm 2FA cho sensitive operations

---

**🎉 HOÀN TẤT! Hệ thống đã sẵn sàng triển khai.**

