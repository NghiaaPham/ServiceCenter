# DEMO MAIN FLOW - EV SERVICE CENTER
## Báo cáo cho Thầy/Cô

---

## 📋 THÔNG TIN DEMO

**Tài khoản test:**
- Email: `nghiadaucau1@gmail.com`
- Password: `nghiadaucau123@`
- Role: Customer (Khách hàng)
- Customer ID: 1014

**API Base URL:** `http://localhost:5153`

---

## 🎯 MAIN FLOW KHÁCH HÀNG

Theo đề tài "EV Service Center Maintenance Management System", các chức năng chính của khách hàng bao gồm:

### **1. CHỨC NĂNG CHO KHÁCH HÀNG (Customer)**

#### a. Theo dõi xe & nhắc nhở
✅ **Đã implement:**
- Nhắc nhở bảo dưỡng định kỳ theo km hoặc thời gian
- Nhận thông báo gói bảo dưỡng định kỳ có sẵn gói dịch vụ

#### b. Đặt lịch dịch vụ
✅ **Đã implement - 2 CÁCH ĐẶT LỊCH:**

**CÁCH 1: Đặt lịch với GÓI BẢO DƯỠNG (Subscription)**
- Đặt lịch bảo dưỡng/sửa chữa trước tuyến
- Chọn trung tâm dịch vụ & loại dịch vụ
- Nhận xác nhận & thông báo trạng thái
- **CHI PHÍ: 0 VNĐ** (đã thanh toán trọn gói trước)

**CÁCH 2: Đặt lịch với DỊCH VỤ ĐƠN LẺ**
- Đặt lịch bảo dưỡng/sửa chữa trước tuyến
- Chọn trung tâm dịch vụ & loại dịch vụ
- Nhận xác nhận & thông báo trạng thái
- **CHI PHÍ:** Tính theo từng dịch vụ + Giảm giá (nếu có)

#### c. Quản lý hồ sơ & chi phí
✅ **Đã implement:**
- Lưu lịch sử bảo dưỡng xe điện
- Quản lý chi phí bảo dưỡng & sửa chữa theo từng lần
- Thanh toán online (**Sẽ tích hợp sau**)

---

## 🚀 HƯỚNG DẪN CHẠY DEMO

### **Bước 1: Khởi động API**

```bash
cd EVServiceCenter.API
dotnet run --environment Development
```

**Hoặc** sử dụng Visual Studio: Run project `EVServiceCenter.API`

API sẽ chạy tại: `http://localhost:5153`

---

### **Bước 2: Mở Swagger UI**

Truy cập: `http://localhost:5153/swagger`

---

### **Bước 3: Test Main Flow**

#### **1. LOGIN - Đăng nhập**

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "nghiadaucau1@gmail.com",
  "password": "nghiadaucau123@"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "customerId": 1014,
    "email": "nghiadaucau1@gmail.com",
    "role": "Customer"
  }
}
```

👉 **Copy token** để dùng cho các requests tiếp theo

---

#### **2. XEM THÔNG TIN PROFILE**

```http
GET /api/customers/1014
Authorization: Bearer <token>
```

**Response:**
```json
{
  "success": true,
  "data": {
    "customerId": 1014,
    "customerCode": "KH1014",
    "fullName": "Phạm Nhật Nghĩa",
    "customerTypeName": "VIP",
    "loyaltyPoints": 150,
    "phoneNumber": "0912345678"
  }
}
```

---

#### **3. XEM DANH SÁCH XE**

```http
GET /api/customer-vehicles/my-vehicles
Authorization: Bearer <token>
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "vehicleId": 7,
      "licensePlate": "77E-55555",
      "brandName": "Hyundai",
      "modelName": "IONIQ 6",
      "year": 2024,
      "mileage": 5000
    }
  ]
}
```

---

#### **4. XEM DANH SÁCH GÓI BẢO DƯỠNG**

```http
GET /api/maintenance-packages?isActive=true
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "packageId": 1,
      "packageName": "Gói Bảo Dưỡng Cơ Bản",
      "totalPriceAfterDiscount": 1600000,
      "discountPercent": 20,
      "validityPeriodInDays": 365
    },
    {
      "packageId": 2,
      "packageName": "Gói Bảo Dưỡng Cao Cấp",
      "totalPriceAfterDiscount": 3375000,
      "discountPercent": 25,
      "validityPeriodInDays": 365
    },
    {
      "packageId": 3,
      "packageName": "Gói Bảo Dưỡng VIP",
      "totalPriceAfterDiscount": 5600000,
      "discountPercent": 30,
      "validityPeriodInDays": 730
    }
  ]
}
```

---

#### **5. XEM SUBSCRIPTION HIỆN TẠI**

```http
GET /api/package-subscriptions/my-subscriptions
Authorization: Bearer <token>
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "subscriptionId": 8,
      "packageName": "Gói Bảo Dưỡng VIP",
      "status": "Active",
      "statusDisplayName": "Đang hoạt động",
      "vehiclePlateNumber": "77E-55555",
      "usageStatus": "0/131",
      "canUse": true,
      "daysUntilExpiry": 730
    }
  ]
}
```

---

#### **6. XEM DANH SÁCH DỊCH VỤ**

```http
GET /api/maintenance-services
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "serviceId": 1,
      "serviceName": "Thay dầu động cơ",
      "categoryName": "Bảo dưỡng định kỳ",
      "estimatedDuration": 60
    },
    {
      "serviceId": 2,
      "serviceName": "Kiểm tra hệ thống phanh",
      "categoryName": "Kiểm tra an toàn",
      "estimatedDuration": 45
    }
  ]
}
```

---

#### **7. XEM LỊCH TRỐNG (Time Slots)**

```http
GET /api/time-slots/available?serviceCenterId=2&date=2025-10-15
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "slotId": 201,
      "startTime": "08:00:00",
      "endTime": "09:00:00",
      "isAvailable": true
    },
    {
      "slotId": 202,
      "startTime": "09:00:00",
      "endTime": "10:00:00",
      "isAvailable": true
    }
  ]
}
```

---

#### **8A. ĐẶT LỊCH VỚI SUBSCRIPTION (Cách 1)**

```http
POST /api/appointments
Authorization: Bearer <token>
Content-Type: application/json

{
  "customerId": 1014,
  "vehicleId": 7,
  "serviceCenterId": 2,
  "slotId": 201,
  "subscriptionId": 8,
  "appointmentDate": "2025-10-15",
  "customerNotes": "Test booking with VIP subscription",
  "preferredServices": []
}
```

**Response:**
```json
{
  "success": true,
  "message": "Đặt lịch thành công",
  "data": {
    "appointmentId": 123,
    "appointmentCode": "APT-20251013-001",
    "appointmentDate": "2025-10-15",
    "timeSlot": "08:00 - 09:00",
    "status": "Pending",
    "subscriptionCode": "SUB-TEST-1014",
    "estimatedCost": 0,
    "discountAmount": 0,
    "finalCost": 0
  }
}
```

**✅ CHI PHÍ: 0 VNĐ** (Sử dụng gói đã mua)

---

#### **8B. ĐẶT LỊCH VỚI DỊCH VỤ ĐƠN LẺ (Cách 2)**

```http
POST /api/appointments
Authorization: Bearer <token>
Content-Type: application/json

{
  "customerId": 1014,
  "vehicleId": 7,
  "serviceCenterId": 2,
  "slotId": 202,
  "appointmentDate": "2025-10-15",
  "customerNotes": "Test booking with individual services",
  "preferredServices": [1, 2, 3]
}
```

**Response:**
```json
{
  "success": true,
  "message": "Đặt lịch thành công",
  "data": {
    "appointmentId": 124,
    "appointmentCode": "APT-20251013-002",
    "appointmentDate": "2025-10-15",
    "timeSlot": "09:00 - 10:00",
    "status": "Pending",
    "estimatedCost": 1500000,
    "discountAmount": 225000,
    "finalCost": 1275000
  }
}
```

**💰 CHI PHÍ:**
- Ước tính: 1,500,000 VNĐ
- Giảm giá: 225,000 VNĐ (15% - VIP customer)
- Thanh toán: 1,275,000 VNĐ

---

#### **9. XEM DANH SÁCH LỊCH ĐÃ ĐẶT**

```http
GET /api/appointments/my-appointments
Authorization: Bearer <token>
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "appointmentId": 123,
      "appointmentCode": "APT-20251013-001",
      "appointmentDate": "2025-10-15",
      "timeSlot": "08:00 - 09:00",
      "statusName": "Đang chờ xác nhận",
      "vehiclePlateNumber": "77E-55555",
      "subscriptionCode": "SUB-TEST-1014"
    },
    {
      "appointmentId": 124,
      "appointmentCode": "APT-20251013-002",
      "appointmentDate": "2025-10-15",
      "timeSlot": "09:00 - 10:00",
      "statusName": "Đang chờ xác nhận",
      "vehiclePlateNumber": "77E-55555",
      "finalCost": 1275000
    }
  ]
}
```

---

## ✅ TỔNG KẾT CHỨC NĂNG ĐÃ IMPLEMENT

| STT | Chức năng | Trạng thái |
|-----|-----------|-----------|
| 1 | Login - Đăng nhập | ✅ Hoàn thành |
| 2 | Xem profile khách hàng | ✅ Hoàn thành |
| 3 | Xem danh sách xe | ✅ Hoàn thành |
| 4 | Xem danh sách gói bảo dưỡng | ✅ Hoàn thành |
| 5 | Xem subscription hiện tại | ✅ Hoàn thành |
| 6 | Xem danh sách dịch vụ | ✅ Hoàn thành |
| 7 | Xem lịch trống (time slots) | ✅ Hoàn thành |
| 8 | Đặt lịch với subscription | ✅ Hoàn thành |
| 9 | Đặt lịch với dịch vụ đơn lẻ | ✅ Hoàn thành |
| 10 | Xem danh sách lịch đã đặt | ✅ Hoàn thành |
| 11 | Quản lý lịch sử bảo dưỡng | ✅ Hoàn thành |
| 12 | Quản lý chi phí | ✅ Hoàn thành |

---

## 🔄 FLOW HOẠT ĐỘNG

```
┌─────────────────────────────────────────────────────────┐
│                   KHÁCH HÀNG LOGIN                       │
└──────────────────┬──────────────────────────────────────┘
                   │
                   ▼
┌─────────────────────────────────────────────────────────┐
│              XEM THÔNG TIN & QUẢN LÝ XE                  │
│  • Profile khách hàng                                    │
│  • Danh sách xe                                          │
│  • Lịch sử bảo dưỡng                                     │
└──────────────────┬──────────────────────────────────────┘
                   │
                   ▼
┌─────────────────────────────────────────────────────────┐
│               XEM CÁC GÓI & DỊCH VỤ                      │
│  • Danh sách gói bảo dưỡng                               │
│  • Subscription hiện tại                                 │
│  • Danh sách dịch vụ đơn lẻ                              │
└──────────────────┬──────────────────────────────────────┘
                   │
                   ▼
┌─────────────────────────────────────────────────────────┐
│                  CHỌN CÁCH ĐẶT LỊCH                      │
│                                                          │
│  ┌──────────────────┐       ┌──────────────────┐        │
│  │  CÁCH 1:         │       │  CÁCH 2:         │        │
│  │  VỚI SUBSCRIPTION│       │  VỚI DỊCH VỤ ĐƠN │        │
│  │  (0 VNĐ)         │       │  (Tính tiền)     │        │
│  └────────┬─────────┘       └────────┬─────────┘        │
│           │                          │                  │
└───────────┼──────────────────────────┼──────────────────┘
            │                          │
            └──────────┬───────────────┘
                       ▼
┌─────────────────────────────────────────────────────────┐
│                 CHỌN TRUNG TÂM & GIỜ                     │
│  • Chọn service center                                   │
│  • Chọn ngày & giờ (time slot)                           │
└──────────────────┬──────────────────────────────────────┘
                   │
                   ▼
┌─────────────────────────────────────────────────────────┐
│                  TẠO APPOINTMENT                         │
│  • Xác nhận thông tin                                    │
│  • Tạo lịch hẹn                                          │
│  • Nhận mã appointment                                   │
└──────────────────┬──────────────────────────────────────┘
                   │
                   ▼
┌─────────────────────────────────────────────────────────┐
│              XEM & QUẢN LÝ LỊCH HẸN                      │
│  • Danh sách lịch hẹn                                    │
│  • Trạng thái lịch hẹn                                   │
│  • Lịch sử bảo dưỡng                                     │
└─────────────────────────────────────────────────────────┘
```

---

## 📝 GHI CHÚ

### **Điểm mạnh của hệ thống:**

1. **Linh hoạt trong đặt lịch:**
   - Hỗ trợ 2 cách đặt lịch (với gói / với dịch vụ đơn lẻ)
   - Khách hàng có thể chọn cách phù hợp với nhu cầu

2. **Quản lý subscription thông minh:**
   - Theo dõi lượt sử dụng
   - Cảnh báo sắp hết hạn
   - Tự động tính chi phí

3. **Giảm giá tự động:**
   - Theo loại khách hàng (VIP = 15%)
   - Theo gói đã mua (20-30%)
   - Linh hoạt và minh bạch

### **Chức năng cần tích hợp sau:**

- 🔲 Thanh toán online (MoMo, ZaloPay, VNPay)
- 🔲 Thông báo qua email/SMS
- 🔲 Đánh giá dịch vụ

---

## 🎥 DEMO CHO THẦY/CÔ

**Bước 1:** Mở Swagger UI tại `http://localhost:5153/swagger`

**Bước 2:** Test các API theo thứ tự trong tài liệu này

**Bước 3:** Hoặc chạy PowerShell script tự động:
```powershell
.\test-mainflow.ps1
```

---

**Người thực hiện:** Sinh viên
**Ngày:** 13/10/2025
**Trạng thái:** Main flow hoàn thành, sẵn sàng demo

---
