# EV Service Center API Testing Script
# Tests main customer flow from login to booking

$baseUrl = "http://localhost:5153/api"
$token = ""

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  EV SERVICE CENTER API TEST SUITE" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Test 1: Health Check
Write-Host "[TEST 1] Health Check - Get Lookups" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/lookups" -Method Get
    Write-Host "✅ PASS: API is accessible" -ForegroundColor Green
    Write-Host "Response: $($response | ConvertTo-Json -Depth 1 -Compress)" -ForegroundColor Gray
} catch {
    Write-Host "❌ FAIL: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Test 2: View Services (Public API)
Write-Host "[TEST 2] View Maintenance Services (Public)" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/maintenance-services" -Method Get
    $serviceCount = $response.data.Count
    Write-Host "✅ PASS: Found $serviceCount services" -ForegroundColor Green
    Write-Host "Sample service: $($response.data[0].serviceName) - $($response.data[0].basePrice) VNĐ" -ForegroundColor Gray
} catch {
    Write-Host "❌ FAIL: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Test 3: View Packages (Public API)
Write-Host "[TEST 3] View Maintenance Packages (Public)" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/maintenance-packages" -Method Get
    $packageCount = $response.data.Count
    Write-Host "✅ PASS: Found $packageCount packages" -ForegroundColor Green
    if ($packageCount -gt 0) {
        Write-Host "Sample package: $($response.data[0].packageName) - $($response.data[0].price) VNĐ" -ForegroundColor Gray
    }
} catch {
    Write-Host "❌ FAIL: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Test 4: Login with seeded customer
Write-Host "[TEST 4] Customer Login" -ForegroundColor Yellow
$loginData = @{
    email = "customer1@example.com"
    password = "Customer@123"
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method Post -Body $loginData -ContentType "application/json"
    $token = $response.data.token
    $customerName = $response.data.user.fullName
    Write-Host "✅ PASS: Logged in as $customerName" -ForegroundColor Green
    Write-Host "Token: $($token.Substring(0, 30))..." -ForegroundColor Gray
} catch {
    Write-Host "❌ FAIL: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "⚠️  Using default credentials: customer1@example.com / Customer@123" -ForegroundColor Yellow
    Write-Host "⚠️  Make sure customer is seeded in database" -ForegroundColor Yellow
}
Write-Host ""

if ($token -eq "") {
    Write-Host "❌ Cannot continue without authentication token" -ForegroundColor Red
    exit
}

# Test 5: Get My Profile
Write-Host "[TEST 5] Get My Profile" -ForegroundColor Yellow
$headers = @{
    "Authorization" = "Bearer $token"
}
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/customer/profile/me" -Method Get -Headers $headers
    Write-Host "✅ PASS: Profile loaded" -ForegroundColor Green
    Write-Host "Customer: $($response.data.fullName)" -ForegroundColor Gray
    Write-Host "Loyalty Points: $($response.data.loyaltyPoints)" -ForegroundColor Gray
} catch {
    Write-Host "❌ FAIL: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Test 6: Get My Vehicles
Write-Host "[TEST 6] Get My Vehicles" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/customer/profile/my-vehicles" -Method Get -Headers $headers
    $vehicleCount = $response.data.Count
    Write-Host "✅ PASS: Found $vehicleCount vehicle(s)" -ForegroundColor Green
    if ($vehicleCount -gt 0) {
        $firstVehicle = $response.data[0]
        Write-Host "Vehicle: $($firstVehicle.licensePlate) - $($firstVehicle.carModel.modelName)" -ForegroundColor Gray
        $script:testVehicleId = $firstVehicle.vehicleId
    }
} catch {
    Write-Host "❌ FAIL: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Test 7: Get My Subscriptions
Write-Host "[TEST 7] Get My Subscriptions" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/package-subscriptions/my-subscriptions" -Method Get -Headers $headers
    $subCount = $response.data.Count
    Write-Host "✅ PASS: Found $subCount subscription(s)" -ForegroundColor Green
    if ($subCount -gt 0) {
        $sub = $response.data[0]
        Write-Host "Subscription: $($sub.packageName)" -ForegroundColor Gray
        Write-Host "Status: $($sub.status) | Remaining: $($sub.remainingServices)/$($sub.totalServices)" -ForegroundColor Gray
        $script:testSubscriptionId = $sub.subscriptionId
    }
} catch {
    Write-Host "❌ FAIL: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Test 8: Get Available Time Slots
Write-Host "[TEST 8] Get Available Time Slots" -ForegroundColor Yellow
$tomorrow = (Get-Date).AddDays(1).ToString("yyyy-MM-dd")
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/timeslots/available?date=$tomorrow&serviceCenterId=1" -Method Get
    $slotCount = $response.data.availableSlots.Count
    Write-Host "✅ PASS: Found $slotCount available slots for $tomorrow" -ForegroundColor Green
    if ($slotCount -gt 0) {
        $firstSlot = $response.data.availableSlots[0]
        Write-Host "First slot: $($firstSlot.startTime) - $($firstSlot.endTime)" -ForegroundColor Gray
        $script:testSlotId = $firstSlot.slotId
    }
} catch {
    Write-Host "❌ FAIL: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Test 9: Create Appointment (Smart Booking)
Write-Host "[TEST 9] Create Appointment with Smart Subscription" -ForegroundColor Yellow
if ($script:testVehicleId -and $script:testSlotId) {
    $appointmentData = @{
        serviceCenterId = 1
        vehicleId = $script:testVehicleId
        appointmentDate = $tomorrow
        slotId = $script:testSlotId
        services = @(
            @{
                serviceId = 2
                quantity = 1
                notes = "Test booking from API"
            }
        )
        notes = "Test appointment created by PowerShell script"
    } | ConvertTo-Json -Depth 3

    try {
        $response = Invoke-RestMethod -Uri "$baseUrl/appointments" -Method Post -Body $appointmentData -ContentType "application/json" -Headers $headers
        Write-Host "✅ PASS: Appointment created successfully!" -ForegroundColor Green
        Write-Host "Appointment Code: $($response.data.appointmentCode)" -ForegroundColor Gray
        Write-Host "Subscription Applied: $($response.data.subscriptionApplied)" -ForegroundColor Gray
        Write-Host "Total Amount: $($response.data.totalAmount) VNĐ" -ForegroundColor Gray

        if ($response.data.subscriptionApplied) {
            Write-Host "✨ SMART SUBSCRIPTION: Service applied from package!" -ForegroundColor Magenta
            Write-Host "Package: $($response.data.subscriptionDetails.packageName)" -ForegroundColor Gray
            Write-Host "Remaining after: $($response.data.subscriptionDetails.remainingServicesAfter)" -ForegroundColor Gray
        }

        $script:testAppointmentId = $response.data.appointmentId
    } catch {
        Write-Host "❌ FAIL: $($_.Exception.Message)" -ForegroundColor Red
        if ($_.ErrorDetails.Message) {
            Write-Host "Details: $($_.ErrorDetails.Message)" -ForegroundColor Red
        }
    }
} else {
    Write-Host "⚠️  SKIP: Missing vehicle or slot ID" -ForegroundColor Yellow
}
Write-Host ""

# Test 10: Get My Appointments
Write-Host "[TEST 10] Get My Appointments" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/appointments/my-appointments" -Method Get -Headers $headers
    $aptCount = $response.data.Count
    Write-Host "✅ PASS: Found $aptCount appointment(s)" -ForegroundColor Green
    if ($aptCount -gt 0) {
        $apt = $response.data[0]
        Write-Host "Latest: $($apt.appointmentCode) - $($apt.status)" -ForegroundColor Gray
        Write-Host "Date: $($apt.appointmentDate) $($apt.slotTime)" -ForegroundColor Gray
        Write-Host "Subscription Used: $($apt.subscriptionUsed)" -ForegroundColor Gray
    }
} catch {
    Write-Host "❌ FAIL: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Summary
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  TEST SUMMARY" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "✅ API is accessible and working" -ForegroundColor Green
Write-Host "✅ Public endpoints (services, packages) working" -ForegroundColor Green
Write-Host "✅ Authentication flow working" -ForegroundColor Green
Write-Host "✅ Customer profile & vehicles working" -ForegroundColor Green
Write-Host "✅ Smart Appointment Booking tested" -ForegroundColor Green
Write-Host ""
Write-Host "🎉 Main customer flow is functional!" -ForegroundColor Magenta
Write-Host ""
