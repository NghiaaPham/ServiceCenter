# 📋 Frontend API Integration Checklist - EV Service Center

**Last Updated:** 2025-10-10
**API Base URL:** `http://localhost:5153` (Dev) | `https://api.evservicecenter.com` (Prod)
**Swagger:** `http://localhost:5153/swagger`

---

## 📊 Tổng Quan Tiến Độ

| Module | Total APIs | Implemented | Pending | Priority |
|--------|-----------|-------------|---------|----------|
| **Authentication** | 11 | ⏳ 0/11 | 11 | 🔴 Critical |
| **Customer Profile** | 6 | ⏳ 0/6 | 6 | 🔴 High |
| **Appointments** | 9 | ⏳ 0/9 | 9 | 🔴 High |
| **Smart Maintenance Reminder** | 5 | ✅ 5/5 | 0 | 🟢 Complete |
| **Package Subscriptions** | 6 | ⏳ 0/6 | 6 | 🟡 Medium |
| **Vehicle Management** | 4 | ⏳ 0/4 | 4 | 🟡 Medium |
| **Lookups/Master Data** | 3 | ⏳ 0/3 | 3 | 🟢 Low |
| **Account Management** | 4 | ⏳ 0/4 | 4 | 🟡 Medium |
| **TOTAL** | **48** | **5/48** | **43** | **10.4%** |

---

## 🔐 Module 1: Authentication & Authorization

### 📌 Priority: 🔴 CRITICAL (Implement First)

### APIs Checklist

| # | Endpoint | Method | Status | Priority | Notes |
|---|----------|--------|--------|----------|-------|
| 1.1 | `/api/auth/register` | POST | ⏳ Pending | 🔴 Critical | Đăng ký khách hàng mới |
| 1.2 | `/api/auth/login` | POST | ⏳ Pending | 🔴 Critical | Đăng nhập, nhận JWT token |
| 1.3 | `/api/auth/logout` | POST | ⏳ Pending | 🟡 Medium | Đăng xuất |
| 1.4 | `/api/auth/profile` | GET | ⏳ Pending | 🔴 High | Xem thông tin user đăng nhập |
| 1.5 | `/api/auth/change-password` | PUT | ⏳ Pending | 🟡 Medium | Đổi mật khẩu |
| 1.6 | `/api/auth/external/google` | POST | ⏳ Pending | 🟢 Low | Đăng nhập Google |
| 1.7 | `/api/auth/external/facebook` | POST | ⏳ Pending | 🟢 Low | Đăng nhập Facebook |
| 1.8 | `/api/account/forgot-password` | POST | ⏳ Pending | 🟡 Medium | Quên mật khẩu |
| 1.9 | `/api/account/reset-password` | POST | ⏳ Pending | 🟡 Medium | Reset mật khẩu |
| 1.10 | `/api/verification/verify-email` | POST | ⏳ Pending | 🟡 Medium | Xác thực email |
| 1.11 | `/api/verification/resend-verification` | POST | ⏳ Pending | 🟢 Low | Gửi lại email xác thực |

### 🔑 Implementation Guide

#### 1.1 Register Customer
```typescript
POST /api/auth/register
Content-Type: application/json

{
  "email": "customer@example.com",
  "password": "Password123!",
  "fullName": "Nguyen Van A",
  "phoneNumber": "0912345678"
}

Response 200:
{
  "success": true,
  "message": "Đăng ký thành công",
  "data": {
    "userId": 1,
    "email": "customer@example.com",
    "requireEmailVerification": true
  }
}
```

#### 1.2 Login (MOST IMPORTANT)
```typescript
POST /api/auth/login
Content-Type: application/json

{
  "email": "customer@example.com",
  "password": "Password123!"
}

Response 200:
{
  "success": true,
  "message": "Đăng nhập thành công",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresAt": "2025-10-11T17:00:00Z",
    "user": {
      "userId": 1,
      "email": "customer@example.com",
      "fullName": "Nguyen Van A",
      "role": "Customer"
    }
  }
}
```

**Frontend Action:**
1. Save token to `localStorage.setItem('authToken', data.token)`
2. Set axios default header: `axios.defaults.headers.common['Authorization'] = 'Bearer ' + token`
3. Redirect to dashboard
4. Setup token refresh logic

---

## 👤 Module 2: Customer Profile Management

### 📌 Priority: 🔴 HIGH

### APIs Checklist

| # | Endpoint | Method | Status | Priority | Notes |
|---|----------|--------|--------|----------|-------|
| 2.1 | `/api/customer/profile/me` | GET | ⏳ Pending | 🔴 High | Xem thông tin profile |
| 2.2 | `/api/customer/profile/me` | PUT | ⏳ Pending | 🔴 High | Cập nhật profile |
| 2.3 | `/api/customer/profile/my-vehicles` | GET | ⏳ Pending | 🔴 High | Danh sách xe của tôi |
| 2.4 | `/api/customer/profile/my-vehicles` | POST | ⏳ Pending | 🟡 Medium | Đăng ký xe mới |
| 2.5 | `/api/customer/profile/my-vehicles/{id}` | GET | ⏳ Pending | 🟡 Medium | Chi tiết 1 xe |
| 2.6 | `/api/customer/profile/my-vehicles/{id}` | DELETE | ⏳ Pending | 🟢 Low | Xóa xe |

### 🔑 Implementation Guide

#### 2.1 Get My Profile
```typescript
GET /api/customer/profile/me
Authorization: Bearer {token}

Response 200:
{
  "success": true,
  "message": "Lấy thông tin thành công",
  "data": {
    "customerId": 1,
    "customerCode": "KH000001",
    "fullName": "Nguyen Van A",
    "email": "customer@example.com",
    "phoneNumber": "0912345678",
    "address": "123 Nguyen Hue, Q1, HCM",
    "dateOfBirth": "1990-01-01",
    "gender": "Male",
    "loyaltyPoints": 1500,
    "typeId": 1,
    "typeName": "Regular"
  }
}
```

**Frontend Usage:**
- Display in "Tài khoản của tôi" page
- Show loyalty points in header
- Pre-fill edit profile form

#### 2.3 Get My Vehicles
```typescript
GET /api/customer/profile/my-vehicles
Authorization: Bearer {token}

Response 200:
{
  "success": true,
  "message": "Tìm thấy 2 xe",
  "data": [
    {
      "vehicleId": 1,
      "licensePlate": "51A-12345",
      "modelName": "VF8",
      "brandName": "VinFast",
      "color": "Đỏ",
      "mileage": 15000,
      "purchaseDate": "2023-01-15",
      "nextMaintenanceDate": "2025-11-01"
    },
    {
      "vehicleId": 2,
      "licensePlate": "51B-67890",
      "modelName": "Model 3",
      "brandName": "Tesla",
      "color": "Trắng",
      "mileage": 20000,
      "purchaseDate": "2022-06-10",
      "nextMaintenanceDate": "2025-10-15"
    }
  ]
}
```

**Frontend Usage:**
- Display vehicle list in profile
- Use for appointment booking (select vehicle)
- Link to maintenance status

---

## 📅 Module 3: Appointment Management

### 📌 Priority: 🔴 HIGH

### APIs Checklist

| # | Endpoint | Method | Status | Priority | Notes |
|---|----------|--------|--------|----------|-------|
| 3.1 | `/api/appointments` | POST | ⏳ Pending | 🔴 Critical | Tạo lịch hẹn mới |
| 3.2 | `/api/appointments/{id}` | GET | ⏳ Pending | 🔴 High | Xem chi tiết lịch hẹn |
| 3.3 | `/api/appointments/{id}` | PUT | ⏳ Pending | 🟡 Medium | Cập nhật lịch hẹn |
| 3.4 | `/api/appointments/{id}` | DELETE | ⏳ Pending | 🟢 Low | Xóa lịch hẹn |
| 3.5 | `/api/appointments/my-appointments` | GET | ⏳ Pending | 🔴 High | Danh sách lịch hẹn của tôi |
| 3.6 | `/api/appointments/my-appointments/upcoming` | GET | ⏳ Pending | 🔴 High | Lịch hẹn sắp tới |
| 3.7 | `/api/appointments/{id}/reschedule` | POST | ⏳ Pending | 🟡 Medium | Dời lịch hẹn |
| 3.8 | `/api/appointments/{id}/cancel` | POST | ⏳ Pending | 🟡 Medium | Hủy lịch hẹn |
| 3.9 | `/api/appointments/by-code/{code}` | GET | ⏳ Pending | 🟡 Medium | Tra cứu bằng mã |

### 🔑 Implementation Guide

#### 3.1 Create Appointment (CRITICAL)
```typescript
POST /api/appointments
Authorization: Bearer {token}
Content-Type: application/json

{
  "vehicleId": 1,
  "slotId": 10,
  "appointmentDate": "2025-10-15",
  "serviceIds": [1, 2, 3],
  "notes": "Xe bị rung lắc khi phanh",
  "packageSubscriptionId": null  // or package ID if using subscription
}

Response 201:
{
  "success": true,
  "message": "Đặt lịch thành công",
  "data": {
    "appointmentId": 100,
    "appointmentCode": "APT100",
    "appointmentDate": "2025-10-15",
    "slotTime": "08:00 - 10:00",
    "vehicleLicensePlate": "51A-12345",
    "serviceNames": ["Bảo dưỡng định kỳ", "Thay nhớt", "Kiểm tra phanh"],
    "totalEstimatedCost": 1500000,
    "status": "Pending"
  }
}
```

**Frontend Flow:**
1. Customer selects vehicle
2. Choose service date & time slot
3. Select services (or use package)
4. Review & confirm
5. Show appointment code
6. Send confirmation SMS/Email

#### 3.6 Get Upcoming Appointments
```typescript
GET /api/appointments/my-appointments/upcoming?limit=5
Authorization: Bearer {token}

Response 200:
{
  "success": true,
  "message": "Tìm thấy 2 lịch hẹn sắp tới",
  "data": [
    {
      "appointmentId": 100,
      "appointmentCode": "APT100",
      "appointmentDate": "2025-10-15",
      "slotTime": "08:00 - 10:00",
      "vehicleLicensePlate": "51A-12345",
      "status": "Confirmed",
      "canCancel": true,
      "canReschedule": true
    }
  ]
}
```

**Frontend Usage:**
- Dashboard widget showing upcoming appointments
- Notification badge
- Quick access to appointment details

---

## 🔧 Module 4: Smart Maintenance Reminder

### 📌 Priority: ✅ COMPLETED | Status: 🟢 Ready for Integration

### APIs Checklist

| # | Endpoint | Method | Status | Priority | Notes |
|---|----------|--------|--------|----------|-------|
| 4.1 | `/api/VehicleMaintenance/{id}/status` | GET | ✅ Done | 🔴 High | Trạng thái bảo dưỡng 1 xe |
| 4.2 | `/api/VehicleMaintenance/my-vehicles/status` | GET | ✅ Done | 🔴 High | Trạng thái tất cả xe |
| 4.3 | `/api/VehicleMaintenance/{id}/history` | GET | ✅ Done | 🟡 Medium | Lịch sử bảo dưỡng |
| 4.4 | `/api/VehicleMaintenance/{id}/mileage` | PUT | ✅ Done | 🟡 Medium | Cập nhật km |
| 4.5 | `/api/VehicleMaintenance/reminders` | GET | ✅ Done | 🔴 High | Xe cần bảo dưỡng |

### 🔑 Implementation Guide

**📘 Detailed Documentation:** `FRONTEND_INTEGRATION_GUIDE.md`
**🎨 React Component:** `VehicleMaintenanceTracker.tsx`
**📖 Technical Guide:** `SMART_MAINTENANCE_REMINDER_GUIDE.md`

#### 4.2 Get All Vehicles Maintenance Status
```typescript
GET /api/VehicleMaintenance/my-vehicles/status
Authorization: Bearer {token}

Response 200:
{
  "success": true,
  "message": "Lấy trạng thái bảo dưỡng thành công cho 2 xe",
  "data": [
    {
      "vehicleId": 1,
      "licensePlate": "51A-12345",
      "modelName": "VinFast VF8",
      "estimatedCurrentKm": 15234,
      "lastMaintenanceKm": 10000,
      "nextMaintenanceKm": 20000,
      "averageKmPerDay": 45.5,
      "remainingKm": 4766,
      "estimatedDaysUntilMaintenance": 104,
      "progressPercent": 52.34,
      "status": "Normal",
      "message": "✅ Xe của bạn vẫn trong tình trạng tốt."
    },
    {
      "vehicleId": 2,
      "licensePlate": "51B-67890",
      "estimatedCurrentKm": 19850,
      "remainingKm": 150,
      "progressPercent": 98.5,
      "status": "Urgent",
      "message": "⚠️ Xe của bạn sắp đến hạn bảo dưỡng!"
    }
  ]
}
```

**Frontend Integration:**
1. Import `VehicleMaintenanceTracker.tsx`
2. Install: `npm install echarts axios`
3. Display gauge charts for each vehicle
4. Add "Đặt Lịch Ngay" button for Urgent vehicles
5. Link to appointment booking

**Status Colors:**
- `Normal` (< 70%): Green `#4CAF50`
- `NeedAttention` (70-90%): Orange `#FFA500`
- `Urgent` (≥ 90%): Red `#FF4444`

---

## 📦 Module 5: Package Subscriptions (Smart Subscription)

### 📌 Priority: 🟡 MEDIUM

### APIs Checklist

| # | Endpoint | Method | Status | Priority | Notes |
|---|----------|--------|--------|----------|-------|
| 5.1 | `/api/package-subscriptions/my-subscriptions` | GET | ⏳ Pending | 🔴 High | Gói dịch vụ của tôi |
| 5.2 | `/api/package-subscriptions/{id}` | GET | ⏳ Pending | 🟡 Medium | Chi tiết gói |
| 5.3 | `/api/package-subscriptions/{id}/usage` | GET | ⏳ Pending | 🔴 High | Lịch sử sử dụng |
| 5.4 | `/api/package-subscriptions/purchase` | POST | ⏳ Pending | 🔴 High | Mua gói mới |
| 5.5 | `/api/package-subscriptions/{id}/cancel` | POST | ⏳ Pending | 🟡 Medium | Hủy gói |
| 5.6 | `/api/package-subscriptions/vehicle/{vehicleId}/active` | GET | ⏳ Pending | 🔴 High | Gói active của xe |

### 🔑 Implementation Guide

#### 5.1 Get My Subscriptions
```typescript
GET /api/package-subscriptions/my-subscriptions?statusFilter=Active
Authorization: Bearer {token}

Response 200:
{
  "success": true,
  "message": "Tìm thấy 1 gói đang hoạt động",
  "data": [
    {
      "subscriptionId": 1,
      "packageName": "Gói Bảo Dưỡng VIP",
      "vehicleLicensePlate": "51A-12345",
      "startDate": "2025-01-01",
      "endDate": "2025-12-31",
      "totalServices": 10,
      "usedServices": 3,
      "remainingServices": 7,
      "totalPrice": 15000000,
      "status": "Active",
      "autoRenew": true
    }
  ]
}
```

**Frontend Usage:**
- Display active packages in dashboard
- Show progress bar (used/total services)
- Highlight packages near expiry
- Allow auto-renew toggle

#### 5.3 Get Package Usage History
```typescript
GET /api/package-subscriptions/1/usage
Authorization: Bearer {token}

Response 200:
{
  "success": true,
  "data": {
    "subscriptionId": 1,
    "packageName": "Gói Bảo Dưỡng VIP",
    "totalServices": 10,
    "usedServices": 3,
    "remainingServices": 7,
    "usageHistory": [
      {
        "usageId": 1,
        "serviceName": "Bảo dưỡng định kỳ",
        "usedDate": "2025-03-15",
        "appointmentCode": "APT050",
        "quantityUsed": 1
      },
      {
        "usageId": 2,
        "serviceName": "Thay nhớt",
        "usedDate": "2025-06-20",
        "appointmentCode": "APT075",
        "quantityUsed": 1
      }
    ]
  }
}
```

---

## 🚗 Module 6: Vehicle Management (Master Data)

### 📌 Priority: 🟡 MEDIUM

### APIs Checklist

| # | Endpoint | Method | Status | Priority | Notes |
|---|----------|--------|--------|----------|-------|
| 6.1 | `/api/car-brands` | GET | ⏳ Pending | 🟡 Medium | Danh sách hãng xe |
| 6.2 | `/api/car-models/by-brand/{brandId}` | GET | ⏳ Pending | 🔴 High | Models theo hãng |
| 6.3 | `/api/maintenance-services` | GET | ⏳ Pending | 🔴 High | Danh sách dịch vụ |
| 6.4 | `/api/maintenance-packages` | GET | ⏳ Pending | 🟡 Medium | Danh sách gói |

### 🔑 Implementation Guide

#### 6.1 Get Car Brands
```typescript
GET /api/car-brands
Response 200:
{
  "success": true,
  "data": [
    { "brandId": 1, "brandName": "VinFast", "country": "Vietnam", "logoUrl": "/logos/vinfast.png" },
    { "brandId": 2, "brandName": "Tesla", "country": "USA", "logoUrl": "/logos/tesla.png" },
    { "brandId": 3, "brandName": "BYD", "country": "China", "logoUrl": "/logos/byd.png" }
  ]
}
```

**Frontend Usage:**
- Dropdown trong form đăng ký xe
- Filter trong trang quản lý xe

#### 6.2 Get Models by Brand
```typescript
GET /api/car-models/by-brand/1
Response 200:
{
  "success": true,
  "data": [
    {
      "modelId": 1,
      "modelName": "VF8",
      "brandName": "VinFast",
      "year": 2023,
      "batteryCapacity": 87.7,
      "range": 420,
      "serviceInterval": 10000
    },
    {
      "modelId": 2,
      "modelName": "VF9",
      "brandName": "VinFast",
      "year": 2023,
      "batteryCapacity": 123,
      "range": 594,
      "serviceInterval": 10000
    }
  ]
}
```

---

## 📚 Module 7: Lookups & Master Data

### 📌 Priority: 🟢 LOW

### APIs Checklist

| # | Endpoint | Method | Status | Priority | Notes |
|---|----------|--------|--------|----------|-------|
| 7.1 | `/api/lookups` | GET | ⏳ Pending | 🟡 Medium | Tất cả master data |
| 7.2 | `/api/lookups/appointment-statuses` | GET | ⏳ Pending | 🟡 Medium | Trạng thái lịch hẹn |
| 7.3 | `/api/time-slots/available` | GET | ⏳ Pending | 🔴 High | Time slots available |

---

## 🔐 Module 8: Account Management

### 📌 Priority: 🟡 MEDIUM

### APIs Checklist

| # | Endpoint | Method | Status | Priority | Notes |
|---|----------|--------|--------|----------|-------|
| 8.1 | `/api/account/forgot-password` | POST | ⏳ Pending | 🟡 Medium | Quên mật khẩu |
| 8.2 | `/api/account/reset-password` | POST | ⏳ Pending | 🟡 Medium | Reset mật khẩu |
| 8.3 | `/api/account/validate-reset-token` | GET | ⏳ Pending | 🟡 Medium | Validate token |
| 8.4 | `/api/verification/verify-email` | POST | ⏳ Pending | 🟡 Medium | Xác thực email |

---

## 🎯 Implementation Roadmap

### Phase 1: Core Features (Week 1-2) 🔴
**Goal:** Customer có thể đăng nhập và đặt lịch bảo dưỡng

- [ ] 1.1 - 1.5: Authentication (Login, Register, Profile)
- [ ] 2.1 - 2.3: Customer Profile (View, Edit, Vehicles)
- [ ] 3.1, 3.5, 3.6: Appointments (Create, List, Upcoming)
- [ ] 6.1 - 6.3: Master Data (Brands, Models, Services)
- [ ] 7.3: Time Slots

**Deliverables:**
- Login page
- Registration page
- Dashboard with profile
- Appointment booking flow
- My Appointments page

### Phase 2: Smart Features (Week 3) 🟡
**Goal:** Customer thấy được trạng thái xe và gói dịch vụ

- [ ] 4.1 - 4.5: Smart Maintenance Reminder (ALL)
- [ ] 5.1 - 5.4: Package Subscriptions
- [ ] 3.7 - 3.8: Reschedule/Cancel Appointments

**Deliverables:**
- Maintenance status dashboard with gauges
- Package subscription management
- Appointment management (edit/cancel)

### Phase 3: Additional Features (Week 4) 🟢
**Goal:** Hoàn thiện các tính năng phụ

- [ ] 2.4 - 2.6: Vehicle CRUD operations
- [ ] 8.1 - 8.4: Account Management (Password reset, Email verification)
- [ ] 1.6 - 1.7: Social Login (Google, Facebook)

**Deliverables:**
- Complete vehicle management
- Password reset flow
- Social login integration

---

## 🧪 Testing Checklist

### Unit Testing
- [ ] API client functions
- [ ] Request/Response type validation
- [ ] Error handling

### Integration Testing
- [ ] Login flow end-to-end
- [ ] Appointment booking flow
- [ ] Package purchase flow
- [ ] Maintenance status display

### E2E Testing
- [ ] User journey: Register → Login → Book Appointment
- [ ] User journey: Login → Check Maintenance → Book Service
- [ ] User journey: Login → View Subscription → Use Service

---

## 📝 API Response Standards

### Success Response
```typescript
{
  "success": true,
  "message": "Operation successful message",
  "data": { /* actual data */ }
}
```

### Error Response
```typescript
{
  "success": false,
  "message": "Error message",
  "errorCode": "ERROR_CODE",
  "errors": { /* validation errors */ }
}
```

### HTTP Status Codes
- `200 OK`: Success
- `201 Created`: Resource created
- `400 Bad Request`: Validation error
- `401 Unauthorized`: Not authenticated
- `403 Forbidden`: Not authorized
- `404 Not Found`: Resource not found
- `500 Internal Server Error`: Server error

---

## 🔧 Common Headers

```typescript
// All authenticated requests
{
  "Authorization": "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "Content-Type": "application/json"
}
```

---

## 📊 Dependencies Map

```
Authentication (Login)
  ↓
Customer Profile
  ↓
Vehicle List → Smart Maintenance → Appointment Booking
              ↓
         Package Subscription → Service Usage
```

**Critical Path:**
1. Implement Authentication first
2. Then Customer Profile
3. Then Vehicle Management
4. Then Appointment Booking
5. Finally Smart Features (Maintenance, Packages)

---

## 🚀 Quick Start for Frontend Team

### Step 1: Setup
```bash
npm install axios
npm install echarts  # for maintenance gauges
```

### Step 2: Create API Client
```typescript
// src/services/api.ts
import axios from 'axios';

const apiClient = axios.create({
  baseURL: process.env.REACT_APP_API_URL || 'http://localhost:5153',
  headers: {
    'Content-Type': 'application/json',
  },
});

apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('authToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export default apiClient;
```

### Step 3: Create Service Layer
```typescript
// src/services/authService.ts
import apiClient from './api';

export const login = async (email: string, password: string) => {
  const response = await apiClient.post('/api/auth/login', {
    email,
    password,
  });
  return response.data;
};

export const getProfile = async () => {
  const response = await apiClient.get('/api/customer/profile/me');
  return response.data;
};
```

### Step 4: Use in Components
```typescript
// src/pages/Login.tsx
import { login } from '../services/authService';

const handleLogin = async () => {
  try {
    const result = await login(email, password);
    localStorage.setItem('authToken', result.data.token);
    navigate('/dashboard');
  } catch (error) {
    alert('Login failed');
  }
};
```

---

## 📞 Support & Resources

- **Swagger UI**: `http://localhost:5153/swagger` - Interactive API testing
- **Postman Collection**: Available in project root
- **Backend Team**: backend@evservicecenter.com
- **Slack**: #api-support

---

## 📈 Progress Tracking

### Weekly Goals

**Week 1:**
- [ ] Complete Authentication module (100%)
- [ ] Complete Customer Profile module (100%)
- [ ] Start Appointment module (50%)

**Week 2:**
- [ ] Complete Appointment module (100%)
- [ ] Complete Master Data (100%)
- [ ] Start Smart Maintenance (50%)

**Week 3:**
- [ ] Complete Smart Maintenance (100%)
- [ ] Complete Package Subscriptions (100%)

**Week 4:**
- [ ] Complete remaining features
- [ ] Testing & Bug fixes
- [ ] Production deployment

---

**Generated:** 2025-10-10
**Version:** 1.0.0
**Maintained by:** Backend Team
