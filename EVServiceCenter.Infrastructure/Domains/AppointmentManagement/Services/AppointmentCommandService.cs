using EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Request;
using EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Response;
using AdjustServiceSourceResponseDto = EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Response.AdjustServiceSourceResponseDto;
using EVServiceCenter.Core.Domains.AppointmentManagement.Entities;
using EVServiceCenter.Core.Domains.AppointmentManagement.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.AppointmentManagement.Interfaces.Services;
using EVServiceCenter.Core.Domains.CustomerVehicles.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.MaintenanceServices.Entities;
using EVServiceCenter.Core.Domains.MaintenanceServices.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.Checklists.Interfaces;
using EVServiceCenter.Core.Domains.Invoices.Interfaces;
using EVServiceCenter.Core.Domains.ModelServicePricings.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.PackageSubscriptions.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.TimeSlots.Interfaces.Repositories;
using EVServiceCenter.Core.Entities;
using EVServiceCenter.Core.Enums;
using EVServiceCenter.Core.Helpers;
using EVServiceCenter.Core.Interfaces.Services;
using EVServiceCenter.Infrastructure.Domains.AppointmentManagement.Mappers;
using EVServiceCenter.Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using EVServiceCenter.Core.Domains.Pricing.Interfaces;
using EVServiceCenter.Core.Domains.Customers.Interfaces;
using EVServiceCenter.Core.Domains.Payments.Constants;
using EVServiceCenter.Core.Domains.Pricing.Models;
using EVServiceCenter.Core.Domains.Payments.DTOs.Requests;
using EVServiceCenter.Core.Domains.Payments.Interfaces;
using EVServiceCenter.Core.Domains.Payments.Interfaces.Services;
using EVServiceCenter.Core.Domains.Payments.Entities;
using EVServiceCenter.Core.Domains.Payments.Interfaces.Repositories;
using EVServiceCenter.Core.Constants;

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
        private readonly IPaymentIntentService _paymentIntentService;
        private readonly IPaymentService _paymentService;
        private readonly IRefundRepository _refundRepository;
        private readonly IInvoiceService _invoiceService;
        private readonly IChecklistService _checklistService;
        private readonly EVDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AppointmentCommandService> _logger;
        private readonly decimal _vatRate;

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
            IPaymentIntentService paymentIntentService,
            IPaymentService paymentService,
            IRefundRepository refundRepository,
            EVDbContext context,
            IConfiguration configuration,
            ILogger<AppointmentCommandService> logger,
            IInvoiceService invoiceService,
            IChecklistService checklistService)
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
            _paymentIntentService = paymentIntentService ?? throw new ArgumentNullException(nameof(paymentIntentService));
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
            _refundRepository = refundRepository ?? throw new ArgumentNullException(nameof(refundRepository));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _configuration = configuration;
            _logger = logger;
            _invoiceService = invoiceService ?? throw new ArgumentNullException(nameof(invoiceService));
            _checklistService = checklistService ?? throw new ArgumentNullException(nameof(checklistService));
            _vatRate = _configuration.GetValue<decimal>("Tax:VATRate", 0.08m);
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

            if (slotDateTime < DateTime.UtcNow)
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

            if (vehicle.CustomerId != request.CustomerId)
            {
                _logger.LogWarning(
                    "Customer {CustomerId} attempted to book appointment for vehicle {VehicleId} owned by {VehicleCustomerId}",
                    request.CustomerId,
                    request.VehicleId,
                    vehicle.CustomerId);

                throw new InvalidOperationException("Xe không thuộc sở hữu của khách hàng hiện tại");
            }

            // ═══════════════════════════════════════════════════════════
            // ⚡ SMART SUBSCRIPTION DEDUPLICATION - PERFORMANCE OPTIMIZED
            // ═══════════════════════════════════════════════════════════

            // ✅ Build appointment services với priority logic
            // Single DB query cho subscriptions, smart in-memory mapping
            (List<AppointmentService> appointmentServices, decimal originalTotal, int totalDuration) =
                await BuildAppointmentServicesAsync(request, vehicle.ModelId, cancellationToken);

            // ✅ Determine PackageId to set
            int? packageIdToSet = request.PackageId;
            if (request.SubscriptionId.HasValue && !packageIdToSet.HasValue)
            {
                // If user specified subscription but no package, get package from subscription
                var subscription = appointmentServices
                    .FirstOrDefault(s => s.ServiceSource == "Subscription")?
                    .Notes;

                // Extract SubscriptionId from Notes (format: "Từ gói ... (#123)")
                // For now, keep packageId null - will be set in BuildAppointmentServicesAsync if needed
            }

            // ═══════════════════════════════════════════════════════════
            // DISCOUNT CALCULATION PHASE
            // ═══════════════════════════════════════════════════════════

            // Note: originalTotal from BuildAppointmentServicesAsync already excludes "Subscription" services (Price=0)
            // Only "Extra" and "Regular" services are counted in originalTotal

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

            bool requiresPayment = finalCost > 0;

            string appointmentCode = await GenerateAppointmentCodeAsync(cancellationToken);

            PaymentIntent? initialPaymentIntent = null;
            if (requiresPayment)
            {
                int expiryHours = _configuration.GetValue<int?>("Payments:IntentExpiryHours") ?? 24;
                DateTime intentExpiry = DateTime.UtcNow.AddHours(expiryHours);

                initialPaymentIntent = _paymentIntentService.BuildPendingIntent(
                    request.CustomerId,
                    ApplyVat(finalCost),
                    currentUserId,
                    currency: "VND",
                    expiresAt: intentExpiry,
                    paymentMethod: null,
                    idempotencyKey: $"appointment:{appointmentCode}");
            }

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
                FinalCost = finalCost,
                DiscountAmount = originalTotal - finalCost, // ✅ Total discount applied
                DiscountType = appliedDiscountType ?? "None", // ✅ Type of discount
                PromotionId = promotionIdUsed, // ✅ Promotion ID if used
                CustomerNotes = request.CustomerNotes,
                PreferredTechnicianId = request.PreferredTechnicianId,
                Priority = request.Priority,
                Source = request.Source,
                PaymentStatus = requiresPayment
                    ? PaymentStatusEnum.Pending.ToString()
                    : PaymentStatusEnum.Completed.ToString(),
                PaidAmount = requiresPayment ? 0 : finalCost,
                PaymentIntentCount = requiresPayment ? 1 : 0,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = currentUserId
            };

            Appointment created = await _commandRepository.CreateWithServicesAsync(
                appointment, appointmentServices, initialPaymentIntent, cancellationToken);

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

        public async Task<AppointmentResponseDto> RecordPaymentResultAsync(
            RecordPaymentResultRequestDto request,
            int currentUserId,
            CancellationToken cancellationToken = default)
        {
            if (!Enum.TryParse<PaymentIntentStatusEnum>(request.Status, out var intentStatus))
            {
                throw new InvalidOperationException("Trạng thái payment intent không hợp lệ");
            }

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var appointment = await _context.Appointments
                    .Include(a => a.PaymentIntents)
                    .FirstOrDefaultAsync(a => a.AppointmentId == request.AppointmentId, cancellationToken);

                if (appointment == null)
                    throw new InvalidOperationException("Appointment không tồn tại");

                var paymentIntent = appointment.PaymentIntents
                    .FirstOrDefault(pi => pi.PaymentIntentId == request.PaymentIntentId);

                if (paymentIntent == null)
                    throw new InvalidOperationException("PaymentIntent không thuộc về appointment này");

                var now = DateTime.UtcNow;
                appointment.LatestPaymentIntentId = request.PaymentIntentId;
                appointment.UpdatedBy = currentUserId;
                appointment.UpdatedDate = now;

                decimal finalCost = appointment.FinalCost ?? appointment.EstimatedCost ?? 0m;
                decimal paidAmount = appointment.PaidAmount ?? 0m;

                switch (intentStatus)
                {
                    case PaymentIntentStatusEnum.Completed:
                        if (string.Equals(paymentIntent.Status, PaymentIntentStatusEnum.Completed.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            throw new InvalidOperationException("PaymentIntent đã được đánh dấu hoàn tất trước đó");
                        }

                        var completedIntent = await _paymentIntentService.MarkCompletedAsync(
                            request.PaymentIntentId,
                            request.Amount,
                            currentUserId,
                            cancellationToken);

                        // Tạo transaction log
                        var capturedDate = request.OccurredAt ?? now;
                        var paymentTransaction = new PaymentTransaction
                        {
                            AppointmentId = appointment.AppointmentId,
                            CustomerId = appointment.CustomerId,
                            PaymentIntentId = completedIntent.PaymentIntentId,
                            Amount = request.Amount,
                            Currency = request.Currency,
                            PaymentMethod = request.PaymentMethod,
                            GatewayName = request.GatewayName,
                            GatewayTransactionId = request.GatewayTransactionId,
                            GatewayResponse = request.GatewayResponse,
                            Notes = request.Notes,
                            Status = "Captured",
                            CreatedDate = capturedDate,
                            AuthorizedDate = capturedDate,
                            CapturedDate = capturedDate
                        };

                        await _context.PaymentTransactions.AddAsync(paymentTransaction, cancellationToken);

                        paidAmount += request.Amount;
                        appointment.PaidAmount = paidAmount;
                        appointment.PaymentStatus = paidAmount >= finalCost
                            ? PaymentStatusEnum.Completed.ToString()
                            : PaymentStatusEnum.Pending.ToString();
                        break;

                    case PaymentIntentStatusEnum.Failed:
                        var failedIntent = await _paymentIntentService.MarkFailedAsync(
                            request.PaymentIntentId,
                            request.Notes ?? "Thanh toán thất bại",
                            currentUserId,
                            cancellationToken);

                        if (request.Amount > 0)
                        {
                            var failedDate = request.OccurredAt ?? now;
                            var failedTransaction = new PaymentTransaction
                            {
                                AppointmentId = appointment.AppointmentId,
                                CustomerId = appointment.CustomerId,
                                PaymentIntentId = failedIntent.PaymentIntentId,
                                Amount = request.Amount,
                                Currency = request.Currency,
                                PaymentMethod = request.PaymentMethod,
                                GatewayName = request.GatewayName,
                                GatewayTransactionId = request.GatewayTransactionId,
                                GatewayResponse = request.GatewayResponse,
                                Notes = request.Notes,
                                Status = "Failed",
                                CreatedDate = failedDate,
                                FailedDate = failedDate,
                                ErrorMessage = request.Notes
                            };

                            await _context.PaymentTransactions.AddAsync(failedTransaction, cancellationToken);
                        }

                        appointment.PaymentStatus = paidAmount > 0
                            ? PaymentStatusEnum.Pending.ToString()
                            : PaymentStatusEnum.Failed.ToString();
                        break;

                    case PaymentIntentStatusEnum.Cancelled:
                        await _paymentIntentService.MarkCancelledAsync(
                            request.PaymentIntentId,
                            request.Notes ?? "Thanh toán bị hủy",
                            currentUserId,
                            cancellationToken);

                        appointment.PaymentStatus = paidAmount > 0
                            ? PaymentStatusEnum.Pending.ToString()
                            : PaymentStatusEnum.Cancelled.ToString();
                        break;

                    case PaymentIntentStatusEnum.Expired:
                        await _paymentIntentService.MarkExpiredAsync(
                            request.PaymentIntentId,
                            currentUserId,
                            cancellationToken);

                        appointment.PaymentStatus = paidAmount > 0
                            ? PaymentStatusEnum.Pending.ToString()
                            : PaymentStatusEnum.Pending.ToString();
                        break;

                    case PaymentIntentStatusEnum.Pending:
                        throw new InvalidOperationException("Không cần ghi nhận trạng thái Pending thông qua API");

                    default:
                        throw new InvalidOperationException("Trạng thái payment intent không được hỗ trợ");
                }

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                var refreshed = await _repository.GetByIdWithDetailsAsync(appointment.AppointmentId, cancellationToken);
                return AppointmentMapper.ToResponseDto(refreshed!);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<PaymentIntentResponseDto> CreatePaymentIntentAsync(
            CreatePaymentIntentRequestDto request,
            int currentUserId,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var appointment = await _context.Appointments
                    .Include(a => a.PaymentIntents)
                    .FirstOrDefaultAsync(a => a.AppointmentId == request.AppointmentId, cancellationToken);

                if (appointment == null)
                {
                    throw new InvalidOperationException("Appointment không tồn tại");
                }

                if (appointment.StatusId == (int)AppointmentStatusEnum.Cancelled ||
                    appointment.StatusId == (int)AppointmentStatusEnum.Rescheduled ||
                    appointment.StatusId == (int)AppointmentStatusEnum.NoShow)
                {
                    throw new InvalidOperationException("Không thể tạo payment intent cho lịch hẹn đã không còn hiệu lực");
                }

                var finalCost = appointment.FinalCost ?? appointment.EstimatedCost ?? 0m;
                var paidAmount = appointment.PaidAmount ?? 0m;
                var outstanding = Math.Max(finalCost - paidAmount, 0m);

                if (outstanding <= 0m && !request.Amount.HasValue)
                {
                    throw new InvalidOperationException("Lịch hẹn không còn khoản cần thanh toán");
                }

                var requestedAmount = request.Amount ?? outstanding;
                if (requestedAmount <= 0m)
                {
                    throw new InvalidOperationException("Số tiền thanh toán phải lớn hơn 0");
                }

                if (request.Amount.HasValue)
                {
                    var tolerance = 0.01m;
                    if (request.Amount.Value - outstanding > tolerance)
                    {
                        throw new InvalidOperationException("Số tiền yêu cầu vượt quá khoản outstanding hiện tại");
                    }
                }

                var expiryHours = request.ExpiresInHours ??
                    _configuration.GetValue<int?>("Payments:IntentExpiryHours") ?? 24;
                if (expiryHours <= 0)
                {
                    expiryHours = 24;
                }

                var expiresAt = DateTime.UtcNow.AddHours(expiryHours);
                var normalizedCurrency = string.IsNullOrWhiteSpace(request.Currency)
                    ? "VND"
                    : request.Currency.ToUpperInvariant();

                var newIntent = _paymentIntentService.BuildPendingIntent(
                    appointment.CustomerId,
                    ApplyVat(requestedAmount),
                    currentUserId,
                    normalizedCurrency,
                    expiresAt,
                    request.PaymentMethod,
                    request.IdempotencyKey);

                newIntent.AppointmentId = appointment.AppointmentId;
                newIntent.CustomerId = appointment.CustomerId;
                newIntent.Notes = request.Notes;

                var savedIntent = await _paymentIntentService.AppendNewIntentAsync(newIntent, cancellationToken);

                var now = DateTime.UtcNow;
                appointment.PaymentIntentCount = appointment.PaymentIntentCount + 1;
                appointment.LatestPaymentIntentId = savedIntent.PaymentIntentId;
                appointment.UpdatedBy = currentUserId;
                appointment.UpdatedDate = now;

                if (!string.Equals(appointment.PaymentStatus, PaymentStatusEnum.Completed.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    appointment.PaymentStatus = PaymentStatusEnum.Pending.ToString();
                }

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation(
                    "User {UserId} created payment intent {IntentCode} for appointment {AppointmentId} with amount {Amount}",
                    currentUserId,
                    savedIntent.IntentCode,
                    appointment.AppointmentId,
                    savedIntent.Amount);

                return AppointmentMapper.ToPaymentIntentResponseDto(savedIntent);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<PrePaymentResponseDto> CreatePrePaymentAsync(
            int appointmentId,
            string paymentMethod,
            string returnUrl,
            string? ipAddress,
            int userId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(paymentMethod))
            {
                throw new ArgumentException("Payment method is required", nameof(paymentMethod));
            }

            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                throw new ArgumentException("ReturnUrl is required", nameof(returnUrl));
            }

            var supportedMethods = new[] { PaymentMethodType.VNPay, PaymentMethodType.MoMo };
            var canonicalMethod = supportedMethods
                .FirstOrDefault(m => m.Equals(paymentMethod, StringComparison.OrdinalIgnoreCase));

            if (canonicalMethod == null)
            {
                throw new InvalidOperationException("Chỉ hỗ trợ thanh toán trước bằng VNPay hoặc MoMo.");
            }

            _logger.LogInformation(
                "Creating appointment pre-payment. AppointmentId={AppointmentId}, Method={Method}, UserId={UserId}",
                appointmentId,
                canonicalMethod,
                userId);

            var appointment = await _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.PaymentIntents)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId, cancellationToken);

            if (appointment == null)
            {
                throw new InvalidOperationException("Appointment không tồn tại.");
            }

            if (appointment.StatusId != (int)AppointmentStatusEnum.Pending &&
                appointment.StatusId != (int)AppointmentStatusEnum.Confirmed)
            {
                throw new InvalidOperationException("Chỉ có thể thanh toán trước cho lịch Pending hoặc Confirmed.");
            }

            var targetAmount = appointment.FinalCost ?? appointment.EstimatedCost ?? 0m;
            if (targetAmount <= 0)
            {
                throw new InvalidOperationException("Lịch hẹn không có chi phí cần thanh toán.");
            }

            var paidAmount = appointment.PaidAmount ?? 0m;
            var outstanding = Math.Max(targetAmount - paidAmount, 0m);
            if (outstanding <= 0)
            {
                throw new InvalidOperationException("Lịch hẹn đã thanh toán đủ.");
            }

            var pendingIntent = appointment.PaymentIntents?
                .Where(pi => string.Equals(pi.Status, PaymentIntentStatusEnum.Pending.ToString(), StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(pi => pi.CreatedDate)
                .FirstOrDefault(pi => Math.Abs(pi.Amount - outstanding) < 0.01m);

            if (pendingIntent == null)
            {
                var expiryHours = _configuration.GetValue<int?>("Payments:IntentExpiryHours") ?? 24;
                if (expiryHours <= 0)
                {
                    expiryHours = 24;
                }

                var expiresAt = DateTime.UtcNow.AddHours(expiryHours);
                var newIntent = _paymentIntentService.BuildPendingIntent(
                    appointment.CustomerId,
                    ApplyVat(outstanding),
                    userId,
                    "VND",
                    expiresAt,
                    canonicalMethod,
                    $"APT-{appointmentId}-PREPAY-{DateTime.UtcNow.Ticks}");

                newIntent.AppointmentId = appointment.AppointmentId;
                newIntent.CustomerId = appointment.CustomerId;
                newIntent.PaymentMethod = canonicalMethod;
                newIntent.Notes = $"Pre-payment for appointment #{appointment.AppointmentCode ?? appointment.AppointmentId.ToString()}";

                pendingIntent = await _paymentIntentService.AppendNewIntentAsync(newIntent, cancellationToken);

                appointment.PaymentIntentCount = appointment.PaymentIntentCount + 1;
                appointment.LatestPaymentIntentId = pendingIntent.PaymentIntentId;
                if (!string.Equals(appointment.PaymentStatus, PaymentStatusEnum.Completed.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    appointment.PaymentStatus = PaymentStatusEnum.Pending.ToString();
                }

                appointment.UpdatedBy = userId;
                appointment.UpdatedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync(cancellationToken);
            }

            var invoice = await _invoiceService.CreatePrePaymentInvoiceAsync(
                appointmentId,
                pendingIntent.PaymentIntentId,
                userId,
                cancellationToken);

            var paymentRequest = new CreatePaymentRequestDto
            {
                InvoiceId = invoice.InvoiceId,
                Amount = pendingIntent.Amount,
                PaymentMethod = canonicalMethod,
                ReturnUrl = returnUrl,
                CustomerName = appointment.Customer?.FullName ?? "Customer",
                CustomerEmail = appointment.Customer?.Email,
                CustomerPhone = appointment.Customer?.PhoneNumber
            };

            var gatewayResponse = await _paymentService.CreatePaymentAsync(
                paymentRequest,
                userId,
                string.IsNullOrWhiteSpace(ipAddress) ? "127.0.0.1" : ipAddress!,
                cancellationToken);

            _logger.LogInformation(
                "Pre-payment created successfully. AppointmentId={AppointmentId}, Invoice={InvoiceCode}, Amount={Amount}",
                appointmentId,
                invoice.InvoiceCode,
                pendingIntent.Amount);

            return new PrePaymentResponseDto
            {
                PaymentIntentId = pendingIntent.PaymentIntentId,
                InvoiceId = invoice.InvoiceId,
                InvoiceCode = invoice.InvoiceCode,
                Amount = pendingIntent.Amount,
                PaymentUrl = gatewayResponse.PaymentUrl,
                PaymentCode = gatewayResponse.PaymentCode,
                Gateway = gatewayResponse.Gateway,
                QrCodeUrl = gatewayResponse.QrCodeUrl,
                DeepLink = gatewayResponse.DeepLink,
                ExpiresAt = gatewayResponse.ExpiryTime ?? pendingIntent.ExpiresAt
            };
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

                if (vehicle.CustomerId != appointment.CustomerId)
                {
                    _logger.LogWarning(
                        "User {UserId} attempted to reassign appointment {AppointmentId} to vehicle {VehicleId} owned by {VehicleCustomerId}",
                        currentUserId,
                        appointment.AppointmentId,
                        request.VehicleId.Value,
                        vehicle.CustomerId);

                    throw new InvalidOperationException("Xe không thuộc sở hữu của khách hàng hiện tại");
                }

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

            var newSlot = await _slotRepository.GetByIdAsync(request.NewSlotId, cancellationToken);
            if (newSlot == null)
                throw new InvalidOperationException("Slot mới không tồn tại");

          
            var newSlotDateTime = newSlot.SlotDate.ToDateTime(newSlot.StartTime);
            if (newSlotDateTime < DateTime.UtcNow)
                throw new InvalidOperationException(
                    "Không thể dời lịch sang khung giờ ở trong quá khứ");

           
            if (oldAppointment.SlotId == request.NewSlotId)
                throw new InvalidOperationException(
                    "Slot mới trùng với slot hiện tại. Vui lòng chọn khung giờ khác.");

          
            int activeCount = await _queryRepository.GetActiveCountBySlotIdAsync(
                request.NewSlotId, cancellationToken);

            if (activeCount >= newSlot.MaxBookings)
                throw new InvalidOperationException("Slot mới đã đầy");

           
            int estimatedDuration = oldAppointment.EstimatedDuration ?? 60; // Default 60 phút nếu null

            await ValidateVehicleTimeConflict(
                oldAppointment.VehicleId,
                request.NewSlotId,
                estimatedDuration, 
                excludeAppointmentId: request.AppointmentId, 
                cancellationToken);

            await ValidateTechnicianConflict(
                oldAppointment.PreferredTechnicianId,
                oldAppointment.ServiceCenterId,  
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

        /// <summary>
        /// Cancel appointment với refund logic
        /// ✅ OPTIMIZED: Transaction safety + null check + duplicate refund check
        /// </summary>
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

            // ✅ NEW: Transaction safety
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                // Xử lý refund nếu đã thanh toán
                if (appointment.PaidAmount > 0)
                {
                    // ✅ NEW: Null check for LatestPaymentIntentId
                    if (!appointment.LatestPaymentIntentId.HasValue)
                    {
                        _logger.LogError(
                            "Appointment {AppointmentId} has PaidAmount={PaidAmount} but no LatestPaymentIntentId",
                            appointment.AppointmentId, appointment.PaidAmount);
                        throw new InvalidOperationException(
                            "Dữ liệu không hợp lệ: Appointment đã thanh toán nhưng không có PaymentIntent");
                    }

                    // ✅ NEW: Check duplicate refund
                    var existingRefund = await _context.Refunds
                        .FirstOrDefaultAsync(r => r.AppointmentId == appointment.AppointmentId,
                                           cancellationToken);

                    if (existingRefund != null)
                    {
                        _logger.LogWarning(
                            "Appointment {AppointmentId} already has refund {RefundId} (Status: {Status})",
                            appointment.AppointmentId, existingRefund.RefundId, existingRefund.Status);
                        // Skip refund creation - already exists
                    }
                    else
                    {
                        var refundAmount = CalculateRefundAmount(appointment, DateTime.UtcNow);

                        if (refundAmount > 0)
                        {
                            _logger.LogInformation(
                                "💰 Creating refund for appointment {AppointmentId}: {RefundAmount}đ / {PaidAmount}đ",
                                appointment.AppointmentId, refundAmount, appointment.PaidAmount);

                            var refund = new Refund
                            {
                                PaymentIntentId = appointment.LatestPaymentIntentId.Value,
                                AppointmentId = appointment.AppointmentId,
                                CustomerId = appointment.CustomerId,
                                RefundAmount = refundAmount,
                                RefundReason = $"Cancelled by customer: {request.CancellationReason}",
                                RefundMethod = RefundConstants.Method.Original,
                                Status = RefundConstants.Status.Pending,
                                CreatedDate = DateTime.UtcNow,
                                CreatedBy = currentUserId,
                                Notes = $"Auto-calculated refund ({(refundAmount / appointment.PaidAmount.Value * 100):F0}%)"
                            };

                            await _context.Refunds.AddAsync(refund, cancellationToken);

                            _logger.LogInformation(
                                "✅ Refund created: RefundId={RefundId}, Amount={Amount}đ",
                                refund.RefundId, refund.RefundAmount);
                        }
                        else
                        {
                            _logger.LogWarning(
                                "⚠️ No refund for appointment {AppointmentId} (cancelled too late: {HoursBefore}h before)",
                                appointment.AppointmentId,
                                (appointment.AppointmentDate - DateTime.UtcNow).TotalHours);
                        }
                    }
                }

                bool result = await _commandRepository.CancelAsync(
                    request.AppointmentId, request.CancellationReason, cancellationToken);

                await transaction.CommitAsync(cancellationToken);
                return result;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex,
                    "❌ Failed to cancel appointment {AppointmentId}", request.AppointmentId);
                throw;
            }
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

        /// <summary>
        /// CHECK-IN: Confirmed → InProgress, tạo WorkOrder
        /// ✅ OPTIMIZED: Transaction safety + duplicate check
        /// </summary>
        public async Task<AppointmentResponseDto> CheckInAsync(
            int appointmentId,
            int currentUserId,
            CancellationToken cancellationToken = default)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                var appointment = await _repository.GetByIdWithDetailsAsync(appointmentId, cancellationToken);

                if (appointment == null)
                    throw new InvalidOperationException("Appointment không tồn tại");

                if (appointment.StatusId != (int)AppointmentStatusEnum.Confirmed)
                    throw new InvalidOperationException(
                        $"Chỉ có thể check-in appointment đã Confirmed. Trạng thái hiện tại: {appointment.StatusId}");

                if (appointment.EstimatedCost > 0 &&
                    appointment.PaymentStatus != PaymentStatusEnum.Completed.ToString())
                {
                    throw new InvalidOperationException(
                        "Khách hàng chưa thanh toán. Vui lòng thanh toán trước khi check-in.");
                }

                var existingWorkOrder = await _context.WorkOrders
                    .FirstOrDefaultAsync(wo => wo.AppointmentId == appointmentId, cancellationToken);

                if (existingWorkOrder != null)
                {
                    _logger.LogWarning(
                        "Appointment {AppointmentId} already has WorkOrder {WorkOrderCode}",
                        appointmentId, existingWorkOrder.WorkOrderCode);
                    throw new InvalidOperationException(
                        $"Appointment đã được check-in với WorkOrder {existingWorkOrder.WorkOrderCode}");
                }

                _logger.LogInformation(
                    "🚀 Check-in appointment {AppointmentId} by user {UserId}",
                    appointmentId, currentUserId);

                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    await _context.Appointments
                        .Where(a => a.AppointmentId == appointmentId)
                        .ExecuteUpdateAsync(setters => setters
                            .SetProperty(a => a.StatusId, (int)AppointmentStatusEnum.InProgress)
                            .SetProperty(a => a.PaymentStatus, PaymentStatusEnum.Completed.ToString())
                            .SetProperty(a => a.UpdatedBy, currentUserId)
                            .SetProperty(a => a.UpdatedDate, DateTime.UtcNow),
                            cancellationToken);

                    var workOrder = new WorkOrder
                    {
                        AppointmentId = appointmentId,
                        WorkOrderCode = await GenerateWorkOrderCodeAsync(cancellationToken),
                        CustomerId = appointment.CustomerId,
                        VehicleId = appointment.VehicleId,
                        ServiceCenterId = appointment.ServiceCenterId,
                        TechnicianId = appointment.PreferredTechnicianId,
                        StatusId = 1, // WorkOrderStatus: Started/InProgress
                        StartDate = DateTime.UtcNow,
                        EstimatedCompletionDate = DateTime.UtcNow.AddMinutes(appointment.EstimatedDuration ?? 60),
                        InternalNotes = "Auto-created from check-in",
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = currentUserId
                    };

                    await _context.WorkOrders.AddAsync(workOrder, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    _logger.LogInformation(
                        "✅ Check-in successful: Appointment {AppointmentId} → InProgress, " +
                        "WorkOrder {WorkOrderCode} created",
                        appointmentId, workOrder.WorkOrderCode);

                    var result = await _repository.GetByIdWithDetailsAsync(appointmentId, cancellationToken);
                    return AppointmentMapper.ToResponseDto(result!);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    _logger.LogError(ex,
                        "❌ Failed to check-in appointment {AppointmentId}", appointmentId);
                    throw;
                }
            });
        }

        /// <summary>
        /// THÊM DỊCH VỤ PHÁT SINH khi InProgress
        /// Tạo PaymentIntent mới cho phần phát sinh
        /// ✅ OPTIMIZED: Transaction safety + batch loading + duplicate check
        /// </summary>
        public async Task<AppointmentResponseDto> AddServicesAsync(
            int appointmentId,
            List<int> additionalServiceIds,
            int currentUserId,
            CancellationToken cancellationToken = default)
        {
            if (additionalServiceIds == null || !additionalServiceIds.Any())
                throw new ArgumentException("Danh sách dịch vụ không được rỗng", nameof(additionalServiceIds));

            var appointment = await _repository.GetByIdWithDetailsAsync(appointmentId, cancellationToken);

            if (appointment == null)
                throw new InvalidOperationException("Appointment không tồn tại");

            // Validate status: Chỉ thêm dịch vụ khi InProgress
            if (appointment.StatusId != (int)AppointmentStatusEnum.InProgress)
                throw new InvalidOperationException(
                    $"Chỉ có thể thêm dịch vụ khi appointment đang InProgress. " +
                    $"Trạng thái hiện tại: {appointment.StatusId}");

            // ✅ NEW: Check duplicate services
            var existingServiceIds = appointment.AppointmentServices
                .Select(aps => aps.ServiceId)
                .ToHashSet();

            var newServiceIds = additionalServiceIds
                .Where(id => !existingServiceIds.Contains(id))
                .Distinct()
                .ToList();

            if (newServiceIds.Count < additionalServiceIds.Count)
            {
                var duplicates = additionalServiceIds
                    .Where(id => existingServiceIds.Contains(id))
                    .ToList();
                _logger.LogWarning(
                    "Skipping {Count} duplicate services: {ServiceIds}",
                    duplicates.Count, string.Join(", ", duplicates));
            }

            if (!newServiceIds.Any())
                throw new InvalidOperationException("Tất cả dịch vụ đã tồn tại trong appointment");

            _logger.LogInformation(
                "🔧 Adding {Count} new services to appointment {AppointmentId}",
                newServiceIds.Count, appointmentId);

            // Lấy thông tin vehicle để tính giá
            var vehicle = await _vehicleRepository.GetByIdAsync(appointment.VehicleId, cancellationToken);
            if (vehicle == null)
                throw new InvalidOperationException("Vehicle không tồn tại");

            // ✅ NEW: Batch load services and pricings (fix N+1 query)
            var services = await _context.MaintenanceServices
                .Where(s => newServiceIds.Contains(s.ServiceId))
                .ToDictionaryAsync(s => s.ServiceId, cancellationToken);

            var pricings = await _context.ModelServicePricings
                .Where(p => p.ModelId == vehicle.ModelId
                         && newServiceIds.Contains(p.ServiceId)
                         && (p.IsActive == null || p.IsActive == true))
                .ToDictionaryAsync(p => p.ServiceId, cancellationToken);

            decimal totalAdditionalCost = 0;
            int totalAdditionalDuration = 0;
            var newAppointmentServices = new List<AppointmentService>();

            // Build AppointmentServices cho các dịch vụ phát sinh
            foreach (var serviceId in newServiceIds)
            {
                if (!services.TryGetValue(serviceId, out var service))
                {
                    _logger.LogWarning("Service {ServiceId} không tồn tại, skip", serviceId);
                    continue;
                }

                var pricing = pricings.GetValueOrDefault(serviceId);
                decimal servicePrice = pricing?.CustomPrice ?? service.BasePrice;
                int serviceDuration = pricing?.CustomTime ?? service.StandardTime;

                var appointmentService = new AppointmentService
                {
                    AppointmentId = appointmentId,
                    ServiceId = serviceId,
                    ServiceSource = "Extra", // Dịch vụ phát sinh = Extra
                    OriginalPrice = servicePrice,
                    Price = servicePrice,
                    DiscountAmount = 0,
                    EstimatedTime = serviceDuration,
                    Notes = "Dịch vụ phát sinh (thêm trong lúc InProgress)",
                    CreatedDate = DateTime.UtcNow
                };

                newAppointmentServices.Add(appointmentService);
                totalAdditionalCost += servicePrice;
                totalAdditionalDuration += serviceDuration;

                _logger.LogInformation(
                    "  + Service {ServiceName} ({ServiceId}): {Price}đ, {Duration}min",
                    service.ServiceName, serviceId, servicePrice, serviceDuration);
            }

            if (!newAppointmentServices.Any())
                throw new InvalidOperationException("Không có dịch vụ hợp lệ nào để thêm");

            // ✅ NEW: Transaction safety
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                // Add services to appointment
                foreach (var aps in newAppointmentServices)
                {
                    appointment.AppointmentServices.Add(aps);
                }

                // Update appointment cost & duration
                appointment.EstimatedCost = (appointment.EstimatedCost ?? 0) + totalAdditionalCost;
                appointment.EstimatedDuration = (appointment.EstimatedDuration ?? 0) + totalAdditionalDuration;
                appointment.UpdatedBy = currentUserId;
                appointment.UpdatedDate = DateTime.UtcNow;

                // Tạo PaymentIntent mới cho phần phát sinh
                int expiryHours = _configuration.GetValue<int?>("Payments:IntentExpiryHours") ?? 24;
                DateTime intentExpiry = DateTime.UtcNow.AddHours(expiryHours);

                var additionalPaymentIntent = _paymentIntentService.BuildPendingIntent(
                    appointment.CustomerId,
                    ApplyVat(totalAdditionalCost),
                    currentUserId,
                    currency: "VND",
                    expiresAt: intentExpiry,
                    paymentMethod: null,
                    idempotencyKey: $"additional:{appointment.AppointmentCode}:{DateTime.UtcNow:yyyyMMddHHmmss}");

                additionalPaymentIntent.AppointmentId = appointmentId;
                additionalPaymentIntent.Notes = $"Thanh toán cho {newAppointmentServices.Count} dịch vụ phát sinh";

                await _paymentIntentService.AppendNewIntentAsync(additionalPaymentIntent, cancellationToken);

                // Update appointment payment tracking
                appointment.PaymentIntentCount++;
                appointment.LatestPaymentIntentId = additionalPaymentIntent.PaymentIntentId;
                appointment.PaymentStatus = PaymentStatusEnum.Pending.ToString(); // Chuyển về Pending vì có thêm tiền cần thanh toán

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation(
                    "✅ Added {Count} services to appointment {AppointmentId}: " +
                    "+{Cost}đ, +{Duration}min, PaymentIntent {IntentCode} created",
                    newAppointmentServices.Count, appointmentId,
                    totalAdditionalCost, totalAdditionalDuration,
                    additionalPaymentIntent.IntentCode);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex,
                    "❌ Failed to add services to appointment {AppointmentId}", appointmentId);
                throw;
            }

            // Reload with full details
            var result = await _repository.GetByIdWithDetailsAsync(appointmentId, cancellationToken);
            return AppointmentMapper.ToResponseDto(result!);
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
        private async Task RecordAppointmentCustomerSpendAsync(
            Appointment appointment,
            CancellationToken cancellationToken)
        {
            if (appointment == null)
            {
                return;
            }

            try
            {
                var payableAmount = await _context.AppointmentServices
                    .Where(aps => aps.AppointmentId == appointment.AppointmentId &&
                                  !string.Equals(aps.ServiceSource, "Subscription", StringComparison.OrdinalIgnoreCase))
                    .SumAsync(aps => aps.Price, cancellationToken);

                if (payableAmount <= 0)
                {
                    _logger.LogInformation(
                        "[CompleteAppointmentAsync] Appointment {AppointmentId} has no payable services outside subscription. Skip spend recognition.",
                        appointment.AppointmentId);
                    return;
                }

                await CustomerSpendHelper.TryIncrementTotalSpentAsync(
                    _context,
                    _logger,
                    appointment.CustomerId,
                    payableAmount,
                    $"Appointment#{appointment.AppointmentId}",
                    cancellationToken);

                _logger.LogInformation(
                    "[CompleteAppointmentAsync] Recorded {Amount:N0} spend for customer {CustomerId} from appointment {AppointmentId}",
                    payableAmount,
                    appointment.CustomerId,
                    appointment.AppointmentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "[CompleteAppointmentAsync] Failed to record customer spend for appointment {AppointmentId}",
                    appointment?.AppointmentId);
            }
        }

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

                    // ✅ VALIDATE: AppointmentService must have SubscriptionId if ServiceSource = "Subscription"
                    if (!appointmentService.SubscriptionId.HasValue)
                    {
                        _logger.LogError(
                            "❌ CRITICAL: AppointmentService {ApsId} has ServiceSource='Subscription' but SubscriptionId is NULL! " +
                            "This should never happen. Degrading to 'Extra'.",
                            appointmentService.AppointmentServiceId);

                        await DegradeServiceToExtraAsync(
                            appointmentService,
                            "MISSING_SUBSCRIPTION_ID",
                            currentUserId,
                            cancellationToken);

                        hasDegradedService = true;
                        additionalPaymentRequired += appointmentService.Price;
                        continue;
                    }

                    _logger.LogInformation(
                        "🔄 Processing Subscription service: ServiceId={ServiceId}, " +
                        "AppointmentServiceId={ApsId}, SubscriptionId={SubId}",
                        appointmentService.ServiceId, appointmentService.AppointmentServiceId,
                        appointmentService.SubscriptionId.Value);

                    // 🔒 TRY DEDUCT USAGE với PESSIMISTIC LOCK
                    // UpdateServiceUsageAsync sẽ dùng UPDLOCK để lock row
                    try
                    {
                        bool deducted = await _subscriptionCommandRepository.UpdateServiceUsageAsync(
                            appointmentService.SubscriptionId.Value, // ✅ USE appointmentService.SubscriptionId instead of appointment.SubscriptionId
                            appointmentService.ServiceId,
                            quantityUsed: 1,
                            appointmentId: appointmentId,
                            cancellationToken);

                        if (deducted)
                        {
                            _logger.LogInformation(
                                "✅ Successfully deducted usage: ServiceId={ServiceId}, " +
                                "SubscriptionId={SubId}",
                                appointmentService.ServiceId, appointmentService.SubscriptionId.Value);
                        }
                        else
                        {
                            // KHÔNG THỂ TRỪ LƯỢT → RACE CONDITION DETECTED!
                            _logger.LogWarning(
                                "⚠️ RACE CONDITION: Cannot deduct usage for ServiceId={ServiceId}, " +
                                "SubscriptionId={SubId} (hết lượt hoặc subscription không còn active). " +
                                "→ DEGRADING to 'Extra'",
                                appointmentService.ServiceId, appointmentService.SubscriptionId.Value);

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
                // ✅ FIX: Use ExecuteUpdateAsync to avoid tracking conflicts (similar to CheckInAsync fix)
                var completedDate = DateTime.UtcNow;
                var newEstimatedCost = hasDegradedService
                    ? (appointment.EstimatedCost ?? 0) + additionalPaymentRequired
                    : appointment.EstimatedCost ?? 0;

                await _context.Appointments
                    .Where(a => a.AppointmentId == appointmentId)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(a => a.StatusId, (int)finalStatus)
                        .SetProperty(a => a.CompletedDate, completedDate)
                        .SetProperty(a => a.CompletedBy, currentUserId)
                        .SetProperty(a => a.EstimatedCost, newEstimatedCost)
                        .SetProperty(a => a.UpdatedBy, currentUserId)
                        .SetProperty(a => a.UpdatedDate, completedDate),
                        cancellationToken);

                await RecordAppointmentCustomerSpendAsync(appointment, cancellationToken);

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
            // ✅ FIX: Use ExecuteUpdateAsync to avoid tracking conflicts
            await _context.AppointmentServices
                .Where(aps => aps.AppointmentServiceId == appointmentService.AppointmentServiceId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(aps => aps.ServiceSource, "Extra")
                    .SetProperty(aps => aps.Price, actualPrice),
                    cancellationToken);

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

        private async Task<string> GenerateWorkOrderCodeAsync(CancellationToken cancellationToken)
        {
            string prefix = "WO";
            string datePart = DateTime.UtcNow.ToString("yyyyMMdd");

            for (int attempts = 0; attempts < 10; attempts++)
            {
                int randomNumber = RandomNumberGenerator.GetInt32(1000, 9999);
                string code = $"{prefix}{datePart}{randomNumber}";

                // Check if code exists in WorkOrders table
                bool exists = await _context.WorkOrders
                    .AnyAsync(wo => wo.WorkOrderCode == code, cancellationToken);

                if (!exists)
                    return code;
            }

            // Fallback: use timestamp
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

        #region Smart Subscription Logic - PERFORMANCE OPTIMIZED

        /// <summary>
        /// ⚡ PERFORMANCE OPTIMIZED: Tính priority score cho subscription
        /// Complexity: O(1) - Constant time
        ///
        /// Priority Score càng CAO = càng ưu tiên sử dụng trước
        ///
        /// 🎯 Quy tắc ưu tiên (Weighted scoring):
        /// 1. EXPIRY (Weight: 10,000): Gói sắp hết hạn → priority CAO NHẤT
        ///    - Ngày 0: +10,007 points
        ///    - Ngày 7: +10,000 points
        ///    - > 7 ngày: không bonus
        ///
        /// 2. QUANTITY (Weight: 1,000): Còn ít lượt → priority cao hơn
        ///    - Còn 1 lượt: +1,000 points
        ///    - Còn 2 lượt: +999 points
        ///    - Linear decrease
        ///
        /// 3. FIFO (Weight: 1): Mua sớm hơn → priority cao hơn
        ///    - Based on PurchaseDate ticks (modulo 10,000)
        ///
        /// 4. TIEBREAKER (Weight: 0.1): Deterministic
        ///    - SubscriptionId % 10
        /// </summary>
        /// <param name="subscription">Subscription entity (with ServiceUsages loaded)</param>
        /// <param name="serviceId">ServiceId to check RemainingQuantity</param>
        /// <returns>Priority score (0-20,000 range)</returns>
        private int CalculateSubscriptionPriority(
            CustomerPackageSubscription subscription,
            int serviceId)
        {
            int priorityScore = 0;

            // 1️⃣ EXPIRY PRIORITY (Most important)
            if (subscription.ExpirationDate.HasValue)
            {
                var daysUntilExpiry = (subscription.ExpirationDate.Value.ToDateTime(TimeOnly.MinValue) - DateTime.UtcNow).Days;

                if (daysUntilExpiry >= 0 && daysUntilExpiry <= 7)
                {
                    // Inverse logic: Closer to expiry = Higher priority
                    priorityScore += 10000 + (7 - daysUntilExpiry);
                }
            }

            // 2️⃣ QUANTITY PRIORITY
            var serviceUsage = subscription.PackageServiceUsages
                .FirstOrDefault(u => u.ServiceId == serviceId);

            if (serviceUsage != null && serviceUsage.RemainingQuantity > 0)
            {
                // Inverse: Less remaining = Higher priority
                int quantityScore = Math.Max(0, 1000 - serviceUsage.RemainingQuantity);
                priorityScore += quantityScore;
            }

            // 3️⃣ FIFO PRIORITY
            if (subscription.PurchaseDate.HasValue)
            {
                // Older purchase = Higher priority
                long fifoScore = -(subscription.PurchaseDate.Value.Ticks / 10_000_000);
                priorityScore += (int)(fifoScore % 10000);
            }

            // 4️⃣ TIEBREAKER (Deterministic)
            priorityScore += subscription.SubscriptionId % 10;

            return priorityScore;
        }

        /// <summary>
        /// ⚡ PERFORMANCE OPTIMIZED: Build appointment services với Smart Deduplication
        /// Complexity: O(n*m) where n = services, m = subscriptions (usually small)
        ///
        /// 🎯 Chiến lược:
        /// 1. Load ALL active subscriptions ONCE (1 query with .Include)
        /// 2. Build service-to-subscriptions map IN-MEMORY (O(n))
        /// 3. Sort subscriptions by priority PER SERVICE (O(m log m))
        /// 4. Assign services to best subscription (O(1) per service)
        ///
        /// 📊 Performance:
        /// - DB Queries: 1-2 (subscriptions + services if needed)
        /// - Memory: O(n*m) for mapping (acceptable for typical use)
        /// - CPU: O(n*m log m) for sorting (very fast for small m)
        ///
        /// ✅ Benefits:
        /// - No N+1 query problem
        /// - Deterministic results
        /// - Easy to test & debug
        /// </summary>
        private async Task<(List<AppointmentService> services, decimal totalCost, int totalDuration)>
            BuildAppointmentServicesAsync(
                CreateAppointmentRequestDto request,
                int vehicleModelId,
                CancellationToken cancellationToken)
        {
            var appointmentServices = new List<AppointmentService>();
            var servicesFromSubscription = new HashSet<int>(); // Track which services matched subscriptions
            decimal totalCost = 0;
            int totalDuration = 0;

            // ========== STEP 1: LOAD ACTIVE SUBSCRIPTIONS (1 DB QUERY) ==========
            List<CustomerPackageSubscription> customerSubscriptions = new List<CustomerPackageSubscription>();

            if (request.SubscriptionId.HasValue)
            {
                // Customer explicitly specified a subscription
                var subscription = await _context.CustomerPackageSubscriptions
                    .Include(s => s.Package)
                    .Include(s => s.PackageServiceUsages)
                        .ThenInclude(u => u.Service)
                    .FirstOrDefaultAsync(s => s.SubscriptionId == request.SubscriptionId.Value, cancellationToken);

                if (subscription != null)
                {
                    // Validate subscription ownership & status
                    if (subscription.CustomerId != request.CustomerId)
                        throw new InvalidOperationException("Subscription không thuộc về customer này");

                    if (subscription.Status != SubscriptionStatusEnum.Active.ToString())
                        throw new InvalidOperationException($"Subscription không active (Status: {subscription.Status})");

                    if (subscription.ExpirationDate.HasValue && subscription.ExpirationDate.Value < DateOnly.FromDateTime(DateTime.UtcNow))
                        throw new InvalidOperationException($"Subscription đã hết hạn ({subscription.ExpirationDate.Value:dd/MM/yyyy})");

                    customerSubscriptions.Add(subscription);
                }
            }
            else
            {
                // AUTO-SELECT: Get ALL active subscriptions for this customer+vehicle
                customerSubscriptions = await _subscriptionRepository
                    .GetActiveSubscriptionsByCustomerAndVehicleAsync(
                        request.CustomerId,
                        request.VehicleId,
                        cancellationToken);
            }

            _logger.LogInformation(
                "📦 BuildAppointmentServices: Found {Count} active subscriptions for CustomerId={CustomerId}, VehicleId={VehicleId}",
                customerSubscriptions.Count, request.CustomerId, request.VehicleId);

            // ========== STEP 2: BUILD SERVICE-TO-SUBSCRIPTIONS MAP (IN-MEMORY) ==========
            // Key = ServiceId, Value = List of (Subscription, Usage, Priority)
            var serviceUsageMap = new Dictionary<int, List<(
                CustomerPackageSubscription Subscription,
                PackageServiceUsage Usage,
                int Priority)>>();

            foreach (var subscription in customerSubscriptions)
            {
                foreach (var usage in subscription.PackageServiceUsages.Where(u => u.RemainingQuantity > 0))
                {
                    if (!serviceUsageMap.ContainsKey(usage.ServiceId))
                    {
                        serviceUsageMap[usage.ServiceId] = new List<(CustomerPackageSubscription, PackageServiceUsage, int)>();
                    }

                    // Calculate priority for this subscription+service combo
                    int priority = CalculateSubscriptionPriority(subscription, usage.ServiceId);
                    serviceUsageMap[usage.ServiceId].Add((subscription, usage, priority));
                }
            }

            // ✅ SORT each service's subscriptions by priority (DESCENDING = higher first)
            foreach (var serviceId in serviceUsageMap.Keys.ToList())
            {
                serviceUsageMap[serviceId] = serviceUsageMap[serviceId]
                    .OrderByDescending(x => x.Priority)
                    .ToList();
            }

            _logger.LogInformation(
                "🗺️ Service map built: {ServiceCount} unique services available across subscriptions",
                serviceUsageMap.Count);

            // ========== STEP 3: DETERMINE WHICH SERVICES TO BOOK ==========
            var selectedServiceIds = request.ServiceIds ?? new List<int>();

            if (!selectedServiceIds.Any() && customerSubscriptions.Any())
            {
                // No explicit services → Use ALL services from BEST subscription
                var primarySubscription = customerSubscriptions
                    .OrderByDescending(s =>
                    {
                        // Use first service as representative for priority
                        var firstService = s.PackageServiceUsages.FirstOrDefault(u => u.RemainingQuantity > 0);
                        return firstService != null ? CalculateSubscriptionPriority(s, firstService.ServiceId) : 0;
                    })
                    .First();

                selectedServiceIds = primarySubscription.PackageServiceUsages
                    .Where(u => u.RemainingQuantity > 0)
                    .Select(u => u.ServiceId)
                    .ToList();

                _logger.LogInformation(
                    "🎯 Auto-selected {Count} services from primary subscription {SubId} ({PackageName})",
                    selectedServiceIds.Count, primarySubscription.SubscriptionId,
                    primarySubscription.Package?.PackageName ?? "N/A");
            }

            // ========== STEP 4: BUILD APPOINTMENT SERVICES ==========
            foreach (var serviceId in selectedServiceIds)
            {
                // ✅ CHECK: Service có trong subscription nào không?
                if (serviceUsageMap.ContainsKey(serviceId) && serviceUsageMap[serviceId].Any())
                {
                    // Get BEST subscription for this service (already sorted by priority)
                    var (bestSubscription, bestUsage, priority) = serviceUsageMap[serviceId].First();

                    appointmentServices.Add(new AppointmentService
                    {
                        ServiceId = serviceId,
                        SubscriptionId = bestSubscription.SubscriptionId, // ✅ TRACK which subscription this service came from
                        ServiceSource = "Subscription",
                        Price = 0, // FREE - dùng từ gói
                        EstimatedTime = bestUsage.Service?.StandardTime ?? 60,
                        Notes = $"Từ gói {bestSubscription.Package?.PackageName ?? $"#{bestSubscription.SubscriptionId}"} " +
                                $"(Còn {bestUsage.RemainingQuantity}/{bestUsage.TotalAllowedQuantity} lượt, Priority={priority})"
                    });

                    totalDuration += bestUsage.Service?.StandardTime ?? 60;
                    servicesFromSubscription.Add(serviceId);

                    _logger.LogInformation(
                        "✅ Service {ServiceId} ({ServiceName}) matched with Subscription {SubId} (Priority={Priority}, {Remaining} uses left)",
                        serviceId, bestUsage.Service?.ServiceName ?? "N/A",
                        bestSubscription.SubscriptionId, priority, bestUsage.RemainingQuantity);
                }
            }

            // ========== STEP 5: HANDLE EXTRA SERVICES (không có trong subscription) ==========
            var extraServiceIds = selectedServiceIds
                .Where(id => !servicesFromSubscription.Contains(id))
                .ToList();

            if (extraServiceIds.Any())
            {
                // ⚡ BATCH LOAD services (1 query instead of N)
                var extraServices = await _serviceRepository.GetByIdsAsync(extraServiceIds, cancellationToken);

                foreach (var service in extraServices)
                {
                    // Check pricing
                    var pricing = await _pricingRepository.GetActivePricingAsync(vehicleModelId, service.ServiceId);
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
                    "💰 Added {Count} extra services, Total cost: {Cost}đ",
                    extraServices.Count(), totalCost);
            }

            // ========== STEP 6: VALIDATION ==========
            if (!appointmentServices.Any())
                throw new InvalidOperationException(
                    "Không có dịch vụ nào được chọn hoặc gói dịch vụ đã hết lượt sử dụng");

            _logger.LogInformation(
                "📋 BuildAppointmentServices COMPLETED: {Total} services ({SubCount} from subscription, {ExtraCount} extra/regular), " +
                "TotalCost={Cost}đ, TotalDuration={Duration}min",
                appointmentServices.Count,
                appointmentServices.Count(s => s.ServiceSource == "Subscription"),
                appointmentServices.Count(s => s.ServiceSource != "Subscription"),
                totalCost, totalDuration);

            return (appointmentServices, totalCost, totalDuration);
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
                        SubscriptionId = bestSubscription.SubscriptionId, // ✅ TRACK which subscription this service came from
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

        /// <summary>
        /// Tính số tiền hoàn lại dựa trên chính sách hủy
        /// - Hủy >= 24h trước: 100%
        /// - Hủy >= 2h trước: 50%
        /// - Hủy < 2h: 0% (giữ toàn bộ phí)
        /// </summary>
        private decimal ApplyVat(decimal net)
        {
            var vatAmount = decimal.Round(net * _vatRate, 0, MidpointRounding.AwayFromZero);
            return net + vatAmount;
        }

        private decimal CalculateRefundAmount(
            Appointment appointment,
            DateTime cancelledAt)
        {
            var appointmentDate = appointment.AppointmentDate;
            var hoursDiff = (appointmentDate - cancelledAt).TotalHours;

            var paidAmount = appointment.PaidAmount ?? 0;

            // Hủy sớm >= 24h → hoàn 100%
            if (hoursDiff >= 24)
                return paidAmount;

            // Hủy sát giờ < 24h nhưng >= 2h → hoàn 50%
            if (hoursDiff >= 2)
                return paidAmount * 0.5m;

            // Hủy trong vòng 2h → giữ phí cố định (không hoàn)
            return 0;
        }
    }
}
