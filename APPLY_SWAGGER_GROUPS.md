# 🚀 QUICK GUIDE: Nhóm Swagger theo Module

## ✅ ĐÃ XONG:

### **1. Program.cs**
- ✅ Đã config `TagActionsBy()` để nhóm endpoints (line 40-57)

### **2. Controllers đã update:**
- ✅ `AppointmentController` → **Customer - Appointments**
- ✅ `AppointmentManagementController` → **Staff - Appointment Management**
- ✅ `CustomersController` → **Staff - Customer Management**
- ✅ `CustomerProfileController` → **Customer - Profile**

---

## 🔧 CÁCH ÁP DỤNG CHO CONTROLLERS KHÁC:

### **Bước 1: Mở controller cần update**

### **Bước 2: Thêm 1 dòng này vào trước `public class`:**

```csharp
[ApiExplorerSettings(GroupName = "Tên Module")]
```

### **Bước 3: Chọn tên module:**

#### **🙋 Customer APIs:**
```csharp
[ApiExplorerSettings(GroupName = "Customer - Profile")]
[ApiExplorerSettings(GroupName = "Customer - Appointments")]
[ApiExplorerSettings(GroupName = "Customer - Vehicles")]
[ApiExplorerSettings(GroupName = "Customer - Service History")]
[ApiExplorerSettings(GroupName = "Customer - Invoices")]
```

#### **👨‍💼 Staff APIs:**
```csharp
[ApiExplorerSettings(GroupName = "Staff - Customer Management")]
[ApiExplorerSettings(GroupName = "Staff - Appointment Management")]
[ApiExplorerSettings(GroupName = "Staff - Vehicle Management")]
[ApiExplorerSettings(GroupName = "Staff - Work Orders")]
[ApiExplorerSettings(GroupName = "Staff - Services")]
[ApiExplorerSettings(GroupName = "Staff - Car Brands")]
[ApiExplorerSettings(GroupName = "Staff - Car Models")]
[ApiExplorerSettings(GroupName = "Staff - Service Centers")]
```

#### **👑 Admin APIs:**
```csharp
[ApiExplorerSettings(GroupName = "Admin - User Management")]
[ApiExplorerSettings(GroupName = "Admin - System Config")]
[ApiExplorerSettings(GroupName = "Admin - Reports")]
```

#### **🌐 Public APIs:**
```csharp
[ApiExplorerSettings(GroupName = "Public - Authentication")]
[ApiExplorerSettings(GroupName = "Public - Registration")]
[ApiExplorerSettings(GroupName = "Public - Service Centers")]
```

---

## 📝 VÍ DỤ CỤ THỂ:

### **TRƯỚC:**
```csharp
[ApiController]
[Route("api/car-brands")]
[Authorize(Policy = "AllInternal")]
public class CarBrandController : BaseController
{
    // ...
}
```

### **SAU:**
```csharp
[ApiController]
[Route("api/car-brands")]
[Authorize(Policy = "AllInternal")]
[ApiExplorerSettings(GroupName = "Staff - Car Brands")]  // ← THÊM DÒNG NÀY
public class CarBrandController : BaseController
{
    // ...
}
```

---

## 📋 DANH SÁCH CONTROLLERS CẦN UPDATE:

### **✅ Auth & Account (Public):**
- [ ] `AuthController` → `Public - Authentication`
- [ ] `ExternalAuthController` → `Public - Social Login`
- [ ] `VerificationController` → `Public - Email Verification`
- [ ] `AccountRecoveryController` → `Public - Password Recovery`
- [ ] `CustomerRegistrationController` → `Public - Registration`

### **✅ Car Management (Staff):**
- [ ] `CarBrandController` → `Staff - Car Brands`
- [ ] `CarBrandQueryController` → `Staff - Car Brands`
- [ ] `CarBrandStatisticsController` → `Staff - Car Brands`
- [ ] `CarModelController` → `Staff - Car Models`
- [ ] `CarModelQueryController` → `Staff - Car Models`
- [ ] `CarModelStatisticsController` → `Staff - Car Models`

### **✅ Service Center (Staff):**
- [ ] `ServiceCenterController` → `Staff - Service Centers`
- [ ] `ServiceCenterQueryController` → `Staff - Service Centers`
- [ ] `ServiceCenterStatisticsController` → `Staff - Service Centers`
- [ ] `ServiceCenterAvailabilityController` → `Staff - Service Centers`

### **✅ Services (Staff):**
- [ ] `MaintenanceServiceController` → `Staff - Services`
- [ ] `ServiceCategoryController` → `Staff - Service Categories`
- [ ] `ModelServicePricingController` → `Staff - Service Pricing`

### **✅ Vehicle Management:**
- [ ] `CustomerVehicleController` (Customer) → `Customer - Vehicles`
- [ ] `CustomerVehicleQueryController` (Staff) → `Staff - Vehicle Management`
- [ ] `CustomerVehicleStatisticsController` (Staff) → `Staff - Vehicle Management`

### **✅ Scheduling:**
- [ ] `TimeSlotCommandController` → `Staff - Time Slots`
- [ ] `TimeSlotQueryController` → `Staff - Time Slots`

### **✅ Customer Types:**
- [ ] `CustomerTypesController` → `Staff - Customer Types`

### **✅ Lookup:**
- [ ] `LookupController` → `Public - Lookups`

### **✅ Users (Admin):**
- [ ] `UserController` → `Admin - User Management`

---

## 🎯 KẾT QUẢ MONG ĐỢI:

### **Trước (lộn xộn):**
```
Appointments
AppointmentManagement
Auth
CarBrands
CarBrandQuery
CarBrandStatistics
CarModels
Customers
CustomerProfile
CustomerRegistration
...
(40+ groups riêng lẻ)
```

### **Sau (gọn gàng):**
```
📂 Admin - User Management (5 endpoints)
📂 Customer - Appointments (9 endpoints)
📂 Customer - Profile (4 endpoints)
📂 Customer - Vehicles (7 endpoints)
📂 Public - Authentication (6 endpoints)
📂 Public - Registration (2 endpoints)
📂 Staff - Appointment Management (12 endpoints)
📂 Staff - Car Brands (10 endpoints)
📂 Staff - Car Models (10 endpoints)
📂 Staff - Customer Management (15 endpoints)
📂 Staff - Service Centers (12 endpoints)
📂 Staff - Services (8 endpoints)
📂 Staff - Vehicle Management (10 endpoints)
```

**Từ 40+ groups → 13 groups rõ ràng!** 🎉

---

## 🧪 TEST:

```bash
# 1. Build
dotnet build

# 2. Run
dotnet run

# 3. Mở Swagger
https://localhost:5001/swagger

# 4. Kiểm tra: Các endpoints đã được nhóm gọn chưa?
```

---

## 💡 TIPS:

### **1. Nếu muốn tìm controllers chưa có ApiExplorerSettings:**
```bash
cd EVServiceCenter.API/Controllers
grep -L "ApiExplorerSettings" **/*.cs
```

### **2. Nếu muốn apply hàng loạt (cẩn thận):**
```bash
# Ví dụ: Thêm cho tất cả CarBrand controllers
find . -name "CarBrand*.cs" -exec sed -i 's/\[Authorize/\[ApiExplorerSettings(GroupName = "Staff - Car Brands")\]\n    [Authorize/g' {} \;
```

### **3. Quy tắc đặt tên:**
- ✅ `{Role} - {Feature}` (VD: Staff - Car Brands)
- ✅ Ngắn gọn, rõ ràng
- ✅ Consistent với các module khác
- ❌ Không dùng tên controller

---

## 🎨 MÀU SẮC TRONG SWAGGER (optional):

Nếu muốn thêm màu/icon cho từng nhóm, config thêm trong `Program.cs`:

```csharp
c.SwaggerDoc("v1", new OpenApiInfo
{
    Title = "EV Service Center API",
    Version = "v1",
    Description = @"
        ## Modules:
        - 🙋 **Customer**: APIs for customers
        - 👨‍💼 **Staff**: APIs for staff & technicians
        - 👑 **Admin**: APIs for administrators
        - 🌐 **Public**: Public APIs (no auth)
    "
});
```

---

**Bắt đầu update từ controllers quan trọng nhất trước! 🚀**

**Priority:**
1. ✅ Auth controllers (Public)
2. ✅ Customer controllers
3. ✅ Staff management controllers
4. ✅ Car/Service controllers
