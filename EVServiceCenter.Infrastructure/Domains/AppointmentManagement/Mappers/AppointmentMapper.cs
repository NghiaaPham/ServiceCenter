using EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Response;
using EVServiceCenter.Core.Domains.AppointmentManagement.Entities;


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
                ConfirmationStatus = appointment.ConfirmationStatus,
                ReminderSent = appointment.ReminderSent,
                ReminderSentDate = appointment.ReminderSentDate,
                NoShowFlag = appointment.NoShowFlag,
                CreatedBy = appointment.CreatedBy,
                UpdatedBy = appointment.UpdatedBy
            };
        }
    }
}
