# Test Subscription Booking Flow - Customer 1014

## Thông tin test
- **Customer**: 1014 (nghiadaucau1@gmail.com)
- **Password**: nghiadaucau123@
- **Vehicle ID**: 7
- **Subscription ID**: 8 (VIP Package)
- **Base URL**: http://localhost:5153

---

## BƯỚC 1: Đăng nhập

### Request:
```
POST http://localhost:5153/api/auth/login
Content-Type: application/json
```

### Body:
```json
{
  "email": "nghiadaucau1@gmail.com",
  "password": "nghiadaucau123@"
}
```

### Response mẫu:
```json
{
  "success": true,
  "message": "Đăng nhập thành công",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "userId": 1014,
    "email": "nghiadaucau1@gmail.com",
    "role": "Customer"
  }
}
```

👉 **Copy token từ response** để dùng cho các bước tiếp theo

---

## BƯỚC 2: Kiểm tra Subscription ID 8

### Request:
```
GET http://localhost:5153/api/package-subscriptions/8
Authorization: Bearer <YOUR_TOKEN_HERE>
```

### Response mẫu:
```json
{
  "success": true,
  "data": {
    "subscriptionId": 8,
    "subscriptionCode": "SUB-TEST-1014",
    "packageId": 3,
    "packageName": "Gói Bảo Dưỡng VIP",
    "customerId": 1014,
    "vehicleId": 7,
    "status": "Active",
    "startDate": "2025-10-13",
    "expirationDate": "2027-10-13",
    "remainingServices": 131,
    "paymentAmount": 5600000
  }
}
```

✅ **Xác nhận subscription Active và có lượt dịch vụ còn lại**

---

## BƯỚC 3: Lấy danh sách Time Slots có sẵn

### Request:
```
GET http://localhost:5153/api/time-slots/available?serviceCenterId=2&date=2025-10-16
```

*Lưu ý: Không cần token cho API này*

### Response mẫu:
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

👉 **Chọn 1 slotId** để dùng cho appointment (ví dụ: 201)

---

## BƯỚC 4: Tạo Appointment với Subscription

### Request:
```
POST http://localhost:5153/api/appointments
Authorization: Bearer <YOUR_TOKEN_HERE>
Content-Type: application/json
```

### Body:
```json
{
  "customerId": 1014,
  "vehicleId": 7,
  "serviceCenterId": 2,
  "slotId": 201,
  "subscriptionId": 8,
  "appointmentDate": "2025-10-16",
  "customerNotes": "Test booking with VIP subscription",
  "preferredServices": []
}
```

### Response mẫu (Success):
```json
{
  "success": true,
  "message": "Đặt lịch thành công",
  "data": {
    "appointmentId": 123,
    "appointmentCode": "APT-20251013-001",
    "customerId": 1014,
    "vehicleId": 7,
    "serviceCenterId": 2,
    "slotId": 201,
    "subscriptionId": 8,
    "subscriptionCode": "SUB-TEST-1014",
    "appointmentDate": "2025-10-16",
    "timeSlot": "08:00 - 09:00",
    "status": "Pending",
    "estimatedCost": 0,
    "discountAmount": 0,
    "finalCost": 0,
    "customerNotes": "Test booking with VIP subscription"
  }
}
```

✅ **Appointment được tạo với subscription!**

---

## BƯỚC 5: Xác nhận Appointment đã tạo

### Request:
```
GET http://localhost:5153/api/appointments/{appointmentId}
Authorization: Bearer <YOUR_TOKEN_HERE>
```

*Thay {appointmentId} bằng ID từ response bước 4*

---

## KẾT QUẢ MONG ĐỢI:

✅ **Đặt lịch thành công với subscription:**
- Appointment được tạo với `subscriptionId = 8`
- `estimatedCost = 0` (vì dùng subscription, không tính tiền riêng lẻ)
- Status = "Pending"
- Customer có thể thấy appointment trong danh sách của mình

✅ **Khi appointment Complete:**
- Lượt dịch vụ trong subscription sẽ bị trừ
- `RemainingServices` giảm xuống
- `UsedServices` tăng lên

---

## Test bằng Postman:

1. Tạo Collection mới: "EV Service Center - Subscription Booking"
2. Add các requests theo thứ tự trên
3. Sử dụng Variables cho token:
   - Tạo variable `{{token}}` trong Collection
   - Sau khi login, copy token vào variable
   - Dùng `Authorization: Bearer {{token}}` cho các requests cần auth

---

## Test bằng curl (Windows):

### Login:
```bash
curl -X POST "http://localhost:5153/api/auth/login" -H "Content-Type: application/json" -d "{\"email\":\"nghiadaucau1@gmail.com\",\"password\":\"nghiadaucau123@\"}"
```

### Get Subscription:
```bash
curl -X GET "http://localhost:5153/api/package-subscriptions/8" -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

### Get Time Slots:
```bash
curl -X GET "http://localhost:5153/api/time-slots/available?serviceCenterId=2&date=2025-10-16"
```

### Create Appointment:
```bash
curl -X POST "http://localhost:5153/api/appointments" -H "Authorization: Bearer YOUR_TOKEN_HERE" -H "Content-Type: application/json" -d "{\"customerId\":1014,\"vehicleId\":7,\"serviceCenterId\":2,\"slotId\":201,\"subscriptionId\":8,\"appointmentDate\":\"2025-10-16\",\"customerNotes\":\"Test booking with VIP subscription\",\"preferredServices\":[]}"
```

---

## Troubleshooting:

### Lỗi 401 Unauthorized:
- Token expired → Login lại
- Token sai format → Kiểm tra "Bearer <token>"

### Lỗi 400 Bad Request:
- Check validation errors trong response
- Đảm bảo tất cả field bắt buộc đã điền đúng

### Lỗi "Subscription không thuộc về khách hàng":
- Kiểm tra subscription ID 8 có đúng customerId 1014 không

### Lỗi "Slot không available":
- Chọn date trong tương lai (ít nhất +1 ngày)
- Kiểm tra slot có tồn tại và available không

---

Chúc bạn test thành công! 🚀
