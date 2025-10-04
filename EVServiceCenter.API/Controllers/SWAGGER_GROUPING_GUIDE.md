# 📚 SWAGGER GROUPING GUIDE

## 🎯 Mục đích
Nhóm các controllers trong Swagger theo **module/chức năng** để dễ tìm và dễ nhìn hơn.

---

## ✅ HOÀN THÀNH REFACTORING (Updated: Oct 10, 2025)

### **📦 Customer APIs (Customer-facing):**
- ✅ `Customer - Profile` → CustomerProfileController
- ✅ `Customer - Appointments` → AppointmentController

### **🔧 Staff APIs (Internal Management):**
- ✅ `Staff - Appointments` → AppointmentManagementController
- ✅ `Staff - Customers` → CustomersController  
- ✅ `Staff - Customer Types` → CustomerTypesController
- ✅ `Staff - Vehicles` → CustomerVehicleController, CustomerVehicleQueryController, CustomerVehicleStatisticsController
- ✅ `Staff - Car Brands` → CarBrandController, CarBrandQueryController, CarBrandStatisticsController
- ✅ `Staff - Car Models` → CarModelController, CarModelQueryController, CarModelStatisticsController
- ✅ `Staff - Service Centers` → ServiceCenterController, ServiceCenterQueryController, ServiceCenterStatisticsController, ServiceCenterAvailabilityController
- ✅ `Staff - Services` → MaintenanceServiceController
- ✅ `Staff - Service Categories` → ServiceCategoryController
- ✅ `Staff - Time Slots` → TimeSlotCommandController, TimeSlotQueryController
- ✅ `Staff - Pricing` → ModelServicePricingController

### **👨‍💼 Admin APIs (Administration):**
- ✅ `Admin - Users` → UserController

### **🌐 Public APIs (No authentication required):**
- ✅ `Public - Authentication` → AuthController, ExternalAuthController
- ✅ `Public - Verification` → VerificationController
- ✅ `Public - Registration` → CustomerRegistrationController
- ✅ `Public - Lookups` → LookupController

---

## 🎨 Kết quả trong Swagger:

Swagger sẽ tự động sắp xếp theo alphabet (A-Z):

```
📁 Admin - Users
   └─ GET    /api/users
   └─ GET    /api/users/{id}
   └─ PUT    /api/users/{id}
   └─ DELETE /api/users/{id}

📁 Customer - Appointments  
   └─ POST   /api/appointments
   └─ GET    /api/appointments/{id}
   └─ GET    /api/appointments/my-appointments
   └─ PUT    /api/appointments/{id}
   └─ POST   /api/appointments/{id}/reschedule
   └─ POST   /api/appointments/{id}/cancel
   └─ DELETE /api/appointments/{id}

📁 Customer - Profile
   └─ GET    /api/customer/profile/me
   └─ PUT    /api/customer/profile/me

📁 Public - Authentication
   └─ POST   /api/auth/login
   └─ POST   /api/auth/register
   └─ POST   /api/auth/logout
   └─ POST   /api/auth/external/google
   └─ POST   /api/auth/external/facebook

📁 Public - Registration
   └─ POST   /api/customer-registration

📁 Public - Verification
   └─ POST   /api/verification/verify-email
   └─ POST   /api/verification/resend-verification

📁 Staff - Appointments
   └─ GET    /api/appointment-management
   └─ POST   /api/appointment-management
   └─ GET    /api/appointment-management/{id}
   └─ POST   /api/appointment-management/{id}/confirm
   └─ POST   /api/appointment-management/{id}/cancel
   └─ GET    /api/appointment-management/statistics

📁 Staff - Car Brands
   └─ GET    /api/car-brands
   └─ POST   /api/car-brands
   └─ GET    /api/car-brands/{id}
   └─ PUT    /api/car-brands/{id}
   └─ DELETE /api/car-brands/{id}
   └─ GET    /api/car-brands/statistics

📁 Staff - Car Models
   └─ GET    /api/car-models
   └─ POST   /api/car-models
   └─ GET    /api/car-models/{id}
   └─ PUT    /api/car-models/{id}
   └─ DELETE /api/car-models/{id}

📁 Staff - Customers
   └─ GET    /api/customers
   └─ POST   /api/customers
   └─ GET    /api/customers/{id}
   └─ PUT    /api/customers/{id}
   └─ DELETE /api/customers/{id}
   └─ GET    /api/customers/statistics

📁 Staff - Service Centers
   └─ GET    /api/service-centers
   └─ POST   /api/service-centers
   └─ GET    /api/service-centers/{id}
   └─ PUT    /api/service-centers/{id}
   └─ GET    /api/service-centers/statistics

📁 Staff - Vehicles
   └─ GET    /api/customer-vehicles
   └─ POST   /api/customer-vehicles
   └─ GET    /api/customer-vehicles/{id}
   └─ PUT    /api/customer-vehicles/{id}
   └─ DELETE /api/customer-vehicles/{id}
```

---

## 🎯 Best Practices (Đã áp dụng)

### **1. Quy tắc đặt tên module:**
✅ **Pattern: `{Role} - {Feature}`**

- **Admin - {Feature}** → API cho admin (quản trị hệ thống)
- **Customer - {Feature}** → API cho customer (khách hàng tự dùng)
- **Public - {Feature}** → API public (không cần auth hoặc auth optional)
- **Staff - {Feature}** → API cho staff/technician (nhân viên nội bộ)

### **2. Sắp xếp tự động:**
Swagger tự động sort theo alphabet, cho kết quả:
1. **Admin -** ... (quản trị)
2. **Customer -** ... (khách hàng)
3. **Public -** ... (công khai)
4. **Staff -** ... (nhân viên)

### **3. Lợi ích của pattern này:**
- ✅ **Ngắn gọn, dễ đọc** - Không dùng emoji, không dùng tiếng Việt dài dòng
- ✅ **Consistent** - Tất cả đều theo format giống nhau
- ✅ **Dễ tìm kiếm** - Swagger có search box, gõ "Staff" hoặc "Customer" là lọc được ngay
- ✅ **Professional** - Phù hợp với best practices của ngành
- ✅ **Scalable** - Dễ thêm controller mới mà không làm rối structure

---

## 🚀 Cách thêm Controller mới

### **Template:**

```csharp
[ApiController]
[Route("api/...")]
[Authorize(Policy = "...")]
[ApiExplorerSettings(GroupName = "{Role} - {Feature}")]
public class YourController : BaseController
{
    // Your endpoints here
}
```

### **Chọn Role phù hợp:**

| Loại Controller | Role | Example |
|----------------|------|---------|
| Khách hàng tự sử dụng | `Customer` | "Customer - Orders" |
| Nhân viên quản lý | `Staff` | "Staff - Inventory" |
| Admin quản trị | `Admin` | "Admin - Settings" |
| Không cần auth | `Public` | "Public - Catalog" |

### **Chọn Feature name:**

- Dùng **danh từ số nhiều** cho CRUD endpoints: `Customers`, `Products`, `Orders`
- Dùng **danh từ số ít** cho single-purpose: `Profile`, `Dashboard`
- **Ngắn gọn, súc tích**: `Appointments` thay vì `Appointment Management`
- **Nhóm liên quan**: `Car Brands` và `Car Models` gần nhau trong alphabet

---

## 🧪 Testing

Sau khi refactor, đã verify:

1. ✅ **Build successful** - No compilation errors
2. ✅ **All controllers updated** - 28 controllers refactored
3. ✅ **Consistent naming** - All follow `{Role} - {Feature}` pattern
4. ✅ **Swagger grouping** - Endpoints properly grouped in Swagger UI

### **Kiểm tra thủ công:**

```bash
# Build project
dotnet build

# Run project
dotnet run

# Mở Swagger
# https://localhost:5001/swagger

# Kiểm tra:
# - Groups được sắp xếp A-Z
# - Endpoints nằm đúng group
# - Không có group lẻ hoặc duplicate
```

---

## 📝 Notes

### **Controllers không cần refactor:**
- `BaseController` - Base class, không có endpoints
- Internal helpers, middleware - Không expose API

### **Future enhancements:**
- [ ] Thêm versioning: `Customer - Appointments (v1)`, `Customer - Appointments (v2)`
- [ ] Thêm badges: `[Beta]`, `[Deprecated]`
- [ ] Custom sorting nếu cần (hiện tại dùng alphabet sort)

---

## 📚 Related Documentation

- **Program.cs** (line 40-57) - Swagger configuration
- **APPOINTMENT_API_ENDPOINTS.md** - Detailed API documentation
- **README.md** trong từng module - Module-specific docs

---

**✨ Refactoring completed successfully! Enjoy your clean and organized Swagger documentation! 🎉**
