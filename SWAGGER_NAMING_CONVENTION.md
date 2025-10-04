# 📚 QUY TẮC ĐẶT TÊN SWAGGER GROUPS

## 🎯 FORMAT: `{Icon} {Chức năng} ({Role})`

### **Ví dụ:**
```
📅 Quản lý lịch hẹn (Customer)
📅 Quản lý lịch hẹn (Staff/Admin)
👤 Quản lý khách hàng (Customer)
👤 Quản lý khách hàng (Staff/Admin)
```

---

## 📋 DANH SÁCH GROUPS CẦN ÁP DỤNG

### **📅 Quản lý lịch hẹn**
- ✅ `AppointmentController` → `📅 Quản lý lịch hẹn (Customer)`
- ✅ `AppointmentManagementController` → `📅 Quản lý lịch hẹn (Staff/Admin)`
- [ ] `TimeSlotCommandController` → `📅 Quản lý lịch hẹn (Staff/Admin)`
- [ ] `TimeSlotQueryController` → `📅 Quản lý lịch hẹn (Public)`

### **👤 Quản lý khách hàng**
- ✅ `CustomersController` → `👤 Quản lý khách hàng (Staff/Admin)`
- ✅ `CustomerProfileController` → `👤 Quản lý khách hàng (Customer)`
- [ ] `CustomerRegistrationController` → `👤 Quản lý khách hàng (Public)`
- [ ] `CustomerTypesController` → `👤 Quản lý khách hàng (Staff/Admin)`

### **🚗 Quản lý xe**
- [ ] `CustomerVehicleController` → `🚗 Quản lý xe (Customer)`
- [ ] `CustomerVehicleQueryController` → `🚗 Quản lý xe (Staff/Admin)`
- [ ] `CustomerVehicleStatisticsController` → `🚗 Quản lý xe (Staff/Admin)`
- [ ] `CarBrandController` → `🚗 Quản lý xe - Hãng xe (Staff/Admin)`
- [ ] `CarBrandQueryController` → `🚗 Quản lý xe - Hãng xe (Public)`
- [ ] `CarBrandStatisticsController` → `🚗 Quản lý xe - Hãng xe (Staff/Admin)`
- [ ] `CarModelController` → `🚗 Quản lý xe - Dòng xe (Staff/Admin)`
- [ ] `CarModelQueryController` → `🚗 Quản lý xe - Dòng xe (Public)`
- [ ] `CarModelStatisticsController` → `🚗 Quản lý xe - Dòng xe (Staff/Admin)`

### **🔧 Quản lý dịch vụ**
- [ ] `MaintenanceServiceController` → `🔧 Quản lý dịch vụ (Staff/Admin)`
- [ ] `ServiceCategoryController` → `🔧 Quản lý dịch vụ - Danh mục (Staff/Admin)`
- [ ] `ModelServicePricingController` → `🔧 Quản lý dịch vụ - Bảng giá (Staff/Admin)`

### **🏢 Quản lý trung tâm dịch vụ**
- [ ] `ServiceCenterController` → `🏢 Quản lý trung tâm dịch vụ (Staff/Admin)`
- [ ] `ServiceCenterQueryController` → `🏢 Quản lý trung tâm dịch vụ (Public)`
- [ ] `ServiceCenterStatisticsController` → `🏢 Quản lý trung tâm dịch vụ (Staff/Admin)`
- [ ] `ServiceCenterAvailabilityController` → `🏢 Quản lý trung tâm dịch vụ (Public)`

### **🔐 Xác thực & Tài khoản**
- [ ] `AuthController` → `🔐 Xác thực & Tài khoản (Public)`
- [ ] `ExternalAuthController` → `🔐 Xác thực & Tài khoản (Public)`
- [ ] `VerificationController` → `🔐 Xác thực & Tài khoản (Public)`
- [ ] `AccountRecoveryController` → `🔐 Xác thực & Tài khoản (Public)`
- [ ] `UserController` → `🔐 Xác thực & Tài khoản (Admin)`

### **🛠️ Quản lý WorkOrder**
- [ ] `WorkOrderController` → `🛠️ Quản lý WorkOrder (Staff/Technician)`

### **💰 Quản lý tài chính**
- [ ] `InvoiceController` → `💰 Quản lý tài chính - Hóa đơn (Staff/Admin)`
- [ ] `PaymentController` → `💰 Quản lý tài chính - Thanh toán (Customer/Staff)`

### **📊 Báo cáo & Thống kê**
- [ ] `ReportController` → `📊 Báo cáo & Thống kê (Admin)`

### **📂 Tra cứu chung**
- [ ] `LookupController` → `📂 Tra cứu chung (Public)`

---

## 🔧 CÁCH ÁP DỤNG

```csharp
[ApiController]
[Route("api/...")]
[Authorize(Policy = "...")]
[ApiExplorerSettings(GroupName = "{Icon} {Chức năng} ({Role})")]
public class YourController : BaseController
```

### **Ví dụ cụ thể:**

```csharp
// 1. Customer đặt lịch
[ApiExplorerSettings(GroupName = "📅 Quản lý lịch hẹn (Customer)")]
public class AppointmentController : BaseController

// 2. Staff quản lý lịch
[ApiExplorerSettings(GroupName = "📅 Quản lý lịch hẹn (Staff/Admin)")]
public class AppointmentManagementController : BaseController

// 3. Public xem danh sách trung tâm
[ApiExplorerSettings(GroupName = "🏢 Quản lý trung tâm dịch vụ (Public)")]
public class ServiceCenterQueryController : BaseController

// 4. Staff CRUD trung tâm
[ApiExplorerSettings(GroupName = "🏢 Quản lý trung tâm dịch vụ (Staff/Admin)")]
public class ServiceCenterController : BaseController
```

---

## 🎨 KẾT QUẢ TRONG SWAGGER

```
📅 Quản lý lịch hẹn (Customer)
   └─ POST   /api/appointments
   └─ GET    /api/appointments/my-appointments
   └─ PUT    /api/appointments/{id}

📅 Quản lý lịch hẹn (Staff/Admin)
   └─ GET    /api/appointment-management
   └─ POST   /api/appointment-management/{id}/confirm
   └─ GET    /api/appointment-management/statistics

👤 Quản lý khách hàng (Customer)
   └─ GET    /api/customer/profile
   └─ PUT    /api/customer/profile

👤 Quản lý khách hàng (Staff/Admin)
   └─ GET    /api/customers
   └─ POST   /api/customers
   └─ DELETE /api/customers/{id}

🚗 Quản lý xe (Customer)
   └─ GET    /api/customer/vehicles
   └─ POST   /api/customer/vehicles

🚗 Quản lý xe - Hãng xe (Public)
   └─ GET    /api/car-brands
   └─ GET    /api/car-brands/{id}

🚗 Quản lý xe - Dòng xe (Public)
   └─ GET    /api/car-models
   └─ GET    /api/car-models/by-brand/{brandId}

🔧 Quản lý dịch vụ (Staff/Admin)
   └─ GET    /api/maintenance-services
   └─ POST   /api/maintenance-services

🏢 Quản lý trung tâm dịch vụ (Public)
   └─ GET    /api/service-centers
   └─ GET    /api/service-centers/{id}/availability

🔐 Xác thực & Tài khoản (Public)
   └─ POST   /api/auth/login
   └─ POST   /api/auth/register
   └─ POST   /api/auth/forgot-password
```

---

## 📝 QUY TẮC

### **1. Chức năng chính (bắt buộc):**
- Phải có Icon emoji
- Tên chức năng rõ ràng (VD: Quản lý lịch hẹn, Quản lý xe)

### **2. Role (bắt buộc):**
- `(Customer)` - API cho khách hàng
- `(Staff/Admin)` - API cho nhân viên/quản trị
- `(Staff/Technician)` - API cho nhân viên/kỹ thuật viên
- `(Admin)` - API chỉ cho admin
- `(Public)` - API công khai (không cần auth hoặc auth optional)

### **3. Sub-category (optional):**
- Nếu có nhiều controllers trong 1 chức năng
- VD: `🚗 Quản lý xe - Hãng xe (Public)`
- VD: `🚗 Quản lý xe - Dòng xe (Public)`

---

## 🚀 TEST

```bash
dotnet build
dotnet run
```

Mở: `https://localhost:5001/swagger`

**Kết quả:** Swagger gọn gàng, rõ ràng cả CHỨC NĂNG và ROLE! 🎉

---

## 📌 LƯU Ý CHO FRONTEND

Frontend có thể filter theo role dễ dàng:
```javascript
// Lọc API cho Customer
const customerAPIs = swaggerDoc.tags.filter(tag => tag.includes('(Customer)'));

// Lọc API cho Staff
const staffAPIs = swaggerDoc.tags.filter(tag => tag.includes('(Staff'));

// Lọc API Public
const publicAPIs = swaggerDoc.tags.filter(tag => tag.includes('(Public)'));
```
