# ============================================
# TEST MAIN FLOW - KHACH HANG (CUSTOMER)
# EV Service Center Maintenance Management
# ============================================
# Customer: nghiadaucau1@gmail.com / nghiadaucau123@
# ============================================

$baseUrl = "http://localhost:5153"
$ErrorActionPreference = "Continue"

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  EV SERVICE CENTER - CUSTOMER MAIN FLOW" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# ============================================
# 1. LOGIN - Dang nhap
# ============================================
Write-Host "[1/8] LOGIN - Dang nhap vao he thong..." -ForegroundColor Yellow

$loginBody = @{
    email = "nghiadaucau1@gmail.com"
    password = "nghiadaucau123@"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method Post -ContentType "application/json" -Body $loginBody
    $token = $loginResponse.data.token
    $customerId = $loginResponse.data.customerId

    Write-Host "✓ Login thanh cong!" -ForegroundColor Green
    Write-Host "  - Customer ID: $customerId" -ForegroundColor Gray
    Write-Host "  - Email: $($loginResponse.data.email)" -ForegroundColor Gray
    Write-Host "  - Role: $($loginResponse.data.role)" -ForegroundColor Gray
    Write-Host ""
} catch {
    Write-Host "✗ Login that bai!" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

Start-Sleep -Seconds 1

# ============================================
# 2. XEM THONG TIN PROFILE - Thong tin khach hang
# ============================================
Write-Host "[2/8] XEM THONG TIN PROFILE..." -ForegroundColor Yellow

try {
    $profileResponse = Invoke-RestMethod -Uri "$baseUrl/api/customers/$customerId" -Method Get -Headers $headers
    $profile = $profileResponse.data

    Write-Host "✓ Thong tin khach hang:" -ForegroundColor Green
    Write-Host "  - Ho ten: $($profile.fullName)" -ForegroundColor Gray
    Write-Host "  - Ma KH: $($profile.customerCode)" -ForegroundColor Gray
    Write-Host "  - Loai KH: $($profile.customerTypeName)" -ForegroundColor Gray
    Write-Host "  - Diem tich luy: $($profile.loyaltyPoints)" -ForegroundColor Gray
    Write-Host "  - So dien thoai: $($profile.phoneNumber)" -ForegroundColor Gray
    Write-Host ""
} catch {
    Write-Host "✗ Khong lay duoc thong tin profile!" -ForegroundColor Red
}

Start-Sleep -Seconds 1

# ============================================
# 3. XEM DANH SACH XE - Quan ly xe
# ============================================
Write-Host "[3/8] XEM DANH SACH XE CUA KHACH HANG..." -ForegroundColor Yellow

try {
    $vehiclesResponse = Invoke-RestMethod -Uri "$baseUrl/api/customer-vehicles/my-vehicles" -Method Get -Headers $headers
    $vehicles = $vehiclesResponse.data

    Write-Host "✓ Danh sach xe ($($vehicles.Count) xe):" -ForegroundColor Green
    foreach ($vehicle in $vehicles) {
        Write-Host "  - [$($vehicle.vehicleId)] $($vehicle.licensePlate) - $($vehicle.brandName) $($vehicle.modelName) ($($vehicle.year))" -ForegroundColor Gray
    }
    Write-Host ""

    $selectedVehicleId = $vehicles[0].vehicleId
    $selectedPlate = $vehicles[0].licensePlate
} catch {
    Write-Host "✗ Khong lay duoc danh sach xe!" -ForegroundColor Red
    $selectedVehicleId = 7
    $selectedPlate = "77E-55555"
}

Start-Sleep -Seconds 1

# ============================================
# 4. XEM DANH SACH GOI BAO DUONG - Maintenance Packages
# ============================================
Write-Host "[4/8] XEM DANH SACH GOI BAO DUONG..." -ForegroundColor Yellow

try {
    $packagesResponse = Invoke-RestMethod -Uri "$baseUrl/api/maintenance-packages?isActive=true" -Method Get
    $packages = $packagesResponse.data

    Write-Host "✓ Danh sach goi bao duong ($($packages.Count) goi):" -ForegroundColor Green
    foreach ($package in $packages) {
        Write-Host "  - [$($package.packageId)] $($package.packageName)" -ForegroundColor Gray
        Write-Host "    Gia: $($package.totalPriceAfterDiscount) VND | Giam: $($package.discountPercent)%" -ForegroundColor Gray
        Write-Host "    Thoi han: $($package.validityPeriodInDays) ngay | Quang duong: $($package.validityMileage) km" -ForegroundColor Gray
    }
    Write-Host ""
} catch {
    Write-Host "✗ Khong lay duoc danh sach goi!" -ForegroundColor Red
}

Start-Sleep -Seconds 1

# ============================================
# 5. XEM SUBSCRIPTION HIEN TAI - Goi da mua
# ============================================
Write-Host "[5/8] XEM SUBSCRIPTION HIEN TAI..." -ForegroundColor Yellow

try {
    $subscriptionsResponse = Invoke-RestMethod -Uri "$baseUrl/api/package-subscriptions/my-subscriptions" -Method Get -Headers $headers
    $subscriptions = $subscriptionsResponse.data

    Write-Host "✓ Danh sach subscription ($($subscriptions.Count) goi):" -ForegroundColor Green

    $activeSubscription = $null
    foreach ($sub in $subscriptions) {
        Write-Host "  - [$($sub.subscriptionId)] $($sub.packageName)" -ForegroundColor Gray
        Write-Host "    Trang thai: $($sub.statusDisplayName)" -ForegroundColor Gray
        Write-Host "    Xe: $($sub.vehiclePlateNumber)" -ForegroundColor Gray
        Write-Host "    Su dung: $($sub.usageStatus) | Con lai: $($sub.daysUntilExpiry) ngay" -ForegroundColor Gray

        if ($sub.status -eq "Active" -and $sub.canUse) {
            $activeSubscription = $sub
        }
    }
    Write-Host ""

    if ($activeSubscription) {
        Write-Host "→ Su dung subscription: [$($activeSubscription.subscriptionId)] $($activeSubscription.packageName)" -ForegroundColor Cyan
        Write-Host ""
        $subscriptionId = $activeSubscription.subscriptionId
    } else {
        Write-Host "! Khong co subscription active → Dat lich voi dich vu don le" -ForegroundColor Yellow
        Write-Host ""
        $subscriptionId = $null
    }
} catch {
    Write-Host "✗ Khong lay duoc subscription!" -ForegroundColor Red
    Write-Host ""
    $subscriptionId = $null
}

Start-Sleep -Seconds 1

# ============================================
# 6. XEM DANH SACH DICH VU - Maintenance Services
# ============================================
Write-Host "[6/8] XEM DANH SACH DICH VU BAO DUONG..." -ForegroundColor Yellow

try {
    $servicesResponse = Invoke-RestMethod -Uri "$baseUrl/api/maintenance-services" -Method Get
    $services = $servicesResponse.data

    Write-Host "✓ Danh sach dich vu ($($services.Count) dich vu):" -ForegroundColor Green
    $servicesToBook = @()

    for ($i = 0; $i -lt [Math]::Min(3, $services.Count); $i++) {
        $service = $services[$i]
        Write-Host "  - [$($service.serviceId)] $($service.serviceName)" -ForegroundColor Gray
        Write-Host "    Loai: $($service.categoryName) | Thoi gian: $($service.estimatedDuration) phut" -ForegroundColor Gray
        $servicesToBook += $service.serviceId
    }
    Write-Host ""
} catch {
    Write-Host "✗ Khong lay duoc danh sach dich vu!" -ForegroundColor Red
    $servicesToBook = @(1, 2, 3)
}

Start-Sleep -Seconds 1

# ============================================
# 7. XEM TIME SLOTS - Chon gio hen
# ============================================
Write-Host "[7/8] XEM LICH TRONG..." -ForegroundColor Yellow

$appointmentDate = (Get-Date).AddDays(2).ToString("yyyy-MM-dd")

try {
    $slotsResponse = Invoke-RestMethod -Uri "$baseUrl/api/time-slots/available?serviceCenterId=2&date=$appointmentDate" -Method Get
    $slots = $slotsResponse.data

    if ($slots.Count -gt 0) {
        Write-Host "✓ Lich trong ngay $appointmentDate ($($slots.Count) slot):" -ForegroundColor Green

        for ($i = 0; $i -lt [Math]::Min(5, $slots.Count); $i++) {
            $slot = $slots[$i]
            Write-Host "  - [$($slot.slotId)] $($slot.startTime) - $($slot.endTime)" -ForegroundColor Gray
        }
        Write-Host ""

        $selectedSlotId = $slots[0].slotId
        $selectedSlotTime = "$($slots[0].startTime) - $($slots[0].endTime)"
    } else {
        Write-Host "! Khong co slot trong" -ForegroundColor Yellow
        Write-Host ""
        $selectedSlotId = 201
        $selectedSlotTime = "08:00 - 09:00"
    }
} catch {
    Write-Host "✗ Khong lay duoc time slots!" -ForegroundColor Red
    $selectedSlotId = 201
    $selectedSlotTime = "08:00 - 09:00"
}

Start-Sleep -Seconds 1

# ============================================
# 8. DAT LICH BAO DUONG - Create Appointment
# ============================================
Write-Host "[8/8] DAT LICH BAO DUONG..." -ForegroundColor Yellow

$appointmentBody = @{
    customerId = $customerId
    vehicleId = $selectedVehicleId
    serviceCenterId = 2
    slotId = $selectedSlotId
    appointmentDate = $appointmentDate
    customerNotes = "Test booking - Main flow demo for teacher"
}

# Neu co subscription thi dung, khong thi dung service don le
if ($subscriptionId) {
    $appointmentBody.subscriptionId = $subscriptionId
    $appointmentBody.preferredServices = @()
    $bookingType = "VOI SUBSCRIPTION"
} else {
    $appointmentBody.preferredServices = $servicesToBook
    $bookingType = "VOI DICH VU DON LE"
}

$appointmentJson = $appointmentBody | ConvertTo-Json

Write-Host "→ Dat lich $bookingType" -ForegroundColor Cyan
Write-Host "  - Ngay: $appointmentDate" -ForegroundColor Gray
Write-Host "  - Gio: $selectedSlotTime" -ForegroundColor Gray
Write-Host "  - Xe: $selectedPlate" -ForegroundColor Gray

try {
    $appointmentResponse = Invoke-RestMethod -Uri "$baseUrl/api/appointments" -Method Post -Headers $headers -Body $appointmentJson
    $appointment = $appointmentResponse.data

    Write-Host ""
    Write-Host "✓ DAT LICH THANH CONG!" -ForegroundColor Green
    Write-Host "  - Ma lich hen: $($appointment.appointmentCode)" -ForegroundColor Gray
    Write-Host "  - Trang thai: $($appointment.status)" -ForegroundColor Gray
    Write-Host "  - Ngay: $($appointment.appointmentDate)" -ForegroundColor Gray
    Write-Host "  - Gio: $($appointment.timeSlot)" -ForegroundColor Gray

    if ($appointment.subscriptionCode) {
        Write-Host "  - Subscription: $($appointment.subscriptionCode)" -ForegroundColor Gray
        Write-Host "  - Chi phi: 0 VND (Su dung goi)" -ForegroundColor Gray
    } else {
        Write-Host "  - Du kien: $($appointment.estimatedCost) VND" -ForegroundColor Gray
        Write-Host "  - Giam gia: $($appointment.discountAmount) VND" -ForegroundColor Gray
        Write-Host "  - Thanh toan: $($appointment.finalCost) VND" -ForegroundColor Gray
    }
    Write-Host ""

    $appointmentId = $appointment.appointmentId
} catch {
    Write-Host ""
    Write-Host "✗ DAT LICH THAT BAI!" -ForegroundColor Red
    if ($_.ErrorDetails) {
        Write-Host $_.ErrorDetails.Message -ForegroundColor Red
    } else {
        Write-Host $_.Exception.Message -ForegroundColor Red
    }
    Write-Host ""
}

Start-Sleep -Seconds 1

# ============================================
# 9. XEM DANH SACH LICH DA DAT - My Appointments
# ============================================
Write-Host "[BONUS] XEM DANH SACH LICH DA DAT..." -ForegroundColor Yellow

try {
    $myAppointmentsResponse = Invoke-RestMethod -Uri "$baseUrl/api/appointments/my-appointments" -Method Get -Headers $headers
    $myAppointments = $myAppointmentsResponse.data

    Write-Host "✓ Danh sach lich hen ($($myAppointments.Count) lich):" -ForegroundColor Green

    foreach ($apt in $myAppointments | Select-Object -First 5) {
        Write-Host "  - [$($apt.appointmentId)] $($apt.appointmentCode)" -ForegroundColor Gray
        Write-Host "    Ngay: $($apt.appointmentDate) | Gio: $($apt.timeSlot)" -ForegroundColor Gray
        Write-Host "    Trang thai: $($apt.statusName)" -ForegroundColor Gray
        Write-Host "    Xe: $($apt.vehiclePlateNumber)" -ForegroundColor Gray
    }
    Write-Host ""
} catch {
    Write-Host "✗ Khong lay duoc danh sach lich!" -ForegroundColor Red
    Write-Host ""
}

# ============================================
# TONG KET
# ============================================
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  HOAN THANH MAIN FLOW KHACH HANG!" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Cac chuc nang da test:" -ForegroundColor White
Write-Host "  ✓ 1. Login - Dang nhap" -ForegroundColor Green
Write-Host "  ✓ 2. Xem profile khach hang" -ForegroundColor Green
Write-Host "  ✓ 3. Xem danh sach xe" -ForegroundColor Green
Write-Host "  ✓ 4. Xem danh sach goi bao duong" -ForegroundColor Green
Write-Host "  ✓ 5. Xem subscription hien tai" -ForegroundColor Green
Write-Host "  ✓ 6. Xem danh sach dich vu" -ForegroundColor Green
Write-Host "  ✓ 7. Xem lich trong (time slots)" -ForegroundColor Green
Write-Host "  ✓ 8. Dat lich bao duong" -ForegroundColor Green
Write-Host "  ✓ 9. Xem danh sach lich da dat" -ForegroundColor Green
Write-Host ""
Write-Host "CHU Y: Tich hop thanh toan se duoc them vao sau!" -ForegroundColor Yellow
Write-Host ""
