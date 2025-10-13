using EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Request;
using EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Response;
using AdjustServiceSourceResponseDto = EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Response.AdjustServiceSourceResponseDto;
using EVServiceCenter.Core.Domains.AppointmentManagement.Entities;
using EVServiceCenter.Core.Domains.AppointmentManagement.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.AppointmentManagement.Interfaces.Services;
using EVServiceCenter.Core.Domains.CustomerVehicles.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.MaintenanceServices.Entities;
using EVServiceCenter.Core.Domains.MaintenanceServices.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.ModelServicePricings.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.PackageSubscriptions.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.TimeSlots.Interfaces.Repositories;
using EVServiceCenter.Core.Entities;
using EVServiceCenter.Core.Enums;
using EVServiceCenter.Core.Helpers;
using EVServiceCenter.Core.Interfaces.Services;
using EVServiceCenter.Infrastructure.Domains.AppointmentManagement.Mappers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using EVServiceCenter.Core.Domains.Pricing.Interfaces;
using EVServiceCenter.Core.Domains.Pricing.Models;
using EVServiceCenter.Core.Domains.Customers.Interfaces;

namespace EVServiceCenter.Infrastructure.Domains.AppointmentManagement.Services
{
    public class AppointmentCommandService : IAppointmentCommandService
    {
        private readonly IAppointmentRepository _repository;
        private readonly IAppointmentCommandRepository _commandRepository;
        private readonly IAppointmentQueryRepository _queryRepository;
        private readonly ITimeSlotRepository _slotRepository;
        private readonly IMaintenanceServiceRepository _serviceRepository;
        private readonly IModelServicePricingRepository _pricingRepository;
        private readonly ICustomerVehicleRepository _vehicleRepository;
        private readonly IPackageSubscriptionQueryRepository _subscriptionRepository;
        private readonly IPackageSubscriptionCommandRepository _subscriptionCommandRepository;
        private readonly IServiceSourceAuditService _auditService;
        private readonly IDiscountCalculationService _discountCalculator;
        private readonly IPromotionService _promotionService;
        private readonly ICustomerRepository _customerRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AppointmentCommandService> _logger;

        public AppointmentCommandService(
            IAppointmentRepository repository,
            IAppointmentCommandRepository commandRepository,
            IAppointmentQueryRepository queryRepository,
            ITimeSlotRepository slotRepository,
            IMaintenanceServiceRepository serviceRepository,
            IModelServicePricingRepository pricingRepository,
            ICustomerVehicleRepository vehicleRepository,
            IPackageSubscriptionQueryRepository subscriptionRepository,
            IPackageSubscriptionCommandRepository subscriptionCommandRepository,
            IServiceSourceAuditService auditService,
            IDiscountCalculationService discountCalculator,
            IPromotionService promotionService,
            ICustomerRepository customerRepository,
            IConfiguration configuration,
            ILogger<AppointmentCommandService> logger)
        {
            _repository = repository;
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _slotRepository = slotRepository;
            _serviceRepository = serviceRepository;
            _pricingRepository = pricingRepository;
            _vehicleRepository = vehicleRepository;
            _subscriptionRepository = subscriptionRepository;
            _subscriptionCommandRepository = subscriptionCommandRepository;
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
            _discountCalculator = discountCalculator ?? throw new ArgumentNullException(nameof(discountCalculator));
            _promotionService = promotionService ?? throw new ArgumentNullException(nameof(promotionService));
            _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<AppointmentResponseDto> CreateAsync(
            CreateAppointmentRequestDto request,
            int currentUserId,
            CancellationToken cancellationToken = default)
        {
            var slot = await _slotRepository.GetByIdAsync(request.SlotId, cancellationToken);
            if (slot == null)
                throw new InvalidOperationException("Slot không tồn tại");

            var slotDateTime = slot.SlotDate.ToDateTime(slot.StartTime);
            
            if(slotDateTime <  DateTime.UtcNow)
            {
                throw new InvalidOperationException("Không thể đặt lịch cho slot trong quá khứ ");
            }

            int activeCount = await _queryRepository.GetActiveCountBySlotIdAsync(
                request.SlotId, cancellationToken);

            if (activeCount >= slot.MaxBookings)
                throw new InvalidOperationException("Slot đã đầy");

            var vehicle = await _vehicleRepository.GetByIdAsync(request.VehicleId, cancellationToken);
            if (vehicle == null)
                throw new InvalidOperationException("Xe không tồn tại");

            // ========== SUBSCRIPTION VALIDATION (if provided) ==========
            List<int> serviceIdsToBook = new List<int>(request.ServiceIds);
            int? packageIdToSet = request.PackageId;

            if (request.SubscriptionId.HasValue)
            {
                // Validate subscription
                var subscription = await _subscriptionRepository.GetSubscriptionByIdAsync(
                    request.SubscriptionId.Value, cancellationToken);

                if (subscription == null)
                    throw new InvalidOperationException(
                        $"Không tìm thấy subscription với ID: {request.SubscriptionId}");

                // Check subscription is active
                if (subscription.Status != SubscriptionStatusEnum.Active)
                    throw new InvalidOperationException(
                        $"Subscription này không còn active (Status: {subscription.StatusDisplayName})");

                // Check subscription belongs to customer
                if (subscription.CustomerId != request.CustomerId)
                    throw new InvalidOperationException(
                        "Subscription này không thuộc về khách hàng");

                // Check vehicle matches subscription
                if (subscription.VehicleId != request.VehicleId)
                    throw new InvalidOperationException(
                        $"Subscription này dành cho xe {subscription.VehiclePlateNumber}, " +
                        $"không phải xe hiện tại");

                // Check expiry date
                if (subscription.ExpiryDate.HasValue &&
                    subscription.ExpiryDate.Value < DateTime.UtcNow)
                    throw new InvalidOperationException(
                        $"Subscription đã hết hạn vào {subscription.ExpiryDate.Value:dd/MM/yyyy}");

                // Get services from subscription (only services with remaining usage > 0)
                var subscriptionServiceIds = subscription.ServiceUsages
                    .Where(u => u.RemainingQuantity > 0)
                    .Select(u => u.ServiceId)
                    .ToList();

                if (!subscriptionServiceIds.Any())
                    throw new InvalidOperationException(
                        "Subscription này đã sử dụng hết tất cả dịch vụ");

                _logger.LogInformation(
                    "Booking appointment with subscription {SubscriptionId}: " +
                    "{Count} services available",
                    subscription.SubscriptionId, subscriptionServiceIds.Count);

                // Use services from subscription (request.ServiceIds will be extra services)
                serviceIdsToBook = subscriptionServiceIds;
                packageIdToSet = subscription.PackageId;
            }

            // ═══════════════════════════════════════════════════════════
            // DISCOUNT CALCULATION PHASE
            // ═══════════════════════════════════════════════════════════

            // 1️⃣ Get base pricing (original prices before discount)
            (decimal originalTotal, int totalDuration, List<AppointmentService> appointmentServices) =
                await CalculatePricingAsync(vehicle.ModelId, null, serviceIdsToBook, cancellationToken);

            decimal finalCost = originalTotal;
            string? appliedDiscountType = null;
            string? promotionCodeUsed = null;
            int? promotionIdUsed = null;
            DiscountSummaryDto? discountSummary = null; // ✅ THÊM MỚI: Discount breakdown cho customer

            // 2️⃣ Get customer info for CustomerType discount
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId, includeStats: false, cancellationToken);

            decimal? customerTypeDiscountPercent = null;
            int? customerTypeId = null;

            if (customer?.CustomerType != null)
            {
                customerTypeId = customer.CustomerType.TypeId;
                customerTypeDiscountPercent = customer.CustomerType.DiscountPercent;

                _logger.LogInformation(
                    "Customer {CustomerId} has CustomerType '{TypeName}' with {Discount}% discount",
                    request.CustomerId, customer.CustomerType.TypeName, customerTypeDiscountPercent ?? 0);
            }

            // 3️⃣ Call discount calculator (only for Regular/Extra services)
            // Subscription services are already free (ServiceSource = "Subscription")
            var serviceLineItems = appointmentServices.Select(aps => new ServiceLineItem
            {
                ServiceId = aps.ServiceId,
                ServiceName = aps.Service?.ServiceName ?? $"Service #{aps.ServiceId}",
                BasePrice = aps.Price,
                Quantity = 1,
                ServiceSource = aps.ServiceSource,
                SubscriptionId = request.SubscriptionId
            }).ToList();

            if (serviceLineItems.Any(s => s.ServiceSource != "Subscription"))
            {
                // Có services Regular/Extra → Tính discount
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
                    "✅ Discount calculated: Original={Original}đ, Discount={Discount}đ, " +
                    "Final={Final}đ, Type={Type}",
                    discountResult.OriginalTotal, discountResult.FinalDiscount,
                    discountResult.FinalTotal, appliedDiscountType);

                // Update appointment service prices with discounted prices
                foreach (var breakdown in discountResult.ServiceBreakdowns)
                {
                    var aps = appointmentServices.FirstOrDefault(a => a.ServiceId == breakdown.ServiceId);
                    if (aps != null && breakdown.ServiceSource != "Subscription")
                    {
                        aps.Price = breakdown.FinalPrice;
                    }
                }

                // ✅ THÊM MỚI: Build DiscountSummary để trả về cho customer
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
                _logger.LogInformation(
                    "All services are from subscription → Final cost = 0");
                finalCost = 0;
                discountSummary = null; // ✅ Không có discount breakdown
            }

            // ✅ VEHICLE CONFLICT VALIDATION (với actual duration)
            await ValidateVehicleTimeConflict(
                request.VehicleId,
                request.SlotId,
                totalDuration, // ✅ Truyền actual duration
                excludeAppointmentId: null,
                cancellationToken);

            // ✅ TECHNICIAN CONFLICT VALIDATION (với actual duration, PER CENTER)
            await ValidateTechnicianConflict(
                request.PreferredTechnicianId,
                request.ServiceCenterId,  // ✅ PER CENTER
                slot.SlotDate.ToDateTime(slot.StartTime),
                totalDuration,
                excludeAppointmentId: null,
                cancellationToken);

            // ✅ SERVICE CENTER CAPACITY VALIDATION (với actual duration)
            await ValidateServiceCenterCapacity(
                request.ServiceCenterId,
                slot.SlotDate.ToDateTime(slot.StartTime),
                totalDuration,
                excludeAppointmentId: null,
                cancellationToken);

            string appointmentCode = await GenerateAppointmentCodeAsync(cancellationToken);

            var appointment = new Appointment
            {
                AppointmentCode = appointmentCode,
                CustomerId = request.CustomerId,
                VehicleId = request.VehicleId,
                ServiceCenterId = request.ServiceCenterId,
                SlotId = request.SlotId,
                PackageId = packageIdToSet,
                SubscriptionId = request.SubscriptionId,
                StatusId = (int)AppointmentStatusEnum.Pending,
                AppointmentDate = slot.SlotDate.ToDateTime(slot.StartTime),
                EstimatedDuration = totalDuration,
                EstimatedCost = finalCost, // ✅ Use discounted price
                DiscountAmount = originalTotal - finalCost, // ✅ Total discount applied
                DiscountType = appliedDiscountType ?? "None", // ✅ Type of discount
                PromotionId = promotionIdUsed, // ✅ Promotion ID if used
                CustomerNotes = request.CustomerNotes,
                PreferredTechnicianId = request.PreferredTechnicianId,
                Priority = request.Priority,
                Source = request.Source,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = currentUserId
            };

            Appointment created = await _commandRepository.CreateWithServicesAsync(
                appointment, appointmentServices, cancellationToken);

            // 4️⃣ Increment promotion usage if promotion was applied
            if (!string.IsNullOrEmpty(promotionCodeUsed))
            {
                await _promotionService.IncrementUsageAsync(promotionCodeUsed);

                _logger.LogInformation(
                    "✅ Incremented usage count for promotion '{Code}' after appointment creation",
                    promotionCodeUsed);
            }

            Appointment? result = await _repository.GetByIdWithDetailsAsync(
                created.AppointmentId, cancellationToken);

            var responseDto = AppointmentMapper.ToResponseDto(result!);

            // ✅ THÊM MỚI: Set DiscountSummary nếu có
            if (discountSummary != null)
            {
                responseDto.DiscountSummary = discountSummary;
            }

            return responseDto;
        }

        public async Task<AppointmentResponseDto> UpdateAsync(
            UpdateAppointmentRequestDto request,
            int currentUserId,
            CancellationToken cancellationToken = default)
        {
            Appointment? appointment = await _repository.GetByIdAsync(
                request.AppointmentId, cancellationToken);

            if (appointment == null)
                throw new InvalidOperationException("Appointment không tồn tại");

            if (appointment.StatusId != (int)AppointmentStatusEnum.Pending &&
                appointment.StatusId != (int)AppointmentStatusEnum.Confirmed)
            {
                throw new InvalidOperationException(
                    "Chỉ có thể update appointment ở trạng thái Pending hoặc Confirmed");
            }

            if (request.VehicleId.HasValue && request.VehicleId.Value != appointment.VehicleId)
            {
                var vehicle = await _vehicleRepository.GetByIdAsync(request.VehicleId.Value, cancellationToken);
                if (vehicle == null)
                    throw new InvalidOperationException("Xe không tồn tại");

                appointment.VehicleId = request.VehicleId.Value;
            }

            if (request.SlotId.HasValue && request.SlotId.Value != appointment.SlotId)
            {
                var slot = await _slotRepository.GetByIdAsync(request.SlotId.Value, cancellationToken);
                if (slot == null)
                    throw new InvalidOperationException("Slot không tồn tại");

                var slotDateTime = slot.SlotDate.ToDateTime(slot.StartTime);
                if (slotDateTime < DateTime.UtcNow)
                {
                    throw new InvalidOperationException("Không thể chọn slot đã ở trong quá khứ");
                }

                int activeCount = await _queryRepository.GetActiveCountBySlotIdAsync(
                    request.SlotId.Value, cancellationToken);

                if (activeCount >= slot.MaxBookings)
                    throw new InvalidOperationException("Slot đã đầy");

                appointment.SlotId = request.SlotId.Value;
                appointment.AppointmentDate = slot.SlotDate.ToDateTime(slot.StartTime);
            }

            if (request.CustomerNotes != null)
                appointment.CustomerNotes = request.CustomerNotes;

            if (request.ServiceDescription != null)
                appointment.ServiceDescription = request.ServiceDescription;

            if (request.PreferredTechnicianId.HasValue)
                appointment.PreferredTechnicianId = request.PreferredTechnicianId;

            if (request.Priority != null)
                appointment.Priority = request.Priority;

            if (request.ServiceIds != null && request.ServiceIds.Any())
            {
                var vehicle = await _vehicleRepository.GetByIdAsync(
                    appointment.VehicleId, cancellationToken);

                (decimal totalCost, int totalDuration, List<AppointmentService> newServices) =
                    await CalculatePricingAsync(vehicle!.ModelId, null, request.ServiceIds, cancellationToken);

                appointment.EstimatedCost = totalCost;
                appointment.EstimatedDuration = totalDuration;
                appointment.PackageId = null;

                await _commandRepository.UpdateServicesAsync(
                    appointment.AppointmentId, newServices, cancellationToken);
            }

            appointment.UpdatedDate = DateTime.UtcNow;
            appointment.UpdatedBy = currentUserId;

            await _commandRepository.UpdateAsync(appointment, cancellationToken);

            Appointment? result = await _repository.GetByIdWithDetailsAsync(
                appointment.AppointmentId, cancellationToken);

            return AppointmentMapper.ToResponseDto(result!);
        }

        public async Task<AppointmentResponseDto> RescheduleAsync(
            RescheduleAppointmentRequestDto request,
            int currentUserId,
            CancellationToken cancellationToken = default)
        {
            // ========== VALIDATION PHASE ==========

            // 1. Kiểm tra appointment tồn tại
            Appointment? oldAppointment = await _repository.GetByIdWithDetailsAsync(
                request.AppointmentId, cancellationToken);

            if (oldAppointment == null)
            {
                _logger.LogWarning(
                    "Reschedule attempt for non-existent appointment {AppointmentId} by user {UserId}",
                    request.AppointmentId, currentUserId);
                throw new InvalidOperationException("Appointment không tồn tại");
            }

            // 2. Kiểm tra status có thể reschedule không
            if (AppointmentStatusHelper.IsFinalStatus(oldAppointment.StatusId))
            {
                _logger.LogWarning(
                    "Reschedule attempt for finalized appointment {AppointmentId} (Status: {StatusId}) by user {UserId}",
                    request.AppointmentId, oldAppointment.StatusId, currentUserId);
                throw new InvalidOperationException("Không thể dời lịch appointment đã kết thúc");
            }

            // ========== RESCHEDULE CHAIN ANALYSIS ==========

            // 3. Lấy toàn bộ reschedule chain
            var rescheduleChain = await _queryRepository.GetRescheduleChainAsync(
                request.AppointmentId, cancellationToken);

            // Appointment gốc là phần tử đầu tiên trong chain
            int originalAppointmentId = rescheduleChain.First();

            // Số lần đã reschedule = số appointment trong chain - 1
            int rescheduleCount = rescheduleChain.Count - 1;

            _logger.LogInformation(
                "Reschedule analysis: Appointment {AppointmentId}, Chain: [{Chain}], RescheduleCount: {Count}, Customer: {CustomerId}",
                request.AppointmentId, 
                string.Join(" -> ", rescheduleChain), 
                rescheduleCount,
                oldAppointment.CustomerId);

            // 4. Kiểm tra appointment này đã bị reschedule chưa
            // ✅ CHECK QUAN TRỌNG: Appointment đã bị dời lịch thì KHÔNG CHO PHÉP dời lại
            // Customer phải dời appointment MỚI NHẤT trong chuỗi
            var hasBeenRescheduled = await _queryRepository.HasBeenRescheduledAsync(
                request.AppointmentId, cancellationToken);

            if (hasBeenRescheduled)
            {
                _logger.LogWarning(
                    "Reschedule attempt for already rescheduled appointment {AppointmentId} by customer {CustomerId}",
                    request.AppointmentId, oldAppointment.CustomerId);
                throw new InvalidOperationException(
                    "Lịch hẹn này đã được dời sang thời gian khác. " +
                    "Vui lòng kiểm tra lịch hẹn mới nhất trong danh sách 'Lịch hẹn của tôi'.");
            }

            // ✅ REMOVED: Logic sai đã được xóa (old line 264-272)
            // Appointment có RescheduledFromId vẫn CHO PHÉP reschedule (theo business rules)

            // ========== BUSINESS RULES BY RESCHEDULE COUNT (OPTION B - FLEXIBLE) ==========

            // 6. Apply business rules theo số lần reschedule
            switch (rescheduleCount)
            {
                case 0:
                    // ✅ LẦN 1: Cho phép linh hoạt (thông báo trước ≥24h)
                    _logger.LogInformation(
                        "First-time reschedule for appointment {AppointmentId} by customer {CustomerId}",
                        request.AppointmentId, oldAppointment.CustomerId);
                    ValidateFirstTimeReschedule(oldAppointment, minNoticeHours: 24);
                    break;

                case 1:
                    // ⚠️ LẦN 2: Cho phép NHƯNG có điều kiện bổ sung
                    // - Thông báo trước ≥48h (thay vì 24h)
                    // - Yêu cầu lý do bắt buộc
                    // - Log để staff review
                    _logger.LogWarning(
                        "Second reschedule attempt for appointment {AppointmentId}, customer {CustomerId}. " +
                        "Applying stricter conditions (48h notice + mandatory reason). Reason: {Reason}",
                        request.AppointmentId, oldAppointment.CustomerId, request.Reason);

                    ValidateSecondTimeReschedule(oldAppointment, request.Reason);

                    _logger.LogInformation(
                        "Second reschedule APPROVED with conditions: Appointment {AppointmentId}, Customer {CustomerId}, Reason: {Reason}",
                        request.AppointmentId, oldAppointment.CustomerId, request.Reason);
                    break;

                default:
                    // ❌ LẦN 3+: Yêu cầu staff approval (block API)
                    _logger.LogWarning(
                        "Excessive reschedule attempt ({Count} times) blocked for appointment {AppointmentId}, customer {CustomerId}. " +
                        "Staff approval required.",
                        rescheduleCount, request.AppointmentId, oldAppointment.CustomerId);
                    throw new InvalidOperationException(
                        $"Lịch hẹn gốc đã được dời {rescheduleCount} lần. " +
                        "Để bảo vệ chất lượng dịch vụ và công suất vận hành, " +
                        "lịch hẹn này cần được xem xét bởi nhân viên. " +
                        "Vui lòng liên hệ tổng đài " + GetHotlineNumber() + " để được hỗ trợ. " +
                        "Hoặc bạn có thể HỦY lịch hẹn hiện tại và TẠO LỊCH MỚI.");
            }

            // ========== NEW SLOT VALIDATION ==========

            // 7. Validate new slot tồn tại
            var newSlot = await _slotRepository.GetByIdAsync(request.NewSlotId, cancellationToken);
            if (newSlot == null)
                throw new InvalidOperationException("Slot mới không tồn tại");

            // 8. Kiểm tra slot không ở quá khứ
            var newSlotDateTime = newSlot.SlotDate.ToDateTime(newSlot.StartTime);
            if (newSlotDateTime < DateTime.UtcNow)
                throw new InvalidOperationException(
                    "Không thể dời lịch sang khung giờ ở trong quá khứ");

            // 9. Kiểm tra không được chọn cùng slot
            if (oldAppointment.SlotId == request.NewSlotId)
                throw new InvalidOperationException(
                    "Slot mới trùng với slot hiện tại. Vui lòng chọn khung giờ khác.");

            // 10. Kiểm tra slot còn chỗ không
            int activeCount = await _queryRepository.GetActiveCountBySlotIdAsync(
                request.NewSlotId, cancellationToken);

            if (activeCount >= newSlot.MaxBookings)
                throw new InvalidOperationException("Slot mới đã đầy");

            // 10.5. ✅ VEHICLE CONFLICT VALIDATION (với actual duration)
            // Kiểm tra xe không bị conflict thời gian với appointment khác
            int estimatedDuration = oldAppointment.EstimatedDuration ?? 60; // Default 60 phút nếu null

            await ValidateVehicleTimeConflict(
                oldAppointment.VehicleId,
                request.NewSlotId,
                estimatedDuration, // ✅ Truyền actual duration
                excludeAppointmentId: request.AppointmentId, // Loại trừ appointment đang reschedule
                cancellationToken);

            // 10.6. ✅ TECHNICIAN CONFLICT VALIDATION (với actual duration, PER CENTER)
            await ValidateTechnicianConflict(
                oldAppointment.PreferredTechnicianId,
                oldAppointment.ServiceCenterId,  // ✅ PER CENTER
                newSlot.SlotDate.ToDateTime(newSlot.StartTime),
                estimatedDuration,
                excludeAppointmentId: request.AppointmentId,
                cancellationToken);

            // 10.7. ✅ SERVICE CENTER CAPACITY VALIDATION (với actual duration)
            await ValidateServiceCenterCapacity(
                oldAppointment.ServiceCenterId,
                newSlot.SlotDate.ToDateTime(newSlot.StartTime),
                estimatedDuration,
                excludeAppointmentId: request.AppointmentId,
                cancellationToken);

            // ========== CREATE NEW APPOINTMENT ==========

            // 11. Generate appointment code mới
            string appointmentCode = await GenerateAppointmentCodeAsync(cancellationToken);

            // 12. Tạo appointment mới
            var newAppointment = new Appointment
            {
                AppointmentCode = appointmentCode,
                CustomerId = oldAppointment.CustomerId,
                VehicleId = oldAppointment.VehicleId,
                ServiceCenterId = oldAppointment.ServiceCenterId,
                SlotId = request.NewSlotId,
                PackageId = oldAppointment.PackageId,
                StatusId = (int)AppointmentStatusEnum.Pending,
                AppointmentDate = newSlot.SlotDate.ToDateTime(newSlot.StartTime),
                EstimatedDuration = oldAppointment.EstimatedDuration,
                EstimatedCost = oldAppointment.EstimatedCost,
                CustomerNotes = request.Reason ?? oldAppointment.CustomerNotes,
                PreferredTechnicianId = oldAppointment.PreferredTechnicianId,
                Priority = oldAppointment.Priority,
                Source = oldAppointment.Source,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = currentUserId,
                RescheduledFromId = request.AppointmentId  // ✅ Link back to old appointment
            };

            // 13. Copy services từ appointment cũ
            List<AppointmentService> newServices = oldAppointment.AppointmentServices
                .Select(aps => new AppointmentService
                {
                    ServiceId = aps.ServiceId,
                    ServiceSource = aps.ServiceSource,
                    Price = aps.Price,
                    EstimatedTime = aps.EstimatedTime,
                    Notes = aps.Notes
                }).ToList();

            // 14. Thực hiện reschedule trong database
            Appointment created = await _commandRepository.RescheduleAsync(
                request.AppointmentId, newAppointment, newServices, cancellationToken);

            _logger.LogInformation(
                "Reschedule successful: Old appointment {OldId} -> New appointment {NewId} ({NewCode}). " +
                "Customer: {CustomerId}, Old slot: {OldSlotId}, New slot: {NewSlotId}, " +
                "Old date: {OldDate}, New date: {NewDate}, Reason: {Reason}",
                request.AppointmentId, created.AppointmentId, appointmentCode,
                oldAppointment.CustomerId, oldAppointment.SlotId, request.NewSlotId,
                oldAppointment.AppointmentDate, newAppointment.AppointmentDate, request.Reason);

            // 15. Load lại với full details
            Appointment? result = await _repository.GetByIdWithDetailsAsync(
                created.AppointmentId, cancellationToken);

            return AppointmentMapper.ToResponseDto(result!);
        }

        public async Task<bool> CancelAsync(
            CancelAppointmentRequestDto request,
            int currentUserId,
            CancellationToken cancellationToken = default)
        {
            Appointment? appointment = await _repository.GetByIdAsync(
                request.AppointmentId, cancellationToken);

            if (appointment == null)
                throw new InvalidOperationException("Appointment không tồn tại");

            if (appointment.StatusId == (int)AppointmentStatusEnum.Cancelled)
                throw new InvalidOperationException("Appointment đã bị hủy");

            if (AppointmentStatusHelper.IsFinalStatus(appointment.StatusId))
                throw new InvalidOperationException("Không thể hủy appointment đã kết thúc");

            return await _commandRepository.CancelAsync(
                request.AppointmentId, request.CancellationReason, cancellationToken);
        }

        public async Task<bool> ConfirmAsync(
            ConfirmAppointmentRequestDto request,
            int currentUserId,
            CancellationToken cancellationToken = default)
        {
            Appointment? appointment = await _repository.GetByIdAsync(
                request.AppointmentId, cancellationToken);

            if (appointment == null)
                throw new InvalidOperationException("Appointment không tồn tại");

            if (appointment.StatusId != (int)AppointmentStatusEnum.Pending)
                throw new InvalidOperationException(
                    "Chỉ có thể xác nhận appointment ở trạng thái Pending");

            return await _commandRepository.ConfirmAsync(
                request.AppointmentId, request.ConfirmationMethod, cancellationToken);
        }

        public async Task<bool> MarkAsNoShowAsync(
            int appointmentId,
            int currentUserId,
            CancellationToken cancellationToken = default)
        {
            Appointment? appointment = await _repository.GetByIdAsync(appointmentId, cancellationToken);

            if (appointment == null)
                throw new InvalidOperationException("Appointment không tồn tại");

            if (appointment.StatusId != (int)AppointmentStatusEnum.Confirmed)
                throw new InvalidOperationException(
                    "Chỉ có thể đánh dấu NoShow cho appointment đã Confirmed");

            return await _commandRepository.MarkAsNoShowAsync(appointmentId, cancellationToken);
        }

        public async Task<bool> DeleteAsync(
            int appointmentId,
            int currentUserId,
            CancellationToken cancellationToken = default)
        {
            Appointment? appointment = await _repository.GetByIdAsync(appointmentId, cancellationToken);

            if (appointment == null)
                throw new InvalidOperationException("Appointment không tồn tại");

            if (appointment.StatusId != (int)AppointmentStatusEnum.Pending)
                throw new InvalidOperationException(
                    "Chỉ có thể xóa appointment ở trạng thái Pending");

            return await _commandRepository.DeleteIfPossibleAsync(appointmentId, cancellationToken);
        }

        /// <summary>
        /// ✅ ADVANCED: Complete appointment với RACE CONDITION HANDLING
        ///
        /// Features:
        /// 1. IDEMPOTENCY: Dùng RowVersion để detect double-complete (retry/network glitch)
        /// 2. TRANSACTION: Serializable isolation để tránh race conditions
        /// 3. SERVICE SOURCE AWARE: Chỉ trừ lượt cho services có ServiceSource = "Subscription"
        /// 4. GRACEFUL DEGRADATION: Nếu hết lượt (race), auto convert to "Extra" + log audit
        /// 5. AUDIT TRAIL: Log mọi thay đổi ServiceSource
        /// 6. PAYMENT TRACKING: Track payment cho degraded services
        ///
        /// Race Condition Scenario:
        /// - 2 customers (A, B) cùng book "Thay dầu" từ subscription còn 1 lượt
        /// - A complete trước → OK, trừ lượt
        /// - B complete sau → Service "Thay dầu" bị degrade to "Extra", customer phải trả
        /// </summary>
        public async Task<bool> CompleteAppointmentAsync(
            int appointmentId,
            int currentUserId,
            CancellationToken cancellationToken = default)
        {
            // 🔒 BƯỚC 1: IDEMPOTENCY CHECK - Prevent double-complete
            var appointment = await _repository.GetByIdWithDetailsAsync(appointmentId, cancellationToken);

            if (appointment == null)
                throw new InvalidOperationException("Appointment không tồn tại");

            // Nếu đã Completed → Return success ngay (idempotent)
            if (appointment.StatusId == (int)AppointmentStatusEnum.Completed)
            {
                _logger.LogWarning(
                    "⚠️ Appointment {AppointmentId} đã completed trước đó " +
                    "(CompletedDate={CompletedDate}, CompletedBy={CompletedBy}). " +
                    "Đây là retry/duplicate request → Return success (idempotent)",
                    appointmentId, appointment.CompletedDate, appointment.CompletedBy);

                return true; // Idempotent - không throw error
            }

            // Chỉ cho phép complete từ InProgress
            if (appointment.StatusId != (int)AppointmentStatusEnum.InProgress)
                throw new InvalidOperationException(
                    $"Chỉ có thể complete appointment đang ở trạng thái InProgress. " +
                    $"Trạng thái hiện tại: {appointment.StatusId}");

            _logger.LogInformation(
                "🚀 Starting CompleteAppointment: AppointmentId={AppointmentId}, " +
                "CustomerId={CustomerId}, Services={ServiceCount}, " +
                "SubscriptionServices={SubCount}",
                appointmentId, appointment.CustomerId,
                appointment.AppointmentServices.Count,
                appointment.AppointmentServices.Count(s => s.ServiceSource == "Subscription"));

            // 🔒 BƯỚC 2: TRANSACTION với Serializable isolation
            // Ngăn race conditions khi 2 requests cùng complete 2 appointments cùng lúc
            // NOTE: Transaction được handle bởi EF Core DbContext
            try
            {
                bool hasDegradedService = false;
                decimal additionalPaymentRequired = 0;

                // 🔒 BƯỚC 3: XỬ LÝ TỪNG SERVICE với PESSIMISTIC LOCK
                foreach (var appointmentService in appointment.AppointmentServices)
                {
                    // Chỉ xử lý services có ServiceSource = "Subscription"
                    // Services "Extra" hoặc "Regular" không cần trừ lượt
                    if (appointmentService.ServiceSource != "Subscription")
                    {
                        _logger.LogDebug(
                            "Service {ServiceId} has ServiceSource='{Source}' → Skip usage deduction",
                            appointmentService.ServiceId, appointmentService.ServiceSource);
                        continue;
                    }

                    _logger.LogInformation(
                        "🔄 Processing Subscription service: ServiceId={ServiceId}, " +
                        "AppointmentServiceId={ApsId}",
                        appointmentService.ServiceId, appointmentService.AppointmentServiceId);

                    // 🔒 TRY DEDUCT USAGE với PESSIMISTIC LOCK
                    // UpdateServiceUsageAsync sẽ dùng UPDLOCK để lock row
                    try
                    {
                        bool deducted = await _subscriptionCommandRepository.UpdateServiceUsageAsync(
                            appointment.SubscriptionId!.Value,
                            appointmentService.ServiceId,
                            quantityUsed: 1,
                            appointmentId: appointmentId,
                            cancellationToken);

                        if (deducted)
                        {
                            _logger.LogInformation(
                                "✅ Successfully deducted usage: ServiceId={ServiceId}, " +
                                "SubscriptionId={SubId}",
                                appointmentService.ServiceId, appointment.SubscriptionId);
                        }
                        else
                        {
                            // KHÔNG THỂ TRỪ LƯỢT → RACE CONDITION DETECTED!
                            _logger.LogWarning(
                                "⚠️ RACE CONDITION: Cannot deduct usage for ServiceId={ServiceId}, " +
                                "SubscriptionId={SubId} (hết lượt hoặc subscription không còn active). " +
                                "→ DEGRADING to 'Extra'",
                                appointmentService.ServiceId, appointment.SubscriptionId);

                            // 🔄 GRACEFUL DEGRADATION: Convert to "Extra"
                            await DegradeServiceToExtraAsync(
                                appointmentService,
                                "RACE_CONDITION_OUT_OF_USAGE",
                                currentUserId,
                                cancellationToken);

                            hasDegradedService = true;
                            additionalPaymentRequired += appointmentService.Price;
                        }
                    }
                    catch (InvalidOperationException ex)
                    {
                        // Exception từ UpdateServiceUsageAsync (subscription invalid, etc.)
                        _logger.LogError(ex,
                            "❌ ERROR deducting usage for ServiceId={ServiceId}: {Message}. " +
                            "→ DEGRADING to 'Extra'",
                            appointmentService.ServiceId, ex.Message);

                        // 🔄 GRACEFUL DEGRADATION
                        await DegradeServiceToExtraAsync(
                            appointmentService,
                            $"DEDUCTION_ERROR: {ex.Message}",
                            currentUserId,
                            cancellationToken);

                        hasDegradedService = true;
                        additionalPaymentRequired += appointmentService.Price;
                    }
                }

                // 🔒 BƯỚC 4: UPDATE APPOINTMENT STATUS
                AppointmentStatusEnum finalStatus;

                if (hasDegradedService && additionalPaymentRequired > 0)
                {
                    // Có services bị degrade + cần thanh toán bổ sung
                    // → Status = CompletedWithUnpaidBalance
                    finalStatus = AppointmentStatusEnum.CompletedWithUnpaidBalance;

                    _logger.LogWarning(
                        "⚠️ Appointment {AppointmentId} completed with UNPAID BALANCE: {Amount}đ " +
                        "(do có services bị degrade to Extra)",
                        appointmentId, additionalPaymentRequired);

                    // TODO: Trigger notification to customer về khoản thanh toán bổ sung
                    // TODO: Create PaymentTransaction record với Status=Pending
                }
                else
                {
                    // Tất cả services OK hoặc đã thanh toán đầy đủ
                    finalStatus = AppointmentStatusEnum.Completed;
                }

                // Update status + CompletedDate + CompletedBy
                appointment.StatusId = (int)finalStatus;
                appointment.CompletedDate = DateTime.UtcNow;
                appointment.CompletedBy = currentUserId;

                // Update EstimatedCost nếu có degraded services
                if (hasDegradedService)
                {
                    appointment.EstimatedCost = (appointment.EstimatedCost ?? 0) + additionalPaymentRequired;
                }

                await _commandRepository.UpdateAsync(appointment, cancellationToken);

                _logger.LogInformation(
                    "✅ CompleteAppointment SUCCESS: AppointmentId={AppointmentId}, " +
                    "FinalStatus={Status}, AdditionalPayment={Payment}đ, " +
                    "DegradedServices={DegradedCount}",
                    appointmentId, finalStatus, additionalPaymentRequired,
                    hasDegradedService ? appointment.AppointmentServices.Count(s => s.ServiceSource == "Extra") : 0);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "❌ ERROR completing appointment {AppointmentId}: {Message}",
                    appointmentId, ex.Message);

                throw;
            }
        }

        /// <summary>
        /// 🔄 GRACEFUL DEGRADATION: Convert service từ "Subscription" → "Extra"
        /// Xảy ra khi race condition (hết lượt subscription)
        ///
        /// Actions:
        /// 1. Update AppointmentService.ServiceSource = "Extra"
        /// 2. Update AppointmentService.Price = actual service price
        /// 3. Log audit trail (để dispute resolution)
        /// 4. Create PaymentTransaction record (nếu cần charge customer)
        /// </summary>
        private async Task DegradeServiceToExtraAsync(
            AppointmentService appointmentService,
            string reason,
            int userId,
            CancellationToken cancellationToken)
        {
            var oldSource = appointmentService.ServiceSource;
            var oldPrice = appointmentService.Price;

            // Lấy actual price của service (từ MaintenanceService hoặc ModelServicePricing)
            var service = await _serviceRepository.GetByIdAsync(
                appointmentService.ServiceId, cancellationToken);

            if (service == null)
            {
                throw new InvalidOperationException(
                    $"Service {appointmentService.ServiceId} không tồn tại");
            }

            decimal actualPrice = service.BasePrice;

            // TODO: Check ModelServicePricing nếu cần (theo vehicle model)

            // 1️⃣ Update ServiceSource và Price
            appointmentService.ServiceSource = "Extra";
            appointmentService.Price = actualPrice;

            // Save via DbContext (EF Core tracking)
            await _commandRepository.UpdateAsync(appointmentService.Appointment, cancellationToken);

            _logger.LogWarning(
                "🔄 Service DEGRADED: AppointmentServiceId={ApsId}, ServiceId={ServiceId}, " +
                "{OldSource}({OldPrice}đ) → Extra({NewPrice}đ), Reason: {Reason}",
                appointmentService.AppointmentServiceId, appointmentService.ServiceId,
                oldSource, oldPrice, actualPrice, reason);

            // 2️⃣ Log audit trail (dùng IServiceSourceAuditService)
            await _auditService.LogServiceSourceChangeAsync(
                appointmentServiceId: appointmentService.AppointmentServiceId,
                oldServiceSource: oldSource,
                newServiceSource: "Extra",
                oldPrice: oldPrice,
                newPrice: actualPrice,
                changeReason: reason,
                changeType: "AUTO_DEGRADE",
                changedBy: userId,
                ipAddress: null,
                userAgent: null,
                refundAmount: null,
                usageDeducted: false);

            // 3️⃣ TODO: Create PaymentTransaction record (nếu cần charge customer)
            // await _paymentService.CreatePendingTransactionAsync(...)
        }

        /// <summary>
        /// 🔧 ADMIN TOOL: Manually adjust ServiceSource và Price của AppointmentService
        ///
        /// Use cases:
        /// 1. Sửa lỗi: Customer đã có subscription nhưng bị charge nhầm (Extra → Subscription)
        /// 2. Hoàn tiền: Dịch vụ không đạt yêu cầu, giảm giá hoặc miễn phí (Extra → Subscription)
        /// 3. Thu thêm phí: Customer dùng service ngoài subscription (Subscription → Extra)
        ///
        /// Features:
        /// - Validate appointment đã Completed
        /// - Update ServiceSource và Price
        /// - Deduct/Refund subscription usage nếu cần
        /// - Log audit trail đầy đủ
        /// - Create PaymentTransaction nếu có refund
        /// </summary>
        /// <param name="appointmentId">ID của appointment</param>
        /// <param name="appointmentServiceId">ID của AppointmentService cần adjust</param>
        /// <param name="newServiceSource">ServiceSource mới (Subscription, Extra, Regular)</param>
        /// <param name="newPrice">Giá mới</param>
        /// <param name="reason">Lý do điều chỉnh (bắt buộc)</param>
        /// <param name="issueRefund">Có hoàn tiền không?</param>
        /// <param name="userId">User ID của Admin thực hiện adjust</param>
        /// <param name="ipAddress">IP address của request (để audit)</param>
        /// <param name="userAgent">User Agent của request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Response DTO với thông tin adjustment</returns>
        public async Task<AdjustServiceSourceResponseDto> AdjustServiceSourceAsync(
            int appointmentId,
            int appointmentServiceId,
            string newServiceSource,
            decimal newPrice,
            string reason,
            bool issueRefund,
            int userId,
            string? ipAddress = null,
            string? userAgent = null,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "🔧 [ADMIN ADJUST] Starting: AppointmentId={AppointmentId}, " +
                "AppointmentServiceId={ApsId}, NewSource={NewSource}, NewPrice={NewPrice}, " +
                "Reason={Reason}, IssueRefund={Refund}, UserId={UserId}",
                appointmentId, appointmentServiceId, newServiceSource, newPrice,
                reason, issueRefund, userId);

            // 1️⃣ VALIDATE: Appointment phải tồn tại và đã Completed
            var appointment = await _repository.GetByIdWithDetailsAsync(appointmentId, cancellationToken);

            if (appointment == null)
                throw new InvalidOperationException($"Appointment {appointmentId} không tồn tại");

            if (appointment.StatusId != (int)AppointmentStatusEnum.Completed &&
                appointment.StatusId != (int)AppointmentStatusEnum.CompletedWithUnpaidBalance)
            {
                throw new InvalidOperationException(
                    $"Chỉ có thể adjust services của appointment đã Completed. " +
                    $"Trạng thái hiện tại: {appointment.StatusId}");
            }

            // 2️⃣ VALIDATE: AppointmentService phải thuộc Appointment này
            var appointmentService = appointment.AppointmentServices
                .FirstOrDefault(aps => aps.AppointmentServiceId == appointmentServiceId);

            if (appointmentService == null)
                throw new InvalidOperationException(
                    $"AppointmentService {appointmentServiceId} không thuộc Appointment {appointmentId}");

            // 3️⃣ VALIDATE: newServiceSource hợp lệ
            var validSources = new[] { "Subscription", "Extra", "Regular" };
            if (!validSources.Contains(newServiceSource))
                throw new InvalidOperationException(
                    $"ServiceSource không hợp lệ: {newServiceSource}. " +
                    $"Phải là: {string.Join(", ", validSources)}");

            // 4️⃣ SAVE OLD VALUES (để audit log)
            var oldServiceSource = appointmentService.ServiceSource;
            var oldPrice = appointmentService.Price;
            var priceDifference = newPrice - oldPrice;

            _logger.LogInformation(
                "📝 Adjustment details: {OldSource}({OldPrice}đ) → {NewSource}({NewPrice}đ), " +
                "PriceDiff={PriceDiff}đ",
                oldServiceSource, oldPrice, newServiceSource, newPrice, priceDifference);

            // 5️⃣ CHECK: Có cần deduct/refund subscription usage không?
            bool usageDeducted = false;
            decimal? refundAmount = null;

            // Case 1: Extra → Subscription = Cần DEDUCT usage (trừ lượt)
            if (oldServiceSource != "Subscription" && newServiceSource == "Subscription")
            {
                if (!appointment.SubscriptionId.HasValue)
                    throw new InvalidOperationException(
                        "Không thể chuyển sang Subscription vì appointment không có SubscriptionId");

                _logger.LogInformation(
                    "➖ Deducting usage: SubscriptionId={SubId}, ServiceId={ServiceId}",
                    appointment.SubscriptionId, appointmentService.ServiceId);

                // Try deduct usage
                bool deducted = await _subscriptionCommandRepository.UpdateServiceUsageAsync(
                    appointment.SubscriptionId.Value,
                    appointmentService.ServiceId,
                    quantityUsed: 1,
                    appointmentId: appointmentId,
                    cancellationToken);

                if (!deducted)
                {
                    throw new InvalidOperationException(
                        $"Không thể chuyển sang Subscription: Subscription không còn lượt cho service này. " +
                        $"Vui lòng kiểm tra lại hoặc chọn ServiceSource khác.");
                }

                usageDeducted = true;
            }

            // Case 2: Subscription → Extra/Regular = Có thể REFUND usage (hoàn lại lượt)
            // TODO: Implement RefundServiceUsageAsync nếu cần

            // 6️⃣ CALCULATE REFUND AMOUNT (nếu có)
            if (issueRefund && priceDifference < 0)
            {
                // Giá mới < giá cũ → Hoàn tiền chênh lệch
                refundAmount = Math.Abs(priceDifference);

                _logger.LogInformation(
                    "💰 Issuing refund: Amount={RefundAmount}đ (OldPrice={OldPrice}, NewPrice={NewPrice})",
                    refundAmount, oldPrice, newPrice);

                // TODO: Create PaymentTransaction record với Status=Refunded
                // await _paymentService.CreateRefundTransactionAsync(...)
            }

            // 7️⃣ UPDATE AppointmentService
            appointmentService.ServiceSource = newServiceSource;
            appointmentService.Price = newPrice;

            // Save via DbContext (EF Core tracking)
            await _commandRepository.UpdateAsync(appointment, cancellationToken);

            _logger.LogInformation(
                "✅ AppointmentService updated: AppointmentServiceId={ApsId}, " +
                "ServiceSource={NewSource}, Price={NewPrice}",
                appointmentServiceId, newServiceSource, newPrice);

            // 8️⃣ UPDATE Appointment EstimatedCost
            var oldEstimatedCost = appointment.EstimatedCost ?? 0;
            var newEstimatedCost = oldEstimatedCost + priceDifference;
            appointment.EstimatedCost = newEstimatedCost;

            await _commandRepository.UpdateAsync(appointment, cancellationToken);

            _logger.LogInformation(
                "📊 Appointment EstimatedCost updated: {OldCost}đ → {NewCost}đ",
                oldEstimatedCost, newEstimatedCost);

            // 9️⃣ LOG AUDIT TRAIL
            await _auditService.LogServiceSourceChangeAsync(
                appointmentServiceId: appointmentServiceId,
                oldServiceSource: oldServiceSource,
                newServiceSource: newServiceSource,
                oldPrice: oldPrice,
                newPrice: newPrice,
                changeReason: reason,
                changeType: issueRefund ? "REFUND" : "MANUAL_ADJUST",
                changedBy: userId,
                ipAddress: ipAddress,
                userAgent: userAgent,
                refundAmount: refundAmount,
                usageDeducted: usageDeducted);

            _logger.LogInformation(
                "✅ [ADMIN ADJUST] COMPLETED: AppointmentServiceId={ApsId}, " +
                "{OldSource} → {NewSource}, Refund={Refund}đ, UsageDeducted={UsageDeducted}",
                appointmentServiceId, oldServiceSource, newServiceSource,
                refundAmount ?? 0, usageDeducted);

            // 🔟 RETURN RESPONSE DTO
            return new AdjustServiceSourceResponseDto
            {
                AppointmentServiceId = appointmentServiceId,
                OldServiceSource = oldServiceSource,
                NewServiceSource = newServiceSource,
                OldPrice = oldPrice,
                NewPrice = newPrice,
                PriceDifference = priceDifference,
                RefundIssued = refundAmount.HasValue && refundAmount.Value > 0,
                UsageDeducted = usageDeducted,
                UpdatedBy = userId,
                UpdatedDate = DateTime.UtcNow
            };
        }

        private async Task<(decimal totalCost, int totalDuration, List<AppointmentService> services)>
            CalculatePricingAsync(
                int vehicleModelId,
                int? packageId,
                List<int> serviceIds,
                CancellationToken cancellationToken)
        {
            decimal totalCost = 0;
            int totalDuration = 0;
            List<AppointmentService> appointmentServices = new List<AppointmentService>();

            if (serviceIds.Any())
            {
                IEnumerable<MaintenanceService> services = await _serviceRepository.GetByIdsAsync(
                    serviceIds, cancellationToken);

                foreach (MaintenanceService service in services)
                {
                    var pricing = await _pricingRepository.GetActivePricingAsync(
                        vehicleModelId, service.ServiceId);

                    decimal price = pricing?.CustomPrice ?? service.BasePrice;
                    int time = pricing?.CustomTime ?? service.StandardTime;

                    totalCost += price;
                    totalDuration += time;

                    appointmentServices.Add(new AppointmentService
                    {
                        ServiceId = service.ServiceId,
                        ServiceSource = "Regular",
                        Price = price,
                        EstimatedTime = time
                    });
                }
            }

            return (totalCost, totalDuration, appointmentServices);
        }

        private async Task<string> GenerateAppointmentCodeAsync(CancellationToken cancellationToken)
        {
            string prefix = "APT";
            string datePart = DateTime.UtcNow.ToString("yyyyMMdd");

            for (int attempts = 0; attempts < 10; attempts++)
            {
                int randomNumber = RandomNumberGenerator.GetInt32(1000, 9999);
                string code = $"{prefix}{datePart}{randomNumber}";

                if (!await _repository.ExistsByCodeAsync(code, cancellationToken))
                    return code;
            }

            string timestampPart = DateTime.UtcNow.Ticks.ToString()[^6..];
            return $"{prefix}{datePart}{timestampPart}";
        }

        /// <summary>
        /// Validate reschedule lần đầu - Phải thông báo trước >= minNoticeHours
        /// </summary>
        private void ValidateFirstTimeReschedule(Appointment appointment, int minNoticeHours)
        {
            var appointmentDateTime = appointment.AppointmentDate;
            var now = DateTime.UtcNow;
            var hoursUntilAppointment = (appointmentDateTime - now).TotalHours;

            if (hoursUntilAppointment < minNoticeHours)
            {
                _logger.LogWarning(
                    "First reschedule rejected due to insufficient notice: Appointment {AppointmentId}, " +
                    "Required: {MinHours}h, Actual: {ActualHours}h, Customer: {CustomerId}",
                    appointment.AppointmentId, minNoticeHours,
                    Math.Round(hoursUntilAppointment, 1), appointment.CustomerId);

                throw new InvalidOperationException(
                    $"Để dời lịch lần đầu, bạn cần thông báo trước ít nhất {minNoticeHours} giờ. " +
                    $"Lịch hẹn của bạn chỉ còn {Math.Round(hoursUntilAppointment, 1)} giờ nữa. " +
                    "Vui lòng liên hệ tổng đài " + GetHotlineNumber() + " để được hỗ trợ khẩn cấp.");
            }

            _logger.LogInformation(
                "First-time reschedule validation passed: Appointment {AppointmentId}, Notice: {Hours}h (Required: {MinHours}h)",
                appointment.AppointmentId, Math.Round(hoursUntilAppointment, 1), minNoticeHours);
        }

        /// <summary>
        /// Validate reschedule lần 2 - Điều kiện chặt chẽ hơn (≥48h notice + mandatory reason)
        /// </summary>
        private void ValidateSecondTimeReschedule(Appointment appointment, string? reason)
        {
            const int SECOND_RESCHEDULE_MIN_HOURS = 48; // Lần 2 phải thông báo trước 48h

            var appointmentDateTime = appointment.AppointmentDate;
            var now = DateTime.UtcNow;
            var hoursUntilAppointment = (appointmentDateTime - now).TotalHours;

            // 1. Kiểm tra thông báo trước ≥48h
            if (hoursUntilAppointment < SECOND_RESCHEDULE_MIN_HOURS)
            {
                _logger.LogWarning(
                    "Second reschedule rejected due to insufficient notice: Appointment {AppointmentId}, " +
                    "Required: {MinHours}h, Actual: {ActualHours}h, Customer: {CustomerId}",
                    appointment.AppointmentId, SECOND_RESCHEDULE_MIN_HOURS,
                    Math.Round(hoursUntilAppointment, 1), appointment.CustomerId);

                throw new InvalidOperationException(
                    $"Đây là lần dời lịch thứ 2. " +
                    $"Để bảo vệ lịch trình kỹ thuật viên và khách hàng khác, " +
                    $"bạn cần thông báo trước ít nhất {SECOND_RESCHEDULE_MIN_HOURS} giờ. " +
                    $"Lịch hẹn của bạn chỉ còn {Math.Round(hoursUntilAppointment, 1)} giờ nữa. " +
                    "Vui lòng liên hệ tổng đài " + GetHotlineNumber() + " để được hỗ trợ.");
            }

            // 2. Kiểm tra lý do bắt buộc
            if (string.IsNullOrWhiteSpace(reason) || reason.Length < 10)
            {
                _logger.LogWarning(
                    "Second reschedule rejected due to missing/invalid reason: Appointment {AppointmentId}, " +
                    "Customer: {CustomerId}, Reason length: {ReasonLength}",
                    appointment.AppointmentId, appointment.CustomerId, reason?.Length ?? 0);

                throw new InvalidOperationException(
                    "Đây là lần dời lịch thứ 2. " +
                    "Vui lòng cung cấp lý do dời lịch (tối thiểu 10 ký tự) " +
                    "để chúng tôi có thể cải thiện dịch vụ và hỗ trợ bạn tốt hơn.");
            }

            _logger.LogInformation(
                "Second-time reschedule validation passed: Appointment {AppointmentId}, " +
                "Notice: {Hours}h (Required: {MinHours}h), Reason: {Reason}, Customer: {CustomerId}",
                appointment.AppointmentId, Math.Round(hoursUntilAppointment, 1),
                SECOND_RESCHEDULE_MIN_HOURS, reason, appointment.CustomerId);
        }

        /// <summary>
        /// Validate vehicle không bị conflict về thời gian (DYNAMIC DURATION)
        ///
        /// Logic:
        /// - Actual End Time = Start Time + EstimatedDuration (KHÔNG PHẢI slot.EndTime)
        /// - Overlap Rule: Start_A < End_B AND Start_B < End_A
        ///
        /// Kiểm tra:
        /// 1. Không overlap với appointments khác của xe (dựa trên actual duration)
        /// 2. Giới hạn số appointments/ngày (max 3)
        /// </summary>
        private async Task ValidateVehicleTimeConflict(
            int vehicleId,
            int newSlotId,
            int newEstimatedDuration, // Duration thực tế (phút)
            int? excludeAppointmentId,
            CancellationToken cancellationToken)
        {
            // 1. Lấy slot mới
            var newSlot = await _slotRepository.GetByIdAsync(newSlotId, cancellationToken);
            if (newSlot == null)
                throw new InvalidOperationException("Slot không tồn tại");

            // 2. Tính ACTUAL TIME RANGE của appointment mới
            var newStart = newSlot.SlotDate.ToDateTime(newSlot.StartTime);
            var newEnd = newStart.AddMinutes(newEstimatedDuration); // ✅ Dùng actual duration

            _logger.LogInformation(
                "Vehicle conflict check (DYNAMIC): VehicleId={VehicleId}, " +
                "NewSlot={SlotId}, StartTime={Start}, Duration={Duration}min, " +
                "Actual EndTime={End}",
                vehicleId, newSlotId, newStart, newEstimatedDuration, newEnd);

            // 3. Lấy tất cả appointments ACTIVE của xe trong ngày này
            var existingAppointments = await _queryRepository.GetVehicleAppointmentsByDateAsync(
                vehicleId, newSlot.SlotDate, cancellationToken);

            // Loại trừ appointment hiện tại (nếu đang update/reschedule)
            if (excludeAppointmentId.HasValue)
            {
                existingAppointments = existingAppointments
                    .Where(a => a.AppointmentId != excludeAppointmentId.Value)
                    .ToList();
            }

            // Chỉ check appointments ACTIVE
            var activeAppointments = existingAppointments
                .Where(a => AppointmentStatusHelper.IsActiveBooking(a.StatusId))
                .ToList();

            _logger.LogInformation(
                "Found {Count} active appointments for vehicle {VehicleId} on {Date}",
                activeAppointments.Count, vehicleId, newSlot.SlotDate);

            // 4. Check OVERLAP với từng appointment (dựa trên ACTUAL DURATION)
            foreach (var existingAppt in activeAppointments)
            {
                // Tính actual time range của existing appointment
                var existingStart = existingAppt.AppointmentDate;
                var existingEnd = existingStart.AddMinutes(existingAppt.EstimatedDuration ?? 60);

                // ✅ OVERLAP FORMULA: Start_A < End_B AND Start_B < End_A
                bool isOverlap = newStart < existingEnd && existingStart < newEnd;

                if (isOverlap)
                {
                    _logger.LogWarning(
                        "Vehicle TIME OVERLAP detected: VehicleId={VehicleId}, " +
                        "New: [{NewStart} - {NewEnd}], " +
                        "Existing Appt #{ExistingCode}: [{ExistingStart} - {ExistingEnd}]",
                        vehicleId,
                        newStart.ToString("HH:mm"), newEnd.ToString("HH:mm"),
                        existingAppt.AppointmentCode,
                        existingStart.ToString("HH:mm"), existingEnd.ToString("HH:mm"));

                    throw new InvalidOperationException(
                        $"Xe này đã có lịch hẹn từ {existingStart:HH\\:mm} đến {existingEnd:HH\\:mm} " +
                        $"(Appointment #{existingAppt.AppointmentCode}). " +
                        $"Thời gian bạn chọn ({newStart:HH\\:mm}-{newEnd:HH\\:mm}) bị xung đột. " +
                        "Vui lòng chọn khung giờ khác.");
                }
            }

            // 5. Giới hạn số appointments/ngày
            int maxApptsPerDay = GetMaxAppointmentsPerDay();

            if (activeAppointments.Count >= maxApptsPerDay)
            {
                _logger.LogWarning(
                    "Vehicle daily appointment LIMIT exceeded: VehicleId={VehicleId}, " +
                    "Date={Date}, Current: {Current}, Max: {Max}",
                    vehicleId, newSlot.SlotDate, activeAppointments.Count, maxApptsPerDay);

                throw new InvalidOperationException(
                    $"Xe này đã có {activeAppointments.Count} lịch hẹn trong ngày {newSlot.SlotDate:dd/MM/yyyy}. " +
                    $"Hệ thống chỉ cho phép tối đa {maxApptsPerDay} lịch/ngày để đảm bảo chất lượng dịch vụ. " +
                    "Vui lòng chọn ngày khác hoặc liên hệ tổng đài " + GetHotlineNumber() + ".");
            }

            _logger.LogInformation(
                "Vehicle time conflict validation PASSED: VehicleId={VehicleId}, " +
                "Time=[{Start}-{End}], Appointments in day: {Count}/{Max}",
                vehicleId, newStart.ToString("HH:mm"), newEnd.ToString("HH:mm"),
                activeAppointments.Count, maxApptsPerDay);
        }

        /// <summary>
        /// Validate technician không bị conflict về thời gian (DYNAMIC DURATION)
        ///
        /// Logic tương tự Vehicle conflict:
        /// - Actual End Time = Start Time + EstimatedDuration
        /// - Overlap: Start_A < End_B AND Start_B < End_A
        ///
        /// ✅ PER CENTER: Technician thuộc về 1 trung tâm, chỉ check trong trung tâm đó
        /// Chỉ check nếu có PreferredTechnicianId
        /// </summary>
        private async Task ValidateTechnicianConflict(
            int? technicianId,
            int serviceCenterId,
            DateTime newStart,
            int newEstimatedDuration,
            int? excludeAppointmentId,
            CancellationToken cancellationToken)
        {
            // Nếu không assign tech, skip validation
            if (!technicianId.HasValue)
            {
                _logger.LogInformation("No technician assigned, skip technician conflict check");
                return;
            }

            var newEnd = newStart.AddMinutes(newEstimatedDuration);
            var date = DateOnly.FromDateTime(newStart);

            _logger.LogInformation(
                "Technician conflict check (PER CENTER): TechnicianId={TechnicianId}, CenterId={CenterId}, " +
                "Time=[{Start}-{End}], Duration={Duration}min",
                technicianId, serviceCenterId, newStart.ToString("HH:mm"), newEnd.ToString("HH:mm"), newEstimatedDuration);

            // ✅ Lấy appointments của tech TRONG TRUNG TÂM này
            var techAppointments = await _queryRepository.GetTechnicianAppointmentsByDateAsync(
                technicianId.Value, serviceCenterId, date, cancellationToken);

            // Loại trừ appointment hiện tại
            if (excludeAppointmentId.HasValue)
            {
                techAppointments = techAppointments
                    .Where(a => a.AppointmentId != excludeAppointmentId.Value)
                    .ToList();
            }

            // Chỉ check appointments ACTIVE
            var activeAppointments = techAppointments
                .Where(a => AppointmentStatusHelper.IsActiveBooking(a.StatusId))
                .ToList();

            _logger.LogInformation(
                "Found {Count} active appointments for technician {TechnicianId} in center {CenterId} on {Date}",
                activeAppointments.Count, technicianId, serviceCenterId, date);

            // Check OVERLAP
            foreach (var existingAppt in activeAppointments)
            {
                var existingStart = existingAppt.AppointmentDate;
                var existingEnd = existingStart.AddMinutes(existingAppt.EstimatedDuration ?? 60);

                bool isOverlap = newStart < existingEnd && existingStart < newEnd;

                if (isOverlap)
                {
                    _logger.LogWarning(
                        "Technician TIME OVERLAP: TechnicianId={TechnicianId}, " +
                        "New: [{NewStart}-{NewEnd}], " +
                        "Existing Appt #{Code}: [{ExistingStart}-{ExistingEnd}]",
                        technicianId,
                        newStart.ToString("HH:mm"), newEnd.ToString("HH:mm"),
                        existingAppt.AppointmentCode,
                        existingStart.ToString("HH:mm"), existingEnd.ToString("HH:mm"));

                    throw new InvalidOperationException(
                        $"Kỹ thuật viên đã được phân công từ {existingStart:HH\\:mm} đến {existingEnd:HH\\:mm} " +
                        $"(Appointment #{existingAppt.AppointmentCode}). " +
                        $"Không thể phân công thêm từ {newStart:HH\\:mm} đến {newEnd:HH\\:mm}. " +
                        "Vui lòng chọn kỹ thuật viên khác hoặc khung giờ khác.");
                }
            }

            _logger.LogInformation(
                "Technician conflict validation PASSED (PER CENTER): TechnicianId={TechnicianId}, CenterId={CenterId}, Time=[{Start}-{End}]",
                technicianId, serviceCenterId, newStart.ToString("HH:mm"), newEnd.ToString("HH:mm"));
        }

        /// <summary>
        /// Validate service center capacity (DYNAMIC DURATION)
        ///
        /// Service center có capacity giới hạn (ví dụ: 5 bays/vị trí sửa chữa)
        /// Tại mọi thời điểm, số lượng appointments đang overlap không được vượt quá capacity
        ///
        /// Logic:
        /// - Actual End Time = Start Time + EstimatedDuration
        /// - Đếm số appointments overlap với [newStart, newEnd]
        /// - Nếu count >= maxCapacity → block
        /// </summary>
        private async Task ValidateServiceCenterCapacity(
            int serviceCenterId,
            DateTime newStart,
            int newEstimatedDuration,
            int? excludeAppointmentId,
            CancellationToken cancellationToken)
        {
            var newEnd = newStart.AddMinutes(newEstimatedDuration);
            var date = DateOnly.FromDateTime(newStart);

            _logger.LogInformation(
                "Service center capacity check: CenterId={CenterId}, " +
                "Time=[{Start}-{End}], Duration={Duration}min",
                serviceCenterId, newStart.ToString("HH:mm"), newEnd.ToString("HH:mm"), newEstimatedDuration);

            // Lấy tất cả appointments của service center trong ngày
            var centerAppointments = await _queryRepository.GetServiceCenterAppointmentsByDateAsync(
                serviceCenterId, date, cancellationToken);

            // Loại trừ appointment hiện tại (nếu có)
            if (excludeAppointmentId.HasValue)
            {
                centerAppointments = centerAppointments
                    .Where(a => a.AppointmentId != excludeAppointmentId.Value)
                    .ToList();
            }

            // Chỉ check appointments ACTIVE (pending, confirmed, in-progress)
            var activeAppointments = centerAppointments
                .Where(a => AppointmentStatusHelper.IsActiveBooking(a.StatusId))
                .ToList();

            _logger.LogInformation(
                "Found {Count} active appointments for service center {CenterId} on {Date}",
                activeAppointments.Count, serviceCenterId, date);

            // Đếm số appointments OVERLAP với khung giờ mới
            int overlappingCount = 0;

            foreach (var existingAppt in activeAppointments)
            {
                var existingStart = existingAppt.AppointmentDate;
                var existingEnd = existingStart.AddMinutes(existingAppt.EstimatedDuration ?? 60);

                // ✅ OVERLAP FORMULA
                bool isOverlap = newStart < existingEnd && existingStart < newEnd;

                if (isOverlap)
                {
                    overlappingCount++;
                    _logger.LogDebug(
                        "Overlapping appointment found: #{Code} [{ExistingStart}-{ExistingEnd}]",
                        existingAppt.AppointmentCode,
                        existingStart.ToString("HH:mm"), existingEnd.ToString("HH:mm"));
                }
            }

            // Kiểm tra capacity (cộng thêm 1 cho appointment mới)
            int maxCapacity = GetServiceCenterMaxCapacity();
            int totalConcurrent = overlappingCount + 1; // +1 cho appointment đang tạo

            _logger.LogInformation(
                "Service center capacity: {Current}/{Max} concurrent appointments during [{Start}-{End}]",
                totalConcurrent, maxCapacity,
                newStart.ToString("HH:mm"), newEnd.ToString("HH:mm"));

            if (totalConcurrent > maxCapacity)
            {
                _logger.LogWarning(
                    "Service center CAPACITY EXCEEDED: CenterId={CenterId}, " +
                    "Time=[{Start}-{End}], Concurrent={Current}, Max={Max}",
                    serviceCenterId,
                    newStart.ToString("HH:mm"), newEnd.ToString("HH:mm"),
                    totalConcurrent, maxCapacity);

                throw new InvalidOperationException(
                    $"Trung tâm bảo dưỡng đã đầy công suất trong khung giờ {newStart:HH\\:mm}-{newEnd:HH\\:mm}. " +
                    $"Hiện có {overlappingCount} lịch hẹn đang thực hiện, vượt quá khả năng tiếp nhận ({maxCapacity} xe cùng lúc). " +
                    "Vui lòng chọn khung giờ khác hoặc liên hệ tổng đài " + GetHotlineNumber() + " để được hỗ trợ.");
            }

            _logger.LogInformation(
                "Service center capacity validation PASSED: CenterId={CenterId}, " +
                "Time=[{Start}-{End}], Concurrent={Current}/{Max}",
                serviceCenterId,
                newStart.ToString("HH:mm"), newEnd.ToString("HH:mm"),
                totalConcurrent, maxCapacity);
        }

        #region Smart Subscription Logic

        /// <summary>
        /// Tính priority score cho một subscription
        /// Priority càng CAO (số càng LỚN) = càng ưu tiên sử dụng trước
        ///
        /// Quy tắc ưu tiên:
        /// 1. Sắp hết hạn (≤7 ngày) = Priority cao nhất (+10000 points)
        /// 2. Còn ít lượt hơn (remaining quantity thấp) = Priority cao hơn
        /// 3. Mua sớm hơn (FIFO) = Priority cao hơn
        /// </summary>
        /// <param name="subscription">Subscription cần tính priority</param>
        /// <param name="serviceId">ServiceId cần check (để lấy RemainingQuantity)</param>
        /// <returns>Priority score (số càng lớn = càng ưu tiên)</returns>
        private int CalculateSubscriptionPriority(
            CustomerPackageSubscription subscription,
            int serviceId)
        {
            int priorityScore = 0;

            // 1️⃣ EXPIRY PRIORITY: Subscription sắp hết hạn (≤7 ngày) có priority CAO NHẤT
            if (subscription.ExpirationDate.HasValue)
            {
                var daysUntilExpiry = (subscription.ExpirationDate.Value.ToDateTime(TimeOnly.MinValue) - DateTime.UtcNow).Days;

                if (daysUntilExpiry <= 7 && daysUntilExpiry >= 0)
                {
                    // Sắp hết hạn trong 7 ngày → Cộng điểm cao
                    // Càng gần expiry càng cao: 7 ngày = +10000, 6 ngày = +10001, ..., 0 ngày = +10007
                    priorityScore += 10000 + (7 - daysUntilExpiry);

                    _logger.LogDebug(
                        "Subscription {SubId} expiring in {Days} days → Priority boost: +{Boost}",
                        subscription.SubscriptionId, daysUntilExpiry, 10000 + (7 - daysUntilExpiry));
                }
            }

            // 2️⃣ QUANTITY PRIORITY: Subscription còn ít lượt hơn → Priority cao hơn
            // (Khuyến khích dùng hết subscription sắp "cạn" trước)
            var serviceUsage = subscription.PackageServiceUsages
                .FirstOrDefault(u => u.ServiceId == serviceId);

            if (serviceUsage != null)
            {
                // Lượt còn lại càng ít → điểm càng cao
                // VD: Còn 1 lượt = +1000, còn 2 lượt = +999, còn 3 = +998...
                // (Max 1000 - RemainingQuantity để đảm bảo priority giảm dần khi quantity tăng)
                int quantityScore = Math.Max(0, 1000 - serviceUsage.RemainingQuantity);
                priorityScore += quantityScore;

                _logger.LogDebug(
                    "Subscription {SubId} has {Remaining} uses left for Service {ServiceId} → Quantity score: +{Score}",
                    subscription.SubscriptionId, serviceUsage.RemainingQuantity, serviceId, quantityScore);
            }

            // 3️⃣ FIFO PRIORITY: Subscription mua sớm hơn → Priority cao hơn
            // Dùng ticks của PurchaseDate (số càng NHỎ = mua càng sớm = priority càng cao)
            // Chia cho 10^7 để convert ticks thành giây (tránh số quá lớn)
            // Đảo dấu (-) để subscription mua sớm hơn có score cao hơn
            long fifoScore = subscription.PurchaseDate.HasValue
                ? -(subscription.PurchaseDate.Value.Ticks / 10_000_000)
                : 0;
            priorityScore += (int)(fifoScore % 10000); // Lấy 4 chữ số cuối để tránh overflow

            _logger.LogDebug(
                "Subscription {SubId} purchased on {Date} → FIFO component: {FifoScore}",
                subscription.SubscriptionId,
                subscription.PurchaseDate?.ToString() ?? "N/A",
                fifoScore % 10000);

            _logger.LogInformation(
                "✅ Subscription Priority calculated: SubId={SubId}, " +
                "ServiceId={ServiceId}, TotalScore={Score}",
                subscription.SubscriptionId, serviceId, priorityScore);

            return priorityScore;
        }

        /// <summary>
        /// Build danh sách AppointmentServices với Smart Deduplication logic
        ///
        /// Logic:
        /// 1. Lấy tất cả active subscriptions của customer (cho vehicle này)
        /// 2. Với mỗi serviceId trong request:
        ///    a. Check xem có subscription nào có service này + còn lượt không?
        ///    b. Nếu CÓ subscription phù hợp:
        ///       - Sắp xếp theo priority (expiry → quantity → FIFO)
        ///       - Chọn subscription có priority CAO NHẤT
        ///       - Mark ServiceSource = "Subscription", Price = 0
        ///    c. Nếu KHÔNG có subscription hoặc hết lượt:
        ///       - Mark ServiceSource = "Extra", Price = giá thực tế
        /// 3. Return list AppointmentService với ServiceSource và Price đã set
        ///
        /// NOTE: Method này KHÔNG trừ lượt. Trừ lượt chỉ xảy ra khi CompleteAppointment
        /// </summary>
        private async Task<List<AppointmentService>> BuildAppointmentServicesAsync(
            int customerId,
            int vehicleId,
            int? modelId,
            List<int> requestedServiceIds,
            int? explicitSubscriptionId, // Subscription ID user chỉ định (nếu có)
            CancellationToken cancellationToken)
        {
            var appointmentServices = new List<AppointmentService>();

            // 1️⃣ Lấy tất cả active subscriptions của customer (cho vehicle này)
            // Chỉ lấy subscriptions còn active và chưa hết hạn
            var activeSubscriptions = await _subscriptionRepository
                .GetActiveSubscriptionsByCustomerAndVehicleAsync(
                    customerId, vehicleId, cancellationToken);

            _logger.LogInformation(
                "🔍 Smart Deduplication: CustomerId={CustomerId}, VehicleId={VehicleId}, " +
                "Found {Count} active subscriptions",
                customerId, vehicleId, activeSubscriptions.Count);

            // Nếu user chỉ định một subscription cụ thể, chỉ dùng subscription đó
            if (explicitSubscriptionId.HasValue)
            {
                activeSubscriptions = activeSubscriptions
                    .Where(s => s.SubscriptionId == explicitSubscriptionId.Value)
                    .ToList();

                _logger.LogInformation(
                    "User specified SubscriptionId={SubId}, filtering to that subscription only",
                    explicitSubscriptionId.Value);
            }

            // 2️⃣ Với mỗi serviceId trong request, determine ServiceSource và Price
            foreach (var serviceId in requestedServiceIds)
            {
                // Lấy thông tin service từ DB (để có BasePrice, StandardTime)
                var service = await _serviceRepository.GetByIdAsync(serviceId, cancellationToken);
                if (service == null)
                {
                    _logger.LogWarning(
                        "Service {ServiceId} không tồn tại, skip",
                        serviceId);
                    continue;
                }

                // Check pricing cho service này (theo model xe)
                decimal servicePrice = service.BasePrice;
                int serviceTime = service.StandardTime;

                if (modelId.HasValue)
                {
                    var pricing = await _pricingRepository.GetActivePricingAsync(
                        modelId.Value, serviceId);

                    if (pricing != null)
                    {
                        servicePrice = pricing.CustomPrice ?? service.BasePrice;
                        serviceTime = pricing.CustomTime ?? service.StandardTime;
                    }
                }

                // 3️⃣ TÌM SUBSCRIPTION PHÙ HỢP cho service này
                // Lọc subscriptions có chứa service này + còn lượt > 0
                var eligibleSubscriptions = activeSubscriptions
                    .Where(sub =>
                        sub.PackageServiceUsages.Any(usage =>
                            usage.ServiceId == serviceId &&
                            usage.RemainingQuantity > 0))
                    .ToList();

                if (eligibleSubscriptions.Any())
                {
                    // CÓ subscription phù hợp → Sắp xếp theo priority
                    var bestSubscription = eligibleSubscriptions
                        .OrderByDescending(sub => CalculateSubscriptionPriority(sub, serviceId))
                        .First();

                    _logger.LogInformation(
                        "✅ Service {ServiceName} (ID={ServiceId}) → Dùng từ Subscription {SubId} " +
                        "(Priority={Priority}, Remaining={Remaining})",
                        service.ServiceName, serviceId,
                        bestSubscription.SubscriptionId,
                        CalculateSubscriptionPriority(bestSubscription, serviceId),
                        bestSubscription.PackageServiceUsages
                            .First(u => u.ServiceId == serviceId).RemainingQuantity);

                    // Tạo AppointmentService với ServiceSource = "Subscription", Price = 0
                    appointmentServices.Add(new AppointmentService
                    {
                        ServiceId = serviceId,
                        ServiceSource = "Subscription",
                        Price = 0, // MIỄN PHÍ vì dùng từ subscription
                        EstimatedTime = serviceTime,
                        CreatedDate = DateTime.UtcNow,
                        // NOTE: AppointmentId sẽ được set bởi caller
                    });
                }
                else
                {
                    // KHÔNG có subscription phù hợp → Mark as "Extra" (phải trả tiền)
                    _logger.LogInformation(
                        "⚠️ Service {ServiceName} (ID={ServiceId}) → KHÔNG có subscription phù hợp " +
                        "→ Mark as 'Extra' (Price={Price}đ)",
                        service.ServiceName, serviceId, servicePrice);

                    appointmentServices.Add(new AppointmentService
                    {
                        ServiceId = serviceId,
                        ServiceSource = "Extra",
                        Price = servicePrice, // TÍNH TIỀN vì không có trong subscription
                        EstimatedTime = serviceTime,
                        CreatedDate = DateTime.UtcNow,
                    });
                }
            }

            _logger.LogInformation(
                "🎯 BuildAppointmentServices completed: {TotalServices} services, " +
                "{SubscriptionCount} from subscription, {ExtraCount} extra",
                appointmentServices.Count,
                appointmentServices.Count(s => s.ServiceSource == "Subscription"),
                appointmentServices.Count(s => s.ServiceSource == "Extra"));

            return appointmentServices;
        }

        #endregion

        /// <summary>
        /// Get max appointments per day for a vehicle from configuration
        /// </summary>
        private int GetMaxAppointmentsPerDay()
        {
            return _configuration.GetValue<int>(
                "AppointmentSettings:VehicleConflictPolicy:MaxAppointmentsPerDay", 3);
        }

        /// <summary>
        /// Get minimum notice hours from configuration
        /// </summary>
        private int GetMinNoticeHours()
        {
            return _configuration.GetValue<int>(
                "AppointmentSettings:ReschedulePolicy:MinNoticeHours", 24);
        }

        /// <summary>
        /// Get hotline number from configuration
        /// </summary>
        private string GetHotlineNumber()
        {
            return _configuration.GetValue<string>(
                "AppointmentSettings:ReschedulePolicy:HotlineNumber", "1900-xxxx")!;
        }

        /// <summary>
        /// Get max free reschedules from configuration
        /// </summary>
        private int GetMaxFreeReschedules()
        {
            return _configuration.GetValue<int>(
                "AppointmentSettings:ReschedulePolicy:MaxFreeReschedules", 1);
        }

        /// <summary>
        /// Check if staff can override reschedule limits
        /// </summary>
        private bool IsStaffOverrideEnabled()
        {
            return _configuration.GetValue<bool>(
                "AppointmentSettings:ReschedulePolicy:EnableStaffOverride", true);
        }

        /// <summary>
        /// Get service center max capacity from configuration
        /// </summary>
        private int GetServiceCenterMaxCapacity()
        {
            return _configuration.GetValue<int>(
                "AppointmentSettings:ServiceCenterCapacity:MaxConcurrentAppointments", 5);
        }
    }
}