# 📘 IMPLEMENTATION GUIDE - PART 2: CORE LOGIC

> **Phần này chứa logic phức tạp nhất: AppointmentCommandService**

---

## ⚠️ LƯU Ý QUAN TRỌNG

File `AppointmentCommandService.cs` hiện tại rất dài (>1000 lines).

Thay vì sửa trực tiếp, tôi khuyên bạn:
1. **Backup file gốc** trước
2. **Thêm các methods mới** vào cuối class
3. **Sửa methods hiện có** từng phần

---

## 🔧 FILE CẦN SỬA: AppointmentCommandService.cs

**Đường dẫn:** `EVServiceCenter.Infrastructure/Domains/AppointmentManagement/Services/AppointmentCommandService.cs`

---

### ➕ THÊM DEPENDENCIES

Tìm constructor của `AppointmentCommandService` và thêm:

```csharp
private readonly IServiceSourceAuditService _auditService;
private readonly IHttpContextAccessor _httpContextAccessor;
private readonly IPackageSubscriptionQueryRepository _subscriptionQueryRepository;

public AppointmentCommandService(
    // ... existing dependencies ...
    IServiceSourceAuditService auditService,  // ← ADD
    IHttpContextAccessor httpContextAccessor,  // ← ADD
    IPackageSubscriptionQueryRepository subscriptionQueryRepository  // ← ADD IF NOT EXISTS
)
{
    // ... existing assignments ...
    _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
    _httpContextAccessor = httpContextAccessor;
    _subscriptionQueryRepository = subscriptionQueryRepository;
}
```

---

### 🆕 THÊM HELPER METHOD: CalculateSubscriptionPriority

Thêm vào cuối class (trước closing brace):

```csharp
/// <summary>
/// Tính priority score cho subscription
/// Lower score = Higher priority
/// </summary>
private int CalculateSubscriptionPriority(
    PackageSubscriptionResponseDto subscription,
    PackageServiceUsageDto? usage = null)
{
    int score = 0;

    // Rule 1: Gói sắp hết hạn (weight = 10000)
    if (subscription.ExpiryDate.HasValue)
    {
        var daysUntilExpiry = (subscription.ExpiryDate.Value.ToDateTime(TimeOnly.MinValue) - DateTime.UtcNow).Days;

        if (daysUntilExpiry <= 7)
            score += 0; // Highest priority: < 7 days
        else if (daysUntilExpiry <= 30)
            score += 5000; // Medium priority: < 30 days
        else
            score += 10000; // Lower priority: > 30 days
    }
    else
    {
        score += 20000; // Không expiry → lowest priority
    }

    // Rule 2: Ít lượt còn lại hơn (weight = 100)
    if (usage != null)
    {
        score += usage.RemainingQuantity * 100;
    }

    // Rule 3: FIFO - Mua sớm hơn (weight = 1)
    if (subscription.PurchaseDate.HasValue)
    {
        var daysSincePurchase = (DateTime.UtcNow - subscription.PurchaseDate.Value).Days;
        score += Math.Max(0, 365 - daysSincePurchase); // Older = lower score
    }

    // Rule 4: Deterministic tiebreaker
    score += subscription.SubscriptionId % 10;

    return score;
}
```

---

### 🆕 THÊM HELPER METHOD: BuildAppointmentServicesAsync

```csharp
/// <summary>
/// Build danh sách AppointmentService với smart deduplication
/// </summary>
private async Task<(List<AppointmentService> services, decimal totalCost, int totalDuration)>
    BuildAppointmentServicesAsync(
        CreateAppointmentRequestDto request,
        int vehicleModelId,
        CancellationToken cancellationToken)
{
    var appointmentServices = new List<AppointmentService>();
    var servicesFromSubscription = new HashSet<int>();
    decimal totalCost = 0;
    int totalDuration = 0;

    // ========== BƯỚC 1: LẤY ACTIVE SUBSCRIPTIONS ==========
    List<PackageSubscriptionResponseDto> customerSubscriptions = new List<PackageSubscriptionResponseDto>();

    if (request.SubscriptionId.HasValue)
    {
        // Customer chọn 1 subscription cụ thể
        var subscription = await _subscriptionQueryRepository.GetSubscriptionByIdAsync(
            request.SubscriptionId.Value, cancellationToken);

        if (subscription != null)
        {
            ValidateSubscription(subscription, request);
            customerSubscriptions.Add(subscription);
        }
    }
    else
    {
        // AUTO-SELECT: Lấy TẤT CẢ active subscriptions
        customerSubscriptions = (await _subscriptionQueryRepository
            .GetActiveSubscriptionsByCustomerAndVehicleAsync(
                request.CustomerId,
                request.VehicleId,
                cancellationToken))
            .ToList();
    }

    // ========== BƯỚC 2: BUILD SERVICE USAGE MAP WITH PRIORITY ==========
    var serviceUsageMap = new Dictionary<int, List<(
        PackageSubscriptionResponseDto Subscription,
        PackageServiceUsageDto Usage,
        int Priority)>>();

    foreach (var subscription in customerSubscriptions)
    {
        foreach (var usage in subscription.ServiceUsages.Where(u => u.RemainingQuantity > 0))
        {
            if (!serviceUsageMap.ContainsKey(usage.ServiceId))
            {
                serviceUsageMap[usage.ServiceId] = new List<(PackageSubscriptionResponseDto, PackageServiceUsageDto, int)>();
            }

            int priority = CalculateSubscriptionPriority(subscription, usage);
            serviceUsageMap[usage.ServiceId].Add((subscription, usage, priority));
        }
    }

    // Sort by priority
    foreach (var serviceId in serviceUsageMap.Keys)
    {
        serviceUsageMap[serviceId] = serviceUsageMap[serviceId]
            .OrderBy(x => x.Priority)
            .ToList();
    }

    // ========== BƯỚC 3: XỬ LÝ SERVICE SELECTION ==========
    var selectedServiceIds = request.ServiceIds ?? new List<int>();

    if (!selectedServiceIds.Any() && customerSubscriptions.Any())
    {
        // Không chọn services → Dùng TẤT CẢ từ subscription priority cao nhất
        var primarySubscription = customerSubscriptions
            .OrderBy(s => CalculateSubscriptionPriority(s, null))
            .First();

        selectedServiceIds = primarySubscription.ServiceUsages
            .Where(u => u.RemainingQuantity > 0)
            .Select(u => u.ServiceId)
            .ToList();
    }

    foreach (var serviceId in selectedServiceIds)
    {
        // ✅ CHECK: Service có trong subscription nào không?
        if (serviceUsageMap.ContainsKey(serviceId) &&
            serviceUsageMap[serviceId].Any())
        {
            // Lấy subscription có priority CAO NHẤT
            var (bestSubscription, bestUsage, _) = serviceUsageMap[serviceId].First();

            var service = bestUsage.Service; // Assuming eager loaded

            appointmentServices.Add(new AppointmentService
            {
                ServiceId = serviceId,
                ServiceSource = "Subscription",
                Price = 0,
                EstimatedTime = service.StandardTime,
                Notes = $"Từ gói {bestSubscription.PackageName} " +
                        $"(Sub#{bestSubscription.SubscriptionId}): " +
                        $"Còn {bestUsage.RemainingQuantity}/{bestUsage.TotalAllowedQuantity} lượt"
            });

            totalDuration += service.StandardTime;
            servicesFromSubscription.Add(serviceId);

            _logger.LogInformation(
                "Service {ServiceId} matched with subscription {SubscriptionId} " +
                "(Priority winner among {Count} options)",
                serviceId, bestSubscription.SubscriptionId,
                serviceUsageMap[serviceId].Count);
        }
    }

    // ========== BƯỚC 4: EXTRA SERVICES ==========
    var extraServiceIds = selectedServiceIds
        .Where(id => !servicesFromSubscription.Contains(id))
        .ToList();

    if (extraServiceIds.Any())
    {
        var extraServices = await _serviceRepository.GetByIdsAsync(
            extraServiceIds, cancellationToken);

        foreach (var service in extraServices)
        {
            var pricing = await _pricingRepository.GetActivePricingAsync(
                vehicleModelId, service.ServiceId);

            decimal price = pricing?.CustomPrice ?? service.BasePrice;
            int time = pricing?.CustomTime ?? service.StandardTime;

            appointmentServices.Add(new AppointmentService
            {
                ServiceId = service.ServiceId,
                ServiceSource = request.SubscriptionId.HasValue ? "Extra" : "Regular",
                Price = price,
                EstimatedTime = time,
                Notes = request.SubscriptionId.HasValue
                    ? "Dịch vụ bổ sung ngoài gói (không có trong gói hoặc đã hết lượt)"
                    : null
            });

            totalCost += price;
            totalDuration += time;
        }

        _logger.LogInformation(
            "Added {Count} extra services, Total extra cost: {Cost}đ",
            extraServices.Count(), totalCost);
    }

    if (!appointmentServices.Any())
        throw new InvalidOperationException(
            "Không có dịch vụ nào được chọn hoặc gói dịch vụ đã hết lượt sử dụng");

    return (appointmentServices, totalCost, totalDuration);
}
```

---

### ✏️ SỬA METHOD: CreateAsync

Tìm method `CreateAsync` và **THAY THẾ** phần tính services bằng:

```csharp
// TÌM ĐOẠN NÀY (khoảng line 87-145):
// List<int> serviceIdsToBook = new List<int>(request.ServiceIds);
// ...

// THAY BẰNG:

var vehicle = await _vehicleRepository.GetByIdAsync(request.VehicleId, cancellationToken);
if (vehicle == null)
    throw new InvalidOperationException($"Không tìm thấy xe với ID: {request.VehicleId}");

// ✅ SỬ DỤNG HELPER METHOD MỚI
var (appointmentServices, totalCost, totalDuration) =
    await BuildAppointmentServicesAsync(request, vehicle.ModelId, cancellationToken);

// Tiếp tục với phần validate conflicts...
```

---

Còn nhiều nội dung, bạn muốn tôi tiếp tục với **CompleteAppointmentAsync** (phần phức tạp nhất với race condition handling) không?

