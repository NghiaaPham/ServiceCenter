using EVServiceCenter.Core.Domains.AppointmentManagement.Entities;
using EVServiceCenter.Core.Domains.ServiceCenters.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.TimeSlots.DTOs.Requests;
using EVServiceCenter.Core.Domains.TimeSlots.DTOs.Responses;
using EVServiceCenter.Core.Domains.TimeSlots.Entities;
using EVServiceCenter.Core.Domains.TimeSlots.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.TimeSlots.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace EVServiceCenter.Infrastructure.Domains.TimeSlots.Services
{
    public class TimeSlotCommandService : ITimeSlotCommandService
    {
        private readonly ITimeSlotRepository _repository;
        private readonly ITimeSlotQueryRepository _queryRepository;
        private readonly ITimeSlotCommandRepository _commandRepository;
        private readonly IServiceCenterRepository _centerRepository;
        private readonly ILogger<TimeSlotCommandService> _logger;

        public TimeSlotCommandService(
            ITimeSlotRepository repository,
            ITimeSlotQueryRepository queryRepository,
            ITimeSlotCommandRepository commandRepository,
            IServiceCenterRepository centerRepository,
            ILogger<TimeSlotCommandService> logger)
        {
            _repository = repository;
            _queryRepository = queryRepository;
            _commandRepository = commandRepository;
            _centerRepository = centerRepository;
            _logger = logger;
        }

        public async Task<TimeSlotResponseDto> CreateAsync(
            CreateTimeSlotRequestDto request,
            CancellationToken cancellationToken = default)
        {
            // Validate center exists
            var center = await _centerRepository.GetByIdAsync(request.CenterId);
            if (center == null)
                throw new InvalidOperationException($"Không tìm thấy trung tâm {request.CenterId}");

            // Check conflict
            if (await _commandRepository.HasConflictAsync(
                request.CenterId, request.SlotDate, request.StartTime, request.EndTime, null, cancellationToken))
            {
                throw new InvalidOperationException(
                    $"Đã có slot trùng thời gian tại trung tâm {center.CenterName} vào ngày {request.SlotDate}");
            }

            var slot = new TimeSlot
            {
                CenterId = request.CenterId,
                SlotDate = request.SlotDate,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                MaxBookings = request.MaxBookings,
                SlotType = request.SlotType?.Trim(),
                IsBlocked = request.IsBlocked,
                Notes = request.Notes?.Trim()
            };

            var created = await _repository.CreateAsync(slot);

            _logger.LogInformation("TimeSlot created: {SlotId} for Center {CenterId} on {Date}",
                created.SlotId, request.CenterId, request.SlotDate);

            var slotWithDetails = await _repository.GetByIdWithDetailsAsync(created.SlotId, cancellationToken);
            return MapToDto(slotWithDetails!);
        }

        public async Task<TimeSlotResponseDto> UpdateAsync(
            UpdateTimeSlotRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var existing = await _repository.GetByIdAsync(request.SlotId, cancellationToken);
            if (existing == null)
                throw new InvalidOperationException($"Không tìm thấy TimeSlot {request.SlotId}");

            // Validate center exists
            var center = await _centerRepository.GetByIdAsync(request.CenterId);
            if (center == null)
                throw new InvalidOperationException($"Không tìm thấy trung tâm {request.CenterId}");

            // Check conflict
            if (await _commandRepository.HasConflictAsync(
                request.CenterId, request.SlotDate, request.StartTime, request.EndTime, request.SlotId, cancellationToken))
            {
                throw new InvalidOperationException(
                    $"Đã có slot trùng thời gian tại trung tâm {center.CenterName} vào ngày {request.SlotDate}");
            }

            existing.CenterId = request.CenterId;
            existing.SlotDate = request.SlotDate;
            existing.StartTime = request.StartTime;
            existing.EndTime = request.EndTime;
            existing.MaxBookings = request.MaxBookings;
            existing.SlotType = request.SlotType?.Trim();
            existing.IsBlocked = request.IsBlocked;
            existing.Notes = request.Notes?.Trim();

            await _repository.UpdateAsync(existing);

            _logger.LogInformation("TimeSlot updated: {SlotId}", request.SlotId);

            var updated = await _repository.GetByIdWithDetailsAsync(request.SlotId, cancellationToken);
            return MapToDto(updated!);
        }

        public async Task<bool> DeleteAsync(
            int slotId,
            CancellationToken cancellationToken = default)
        {
            var slot = await _repository.GetByIdWithDetailsAsync(slotId, cancellationToken);
            if (slot == null)
                return false;

            // Check if has bookings
            if (slot.Appointments.Any())
            {
                throw new InvalidOperationException(
                    "Không thể xóa slot đã có lịch hẹn. Hãy block slot thay vì xóa.");
            }

            var result = await _repository.DeleteAsync(slotId);

            if (result)
            {
                _logger.LogInformation("TimeSlot deleted: {SlotId}", slotId);
            }

            return result;
        }

        public async Task<int> GenerateSlotsAsync(
            GenerateSlotsRequestDto request,
            CancellationToken cancellationToken = default)
        {
            // Validate center
            var center = await _centerRepository.GetByIdAsync(request.CenterId);
            if (center == null)
                throw new InvalidOperationException($"Không tìm thấy trung tâm {request.CenterId}");

            // Delete existing if overwrite
            if (request.OverwriteExisting)
            {
                await _commandRepository.DeleteSlotsByDateRangeAsync(
                    request.CenterId, request.StartDate, request.EndDate, cancellationToken);
            }

            var slotsToCreate = new List<TimeSlot>();
            var currentDate = request.StartDate;

            while (currentDate <= request.EndDate)
            {
               
                // With this:
                var workingStartTime = center.OpenTime != default ? center.OpenTime : TimeOnly.Parse("08:00");
                var workingEndTime = center.CloseTime != default ? center.CloseTime : TimeOnly.Parse("18:00");

                var currentTime = workingStartTime;

                while (currentTime < workingEndTime)
                {
                    var slotEndTime = currentTime.AddMinutes(request.SlotDurationMinutes);

                    if (slotEndTime > workingEndTime)
                        break;

                    // Check if slot already exists
                    if (!await _queryRepository.ExistsAsync(request.CenterId, currentDate, currentTime, cancellationToken))
                    {
                        slotsToCreate.Add(new TimeSlot
                        {
                            CenterId = request.CenterId,
                            SlotDate = currentDate,
                            StartTime = currentTime,
                            EndTime = slotEndTime,
                            MaxBookings = request.MaxBookingsPerSlot,
                            SlotType = request.SlotType ?? "Regular",
                            IsBlocked = false
                        });
                    }

                    currentTime = slotEndTime;
                }

                currentDate = currentDate.AddDays(1);
            }

            if (slotsToCreate.Any())
            {
                var count = await _commandRepository.BulkCreateAsync(slotsToCreate, cancellationToken);
                _logger.LogInformation("Generated {Count} slots for Center {CenterId}", count, request.CenterId);
                return count;
            }

            return 0;
        }

        public async Task<int> DeleteEmptySlotsAsync(
            int centerId,
            DateOnly date,
            CancellationToken cancellationToken = default)
        {
            var count = await _commandRepository.DeleteEmptySlotsAsync(centerId, date, cancellationToken);
            _logger.LogInformation("Deleted {Count} empty slots for Center {CenterId} on {Date}",
                count, centerId, date);
            return count;
        }

        public async Task<TimeSlotResponseDto> ToggleBlockAsync(
            int slotId,
            bool isBlocked,
            CancellationToken cancellationToken = default)
        {
            var slot = await _repository.GetByIdAsync(slotId, cancellationToken);
            if (slot == null)
                throw new InvalidOperationException($"Không tìm thấy TimeSlot {slotId}");

            slot.IsBlocked = isBlocked;
            await _repository.UpdateAsync(slot);

            _logger.LogInformation("TimeSlot {SlotId} {Action}",
                slotId, isBlocked ? "blocked" : "unblocked");

            var updated = await _repository.GetByIdWithDetailsAsync(slotId, cancellationToken);
            return MapToDto(updated!);
        }

        private static TimeSlotResponseDto MapToDto(TimeSlot slot)
        {
            return new TimeSlotResponseDto
            {
                SlotId = slot.SlotId,
                CenterId = slot.CenterId,
                CenterName = slot.Center?.CenterName ?? string.Empty,
                SlotDate = slot.SlotDate,
                StartTime = slot.StartTime,
                EndTime = slot.EndTime,
                DurationMinutes = slot.DurationMinutes,
                MaxBookings = slot.MaxBookings,
                CurrentBookings = slot.CurrentBookings,
                RemainingCapacity = slot.RemainingCapacity,
                SlotType = slot.SlotType,
                IsBlocked = slot.IsBlocked,
                IsAvailable = slot.IsAvailable,
                Notes = slot.Notes,
                CreatedDate = slot.CreatedDate,
                UpdatedDate = slot.UpdatedDate
            };
        }
    }
}