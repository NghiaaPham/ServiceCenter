# Test Subscription Booking Flow
$baseUrl = "http://localhost:5153"

# Step 1: Login
Write-Host "Step 1: Logging in..." -ForegroundColor Yellow
$loginBody = @{
    email = "nghiadaucau1@gmail.com"
    password = "nghiadaucau123@"
} | ConvertTo-Json

$loginResponse = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method Post -ContentType "application/json" -Body $loginBody
$token = $loginResponse.data.token
Write-Host "Login successful! Token: $($token.Substring(0,50))..." -ForegroundColor Green

# Step 2: Get subscription details (ID 8)
Write-Host "`nStep 2: Getting subscription details (ID 8)..." -ForegroundColor Yellow
$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

$subscriptionResponse = Invoke-RestMethod -Uri "$baseUrl/api/package-subscriptions/8" -Method Get -Headers $headers
Write-Host "Subscription found:" -ForegroundColor Green
Write-Host "  - ID: $($subscriptionResponse.data.subscriptionId)"
Write-Host "  - Code: $($subscriptionResponse.data.subscriptionCode)"
Write-Host "  - Package: $($subscriptionResponse.data.packageName)"
Write-Host "  - Status: $($subscriptionResponse.data.status)"
Write-Host "  - Remaining Services: $($subscriptionResponse.data.remainingServices)"

# Step 3: Get available time slots
Write-Host "`nStep 3: Getting available time slots..." -ForegroundColor Yellow
$date = (Get-Date).AddDays(3).ToString("yyyy-MM-dd")
$slotsResponse = Invoke-RestMethod -Uri "$baseUrl/api/time-slots/available?serviceCenterId=2&date=$date" -Method Get
Write-Host "Found $($slotsResponse.data.Count) available slots for $date" -ForegroundColor Green
$slotId = $slotsResponse.data[0].slotId
Write-Host "Using first slot: ID=$slotId, Time=$($slotsResponse.data[0].startTime)-$($slotsResponse.data[0].endTime)"

# Step 4: Create appointment with subscription
Write-Host "`nStep 4: Creating appointment with subscription..." -ForegroundColor Yellow
$appointmentBody = @{
    customerId = 1014
    vehicleId = 7
    serviceCenterId = 2
    slotId = $slotId
    subscriptionId = 8
    appointmentDate = $date
    customerNotes = "Test booking with VIP subscription package"
    preferredServices = @()
} | ConvertTo-Json

Write-Host "Request body:"
Write-Host $appointmentBody

try {
    $appointmentResponse = Invoke-RestMethod -Uri "$baseUrl/api/appointments" -Method Post -Headers $headers -Body $appointmentBody
    Write-Host "`n=== APPOINTMENT CREATED SUCCESSFULLY ===" -ForegroundColor Green
    Write-Host "Appointment ID: $($appointmentResponse.data.appointmentId)"
    Write-Host "Appointment Code: $($appointmentResponse.data.appointmentCode)"
    Write-Host "Status: $($appointmentResponse.data.status)"
    Write-Host "Date: $($appointmentResponse.data.appointmentDate)"
    Write-Host "Time Slot: $($appointmentResponse.data.timeSlot)"
    Write-Host "Using Subscription: $($appointmentResponse.data.subscriptionCode)"
    Write-Host "Estimated Cost: $($appointmentResponse.data.estimatedCost) VND"
    Write-Host "Discount: $($appointmentResponse.data.discountAmount) VND"
    Write-Host "Final Cost: $($appointmentResponse.data.finalCost) VND"
} catch {
    Write-Host "`nERROR creating appointment:" -ForegroundColor Red
    Write-Host $_.Exception.Message
    if ($_.ErrorDetails) {
        Write-Host $_.ErrorDetails.Message
    }
}
