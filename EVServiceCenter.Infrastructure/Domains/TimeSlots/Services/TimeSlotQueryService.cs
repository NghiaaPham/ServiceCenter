using EVServiceCenter.Core.Domains.AppointmentManagement.Entities;
using EVServiceCenter.Core.Domains.Shared.Models;
using EVServiceCenter.Core.Domains.TimeSlots.DTOs.Requests;
using EVServiceCenter.Core.Domains.TimeSlots.DTOs.Responses;
using EVServiceCenter.Core.Domains.TimeSlots.Entities;
using EVServiceCenter.Core.Domains.TimeSlots.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.TimeSlots.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace EVServiceCenter.Infrastructure.Domains.TimeSlots.Services
{
    public class TimeSlotQueryService : ITimeSlotQueryService
    {
        private readonly ITimeSlotRepository _repository;
        private readonly ITimeSlotQueryRepository _queryRepository;

        public TimeSlotQueryService(
            ITimeSlotRepository repository,
            ITimeSlotQueryRepository queryRepository)
        {
            _repository = repository;
            _queryRepository = queryRepository;
        }

        public async Task<PagedResult<TimeSlotResponseDto>> GetAllAsync(
            TimeSlotQueryDto query,
            CancellationToken cancellationToken = default)
        {
            IQueryable<TimeSlot> slotsQuery = _repository.GetQueryable()
                .Include(s => s.Center)
                .Include(s => s.Appointments);

            // Filters
            if (query.CenterId.HasValue)
            {
                slotsQuery = slotsQuery.Where(s => s.CenterId == query.CenterId.Value);
            }

            if (query.StartDate.HasValue)
            {
                slotsQuery = slotsQuery.Where(s => s.SlotDate >= query.StartDate.Value);
            }

            if (query.EndDate.HasValue)
            {
                slotsQuery = slotsQuery.Where(s => s.SlotDate <= query.EndDate.Value);
            }

            if (query.IsBlocked.HasValue)
            {
                slotsQuery = slotsQuery.Where(s => s.IsBlocked == query.IsBlocked.Value);
            }

            if (query.OnlyAvailable == true)
            {
                var now = DateTime.Now;
                var today = DateOnly.FromDateTime(now);
                var currentTime = TimeOnly.FromDateTime(now);

                slotsQuery = slotsQuery.Where(s =>
                    !s.IsBlocked &&
                    (s.SlotDate > today || (s.SlotDate == today && s.StartTime > currentTime)));
            }

            // Count
            var totalCount = await slotsQuery.CountAsync(cancellationToken);

            // Sorting
            slotsQuery = ApplySorting(slotsQuery, query.SortBy, query.SortOrder);

            // Pagination
            var slots = await slotsQuery
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var dtos = slots.Select(MapToDto).ToList();

            return PagedResultFactory.Create(dtos, totalCount, query.Page, query.PageSize);
        }

        public async Task<TimeSlotResponseDto?> GetByIdAsync(
            int slotId,
            CancellationToken cancellationToken = default)
        {
            var slot = await _repository.GetByIdWithDetailsAsync(slotId, cancellationToken);
            return slot != null ? MapToDto(slot) : null;
        }

        public async Task<IEnumerable<TimeSlotResponseDto>> GetAvailableSlotsAsync(
            int centerId,
            DateOnly date,
            CancellationToken cancellationToken = default)
        {
            var slots = await _queryRepository.GetAvailableSlotsAsync(centerId, date, date, cancellationToken);
            return slots.Select(MapToDto);
        }

        public async Task<IEnumerable<TimeSlotResponseDto>> GetAvailableSlotsByDateRangeAsync(
            int centerId,
            DateOnly startDate,
            DateOnly endDate,
            CancellationToken cancellationToken = default)
        {
            var slots = await _queryRepository.GetAvailableSlotsAsync(centerId, startDate, endDate, cancellationToken);
            return slots.Select(MapToDto);
        }

        public async Task<IEnumerable<TimeSlotResponseDto>> GetSlotsByCenterAndDateAsync(
            int centerId,
            DateOnly date,
            CancellationToken cancellationToken = default)
        {
            var slots = await _queryRepository.GetSlotsByCenterAndDateAsync(centerId, date, cancellationToken);
            return slots.Select(MapToDto);
        }

        public async Task<bool> IsSlotAvailableAsync(
            int slotId,
            CancellationToken cancellationToken = default)
        {
            return await _queryRepository.IsSlotAvailableAsync(slotId, cancellationToken);
        }

        public async Task<int> GetBookingCountAsync(
            int slotId,
            CancellationToken cancellationToken = default)
        {
            return await _queryRepository.GetBookingCountAsync(slotId, cancellationToken);
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

        private static IQueryable<TimeSlot> ApplySorting(
            IQueryable<TimeSlot> query,
            string sortBy,
            string sortOrder)
        {
            var isDesc = sortOrder.Equals("desc", StringComparison.OrdinalIgnoreCase);

            return sortBy.ToLower() switch
            {
                "slotdate" => isDesc ? query.OrderByDescending(s => s.SlotDate) : query.OrderBy(s => s.SlotDate),
                "starttime" => isDesc ? query.OrderByDescending(s => s.StartTime) : query.OrderBy(s => s.StartTime),
                "centername" => isDesc ? query.OrderByDescending(s => s.Center.CenterName) : query.OrderBy(s => s.Center.CenterName),
                "maxbookings" => isDesc ? query.OrderByDescending(s => s.MaxBookings) : query.OrderBy(s => s.MaxBookings),
                _ => query.OrderBy(s => s.SlotDate).ThenBy(s => s.StartTime)
            };
        }
    }
}