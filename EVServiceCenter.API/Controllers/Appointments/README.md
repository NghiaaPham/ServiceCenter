# 🎯 APPOINTMENT MODULE - HOÀN THÀNH

## 📂 Cấu trúc Files

```
EVServiceCenter.API/Controllers/Appointments/
├── AppointmentController.cs                    ✅ (Customer API)
├── AppointmentManagementController.cs          ✅ (Staff/Admin API)
├── APPOINTMENT_API_ENDPOINTS.md               ✅ (API Documentation)
└── README.md                                   ✅ (File này)
```

---

## ✅ ĐÃ HOÀN THÀNH

### 1. **AppointmentController.cs** (Customer)
**Route:** `/api/appointments`

**Endpoints:**
- ✅ `POST /api/appointments` - Đặt lịch hẹn mới
- ✅ `GET /api/appointments/{id}` - Xem chi tiết lịch hẹn
- ✅ `GET /api/appointments/my-appointments` - Xem lịch hẹn của tôi
- ✅ `GET /api/appointments/my-appointments/upcoming` - Lịch sắp tới
- ✅ `GET /api/appointments/by-code/{code}` - Tìm theo mã
- ✅ `PUT /api/appointments/{id}` - Cập nhật lịch hẹn
- ✅ `POST /api/appointments/{id}/reschedule` - Dời lịch
- ✅ `POST /api/appointments/{id}/cancel` - Hủy lịch
- ✅ `DELETE /api/appointments/{id}` - Xóa lịch (Pending only)

**Features:**
- ✅ Customer chỉ thao tác với lịch của mình
- ✅ Validation quyền truy cập (check CustomerId)
- ✅ Error handling đầy đủ
- ✅ Logging mọi hành động

---

### 2. **AppointmentManagementController.cs** (Staff/Admin)
**Route:** `/api/appointment-management`

**Endpoints:**
- ✅ `GET /api/appointment-management` - Xem tất cả (filter, sort, pagination)
- ✅ `GET /api/appointment-management/{id}` - Chi tiết
- ✅ `GET /api/appointment-management/by-service-center/{id}/date/{date}` - Theo trung tâm & ngày
- ✅ `GET /api/appointment-management/by-customer/{id}` - Theo khách hàng
- ✅ `POST /api/appointment-management/{id}/confirm` - Xác nhận lịch hẹn
- ✅ `POST /api/appointment-management/{id}/check-in` - Check-in (placeholder)
- ✅ `POST /api/appointment-management/{id}/mark-no-show` - Đánh dấu NoShow
- ✅ `POST /api/appointment-management/{id}/cancel` - Hủy lịch
- ✅ `PUT /api/appointment-management/{id}` - Cập nhật
- ✅ `DELETE /api/appointment-management/{id}` - Xóa (Admin only)
- ✅ `POST /api/appointment-management` - Tạo lịch cho khách (Walk-in/Phone)
- ✅ `GET /api/appointment-management/statistics/by-status` - Thống kê

**Features:**
- ✅ Full CRUD operations
- ✅ Advanced filtering & pagination
- ✅ Statistics & reporting
- ✅ Role-based authorization (Admin/Staff/Technician)

---

## 🔧 DEPENDENCIES

### Services Used:
```csharp
IAppointmentCommandService   // Đã có ✅
IAppointmentQueryService      // Đã có ✅
```

### DTOs Used:
```csharp
// Request DTOs
CreateAppointmentRequestDto    ✅
UpdateAppointmentRequestDto    ✅
RescheduleAppointmentRequestDto ✅
CancelAppointmentRequestDto    ✅
ConfirmAppointmentRequestDto   ✅

// Response DTOs
AppointmentResponseDto         ✅
AppointmentDetailResponseDto   ✅
PagedResult<T>                 ✅

// Query DTOs
AppointmentQueryDto            ✅
```

---

## 🔐 AUTHORIZATION POLICIES

Cần đảm bảo các policies này được config trong `Program.cs`:

```csharp
// EVServiceCenter.API/Program.cs hoặc Startup.cs

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CustomerOnly", policy =>
        policy.RequireRole("Customer"));

    options.AddPolicy("AllInternal", policy =>
        policy.RequireRole("Admin", "Staff", "Technician"));

    options.AddPolicy("AdminOrStaff", policy =>
        policy.RequireRole("Admin", "Staff"));

    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));
});
```

---

## 🚀 CÁCH SỬ DỤNG

### 1. **Đảm bảo Services đã được register:**

Kiểm tra file `Program.cs` hoặc `DependencyInjection.cs`:

```csharp
// Services
services.AddScoped<IAppointmentCommandService, AppointmentCommandService>();
services.AddScoped<IAppointmentQueryService, AppointmentQueryService>();

// Repositories
services.AddScoped<IAppointmentRepository, AppointmentRepository>();
services.AddScoped<IAppointmentCommandRepository, AppointmentCommandRepository>();
services.AddScoped<IAppointmentQueryRepository, AppointmentQueryRepository>();
```

### 2. **Test với Swagger:**

1. Chạy API: `dotnet run` hoặc F5 trong Visual Studio
2. Mở browser: `https://localhost:5001/swagger`
3. Login để lấy token:
   ```
   POST /api/auth/login
   {
     "email": "customer@example.com",
     "password": "password"
   }
   ```
4. Click "Authorize" trong Swagger
5. Nhập: `Bearer {your_token}`
6. Test các endpoints

### 3. **Test với Postman:**

Import collection từ file `APPOINTMENT_API_ENDPOINTS.md`

---

## 📊 LUỒNG HOẠT ĐỘNG

### Customer Flow:
```
1. Customer login → lấy token
2. POST /api/appointments → Tạo lịch (Status: Pending)
3. GET /api/appointments/my-appointments → Xem lịch của mình
4. PUT /api/appointments/{id} → Sửa lịch (nếu cần)
5. POST /api/appointments/{id}/reschedule → Dời lịch (nếu cần)
```

### Staff Flow:
```
1. Staff login → lấy token
2. GET /api/appointment-management?statusId=1 → Xem lịch Pending
3. POST /api/appointment-management/{id}/confirm → Xác nhận (Status: Confirmed)
4. Khách đến → POST /api/appointment-management/{id}/check-in (Status: CheckedIn)
5. Tạo WorkOrder từ CheckedIn appointment
6. WorkOrder InProgress → Completed
7. Tạo Invoice
```

---

## ⚠️ KNOWN ISSUES & TODO

### 1. **Check-in Feature** (Line 194 - AppointmentManagementController.cs)
```csharp
// TODO: Cần implement CheckInAsync() trong IAppointmentCommandService
// Hiện tại chỉ có placeholder
```

**Cần làm:**
```csharp
// EVServiceCenter.Core/.../IAppointmentCommandService.cs
Task<bool> CheckInAsync(
    int appointmentId,
    int currentUserId,
    CancellationToken cancellationToken = default);

// EVServiceCenter.Infrastructure/.../AppointmentCommandService.cs
public async Task<bool> CheckInAsync(int appointmentId, int currentUserId, ...)
{
    // Kiểm tra appointment ở trạng thái Confirmed (2)
    // Update StatusId = CheckedIn (3)
    // Update CheckedInDate = DateTime.UtcNow
    // Log activity
    // Gửi notification (optional)
    // Return true
}
```

### 2. **Notification Integration**
Khi appointment status thay đổi, cần gửi notification:
- Tạo mới (Pending) → Email xác nhận
- Xác nhận (Confirmed) → SMS nhắc lịch
- Check-in (CheckedIn) → Notification "đang chờ"
- Hoàn thành (Completed) → Email hóa đơn

**Suggestion:**
```csharp
// Thêm vào AppointmentCommandService
private readonly INotificationService _notificationService;

// Trong CreateAsync(), sau khi save:
await _notificationService.SendAppointmentCreatedAsync(appointment);
```

### 3. **WorkOrder Integration**
Sau khi Check-in, tự động tạo WorkOrder:

```csharp
// POST /api/work-orders/from-appointment/{appointmentId}
// Tạo WorkOrder từ CheckedIn appointment
// Copy services, customer, vehicle info
// StatusId = 1 (Chờ)
```

---

## 📈 METRICS & MONITORING

### Các log events được ghi:
- ✅ Customer created appointment
- ✅ Customer updated/cancelled/rescheduled appointment
- ✅ Staff confirmed appointment
- ✅ Staff marked NoShow
- ✅ Staff cancelled appointment (với reason)
- ✅ Admin deleted appointment

### Suggested metrics để track:
- Số appointment created per day
- Conversion rate: Pending → Confirmed → Completed
- No-show rate
- Cancellation rate
- Average booking lead time
- Most booked services/time slots

---

## 🧪 UNIT TEST CHECKLIST

### AppointmentController:
- [ ] CreateAppointment_Success
- [ ] CreateAppointment_InvalidCustomerId_ReturnsForbidden
- [ ] GetMyAppointments_ReturnsOnlyMyAppointments
- [ ] UpdateAppointment_NotOwner_ReturnsForbidden
- [ ] CancelAppointment_AlreadyCancelled_ReturnsError
- [ ] RescheduleAppointment_CreatesNewAppointment

### AppointmentManagementController:
- [ ] GetAllAppointments_WithFilters_ReturnsFiltered
- [ ] ConfirmAppointment_PendingStatus_Success
- [ ] ConfirmAppointment_AlreadyConfirmed_ReturnsError
- [ ] MarkAsNoShow_ConfirmedStatus_Success
- [ ] GetStatistics_ReturnsCorrectCounts

---

## 📝 CHANGELOG

**v1.0.0** (2025-10-03)
- ✅ Initial release
- ✅ AppointmentController với 9 endpoints
- ✅ AppointmentManagementController với 12 endpoints
- ✅ Full authorization & validation
- ✅ Error handling & logging
- ✅ API documentation

---

## 🆘 SUPPORT

Nếu có lỗi, check:

1. **Services đã register chưa?** → Check `Program.cs`
2. **Token có hợp lệ không?** → Check Authorization header
3. **Database có data test chưa?** → Check seeder
4. **Policies đã config chưa?** → Check authorization setup

**Logs location:**
- Console output
- File: `logs/appointment-{date}.log` (nếu có Serilog)

---

**🎉 Chúc bạn test thành công!**
