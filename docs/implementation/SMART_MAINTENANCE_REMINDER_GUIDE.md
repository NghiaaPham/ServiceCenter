# Smart Maintenance Reminder - Implementation Guide

## Tổng quan

Smart Maintenance Reminder là tính năng **ước tính tự động km hiện tại** của xe dựa trên lịch sử bảo dưỡng, giúp nhắc nhở khách hàng đến hạn bảo dưỡng định kỳ mà không cần phải cập nhật km thủ công thường xuyên.

## Công thức tính toán

### 1. Thu thập dữ liệu lịch sử
Hệ thống lấy 2 lần bảo dưỡng gần nhất:
- Lần 1: `last_date`, `last_km`
- Lần 2: `previous_date`, `previous_km`

### 2. Tính km trung bình mỗi ngày
```
avg_km_per_day = (last_km - previous_km) / (last_date - previous_date)
```

### 3. Ước tính km hiện tại
```
estimated_current_km = last_km + (avg_km_per_day × days_since_last_maintenance)
```

### 4. Tính km còn lại đến lần bảo dưỡng tiếp theo
```
next_maintenance_km = last_km + 10,000 km (mặc định)
remaining_km = next_maintenance_km - estimated_current_km
```

### 5. Dự kiến ngày bảo dưỡng tiếp theo
```
estimated_days = remaining_km / avg_km_per_day
estimated_date = today + estimated_days
```

### 6. Phân loại trạng thái
- **Normal** (✅): progress < 70% - Xe vẫn trong tình trạng tốt
- **NeedAttention** (⚡): 70% ≤ progress < 90% - Cần chuẩn bị đặt lịch
- **Urgent** (⚠️): progress ≥ 90% - Cần đặt lịch ngay

## Backend Implementation

### 1. DTOs Created

#### VehicleMaintenanceStatusDto.cs
```csharp
EVServiceCenter.Core/Domains/CustomerVehicles/DTOs/Response/VehicleMaintenanceStatusDto.cs
```
Chứa toàn bộ thông tin trạng thái bảo dưỡng:
- EstimatedCurrentKm: Km ước tính hiện tại
- AverageKmPerDay: Trung bình km mỗi ngày
- RemainingKm: Km còn lại
- EstimatedDaysUntilMaintenance: Số ngày còn lại
- ProgressPercent: Phần trăm tiến độ (0-100%)
- Status: Normal/NeedAttention/Urgent
- Message: Thông báo cho người dùng

#### MaintenanceHistoryItemDto.cs
```csharp
EVServiceCenter.Core/Domains/CustomerVehicles/DTOs/Response/MaintenanceHistoryItemDto.cs
```
Thông tin lịch sử bảo dưỡng

#### UpdateVehicleMileageRequestDto.cs
```csharp
EVServiceCenter.Core/Domains/CustomerVehicles/DTOs/Request/UpdateVehicleMileageRequestDto.cs
```
Request để cập nhật km thủ công

### 2. Service Layer

#### IVehicleMaintenanceService.cs
```csharp
EVServiceCenter.Core/Domains/CustomerVehicles/Interfaces/Services/IVehicleMaintenanceService.cs
```

#### VehicleMaintenanceService.cs
```csharp
EVServiceCenter.Infrastructure/Domains/CustomerVehicles/Services/VehicleMaintenanceService.cs
```

**Core Logic:**
- Line 100-108: Tính km trung bình mỗi ngày
- Line 110-112: Ước tính km hiện tại
- Line 114-125: Tính km còn lại và dự kiến ngày bảo dưỡng
- Line 230-244: Xác định trạng thái (Normal/NeedAttention/Urgent)
- Line 249-257: Tạo message phù hợp

### 3. API Endpoints

#### VehicleMaintenanceController.cs
```csharp
EVServiceCenter.API/Controllers/CustomerVehicles/VehicleMaintenanceController.cs
```

**Available Endpoints:**

1. **GET /api/VehicleMaintenance/{vehicleId}/status**
   - Lấy trạng thái bảo dưỡng của 1 xe
   - Requires: CustomerOnly authorization
   - Returns: VehicleMaintenanceStatusDto

2. **GET /api/VehicleMaintenance/my-vehicles/status**
   - Lấy trạng thái tất cả xe của khách hàng đang đăng nhập
   - Requires: CustomerOnly authorization
   - Returns: List<VehicleMaintenanceStatusDto>

3. **GET /api/VehicleMaintenance/{vehicleId}/history**
   - Lấy lịch sử bảo dưỡng của xe
   - Requires: CustomerOnly authorization
   - Returns: List<MaintenanceHistoryItemDto>

4. **PUT /api/VehicleMaintenance/{vehicleId}/mileage**
   - Cập nhật km hiện tại (thủ công)
   - Requires: CustomerOnly authorization
   - Body: UpdateVehicleMileageRequestDto

5. **GET /api/VehicleMaintenance/reminders**
   - Lấy danh sách xe cần bảo dưỡng sớm
   - Requires: CustomerOnly authorization
   - Returns: Vehicles with NeedAttention or Urgent status

### 4. Dependency Injection

Service đã được đăng ký trong `CustomerVehicleDependencyInjection.cs`:
```csharp
services.AddScoped<IVehicleMaintenanceService, VehicleMaintenanceService>();
```

## Frontend Implementation

### React Component

**File:** `VehicleMaintenanceTracker.tsx`

#### Components Provided:

1. **VehicleMaintenanceTracker**
   - Single vehicle maintenance tracker với gauge chart
   - Props: vehicleId, apiBaseUrl, authToken
   - Features:
     - Gauge chart hiển thị progress %
     - Color coding: Green (Normal), Orange (NeedAttention), Red (Urgent)
     - Detailed statistics grid
     - Status message
     - Data quality notice

2. **MyVehiclesMaintenanceTracker**
   - Multiple vehicles display
   - Grid layout responsive
   - Tự động fetch tất cả xe của customer

#### Dependencies Required:
```bash
npm install echarts axios
npm install --save-dev @types/react @types/node
```

#### Usage Example:
```tsx
import VehicleMaintenanceTracker, { MyVehiclesMaintenanceTracker } from './VehicleMaintenanceTracker';

// Single vehicle
<VehicleMaintenanceTracker
  vehicleId={123}
  apiBaseUrl="https://api.example.com"
  authToken="your-jwt-token"
/>

// All vehicles
<MyVehiclesMaintenanceTracker
  apiBaseUrl="https://api.example.com"
  authToken="your-jwt-token"
/>
```

## API Testing Guide

### Using Swagger
1. Start the API: `dotnet run --project EVServiceCenter.API`
2. Navigate to: `https://localhost:5001/swagger`
3. Authorize with a Customer JWT token
4. Test endpoints:
   - Try `/api/VehicleMaintenance/my-vehicles/status` to see all your vehicles
   - Try `/api/VehicleMaintenance/reminders` to see urgent reminders

### Using curl
```bash
# Get vehicle status
curl -X GET "https://localhost:5001/api/VehicleMaintenance/1/status" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"

# Get all vehicles status
curl -X GET "https://localhost:5001/api/VehicleMaintenance/my-vehicles/status" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"

# Update mileage
curl -X PUT "https://localhost:5001/api/VehicleMaintenance/1/mileage" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"currentMileage": 15000, "notes": "Manual update"}'

# Get reminders
curl -X GET "https://localhost:5001/api/VehicleMaintenance/reminders" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

## Database Requirements

### Tables Used:
1. **CustomerVehicles** - Thông tin xe
   - VehicleId, LicensePlate, Mileage, CustomerId

2. **MaintenanceHistories** - Lịch sử bảo dưỡng
   - HistoryId, VehicleId, ServiceDate (DateOnly), Mileage, ServicesPerformed, TechnicianNotes

3. **VehicleModels** - Model xe
   - ModelId, ModelName, BrandId

4. **CarBrands** - Hãng xe
   - BrandId, BrandName

### Data Requirements:
- Cần **tối thiểu 2 lần bảo dưỡng** trong lịch sử để tính toán chính xác
- Nếu < 2 lần: hệ thống vẫn hoạt động nhưng không thể ước tính km/day

## Configuration

### Default Maintenance Interval
```csharp
// In VehicleMaintenanceService.cs line 19
private const decimal DEFAULT_MAINTENANCE_INTERVAL_KM = 10000;
```

Có thể thay đổi khoảng cách bảo dưỡng mặc định (10,000 km) theo nhu cầu.

### Status Thresholds
```csharp
// In VehicleMaintenanceService.cs lines 232-243
if (progressPercent >= 90) return "Urgent";
else if (progressPercent >= 70) return "NeedAttention";
else return "Normal";
```

## Business Logic Flow

1. **Customer logs in** → JWT token with CustomerId
2. **Frontend calls** `/api/VehicleMaintenance/my-vehicles/status`
3. **Backend fetches** all vehicles for that customer
4. **For each vehicle:**
   - Fetch last 2 maintenance records
   - Calculate avg_km_per_day
   - Estimate current km
   - Calculate remaining km
   - Determine status
5. **Return** aggregated data to frontend
6. **Frontend displays** gauge charts with color coding
7. **Customer sees** which vehicles need attention

## Integration with Appointment System

### Recommended Flow:
1. Customer views maintenance status → sees "Urgent" vehicle
2. Click "Đặt lịch bảo dưỡng" button
3. Pre-fill appointment form with:
   - Selected vehicle
   - Service type: "Bảo dưỡng định kỳ"
   - Estimated km
4. Submit appointment

### Future Enhancement:
- Automatic reminder emails/SMS when vehicle reaches 80%, 90% thresholds
- Background job to check all vehicles daily
- Push notifications for mobile app

## Error Handling

### Cases Handled:
1. **Vehicle not found** → 404 NotFound
2. **No maintenance history** → Returns estimate based on current vehicle mileage
3. **Only 1 maintenance record** → Can't calculate avg, uses current mileage
4. **Customer not found** → 400 BadRequest
5. **Division by zero** (same service dates) → Sets avg to 0

## Performance Considerations

- Service uses EF Core with `.Include()` for optimized queries
- Indexes recommended on:
  - `CustomerVehicles.CustomerId`
  - `MaintenanceHistories.VehicleId`
  - `MaintenanceHistories.ServiceDate`

## Security

- All endpoints protected with `[Authorize(Policy = "CustomerOnly")]`
- Customers can only see their own vehicles
- GetCurrentCustomerId() validates JWT token
- No direct vehicle access without ownership verification

## Testing Checklist

- [ ] Build succeeds with 0 errors
- [ ] Test GET /api/VehicleMaintenance/{vehicleId}/status with valid vehicle
- [ ] Test with vehicle that has 0 maintenance records
- [ ] Test with vehicle that has 1 maintenance record
- [ ] Test with vehicle that has 2+ maintenance records
- [ ] Test my-vehicles endpoint returns all customer vehicles
- [ ] Test reminders endpoint filters Urgent/NeedAttention correctly
- [ ] Test mileage update endpoint
- [ ] Test React component renders gauge correctly
- [ ] Test color coding (Green/Orange/Red) based on status
- [ ] Verify calculations match expected values

## Troubleshooting

### Build Errors:
- Ensure all DTOs are in correct namespace
- Check DateOnly to DateTime conversions use `.ToDateTime(TimeOnly.MinValue)`
- Verify GetCurrentCustomerId() returns `int` not `int?`

### Runtime Errors:
- Check customer is authenticated
- Verify vehicle belongs to logged-in customer
- Ensure MaintenanceHistory has Mileage values (not null)

## Next Steps (Optional Enhancements)

1. **Email/SMS Reminders**
   - Create background job (Hangfire/Quartz)
   - Send automatic reminders at 80%, 90% thresholds

2. **Mobile Push Notifications**
   - Integrate with Firebase Cloud Messaging
   - Send push when status changes to Urgent

3. **Custom Maintenance Intervals**
   - Allow per-vehicle custom intervals
   - Support different intervals for different service types

4. **Machine Learning Enhancement**
   - Train model on historical data
   - Predict maintenance needs based on usage patterns

5. **Dashboard Analytics**
   - Admin dashboard showing all customer vehicles
   - Statistics on maintenance compliance rates

---

## Files Summary

### Created Files:
1. `EVServiceCenter.Core/Domains/CustomerVehicles/DTOs/Response/VehicleMaintenanceStatusDto.cs`
2. `EVServiceCenter.Core/Domains/CustomerVehicles/DTOs/Response/MaintenanceHistoryItemDto.cs`
3. `EVServiceCenter.Core/Domains/CustomerVehicles/DTOs/Request/UpdateVehicleMileageRequestDto.cs`
4. `EVServiceCenter.Core/Domains/CustomerVehicles/Interfaces/Services/IVehicleMaintenanceService.cs`
5. `EVServiceCenter.Infrastructure/Domains/CustomerVehicles/Services/VehicleMaintenanceService.cs`
6. `EVServiceCenter.API/Controllers/CustomerVehicles/VehicleMaintenanceController.cs`
7. `VehicleMaintenanceTracker.tsx` (React component)

### Modified Files:
1. `EVServiceCenter.API/Extensions/CustomerVehicleDependencyInjection.cs` - Added service registration

---

**Status:** ✅ Implementation Complete
**Build:** ✅ Succeeded (0 errors, 16 warnings)
**Ready for:** Testing and Integration

---

Generated: 2025-10-10
