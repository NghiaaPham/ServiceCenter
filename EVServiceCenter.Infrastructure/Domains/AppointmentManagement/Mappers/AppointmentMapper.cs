using System;
using System.Collections.Generic;
using System.Linq;
using EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Response;
using EVServiceCenter.Core.Domains.AppointmentManagement.Entities;
using EVServiceCenter.Core.Domains.Payments.Entities;


namespace EVServiceCenter.Infrastructure.Domains.AppointmentManagement.Mappers
{
    public static class AppointmentMapper
    {
        public static AppointmentResponseDto ToResponseDto(Appointment appointment)
        {
            if (appointment == null)
            {
                throw new ArgumentNullException(nameof(appointment), "Appointment không được null");
            }

            return new AppointmentResponseDto
            {
                AppointmentId = appointment.AppointmentId,
                AppointmentCode = appointment.AppointmentCode,

                // Customer
                CustomerId = appointment.CustomerId,
                CustomerName = appointment.Customer?.FullName ?? "",
                CustomerPhone = appointment.Customer?.PhoneNumber,
                CustomerEmail = appointment.Customer?.Email,

                // Vehicle
                VehicleId = appointment.VehicleId,
                VehicleName = appointment.Vehicle != null
                    ? $"{appointment.Vehicle.Model?.Brand?.BrandName} {appointment.Vehicle.Model?.ModelName} {appointment.Vehicle.Model?.Year}"
                    : "",
                LicensePlate = appointment.Vehicle?.LicensePlate,
                VIN = appointment.Vehicle?.Vin,

                // Service Center
                ServiceCenterId = appointment.ServiceCenterId,
                ServiceCenterName = appointment.ServiceCenter?.CenterName ?? "",
                ServiceCenterAddress = appointment.ServiceCenter?.Address,

                // Time Slot
                SlotId = appointment.SlotId,
                SlotDate = appointment.Slot?.SlotDate,
                SlotStartTime = appointment.Slot?.StartTime,
                SlotEndTime = appointment.Slot?.EndTime,

                // Package
                PackageId = appointment.PackageId,
                PackageName = appointment.Package?.PackageName,
                PackagePrice = appointment.Package?.TotalPrice,

                // Services
                Services = appointment.AppointmentServices?.Select(aps => new AppointmentServiceDto
                {
                    AppointmentServiceId = aps.AppointmentServiceId,
                    ServiceId = aps.ServiceId,
                    ServiceCode = aps.Service?.ServiceCode ?? "",
                    ServiceName = aps.Service?.ServiceName ?? "",
                    ServiceSource = aps.ServiceSource,
                    Price = aps.Price,
                    EstimatedTime = aps.EstimatedTime,
                    Notes = aps.Notes
                }).ToList() ?? new List<AppointmentServiceDto>(),

                // Status
                StatusId = appointment.StatusId,
                StatusName = appointment.Status?.StatusName ?? "",
                StatusColor = appointment.Status?.StatusColor ?? "",

                // Cost & Duration
                EstimatedDuration = appointment.EstimatedDuration,
                EstimatedCost = appointment.EstimatedCost,
                FinalCost = appointment.FinalCost,
                PaymentStatus = appointment.PaymentStatus,
                PaidAmount = appointment.PaidAmount,
                PaymentIntentCount = appointment.PaymentIntentCount,
                LatestPaymentIntentId = appointment.LatestPaymentIntentId,
                OutstandingAmount = CalculateOutstandingAmount(appointment),

                // ✅ DISCOUNT INFO: Build DiscountSummary from stored fields
                DiscountSummary = BuildDiscountSummary(appointment),

                // Other
                CustomerNotes = appointment.CustomerNotes,
                Priority = appointment.Priority ?? "Normal",
                Source = appointment.Source ?? "",
                PreferredTechnicianId = appointment.PreferredTechnicianId,
                PreferredTechnicianName = appointment.PreferredTechnician?.FullName,

                // Dates
                CreatedDate = appointment.CreatedDate ?? DateTime.UtcNow,
                UpdatedDate = appointment.UpdatedDate,
                ConfirmationDate = appointment.ConfirmationDate,

                // Cancellation/Reschedule
                CancellationReason = appointment.CancellationReason,
                RescheduledFromId = appointment.RescheduledFromId
            };
        }

        public static AppointmentDetailResponseDto ToDetailResponseDto(Appointment appointment)
        {
            if (appointment == null)
            {
                throw new ArgumentNullException(nameof(appointment), "Appointment không được null");
            }

            var baseDto = ToResponseDto(appointment);

            return new AppointmentDetailResponseDto
            {
                // Copy all from base
                AppointmentId = baseDto.AppointmentId,
                AppointmentCode = baseDto.AppointmentCode,
                CustomerId = baseDto.CustomerId,
                CustomerName = baseDto.CustomerName,
                CustomerPhone = baseDto.CustomerPhone,
                CustomerEmail = baseDto.CustomerEmail,
                VehicleId = baseDto.VehicleId,
                VehicleName = baseDto.VehicleName,
                LicensePlate = baseDto.LicensePlate,
                VIN = baseDto.VIN,
                ServiceCenterId = baseDto.ServiceCenterId,
                ServiceCenterName = baseDto.ServiceCenterName,
                ServiceCenterAddress = baseDto.ServiceCenterAddress,
                SlotId = baseDto.SlotId,
                SlotDate = baseDto.SlotDate,
                SlotStartTime = baseDto.SlotStartTime,
                SlotEndTime = baseDto.SlotEndTime,
                PackageId = baseDto.PackageId,
                PackageName = baseDto.PackageName,
                PackagePrice = baseDto.PackagePrice,
                Services = baseDto.Services,
                StatusId = baseDto.StatusId,
                StatusName = baseDto.StatusName,
                StatusColor = baseDto.StatusColor,
                EstimatedDuration = baseDto.EstimatedDuration,
                EstimatedCost = baseDto.EstimatedCost,
                FinalCost = baseDto.FinalCost, // ✅ FIX: Copy FinalCost
                PaymentStatus = baseDto.PaymentStatus, // ✅ FIX: Copy PaymentStatus
                PaidAmount = baseDto.PaidAmount, // ✅ FIX: Copy PaidAmount
                PaymentIntentCount = baseDto.PaymentIntentCount, // ✅ FIX: Copy PaymentIntentCount
                LatestPaymentIntentId = baseDto.LatestPaymentIntentId, // ✅ FIX: Copy LatestPaymentIntentId
                OutstandingAmount = baseDto.OutstandingAmount, // ✅ FIX: Copy OutstandingAmount
                CustomerNotes = baseDto.CustomerNotes,
                Priority = baseDto.Priority,
                Source = baseDto.Source,
                PreferredTechnicianId = baseDto.PreferredTechnicianId,
                PreferredTechnicianName = baseDto.PreferredTechnicianName,
                CreatedDate = baseDto.CreatedDate,
                UpdatedDate = baseDto.UpdatedDate,
                ConfirmationDate = baseDto.ConfirmationDate,
                CancellationReason = baseDto.CancellationReason,
                RescheduledFromId = baseDto.RescheduledFromId,

                // Additional detail fields
                ServiceDescription = appointment.ServiceDescription,
                ConfirmationMethod = appointment.ConfirmationMethod,
                ConfirmationStatus = appointment.ConfirmationStatus ?? "Pending",
                ReminderSent = appointment.ReminderSent ?? false,
                ReminderSentDate = appointment.ReminderSentDate,
                NoShowFlag = appointment.NoShowFlag ?? false,
                CreatedBy = appointment.CreatedBy,
                CreatedByName = appointment.CreatedByNavigation?.FullName,
                UpdatedBy = appointment.UpdatedBy,
                UpdatedByName = appointment.UpdatedByNavigation?.FullName,

                // ✅ TODO: WorkOrders mapping (needs WorkOrder entity check)
                WorkOrders = appointment.WorkOrders?
                    .Select(wo => new WorkOrderSummaryDto
                    {
                        WorkOrderId = wo.WorkOrderId,
                        WorkOrderNumber = wo.WorkOrderCode,
                        StatusName = wo.Status?.StatusName ?? "",
                        TotalAmount = wo.TotalAmount,
                        CreatedDate = wo.CreatedDate ?? DateTime.UtcNow
                    })
                    .ToList() ?? new List<WorkOrderSummaryDto>(),

                // ✅ DISCOUNT: Use baseDto's DiscountSummary (already built)
                DiscountSummary = baseDto.DiscountSummary,

                // Payment intents (descending by creation date for convenience)
                PaymentIntents = appointment.PaymentIntents?
                    .OrderByDescending(pi => pi.CreatedDate)
                    .Select(ToPaymentIntentResponseDto)
                    .ToList()
            };
        }

        public static PaymentIntentResponseDto ToPaymentIntentResponseDto(PaymentIntent intent)
        {
            if (intent == null)
            {
                throw new ArgumentNullException(nameof(intent), "PaymentIntent không được null");
            }

            return new PaymentIntentResponseDto
            {
                PaymentIntentId = intent.PaymentIntentId,
                IntentCode = intent.IntentCode,
                Amount = intent.Amount,
                CapturedAmount = intent.CapturedAmount,
                RefundedAmount = intent.RefundedAmount,
                Currency = intent.Currency,
                Status = intent.Status,
                CreatedDate = intent.CreatedDate,
                ConfirmedDate = intent.ConfirmedDate,
                CancelledDate = intent.CancelledDate,
                FailedDate = intent.FailedDate,
                ExpiredDate = intent.ExpiredDate,
                ExpiresAt = intent.ExpiresAt,
                PaymentMethod = intent.PaymentMethod,
                Notes = intent.Notes,
                Transactions = intent.PaymentTransactions
                    .OrderByDescending(t => t.CreatedDate)
                    .Select(t => new PaymentTransactionResponseDto
                    {
                        PaymentTransactionId = t.TransactionId,
                        PaymentIntentId = t.PaymentIntentId,
                        Amount = t.Amount,
                        Currency = t.Currency,
                        Status = t.Status,
                        PaymentMethod = t.PaymentMethod,
                        GatewayName = t.GatewayName,
                        GatewayTransactionId = t.GatewayTransactionId,
                        GatewayResponse = t.GatewayResponse,
                        Notes = t.Notes,
                        CreatedDate = t.CreatedDate,
                        AuthorizedDate = t.AuthorizedDate,
                        CapturedDate = t.CapturedDate,
                        RefundedDate = t.RefundedDate,
                        FailedDate = t.FailedDate,
                        ErrorCode = t.ErrorCode,
                        ErrorMessage = t.ErrorMessage
                    })
                    .ToList()
            };
        }

        /// <summary>
        /// ✅ BUILD DISCOUNT SUMMARY from stored Appointment fields
        ///
        /// NOTE: Khi appointment được tạo, discount được tính toán chi tiết (CustomerTypeName, PromotionCode, etc.)
        /// Nhưng chỉ có DiscountAmount, DiscountType, PromotionId được lưu vào DB.
        ///
        /// Method này rebuild DiscountSummary từ những gì có trong DB:
        /// - OriginalTotal = EstimatedCost + DiscountAmount
        /// - AppliedDiscountType = DiscountType
        /// - FinalTotal = EstimatedCost
        ///
        /// Các field khác (CustomerTypeName, PromotionCodeUsed) sẽ để null vì không lưu trong DB.
        /// Frontend vẫn có thể hiển thị discount amount và type cho customer.
        /// </summary>
        private static DiscountSummaryDto? BuildDiscountSummary(Appointment appointment)
        {
            // Nếu không có discount hoặc discount = 0 → return null
            if (appointment.DiscountAmount == null || appointment.DiscountAmount.Value == 0)
                return null;

            // Tính OriginalTotal từ EstimatedCost + DiscountAmount
            decimal originalTotal = (appointment.EstimatedCost ?? 0) + appointment.DiscountAmount.Value;
            decimal finalTotal = appointment.EstimatedCost ?? 0;

            // Phân tích DiscountType để set CustomerTypeDiscount hoặc PromotionDiscount
            decimal customerTypeDiscount = 0;
            decimal promotionDiscount = 0;

            if (appointment.DiscountType == "CustomerType")
            {
                customerTypeDiscount = appointment.DiscountAmount.Value;
            }
            else if (appointment.DiscountType == "Promotion")
            {
                promotionDiscount = appointment.DiscountAmount.Value;
            }

            return new DiscountSummaryDto
            {
                OriginalTotal = originalTotal,
                CustomerTypeDiscount = customerTypeDiscount,
                CustomerTypeName = null, // ❌ Không lưu trong DB, để null
                PromotionDiscount = promotionDiscount,
                PromotionCodeUsed = null, // ❌ Không lưu trong DB, để null (chỉ có PromotionId)
                FinalDiscount = appointment.DiscountAmount.Value,
                AppliedDiscountType = appointment.DiscountType ?? "None",
                FinalTotal = finalTotal
            };
        }

        private static decimal CalculateOutstandingAmount(Appointment appointment)
        {
            decimal finalCost = appointment.FinalCost ?? appointment.EstimatedCost ?? 0m;
            decimal paidAmount = appointment.PaidAmount ?? 0m;
            var outstanding = finalCost - paidAmount;
            return outstanding > 0 ? outstanding : 0;
        }
    }
}
