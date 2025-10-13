# ?? **DISCOUNT SYSTEM - FINAL IMPLEMENTATION PROPOSAL**

---

## **?? Executive Summary**

H? th?ng discount hi?n t?i ?ã ???c implement **95% hoàn ch?nh** v?i:
- ? `PromotionService`: Validation ??y ?? (min order, services, customer types, usage limit, date range)
- ? `DiscountCalculationService`: Logic "Choose MAX(CustomerType, Promotion)" chính xác
- ? 3-Tier Priority: Subscription (Free) ? CustomerType/Promotion (Choose MAX) ? Manual Admin
- ? ORDER-LEVEL Discount: Tính trên t?ng ??n, distribute proportionally
- ? Tracking: L?u `DiscountAmount`, `DiscountType`, `PromotionId`

**Còn thi?u 2 ?i?m nh? ?? hoàn thi?n 100%:**
1. Display discount breakdown cho customer (UI transparency)
2. Package purchase discount logic (áp d?ng `MaintenancePackage.DiscountPercent`)

---

## **1?? DISCOUNT SUMMARY DTO - Hi?n th? breakdown cho customer**

### **?? M?c tiêu:**
Customer th?y rõ ???c:
- Giá g?c là bao nhiêu?
- ???c gi?m bao nhiêu t? CustomerType (VIP, Gold)?
- ???c gi?m bao nhiêu t? Promotion code?
- Discount cu?i cùng ???c áp d?ng (Choose MAX)
- T?ng ti?t ki?m ???c bao nhiêu?

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
        /// T?ng giá g?c c?a các services (ch? tính Regular/Extra, không tính Subscription)
        /// </summary>
        public decimal OriginalTotal { get; set; }

        /// <summary>
        /// Discount t? CustomerType (VD: VIP 10%, Gold 5%)
        /// = OriginalTotal × (CustomerTypeDiscountPercent / 100)
        /// </summary>
        public decimal CustomerTypeDiscount { get; set; }

        /// <summary>
        /// Tên lo?i customer (VD: "VIP", "Gold", "Silver", "Regular")
        /// </summary>
        public string? CustomerTypeName { get; set; }

        /// <summary>
        /// Discount t? Promotion code
        /// Có th? là Percentage ho?c Fixed Amount tùy lo?i promotion
        /// </summary>
        public decimal PromotionDiscount { get; set; }

        /// <summary>
        /// Mã promotion ?ã s? d?ng (n?u có)
        /// VD: "SUMMER20", "FLASH100K"
        /// </summary>
        public string? PromotionCodeUsed { get; set; }

        /// <summary>
        /// Discount cu?i cùng ???c áp d?ng
        /// = MAX(CustomerTypeDiscount, PromotionDiscount)
        /// Theo rule: Ch?n discount cao nh?t, không c?ng d?n
        /// </summary>
        public decimal FinalDiscount { get; set; }

        /// <summary>
        /// Lo?i discount ???c apply:
        /// - "None": Không có discount
        /// - "CustomerType": Áp d?ng discount t? lo?i khách hàng
        /// - "Promotion": Áp d?ng discount t? mã khuy?n mãi
        /// </summary>
        public string AppliedDiscountType { get; set; } = "None";

        /// <summary>
        /// T?ng cu?i cùng sau khi tr? discount
        /// = OriginalTotal - FinalDiscount
        /// </summary>
        public decimal FinalTotal { get; set; }

        /// <summary>
        /// Text formatted ?? hi?n th? trên UI
        /// Bao g?m emoji và format ti?n t? VN?
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
                sb.AppendLine($"?? Giá g?c: {OriginalTotal:N0}?");
                sb.AppendLine();

                if (CustomerTypeDiscount > 0 || PromotionDiscount > 0)
                {
                    sb.AppendLine("?? GI?M GIÁ:");

                    if (CustomerTypeDiscount > 0)
                    {
                        sb.AppendLine($"  ? Khách hàng {CustomerTypeName ?? "VIP"}: -{CustomerTypeDiscount:N0}?");
                    }

                    if (PromotionDiscount > 0)
                    {
                        sb.AppendLine($"  ?? Mã {PromotionCodeUsed}: -{PromotionDiscount:N0}?");
                    }

                    sb.AppendLine($"  ? Áp d?ng cao nh?t: -{FinalDiscount:N0}? ({AppliedDiscountType})");
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

**Thêm property sau:**

```csharp
public class AppointmentResponseDto
{
    // ... existing properties ...

    public int? EstimatedDuration { get; set; }
    public decimal? EstimatedCost { get; set; }

    /// <summary>
    /// ? THÊM M?I: Breakdown discount chi ti?t
    /// Hi?n th? cho customer th?y rõ ???c gi?m bao nhiêu t? ?âu
    /// NULL n?u không có discount (all subscription services ho?c no discount applied)
    /// </summary>
    public DiscountSummaryDto? DiscountSummary { get; set; }

    // ... existing properties ...
}
```

---

### **?? B??c 3: Map trong `AppointmentCommandService.CreateAsync()`**

**File:** `EVServiceCenter.Infrastructure/Domains/AppointmentManagement/Services/AppointmentCommandService.cs`

**3.1. Khai báo bi?n `discountSummary` ? ??u method (line ~145):**

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
    DiscountSummaryDto? discountSummary = null; // ? THÊM DÒNG NÀY

    // ... rest of the code ...
}
```

**3.2. Build `discountSummary` sau khi tính discount (line ~187):**

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

    // ? THÊM M?I: Build DiscountSummary
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
    discountSummary = null; // ? THÊM: Không có discount
}
```

**3.3. Set vào response DTO (line ~250):**

```csharp
Appointment? result = await _repository.GetByIdWithDetailsAsync(
    created.AppointmentId, cancellationToken);

var responseDto = AppointmentMapper.ToResponseDto(result!);

// ? THÊM M?I: Set DiscountSummary n?u có
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
      "displayText": "?? Giá g?c: 3,500,000?\n\n?? GI?M GIÁ:\n  ? Khách hàng VIP: -350,000?\n  ?? Mã SUMMER20: -700,000?\n  ? Áp d?ng cao nh?t: -700,000? (Promotion)\n\n?? T?ng c?ng: 2,800,000?\n? B?n ti?t ki?m: 700,000?"
    },
    
    "services": [...]
  }
}
```

---

## **2?? PACKAGE PURCHASE DISCOUNT LOGIC**

### **?? M?c tiêu:**
Áp d?ng `MaintenancePackage.DiscountPercent` khi customer **MUA GÓI SUBSCRIPTION** (không ph?i khi dùng gói ?? book appointment).

### **?? L?u ý quan tr?ng:**
- **Package discount** ch? áp d?ng khi **MUA GÓI**
- **KHÔNG** áp d?ng CustomerType discount lên package purchase (vì package ?ã có discount built-in)
- N?u mu?n ?u ?ãi VIP ? T?o package riêng cho VIP v?i discount cao h?n

---

### **?? B??c 1: Tìm service x? lý mua package**

Service có th? là:
- `PackageSubscriptionService.PurchasePackageAsync()`
- `CustomerPackageSubscriptionService.CreateAsync()`

*(Gi? s? là `PackageSubscriptionService`)*

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

        // 4. ? CALCULATE PACKAGE PRICE (CH? ÁP D?NG PACKAGE DISCOUNT)
        decimal originalPrice = package.TotalPrice ?? 0;
        decimal packageDiscountPercent = package.DiscountPercent ?? 0;
        decimal packageDiscountAmount = originalPrice * (packageDiscountPercent / 100);
        decimal finalPrice = originalPrice - packageDiscountAmount;

        _logger.LogInformation(
            "Package purchase pricing: Original={Original}?, PackageDiscount={Percent}% ({Amount}?), Final={Final}?",
            originalPrice, packageDiscountPercent, packageDiscountAmount, finalPrice);

        // ?? RULE: Package purchase KHÔNG áp d?ng CustomerType discount
        // Vì package ?ã có discount built-in r?i

        // 5. Create subscription
        var subscription = new CustomerPackageSubscription
        {
            CustomerId = request.CustomerId,
            PackageId = request.PackageId,
            VehicleId = request.VehicleId,
            
            // ? L?u pricing breakdown (C?n thêm columns vào entity n?u ch?a có)
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
            
            PricingDisplay = $"?? Giá g?c: {originalPrice:N0}?\n" +
                           (packageDiscountPercent > 0 
                               ? $"?? Gi?m {packageDiscountPercent}%: -{packageDiscountAmount:N0}?\n" +
                                 $"? B?n ti?t ki?m: {packageDiscountAmount:N0}?\n"
                               : "") +
                           $"?? Thành ti?n: {finalPrice:N0}?"
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

**Thêm properties sau (n?u ch?a có):**

```csharp
public partial class CustomerPackageSubscription
{
    // ... existing fields ...

    /// <summary>
    /// ? THÊM M?I: Giá g?c c?a package khi mua
    /// </summary>
    [Column(TypeName = "decimal(15, 2)")]
    public decimal? OriginalPrice { get; set; }

    /// <summary>
    /// ? THÊM M?I: % Discount t? package
    /// </summary>
    [Column(TypeName = "decimal(5, 2)")]
    public decimal? DiscountPercent { get; set; }

    /// <summary>
    /// ? THÊM M?I: S? ti?n ?ã gi?m (VN?)
    /// </summary>
    [Column(TypeName = "decimal(15, 2)")]
    public decimal? DiscountAmount { get; set; }

    /// <summary>
    /// ? THÊM M?I: Giá cu?i cùng customer ph?i tr?
    /// = OriginalPrice - DiscountAmount
    /// </summary>
    [Column(TypeName = "decimal(15, 2)")]
    public decimal? FinalPrice { get; set; }

    /// <summary>
    /// ? THÊM M?I: S? ti?n customer ?ã thanh toán th?c t?
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

**Thêm properties sau:**

```csharp
public class PackageSubscriptionResponseDto
{
    // ... existing fields ...

    /// <summary>
    /// ? THÊM M?I: Pricing breakdown
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
    "packageName": "Gói B?o D??ng Cao C?p",
    
    "originalPrice": 4500000,
    "discountPercent": 25,
    "discountAmount": 1125000,
    "finalPrice": 3375000,
    "savedAmount": 1125000,
    
    "pricingDisplay": "?? Giá g?c: 4,500,000?\n?? Gi?m 25%: -1,125,000?\n? B?n ti?t ki?m: 1,125,000?\n?? Thành ti?n: 3,375,000?",
    
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
- [ ] Thêm property `DiscountSummary` vào `AppointmentResponseDto.cs`
- [ ] Khai báo bi?n `discountSummary` trong `AppointmentCommandService.CreateAsync()`
- [ ] Build `discountSummary` t? `discountResult` sau khi tính discount
- [ ] Set `discountSummary` vào response DTO tr??c khi return
- [ ] Test API `/api/appointments` v?i promotion code
- [ ] Verify response có `discountSummary` field

### **?i?m 2: Package Purchase**
- [ ] Tìm service x? lý mua package (`PackageSubscriptionService` ho?c t??ng t?)
- [ ] Implement logic: `FinalPrice = OriginalPrice × (1 - DiscountPercent / 100)`
- [ ] Thêm properties vào `CustomerPackageSubscription` entity
- [ ] T?o migration: `dotnet ef migrations add AddPackagePurchasePricingFields`
- [ ] Apply migration: `dotnet ef database update`
- [ ] Update `PackageSubscriptionResponseDto` v?i pricing fields
- [ ] Test API mua package, verify discount ???c tính ?úng
- [ ] Verify database có l?u `OriginalPrice`, `DiscountAmount`, `FinalPrice`

---

## **?? TH?I GIAN ??C TÍNH**

| Task | Th?i gian | Ng??i th?c hi?n |
|------|-----------|-----------------|
| **?i?m 1: Display Breakdown** | | |
| - T?o `DiscountSummaryDto` | 15 phút | Developer |
| - Update `AppointmentResponseDto` | 5 phút | Developer |
| - Map trong `AppointmentCommandService` | 30 phút | Developer |
| - Testing | 30 phút | QA |
| **?i?m 2: Package Purchase** | | |
| - Implement discount logic | 45 phút | Developer |
| - Update Entity + Migration | 30 phút | Developer |
| - Update DTO | 15 phút | Developer |
| - Testing | 45 phút | QA |
| **T?NG** | **~4 gi?** | |

---

## **?? K?T QU? MONG ??I**

### **Sau khi hoàn thành:**

? **Customer Experience:**
- Customer th?y rõ ???c gi?m bao nhiêu t? CustomerType (VIP, Gold, Silver)
- Customer th?y rõ ???c gi?m bao nhiêu t? Promotion code
- Hi?n th? discount cu?i cùng ???c áp d?ng (Choose MAX)
- T?ng ti?t ki?m ???c bao nhiêu ? **T?ng transparency & trust**

? **Package Purchase:**
- `MaintenancePackage.DiscountPercent` ???c áp d?ng khi mua gói
- L?u ??y ?? pricing breakdown vào database
- Customer th?y rõ giá g?c, discount, giá cu?i

? **System Completeness:**
- **100% discount features implemented**
- Full audit trail (track pricing history)
- Business rules rõ ràng, consistent

---

## **?? GHI CHÚ CHO NG??I TRI?N KHAI**

### **?? ?u tiên:**
- **?i?m 1 (Display Breakdown):** ????? **VERY HIGH** - C?i thi?n UX ?áng k?
- **?i?m 2 (Package Purchase):** ????? **HIGH** - Hoàn thi?n business logic

### **?? ph?c t?p:**
- **?i?m 1:** ????? (2/5) - ??n gi?n, ch? map data
- **?i?m 2:** ????? (3/5) - C?n thêm migration, update nhi?u files

### **Dependencies:**
- Không có dependencies blocking
- Có th? implement ??c l?p song song
- Không ?nh h??ng existing features

### **Testing scenarios:**
1. VIP customer + Promotion code ? Verify Choose MAX
2. Regular customer + Promotion only ? Verify Promotion applied
3. All subscription services ? Verify `discountSummary = null`
4. Package purchase ? Verify discount tính ?úng
5. Invalid promotion code ? Verify fallback to CustomerType discount

---

## **?? NEXT STEPS**

1. **Review:** Team lead review proposal này
2. **Estimate:** PM confirm timeline phù h?p
3. **Assign:** Assign tasks cho developer
4. **Implement:** Follow checklist t?ng b??c
5. **Test:** QA test t?t c? scenarios
6. **Deploy:** Deploy lên staging ? production

---

## **?? CONTACT**

N?u có câu h?i khi tri?n khai:
- Liên h?: Technical Lead / Senior Developer
- Document này: L?u t?i `docs/implementation/DISCOUNT_IMPLEMENTATION_PROPOSAL.md`
- Code reference: 
  - `PromotionService.cs`
  - `DiscountCalculationService.cs`
  - `AppointmentCommandService.cs`

---

**? Prepared by:** AI Assistant  
**?? Date:** 2025-12-11  
**?? Version:** 1.0  
**?? Status:** Ready for Implementation
