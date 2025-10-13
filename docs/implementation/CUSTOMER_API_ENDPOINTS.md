# 📱 CUSTOMER API ENDPOINTS - DANH SÁCH ĐẦY ĐỦ

> **Mục đích:** Document này liệt kê TẤT CẢ các API endpoints dành cho Customer để Frontend team checklist và triển khai.

---

## 🔐 AUTHORIZATION

Tất cả endpoints dưới đây **BẮT BUỘC** phải có JWT token trong header:

```http
Authorization: Bearer {token_from_login}
```

**Lấy token:** Gọi API login trước (xem mục Authentication bên dưới)

---

## 📋 MỤC LỤC

1. [Authentication](#1-authentication---xác-thực)
2. [Customer Profile](#2-customer-profile---hồ-sơ-khách-hàng)
3. [My Vehicles](#3-my-vehicles---xe-của-tôi)
4. [Appointments](#4-appointments---đặt-lịch-bảo-dưỡng)
5. [Package Subscriptions](#5-package-subscriptions---gói-dịch-vụ)
6. [Lookup Data](#6-lookup-data---dữ-liệu-tra-cứu)

---

## 1. AUTHENTICATION - XÁC THỰC

### 1.1. Đăng ký tài khoản Customer

```http
POST /api/customer-registration/register
```

**Authorization:** AllowAnonymous (không cần token)

**Request Body:**

```json
{
  "username": "customer123",
  "email": "customer@example.com",
  "password": "Password@123",
  "confirmPassword": "Password@123",
  "fullName": "Nguyễn Văn A",
  "phoneNumber": "0912345678",
  "address": "123 Đường ABC, Hà Nội",
  "dateOfBirth": "1990-01-15",
  "gender": "Male",
  "identityNumber": "001234567890"
}
```

**Response (201 Created):**

```json
{
  "success": true,
  "message": "Đăng ký thành công! Vui lòng kiểm tra email để xác thực tài khoản.",
  "data": {
    "userId": 123,
    "customerId": 45,
    "customerCode": "CUST202510001",
    "email": "customer@example.com",
    "requireEmailVerification": true,
    "verificationEmailSent": true
  }
}
```

---

### 1.2. Đăng nhập

```http
POST /api/auth/login
```

**Authorization:** AllowAnonymous

**Request Body:**

```json
{
  "username": "customer123",
  "password": "Password@123"
}
```

**Response (200 OK):**

```json
{
  "success": true,
  "message": "Đăng nhập thành công",
  "data": {
    "user": {
      "userId": 123,
      "username": "customer123",
      "fullName": "Nguyễn Văn A",
      "email": "customer@example.com",
      "roleName": "Customer"
    },
    "customer": {
      "customerId": 45,
      "customerCode": "CUST202510001",
      "loyaltyPoints": 1000,
      "totalSpent": 5000000,
      "customerTypeName": "Regular",
      "customerTypeDiscount": 5
    },
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  }
}
```

**Error - Email chưa xác thực (400 Bad Request):**

```json
{
  "success": false,
  "message": "Email chưa được xác thực. Vui lòng kiểm tra hộp thư và xác thực email.",
  "errorCode": "EMAIL_NOT_VERIFIED",
  "data": {
    "requireEmailVerification": true,
    "username": "customer123",
    "resendVerificationUrl": "/api/verification/resend-verification",
    "instructions": [
      "Kiểm tra hộp thư email của bạn",
      "Tìm email từ EV Service Center (kiểm tra cả thư mục spam)",
      "Click vào link xác thực trong email",
      "Quay lại trang này để đăng nhập"
    ]
  }
}
```

---

### 1.3. Đăng nhập bằng Google

```http
POST /api/auth/external/google
```

**Authorization:** AllowAnonymous

**Request Body:**

```json
{
  "idToken": "google_id_token_from_google_signin"
}
```

**Response (200 OK):**

```json
{
  "success": true,
  "message": "Đăng nhập Google thành công",
  "data": {
    "user": { ... },
    "customer": { ... },
    "token": "jwt_token",
    "isNewUser": false
  }
}
```

---

### 1.4. Xác thực Email

```http
POST /api/verification/verify-email
```

**Authorization:** AllowAnonymous

**Request Body:**

```json
{
  "email": "customer@example.com",
  "token": "verification_token_from_email"
}
```

---

### 1.5. Gửi lại email xác thực

```http
POST /api/verification/resend-verification
```

**Authorization:** AllowAnonymous

**Request Body:**

```json
{
  "email": "customer@example.com"
}
```

---

### 1.6. Đổi mật khẩu (khi đã đăng nhập)

```http
PUT /api/auth/change-password
```

**Authorization:** Required (Customer)

**Request Body:**

```json
{
  "currentPassword": "OldPassword@123",
  "newPassword": "NewPassword@456",
  "confirmNewPassword": "NewPassword@456"
}
```

---

### 1.7. Quên mật khẩu (gửi OTP)

```http
POST /api/account/forgot-password
```

**Authorization:** AllowAnonymous

**Request Body:**

```json
{
  "email": "customer@example.com"
}
```

---

### 1.8. Đặt lại mật khẩu (với OTP)

```http
POST /api/account/reset-password
```

**Authorization:** AllowAnonymous

**Request Body:**

```json
{
  "email": "customer@example.com",
  "otp": "123456",
  "newPassword": "NewPassword@789",
  "confirmPassword": "NewPassword@789"
}
```

---

## 2. CUSTOMER PROFILE - HỒ SƠ KHÁCH HÀNG

### 2.1. Xem thông tin hồ sơ của tôi

```http
GET /api/customer/profile/me
```

**Authorization:** Required (CustomerOnly)

**Response (200 OK):**

```json
{
  "success": true,
  "message": "Lấy thông tin thành công",
  "data": {
    "customerId": 45,
    "customerCode": "CUST202510001",
    "fullName": "Nguyễn Văn A",
    "email": "customer@example.com",
    "phoneNumber": "0912345678",
    "address": "123 Đường ABC, Hà Nội",
    "dateOfBirth": "1990-01-15",
    "gender": "Male",
    "loyaltyPoints": 1000,
    "totalSpent": 5000000,
    "customerTypeName": "Regular",
    "customerTypeDiscount": 5,
    "preferredLanguage": "vi",
    "marketingOptIn": true,
    "createdDate": "2025-01-01T00:00:00Z"
  }
}
```

---

### 2.2. Cập nhật thông tin hồ sơ

```http
PUT /api/customer/profile/me
```

**Authorization:** Required (CustomerOnly)

**Request Body:**

```json
{
  "fullName": "Nguyễn Văn B",
  "phoneNumber": "0987654321",
  "address": "456 Đường XYZ, TP.HCM",
  "dateOfBirth": "1990-01-15",
  "gender": "Male",
  "preferredLanguage": "vi",
  "marketingOptIn": true
}
```

**Customer KHÔNG THỂ sửa:**

- Email (tied to User account)
- CustomerCode (auto-generated)
- LoyaltyPoints (chỉ qua giao dịch)
- TypeId (chỉ Staff/Admin)

---

## 3. MY VEHICLES - XE CỦA TÔI

### 3.1. Xem danh sách xe của tôi

```http
GET /api/customer/profile/my-vehicles
```

**Authorization:** Required (CustomerOnly)

**Response (200 OK):**

```json
{
  "success": true,
  "message": "Tìm thấy 2 xe",
  "data": [
    {
      "vehicleId": 10,
      "licensePlate": "30A-12345",
      "vin": "5YJSA1E14HF123456",
      "brandName": "Tesla",
      "modelName": "Model 3",
      "color": "Trắng",
      "mileage": 15000,
      "purchaseDate": "2023-05-10",
      "nextMaintenanceDate": "2025-11-01",
      "batteryHealthPercent": 95,
      "insuranceNumber": "INS12345",
      "insuranceExpiry": "2026-05-10",
      "registrationExpiry": "2025-05-10"
    }
  ]
}
```

---

### 3.2. Đăng ký xe mới

```http
POST /api/customer/profile/my-vehicles
```

**Authorization:** Required (CustomerOnly)

**Request Body:**

```json
{
  "modelId": 5,
  "licensePlate": "30A-99999",
  "vin": "5YJSA1E14HF999999",
  "color": "Đen",
  "purchaseDate": "2024-10-01",
  "mileage": 5000,
  "insuranceNumber": "INS99999",
  "insuranceExpiry": "2026-10-01",
  "registrationExpiry": "2025-10-01"
}
```

**Response (201 Created):**

```json
{
  "success": true,
  "message": "Đăng ký xe thành công",
  "data": {
    "vehicleId": 20,
    "licensePlate": "30A-99999"
  }
}
```

---

### 3.3. Xem chi tiết 1 xe

```http
GET /api/customer/profile/my-vehicles/{vehicleId}
```

**Authorization:** Required (CustomerOnly)

**Path Parameters:**

- `vehicleId` (int): ID của xe

---

### 3.4. Xóa xe của tôi

```http
DELETE /api/customer/profile/my-vehicles/{vehicleId}
```

**Authorization:** Required (CustomerOnly)

**Điều kiện:**

- Xe không có lịch hẹn active
- Xe không có Work Order đang mở
- Xe không có Subscription active

**Response (200 OK):**

```json
{
  "success": true,
  "message": "Đã xóa xe 30A-12345 khỏi danh sách của bạn",
  "data": {
    "vehicleId": 10,
    "licensePlate": "30A-12345",
    "deletedAt": "2025-10-03T10:00:00Z"
  }
}
```

---

### 3.5. Kiểm tra xe có thể xóa không

```http
GET /api/customer/profile/my-vehicles/{vehicleId}/can-delete
```

**Authorization:** Required (CustomerOnly)

**Response (200 OK):**

```json
{
  "success": true,
  "message": "Xe có thể được xóa",
  "data": {
    "canDelete": true,
    "vehicleId": 10,
    "licensePlate": "30A-12345",
    "reason": null
  }
}
```

Hoặc nếu không xóa được:

```json
{
  "success": true,
  "message": "Xe không thể xóa",
  "data": {
    "canDelete": false,
    "vehicleId": 10,
    "licensePlate": "30A-12345",
    "reason": "Xe đang có lịch hẹn, phiếu công việc hoặc gói dịch vụ đang hoạt động"
  }
}
```

---

## 4. APPOINTMENTS - ĐẶT LỊCH BẢO DƯỠNG

### 4.1. Tạo lịch hẹn mới

```http
POST /api/appointments
```

**Authorization:** Required (CustomerOnly)

**Request Body:**

```json
{
  "customerId": 45,
  "vehicleId": 10,
  "serviceCenterId": 1,
  "slotId": 67,
  "serviceIds": [1, 2, 3],
  "packageId": null,
  "customerNotes": "Cần làm gấp",
  "preferredTechnicianId": null,
  "priority": "High",
  "source": "Online"
}
```

**Response (201 Created):**

```json
{
  "success": true,
  "message": "Đặt lịch thành công! Mã lịch hẹn: APT202510031234",
  "data": {
    "appointmentId": 100,
    "appointmentCode": "APT202510031234",
    "customerId": 45,
    "customerName": "Nguyễn Văn A",
    "vehicleId": 10,
    "vehicleName": "Tesla Model 3",
    "licensePlate": "30A-12345",
    "serviceCenterName": "Trung tâm EV Hà Nội",
    "slotDate": "2025-10-05",
    "slotStartTime": "09:00",
    "slotEndTime": "11:00",
    "services": [
      {
        "serviceName": "Thay lốp",
        "price": 1500000,
        "estimatedTime": 60
      }
    ],
    "statusId": 1,
    "statusName": "Pending",
    "estimatedCost": 1500000,
    "estimatedDuration": 60,
    "priority": "High",
    "createdDate": "2025-10-03T10:30:00Z"
  }
}
```

---

### 4.2. Xem tất cả lịch hẹn của tôi

```http
GET /api/appointments/my-appointments
```

**Authorization:** Required (CustomerOnly)

**Response (200 OK):**

```json
{
  "success": true,
  "message": "Tìm thấy 5 lịch hẹn",
  "data": [
    {
      "appointmentId": 100,
      "appointmentCode": "APT202510031234",
      "vehicleName": "Tesla Model 3",
      "licensePlate": "30A-12345",
      "serviceCenterName": "Trung tâm EV Hà Nội",
      "slotDate": "2025-10-05",
      "slotStartTime": "09:00",
      "statusName": "Confirmed",
      "estimatedCost": 1500000
    }
  ]
}
```

---

### 4.3. Xem lịch hẹn sắp tới

```http
GET /api/appointments/my-appointments/upcoming?limit=5
```

**Authorization:** Required (CustomerOnly)

**Query Parameters:**

- `limit` (int, optional): Số lượng lịch tối đa (default: 5)

---

### 4.4. Xem chi tiết lịch hẹn

```http
GET /api/appointments/{id}
```

**Authorization:** Required (CustomerOnly)

**Path Parameters:**

- `id` (int): ID lịch hẹn

---

### 4.5. Tìm lịch hẹn theo mã

```http
GET /api/appointments/by-code/{code}
```

**Authorization:** Required (CustomerOnly)

**Path Parameters:**

- `code` (string): Mã lịch hẹn (VD: APT202510031234)

---

### 4.6. Cập nhật lịch hẹn

```http
PUT /api/appointments/{id}
```

**Authorization:** Required (CustomerOnly)

**Điều kiện:**

- Lịch hẹn phải ở trạng thái Pending hoặc Confirmed
- Customer chỉ sửa được lịch của mình

**Request Body:**

```json
{
  "appointmentId": 100,
  "vehicleId": 10,
  "slotId": 68,
  "serviceIds": [1, 2],
  "customerNotes": "Đổi giờ",
  "priority": "Normal"
}
```

---

### 4.7. Dời lịch hẹn

```http
POST /api/appointments/{id}/reschedule
```

**Authorization:** Required (CustomerOnly)

**Request Body:**

```json
{
  "appointmentId": 100,
  "newSlotId": 70,
  "reason": "Bận việc đột xuất"
}
```

**Response (200 OK):**

```json
{
  "success": true,
  "message": "Dời lịch thành công! Mã lịch mới: APT202510041567",
  "data": {
    "appointmentId": 101,
    "appointmentCode": "APT202510041567",
    "rescheduledFromId": 100,
    "slotDate": "2025-10-06",
    "statusName": "Pending"
  }
}
```

---

### 4.8. Hủy lịch hẹn

```http
POST /api/appointments/{id}/cancel
```

**Authorization:** Required (CustomerOnly)

**Request Body:**

```json
{
  "appointmentId": 100,
  "cancellationReason": "Không đến được"
}
```

---

### 4.9. Xóa lịch hẹn (chỉ khi Pending)

```http
DELETE /api/appointments/{id}
```

**Authorization:** Required (CustomerOnly)

**Điều kiện:**

- Lịch hẹn phải ở trạng thái Pending
- Chưa được Staff confirm

---

## 5. PACKAGE SUBSCRIPTIONS - GÓI DỊCH VỤ

### 5.1. Xem danh sách gói dịch vụ của tôi

```http
GET /api/package-subscriptions/my-subscriptions?statusFilter=Active
```

**Authorization:** Required (CustomerOnly)

**Query Parameters:**

- `statusFilter` (enum, optional): Active, Expired, Cancelled, Suspended

**Response (200 OK):**

```json
{
  "success": true,
  "message": "Tìm thấy 2 subscriptions",
  "data": [
    {
      "subscriptionId": 10,
      "packageName": "Gói bảo dưỡng cơ bản",
      "packageCode": "PKG-BASIC-2025",
      "vehicleName": "Tesla Model 3",
      "licensePlate": "30A-12345",
      "status": "Active",
      "startDate": "2025-01-01",
      "endDate": "2025-12-31",
      "totalServices": 5,
      "usedServices": 2,
      "remainingServices": 3
    }
  ]
}
```

---

### 5.2. Xem chi tiết subscription

```http
GET /api/package-subscriptions/{id}
```

**Authorization:** Required (CustomerOnly)

**Path Parameters:**

- `id` (int): ID của subscription

---

### 5.3. Xem usage (đã dùng bao nhiêu)

```http
GET /api/package-subscriptions/{id}/usage
```

**Authorization:** Required (CustomerOnly)

**Response (200 OK):**

```json
{
  "success": true,
  "message": "Tìm thấy 5 services trong subscription",
  "data": [
    {
      "serviceId": 1,
      "serviceName": "Thay lốp",
      "allocatedCount": 2,
      "usedCount": 1,
      "remainingCount": 1,
      "lastUsedDate": "2025-05-15"
    }
  ]
}
```

---

### 5.4. Xem subscriptions active cho 1 xe

```http
GET /api/package-subscriptions/vehicle/{vehicleId}/active
```

**Authorization:** Required (CustomerOnly)

**Use case:** Khi customer đặt lịch, chọn xe xong thì hiển thị các gói đang active cho xe đó

---

### 5.5. Mua gói dịch vụ

```http
POST /api/package-subscriptions/purchase
```

**Authorization:** Required (CustomerOnly)

**Request Body:**

```json
{
  "packageId": 5,
  "vehicleId": 10,
  "paymentMethod": "CreditCard",
  "paymentReference": "PAY123456"
}
```

**Response (201 Created):**

```json
{
  "success": true,
  "message": "Mua gói thành công",
  "data": {
    "subscriptionId": 15,
    "packageName": "Gói bảo dưỡng cơ bản",
    "vehicleName": "Tesla Model 3",
    "startDate": "2025-10-03",
    "endDate": "2026-10-03",
    "status": "Active",
    "totalPaid": 5000000
  }
}
```

---

### 5.6. Hủy subscription

```http
POST /api/package-subscriptions/{id}/cancel
```

**Authorization:** Required (CustomerOnly)

**Request Body:**

```json
{
  "cancellationReason": "Không dùng nữa"
}
```

---

## 6. LOOKUP DATA - DỮ LIỆU TRA CỨU

### 6.1. Danh sách hãng xe

```http
GET /api/lookup/car-brands
```

**Authorization:** AllowAnonymous

**Response:**

```json
{
  "success": true,
  "data": [
    {
      "brandId": 1,
      "brandName": "Tesla",
      "logoUrl": "https://...",
      "country": "USA"
    }
  ]
}
```

---

### 6.2. Danh sách model theo hãng

```http
GET /api/lookup/car-models/by-brand/{brandId}
```

**Authorization:** AllowAnonymous

---

### 6.3. Danh sách trung tâm dịch vụ

```http
GET /api/lookup/service-centers
```

**Authorization:** AllowAnonymous

**Response:**

```json
{
  "success": true,
  "data": [
    {
      "serviceCenterId": 1,
      "centerName": "Trung tâm EV Hà Nội",
      "address": "123 Láng Hạ, Đống Đa, Hà Nội",
      "phoneNumber": "024-1234-5678",
      "openTime": "08:00",
      "closeTime": "18:00",
      "latitude": 21.0285,
      "longitude": 105.8542
    }
  ]
}
```

---

### 6.4. Time slots available (khung giờ trống)

```http
GET /api/lookup/time-slots/available?serviceCenterId=1&date=2025-10-05
```

**Authorization:** AllowAnonymous

**Query Parameters:**

- `serviceCenterId` (int): ID trung tâm dịch vụ
- `date` (DateOnly): Ngày cần check (yyyy-MM-dd)

**Response:**

```json
{
  "success": true,
  "data": [
    {
      "slotId": 67,
      "slotDate": "2025-10-05",
      "startTime": "09:00",
      "endTime": "11:00",
      "capacity": 5,
      "bookedCount": 2,
      "availableCount": 3,
      "isAvailable": true
    }
  ]
}
```

---

### 6.5. Danh sách dịch vụ

```http
GET /api/lookup/maintenance-services
```

**Authorization:** AllowAnonymous

**Response:**

```json
{
  "success": true,
  "data": [
    {
      "serviceId": 1,
      "serviceName": "Thay lốp",
      "description": "Thay lốp xe điện chuyên dụng",
      "categoryName": "Tire Service",
      "basePrice": 1500000,
      "standardTime": 60
    }
  ]
}
```

---

### 6.6. Danh sách gói bảo dưỡng (public)

```http
GET /api/maintenance-packages?page=1&pageSize=10
```

**Authorization:** AllowAnonymous

**Response:**

```json
{
  "success": true,
  "message": "Tìm thấy 10 gói dịch vụ",
  "data": {
    "items": [
      {
        "packageId": 5,
        "packageCode": "PKG-BASIC-2025",
        "packageName": "Gói bảo dưỡng cơ bản",
        "description": "Bảo dưỡng định kỳ cơ bản",
        "durationMonths": 12,
        "price": 5000000,
        "discountPercent": 10,
        "finalPrice": 4500000,
        "status": "Active",
        "isPopular": true,
        "servicesIncluded": 5
      }
    ],
    "totalCount": 25,
    "page": 1,
    "pageSize": 10,
    "totalPages": 3
  }
}
```

---

### 6.7. Gói bảo dưỡng phổ biến

```http
GET /api/maintenance-packages/popular?topCount=5
```

**Authorization:** AllowAnonymous

---

### 6.8. Chi tiết gói bảo dưỡng

```http
GET /api/maintenance-packages/{id}
```

**Authorization:** AllowAnonymous

---

### 6.9. Loại khách hàng (Customer Types)

```http
GET /api/customer-types
```

**Authorization:** AllowAnonymous

**Response:**

```json
{
  "success": true,
  "data": [
    {
      "typeId": 1,
      "typeName": "New",
      "description": "Khách hàng mới",
      "discountPercent": 0,
      "minSpend": 0,
      "maxSpend": 5000000
    },
    {
      "typeId": 2,
      "typeName": "Regular",
      "description": "Khách hàng thường xuyên",
      "discountPercent": 5,
      "minSpend": 5000000,
      "maxSpend": 20000000
    },
    {
      "typeId": 3,
      "typeName": "VIP",
      "description": "Khách hàng VIP",
      "discountPercent": 10,
      "minSpend": 20000000,
      "maxSpend": null
    }
  ]
}
```

---

## 📊 APPOINTMENT STATUS ENUM

| ID  | Name        | Mô tả                  | Customer có thể thao tác           |
| --- | ----------- | ---------------------- | ---------------------------------- |
| 1   | Pending     | Vừa tạo, chờ xác nhận  | Update, Reschedule, Cancel, Delete |
| 2   | Confirmed   | Staff đã xác nhận      | Reschedule, Cancel                 |
| 3   | CheckedIn   | Khách đã đến trung tâm | - (chỉ xem)                        |
| 4   | InProgress  | Đang thực hiện dịch vụ | - (chỉ xem)                        |
| 5   | Completed   | Hoàn thành             | - (chỉ xem)                        |
| 6   | Cancelled   | Đã hủy                 | - (chỉ xem)                        |
| 7   | Rescheduled | Đã dời lịch (lịch cũ)  | - (chỉ xem)                        |
| 8   | NoShow      | Khách không đến        | - (chỉ xem)                        |

---

## 📊 SUBSCRIPTION STATUS ENUM

| Value     | Mô tả          |
| --------- | -------------- |
| Active    | Đang hoạt động |
| Expired   | Hết hạn        |
| Cancelled | Đã hủy         |
| Suspended | Tạm ngưng      |

---

## 📊 PRIORITY ENUM (Appointment)

| Value  | Mô tả       |
| ------ | ----------- |
| Normal | Bình thường |
| High   | Ưu tiên cao |
| Urgent | Khẩn cấp    |

---

## 📊 SOURCE ENUM (Appointment)

| Value   | Mô tả                       |
| ------- | ----------------------------- |
| Online  | Đặt online qua app/web      |
| Walk-in | Khách walk-in tại trung tâm |
| Phone   | Đặt qua điện thoại          |

---

## ⚠️ LƯU Ý QUAN TRỌNG CHO FRONTEND

### 1. Authorization Headers

```javascript
// Lưu token sau khi login thành công
localStorage.setItem("authToken", response.data.token);

// Thêm vào mọi request sau đó
axios.defaults.headers.common["Authorization"] = `Bearer ${localStorage.getItem(
  "authToken"
)}`;
```

### 2. Customer chỉ xem/thao tác dữ liệu của mình

- Appointments: Chỉ xem lịch của mình
- Vehicles: Chỉ xem/thêm/xóa xe của mình
- Subscriptions: Chỉ xem gói của mình
- Profile: Chỉ xem/sửa profile của mình

### 3. Validation

- Tất cả các field bắt buộc đều có validation ở backend
- Frontend nên validate trước khi gửi để UX tốt hơn
- Error messages trả về tiếng Việt, có thể hiển thị trực tiếp

### 4. Date/Time Format

- **Date:** `yyyy-MM-dd` (VD: `2025-10-05`)
- **Time:** `HH:mm` (VD: `09:00`)
- **DateTime:** ISO 8601 (VD: `2025-10-03T10:30:00Z`)

### 5. Phone Number Format

- Validate: Vietnamese phone format
- VD: `0912345678`, `0912-345-678`, `+84912345678`

### 6. Error Handling

```javascript
try {
  const response = await api.createAppointment(data);
  // Success
} catch (error) {
  if (error.response.status === 401) {
    // Redirect to login
  } else if (error.response.status === 403) {
    // Show "Không có quyền"
  } else if (error.response.status === 400) {
    // Show validation errors
    const errors = error.response.data.errorCode;
  }
}
```

---

## 🔄 FLOW CHÍNH CHO CUSTOMER APP

### Flow 1: Đăng ký & Đăng nhập

```
1. Customer đăng ký → POST /api/customer-registration/register
2. Nhận email xác thực → Click link
3. Xác thực email → POST /api/verification/verify-email
4. Đăng nhập → POST /api/auth/login
5. Lưu token → localStorage
```

### Flow 2: Đặt lịch bảo dưỡng

```
1. Xem xe của tôi → GET /api/customer/profile/my-vehicles
2. Nếu chưa có xe → POST /api/customer/profile/my-vehicles (Đăng ký xe)
3. Xem trung tâm → GET /api/lookup/service-centers
4. Xem time slots → GET /api/lookup/time-slots/available?serviceCenterId=1&date=2025-10-05
5. Xem dịch vụ → GET /api/lookup/maintenance-services
6. Tạo lịch → POST /api/appointments
7. Nhận email xác nhận
```

### Flow 3: Mua gói bảo dưỡng

```
1. Xem gói phổ biến → GET /api/maintenance-packages/popular
2. Xem chi tiết gói → GET /api/maintenance-packages/{id}
3. Chọn xe → GET /api/customer/profile/my-vehicles
4. Mua gói → POST /api/package-subscriptions/purchase
5. Xem subscription → GET /api/package-subscriptions/my-subscriptions
```

---

## ✅ CHECKLIST CHO FRONTEND TEAM

### Authentication & Profile

- [ ] Đăng ký tài khoản Customer
- [ ] Đăng nhập bằng username/password
- [ ] Đăng nhập bằng Google
- [ ] Xác thực email
- [ ] Quên mật khẩu / Đặt lại mật khẩu
- [ ] Xem profile
- [ ] Sửa profile
- [ ] Đổi mật khẩu

### My Vehicles

- [ ] Xem danh sách xe
- [ ] Đăng ký xe mới
- [ ] Xem chi tiết xe
- [ ] Xóa xe
- [ ] Kiểm tra xe có thể xóa không

### Appointments

- [ ] Xem danh sách lịch hẹn
- [ ] Xem lịch hẹn sắp tới
- [ ] Xem chi tiết lịch hẹn
- [ ] Tạo lịch hẹn mới
- [ ] Tìm lịch theo mã
- [ ] Cập nhật lịch hẹn
- [ ] Dời lịch hẹn
- [ ] Hủy lịch hẹn
- [ ] Xóa lịch hẹn

### Package Subscriptions

- [ ] Xem danh sách subscriptions
- [ ] Xem chi tiết subscription
- [ ] Xem usage (đã dùng bao nhiêu)
- [ ] Xem subscriptions active cho xe
- [ ] Mua gói dịch vụ
- [ ] Hủy subscription

### Lookup Data

- [ ] Lấy danh sách hãng xe
- [ ] Lấy danh sách model theo hãng
- [ ] Lấy danh sách trung tâm dịch vụ
- [ ] Lấy time slots available
- [ ] Lấy danh sách dịch vụ
- [ ] Lấy danh sách gói bảo dưỡng
- [ ] Lấy gói phổ biến
- [ ] Lấy loại khách hàng

---

## 📞 HỖ TRỢ

Nếu có thắc mắc về API, liên hệ:

- Backend Team Lead
- Swagger UI: `https://localhost:5001/swagger`
- Postman Collection: (link nếu có)

---

**Ngày tạo:** 2025-10-03  
**Version:** 1.0  
**Author:** Backend Team
