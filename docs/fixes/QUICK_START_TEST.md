# ?? QUICK START: Test Appointment Fix

## ? **?Ã FIX XONG**

### **V?n ?? ?ã gi?i quy?t:**
- ? `customerId = 0` ? ? `customerId = 1014`
- ? `vehicleId = 0` ? ? `vehicleId = 7`
- ? `customerName = ""` ? ? `customerName = "Ph?m Nh?t Ngh?a"`
- ? `services = []` ? ? `services = [...]`

---

## ?? **TEST NGAY (5 PHÚT)**

### **B??c 1: Run App**
```bash
cd EVServiceCenter.API
dotnet run
```

### **B??c 2: Login**

**Swagger:** https://localhost:7077/swagger

**Or Postman:**
```http
POST https://localhost:7077/api/auth/login
Content-Type: application/json

{
  "username": "nghiadaucau1@gmail.com",
  "password": "Admin@123"
}
```

**Copy token t? response!**

### **B??c 3: Test API**

```http
GET https://localhost:7077/api/appointments/my-appointments
Authorization: Bearer {paste_token_here}
```

### **B??c 4: Check Response**

**Expected:**
```json
{
  "success": true,
  "data": [
    {
      "appointmentId": 1,        // ? > 0
      "customerId": 1014,        // ? > 0
      "customerName": "Ph?m...", // ? Has name
      "vehicleId": 7,            // ? > 0
      "vehicleName": "Hyundai...", // ? Has name
      "serviceCenterId": 1,      // ? > 0
      "services": [...]          // ? Has services
    }
  ]
}
```

---

## ? **VALIDATION CHECKLIST**

Quick check trong response:

- [ ] `customerId` > 0 ?
- [ ] `vehicleId` > 0 ?
- [ ] `serviceCenterId` > 0 ?
- [ ] `customerName` not empty ?
- [ ] `vehicleName` not empty ?
- [ ] `services` array populated ?
- [ ] Response time < 1s ?

**N?u t?t c? ? ? FIX THÀNH CÔNG!** ??

---

## ?? **TROUBLESHOOTING**

### **Issue 1: Build Error**
```bash
dotnet clean
dotnet build
```

### **Issue 2: IDs v?n = 0**
```bash
# Check database có data không
# Run SQL script:
sqlcmd -S localhost -d EVServiceCenterDB -i docs/fixes/VERIFY_APPOINTMENT_DATA.sql
```

### **Issue 3: Services = []**
```sql
-- Check trong SSMS:
SELECT COUNT(*) FROM AppointmentServices WHERE AppointmentID = 1;
```

---

## ?? **SQL PROFILER CHECK**

**Expected queries (with AsSplitQuery):**

1. `SELECT COUNT(*) FROM Appointments WHERE CustomerId = 1014`
2. `SELECT * FROM Appointments WHERE ...`
3. `SELECT * FROM Customers WHERE CustomerId IN (...)`
4. `SELECT * FROM CustomerVehicles WHERE VehicleId IN (...)`
5. `SELECT * FROM CarModels WHERE ModelId IN (...)`
6. `SELECT * FROM CarBrands WHERE BrandId IN (...)`
7. `SELECT * FROM ServiceCenters WHERE CenterId IN (...)`
8. `SELECT * FROM AppointmentServices WHERE AppointmentId IN (...)`

**? GOOD:** Multiple simple queries  
**? BAD:** One big JOIN query

---

## ?? **WHAT WAS CHANGED**

| File | Method | Change |
|------|--------|--------|
| `AppointmentQueryRepository.cs` | `GetPagedAsync()` | Projection ? Include |
| `AppointmentQueryRepository.cs` | `GetByCustomerIdAsync()` | Projection ? Include |

**Key Change:**
```csharp
// ? OLD
.Select(a => new Appointment { ... })

// ? NEW
.Include(a => a.Customer)
.Include(a => a.Vehicle).ThenInclude(...)
.Include(a => a.ServiceCenter)
.AsSplitQuery()
```

---

## ?? **SUCCESS CRITERIA**

**Fix hoàn thành khi:**

1. ? Build successful
2. ? API tr? v? IDs > 0
3. ? All names populated
4. ? Services array có data
5. ? Response time < 1s
6. ? SQL Profiler shows split queries

---

## ?? **NEED HELP?**

**Documents:**
- Technical: `docs/fixes/APPOINTMENT_NULL_IDS_FIX.md`
- Test Plan: `docs/fixes/APPOINTMENT_FIX_TEST_PLAN.md`
- SQL Verify: `docs/fixes/VERIFY_APPOINTMENT_DATA.sql`

**Quick Debug:**
```bash
# Check build
dotnet build --no-restore

# Check database
sqlcmd -S localhost -Q "SELECT TOP 5 * FROM Appointments"

# Check logs
tail -f logs/app.log
```

---

## ? **YOU'RE READY!**

1. Run app: `dotnet run`
2. Open Swagger: https://localhost:7077/swagger
3. Login ? Get token
4. Test `/api/appointments/my-appointments`
5. Verify IDs > 0

**Good luck! ??**
