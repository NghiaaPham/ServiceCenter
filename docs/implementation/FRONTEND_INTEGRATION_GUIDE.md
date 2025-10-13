# Hướng Dẫn Kết Nối API cho Frontend - Smart Maintenance Reminder

## 🎯 Mục tiêu

Tài liệu này hướng dẫn frontend developer tích hợp tính năng **Smart Maintenance Reminder** vào ứng dụng React/Vue/Angular.

---

## 📋 Danh Sách API Endpoints



### Authentication
Tất cả API yêu cầu JWT Token trong header:
```
Authorization: Bearer YOUR_JWT_TOKEN
```

---

## 🔥 API Endpoints Chi Tiết

### 1. Lấy Trạng Thái Bảo Dưỡng Của 1 Xe

**Endpoint:**
```
GET /api/VehicleMaintenance/{vehicleId}/status
```

**Parameters:**
- `vehicleId` (path, required): ID của xe cần xem trạng thái

**Headers:**
```
Authorization: Bearer {your-jwt-token}
Content-Type: application/json
```

**Response Success (200):**
```json
{
  "success": true,
  "message": "Lấy trạng thái bảo dưỡng thành công",
  "data": {
    "vehicleId": 123,
    "licensePlate": "51A-12345",
    "modelName": "VinFast VF8",
    "estimatedCurrentKm": 15234,
    "lastMaintenanceKm": 10000,
    "lastMaintenanceDate": "2025-08-15T00:00:00",
    "nextMaintenanceKm": 20000,
    "averageKmPerDay": 45.5,
    "remainingKm": 4766,
    "estimatedDaysUntilMaintenance": 104,
    "estimatedNextMaintenanceDate": "2026-01-22T00:00:00",
    "progressPercent": 52.34,
    "status": "Normal",
    "message": "✅ Xe của bạn vẫn trong tình trạng tốt. Còn 4766 km hoặc khoảng 104 ngày đến lần bảo dưỡng tiếp theo.",
    "hasSufficientHistory": true,
    "historyCount": 3
  }
}
```

**Response Not Found (404):**
```json
{
  "success": false,
  "message": "Không tìm thấy xe với ID 123"
}
```

**Status Values:**
- `"Normal"` - Bình thường (< 70%)
- `"NeedAttention"` - Cần chú ý (70% - 90%)
- `"Urgent"` - Khẩn cấp (>= 90%)

---

### 2. Lấy Trạng Thái Tất Cả Xe Của Khách Hàng

**Endpoint:**
```
GET /api/VehicleMaintenance/my-vehicles/status
```

**Headers:**
```
Authorization: Bearer {your-jwt-token}
Content-Type: application/json
```

**Response Success (200):**
```json
{
  "success": true,
  "message": "Lấy trạng thái bảo dưỡng thành công cho 3 xe",
  "data": [
    {
      "vehicleId": 123,
      "licensePlate": "51A-12345",
      "modelName": "VinFast VF8",
      "estimatedCurrentKm": 15234,
      "lastMaintenanceKm": 10000,
      "lastMaintenanceDate": "2025-08-15T00:00:00",
      "nextMaintenanceKm": 20000,
      "averageKmPerDay": 45.5,
      "remainingKm": 4766,
      "estimatedDaysUntilMaintenance": 104,
      "estimatedNextMaintenanceDate": "2026-01-22T00:00:00",
      "progressPercent": 52.34,
      "status": "Normal",
      "message": "✅ Xe của bạn vẫn trong tình trạng tốt.",
      "hasSufficientHistory": true,
      "historyCount": 3
    },
    {
      "vehicleId": 124,
      "licensePlate": "51B-67890",
      "modelName": "VinFast VF9",
      "estimatedCurrentKm": 18900,
      "lastMaintenanceKm": 10000,
      "lastMaintenanceDate": "2025-07-01T00:00:00",
      "nextMaintenanceKm": 20000,
      "averageKmPerDay": 60.2,
      "remainingKm": 1100,
      "estimatedDaysUntilMaintenance": 18,
      "estimatedNextMaintenanceDate": "2025-10-28T00:00:00",
      "progressPercent": 89.0,
      "status": "NeedAttention",
      "message": "⚡ Xe của bạn sẽ cần bảo dưỡng sau khoảng 1100 km hoặc 18 ngày.",
      "hasSufficientHistory": true,
      "historyCount": 2
    },
    {
      "vehicleId": 125,
      "licensePlate": "51C-11111",
      "modelName": "Tesla Model 3",
      "estimatedCurrentKm": 19850,
      "lastMaintenanceKm": 10000,
      "lastMaintenanceDate": "2025-05-10T00:00:00",
      "nextMaintenanceKm": 20000,
      "averageKmPerDay": 70.5,
      "remainingKm": 150,
      "estimatedDaysUntilMaintenance": 2,
      "estimatedNextMaintenanceDate": "2025-10-12T00:00:00",
      "progressPercent": 98.5,
      "status": "Urgent",
      "message": "⚠️ Xe của bạn sắp đến hạn bảo dưỡng! Còn khoảng 150 km hoặc 2 ngày nữa. Vui lòng đặt lịch ngay.",
      "hasSufficientHistory": true,
      "historyCount": 4
    }
  ]
}
```

---

### 3. Lấy Lịch Sử Bảo Dưỡng

**Endpoint:**
```
GET /api/VehicleMaintenance/{vehicleId}/history
```

**Parameters:**
- `vehicleId` (path, required): ID của xe

**Response Success (200):**
```json
{
  "success": true,
  "message": "Lấy lịch sử bảo dưỡng thành công (3 lần)",
  "data": [
    {
      "historyId": 101,
      "serviceDate": "2025-08-15T00:00:00",
      "mileageAtService": 10000,
      "serviceType": "Bảo dưỡng định kỳ 10,000km",
      "totalCost": 1500000,
      "notes": "Thay nhớt, kiểm tra hệ thống phanh",
      "workOrderId": 501
    },
    {
      "historyId": 100,
      "serviceDate": "2025-02-20T00:00:00",
      "mileageAtService": 5000,
      "serviceType": "Bảo dưỡng định kỳ 5,000km",
      "totalCost": 1200000,
      "notes": "Thay nhớt, kiểm tra lốp xe",
      "workOrderId": 450
    }
  ]
}
```

---

### 4. Cập Nhật Km Hiện Tại (Thủ Công)

**Endpoint:**
```
PUT /api/VehicleMaintenance/{vehicleId}/mileage
```

**Parameters:**
- `vehicleId` (path, required): ID của xe

**Request Body:**
```json
{
  "currentMileage": 16000,
  "notes": "Cập nhật km sau chuyến đi dài"
}
```

**Response Success (200):**
```json
{
  "success": true,
  "message": "Cập nhật km thành công",
  "data": {
    "vehicleId": 123,
    "currentMileage": 16000,
    "updatedAt": "2025-10-10T17:30:00"
  }
}
```

**Validation:**
- `currentMileage`: Required, phải từ 0 đến 999,999
- `notes`: Optional, string

---

### 5. Lấy Danh Sách Xe Cần Bảo Dưỡng (Reminders)

**Endpoint:**
```
GET /api/VehicleMaintenance/reminders
```

**Response Success (200):**
```json
{
  "success": true,
  "message": "Bạn có 2 xe cần bảo dưỡng sớm",
  "data": [
    {
      "vehicleId": 125,
      "licensePlate": "51C-11111",
      "modelName": "Tesla Model 3",
      "estimatedCurrentKm": 19850,
      "remainingKm": 150,
      "progressPercent": 98.5,
      "status": "Urgent",
      "message": "⚠️ Xe của bạn sắp đến hạn bảo dưỡng!"
    },
    {
      "vehicleId": 124,
      "licensePlate": "51B-67890",
      "modelName": "VinFast VF9",
      "estimatedCurrentKm": 18900,
      "remainingKm": 1100,
      "progressPercent": 89.0,
      "status": "NeedAttention",
      "message": "⚡ Xe của bạn sẽ cần bảo dưỡng sớm."
    }
  ],
  "summary": {
    "totalVehicles": 3,
    "needsAttention": 1,
    "urgent": 1,
    "normal": 1
  }
}
```

**Note:** API này chỉ trả về xe có status `NeedAttention` hoặc `Urgent`, sắp xếp theo `remainingKm` tăng dần.

---

## 💻 Code Examples

### React + Axios

#### 1. Setup API Client

```typescript
// src/services/api.ts
import axios from 'axios';

const API_BASE_URL = process.env.REACT_APP_API_URL || 'https://localhost:5001';

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add JWT token to requests
apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('authToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export default apiClient;
```

#### 2. API Service Functions

```typescript
// src/services/maintenanceService.ts
import apiClient from './api';

export interface VehicleMaintenanceStatus {
  vehicleId: number;
  licensePlate: string;
  modelName: string;
  estimatedCurrentKm: number;
  lastMaintenanceKm: number;
  lastMaintenanceDate: string | null;
  nextMaintenanceKm: number;
  averageKmPerDay: number;
  remainingKm: number;
  estimatedDaysUntilMaintenance: number;
  estimatedNextMaintenanceDate: string | null;
  progressPercent: number;
  status: 'Normal' | 'NeedAttention' | 'Urgent';
  message: string;
  hasSufficientHistory: boolean;
  historyCount: number;
}

export interface MaintenanceHistoryItem {
  historyId: number;
  serviceDate: string;
  mileageAtService: number;
  serviceType: string;
  totalCost: number;
  notes: string | null;
  workOrderId: number | null;
}

// Get single vehicle status
export const getVehicleMaintenanceStatus = async (
  vehicleId: number
): Promise<VehicleMaintenanceStatus> => {
  const response = await apiClient.get(
    `/api/VehicleMaintenance/${vehicleId}/status`
  );
  return response.data.data;
};

// Get all vehicles status
export const getAllVehiclesMaintenanceStatus = async (): Promise<
  VehicleMaintenanceStatus[]
> => {
  const response = await apiClient.get(
    '/api/VehicleMaintenance/my-vehicles/status'
  );
  return response.data.data;
};

// Get maintenance history
export const getVehicleMaintenanceHistory = async (
  vehicleId: number
): Promise<MaintenanceHistoryItem[]> => {
  const response = await apiClient.get(
    `/api/VehicleMaintenance/${vehicleId}/history`
  );
  return response.data.data;
};

// Update vehicle mileage
export const updateVehicleMileage = async (
  vehicleId: number,
  currentMileage: number,
  notes?: string
): Promise<void> => {
  await apiClient.put(`/api/VehicleMaintenance/${vehicleId}/mileage`, {
    currentMileage,
    notes,
  });
};

// Get reminders
export const getMaintenanceReminders = async (): Promise<{
  data: VehicleMaintenanceStatus[];
  summary: {
    totalVehicles: number;
    needsAttention: number;
    urgent: number;
    normal: number;
  };
}> => {
  const response = await apiClient.get('/api/VehicleMaintenance/reminders');
  return {
    data: response.data.data,
    summary: response.data.summary,
  };
};
```

#### 3. React Component Example

```typescript
// src/components/VehicleMaintenanceDashboard.tsx
import React, { useEffect, useState } from 'react';
import { getAllVehiclesMaintenanceStatus, VehicleMaintenanceStatus } from '../services/maintenanceService';
import VehicleMaintenanceCard from './VehicleMaintenanceCard';

const VehicleMaintenanceDashboard: React.FC = () => {
  const [vehicles, setVehicles] = useState<VehicleMaintenanceStatus[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchVehicles = async () => {
      try {
        setLoading(true);
        const data = await getAllVehiclesMaintenanceStatus();
        setVehicles(data);
      } catch (err) {
        setError('Không thể tải trạng thái bảo dưỡng');
        console.error(err);
      } finally {
        setLoading(false);
      }
    };

    fetchVehicles();
  }, []);

  if (loading) {
    return <div className="text-center p-8">Đang tải...</div>;
  }

  if (error) {
    return <div className="text-center p-8 text-red-600">{error}</div>;
  }

  return (
    <div className="container mx-auto p-6">
      <h1 className="text-3xl font-bold mb-6">Trạng Thái Bảo Dưỡng Xe</h1>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {vehicles.map((vehicle) => (
          <VehicleMaintenanceCard key={vehicle.vehicleId} vehicle={vehicle} />
        ))}
      </div>
    </div>
  );
};

export default VehicleMaintenanceDashboard;
```

#### 4. Vehicle Card Component

```typescript
// src/components/VehicleMaintenanceCard.tsx
import React from 'react';
import { VehicleMaintenanceStatus } from '../services/maintenanceService';

interface Props {
  vehicle: VehicleMaintenanceStatus;
}

const VehicleMaintenanceCard: React.FC<Props> = ({ vehicle }) => {
  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Urgent':
        return 'bg-red-50 border-red-200 text-red-800';
      case 'NeedAttention':
        return 'bg-orange-50 border-orange-200 text-orange-800';
      default:
        return 'bg-green-50 border-green-200 text-green-800';
    }
  };

  const getProgressColor = (percent: number) => {
    if (percent >= 90) return 'bg-red-500';
    if (percent >= 70) return 'bg-orange-500';
    return 'bg-green-500';
  };

  return (
    <div className="bg-white rounded-lg shadow-lg p-6">
      {/* Header */}
      <div className="mb-4">
        <h3 className="text-xl font-bold text-gray-800">
          {vehicle.licensePlate}
        </h3>
        <p className="text-gray-600">{vehicle.modelName}</p>
      </div>

      {/* Progress Bar */}
      <div className="mb-4">
        <div className="flex justify-between text-sm mb-1">
          <span>Tiến độ</span>
          <span className="font-semibold">{vehicle.progressPercent.toFixed(1)}%</span>
        </div>
        <div className="w-full bg-gray-200 rounded-full h-4">
          <div
            className={`h-4 rounded-full ${getProgressColor(vehicle.progressPercent)}`}
            style={{ width: `${Math.min(vehicle.progressPercent, 100)}%` }}
          ></div>
        </div>
      </div>

      {/* Status Message */}
      <div className={`p-3 rounded-lg border ${getStatusColor(vehicle.status)} mb-4`}>
        <p className="text-sm font-medium">{vehicle.message}</p>
      </div>

      {/* Stats Grid */}
      <div className="grid grid-cols-2 gap-3">
        <div className="p-3 bg-gray-50 rounded">
          <p className="text-xs text-gray-600">Km ước tính</p>
          <p className="text-lg font-bold">{vehicle.estimatedCurrentKm.toLocaleString()}</p>
        </div>
        <div className="p-3 bg-gray-50 rounded">
          <p className="text-xs text-gray-600">Km còn lại</p>
          <p className="text-lg font-bold">{vehicle.remainingKm.toLocaleString()}</p>
        </div>
        <div className="p-3 bg-gray-50 rounded">
          <p className="text-xs text-gray-600">Trung bình/ngày</p>
          <p className="text-lg font-bold">{vehicle.averageKmPerDay.toFixed(1)}</p>
        </div>
        <div className="p-3 bg-gray-50 rounded">
          <p className="text-xs text-gray-600">Ngày còn lại</p>
          <p className="text-lg font-bold">{vehicle.estimatedDaysUntilMaintenance}</p>
        </div>
      </div>

      {/* Data Quality Warning */}
      {!vehicle.hasSufficientHistory && (
        <div className="mt-4 p-2 bg-yellow-50 border border-yellow-200 rounded">
          <p className="text-xs text-yellow-800">
            ⚠️ Chưa đủ dữ liệu lịch sử ({vehicle.historyCount} lần)
          </p>
        </div>
      )}

      {/* Action Button */}
      <button
        className={`w-full mt-4 py-2 rounded-lg font-semibold ${
          vehicle.status === 'Urgent'
            ? 'bg-red-500 hover:bg-red-600 text-white'
            : vehicle.status === 'NeedAttention'
            ? 'bg-orange-500 hover:bg-orange-600 text-white'
            : 'bg-blue-500 hover:bg-blue-600 text-white'
        }`}
        onClick={() => window.location.href = `/appointments/create?vehicleId=${vehicle.vehicleId}`}
      >
        {vehicle.status === 'Urgent' ? 'Đặt Lịch Ngay' : 'Xem Chi Tiết'}
      </button>
    </div>
  );
};

export default VehicleMaintenanceCard;
```

---

### Vue 3 + Composition API Example

```typescript
// src/composables/useMaintenance.ts
import { ref, onMounted } from 'vue';
import axios from 'axios';

export const useMaintenance = () => {
  const vehicles = ref([]);
  const loading = ref(true);
  const error = ref(null);

  const fetchVehicles = async () => {
    try {
      loading.value = true;
      const token = localStorage.getItem('authToken');
      const response = await axios.get(
        'https://localhost:5001/api/VehicleMaintenance/my-vehicles/status',
        {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        }
      );
      vehicles.value = response.data.data;
    } catch (err) {
      error.value = 'Không thể tải dữ liệu';
    } finally {
      loading.value = false;
    }
  };

  onMounted(fetchVehicles);

  return {
    vehicles,
    loading,
    error,
    refetch: fetchVehicles,
  };
};
```

---

## 🔧 Error Handling

### Common Error Responses

```typescript
// 401 Unauthorized
{
  "message": "Unauthorized"
}

// 404 Not Found
{
  "success": false,
  "message": "Không tìm thấy xe với ID 123"
}

// 400 Bad Request
{
  "success": false,
  "message": "Dữ liệu không hợp lệ",
  "errors": {
    "CurrentMileage": ["The field CurrentMileage must be between 0 and 999999."]
  }
}

// 500 Internal Server Error
{
  "success": false,
  "message": "Lỗi server"
}
```

### Error Handling Example

```typescript
try {
  const status = await getVehicleMaintenanceStatus(123);
} catch (error) {
  if (axios.isAxiosError(error)) {
    if (error.response?.status === 401) {
      // Redirect to login
      window.location.href = '/login';
    } else if (error.response?.status === 404) {
      alert('Không tìm thấy xe');
    } else {
      alert('Có lỗi xảy ra, vui lòng thử lại');
    }
  }
}
```

---

## 🎨 UI/UX Recommendations

### 1. Color Coding
- **Normal** (< 70%): Green (#4CAF50)
- **NeedAttention** (70-90%): Orange (#FFA500)
- **Urgent** (>= 90%): Red (#FF4444)

### 2. Notifications
- Hiển thị badge số lượng xe `Urgent` trên navigation
- Push notification khi xe đạt 80%, 90%
- Email reminder khi còn 7 ngày đến hạn bảo dưỡng

### 3. User Flow
```
1. User vào "Xe của tôi"
2. Xem dashboard với gauge charts
3. Click vào xe có status "Urgent"
4. Xem chi tiết và lịch sử
5. Click "Đặt Lịch Bảo Dưỡng"
6. Pre-fill form appointment với vehicle info
7. Submit appointment
```

---

## 🧪 Testing với Postman/Thunder Client

### Test Collection

```json
{
  "info": {
    "name": "Vehicle Maintenance API",
    "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "item": [
    {
      "name": "Get Vehicle Status",
      "request": {
        "method": "GET",
        "header": [
          {
            "key": "Authorization",
            "value": "Bearer {{token}}",
            "type": "text"
          }
        ],
        "url": {
          "raw": "{{baseUrl}}/api/VehicleMaintenance/1/status",
          "host": ["{{baseUrl}}"],
          "path": ["api", "VehicleMaintenance", "1", "status"]
        }
      }
    },
    {
      "name": "Get All Vehicles Status",
      "request": {
        "method": "GET",
        "header": [
          {
            "key": "Authorization",
            "value": "Bearer {{token}}",
            "type": "text"
          }
        ],
        "url": {
          "raw": "{{baseUrl}}/api/VehicleMaintenance/my-vehicles/status",
          "host": ["{{baseUrl}}"],
          "path": ["api", "VehicleMaintenance", "my-vehicles", "status"]
        }
      }
    },
    {
      "name": "Update Mileage",
      "request": {
        "method": "PUT",
        "header": [
          {
            "key": "Authorization",
            "value": "Bearer {{token}}",
            "type": "text"
          },
          {
            "key": "Content-Type",
            "value": "application/json",
            "type": "text"
          }
        ],
        "body": {
          "mode": "raw",
          "raw": "{\n  \"currentMileage\": 16000,\n  \"notes\": \"Manual update\"\n}"
        },
        "url": {
          "raw": "{{baseUrl}}/api/VehicleMaintenance/1/mileage",
          "host": ["{{baseUrl}}"],
          "path": ["api", "VehicleMaintenance", "1", "mileage"]
        }
      }
    }
  ],
  "variable": [
    {
      "key": "baseUrl",
      "value": "https://localhost:5001"
    },
    {
      "key": "token",
      "value": "your-jwt-token-here"
    }
  ]
}
```

---

## 📚 Additional Resources

- **Backend Guide**: `SMART_MAINTENANCE_REMINDER_GUIDE.md`
- **React Component**: `VehicleMaintenanceTracker.tsx`
- **API Swagger**: `https://localhost:5001/swagger`

---

## 🆘 Troubleshooting

### Issue 1: CORS Error
**Solution:** Backend đã configure CORS, đảm bảo frontend origin được thêm vào `appsettings.json`

### Issue 2: 401 Unauthorized
**Solution:** Kiểm tra JWT token còn hạn, refresh token nếu cần

### Issue 3: Empty Data
**Solution:** Đảm bảo:
- Customer đã có vehicle
- Vehicle đã có maintenance history (tối thiểu 2 lần để tính toán chính xác)

---

## 📞 Support

Nếu gặp vấn đề, liên hệ:
- Backend Team: backend@evservicecenter.com
- Slack Channel: #api-support

---

**Last Updated:** 2025-10-10
**Version:** 1.0.0
