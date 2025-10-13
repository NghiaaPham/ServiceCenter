# ? **PRE-DEMO CHECKLIST - CUSTOMER MAIN FLOW**

**Demo Date:** Tomorrow (Ng�y mai)  
**Time:** 2025-01-14  
**Status:** ? **READY**

---

## **?? BEFORE DEMO - ACTION ITEMS**

### **1. Database Preparation** ?
```sql
-- Run this SQL script BEFORE demo
USE EVServiceCenterDB;

-- 1. Activate all CustomerTypes
UPDATE CustomerTypes SET IsActive = 1 WHERE IsActive = 0 OR IsActive IS NULL;

-- 2. Verify test customer exists
SELECT * FROM Customers WHERE Email = 'nghiadaucau1@gmail.com';

-- 3. Check packages are active
SELECT * FROM MaintenancePackages WHERE Status = 'Active';

-- 4. Verify time slots seeded
SELECT * FROM TimeSlots;

-- 5. Check test vehicle exists
SELECT * FROM CustomerVehicles WHERE CustomerID = 1014;
```

### **2. API Service** ?
```bash
# Kill existing process
./kill-and-run.bat

# OR manually
dotnet run --project EVServiceCenter.API
```

### **3. Test API Endpoints** ?
```bash
# Open Swagger
https://localhost:7077/swagger

# Verify these endpoint groups exist:
- Customer - Profile
- Customer - Vehicles
- Customer - Package Subscriptions ? NEW
- Customer - Appointments
- Maintenance Packages
```

---

## **?? DEMO FLOW CHECKLIST**

### **STEP 1: Login** ?
- [ ] URL: `POST /api/auth/login`
- [ ] Body: `{ "email": "nghiadaucau1@gmail.com", "password": "YourPassword123!" }`
- [ ] Expected: Token received, role = "Customer"

### **STEP 2: View Profile** ?
- [ ] URL: `GET /api/customer/profile/me`
- [ ] Header: `Authorization: Bearer {token}`
- [ ] Expected: 
  - `age` = 19 (not 0)
  - `customerType.typeName` = "VIP"
  - `potentialDiscount` = 15 (not 0)
  - `loyaltyPoints` = 0

### **STEP 3: View Vehicles** ?
- [ ] URL: `GET /api/customer/profile/my-vehicles`
- [ ] Expected: List of vehicles (c� th? empty n?u ch?a register)

### **STEP 4: Register Vehicle** (Optional)
- [ ] URL: `POST /api/customer/profile/my-vehicles`
- [ ] Body: 
```json
{
  "modelId": 1,
  "licensePlate": "30A-12345",
  "vin": "JM1BM1W79E1234567",
  "color": "Tr?ng",
  "purchaseDate": "2024-01-15",
  "mileage": 15000
}
```

### **STEP 5: Browse Packages** ?
- [ ] URL: `GET /api/maintenance-packages?status=Active`
- [ ] Expected: List of active packages

### **STEP 6: View Package Details** ?
- [ ] URL: `GET /api/maintenance-packages/2`
- [ ] Expected: 
  - `originalPriceBeforeDiscount` = 3,000,000
  - `discountPercent` = 15
  - `totalPriceAfterDiscount` = 2,550,000
  - `pricingDisplay` has breakdown

### **STEP 7: Purchase Package** ? **CRITICAL**
- [ ] URL: `POST /api/package-subscriptions/purchase`
- [ ] Body:
```json
{
  "packageId": 2,
  "vehicleId": 101,
  "customerNotes": "Mu?n ??t l?ch v�o bu?i s�ng"
}
```
- [ ] Expected: 
  - Status: 201 Created
  - `subscriptionId` c� gi� tr?
  - `originalPrice` = 3,000,000
  - `discountPercent` = 15
  - `discountAmount` = 450,000
  - `pricePaid` = 2,550,000
  - `serviceUsages` c� items
  - M?i service c� `remainingQuantity` = `totalAllowedQuantity`

### **STEP 8: View My Subscriptions** ? **CRITICAL**
- [ ] URL: `GET /api/package-subscriptions/my-subscriptions`
- [ ] Expected: Array of subscriptions

### **STEP 9: View Subscription Details** ?
- [ ] URL: `GET /api/package-subscriptions/{subscriptionId}`
- [ ] Expected: Full details with service usages

### **STEP 10: View Available Time Slots** ?
- [ ] URL: `GET /api/time-slots/available?date=2025-10-15`
- [ ] Expected: List of available slots

### **STEP 11: Book Appointment** ?
- [ ] URL: `POST /api/appointments`
- [ ] Body:
```json
{
  "vehicleId": 101,
  "appointmentDate": "2025-10-15",
  "timeSlotId": 1,
  "subscriptionId": 123,
  "serviceIds": [1, 2],
  "notes": "Nh? ki?m tra k? phanh"
}
```
- [ ] Expected:
  - `subscriptionId` = 123
  - `totalEstimatedPrice` = 0 (using subscription)
  - Each service has `usesSubscription` = true

### **STEP 12: View My Appointments** ?
- [ ] URL: `GET /api/appointments/my-appointments`
- [ ] Expected: List of appointments

---

## **?? POTENTIAL ISSUES & FIXES**

### **Issue 1: CustomerType inactive**
**Symptom:** `potentialDiscount` = 0  
**Fix:** Run SQL:
```sql
UPDATE CustomerTypes SET IsActive = 1;
```

### **Issue 2: No packages found**
**Symptom:** Empty array from `/api/maintenance-packages`  
**Fix:** Run seeder:
```bash
dotnet run --project EVServiceCenter.API
# Seeders run automatically in Development mode
```

### **Issue 3: No time slots**
**Symptom:** Empty array from `/api/time-slots/available`  
**Fix:** Check TimeSlots table:
```sql
SELECT * FROM TimeSlots;
```

### **Issue 4: Vehicle not found**
**Symptom:** "Kh�ng t�m th?y xe v?i ID: 101"  
**Fix:** Register vehicle first (STEP 4)

### **Issue 5: JWT token expired**
**Symptom:** 401 Unauthorized  
**Fix:** Login again (STEP 1)

### **Issue 6: Appointment fields null** ? **FIXED**
**Symptom:** `createdByName`, `updatedByName`, `workOrders` = null  
**Fix:** ? Already fixed in `AppointmentMapper.cs` and `AppointmentRepository.cs`  
**Details:** See `/docs/fixes/APPOINTMENT_NULL_FIELDS_FIX_COMPLETE.md`

---

## **?? DEMO PRESENTATION TIPS**

### **1. Highlight Key Features:**
- ? **Discount Calculation:** Show pricing breakdown (3,000,000 ? 2,550,000)
- ? **Service Usage Tracking:** Show "C�n 3/3 l??t" for each service
- ? **Free Services:** When using subscription, services = 0?
- ? **Security:** Customer ch? xem ???c data c?a m�nh

### **2. Show Flow Naturally:**
```
"H�m nay t�i s? demo flow c?a kh�ch h�ng t? login ??n ??t l?ch b?o d??ng"

1. "??u ti�n, kh�ch h�ng login v�o h? th?ng"
   ? Show token received

2. "Sau khi login, kh�ch xem profile c?a m�nh"
   ? Highlight: VIP customer, c� discount 15%

3. "Kh�ch h�ng c� th? xem v� ??ng k� xe"
   ? Show vehicle list (ho?c register new)

4. "Ti?p theo, kh�ch browse c�c g�i b?o d??ng"
   ? Show package list
   ? Click 1 package ? Show pricing breakdown

5. "Kh�ch h�ng th?y g�i ph� h?p v� quy?t ??nh mua"
   ? POST purchase ? Show subscription created
   ? Highlight: Gi?m gi� 450,000? t? VIP type

6. "Sau khi mua, kh�ch c� th? xem subscriptions ?� mua"
   ? GET my-subscriptions
   ? Show usage details: "C�n 3 l??t thay d?u, 3 l??t ki?m tra phanh"

7. "B�y gi? kh�ch mu?n ??t l?ch b?o d??ng"
   ? GET time slots ? Choose slot
   ? POST appointment v?i subscriptionId
   ? Highlight: Services = FREE (using subscription)

8. "Cu?i c�ng, kh�ch xem l?i l?ch h?n ?� ??t"
   ? GET my-appointments
   ? Show appointment details
```

### **3. Handle Questions:**

**Q: "N?u customer mua 2 l?n c�ng 1 g�i cho c�ng 1 xe th� sao?"**  
A: "H? th?ng s? block, b�o l?i: 'B?n ?� c� subscription active cho g�i n�y r?i'"

**Q: "L�m sao track vi?c s? d?ng services?"**  
A: "M?i subscription c� b?ng PackageServiceUsages tracking t?ng service ?� d�ng/c�n l?i"

**Q: "N?u customer h?y subscription th� sao?"**  
A: "Customer c� th? cancel qua API /cancel v?i l� do. Status s? chuy?n sang Cancelled"

---

## **?? FINAL CHECKS (5 minutes before demo)**

- [ ] API ?ang ch?y: `https://localhost:7077/swagger`
- [ ] Database c� data: Check Customers, Packages, TimeSlots
- [ ] Test account OK: `nghiadaucau1@gmail.com` / `YourPassword123!`
- [ ] HTTP test file ready: `test/PACKAGE_SUBSCRIPTION_TEST.http`
- [ ] Browser tabs:
  - Tab 1: Swagger UI
  - Tab 2: SQL Server Management Studio (optional)
  - Tab 3: VS Code with HTTP file
- [ ] Screen recording ready (optional)

---

## **?? SUCCESS CRITERIA**

Demo th�nh c�ng khi:
- ? Login v� get profile OK (age = 19, discount = 15%)
- ? Purchase package OK (subscription created v?i discount)
- ? View subscriptions OK (usage tracking hi?n th? ?�ng)
- ? Book appointment OK (v?i subscription, services = FREE)
- ? No errors during demo
- ? Pricing calculations ?�ng
- ? Security checks work (ownership verification)

---

**Good luck! Ch�c b?n demo th�nh c�ng! ????**

---

**Quick Reference:**
- Documentation: `/docs/implementation/PACKAGE_SUBSCRIPTION_MODULE_COMPLETE.md`
- Test File: `/test/PACKAGE_SUBSCRIPTION_TEST.http`
- Summary: `/docs/implementation/FINAL_IMPLEMENTATION_SUMMARY.md`
