# ? **V?N ??: VIP CUSTOMER KH�NG C� DISCOUNT TRONG APPOINTMENT**

**Date:** 2025-01-13  
**Issue:** Customer VIP (15% discount) nh?ng appointment kh�ng c� discount  
**Status:** ?? **ANALYZING**

---

## **?? PH�N T�CH V?N ??**

### **Response hi?n t?i:**
```json
{
  "appointmentId": 1,
  "customerId": 1014,
  "customerName": "Ph?m Nh?t Ngh?a",  // ? VIP customer
  "estimatedCost": 2100000,            // ? Gi� KH�NG gi?m
  "discountSummary": null,             // ? ? NULL
  "services": [
    { "serviceId": 1, "price": 600000, "serviceSource": "Regular" },
    { "serviceId": 2, "price": 1200000, "serviceSource": "Regular" },
    { "serviceId": 3, "price": 300000, "serviceSource": "Regular" }
  ]
}
```

### **Expected v?i VIP 15%:**
```json
{
  "estimatedCost": 1785000,            // ? 2,100,000 - 15% = 1,785,000
  "discountSummary": {
    "originalTotal": 2100000,
    "customerTypeDiscount": 315000,    // ? 15% c?a 2,100,000
    "finalDiscount": 315000,
    "finalTotal": 1785000,
    "appliedDiscountType": "CustomerType"
  }
}
```

---

## **?? NGUY�N NH�N TI?M ?N**

### **Hypothesis 1: Appointment ???c t?o TR??C KHI c� discount logic**
- AppointmentID = 1 c� th? ???c seeded TR??C
- Seeder KH�NG apply discount
- `DiscountAmount` = NULL, `DiscountType` = NULL

### **Hypothesis 2: CustomerType KH�NG ACTIVE khi t?o**
- N?u `CustomerTypes.IsActive = 0` khi seed appointment
- `CreateAsync()` s? KH�NG l?y ???c discount t? CustomerType
- K?t qu?: `customerTypeDiscountPercent` = NULL ? Kh�ng t�nh discount

### **Hypothesis 3: Discount Calculator tr? v? 0**
- `DiscountCalculationService` c� bug
- Ho?c `customerTypeDiscountPercent` NULL ? Kh�ng t�nh ???c

---

## **?? C�CH KI?M TRA**

### **Step 1: Check database**
```sql
-- Run file: docs/fixes/CHECK_APPOINTMENT_DISCOUNT.sql

-- Expected Output:
-- AppointmentID | EstimatedCost | DiscountAmount | DiscountType | DiscountStatus
-- 1             | 2100000       | NULL           | NULL         | ? NO DISCOUNT

-- CustomerID | TypeName | DiscountPercent | IsActive | TypeStatus
-- 1014       | VIP      | 15              | 1        | ? ACTIVE
```

**N?u:**
- `DiscountAmount` = NULL ? Appointment ???c t?o kh�ng c� discount
- `TypeStatus` = ? INACTIVE ? CustomerType ch?a active khi t?o

### **Step 2: Test t?o appointment M?I**
```http
POST https://localhost:7077/api/appointments
Authorization: Bearer {{customerToken}}
Content-Type: application/json

{
  "customerId": 1014,
  "vehicleId": 7,
  "serviceCenterId": 1,
  "slotId": 50,
  "serviceIds": [1, 2, 3],
  "customerNotes": "Test VIP discount",
  "priority": "Normal",
  "source": "Online"
}
```

**Expected Response:**
```json
{
  "success": true,
  "data": {
    "estimatedCost": 1785000,      // ? Ph?i gi?m 15%
    "discountSummary": {
      "originalTotal": 2100000,
      "customerTypeDiscount": 315000,
      "finalDiscount": 315000,
      "finalTotal": 1785000,
      "appliedDiscountType": "CustomerType"
    }
  }
}
```

**N?u discount v?n = 0:**
- Check `DiscountCalculationService` c� ???c inject ?�ng kh�ng
- Check log: "?? Discount calculated:" c� xu?t hi?n kh�ng
- Check `customerTypeDiscountPercent` c� gi� tr? kh�ng

---

## **? GI?I PH�P**

### **Solution 1: RE-SEED appointment v?i discount**

#### **Option A: Update existing appointment (Quick fix)**
```sql
-- Fix appointment #1 v?i VIP 15% discount
DECLARE @OriginalCost DECIMAL(18,2) = 2100000;
DECLARE @DiscountPercent DECIMAL(5,2) = 15;
DECLARE @DiscountAmount DECIMAL(18,2) = @OriginalCost * (@DiscountPercent / 100.0);
DECLARE @FinalCost DECIMAL(18,2) = @OriginalCost - @DiscountAmount;

UPDATE Appointments
SET 
    EstimatedCost = @FinalCost,           -- 1,785,000
    DiscountAmount = @DiscountAmount,      -- 315,000
    DiscountType = 'CustomerType'
WHERE AppointmentID = 1;

-- Verify
SELECT 
    AppointmentID,
    EstimatedCost,
    DiscountAmount,
    DiscountType,
    CONCAT('Gi?m gi�: ', CAST(DiscountAmount AS VARCHAR), '? (', 
           CAST((DiscountAmount * 100.0 / (EstimatedCost + DiscountAmount)) AS DECIMAL(5,2)), '%)') AS DiscountInfo
FROM Appointments
WHERE AppointmentID = 1;
```

#### **Option B: Fix AppointmentMapper ?? show discount t? DB**
```csharp
// In AppointmentMapper.ToDetailResponseDto()

// ? Build DiscountSummary t? Appointment entity (n?u c� discount trong DB)
DiscountSummary = appointment.DiscountAmount.HasValue && appointment.DiscountAmount > 0
    ? new DiscountSummaryDto
    {
        OriginalTotal = (appointment.EstimatedCost ?? 0) + appointment.DiscountAmount.Value,
        CustomerTypeDiscount = appointment.DiscountType == "CustomerType" 
            ? appointment.DiscountAmount.Value 
            : 0,
        PromotionDiscount = appointment.DiscountType == "Promotion" 
            ? appointment.DiscountAmount.Value 
            : 0,
        FinalDiscount = appointment.DiscountAmount.Value,
        AppliedDiscountType = appointment.DiscountType ?? "None",
        FinalTotal = appointment.EstimatedCost ?? 0,
        PromotionCodeUsed = appointment.Promotion?.PromotionCode,
        CustomerTypeName = appointment.Customer?.CustomerType?.TypeName
    }
    : null
```

### **Solution 2: Fix CreateAsync() logic (n?u discount kh�ng ???c t�nh)**

Check c�c ?i?u ki?n:
1. ? Customer c� CustomerType active?
2. ? `customerTypeDiscountPercent` c� gi� tr??
3. ? `DiscountCalculationService` ???c inject?
4. ? Services c� ServiceSource = "Regular" ho?c "Extra"?

---

## **?? ACTION ITEMS**

### **Immediate (Ngay l?p t?c):**
- [ ] Run `CHECK_APPOINTMENT_DISCOUNT.sql` ?? x�c nh?n v?n ??
- [ ] Check CustomerType IsActive = 1 (run `FIX_CUSTOMER_TYPE_ACTIVE.sql`)
- [ ] Test t?o appointment M?I ?? verify discount logic

### **Short-term (Trong ng�y):**
- [ ] Fix appointment #1 b?ng SQL UPDATE (Option A)
- [ ] Ho?c fix AppointmentMapper (Option B)
- [ ] Test l?i API `/api/appointments/1` ? Ph?i c� discountSummary

### **Long-term (Sau demo):**
- [ ] Review t?t c? appointments c? ? Update discount n?u thi?u
- [ ] Add migration ?? ensure `DiscountAmount` lu�n ???c set
- [ ] Add unit tests cho discount calculation

---

## **?? TEST PLAN**

### **Test Case 1: Verify existing appointment #1**
```http
GET https://localhost:7077/api/appointments/1
Authorization: Bearer {{customerToken}}
```

**Before fix:**
- `discountSummary` = null
- `estimatedCost` = 2,100,000

**After fix:**
- `discountSummary` c� gi� tr?
- `estimatedCost` = 1,785,000

### **Test Case 2: Create NEW appointment**
```http
POST https://localhost:7077/api/appointments
Body: {
  "customerId": 1014,
  "vehicleId": 7,
  "serviceIds": [1, 2, 3],
  "slotId": 50
}
```

**Expected:**
- `discountSummary.customerTypeDiscount` = 315,000
- `discountSummary.finalTotal` = 1,785,000

### **Test Case 3: Non-VIP customer**
```http
POST https://localhost:7077/api/appointments
Body: {
  "customerId": 1000,  // Regular customer (no discount)
  "vehicleId": X,
  "serviceIds": [1, 2, 3],
  "slotId": 50
}
```

**Expected:**
- `discountSummary` = null HO?C
- `discountSummary.finalDiscount` = 0

---

## **?? NOTES**

### **Why appointment #1 c� th? kh�ng c� discount:**

1. **Seeder t?o tr??c discount logic:**
   - Seeders ch?y tr??c khi discount logic ???c implement
   - `AppointmentSeeder` (n?u c�) KH�NG g?i `CreateAsync()` ? Kh�ng apply discount
   - Ch? INSERT tr?c ti?p v�o DB

2. **CustomerType inactive khi seed:**
   - N?u `CustomerTypes.IsActive = 0` khi seed appointment
   - `CreateAsync()` s? skip discount calculation

3. **Appointment #1 l� test data:**
   - ???c t?o manually b?ng SQL INSERT
   - KH�NG ?i qua business logic

---

## **? RECOMMENDATION**

### **Quick Fix cho demo:**
```sql
-- Run ngay tr??c demo
UPDATE Appointments
SET 
    EstimatedCost = 1785000,
    DiscountAmount = 315000,
    DiscountType = 'CustomerType'
WHERE AppointmentID = 1 AND CustomerID = 1014;
```

### **Proper Fix sau demo:**
1. Update t?t c? appointments c? thi?u discount
2. Ensure `CreateAsync()` lu�n t�nh discount cho VIP
3. Add mapper ?? show discount t? DB
4. Add unit tests

---

**Status:** ? **PENDING FIX**  
**Priority:** ?? **HIGH** (?nh h??ng demo)  
**ETA:** 15 minutes

---

**Next Steps:**
1. Run SQL check
2. Apply quick fix
3. Test API
4. Update mapper if needed
