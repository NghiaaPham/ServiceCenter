# EV SERVICE CENTER - CUSTOMER MAIN FLOW TEST
# Customer: nghiadaucau1@gmail.com / nghiadaucau123@

$baseUrl = "http://localhost:5153"
$ErrorActionPreference = "Continue"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  CUSTOMER MAIN FLOW TEST" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 1. LOGIN
Write-Host "[1/8] LOGIN..." -ForegroundColor Yellow
$loginBody = @{ email = "nghiadaucau1@gmail.com"; password = "nghiadaucau123@" } | ConvertTo-Json
try {
    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method Post -ContentType "application/json" -Body $loginBody
    $token = $loginResponse.data.token
    $customerId = $loginResponse.data.customerId
    Write-Host "SUCCESS - Customer ID: $customerId" -ForegroundColor Green
    Write-Host ""
} catch {
    Write-Host "FAILED!" -ForegroundColor Red
    exit 1
}

$headers = @{ "Authorization" = "Bearer $token"; "Content-Type" = "application/json" }

# 2. VIEW PROFILE
Write-Host "[2/8] VIEW PROFILE..." -ForegroundColor Yellow
try {
    $profile = (Invoke-RestMethod -Uri "$baseUrl/api/customers/$customerId" -Method Get -Headers $headers).data
    Write-Host "Name: $($profile.fullName)" -ForegroundColor Green
    Write-Host "Code: $($profile.customerCode)" -ForegroundColor Green
    Write-Host "Type: $($profile.customerTypeName)" -ForegroundColor Green
    Write-Host "Points: $($profile.loyaltyPoints)" -ForegroundColor Green
    Write-Host ""
} catch { Write-Host "FAILED" -ForegroundColor Red }

# 3. VIEW VEHICLES
Write-Host "[3/8] VIEW VEHICLES..." -ForegroundColor Yellow
try {
    $vehicles = (Invoke-RestMethod -Uri "$baseUrl/api/customer-vehicles/my-vehicles" -Method Get -Headers $headers).data
    Write-Host "Found $($vehicles.Count) vehicles:" -ForegroundColor Green
    foreach ($v in $vehicles) { Write-Host "  [$($v.vehicleId)] $($v.licensePlate) - $($v.brandName) $($v.modelName)" -ForegroundColor Gray }
    $vehicleId = $vehicles[0].vehicleId
    Write-Host ""
} catch { Write-Host "FAILED" -ForegroundColor Red; $vehicleId = 7 }

# 4. VIEW PACKAGES
Write-Host "[4/8] VIEW PACKAGES..." -ForegroundColor Yellow
try {
    $packages = (Invoke-RestMethod -Uri "$baseUrl/api/maintenance-packages?isActive=true" -Method Get).data
    Write-Host "Found $($packages.Count) packages:" -ForegroundColor Green
    foreach ($p in $packages) { Write-Host "  [$($p.packageId)] $($p.packageName) - $($p.totalPriceAfterDiscount) VND" -ForegroundColor Gray }
    Write-Host ""
} catch { Write-Host "FAILED" -ForegroundColor Red }

# 5. VIEW MY SUBSCRIPTIONS
Write-Host "[5/8] VIEW MY SUBSCRIPTIONS..." -ForegroundColor Yellow
try {
    $subs = (Invoke-RestMethod -Uri "$baseUrl/api/package-subscriptions/my-subscriptions" -Method Get -Headers $headers).data
    Write-Host "Found $($subs.Count) subscriptions:" -ForegroundColor Green
    $activeSub = $null
    foreach ($s in $subs) {
        Write-Host "  [$($s.subscriptionId)] $($s.packageName) - Status: $($s.statusDisplayName)" -ForegroundColor Gray
        if ($s.status -eq "Active" -and $s.canUse) { $activeSub = $s }
    }
    if ($activeSub) {
        Write-Host "Using subscription ID: $($activeSub.subscriptionId)" -ForegroundColor Cyan
        $subscriptionId = $activeSub.subscriptionId
    } else {
        Write-Host "No active subscription - will book with services" -ForegroundColor Yellow
        $subscriptionId = $null
    }
    Write-Host ""
} catch { Write-Host "FAILED" -ForegroundColor Red; $subscriptionId = $null }

# 6. VIEW SERVICES
Write-Host "[6/8] VIEW SERVICES..." -ForegroundColor Yellow
try {
    $services = (Invoke-RestMethod -Uri "$baseUrl/api/maintenance-services" -Method Get).data
    Write-Host "Found $($services.Count) services:" -ForegroundColor Green
    $serviceIds = @()
    for ($i = 0; $i -lt [Math]::Min(3, $services.Count); $i++) {
        Write-Host "  [$($services[$i].serviceId)] $($services[$i].serviceName)" -ForegroundColor Gray
        $serviceIds += $services[$i].serviceId
    }
    Write-Host ""
} catch { Write-Host "FAILED" -ForegroundColor Red; $serviceIds = @(1,2,3) }

# 7. VIEW TIME SLOTS
Write-Host "[7/8] VIEW TIME SLOTS..." -ForegroundColor Yellow
$date = (Get-Date).AddDays(2).ToString("yyyy-MM-dd")
try {
    $slots = (Invoke-RestMethod -Uri "$baseUrl/api/time-slots/available?serviceCenterId=2&date=$date" -Method Get).data
    if ($slots.Count -gt 0) {
        Write-Host "Found $($slots.Count) slots for $date" -ForegroundColor Green
        for ($i = 0; $i -lt [Math]::Min(5, $slots.Count); $i++) {
            Write-Host "  [$($slots[$i].slotId)] $($slots[$i].startTime) - $($slots[$i].endTime)" -ForegroundColor Gray
        }
        $slotId = $slots[0].slotId
    } else {
        Write-Host "No slots available" -ForegroundColor Yellow
        $slotId = 201
    }
    Write-Host ""
} catch { Write-Host "FAILED" -ForegroundColor Red; $slotId = 201 }

# 8. CREATE APPOINTMENT
Write-Host "[8/8] CREATE APPOINTMENT..." -ForegroundColor Yellow
$appointment = @{
    customerId = $customerId
    vehicleId = $vehicleId
    serviceCenterId = 2
    slotId = $slotId
    appointmentDate = $date
    customerNotes = "Main flow test"
}
if ($subscriptionId) {
    $appointment.subscriptionId = $subscriptionId
    $appointment.preferredServices = @()
    Write-Host "Booking WITH SUBSCRIPTION ID: $subscriptionId" -ForegroundColor Cyan
} else {
    $appointment.preferredServices = $serviceIds
    Write-Host "Booking WITH SERVICES: $($serviceIds -join ', ')" -ForegroundColor Cyan
}

try {
    $apt = (Invoke-RestMethod -Uri "$baseUrl/api/appointments" -Method Post -Headers $headers -Body ($appointment | ConvertTo-Json)).data
    Write-Host ""
    Write-Host "SUCCESS! Appointment created:" -ForegroundColor Green
    Write-Host "  Code: $($apt.appointmentCode)" -ForegroundColor Gray
    Write-Host "  Date: $($apt.appointmentDate)" -ForegroundColor Gray
    Write-Host "  Time: $($apt.timeSlot)" -ForegroundColor Gray
    Write-Host "  Status: $($apt.status)" -ForegroundColor Gray
    if ($apt.subscriptionCode) {
        Write-Host "  Subscription: $($apt.subscriptionCode)" -ForegroundColor Gray
        Write-Host "  Cost: 0 VND (using package)" -ForegroundColor Gray
    } else {
        Write-Host "  Estimated: $($apt.estimatedCost) VND" -ForegroundColor Gray
        Write-Host "  Discount: $($apt.discountAmount) VND" -ForegroundColor Gray
        Write-Host "  Final: $($apt.finalCost) VND" -ForegroundColor Gray
    }
    Write-Host ""
} catch {
    Write-Host ""
    Write-Host "FAILED!" -ForegroundColor Red
    if ($_.ErrorDetails) { Write-Host $_.ErrorDetails.Message -ForegroundColor Red }
    Write-Host ""
}

# 9. VIEW MY APPOINTMENTS
Write-Host "[BONUS] VIEW MY APPOINTMENTS..." -ForegroundColor Yellow
try {
    $myApts = (Invoke-RestMethod -Uri "$baseUrl/api/appointments/my-appointments" -Method Get -Headers $headers).data
    Write-Host "Found $($myApts.Count) appointments:" -ForegroundColor Green
    foreach ($a in $myApts | Select-Object -First 5) {
        Write-Host "  [$($a.appointmentId)] $($a.appointmentCode) - $($a.appointmentDate) - $($a.statusName)" -ForegroundColor Gray
    }
    Write-Host ""
} catch { Write-Host "FAILED" -ForegroundColor Red }

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  MAIN FLOW COMPLETED!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
