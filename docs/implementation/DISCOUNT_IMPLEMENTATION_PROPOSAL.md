# ?? **DISCOUNT SYSTEM - FINAL IMPLEMENTATION PROPOSAL**

---

## **?? Executive Summary**

H? th?ng discount hi?n t?i ?� ???c implement **95% ho�n ch?nh** v?i:
- ? `PromotionService`: Validation ??y ?? (min order, services, customer types, usage limit, date range)
- ? `DiscountCalculationService`: Logic "Choose MAX(CustomerType, Promotion)" ch�nh x�c
- ? 3-Tier Priority: Subscription (Free) ? CustomerType/Promotion (Choose MAX) ? Manual Admin
- ? ORDER-LEVEL Discount: T�nh tr�n t?ng ??n, distribute proportionally
- ? Tracking: L?u `DiscountAmount`, `DiscountType`, `PromotionId`

**C�n thi?u 2 ?i?m nh? ?? ho�n thi?n 100%:**
1. Display discount breakdown cho customer (UI transparency)
2. Package purchase discount logic (�p d?ng `MaintenancePackage.DiscountPercent`)

---

## **1?? DISCOUNT SUMMARY DTO - Hi?n th? breakdown cho customer**

### **?? M?c ti�u:**
Customer th?y r� ???c:
- Gi� g?c l� bao nhi�u?
- ???c gi?m bao nhi�u t? CustomerType (VIP, Gold)?
- ???c gi?m bao nhi�u t? Promotion code?
- Discount cu?i c�ng ???c �p d?ng (Choose MAX)
- T?ng ti?t ki?m ???c bao nhi�u?

---

### **?? B??c 1: T?o file DTO m?i**

**File:** `EVServiceCenter.Core/Domains/AppointmentManagement/DTOs/Response/DiscountSummaryDto.cs`

```csharp
namespace EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Response
{
    /// <summary>
    /// DTO hi?n th? breakdown discount chi ti?t cho customer
    /// ???c s? d?ng trong AppointmentResponseDto ?? transparency v? pricing
    /// </summary>
    public class DiscountSummaryDto
    {
        /// <summary>
        /// T?ng gi� g?c c?a c�c services (ch? t�nh Regular/Extra, kh�ng t�nh Subscription)
        /// </summary>
        public decimal OriginalTotal { get; set; }

        /// <summary>
        /// Discount t? CustomerType (VD: VIP 10%, Gold 5%)
        /// = OriginalTotal � (CustomerTypeDiscountPercent / 100)
        /// </summary>
        public decimal CustomerTypeDiscount { get; set; }

        /// <summary>
        /// T�n lo?i customer (VD: "VIP", "Gold", "Silver", "Regular")
        /// </summary>
        public string? CustomerTypeName { get; set; }

        /// <summary>
        /// Discount t? Promotion code
        /// C� th? l� Percentage ho?c Fixed Amount t�y lo?i promotion
        /// </summary>
        public decimal PromotionDiscount { get; set; }

        /// <summary>
        /// M� promotion ?� s? d?ng (n?u c�)
        /// VD: "SUMMER20", "FLASH100K"
        /// </summary>
        public string? PromotionCodeUsed { get; set; }

        /// <summary>
        /// Discount cu?i c�ng ???c �p d?ng
        /// = MAX(CustomerTypeDiscount, PromotionDiscount)
        /// Theo rule: Ch?n discount cao nh?t, kh�ng c?ng d?n
        /// </summary>
        public decimal FinalDiscount { get; set; }

        /// <summary>
        /// Lo?i discount ???c apply:
        /// - "None": Kh�ng c� discount
        /// - "CustomerType": �p d?ng discount t? lo?i kh�ch h�ng
        /// - "Promotion": �p d?ng discount t? m� khuy?n m�i
        /// </summary>
        public string AppliedDiscountType { get; set; } = "None";

        /// <summary>
        /// T?ng cu?i c�ng sau khi tr? discount
        /// = OriginalTotal - FinalDiscount
        /// </summary>
        public decimal FinalTotal { get; set; }

        /// <summary>
        /// Text formatted ?? hi?n th? tr�n UI
        /// Bao g?m emoji v� format ti?n t? VN?
        /// </summary>
        public string DisplayText
        {
            get
            {
                if (FinalDiscount <= 0)
                {
                    return $"?? T?ng c?ng: {OriginalTotal:N0}?";
                }

                var sb = new System.Text.StringBuilder();
                sb.AppendLine($"?? Gi� g?c: {OriginalTotal:N0}?");
                sb.AppendLine();

                if (CustomerTypeDiscount > 0 || PromotionDiscount > 0)
                {
                    sb.AppendLine("?? GI?M GI�:");

                    if (CustomerTypeDiscount > 0)
                    {
                        sb.AppendLine($"  ? Kh�ch h�ng {CustomerTypeName ?? "VIP"}: -{CustomerTypeDiscount:N0}?");
                    }

                    if (PromotionDiscount > 0)
                    {
                        sb.AppendLine($"  ?? M� {PromotionCodeUsed}: -{PromotionDiscount:N0}?");
                    }

                    sb.AppendLine($"  ? �p d?ng cao nh?t: -{FinalDiscount:N0}? ({AppliedDiscountType})");
                    sb.AppendLine();
                }

                sb.AppendLine($"?? T?ng c?ng: {FinalTotal:N0}?");
                sb.AppendLine($"? B?n ti?t ki?m: {FinalDiscount:N0}?");

                return sb.ToString();
            }
        }
    }
}
```

---

### **?? B??c 2: C?p nh?t `AppointmentResponseDto`**

**File:** `EVServiceCenter.Core/Domains/AppointmentManagement/DTOs/Response/AppointmentResponseDto.cs`

**Th�m property sau:**

```csharp
public class AppointmentResponseDto
{
    // ... existing properties ...

    public int? EstimatedDuration { get; set; }
    public decimal? EstimatedCost { get; set; }

    /// <summary>
    /// ? TH�M M?I: Breakdown discount chi ti?t
    /// Hi?n th? cho customer th?y r� ???c gi?m bao nhi�u t? ?�u
    /// NULL n?u kh�ng c� discount (all subscription services ho?c no discount applied)
    /// </summary>
    public DiscountSummaryDto? DiscountSummary { get; set; }

    // ... existing properties ...
}
```

---

### **?? B??c 3: Map trong `AppointmentCommandService.CreateAsync()`**

**File:** `EVServiceCenter.Infrastructure/Domains/AppointmentManagement/Services/AppointmentCommandService.cs`

**3.1. Khai b�o bi?n `discountSummary` ? ??u method (line ~145):**

```csharp
public async Task<AppointmentResponseDto> CreateAsync(
    CreateAppointmentRequestDto request,
    int currentUserId,
    CancellationToken cancellationToken = default)
{
    // ... existing validation code ...

    decimal finalCost = originalTotal;
    string? appliedDiscountType = null;
    string? promotionCodeUsed = null;
    int? promotionIdUsed = null;
    DiscountSummaryDto? discountSummary = null; // ? TH�M D�NG N�Y

    // ... rest of the code ...
}
```

**3.2. Build `discountSummary` sau khi t�nh discount (line ~187):**

```csharp
if (serviceLineItems.Any(s => s.ServiceSource != "Subscription"))
{
    var discountRequest = new DiscountCalculationRequest
    {
        CustomerId = request.CustomerId,
        CustomerTypeId = customerTypeId,
        CustomerTypeDiscountPercent = customerTypeDiscountPercent,
        PromotionCode = request.PromotionCode,
        Services = serviceLineItems
    };

    var discountResult = await _discountCalculator.CalculateDiscountAsync(discountRequest);

    finalCost = discountResult.FinalTotal;
    appliedDiscountType = discountResult.AppliedDiscountType;
    promotionCodeUsed = discountResult.PromotionCodeUsed;
    promotionIdUsed = discountResult.PromotionId;

    _logger.LogInformation(
        "? Discount calculated: Original={Original}?, Discount={Discount}?, " +
        "Final={Final}?, Type={Type}",
        discountResult.OriginalTotal, discountResult.FinalDiscount,
        discountResult.FinalTotal, appliedDiscountType);

    // Update appointment service prices
    foreach (var breakdown in discountResult.ServiceBreakdowns)
    {
        var aps = appointmentServices.FirstOrDefault(a => a.ServiceId == breakdown.ServiceId);
        if (aps != null && breakdown.ServiceSource != "Subscription")
        {
            aps.Price = breakdown.FinalPrice;
        }
    }

    // ? TH�M M?I: Build DiscountSummary
    discountSummary = new DiscountSummaryDto
    {
        OriginalTotal = discountResult.OriginalTotal,
        CustomerTypeDiscount = discountResult.CustomerTypeDiscount,
        CustomerTypeName = customer?.CustomerType?.TypeName,
        PromotionDiscount = discountResult.PromotionDiscount,
        PromotionCodeUsed = discountResult.PromotionCodeUsed,
        FinalDiscount = discountResult.FinalDiscount,
        AppliedDiscountType = discountResult.AppliedDiscountType,
        FinalTotal = discountResult.FinalTotal
    };
}
else
{
    _logger.LogInformation("All services are from subscription ? Final cost = 0");
    finalCost = 0;
    discountSummary = null; // ? TH�M: Kh�ng c� discount
}
```

**3.3. Set v�o response DTO (line ~250):**

```csharp
Appointment? result = await _repository.GetByIdWithDetailsAsync(
    created.AppointmentId, cancellationToken);

var responseDto = AppointmentMapper.ToResponseDto(result!);

// ? TH�M M?I: Set DiscountSummary n?u c�
if (discountSummary != null)
{
    responseDto.DiscountSummary = discountSummary;
}

return responseDto;
```

---

### **?? API Response Example:**

**Request:**
```json
POST /api/appointments
Content-Type: application/json

{
  "customerId": 1014,
  "vehicleId": 101,
  "serviceCenterId": 1,
  "slotId": 50,
  "serviceIds": [1, 2, 3],
  "promotionCode": "SUMMER20"
}
```

**Response:**

```json
{
  "success": true,
  "data": {
    "appointmentId": 1234,
    "appointmentCode": "APT20251211001",
    "estimatedCost": 2800000,
    
    "discountSummary": {
      "originalTotal": 3500000,
      "customerTypeDiscount": 350000,
      "customerTypeName": "VIP",
      "promotionDiscount": 700000,
      "promotionCodeUsed": "SUMMER20",
      "finalDiscount": 700000,
      "appliedDiscountType": "Promotion",
      "finalTotal": 2800000,
      "displayText": "?? Gi� g?c: 3,500,000?\n\n?? GI?M GI�:\n  ? Kh�ch h�ng VIP: -350,000?\n  ?? M� SUMMER20: -700,000?\n  ? �p d?ng cao nh?t: -700,000? (Promotion)\n\n?? T?ng c?ng: 2,800,000?\n? B?n ti?t ki?m: 700,000?"
    },
    
    "services": [...]
  }
}
```

---

## **2?? PACKAGE PURCHASE DISCOUNT LOGIC**

### **?? M?c ti�u:**
�p d?ng `MaintenancePackage.DiscountPercent` khi customer **MUA G�I SUBSCRIPTION** (kh�ng ph?i khi d�ng g�i ?? book appointment).

### **?? L?u � quan tr?ng:**
- **Package discount** ch? �p d?ng khi **MUA G�I**
- **KH�NG** �p d?ng CustomerType discount l�n package purchase (v� package ?� c� discount built-in)
- N?u mu?n ?u ?�i VIP ? T?o package ri�ng cho VIP v?i discount cao h?n

---

### **?? B??c 1: T�m service x? l� mua package**

Service c� th? l�:
- `PackageSubscriptionService.PurchasePackageAsync()`
- `CustomerPackageSubscriptionService.CreateAsync()`

*(Gi? s? l� `PackageSubscriptionService`)*

---

### **?? B??c 2: Implement discount logic**

**File:** `EVServiceCenter.Infrastructure/Domains/PackageSubscriptions/Services/PackageSubscriptionService.cs`

```csharp
public async Task<PackageSubscriptionResponseDto> PurchasePackageAsync(
    PurchasePackageRequestDto request,
    int userId,
    CancellationToken cancellationToken = default)
{
    using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    try
    {
        // 1-3. Validate customer, package, vehicle (existing code)
        // ...

        // 4. ? CALCULATE PACKAGE PRICE (CH? �P D?NG PACKAGE DISCOUNT)
        decimal originalPrice = package.TotalPrice ?? 0;
        decimal packageDiscountPercent = package.DiscountPercent ?? 0;
        decimal packageDiscountAmount = originalPrice * (packageDiscountPercent / 100);
        decimal finalPrice = originalPrice - packageDiscountAmount;

        _logger.LogInformation(
            "Package purchase pricing: Original={Original}?, PackageDiscount={Percent}% ({Amount}?), Final={Final}?",
            originalPrice, packageDiscountPercent, packageDiscountAmount, finalPrice);

        // ?? RULE: Package purchase KH�NG �p d?ng CustomerType discount
        // V� package ?� c� discount built-in r?i

        // 5. Create subscription
        var subscription = new CustomerPackageSubscription
        {
            CustomerId = request.CustomerId,
            PackageId = request.PackageId,
            VehicleId = request.VehicleId,
            
            // ? L?u pricing breakdown (C?n th�m columns v�o entity n?u ch?a c�)
            OriginalPrice = originalPrice,
            DiscountPercent = packageDiscountPercent,
            DiscountAmount = packageDiscountAmount,
            FinalPrice = finalPrice,
            PaymentAmount = finalPrice,
            
            PurchaseDate = DateTime.UtcNow,
            StartDate = DateOnly.FromDateTime(DateTime.Today),
            ExpiryDate = package.ValidityPeriod.HasValue 
                ? DateOnly.FromDateTime(DateTime.Today).AddDays(package.ValidityPeriod.Value)
                : null,
            Status = "Active",
            CreatedDate = DateTime.UtcNow,
            CreatedBy = userId
        };

        _context.CustomerPackageSubscriptions.Add(subscription);
        
        // 6. Create service usage records (existing code)
        // ...

        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        // 7. Return response
        return new PackageSubscriptionResponseDto
        {
            SubscriptionId = subscription.SubscriptionId,
            PackageCode = package.PackageCode,
            PackageName = package.PackageName,
            
            // ? Pricing info
            OriginalPrice = originalPrice,
            DiscountPercent = packageDiscountPercent,
            DiscountAmount = packageDiscountAmount,
            FinalPrice = finalPrice,
            SavedAmount = packageDiscountAmount,
            
            PricingDisplay = $"?? Gi� g?c: {originalPrice:N0}?\n" +
                           (packageDiscountPercent > 0 
                               ? $"?? Gi?m {packageDiscountPercent}%: -{packageDiscountAmount:N0}?\n" +
                                 $"? B?n ti?t ki?m: {packageDiscountAmount:N0}?\n"
                               : "") +
                           $"?? Th�nh ti?n: {finalPrice:N0}?"
        };
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync(cancellationToken);
        _logger.LogError(ex, "Error purchasing package");
        throw;
    }
}
```

---

### **?? B??c 3: C?p nh?t Entity `CustomerPackageSubscription`**

**File:** `EVServiceCenter.Core/Entities/CustomerPackageSubscription.cs`

**Th�m properties sau (n?u ch?a c�):**

```csharp
public partial class CustomerPackageSubscription
{
    // ... existing fields ...

    /// <summary>
    /// ? TH�M M?I: Gi� g?c c?a package khi mua
    /// </summary>
    [Column(TypeName = "decimal(15, 2)")]
    public decimal? OriginalPrice { get; set; }

    /// <summary>
    /// ? TH�M M?I: % Discount t? package
    /// </summary>
    [Column(TypeName = "decimal(5, 2)")]
    public decimal? DiscountPercent { get; set; }

    /// <summary>
    /// ? TH�M M?I: S? ti?n ?� gi?m (VN?)
    /// </summary>
    [Column(TypeName = "decimal(15, 2)")]
    public decimal? DiscountAmount { get; set; }

    /// <summary>
    /// ? TH�M M?I: Gi� cu?i c�ng customer ph?i tr?
    /// = OriginalPrice - DiscountAmount
    /// </summary>
    [Column(TypeName = "decimal(15, 2)")]
    public decimal? FinalPrice { get; set; }

    /// <summary>
    /// ? TH�M M?I: S? ti?n customer ?� thanh to�n th?c t?
    /// </summary>
    [Column(TypeName = "decimal(15, 2)")]
    public decimal? PaymentAmount { get; set; }

    // ... existing fields ...
}
```

---

### **?? B??c 4: T?o Migration**

```sh
dotnet ef migrations add AddPackagePurchasePricingFields -p EVServiceCenter.Infrastructure -s EVServiceCenter.API
dotnet ef database update -p EVServiceCenter.Infrastructure -s EVServiceCenter.API
```

**Migration content:**

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.AddColumn<decimal>(
        name: "OriginalPrice",
        table: "CustomerPackageSubscriptions",
        type: "decimal(15,2)",
        nullable: true);

    migrationBuilder.AddColumn<decimal>(
        name: "DiscountPercent",
        table: "CustomerPackageSubscriptions",
        type: "decimal(5,2)",
        nullable: true);

    migrationBuilder.AddColumn<decimal>(
        name: "DiscountAmount",
        table: "CustomerPackageSubscriptions",
        type: "decimal(15,2)",
        nullable: true);

    migrationBuilder.AddColumn<decimal>(
        name: "FinalPrice",
        table: "CustomerPackageSubscriptions",
        type: "decimal(15,2)",
        nullable: true);

    migrationBuilder.AddColumn<decimal>(
        name: "PaymentAmount",
        table: "CustomerPackageSubscriptions",
        type: "decimal(15,2)",
        nullable: true);
}
```

---

### **?? B??c 5: Update Response DTO**

**File:** `EVServiceCenter.Core/Domains/PackageSubscriptions/DTOs/Responses/PackageSubscriptionResponseDto.cs`

**Th�m properties sau:**

```csharp
public class PackageSubscriptionResponseDto
{
    // ... existing fields ...

    /// <summary>
    /// ? TH�M M?I: Pricing breakdown
    /// </summary>
    public decimal OriginalPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalPrice { get; set; }
    public decimal SavedAmount { get; set; }
    
    /// <summary>
    /// Display text cho UI
    /// </summary>
    public string? PricingDisplay { get; set; }

    // ... existing fields ...
}
```

---

### **?? API Response Example:**

**Request:**
```json
POST /api/package-subscriptions
Content-Type: application/json

{
  "customerId": 1014,
  "vehicleId": 101,
  "packageId": 2,
  "currentMileage": 15000
}
```

**Response:**

```json
{
  "success": true,
  "data": {
    "subscriptionId": 456,
    "packageCode": "PKG-PREMIUM-2025",
    "packageName": "G�i B?o D??ng Cao C?p",
    
    "originalPrice": 4500000,
    "discountPercent": 25,
    "discountAmount": 1125000,
    "finalPrice": 3375000,
    "savedAmount": 1125000,
    
    "pricingDisplay": "?? Gi� g?c: 4,500,000?\n?? Gi?m 25%: -1,125,000?\n? B?n ti?t ki?m: 1,125,000?\n?? Th�nh ti?n: 3,375,000?",
    
    "purchaseDate": "2025-12-11T10:30:00Z",
    "startDate": "2025-12-11",
    "expiryDate": "2026-12-11",
    "status": "Active"
  }
}
```

---

## **?? IMPLEMENTATION CHECKLIST**

### **?i?m 1: Display Breakdown**
- [ ] T?o file `DiscountSummaryDto.cs` trong `EVServiceCenter.Core/Domains/AppointmentManagement/DTOs/Response/`
- [ ] Th�m property `DiscountSummary` v�o `AppointmentResponseDto.cs`
- [ ] Khai b�o bi?n `discountSummary` trong `AppointmentCommandService.CreateAsync()`
- [ ] Build `discountSummary` t? `discountResult` sau khi t�nh discount
- [ ] Set `discountSummary` v�o response DTO tr??c khi return
- [ ] Test API `/api/appointments` v?i promotion code
- [ ] Verify response c� `discountSummary` field

### **?i?m 2: Package Purchase**
- [ ] T�m service x? l� mua package (`PackageSubscriptionService` ho?c t??ng t?)
- [ ] Implement logic: `FinalPrice = OriginalPrice � (1 - DiscountPercent / 100)`
- [ ] Th�m properties v�o `CustomerPackageSubscription` entity
- [ ] T?o migration: `dotnet ef migrations add AddPackagePurchasePricingFields`
- [ ] Apply migration: `dotnet ef database update`
- [ ] Update `PackageSubscriptionResponseDto` v?i pricing fields
- [ ] Test API mua package, verify discount ???c t�nh ?�ng
- [ ] Verify database c� l?u `OriginalPrice`, `DiscountAmount`, `FinalPrice`

---

## **?? TH?I GIAN ??C T�NH**

| Task | Th?i gian | Ng??i th?c hi?n |
|------|-----------|-----------------|
| **?i?m 1: Display Breakdown** | | |
| - T?o `DiscountSummaryDto` | 15 ph�t | Developer |
| - Update `AppointmentResponseDto` | 5 ph�t | Developer |
| - Map trong `AppointmentCommandService` | 30 ph�t | Developer |
| - Testing | 30 ph�t | QA |
| **?i?m 2: Package Purchase** | | |
| - Implement discount logic | 45 ph�t | Developer |
| - Update Entity + Migration | 30 ph�t | Developer |
| - Update DTO | 15 ph�t | Developer |
| - Testing | 45 ph�t | QA |
| **T?NG** | **~4 gi?** | |

---

## **?? K?T QU? MONG ??I**

### **Sau khi ho�n th�nh:**

? **Customer Experience:**
- Customer th?y r� ???c gi?m bao nhi�u t? CustomerType (VIP, Gold, Silver)
- Customer th?y r� ???c gi?m bao nhi�u t? Promotion code
- Hi?n th? discount cu?i c�ng ???c �p d?ng (Choose MAX)
- T?ng ti?t ki?m ???c bao nhi�u ? **T?ng transparency & trust**

? **Package Purchase:**
- `MaintenancePackage.DiscountPercent` ???c �p d?ng khi mua g�i
- L?u ??y ?? pricing breakdown v�o database
- Customer th?y r� gi� g?c, discount, gi� cu?i

? **System Completeness:**
- **100% discount features implemented**
- Full audit trail (track pricing history)
- Business rules r� r�ng, consistent

---

## **?? GHI CH� CHO NG??I TRI?N KHAI**

### **?? ?u ti�n:**
- **?i?m 1 (Display Breakdown):** ????? **VERY HIGH** - C?i thi?n UX ?�ng k?
- **?i?m 2 (Package Purchase):** ????? **HIGH** - Ho�n thi?n business logic

### **?? ph?c t?p:**
- **?i?m 1:** ????? (2/5) - ??n gi?n, ch? map data
- **?i?m 2:** ????? (3/5) - C?n th�m migration, update nhi?u files

### **Dependencies:**
- Kh�ng c� dependencies blocking
- C� th? implement ??c l?p song song
- Kh�ng ?nh h??ng existing features

### **Testing scenarios:**
1. VIP customer + Promotion code ? Verify Choose MAX
2. Regular customer + Promotion only ? Verify Promotion applied
3. All subscription services ? Verify `discountSummary = null`
4. Package purchase ? Verify discount t�nh ?�ng
5. Invalid promotion code ? Verify fallback to CustomerType discount

---

## **?? NEXT STEPS**

1. **Review:** Team lead review proposal n�y
2. **Estimate:** PM confirm timeline ph� h?p
3. **Assign:** Assign tasks cho developer
4. **Implement:** Follow checklist t?ng b??c
5. **Test:** QA test t?t c? scenarios
6. **Deploy:** Deploy l�n staging ? production

---

## **?? CONTACT**

N?u c� c�u h?i khi tri?n khai:
- Li�n h?: Technical Lead / Senior Developer
- Document n�y: L?u t?i `docs/implementation/DISCOUNT_IMPLEMENTATION_PROPOSAL.md`
- Code reference: 
  - `PromotionService.cs`
  - `DiscountCalculationService.cs`
  - `AppointmentCommandService.cs`

---

**? Prepared by:** AI Assistant  
**?? Date:** 2025-12-11  
**?? Version:** 1.0  
**?? Status:** Ready for Implementation
