# 📝 QUY TẮC ĐẶT TÊN ENDPOINTS TRONG SWAGGER

## 🎯 FORMAT: `[Đề mục] Mô tả ngắn`

### **Mục đích:**
- Phân loại endpoints theo **chức năng** trong group
- Dễ tìm, dễ đọc trong Swagger UI
- Frontend biết ngay endpoint dùng để làm gì

---

## ✅ CÁC ĐỀ MỤC CHUẨN

### **1. CRUD Operations:**
```
[Xem danh sách] - Lấy nhiều records (GET list)
[Xem chi tiết] - Lấy 1 record (GET by ID)
[Thêm mới] - Tạo record mới (POST)
[Cập nhật] - Sửa record (PUT/PATCH)
[Xóa] - Xóa record (DELETE)
```

### **2. Appointment/Booking:**
```
[Đặt lịch] - Tạo appointment/booking
[Dời lịch] - Reschedule
[Hủy lịch] - Cancel
[Xác nhận] - Confirm
[Check-in] - Check in khách
[NoShow] - Đánh dấu không đến
```

### **3. Search/Filter:**
```
[Tra cứu] - Tìm kiếm theo điều kiện
[Lọc] - Filter data
[Tìm kiếm] - Search
```

### **4. Reports/Statistics:**
```
[Thống kê] - Thống kê số liệu
[Báo cáo] - Generate report
[Export] - Xuất file
```

### **5. Authentication:**
```
[Đăng nhập] - Login
[Đăng ký] - Register
[Đăng xuất] - Logout
[Quên mật khẩu] - Forgot password
[Đổi mật khẩu] - Change password
```

### **6. Other Actions:**
```
[Upload] - Tải lên file
[Download] - Tải xuống
[Gửi email] - Send email
[Gửi SMS] - Send SMS
[Approve] - Phê duyệt
[Reject] - Từ chối
```

---

## 📋 VÍ DỤ ĐÃ ÁP DỤNG

### **📅 Quản lý lịch hẹn (Customer):**
```csharp
/// <summary>
/// [Đặt lịch] Tạo lịch hẹn mới
/// </summary>
[HttpPost]
public async Task<IActionResult> CreateAppointment(...)

/// <summary>
/// [Xem danh sách] Lịch hẹn của tôi
/// </summary>
[HttpGet("my-appointments")]
public async Task<IActionResult> GetMyAppointments()

/// <summary>
/// [Xem danh sách] Lịch hẹn sắp tới
/// </summary>
[HttpGet("my-appointments/upcoming")]
public async Task<IActionResult> GetMyUpcomingAppointments(...)

/// <summary>
/// [Xem chi tiết] Lấy thông tin lịch hẹn theo ID
/// </summary>
[HttpGet("{id:int}")]
public async Task<IActionResult> GetAppointmentById(int id)

/// <summary>
/// [Tra cứu] Tìm lịch hẹn theo mã
/// </summary>
[HttpGet("by-code/{code}")]
public async Task<IActionResult> GetAppointmentByCode(string code)

/// <summary>
/// [Cập nhật] Sửa thông tin lịch hẹn
/// </summary>
[HttpPut("{id:int}")]
public async Task<IActionResult> UpdateAppointment(...)

/// <summary>
/// [Dời lịch] Đổi sang thời gian khác
/// </summary>
[HttpPost("{id:int}/reschedule")]
public async Task<IActionResult> RescheduleAppointment(...)

/// <summary>
/// [Hủy lịch] Hủy lịch hẹn đã đặt
/// </summary>
[HttpPost("{id:int}/cancel")]
public async Task<IActionResult> CancelAppointment(...)

/// <summary>
/// [Xóa] Xóa lịch hẹn (chỉ khi Pending)
/// </summary>
[HttpDelete("{id:int}")]
public async Task<IActionResult> DeleteAppointment(int id)
```

### **📅 Quản lý lịch hẹn (Staff/Admin):**
```csharp
/// <summary>
/// [Xem danh sách] Tất cả lịch hẹn (có filter/sort/paging)
/// </summary>
[HttpGet]
public async Task<IActionResult> GetAllAppointments(...)

/// <summary>
/// [Xác nhận] Confirm lịch hẹn (Pending → Confirmed)
/// </summary>
[HttpPost("{id:int}/confirm")]
public async Task<IActionResult> ConfirmAppointment(...)

/// <summary>
/// [NoShow] Đánh dấu khách không đến
/// </summary>
[HttpPost("{id:int}/mark-no-show")]
public async Task<IActionResult> MarkAsNoShow(int id)

/// <summary>
/// [Thống kê] Số lượng lịch hẹn theo trạng thái
/// </summary>
[HttpGet("statistics/by-status")]
public async Task<IActionResult> GetStatisticsByStatus()
```

---

## 🎨 KẾT QUẢ TRONG SWAGGER

Swagger sẽ hiển thị như này:

```
📅 Quản lý lịch hẹn (Customer)
   ├─ [Đặt lịch] Tạo lịch hẹn mới
   ├─ [Xem danh sách] Lịch hẹn của tôi
   ├─ [Xem danh sách] Lịch hẹn sắp tới
   ├─ [Xem chi tiết] Lấy thông tin lịch hẹn theo ID
   ├─ [Tra cứu] Tìm lịch hẹn theo mã
   ├─ [Cập nhật] Sửa thông tin lịch hẹn
   ├─ [Dời lịch] Đổi sang thời gian khác
   ├─ [Hủy lịch] Hủy lịch hẹn đã đặt
   └─ [Xóa] Xóa lịch hẹn (chỉ khi Pending)

📅 Quản lý lịch hẹn (Staff/Admin)
   ├─ [Xem danh sách] Tất cả lịch hẹn (có filter/sort/paging)
   ├─ [Xem chi tiết] Lấy chi tiết lịch hẹn
   ├─ [Xác nhận] Confirm lịch hẹn (Pending → Confirmed)
   ├─ [Check-in] Check in khách hàng
   ├─ [NoShow] Đánh dấu khách không đến
   ├─ [Thống kê] Số lượng lịch hẹn theo trạng thái
   └─ [Hủy lịch] Hủy lịch bởi Staff
```

---

## 📝 TEMPLATE ÁP DỤNG CHO CONTROLLERS KHÁC

### **👤 Quản lý khách hàng:**
```csharp
/// <summary>
/// [Xem danh sách] Tất cả khách hàng
/// </summary>
[HttpGet]

/// <summary>
/// [Xem chi tiết] Thông tin khách hàng theo ID
/// </summary>
[HttpGet("{id:int}")]

/// <summary>
/// [Thêm mới] Tạo khách hàng mới
/// </summary>
[HttpPost]

/// <summary>
/// [Cập nhật] Sửa thông tin khách hàng
/// </summary>
[HttpPut("{id:int}")]

/// <summary>
/// [Xóa] Xóa khách hàng
/// </summary>
[HttpDelete("{id:int}")]

/// <summary>
/// [Tra cứu] Tìm khách hàng theo SĐT
/// </summary>
[HttpGet("by-phone")]

/// <summary>
/// [Thống kê] Khách hàng theo loại
/// </summary>
[HttpGet("statistics")]
```

### **🚗 Quản lý xe:**
```csharp
/// <summary>
/// [Xem danh sách] Xe của tôi
/// </summary>
[HttpGet("my-vehicles")]

/// <summary>
/// [Thêm mới] Đăng ký xe mới
/// </summary>
[HttpPost]

/// <summary>
/// [Cập nhật] Cập nhật thông tin xe
/// </summary>
[HttpPut("{id:int}")]

/// <summary>
/// [Xóa] Xóa xe khỏi hệ thống
/// </summary>
[HttpDelete("{id:int}")]

/// <summary>
/// [Tra cứu] Tìm xe theo biển số
/// </summary>
[HttpGet("by-plate/{plate}")]
```

### **🔐 Xác thực & Tài khoản:**
```csharp
/// <summary>
/// [Đăng nhập] Login vào hệ thống
/// </summary>
[HttpPost("login")]

/// <summary>
/// [Đăng ký] Tạo tài khoản mới
/// </summary>
[HttpPost("register")]

/// <summary>
/// [Đăng xuất] Logout khỏi hệ thống
/// </summary>
[HttpPost("logout")]

/// <summary>
/// [Quên mật khẩu] Gửi email reset password
/// </summary>
[HttpPost("forgot-password")]

/// <summary>
/// [Đổi mật khẩu] Thay đổi mật khẩu
/// </summary>
[HttpPost("change-password")]

/// <summary>
/// [Xác thực] Verify email
/// </summary>
[HttpPost("verify-email")]
```

---

## 🔧 CÁCH ÁP DỤNG

### **Bước 1: Thêm XML comment với format `[Đề mục] Mô tả`**

```csharp
/// <summary>
/// [Đề mục] Mô tả ngắn gọn
/// </summary>
[HttpMethod]
public async Task<IActionResult> ActionName(...)
```

### **Bước 2: Chọn đề mục phù hợp từ danh sách trên**

### **Bước 3: Viết mô tả ngắn gọn, rõ ràng**

---

## ✅ QUY TẮC

1. **Bắt buộc có `[Đề mục]`** ở đầu summary
2. **Đề mục phải trong ngoặc vuông** `[...]`
3. **Mô tả phải ngắn gọn** (5-10 từ)
4. **Dùng tiếng Việt** cho dễ hiểu
5. **Nhất quán** trong toàn bộ project

---

## 🚀 TEST

```bash
dotnet build
dotnet run
```

Mở: `https://localhost:5001/swagger`

**Kết quả:** Endpoints được phân loại rõ ràng, dễ tìm! 📚

---

## 📌 LƯU Ý

- Swagger sẽ lấy text từ XML `<summary>` để hiển thị
- Đề mục trong `[...]` giúp nhóm endpoints theo chức năng
- Frontend có thể parse `[Đề mục]` để tạo menu/sidebar tự động
