# 📋 APPOINTMENT API ENDPOINTS

## 🔐 Authorization Policies

- **CustomerOnly**: Chỉ Customer
- **AllInternal**: Admin, Staff, Technician
- **AdminOrStaff**: Admin hoặc Staff
- **AdminOnly**: Chỉ Admin

---

## 👤 CUSTOMER ENDPOINTS (`/api/appointments`)

### 1. Tạo lịch hẹn mới
**POST** `/api/appointments`

**Headers:**
```
Authorization: Bearer {customer_token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "customerId": 123,
  "vehicleId": 45,
  "serviceCenterId": 1,
  "slotId": 67,
  "serviceIds": [1, 2, 3],
  "packageId": null,
  "customerNotes": "Cần làm gấp",
  "preferredTechnicianId": 89,
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
    "customerId": 123,
    "customerName": "Nguyễn Văn A",
    "vehicleId": 45,
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
  },
  "timestamp": "2025-10-03T10:30:00Z"
}
```

---

### 2. Xem lịch hẹn của tôi
**GET** `/api/appointments/my-appointments`

**Headers:**
```
Authorization: Bearer {customer_token}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Tìm thấy 3 lịch hẹn",
  "data": [
    {
      "appointmentId": 100,
      "appointmentCode": "APT202510031234",
      "vehicleName": "Tesla Model 3",
      "serviceCenterName": "Trung tâm EV Hà Nội",
      "slotDate": "2025-10-05",
      "slotStartTime": "09:00",
      "statusName": "Pending",
      "estimatedCost": 1500000
    }
  ],
  "timestamp": "2025-10-03T10:30:00Z"
}
```

---

### 3. Xem lịch hẹn sắp tới
**GET** `/api/appointments/my-appointments/upcoming?limit=5`

**Query Parameters:**
- `limit` (optional): Số lượng lịch tối đa (default: 5)

---

### 4. Xem chi tiết lịch hẹn
**GET** `/api/appointments/{id}`

**Example:** `GET /api/appointments/100`

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Lấy thông tin lịch hẹn thành công",
  "data": {
    "appointmentId": 100,
    "appointmentCode": "APT202510031234",
    "customerId": 123,
    "customerName": "Nguyễn Văn A",
    "vehicleId": 45,
    "vehicleName": "Tesla Model 3",
    "services": [...],
    "statusName": "Confirmed",
    "estimatedCost": 1500000
  }
}
```

---

### 5. Tìm lịch hẹn theo mã
**GET** `/api/appointments/by-code/{code}`

**Example:** `GET /api/appointments/by-code/APT202510031234`

---

### 6. Cập nhật lịch hẹn
**PUT** `/api/appointments/{id}`

**Request Body:**
```json
{
  "appointmentId": 100,
  "vehicleId": 46,
  "slotId": 68,
  "serviceIds": [1, 2],
  "customerNotes": "Đổi giờ",
  "priority": "Normal"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Cập nhật lịch hẹn thành công",
  "data": {...}
}
```

---

### 7. Dời lịch hẹn
**POST** `/api/appointments/{id}/reschedule`

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

### 8. Hủy lịch hẹn
**POST** `/api/appointments/{id}/cancel`

**Request Body:**
```json
{
  "appointmentId": 100,
  "cancellationReason": "Không đến được"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Hủy lịch hẹn thành công",
  "data": {
    "appointmentId": 100,
    "cancelled": true
  }
}
```

---

### 9. Xóa lịch hẹn (chỉ khi Pending)
**DELETE** `/api/appointments/{id}`

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Xóa lịch hẹn thành công",
  "data": {
    "appointmentId": 100,
    "deleted": true
  }
}
```

---

## 🏢 STAFF/ADMIN ENDPOINTS (`/api/appointment-management`)

### 1. Xem tất cả lịch hẹn (có filter & pagination)
**GET** `/api/appointment-management`

**Query Parameters:**
- `page` (int): Trang hiện tại (default: 1)
- `pageSize` (int): Số lượng/trang (default: 10)
- `customerId` (int): Lọc theo customer
- `serviceCenterId` (int): Lọc theo trung tâm
- `statusId` (int): Lọc theo trạng thái (1-8)
- `startDate` (DateOnly): Từ ngày (yyyy-MM-dd)
- `endDate` (DateOnly): Đến ngày
- `priority` (string): High/Normal/Urgent
- `source` (string): Online/Walk-in/Phone
- `searchTerm` (string): Tìm kiếm theo mã/tên
- `sortBy` (string): Trường sắp xếp (default: AppointmentDate)
- `sortOrder` (string): asc/desc (default: desc)

**Example:**
```
GET /api/appointment-management?page=1&pageSize=20&statusId=1&serviceCenterId=1&startDate=2025-10-01
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Tìm thấy 25 lịch hẹn",
  "data": {
    "items": [...],
    "totalCount": 25,
    "page": 1,
    "pageSize": 20,
    "totalPages": 2,
    "hasPreviousPage": false,
    "hasNextPage": true
  }
}
```

---

### 2. Xem chi tiết lịch hẹn (full info)
**GET** `/api/appointment-management/{id}`

---

### 3. Xem lịch hẹn theo trung tâm & ngày
**GET** `/api/appointment-management/by-service-center/{serviceCenterId}/date/{date}`

**Example:**
```
GET /api/appointment-management/by-service-center/1/date/2025-10-05
```

**Response:** Danh sách lịch hẹn trong ngày

---

### 4. Xem lịch hẹn của một khách hàng
**GET** `/api/appointment-management/by-customer/{customerId}`

**Example:**
```
GET /api/appointment-management/by-customer/123
```

---

### 5. Xác nhận lịch hẹn (Pending → Confirmed)
**POST** `/api/appointment-management/{id}/confirm`

**Authorization:** AdminOrStaff only

**Request Body:**
```json
{
  "appointmentId": 100,
  "confirmationMethod": "Phone"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Xác nhận lịch hẹn thành công",
  "data": {
    "appointmentId": 100,
    "confirmed": true
  }
}
```

---

### 6. Check-in khách hàng (Confirmed → CheckedIn)
**POST** `/api/appointment-management/{id}/check-in`

**Authorization:** AdminOrStaff only

**Note:** Tính năng đang phát triển, cần implement `CheckInAsync` trong service

---

### 7. Đánh dấu NoShow
**POST** `/api/appointment-management/{id}/mark-no-show`

**Authorization:** AdminOrStaff only

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Đã đánh dấu khách không đến",
  "data": {
    "appointmentId": 100,
    "noShow": true
  }
}
```

---

### 8. Hủy lịch hẹn (bởi Staff)
**POST** `/api/appointment-management/{id}/cancel`

**Authorization:** AdminOrStaff only

**Request Body:**
```json
{
  "appointmentId": 100,
  "cancellationReason": "Khách yêu cầu hủy qua điện thoại"
}
```

---

### 9. Cập nhật lịch hẹn (bởi Staff)
**PUT** `/api/appointment-management/{id}`

**Authorization:** AdminOrStaff only

---

### 10. Xóa lịch hẹn
**DELETE** `/api/appointment-management/{id}`

**Authorization:** AdminOnly

---

### 11. Tạo lịch hẹn cho khách (Walk-in/Phone)
**POST** `/api/appointment-management`

**Authorization:** AdminOrStaff only

**Request Body:** Giống như Customer create appointment

---

### 12. Thống kê theo trạng thái
**GET** `/api/appointment-management/statistics/by-status`

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Lấy thống kê lịch hẹn thành công",
  "data": {
    "total": 150,
    "byStatus": {
      "Pending": 25,
      "Confirmed": 30,
      "CheckedIn": 5,
      "InProgress": 10,
      "Completed": 60,
      "Cancelled": 15,
      "Rescheduled": 3,
      "NoShow": 2
    },
    "activeAppointments": 70
  }
}
```

---

## 📊 APPOINTMENT STATUS ENUM

| ID | Name | Mô tả |
|----|------|-------|
| 1 | Pending | Vừa tạo, chờ xác nhận |
| 2 | Confirmed | Staff đã xác nhận |
| 3 | CheckedIn | Khách đã đến trung tâm |
| 4 | InProgress | Đang thực hiện dịch vụ |
| 5 | Completed | Hoàn thành |
| 6 | Cancelled | Đã hủy |
| 7 | Rescheduled | Đã dời lịch (lịch cũ) |
| 8 | NoShow | Khách không đến |

---

## 🔄 LUỒNG TRẠNG THÁI

```
1. Customer tạo → Pending (1)
2. Staff xác nhận → Confirmed (2)
3. Khách đến trung tâm → CheckedIn (3)
4. Bắt đầu làm việc → InProgress (4)
5. Hoàn thành → Completed (5)

Hoặc:
- Hủy → Cancelled (6)
- Dời lịch → Rescheduled (7), tạo lịch mới Pending (1)
- Không đến → NoShow (8)
```

---

## 🧪 TEST VỚI POSTMAN/SWAGGER

### 1. **Login để lấy token:**
```
POST /api/auth/login
{
  "email": "customer@example.com",
  "password": "password"
}
```

### 2. **Copy token và thêm vào Header:**
```
Authorization: Bearer {token}
```

### 3. **Gọi API endpoints theo thứ tự:**
- Tạo appointment (Customer)
- Xem my-appointments (Customer)
- Xác nhận appointment (Staff)
- Check-in appointment (Staff)
- Thống kê (Staff)

---

## ⚠️ LƯU Ý QUAN TRỌNG

1. **Customer chỉ thao tác với lịch của mình:**
   - Kiểm tra `GetCurrentCustomerId()` trong mọi endpoint
   - Trả `403 Forbidden` nếu không có quyền

2. **Status transitions hợp lệ:**
   - Pending → Confirmed/Cancelled
   - Confirmed → CheckedIn/Cancelled/NoShow
   - CheckedIn → InProgress/Cancelled
   - InProgress → Completed
   - Completed → KHÔNG thể đổi

3. **Reschedule logic:**
   - Lịch CŨ đổi sang `Rescheduled (7)`
   - Tạo lịch MỚI với `Pending (1)`
   - Copy tất cả services từ cũ sang mới

4. **Pricing calculation:**
   - Tìm `ModelServicePricing` theo vehicleModelId + serviceId
   - Nếu có → dùng CustomPrice & CustomTime
   - Nếu không → dùng BasePrice & StandardTime

---

## 🚀 NEXT STEPS

1. ✅ Đã tạo: **AppointmentController** (Customer)
2. ✅ Đã tạo: **AppointmentManagementController** (Staff/Admin)
3. ⏳ Cần làm tiếp:
   - Implement `CheckInAsync()` trong AppointmentCommandService
   - Tạo WorkOrderController (tạo WorkOrder từ CheckedIn appointment)
   - Notification service (auto gửi khi status thay đổi)
   - Email/SMS integration

---

**📝 Ghi chú:** File này dùng để test API. Mở Swagger UI tại `/swagger` để xem interactive docs.
