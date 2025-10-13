# Multi-Resource Scheduling Implementation

## 📋 Overview

Đã implement hệ thống **Multi-Resource Scheduling** với **Dynamic Duration** và **Multi-Center Architecture** để prevent conflicts giữa các tài nguyên:
- ✅ **Vehicle Time Conflict** - 🌍 GLOBAL: Xe không thể có 2 appointments overlap (bất kể center nào)
- ✅ **Technician Time Conflict** - 🏢 PER CENTER: Kỹ thuật viên không thể được assign vào 2 appointments overlap trong cùng trung tâm
- ✅ **Service Center Capacity** - 🏢 PER CENTER: Garage không thể vượt quá capacity (số xe cùng lúc trong mỗi trung tâm)

---

## 🏗️ Multi-Center Architecture

### Kiến trúc hệ thống

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│  TRUNG TÂM 1    │     │  TRUNG TÂM 2    │     │  TRUNG TÂM 3    │
├─────────────────┤     ├─────────────────┤     ├─────────────────┤
│ • Kỹ thuật viên │     │ • Kỹ thuật viên │     │ • Kỹ thuật viên │
│ • Khu sửa chữa  │     │ • Khu sửa chữa  │     │ • Khu sửa chữa  │
│ • Capacity: 5   │     │ • Capacity: 10  │     │ • Capacity: 3   │
│ • Slots riêng   │     │ • Slots riêng   │     │ • Slots riêng   │
└─────────────────┘     └─────────────────┘     └─────────────────┘
     ↑                       ↑                       ↑
     └───────────────────────┴───────────────────────┘
            Technician & Capacity: ĐỘC LẬP PER CENTER
                   Vehicle: GLOBAL ACROSS CENTERS
```

### Resource Scope Summary

| Resource | Scope | Lý do |
|----------|-------|-------|
| 🚗 **Vehicle** | 🌍 **GLOBAL** | Xe vật lý không thể ở 2 nơi cùng lúc |
| 👨‍🔧 **Technician** | 🏢 **PER CENTER** | Kỹ thuật viên thuộc về 1 trung tâm cụ thể |
| 🏭 **Capacity** | 🏢 **PER CENTER** | Mỗi trung tâm có giới hạn công suất riêng |

### Ví dụ minh họa

| Tình huống | Kết quả | Giải thích |
|-----------|---------|-----------|
| Xe A: CN1 (8h-11h) + CN1 (9h-10h) | ❌ SAI | Cùng xe, cùng center, overlap time |
| Xe A: CN1 (8h-11h) + CN2 (8h-11h) | ❌ SAI | ✅ **GLOBAL CHECK**: Xe không thể ở 2 nơi cùng lúc |
| Xe A: CN1 (8h-11h) + CN2 (12h-14h) | ✅ ĐƯỢC | Khác thời gian → OK |
| Tech B (CN1): 8h-11h + 9h-10h tại CN1 | ❌ SAI | Cùng tech, cùng center, overlap |
| Tech B (CN1): 8h-11h + Tech B (CN2): 9h-10h | ✅ ĐƯỢC | ✅ **PER CENTER**: Tech không thể làm ở CN2 (thuộc CN1) |
| CN1 có 5 xe cùng 9h, thêm xe thứ 6 | ❌ SAI | Vượt capacity của CN1 |

---

## 🔑 Key Concept: Dynamic Duration

**TRƯỚC KHI SỬA:**
- System chỉ dùng Slot time (ví dụ: 9h-10h = 1 giờ)
- Không tính đến actual service duration
- ❌ BUG: Xe book 3 dịch vụ × 3h = 9h vào slot 9h-10h → system nghĩ xe chỉ bận 1h

**SAU KHI SỬA:**
- ✅ Actual End Time = Start Time + EstimatedDuration
- ✅ Overlap formula: `Start_A < End_B AND Start_B < End_A`
- ✅ Xe book 9h với 9h duration → xe bận từ 9h-18h

---

## 📂 Files Changed

### 1. Interface: `IAppointmentQueryRepository.cs`

**Location:** `EVServiceCenter.Core\Domains\AppointmentManagement\Interfaces\Repositories\IAppointmentQueryRepository.cs`

**Changes:**
- **Line 93-96:** Added `GetVehicleAppointmentsByDateAsync()` (đã có từ trước)
- **Line 102-105:** ✅ NEW - Added `GetTechnicianAppointmentsByDateAsync()`
- **Line 111-114:** ✅ NEW - Added `GetServiceCenterAppointmentsByDateAsync()`

```csharp
// Line 102-106: ✅ UPDATED - Added serviceCenterId for PER CENTER check
Task<List<Appointment>> GetTechnicianAppointmentsByDateAsync(
    int technicianId,
    int serviceCenterId,  // ✅ NEW: PER CENTER filter
    DateOnly date,
    CancellationToken cancellationToken = default);

// Line 111-114
Task<List<Appointment>> GetServiceCenterAppointmentsByDateAsync(
    int serviceCenterId,
    DateOnly date,
    CancellationToken cancellationToken = default);
```

---

### 2. Repository: `AppointmentQueryRepository.cs`

**Location:** `EVServiceCenter.Infrastructure\Domains\AppointmentManagement\Repositories\AppointmentQueryRepository.cs`

**Changes:**
- **Line 445-456:** `GetVehicleAppointmentsByDateAsync()` implementation (đã có từ trước)
- **Line 458-469:** ✅ NEW - `GetTechnicianAppointmentsByDateAsync()` implementation
- **Line 471-482:** ✅ NEW - `GetServiceCenterAppointmentsByDateAsync()` implementation

```csharp
// Line 458-472: Technician query - ✅ UPDATED with PER CENTER filter
public async Task<List<Appointment>> GetTechnicianAppointmentsByDateAsync(
    int technicianId, int serviceCenterId, DateOnly date, CancellationToken cancellationToken = default)
{
    return await _context.Appointments
        .AsNoTracking()
        .Include(a => a.Slot)
        .Include(a => a.Status)
        .Where(a => a.PreferredTechnicianId == technicianId
                 && a.ServiceCenterId == serviceCenterId  // ✅ PER CENTER
                 && a.Slot!.SlotDate == date)
        .ToListAsync(cancellationToken);
}

// Line 471-482: Service Center query
public async Task<List<Appointment>> GetServiceCenterAppointmentsByDateAsync(
    int serviceCenterId, DateOnly date, CancellationToken cancellationToken = default)
{
    return await _context.Appointments
        .AsNoTracking()
        .Include(a => a.Slot)
        .Include(a => a.Status)
        .Where(a => a.ServiceCenterId == serviceCenterId && a.Slot!.SlotDate == date)
        .ToListAsync(cancellationToken);
}
```

---

### 3. Service: `AppointmentCommandService.cs`

**Location:** `EVServiceCenter.Infrastructure\Domains\AppointmentManagement\Services\AppointmentCommandService.cs`

#### A. CreateAsync() - Added 3 Validations

**Line 84-106:**

```csharp
// ✅ VEHICLE CONFLICT VALIDATION (với actual duration)
await ValidateVehicleTimeConflict(
    request.VehicleId,
    request.SlotId,
    totalDuration, // ✅ Truyền actual duration
    excludeAppointmentId: null,
    cancellationToken);

// ✅ TECHNICIAN CONFLICT VALIDATION (với actual duration, PER CENTER)
await ValidateTechnicianConflict(
    request.PreferredTechnicianId,
    request.ServiceCenterId,  // ✅ PER CENTER
    slot.SlotDate.ToDateTime(slot.StartTime),
    totalDuration,
    excludeAppointmentId: null,
    cancellationToken);

// ✅ SERVICE CENTER CAPACITY VALIDATION (với actual duration)
await ValidateServiceCenterCapacity(
    request.ServiceCenterId,
    slot.SlotDate.ToDateTime(slot.StartTime),
    totalDuration,
    excludeAppointmentId: null,
    cancellationToken);
```

#### B. RescheduleAsync() - Added 3 Validations

**Line 365-387:**

```csharp
// 10.5. ✅ VEHICLE CONFLICT VALIDATION
await ValidateVehicleTimeConflict(
    oldAppointment.VehicleId,
    request.NewSlotId,
    estimatedDuration,
    excludeAppointmentId: request.AppointmentId, // Loại trừ appointment đang reschedule
    cancellationToken);

// 10.6. ✅ TECHNICIAN CONFLICT VALIDATION
await ValidateTechnicianConflict(
    oldAppointment.PreferredTechnicianId,
    newSlot.SlotDate.ToDateTime(newSlot.StartTime),
    estimatedDuration,
    excludeAppointmentId: request.AppointmentId,
    cancellationToken);

// 10.7. ✅ SERVICE CENTER CAPACITY VALIDATION
await ValidateServiceCenterCapacity(
    oldAppointment.ServiceCenterId,
    newSlot.SlotDate.ToDateTime(newSlot.StartTime),
    estimatedDuration,
    excludeAppointmentId: request.AppointmentId,
    cancellationToken);
```

#### C. ValidateVehicleTimeConflict() - UPDATED

**Line 627-727:** Đã update để dùng dynamic duration thay vì slot.EndTime

```csharp
private async Task ValidateVehicleTimeConflict(
    int vehicleId,
    int newSlotId,
    int newEstimatedDuration, // ✅ ACTUAL DURATION parameter
    int? excludeAppointmentId,
    CancellationToken cancellationToken)
{
    var newSlot = await _slotRepository.GetByIdAsync(newSlotId, cancellationToken);

    var newStart = newSlot.SlotDate.ToDateTime(newSlot.StartTime);
    var newEnd = newStart.AddMinutes(newEstimatedDuration); // ✅ NOT slot.EndTime

    // ... query vehicle appointments ...

    foreach (var existingAppt in activeAppointments)
    {
        var existingStart = existingAppt.AppointmentDate;
        var existingEnd = existingStart.AddMinutes(existingAppt.EstimatedDuration ?? 60);

        // ✅ OVERLAP FORMULA
        bool isOverlap = newStart < existingEnd && existingStart < newEnd;

        if (isOverlap)
            throw new InvalidOperationException($"Xe này đã có lịch hẹn từ {existingStart:HH\\:mm} đến {existingEnd:HH\\:mm}...");
    }
}
```

#### D. ValidateTechnicianConflict() - ✅ UPDATED for PER CENTER

**Line 771-844:** ✅ UPDATED method signature - added serviceCenterId parameter

```csharp
/// <summary>
/// ✅ PER CENTER: Technician thuộc về 1 trung tâm, chỉ check trong trung tâm đó
/// </summary>
private async Task ValidateTechnicianConflict(
    int? technicianId,
    int serviceCenterId,  // ✅ NEW: PER CENTER filter
    DateTime newStart,
    int newEstimatedDuration,
    int? excludeAppointmentId,
    CancellationToken cancellationToken)
{
    if (!technicianId.HasValue) return; // Skip nếu không assign tech

    var newEnd = newStart.AddMinutes(newEstimatedDuration);
    var date = DateOnly.FromDateTime(newStart);

    _logger.LogInformation(
        "Technician conflict check (PER CENTER): TechnicianId={TechnicianId}, CenterId={CenterId}, ...",
        technicianId, serviceCenterId, ...);

    // ✅ Lấy appointments của tech TRONG TRUNG TÂM này
    var techAppointments = await _queryRepository.GetTechnicianAppointmentsByDateAsync(
        technicianId.Value, serviceCenterId, date, cancellationToken);

    // ... exclude current appointment ...

    var activeAppointments = techAppointments
        .Where(a => AppointmentStatusHelper.IsActiveBooking(a.StatusId))
        .ToList();

    foreach (var existingAppt in activeAppointments)
    {
        var existingStart = existingAppt.AppointmentDate;
        var existingEnd = existingStart.AddMinutes(existingAppt.EstimatedDuration ?? 60);

        bool isOverlap = newStart < existingEnd && existingStart < newEnd;

        if (isOverlap)
            throw new InvalidOperationException(
                $"Kỹ thuật viên đã được phân công từ {existingStart:HH\\:mm} đến {existingEnd:HH\\:mm}...");
    }
}
```

#### E. ValidateServiceCenterCapacity() - NEW

**Line 840-927:** ✅ NEW method

```csharp
private async Task ValidateServiceCenterCapacity(
    int serviceCenterId,
    DateTime newStart,
    int newEstimatedDuration,
    int? excludeAppointmentId,
    CancellationToken cancellationToken)
{
    var newEnd = newStart.AddMinutes(newEstimatedDuration);
    var date = DateOnly.FromDateTime(newStart);

    var centerAppointments = await _queryRepository.GetServiceCenterAppointmentsByDateAsync(
        serviceCenterId, date, cancellationToken);

    // ... exclude and filter active ...

    // Đếm số appointments OVERLAP
    int overlappingCount = 0;

    foreach (var existingAppt in activeAppointments)
    {
        var existingStart = existingAppt.AppointmentDate;
        var existingEnd = existingStart.AddMinutes(existingAppt.EstimatedDuration ?? 60);

        bool isOverlap = newStart < existingEnd && existingStart < newEnd;

        if (isOverlap)
            overlappingCount++;
    }

    int maxCapacity = GetServiceCenterMaxCapacity();
    int totalConcurrent = overlappingCount + 1; // +1 cho appointment mới

    if (totalConcurrent > maxCapacity)
        throw new InvalidOperationException(
            $"Trung tâm bảo dưỡng đã đầy công suất trong khung giờ {newStart:HH\\:mm}-{newEnd:HH\\:mm}...");
}
```

#### F. GetServiceCenterMaxCapacity() - NEW

**Line 977-981:** ✅ NEW configuration helper

```csharp
private int GetServiceCenterMaxCapacity()
{
    return _configuration.GetValue<int>(
        "AppointmentSettings:ServiceCenterCapacity:MaxConcurrentAppointments", 5);
}
```

---

## ⚙️ Configuration

Thêm vào `appsettings.json`:

```json
{
  "AppointmentSettings": {
    "VehicleConflictPolicy": {
      "MaxAppointmentsPerDay": 3
    },
    "ServiceCenterCapacity": {
      "MaxConcurrentAppointments": 5
    },
    "ReschedulePolicy": {
      "MinNoticeHours": 24,
      "MaxFreeReschedules": 2,
      "HotlineNumber": "1900-xxxx",
      "EnableStaffOverride": true
    }
  }
}
```

---

## 🧪 Test Scenarios

### Test 1: Vehicle Double Booking (CRITICAL)

**Mục đích:** Verify xe không thể book 2 appointments overlap

**Steps:**
1. Tạo appointment cho Vehicle ID = 1:
   - Slot: 2025-10-05 9:00-10:00
   - Services: 3 dịch vụ × 3h = **9 giờ duration**
   - Actual end time: 18:00 (9h + 9h)

2. Thử tạo appointment thứ 2 cho Vehicle ID = 1:
   - Slot: 2025-10-05 15:00-16:00

**Expected Result:**
```
❌ Error: "Xe này đã có lịch hẹn từ 09:00 đến 18:00 (Appointment #APT-20251005-0001).
Không thể đặt thêm lịch từ 15:00 đến 16:00 vì xe đang bận.
Vui lòng chọn thời gian khác hoặc chọn xe khác."
```

**API Call:**
```http
POST /api/appointments
{
  "customerId": 1,
  "vehicleId": 1,
  "serviceCenterId": 1,
  "slotId": 123,  // 15:00-16:00 slot
  "serviceIds": [1],
  "preferredTechnicianId": null
}
```

---

### Test 2: Vehicle Non-Overlap (Should Pass)

**Steps:**
1. Appointment 1: Vehicle 1, 9:00, duration 9h → ends 18:00
2. Appointment 2: Vehicle 1, **19:00**, duration 2h → ends 21:00

**Expected Result:**
```
✅ Success: Appointment created
```

**Giải thích:** 18:00 < 19:00 → không overlap

---

### Test 3: Technician Double Booking

**Mục đích:** Verify kỹ thuật viên không thể được assign vào 2 appointments overlap

**Steps:**
1. Tạo appointment với Technician ID = 5:
   - Slot: 2025-10-05 10:00-11:00
   - Duration: 4 giờ → ends 14:00

2. Thử assign Technician ID = 5 vào appointment khác:
   - Slot: 2025-10-05 13:00-14:00

**Expected Result:**
```
❌ Error: "Kỹ thuật viên đã được phân công từ 10:00 đến 14:00 (Appointment #APT-20251005-0002).
Không thể phân công thêm từ 13:00 đến 14:00.
Vui lòng chọn kỹ thuật viên khác hoặc khung giờ khác."
```

---

### Test 4: Technician with No Assignment (Should Skip)

**Steps:**
1. Tạo appointment KHÔNG có `preferredTechnicianId`

**Expected Result:**
```
✅ Success: Appointment created
ℹ️ Log: "No technician assigned, skip technician conflict check"
```

---

### Test 5: Service Center Capacity Exceeded

**Mục đích:** Verify garage không thể vượt quá capacity (default: 5 xe cùng lúc)

**Setup:**
- Service Center capacity = 5 (từ config)

**Steps:**
1. Tạo 5 appointments overlap vào cùng thời gian:
   - Appt 1: 9:00, duration 8h → 9:00-17:00
   - Appt 2: 10:00, duration 6h → 10:00-16:00
   - Appt 3: 11:00, duration 5h → 11:00-16:00
   - Appt 4: 12:00, duration 4h → 12:00-16:00
   - Appt 5: 13:00, duration 3h → 13:00-16:00

2. Thử tạo appointment thứ 6:
   - Slot: 14:00, duration 2h → 14:00-16:00

**Expected Result:**
```
❌ Error: "Trung tâm bảo dưỡng đã đầy công suất trong khung giờ 14:00-16:00.
Hiện có 5 lịch hẹn đang thực hiện, vượt quá khả năng tiếp nhận (5 xe cùng lúc).
Vui lòng chọn khung giờ khác hoặc liên hệ tổng đài 1900-xxxx để được hỗ trợ."
```

**Giải thích:** Tại 14:00-16:00, có 5 appointments đang overlap:
- Appt 1: 9-17 ✓
- Appt 2: 10-16 ✓
- Appt 3: 11-16 ✓
- Appt 4: 12-16 ✓
- Appt 5: 13-16 ✓
- **Appt 6 (new): 14-16 → TOTAL = 6 > capacity 5 ❌**

---

### Test 6: Service Center Capacity OK (Partial Overlap)

**Steps:**
1. Appt 1: 9:00-12:00
2. Appt 2: 13:00-16:00 (không overlap với Appt 1)

**Expected Result:**
```
✅ Success: Cả 2 appointments đều pass vì tại mọi thời điểm chỉ có 1 xe
```

---

### Test 7: Reschedule with Conflicts

**Steps:**
1. Tạo Appointment A: Vehicle 1, 9:00, duration 5h
2. Tạo Appointment B: Vehicle 1, 15:00, duration 3h
3. Thử reschedule Appointment B sang 12:00

**Expected Result:**
```
❌ Error: Vehicle conflict vì:
- Appt A: 9:00-14:00
- Appt B (reschedule): 12:00-15:00
- Overlap: 12:00-14:00
```

---

### Test 8: Reschedule Excluding Self

**Steps:**
1. Tạo Appointment A: Vehicle 1, 9:00, duration 5h → 9:00-14:00
2. Reschedule Appointment A sang 10:00 (cùng ngày)

**Expected Result:**
```
✅ Success: Pass vì excludeAppointmentId loại trừ chính nó
```

---

### Test 9: 🌍 GLOBAL Vehicle Conflict - Cross Center (CRITICAL)

**Mục đích:** Verify xe KHÔNG THỂ đặt lịch ở 2 trung tâm khác nhau cùng lúc (GLOBAL check)

**Steps:**
1. Tạo appointment cho Vehicle ID = 1 tại **Center 1**:
   - ServiceCenterId: 1
   - Slot: 2025-10-05 8:00-9:00
   - Duration: 3h → actual end: 11:00

2. Thử tạo appointment cho Vehicle ID = 1 tại **Center 2** (khác center):
   - ServiceCenterId: 2
   - Slot: 2025-10-05 9:00-10:00
   - Duration: 2h → actual end: 11:00

**Expected Result:**
```
❌ Error: "Xe này đã có lịch hẹn từ 08:00 đến 11:00 (Appointment #APT-20251005-0001).
Không thể đặt thêm lịch từ 09:00 đến 11:00 vì xe đang bận.
Vui lòng chọn thời gian khác hoặc chọn xe khác."
```

**API Call:**
```http
POST /api/appointments
{
  "customerId": 1,
  "vehicleId": 1,
  "serviceCenterId": 2,  // ← Khác center
  "slotId": 200,  // 9:00-10:00 tại Center 2
  "serviceIds": [1, 2],
  "preferredTechnicianId": null
}
```

**Giải thích:**
- ✅ **GLOBAL CHECK**: Xe vật lý không thể ở 2 nơi cùng lúc
- Xe đang ở Center 1 (8h-11h) → không thể đến Center 2 (9h-11h)
- Overlap: 8 < 11 AND 9 < 11 = TRUE

---

### Test 10: 🌍 GLOBAL Vehicle - Cross Center OK (Different Time)

**Steps:**
1. Appointment 1: Vehicle 1, **Center 1**, 8:00-11:00
2. Appointment 2: Vehicle 1, **Center 2**, 12:00-14:00

**Expected Result:**
```
✅ Success: Appointments created ở 2 centers khác nhau
```

**Giải thích:**
- Khác thời gian (11:00 < 12:00) → không overlap
- Xe có thể đi từ Center 1 sang Center 2 nếu đủ thời gian

---

### Test 11: 🏢 PER CENTER Technician - Cross Center OK

**Mục đích:** Verify tech KHÔNG BỊ conflict khi làm ở center khác (data inconsistency case)

**Steps:**
1. Tạo appointment với Tech ID = 5 tại **Center 1**:
   - ServiceCenterId: 1
   - TechnicianId: 5
   - Time: 8:00-12:00

2. Thử tạo appointment với Tech ID = 5 tại **Center 2**:
   - ServiceCenterId: 2
   - TechnicianId: 5
   - Time: 9:00-11:00 (overlap với appointment tại Center 1)

**Expected Result:**
```
✅ Success: Appointment created (KHÔNG block)
```

**Giải thích:**
- ✅ **PER CENTER CHECK**: Query chỉ check trong Center 2
- Tech 5 không có appointment nào tại Center 2 → pass validation
- **LƯU Ý**: Đây là data inconsistency case (tech không thể ở 2 center cùng lúc)
- Trong thực tế, cần có constraint: Tech thuộc về 1 center duy nhất

---

### Test 12: 🏢 PER CENTER Capacity - Independent Centers

**Mục đích:** Verify capacity check độc lập từng center

**Setup:**
- Center 1 capacity: 5 xe
- Center 2 capacity: 5 xe

**Steps:**
1. Tạo 5 appointments overlap tại **Center 1** vào 9h-10h (FULL)
2. Thử tạo appointment thứ 6 tại **Center 2** vào 9h-10h

**Expected Result:**
```
✅ Success: Appointment created tại Center 2
```

**Giải thích:**
- ✅ **PER CENTER**: Capacity check riêng cho từng center
- Center 1 full (5/5) KHÔNG ảnh hưởng Center 2 (1/5)

---

## 🔍 How to Verify in Logs

Khi chạy API, check logs để verify validation logic:

### ✅ Success Case Logs:
```
[Information] Vehicle time conflict check: VehicleId=1, Time=[15:00-18:00], Duration=180min
[Information] Found 1 active appointments for vehicle 1 on 2025-10-05
[Information] Vehicle time conflict validation PASSED: VehicleId=1, Time=[15:00-18:00], Appointments in day: 1/3

[Information] Technician conflict check (PER CENTER): TechnicianId=5, CenterId=1, Time=[15:00-18:00], Duration=180min
[Information] Found 0 active appointments for technician 5 in center 1 on 2025-10-05
[Information] Technician conflict validation PASSED (PER CENTER): TechnicianId=5, CenterId=1, Time=[15:00-18:00]

[Information] Service center capacity check: CenterId=1, Time=[15:00-18:00], Duration=180min
[Information] Found 3 active appointments for service center 1 on 2025-10-05
[Information] Service center capacity: 2/5 concurrent appointments during [15:00-18:00]
[Information] Service center capacity validation PASSED: CenterId=1, Time=[15:00-18:00], Concurrent=2/5
```

### ❌ Failure Case Logs:
```
[Warning] Vehicle TIME OVERLAP: VehicleId=1, New: [15:00-18:00], Existing Appt #APT-20251005-0001: [09:00-18:00]
[Error] InvalidOperationException: Xe này đã có lịch hẹn từ 09:00 đến 18:00...

[Warning] Technician TIME OVERLAP: TechnicianId=5, New: [13:00-16:00], Existing Appt #APT-20251005-0002: [10:00-14:00]
[Error] InvalidOperationException: Kỹ thuật viên đã được phân công từ 10:00 đến 14:00...

[Warning] Service center CAPACITY EXCEEDED: CenterId=1, Time=[14:00-16:00], Concurrent=6, Max=5
[Error] InvalidOperationException: Trung tâm bảo dưỡng đã đầy công suất...
```

---

## 📊 Database Queries for Manual Verification

### Query 1: Check Vehicle Appointments
```sql
SELECT
    a.AppointmentId,
    a.AppointmentCode,
    a.VehicleId,
    v.LicensePlate,
    a.AppointmentDate,
    a.EstimatedDuration,
    DATEADD(MINUTE, a.EstimatedDuration, a.AppointmentDate) AS ActualEndTime,
    s.StatusName
FROM Appointments a
JOIN CustomerVehicles v ON a.VehicleId = v.VehicleId
JOIN AppointmentStatuses s ON a.StatusId = s.StatusId
WHERE a.VehicleId = 1
  AND CAST(a.AppointmentDate AS DATE) = '2025-10-05'
  AND s.StatusId IN (1, 2, 6) -- Pending, Confirmed, InProgress
ORDER BY a.AppointmentDate
```

### Query 2: Check Overlapping Appointments
```sql
DECLARE @NewStart DATETIME = '2025-10-05 15:00:00'
DECLARE @NewDuration INT = 180 -- minutes
DECLARE @NewEnd DATETIME = DATEADD(MINUTE, @NewDuration, @NewStart)

SELECT
    a.AppointmentCode,
    a.AppointmentDate AS ExistingStart,
    DATEADD(MINUTE, a.EstimatedDuration, a.AppointmentDate) AS ExistingEnd,
    CASE
        WHEN @NewStart < DATEADD(MINUTE, a.EstimatedDuration, a.AppointmentDate)
         AND a.AppointmentDate < @NewEnd
        THEN 'OVERLAP ❌'
        ELSE 'No Overlap ✅'
    END AS OverlapStatus
FROM Appointments a
WHERE a.VehicleId = 1
  AND CAST(a.AppointmentDate AS DATE) = CAST(@NewStart AS DATE)
  AND a.StatusId IN (1, 2, 6)
```

### Query 3: Service Center Capacity at Specific Time
```sql
DECLARE @CheckTime DATETIME = '2025-10-05 14:00:00'

SELECT
    a.AppointmentCode,
    a.AppointmentDate,
    DATEADD(MINUTE, a.EstimatedDuration, a.AppointmentDate) AS EndTime,
    a.EstimatedDuration
FROM Appointments a
WHERE a.ServiceCenterId = 1
  AND CAST(a.AppointmentDate AS DATE) = CAST(@CheckTime AS DATE)
  AND a.StatusId IN (1, 2, 6)
  AND @CheckTime < DATEADD(MINUTE, a.EstimatedDuration, a.AppointmentDate)
  AND a.AppointmentDate < @CheckTime
ORDER BY a.AppointmentDate

-- Count concurrent at @CheckTime
SELECT COUNT(*) AS ConcurrentCount
FROM Appointments a
WHERE a.ServiceCenterId = 1
  AND CAST(a.AppointmentDate AS DATE) = CAST(@CheckTime AS DATE)
  AND a.StatusId IN (1, 2, 6)
  AND @CheckTime >= a.AppointmentDate
  AND @CheckTime < DATEADD(MINUTE, a.EstimatedDuration, a.AppointmentDate)
```

---

## 🎯 Business Rules Summary

| Resource | Scope | Rule | Config |
|----------|-------|------|--------|
| 🚗 **Vehicle** | 🌍 **GLOBAL** | Không thể có 2 appointments overlap về thời gian (bất kể center nào) | `MaxAppointmentsPerDay: 3` |
| 👨‍🔧 **Technician** | 🏢 **PER CENTER** | Không thể assign vào 2 appointments overlap trong cùng center | N/A (optional) |
| 🏭 **Capacity** | 🏢 **PER CENTER** | Số xe concurrent không vượt quá capacity trong từng center | `MaxConcurrentAppointments: 5` |

### Giải thích Scope:

**🌍 GLOBAL (Vehicle):**
- Check TOÀN HỆ THỐNG (tất cả centers)
- Xe vật lý không thể ở 2 nơi cùng lúc
- Query: `WHERE VehicleId = X AND SlotDate = Y` (KHÔNG filter by CenterId)

**🏢 PER CENTER (Technician & Capacity):**
- Check TRONG PHẠM VI trung tâm cụ thể
- Mỗi center có resource độc lập
- Query: `WHERE ... AND ServiceCenterId = Z`

### Formulas:

**Overlap Formula (áp dụng cho cả 3 resources):**
```
Start_A < End_B AND Start_B < End_A
```

**Dynamic Duration:**
```
ActualEndTime = AppointmentDate + EstimatedDuration (minutes)
```

**Cross-Center Examples:**
```
✅ Xe A: Center1 (8h-11h) → Center2 (12h-14h)  // Khác thời gian
❌ Xe A: Center1 (8h-11h) → Center2 (9h-10h)   // GLOBAL overlap
✅ Tech B (Center1): 8h-11h → Center2: 9h-10h   // PER CENTER (tech thuộc Center1)
✅ Center1 full (5/5) → Center2 (1/5)           // PER CENTER capacity
```

---

## 🚀 Next Steps

1. **Testing:**
   - Chạy các test scenarios ở trên
   - Verify logs
   - Check database queries

2. **Configuration:**
   - Update `appsettings.json` với capacity phù hợp
   - Adjust `MaxConcurrentAppointments` theo từng service center (nếu cần)

3. **Monitoring:**
   - Track logs để xem patterns
   - Identify peak hours để optimize capacity

4. **Future Enhancements (nếu cần):**
   - Per-service-center capacity (khác nhau theo location)
   - Time-based capacity (peak hours vs off-peak)
   - Priority queue cho VIP customers
   - Real-time capacity dashboard

---

## 📞 Support

Nếu có vấn đề, check:
1. Logs (search for "conflict", "overlap", "capacity")
2. Database queries ở trên
3. Configuration trong appsettings.json
4. Status của appointments (chỉ check status 1, 2, 6)

**Files to review:**
- `AppointmentCommandService.cs` (lines 84-106, 365-387, 627-927)
- `AppointmentQueryRepository.cs` (lines 445-482)
- `IAppointmentQueryRepository.cs` (lines 93-114)
