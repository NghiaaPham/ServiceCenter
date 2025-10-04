using EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Request;
using EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Response;
using EVServiceCenter.Core.Domains.AppointmentManagement.Entities;
using EVServiceCenter.Core.Domains.AppointmentManagement.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.AppointmentManagement.Interfaces.Services;
using EVServiceCenter.Core.Domains.CustomerVehicles.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.MaintenanceServices.Entities;
using EVServiceCenter.Core.Domains.MaintenanceServices.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.ModelServicePricings.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.TimeSlots.Interfaces.Repositories;
using EVServiceCenter.Core.Enums;
using EVServiceCenter.Core.Helpers;
using EVServiceCenter.Infrastructure.Domains.AppointmentManagement.Mappers;
using System.Security.Cryptography;

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

        public AppointmentCommandService(
            IAppointmentRepository repository,
            IAppointmentCommandRepository commandRepository,
            IAppointmentQueryRepository queryRepository,
            ITimeSlotRepository slotRepository,
            IMaintenanceServiceRepository serviceRepository,
            IModelServicePricingRepository pricingRepository,
            ICustomerVehicleRepository vehicleRepository)
        {
            _repository = repository;
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _slotRepository = slotRepository;
            _serviceRepository = serviceRepository;
            _pricingRepository = pricingRepository;
            _vehicleRepository = vehicleRepository;
        }

        public async Task<AppointmentResponseDto> CreateAsync(
            CreateAppointmentRequestDto request,
            int currentUserId,
            CancellationToken cancellationToken = default)
        {
            var slot = await _slotRepository.GetByIdAsync(request.SlotId, cancellationToken);
            if (slot == null)
                throw new InvalidOperationException("Slot không tồn tại");

            int activeCount = await _queryRepository.GetActiveCountBySlotIdAsync(
                request.SlotId, cancellationToken);

            if (activeCount >= slot.MaxBookings)
                throw new InvalidOperationException("Slot đã đầy");

            var vehicle = await _vehicleRepository.GetByIdAsync(request.VehicleId, cancellationToken);
            if (vehicle == null)
                throw new InvalidOperationException("Xe không tồn tại");

            (decimal totalCost, int totalDuration, List<AppointmentService> appointmentServices) =
                await CalculatePricingAsync(vehicle.ModelId, null, request.ServiceIds, cancellationToken);

            string appointmentCode = await GenerateAppointmentCodeAsync(cancellationToken);

            var appointment = new Appointment
            {
                AppointmentCode = appointmentCode,
                CustomerId = request.CustomerId,
                VehicleId = request.VehicleId,
                ServiceCenterId = request.ServiceCenterId,
                SlotId = request.SlotId,
                PackageId = null,
                StatusId = (int)AppointmentStatusEnum.Pending,
                AppointmentDate = slot.SlotDate.ToDateTime(slot.StartTime),
                EstimatedDuration = totalDuration,
                EstimatedCost = totalCost,
                CustomerNotes = request.CustomerNotes,
                PreferredTechnicianId = request.PreferredTechnicianId,
                Priority = request.Priority,
                Source = request.Source,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = currentUserId
            };

            Appointment created = await _commandRepository.CreateWithServicesAsync(
                appointment, appointmentServices, cancellationToken);

            Appointment? result = await _repository.GetByIdWithDetailsAsync(
                created.AppointmentId, cancellationToken);

            return AppointmentMapper.ToResponseDto(result!);
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
            Appointment? oldAppointment = await _repository.GetByIdWithDetailsAsync(
                request.AppointmentId, cancellationToken);

            if (oldAppointment == null)
                throw new InvalidOperationException("Appointment không tồn tại");

            if (AppointmentStatusHelper.IsFinalStatus(oldAppointment.StatusId))
                throw new InvalidOperationException("Không thể dời lịch appointment đã kết thúc");

            var newSlot = await _slotRepository.GetByIdAsync(request.NewSlotId, cancellationToken);
            if (newSlot == null)
                throw new InvalidOperationException("Slot mới không tồn tại");

            int activeCount = await _queryRepository.GetActiveCountBySlotIdAsync(
                request.NewSlotId, cancellationToken);

            if (activeCount >= newSlot.MaxBookings)
                throw new InvalidOperationException("Slot mới đã đầy");

            string appointmentCode = await GenerateAppointmentCodeAsync(cancellationToken);

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
                CreatedBy = currentUserId
            };

            List<AppointmentService> newServices = oldAppointment.AppointmentServices.Select(aps => new AppointmentService
            {
                ServiceId = aps.ServiceId,
                ServiceSource = aps.ServiceSource,
                Price = aps.Price,
                EstimatedTime = aps.EstimatedTime,
                Notes = aps.Notes
            }).ToList();

            Appointment created = await _commandRepository.RescheduleAsync(
                request.AppointmentId, newAppointment, newServices, cancellationToken);

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
    }
}