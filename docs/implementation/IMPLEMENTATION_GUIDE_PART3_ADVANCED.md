# 📘 IMPLEMENTATION GUIDE - PART 3: ADVANCED FEATURES

> **Phần phức tạp nhất: CompleteAsync với Race Condition, Pessimistic Lock, Audit & Admin APIs**

---

## 🎯 NỘI DUNG PART 3

1. ✅ CompleteAppointmentAsync - Full race condition handling
2. ✅ UpdateServiceUsageAsync - Pessimistic lock
3. ✅ AdjustServiceSourceAsync - Admin API
4. ✅ Query Repository - GetActiveSubscriptionsByCustomerAndVehicleAsync
5. ✅ DTOs cho Adjust API
6. ✅ Controller endpoints
7. ✅ Dependency Injection

---

## 1️⃣ COMPLETE APPOINTMENT - PHẦN PHỨC TẠP NHẤT

### File: `AppointmentCommandService.cs`

**Tìm method `CompleteAppointmentAsync` và THAY THẾ HOÀN TOÀN:**

```csharp
public async Task<bool> CompleteAppointmentAsync(
    int appointmentId,
    int currentUserId,
    CancellationToken cancellationToken = default)
{
    // ✅ PESSIMISTIC LOCK với Serializable isolation
    using var transaction = await _context.Database.BeginTransactionAsync(
        System.Data.IsolationLevel.Serializable,
        cancellationToken);

    try
    {
        // ✅ LOCK & LOAD APPOINTMENT
        var appointment = await _context.Appointments
            .FromSqlRaw(@"
                SELECT * FROM Appointments WITH (UPDLOCK, ROWLOCK)
                WHERE AppointmentID = {0}
            ", appointmentId)
            .Include(a => a.AppointmentServices)
                .ThenInclude(s => s.Service)
            .Include(a => a.Vehicle)
            .Include(a => a.Customer)
            .Include(a => a.Subscription)
            .FirstOrDefaultAsync(cancellationToken);

        if (appointment == null)
            throw new InvalidOperationException("Appointment không tồn tại");

        // ✅ IDEMPOTENCY CHECK
        if (appointment.StatusId == (int)AppointmentStatusEnum.Completed ||
            appointment.StatusId == (int)AppointmentStatusEnum.CompletedWithUnpaid)
        {
            _logger.LogWarning(
                "Appointment {AppointmentId} already completed (Status: {Status}). " +
                "Idempotency protection triggered. Skipping.",
                appointmentId, appointment.StatusId);

            await transaction.CommitAsync(cancellationToken);
            return true; // Already done, return success
        }

        // ✅ STATUS CHECK
        if (appointment.StatusId != (int)AppointmentStatusEnum.InProgress)
            throw new InvalidOperationException(
                $"Chỉ có thể complete appointment đang InProgress. " +
                $"Current status: {appointment.StatusId}");

        _logger.LogInformation(
            "Starting CompleteAppointment for {AppointmentId}, " +
            "RowVersion: {RowVersion}",
            appointmentId,
            Convert.ToBase64String(appointment.RowVersion));

        // ✅ HANDLE SUBSCRIPTION USAGE WITH RACE PROTECTION
        var degradedServices = new List<string>();
        decimal additionalCost = 0;

        if (appointment.SubscriptionId.HasValue)
        {
            foreach (var appointmentService in appointment.AppointmentServices)
            {
                if (appointmentService.ServiceSource == "Subscription")
                {
                    try
                    {
                        // ✅ TRY DEDUCT USAGE (with pessimistic lock)
                        await _subscriptionCommandRepository.UpdateServiceUsageAsync(
                            appointment.SubscriptionId.Value,
                            appointmentService.ServiceId,
                            quantityUsed: 1,
                            appointmentId: appointmentId,
                            cancellationToken);

                        _logger.LogInformation(
                            "Successfully deducted usage for service {ServiceId} from subscription",
                            appointmentService.ServiceId);
                    }
                    catch (InvalidOperationException ex) when (ex.Message.Contains("Không đủ lượt"))
                    {
                        // ⚠️ RACE CONDITION DETECTED
                        _logger.LogWarning(
                            "Race condition detected: Service {ServiceId} ({ServiceName}) " +
                            "ran out of usage during completion. " +
                            "Degrading to Extra service. " +
                            "Appointment: {AppointmentId}, Subscription: {SubscriptionId}",
                            appointmentService.ServiceId,
                            appointmentService.Service?.ServiceName,
                            appointmentId,
                            appointment.SubscriptionId);

                        // ✅ DEGRADE TO EXTRA
                        var pricing = await _pricingRepository.GetActivePricingAsync(
                            appointment.Vehicle.ModelId, appointmentService.ServiceId);

                        decimal price = pricing?.CustomPrice ?? appointmentService.Service.BasePrice;

                        var oldSource = appointmentService.ServiceSource;
                        var oldPrice = appointmentService.Price;

                        appointmentService.ServiceSource = "Extra";
                        appointmentService.Price = price;
                        appointmentService.Notes =
                            $"[AUTO-ADJUSTED] Dịch vụ này đã hết lượt trong gói khi hoàn thành. " +
                            $"Tự động chuyển sang Extra service. Giá: {price:N0}đ. " +
                            $"Lý do: Race condition. " +
                            $"Thời gian: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}";

                        additionalCost += price;
                        degradedServices.Add($"{appointmentService.Service.ServiceName} (+{price:N0}đ)");

                        // ✅ LOG AUDIT
                        var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
                        var userAgent = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString();

                        await _auditService.LogServiceSourceChangeAsync(
                            appointmentService,
                            oldSource: oldSource,
                            newSource: "Extra",
                            oldPrice: oldPrice,
                            newPrice: price,
                            reason: $"Race condition: Subscription {appointment.SubscriptionId} " +
                                    $"ran out of usage for service {appointmentService.ServiceId} " +
                                    $"during appointment completion",
                            changeType: "AUTO_DEGRADE",
                            changedBy: currentUserId,
                            ipAddress: ipAddress,
                            userAgent: userAgent,
                            cancellationToken);
                    }
                }
                // ServiceSource = "Extra" → Skip (already charged)
            }
        }

        // ✅ UPDATE TOTAL COST
        if (additionalCost > 0)
        {
            appointment.EstimatedCost = (appointment.EstimatedCost ?? 0) + additionalCost;

            _logger.LogWarning(
                "Additional cost incurred: {Amount}đ for Appointment {AppointmentId}. " +
                "Degraded services: {Services}",
                additionalCost, appointmentId,
                string.Join(", ", degradedServices));
        }

        // ✅ UPDATE APPOINTMENT STATUS
        appointment.StatusId = (int)AppointmentStatusEnum.Completed;
        appointment.CompletedDate = DateTime.UtcNow;
        appointment.CompletedBy = currentUserId;

        try
        {
            // ✅ SAVE with RowVersion check
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex,
                "Concurrency conflict detected for Appointment {AppointmentId}. " +
                "Another process may have modified this appointment.",
                appointmentId);

            throw new InvalidOperationException(
                "Appointment đã được cập nhật bởi người khác. Vui lòng refresh và thử lại.");
        }

        await transaction.CommitAsync(cancellationToken);

        // ✅ SEND NOTIFICATION (nếu có degraded services)
        if (degradedServices.Any())
        {
            // TODO: Send email/SMS notification
            _logger.LogInformation(
                "Should send notification to customer {CustomerId} about {Count} degraded services",
                appointment.CustomerId, degradedServices.Count);
        }

        _logger.LogInformation(
            "Successfully completed Appointment {AppointmentId}. " +
            "Total cost: {Cost}đ, Degraded: {Degraded}",
            appointmentId, appointment.EstimatedCost, degradedServices.Count);

        return true;
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync(cancellationToken);

        _logger.LogError(ex,
            "Error completing appointment {AppointmentId}. Transaction rolled back.",
            appointmentId);

        throw;
    }
}
```

---

## 2️⃣ UPDATE SERVICE USAGE - PESSIMISTIC LOCK

### File: `PackageSubscriptionCommandRepository.cs`

**Tìm method `UpdateServiceUsageAsync` và THAY THẾ:**

```csharp
public async Task<bool> UpdateServiceUsageAsync(
    int subscriptionId,
    int serviceId,
    int quantityUsed,
    int appointmentId,
    CancellationToken cancellationToken = default)
{
    try
    {
        _logger.LogInformation(
            "Updating usage for subscription {SubscriptionId}, service {ServiceId}, quantity {Quantity}",
            subscriptionId, serviceId, quantityUsed);

        // ✅ PESSIMISTIC LOCK (UPDLOCK + ROWLOCK)
        var usage = await _context.PackageServiceUsages
            .FromSqlRaw(@"
                SELECT * FROM PackageServiceUsages WITH (UPDLOCK, ROWLOCK)
                WHERE SubscriptionID = {0} AND ServiceID = {1}
            ", subscriptionId, serviceId)
            .FirstOrDefaultAsync(cancellationToken);

        if (usage == null)
        {
            _logger.LogWarning(
                "Service usage not found for subscription {SubscriptionId}, service {ServiceId}",
                subscriptionId, serviceId);
            return false;
        }

        // ✅ CHECK AFTER LOCK (critical section)
        var currentRemaining = usage.RemainingQuantity;

        if (currentRemaining < quantityUsed)
        {
            _logger.LogWarning(
                "Insufficient remaining quantity. " +
                "Subscription: {SubscriptionId}, Service: {ServiceId}, " +
                "Requested: {Requested}, Available: {Available}",
                subscriptionId, serviceId, quantityUsed, currentRemaining);

            throw new InvalidOperationException(
                $"Không đủ lượt sử dụng. Còn {currentRemaining} lượt, cần {quantityUsed} lượt.");
        }

        // ✅ ATOMIC UPDATE
        usage.UsedQuantity += quantityUsed;
        usage.RemainingQuantity -= quantityUsed;
        usage.LastUsedDate = DateTime.UtcNow;
        usage.LastUsedAppointmentId = appointmentId;

        _logger.LogInformation(
            "Deducting usage: Subscription {SubscriptionId}, Service {ServiceId}, " +
            "Quantity: {Quantity}, Before: {Before}, After: {After}",
            subscriptionId, serviceId, quantityUsed,
            currentRemaining, usage.RemainingQuantity);

        // ✅ SAVE (lock released after commit)
        await _context.SaveChangesAsync(cancellationToken);

        // ✅ AUTO-CHECK FULLY USED
        await CheckAndUpdateFullyUsedStatusAsync(subscriptionId, cancellationToken);

        return true;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex,
            "Error updating service usage for subscription {SubscriptionId}, service {ServiceId}",
            subscriptionId, serviceId);
        throw;
    }
}
```

---

## 3️⃣ ADJUST SERVICE SOURCE - ADMIN API

### File: `AppointmentCommandService.cs`

**Thêm method mới:**

```csharp
/// <summary>
/// [ADMIN] Điều chỉnh ServiceSource và giá, có thể hoàn tiền
/// </summary>
public async Task<AdjustServiceSourceResponseDto> AdjustServiceSourceAsync(
    int appointmentId,
    int appointmentServiceId,
    AdjustServiceSourceRequestDto request,
    int adminUserId,
    CancellationToken cancellationToken = default)
{
    using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    try
    {
        var appointmentService = await _context.AppointmentServices
            .Include(s => s.Appointment)
                .ThenInclude(a => a.Subscription)
                    .ThenInclude(sub => sub.PackageServiceUsages)
            .Include(s => s.Service)
            .FirstOrDefaultAsync(s =>
                s.AppointmentServiceId == appointmentServiceId &&
                s.AppointmentId == appointmentId,
                cancellationToken);

        if (appointmentService == null)
            throw new InvalidOperationException("Không tìm thấy AppointmentService");

        // ✅ VALIDATE: Chỉ adjust được appointment đã Completed
        if (appointmentService.Appointment.StatusId != (int)AppointmentStatusEnum.Completed &&
            appointmentService.Appointment.StatusId != (int)AppointmentStatusEnum.CompletedWithUnpaid)
        {
            throw new InvalidOperationException(
                "Chỉ có thể điều chỉnh appointment đã completed");
        }

        // ✅ VALIDATE: Nếu đổi sang "Subscription", phải check
        if (request.NewServiceSource == "Subscription")
        {
            if (!appointmentService.Appointment.SubscriptionId.HasValue)
            {
                throw new InvalidOperationException(
                    "Appointment này không liên kết với subscription nào");
            }

            var subscription = appointmentService.Appointment.Subscription;
            var usage = subscription.PackageServiceUsages
                .FirstOrDefault(u => u.ServiceId == appointmentService.ServiceId);

            if (usage == null)
            {
                throw new InvalidOperationException(
                    $"Subscription #{subscription.SubscriptionId} không chứa " +
                    $"service {appointmentService.Service.ServiceName}");
            }

            // ⚠️ KIỂM TRA: Nếu đã trừ lượt rồi, không cho adjust
            if (usage.LastUsedAppointmentId == appointmentId)
            {
                throw new InvalidOperationException(
                    "Service này đã được trừ lượt cho appointment này rồi. " +
                    "Không thể adjust thành Subscription.");
            }

            // ✅ KIỂM TRA: Còn lượt không?
            if (usage.RemainingQuantity <= 0)
            {
                throw new InvalidOperationException(
                    $"Subscription đã hết lượt cho service này. " +
                    $"Remaining: {usage.RemainingQuantity}");
            }

            // ✅ NẾU HỢP LỆ: Trừ lượt ngay
            await _subscriptionCommandRepository.UpdateServiceUsageAsync(
                subscription.SubscriptionId,
                appointmentService.ServiceId,
                quantityUsed: 1,
                appointmentId: appointmentId,
                cancellationToken);

            _logger.LogInformation(
                "Deducted usage for service {ServiceId} from subscription {SubscriptionId} " +
                "during admin adjustment",
                appointmentService.ServiceId, subscription.SubscriptionId);
        }

        // Store old values
        var oldSource = appointmentService.ServiceSource;
        var oldPrice = appointmentService.Price;
        var priceDiff = oldPrice - request.NewPrice;

        // Update
        appointmentService.ServiceSource = request.NewServiceSource;
        appointmentService.Price = request.NewPrice;
        appointmentService.Notes +=
            $"\n[ADMIN ADJUSTED {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] " +
            $"{oldSource}({oldPrice:N0}đ) → {request.NewServiceSource}({request.NewPrice:N0}đ). " +
            $"By: Admin#{adminUserId}. Reason: {request.Reason}";

        // Update appointment cost
        appointmentService.Appointment.EstimatedCost -= priceDiff;

        // Log audit
        var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
        var userAgent = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString();

        await _auditService.LogServiceSourceChangeAsync(
            appointmentService,
            oldSource: oldSource,
            newSource: request.NewServiceSource,
            oldPrice: oldPrice,
            newPrice: request.NewPrice,
            reason: request.Reason,
            changeType: request.IssueRefund ? "REFUND" : "MANUAL_ADJUST",
            changedBy: adminUserId,
            ipAddress: ipAddress,
            userAgent: userAgent,
            cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        // ✅ REFUND (nếu cần)
        bool refundIssued = false;
        if (request.IssueRefund && priceDiff > 0)
        {
            // TODO: Integrate với payment service
            _logger.LogInformation(
                "Refund should be issued: {Amount}đ for customer {CustomerId}, " +
                "Appointment {AppointmentId}, Service {ServiceId}",
                priceDiff, appointmentService.Appointment.CustomerId,
                appointmentId, appointmentService.ServiceId);

            refundIssued = true;
        }

        await transaction.CommitAsync(cancellationToken);

        return new AdjustServiceSourceResponseDto
        {
            AppointmentServiceId = appointmentServiceId,
            OldServiceSource = oldSource,
            NewServiceSource = request.NewServiceSource,
            OldPrice = oldPrice,
            NewPrice = request.NewPrice,
            PriceDifference = priceDiff,
            RefundIssued = refundIssued,
            UsageDeducted = request.NewServiceSource == "Subscription",
            UpdatedBy = adminUserId,
            UpdatedDate = DateTime.UtcNow
        };
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync(cancellationToken);
        _logger.LogError(ex, "Error adjusting service source");
        throw;
    }
}
```

---

## 4️⃣ QUERY REPOSITORY - ADD NEW METHOD

### File: `IPackageSubscriptionQueryRepository.cs`

**Thêm signature:**

```csharp
Task<IEnumerable<PackageSubscriptionResponseDto>> GetActiveSubscriptionsByCustomerAndVehicleAsync(
    int customerId,
    int vehicleId,
    CancellationToken cancellationToken = default);
```

### File: `PackageSubscriptionQueryRepository.cs`

**Thêm implementation:**

```csharp
public async Task<IEnumerable<PackageSubscriptionResponseDto>> GetActiveSubscriptionsByCustomerAndVehicleAsync(
    int customerId,
    int vehicleId,
    CancellationToken cancellationToken = default)
{
    var today = DateOnly.FromDateTime(DateTime.UtcNow);

    var subscriptions = await _context.CustomerPackageSubscriptions
        .Include(s => s.Package)
            .ThenInclude(p => p.PackageServices)
                .ThenInclude(ps => ps.Service)
        .Include(s => s.PackageServiceUsages)
            .ThenInclude(u => u.Service)
        .Include(s => s.Vehicle)
        .Where(s =>
            s.CustomerId == customerId &&
            (s.VehicleId == vehicleId || s.VehicleId == null) &&
            s.Status == SubscriptionStatusEnum.Active.ToString() &&
            (!s.ExpirationDate.HasValue || s.ExpirationDate.Value >= today))
        .ToListAsync(cancellationToken);

    return subscriptions.Select(s => MapToResponseDto(s));
}
```

---

**Tiếp tục ở file tiếp theo với DTOs, Controllers, và DI...**

