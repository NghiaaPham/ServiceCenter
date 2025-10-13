# 📱 HƯỚNG DẪN TÍCH HỢP API - FRONTEND GUIDE

## 🎯 Tổng quan
Tài liệu này hướng dẫn chi tiết các API endpoints và luồng xử lý để frontend tích hợp với EV Service Center Backend.

**Base URL**: `http://localhost:5153/api`
**Swagger UI**: `http://localhost:5153/swagger`

---

## 📋 MỤC LỤC

1. [Authentication Flow](#1-authentication-flow)
2. [Customer Profile Management](#2-customer-profile-management)
3. [Vehicle Management](#3-vehicle-management)
4. [View Available Services & Packages](#4-view-available-services--packages)
5. [Package Subscription Purchase](#5-package-subscription-purchase)
6. [Smart Appointment Booking](#6-smart-appointment-booking)
7. [View My Appointments](#7-view-my-appointments)
8. [Error Handling](#8-error-handling)

---

## 1. AUTHENTICATION FLOW

### 1.1. Đăng ký tài khoản mới (Customer Registration)

**Endpoint**: `POST /api/customer-registration/register`

**Request Body**:
```json
{
  "email": "customer@example.com",
  "password": "Password123!",
  "confirmPassword": "Password123!",
  "fullName": "Nguyễn Văn A",
  "phoneNumber": "0901234567",
  "address": "123 Đường ABC, Quận 1, TP.HCM",
  "dateOfBirth": "1990-01-15"
}
```

**Response Success (201)**:
```json
{
  "success": true,
  "message": "Đăng ký thành công. Vui lòng kiểm tra email để xác thực tài khoản.",
  "data": {
    "userId": 123,
    "customerId": 456,
    "email": "customer@example.com",
    "fullName": "Nguyễn Văn A",
    "verificationRequired": true
  }
}
```

**Response Error (400)**:
```json
{
  "success": false,
  "message": "Email đã được sử dụng",
  "errors": ["Email này đã tồn tại trong hệ thống"]
}
```

---

### 1.2. Xác thực Email

**Endpoint**: `POST /api/verification/verify-email`

**Request Body**:
```json
{
  "email": "customer@example.com",
  "token": "ABC123XYZ789"
}
```

**Response Success (200)**:
```json
{
  "success": true,
  "message": "Xác thực email thành công"
}
```

---

### 1.3. Đăng nhập (Login)

**Endpoint**: `POST /api/auth/login`

**Request Body**:
```json
{
  "email": "customer@example.com",
  "password": "Password123!"
}
```

**Response Success (200)**:
```json
{
  "success": true,
  "message": "Đăng nhập thành công",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "def50200abc...",
    "expiresIn": 3600,
    "user": {
      "userId": 123,
      "email": "customer@example.com",
      "fullName": "Nguyễn Văn A",
      "role": "Customer",
      "customerId": 456
    }
  }
}
```

**Lưu token vào localStorage/sessionStorage**:
```javascript
// Frontend code example
localStorage.setItem('accessToken', response.data.token);
localStorage.setItem('refreshToken', response.data.refreshToken);
localStorage.setItem('user', JSON.stringify(response.data.user));
```

**Response Error (401)**:
```json
{
  "success": false,
  "message": "Email hoặc mật khẩu không chính xác"
}
```

---

### 1.4. Thêm Authorization Header vào mọi request

Sau khi đăng nhập thành công, **MỌI REQUEST** tiếp theo cần có header:

```javascript
headers: {
  'Authorization': `Bearer ${localStorage.getItem('accessToken')}`,
  'Content-Type': 'application/json'
}
```

**Example với Axios**:
```javascript
const api = axios.create({
  baseURL: 'http://localhost:5153/api',
});

// Interceptor để tự động thêm token
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('accessToken');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);
```

---

## 2. CUSTOMER PROFILE MANAGEMENT

### 2.1. Xem thông tin cá nhân

**Endpoint**: `GET /api/customer/profile/me`
**Auth**: Required

**Response Success (200)**:
```json
{
  "success": true,
  "data": {
    "customerId": 456,
    "userId": 123,
    "fullName": "Nguyễn Văn A",
    "email": "customer@example.com",
    "phoneNumber": "0901234567",
    "address": "123 Đường ABC, Quận 1, TP.HCM",
    "dateOfBirth": "1990-01-15",
    "loyaltyPoints": 1500,
    "totalSpent": 5000000,
    "customerType": {
      "typeId": 1,
      "typeName": "VIP",
      "discountPercent": 10
    }
  }
}
```

---

### 2.2. Cập nhật thông tin cá nhân

**Endpoint**: `PUT /api/customer/profile/me`
**Auth**: Required

**Request Body**:
```json
{
  "fullName": "Nguyễn Văn B",
  "phoneNumber": "0907654321",
  "address": "456 Đường XYZ, Quận 2, TP.HCM"
}
```

**Response Success (200)**:
```json
{
  "success": true,
  "message": "Cập nhật thông tin thành công",
  "data": {
    "customerId": 456,
    "fullName": "Nguyễn Văn B",
    "phoneNumber": "0907654321",
    "address": "456 Đường XYZ, Quận 2, TP.HCM"
  }
}
```

---

## 3. VEHICLE MANAGEMENT

### 3.1. Xem danh sách xe của tôi

**Endpoint**: `GET /api/customer/profile/my-vehicles`
**Auth**: Required

**Response Success (200)**:
```json
{
  "success": true,
  "data": [
    {
      "vehicleId": 1,
      "licensePlate": "51A-12345",
      "carModel": {
        "modelId": 5,
        "modelName": "VinFast VF8",
        "brandName": "VinFast"
      },
      "year": 2023,
      "color": "Đỏ",
      "mileage": 15000,
      "batteryHealthPercent": 95.5,
      "lastMaintenanceDate": "2024-09-01",
      "nextMaintenanceDue": "2025-03-01"
    },
    {
      "vehicleId": 2,
      "licensePlate": "51B-67890",
      "carModel": {
        "modelId": 3,
        "modelName": "Tesla Model 3",
        "brandName": "Tesla"
      },
      "year": 2022,
      "color": "Trắng",
      "mileage": 25000,
      "batteryHealthPercent": 92.0,
      "lastMaintenanceDate": "2024-08-15",
      "nextMaintenanceDue": "2025-02-15"
    }
  ]
}
```

---

### 3.2. Thêm xe mới

**Endpoint**: `POST /api/customer/profile/my-vehicles`
**Auth**: Required

**Request Body**:
```json
{
  "licensePlate": "51C-99999",
  "modelId": 5,
  "year": 2024,
  "color": "Xanh",
  "vinNumber": "VF1234567890ABCDE",
  "purchaseDate": "2024-01-15",
  "mileage": 0
}
```

**Response Success (201)**:
```json
{
  "success": true,
  "message": "Thêm xe thành công",
  "data": {
    "vehicleId": 3,
    "licensePlate": "51C-99999",
    "carModel": {
      "modelId": 5,
      "modelName": "VinFast VF8",
      "brandName": "VinFast"
    },
    "year": 2024,
    "color": "Xanh",
    "mileage": 0
  }
}
```

---

## 4. VIEW AVAILABLE SERVICES & PACKAGES

### 4.1. Xem danh sách dịch vụ

**Endpoint**: `GET /api/maintenance-services`
**Auth**: Not required (public)

**Query Parameters**:
- `categoryId` (optional): Filter by category
- `isActive` (optional): true/false
- `searchTerm` (optional): Search by name

**Response Success (200)**:
```json
{
  "success": true,
  "data": [
    {
      "serviceId": 1,
      "serviceName": "Thay lốp xe điện",
      "categoryName": "Bảo dưỡng định kỳ",
      "description": "Thay 4 lốp xe điện chính hãng",
      "basePrice": 2000000,
      "estimatedDuration": 60,
      "isWarrantyService": false
    },
    {
      "serviceId": 2,
      "serviceName": "Kiểm tra pin",
      "categoryName": "Kiểm tra chẩn đoán",
      "description": "Kiểm tra tình trạng pin, BMS và sức khỏe pin",
      "basePrice": 500000,
      "estimatedDuration": 30,
      "isWarrantyService": true
    }
  ]
}
```

---

### 4.2. Xem giá dịch vụ theo xe

**Endpoint**: `GET /api/model-service-pricing/model/{modelId}/services`
**Auth**: Not required (public)

**Response Success (200)**:
```json
{
  "success": true,
  "data": [
    {
      "pricingId": 10,
      "serviceId": 1,
      "serviceName": "Thay lốp xe điện",
      "modelId": 5,
      "modelName": "VinFast VF8",
      "price": 2200000,
      "estimatedDuration": 60
    },
    {
      "pricingId": 11,
      "serviceId": 2,
      "serviceName": "Kiểm tra pin",
      "modelId": 5,
      "modelName": "VinFast VF8",
      "price": 450000,
      "estimatedDuration": 30
    }
  ]
}
```

---

### 4.3. Xem danh sách gói bảo dưỡng (Maintenance Packages)

**Endpoint**: `GET /api/maintenance-packages`
**Auth**: Not required (public)

**Response Success (200)**:
```json
{
  "success": true,
  "data": [
    {
      "packageId": 1,
      "packageName": "Gói Bảo Dưỡng Cơ Bản",
      "description": "3 lần bảo dưỡng định kỳ trong 6 tháng",
      "totalServices": 3,
      "validityPeriod": 180,
      "price": 3000000,
      "discountPercent": 10,
      "isActive": true,
      "services": [
        {
          "serviceId": 2,
          "serviceName": "Kiểm tra pin",
          "quantity": 3
        },
        {
          "serviceId": 5,
          "serviceName": "Thay dầu phanh",
          "quantity": 3
        }
      ]
    },
    {
      "packageId": 2,
      "packageName": "Gói Bảo Dưỡng VIP",
      "description": "6 lần bảo dưỡng cao cấp trong 1 năm",
      "totalServices": 6,
      "validityPeriod": 365,
      "price": 8000000,
      "discountPercent": 20,
      "isActive": true,
      "services": [
        {
          "serviceId": 1,
          "serviceName": "Thay lốp xe điện",
          "quantity": 1
        },
        {
          "serviceId": 2,
          "serviceName": "Kiểm tra pin",
          "quantity": 6
        }
      ]
    }
  ]
}
```

---

## 5. PACKAGE SUBSCRIPTION PURCHASE

### 5.1. Xem gói đăng ký của tôi

**Endpoint**: `GET /api/package-subscriptions/my-subscriptions`
**Auth**: Required

**Query Parameters**:
- `statusFilter` (optional): Active | Expired | Cancelled

**Response Success (200)**:
```json
{
  "success": true,
  "data": [
    {
      "subscriptionId": 10,
      "packageId": 2,
      "packageName": "Gói Bảo Dưỡng VIP",
      "vehicleId": 1,
      "vehiclePlate": "51A-12345",
      "startDate": "2024-01-01",
      "endDate": "2025-01-01",
      "totalServices": 6,
      "usedServices": 2,
      "remainingServices": 4,
      "status": "Active",
      "purchasePrice": 8000000,
      "discountPercent": 20
    }
  ]
}
```

---

### 5.2. Xem chi tiết usage của subscription

**Endpoint**: `GET /api/package-subscriptions/{subscriptionId}/usage`
**Auth**: Required

**Response Success (200)**:
```json
{
  "success": true,
  "data": {
    "subscriptionId": 10,
    "packageName": "Gói Bảo Dưỡng VIP",
    "totalServices": 6,
    "usedServices": 2,
    "remainingServices": 4,
    "usageHistory": [
      {
        "usageId": 1,
        "appointmentId": 100,
        "serviceName": "Kiểm tra pin",
        "usedDate": "2024-02-15",
        "quantityUsed": 1
      },
      {
        "usageId": 2,
        "appointmentId": 105,
        "serviceName": "Kiểm tra pin",
        "usedDate": "2024-05-20",
        "quantityUsed": 1
      }
    ]
  }
}
```

---

### 5.3. Mua gói đăng ký mới

**Endpoint**: `POST /api/package-subscriptions/purchase`
**Auth**: Required

**Request Body**:
```json
{
  "packageId": 2,
  "vehicleId": 1,
  "paymentMethodId": 1,
  "autoRenew": false
}
```

**Response Success (201)**:
```json
{
  "success": true,
  "message": "Mua gói đăng ký thành công",
  "data": {
    "subscriptionId": 15,
    "packageName": "Gói Bảo Dưỡng VIP",
    "startDate": "2024-10-10",
    "endDate": "2025-10-10",
    "totalServices": 6,
    "remainingServices": 6,
    "status": "Active",
    "paymentRequired": true,
    "paymentAmount": 8000000,
    "paymentUrl": "https://payment-gateway.com/pay?token=xyz123"
  }
}
```

---

## 6. SMART APPOINTMENT BOOKING

### 6.1. Xem thời gian trống (Available Time Slots)

**Endpoint**: `GET /api/timeslots/available`
**Auth**: Not required (public)

**Query Parameters**:
- `date` (required): 2024-10-15
- `serviceCenterId` (optional): 1
- `duration` (optional): 60

**Response Success (200)**:
```json
{
  "success": true,
  "data": {
    "date": "2024-10-15",
    "availableSlots": [
      {
        "slotId": 1,
        "startTime": "08:00",
        "endTime": "09:00",
        "available": true,
        "remainingCapacity": 3
      },
      {
        "slotId": 2,
        "startTime": "09:00",
        "endTime": "10:00",
        "available": true,
        "remainingCapacity": 2
      },
      {
        "slotId": 3,
        "startTime": "10:00",
        "endTime": "11:00",
        "available": false,
        "remainingCapacity": 0
      }
    ]
  }
}
```

---

### 6.2. Tạo Appointment (Smart Subscription Auto-Apply)

**Endpoint**: `POST /api/appointments`
**Auth**: Required

**Request Body**:
```json
{
  "serviceCenterId": 1,
  "vehicleId": 1,
  "appointmentDate": "2024-10-15",
  "slotId": 1,
  "services": [
    {
      "serviceId": 2,
      "quantity": 1,
      "notes": "Kiểm tra kỹ pin trước khi đi xa"
    }
  ],
  "notes": "Tôi sẽ đến đúng giờ",
  "preferredTechnicianId": null
}
```

**Response Success - Với Subscription (201)**:
```json
{
  "success": true,
  "message": "Đặt lịch thành công. Dịch vụ được áp dụng từ gói đăng ký.",
  "data": {
    "appointmentId": 200,
    "appointmentCode": "APT-2024-200",
    "status": "Pending",
    "appointmentDate": "2024-10-15",
    "slotTime": "08:00 - 09:00",
    "serviceCenter": {
      "centerId": 1,
      "centerName": "EV Service Center - Quận 1"
    },
    "vehicle": {
      "vehicleId": 1,
      "licensePlate": "51A-12345",
      "modelName": "VinFast VF8"
    },
    "services": [
      {
        "appointmentServiceId": 500,
        "serviceId": 2,
        "serviceName": "Kiểm tra pin",
        "serviceSource": "Subscription",
        "subscriptionId": 10,
        "price": 0,
        "estimatedDuration": 30
      }
    ],
    "totalAmount": 0,
    "subscriptionApplied": true,
    "subscriptionDetails": {
      "subscriptionId": 10,
      "packageName": "Gói Bảo Dưỡng VIP",
      "remainingServicesAfter": 3
    }
  }
}
```

**Response Success - Không có Subscription (201)**:
```json
{
  "success": true,
  "message": "Đặt lịch thành công. Bạn cần thanh toán khi đến.",
  "data": {
    "appointmentId": 201,
    "appointmentCode": "APT-2024-201",
    "status": "Pending",
    "appointmentDate": "2024-10-15",
    "slotTime": "09:00 - 10:00",
    "serviceCenter": {
      "centerId": 1,
      "centerName": "EV Service Center - Quận 1"
    },
    "vehicle": {
      "vehicleId": 2,
      "licensePlate": "51B-67890",
      "modelName": "Tesla Model 3"
    },
    "services": [
      {
        "appointmentServiceId": 501,
        "serviceId": 1,
        "serviceName": "Thay lốp xe điện",
        "serviceSource": "Extra",
        "subscriptionId": null,
        "price": 2000000,
        "estimatedDuration": 60
      }
    ],
    "totalAmount": 2000000,
    "subscriptionApplied": false,
    "paymentRequired": true
  }
}
```

**Response Error - Không đủ dịch vụ trong gói (400)**:
```json
{
  "success": false,
  "message": "Gói đăng ký không đủ số lượng dịch vụ",
  "data": {
    "availableInSubscription": 1,
    "requested": 2,
    "suggestionMessage": "Bạn có 1 dịch vụ trong gói. Dịch vụ còn lại sẽ tính phí 500,000 VNĐ."
  }
}
```

---

### 6.3. Flow xử lý Smart Subscription trong Frontend

```javascript
// Example Frontend Logic
async function createAppointment(appointmentData) {
  try {
    // 1. Gọi API tạo appointment
    const response = await api.post('/api/appointments', appointmentData);

    // 2. Kiểm tra có subscription được apply không
    if (response.data.subscriptionApplied) {
      // Trường hợp: Dịch vụ được trừ từ gói
      showSuccessMessage(
        `Đặt lịch thành công! Dịch vụ được áp dụng từ ${response.data.subscriptionDetails.packageName}. ` +
        `Còn lại ${response.data.subscriptionDetails.remainingServicesAfter} dịch vụ.`
      );

      // Không cần thanh toán
      redirectTo(`/appointments/${response.data.appointmentId}`);

    } else {
      // Trường hợp: Cần thanh toán
      showSuccessMessage(
        `Đặt lịch thành công! Vui lòng thanh toán ${formatCurrency(response.data.totalAmount)} VNĐ khi đến.`
      );

      // Hiển thị thông tin thanh toán
      if (response.data.paymentRequired) {
        showPaymentOptions(response.data);
      }

      redirectTo(`/appointments/${response.data.appointmentId}`);
    }

  } catch (error) {
    // Xử lý lỗi
    if (error.response?.status === 400) {
      showErrorMessage(error.response.data.message);
    } else {
      showErrorMessage('Đặt lịch thất bại. Vui lòng thử lại.');
    }
  }
}
```

---

## 7. VIEW MY APPOINTMENTS

### 7.1. Xem danh sách lịch hẹn của tôi

**Endpoint**: `GET /api/appointments/my-appointments`
**Auth**: Required

**Query Parameters**:
- `status` (optional): Pending | Confirmed | Completed | Cancelled
- `fromDate` (optional): 2024-01-01
- `toDate` (optional): 2024-12-31

**Response Success (200)**:
```json
{
  "success": true,
  "data": [
    {
      "appointmentId": 200,
      "appointmentCode": "APT-2024-200",
      "status": "Confirmed",
      "appointmentDate": "2024-10-15",
      "slotTime": "08:00 - 09:00",
      "serviceCenter": {
        "centerId": 1,
        "centerName": "EV Service Center - Quận 1",
        "address": "123 Đường ABC, Quận 1",
        "phoneNumber": "028-1234-5678"
      },
      "vehicle": {
        "vehicleId": 1,
        "licensePlate": "51A-12345",
        "modelName": "VinFast VF8"
      },
      "services": [
        {
          "serviceName": "Kiểm tra pin",
          "serviceSource": "Subscription",
          "price": 0
        }
      ],
      "totalAmount": 0,
      "subscriptionUsed": true,
      "createdDate": "2024-10-01T10:30:00"
    },
    {
      "appointmentId": 201,
      "appointmentCode": "APT-2024-201",
      "status": "Pending",
      "appointmentDate": "2024-10-20",
      "slotTime": "14:00 - 16:00",
      "serviceCenter": {
        "centerId": 1,
        "centerName": "EV Service Center - Quận 1",
        "address": "123 Đường ABC, Quận 1",
        "phoneNumber": "028-1234-5678"
      },
      "vehicle": {
        "vehicleId": 2,
        "licensePlate": "51B-67890",
        "modelName": "Tesla Model 3"
      },
      "services": [
        {
          "serviceName": "Thay lốp xe điện",
          "serviceSource": "Extra",
          "price": 2000000
        }
      ],
      "totalAmount": 2000000,
      "subscriptionUsed": false,
      "paymentStatus": "Unpaid",
      "createdDate": "2024-10-05T15:45:00"
    }
  ]
}
```

---

### 7.2. Xem chi tiết lịch hẹn

**Endpoint**: `GET /api/appointments/{appointmentId}`
**Auth**: Required

**Response Success (200)**:
```json
{
  "success": true,
  "data": {
    "appointmentId": 200,
    "appointmentCode": "APT-2024-200",
    "status": "Confirmed",
    "appointmentDate": "2024-10-15",
    "slotTime": "08:00 - 09:00",
    "estimatedCompletionTime": "09:00",
    "serviceCenter": {
      "centerId": 1,
      "centerName": "EV Service Center - Quận 1",
      "address": "123 Đường ABC, Quận 1, TP.HCM",
      "phoneNumber": "028-1234-5678",
      "email": "q1@evservicecenter.com"
    },
    "customer": {
      "customerId": 456,
      "fullName": "Nguyễn Văn A",
      "phoneNumber": "0901234567",
      "email": "customer@example.com"
    },
    "vehicle": {
      "vehicleId": 1,
      "licensePlate": "51A-12345",
      "modelName": "VinFast VF8",
      "brandName": "VinFast",
      "year": 2023,
      "color": "Đỏ",
      "mileage": 15000
    },
    "services": [
      {
        "appointmentServiceId": 500,
        "serviceId": 2,
        "serviceName": "Kiểm tra pin",
        "serviceSource": "Subscription",
        "subscriptionId": 10,
        "price": 0,
        "originalPrice": 450000,
        "estimatedDuration": 30,
        "technician": null,
        "status": "Pending"
      }
    ],
    "subscriptionInfo": {
      "subscriptionId": 10,
      "packageName": "Gói Bảo Dưỡng VIP",
      "remainingServices": 3
    },
    "totalAmount": 0,
    "paidAmount": 0,
    "remainingAmount": 0,
    "notes": "Tôi sẽ đến đúng giờ",
    "createdDate": "2024-10-01T10:30:00",
    "updatedDate": "2024-10-02T09:15:00"
  }
}
```

---

### 7.3. Hủy lịch hẹn

**Endpoint**: `POST /api/appointments/{appointmentId}/cancel`
**Auth**: Required

**Request Body**:
```json
{
  "reason": "Tôi có việc đột xuất, xin lỗi"
}
```

**Response Success (200)**:
```json
{
  "success": true,
  "message": "Hủy lịch hẹn thành công",
  "data": {
    "appointmentId": 200,
    "status": "Cancelled",
    "subscriptionRestored": true,
    "restoredServices": 1,
    "message": "Dịch vụ đã được hoàn trả vào gói đăng ký của bạn"
  }
}
```

---

## 8. ERROR HANDLING

### 8.1. Error Response Format

Mọi lỗi đều trả về format chuẩn:

```json
{
  "success": false,
  "message": "Mô tả lỗi chính",
  "errors": [
    "Chi tiết lỗi 1",
    "Chi tiết lỗi 2"
  ],
  "statusCode": 400
}
```

---

### 8.2. Common HTTP Status Codes

| Status Code | Ý nghĩa | Xử lý Frontend |
|-------------|---------|----------------|
| 200 | Success | Hiển thị kết quả |
| 201 | Created | Redirect hoặc hiển thị success |
| 400 | Bad Request | Hiển thị lỗi validation |
| 401 | Unauthorized | Redirect đến login, clear token |
| 403 | Forbidden | Hiển thị "Bạn không có quyền" |
| 404 | Not Found | Hiển thị "Không tìm thấy" |
| 409 | Conflict | Hiển thị lỗi xung đột (ví dụ: email đã tồn tại) |
| 500 | Server Error | Hiển thị "Lỗi hệ thống, vui lòng thử lại" |

---

### 8.3. Example Error Handler (Axios)

```javascript
// Axios interceptor để xử lý lỗi tập trung
api.interceptors.response.use(
  (response) => response,
  (error) => {
    const { status, data } = error.response || {};

    switch (status) {
      case 401:
        // Token hết hạn hoặc không hợp lệ
        localStorage.removeItem('accessToken');
        localStorage.removeItem('user');
        window.location.href = '/login';
        break;

      case 403:
        showErrorMessage('Bạn không có quyền thực hiện thao tác này');
        break;

      case 404:
        showErrorMessage('Không tìm thấy dữ liệu');
        break;

      case 400:
        // Validation errors
        if (data.errors && Array.isArray(data.errors)) {
          showErrorMessage(data.errors.join('\n'));
        } else {
          showErrorMessage(data.message || 'Dữ liệu không hợp lệ');
        }
        break;

      case 500:
      default:
        showErrorMessage('Lỗi hệ thống. Vui lòng thử lại sau.');
        break;
    }

    return Promise.reject(error);
  }
);
```

---

## 9. COMPLETE FLOW DIAGRAM

```
┌─────────────────────────────────────────────────────────────────┐
│                    CUSTOMER JOURNEY FLOW                         │
└─────────────────────────────────────────────────────────────────┘

1. ĐĂNG KÝ / ĐĂNG NHẬP
   ├─→ POST /api/customer-registration/register
   ├─→ POST /api/verification/verify-email
   └─→ POST /api/auth/login → Nhận JWT Token

2. XEM PROFILE & VEHICLES
   ├─→ GET /api/customer/profile/me
   ├─→ GET /api/customer/profile/my-vehicles
   └─→ POST /api/customer/profile/my-vehicles (Thêm xe mới)

3. XEM SUBSCRIPTIONS
   ├─→ GET /api/package-subscriptions/my-subscriptions
   ├─→ GET /api/package-subscriptions/{id}/usage
   └─→ GET /api/package-subscriptions/vehicle/{vehicleId}/active

4. MUA GÓI ĐĂNG KÝ (Optional)
   ├─→ GET /api/maintenance-packages (Xem các gói)
   └─→ POST /api/package-subscriptions/purchase

5. ĐẶT LỊCH HẸN (SMART BOOKING)
   ├─→ GET /api/maintenance-services (Xem dịch vụ)
   ├─→ GET /api/timeslots/available (Xem giờ trống)
   └─→ POST /api/appointments
       ├─→ Backend tự động check subscription
       ├─→ Nếu có subscription → ServiceSource = "Subscription", Price = 0
       └─→ Nếu không có → ServiceSource = "Extra", Price = giá dịch vụ

6. XEM & QUẢN LÝ LỊCH HẸN
   ├─→ GET /api/appointments/my-appointments
   ├─→ GET /api/appointments/{id}
   ├─→ POST /api/appointments/{id}/cancel
   └─→ POST /api/appointments/{id}/reschedule
```

---

## 10. FRONTEND IMPLEMENTATION CHECKLIST

### ✅ Authentication Module
- [ ] Login page với form validation
- [ ] Registration page với email verification
- [ ] Forgot password flow
- [ ] JWT token storage & refresh logic
- [ ] Auto-redirect khi token hết hạn

### ✅ Profile Module
- [ ] View profile page
- [ ] Edit profile form
- [ ] Vehicle list page
- [ ] Add vehicle form

### ✅ Subscription Module
- [ ] View my subscriptions page
- [ ] Subscription usage details
- [ ] Buy package page
- [ ] Package comparison table

### ✅ Booking Module
- [ ] Service selection page
- [ ] Time slot picker (calendar view)
- [ ] Booking form với smart subscription detection
- [ ] Booking confirmation page
- [ ] Show subscription info if applied

### ✅ Appointment Management
- [ ] My appointments list
- [ ] Appointment details page
- [ ] Cancel appointment modal
- [ ] Reschedule appointment flow

### ✅ UI/UX Features
- [ ] Loading spinners
- [ ] Success/Error toast notifications
- [ ] Confirmation modals
- [ ] Responsive design (mobile-first)
- [ ] Accessibility (WCAG 2.1)

---

## 11. TESTING SCENARIOS

### Scenario 1: Khách hàng mới đặt lịch (Không có subscription)
1. Register → Verify email → Login
2. Add vehicle
3. Browse services
4. Book appointment → System shows: Price = 500,000 VNĐ, ServiceSource = "Extra"
5. Payment required

### Scenario 2: Khách hàng có subscription đặt lịch
1. Login (đã có subscription active)
2. Browse services
3. Book appointment với service có trong gói
4. System auto-apply subscription → Price = 0, ServiceSource = "Subscription"
5. Success message: "Dịch vụ được áp dụng từ gói. Còn X dịch vụ."

### Scenario 3: Subscription hết hạn
1. Login (subscription expired)
2. Book appointment
3. System shows: Price = full price, ServiceSource = "Extra"
4. Suggest buying new package

---

## 📞 SUPPORT & DOCUMENTATION

- **Swagger UI**: `http://localhost:5153/swagger`
- **Postman Collection**: Contact backend team
- **API Issues**: Create GitHub issue
- **Frontend Examples**: Check `/frontend-examples` folder

---

**Last Updated**: 2024-10-10
**API Version**: v1
**Backend Framework**: ASP.NET Core 9.0
