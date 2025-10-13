using EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Query;
using EVServiceCenter.Core.Domains.AppointmentManagement.Entities;
using EVServiceCenter.Core.Domains.AppointmentManagement.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.CarBrands.Entities;
using EVServiceCenter.Core.Domains.CarModels.Entities;
using EVServiceCenter.Core.Domains.Customers.Entities;
using EVServiceCenter.Core.Domains.CustomerVehicles.Entities;
using EVServiceCenter.Core.Domains.MaintenanceServices.Entities;
using EVServiceCenter.Core.Domains.ServiceCenters.Entities;
using EVServiceCenter.Core.Domains.Shared.Models;
using EVServiceCenter.Core.Domains.TimeSlots.Entities;
using EVServiceCenter.Core.Entities;
using EVServiceCenter.Core.Helpers;
using Microsoft.EntityFrameworkCore;

namespace EVServiceCenter.Infrastructure.Domains.AppointmentManagement.Repositories
{
    public class AppointmentQueryRepository : IAppointmentQueryRepository
    {
        private readonly EVDbContext _context;

        public AppointmentQueryRepository(EVDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<Appointment>> GetPagedAsync(
            AppointmentQueryDto query,
            CancellationToken cancellationToken = default)
        {
            // ✅ PERFORMANCE: Base query without includes for fast counting
            var baseQuery = _context.Appointments.AsNoTracking().AsQueryable();

            // Apply filters
            baseQuery = ApplyFilters(baseQuery, query);

            // ✅ PERFORMANCE: COUNT without joins - FAST
            var totalCount = await baseQuery.CountAsync(cancellationToken);

            if (totalCount == 0)
            {
                return PagedResultFactory.Empty<Appointment>(query.Page, query.PageSize);
            }

            // Apply sorting
            baseQuery = ApplySorting(baseQuery, query);

            // ✅ FIX: Use Include instead of Select projection to preserve all IDs
            // ✅ PERFORMANCE: AsSplitQuery() prevents cartesian explosion
            var items = await baseQuery
                .Skip(query.Skip)
                .Take(query.PageSize)
                .Include(a => a.Customer)
                .Include(a => a.Vehicle)
                    .ThenInclude(v => v.Model)
                        .ThenInclude(m => m!.Brand)
                .Include(a => a.ServiceCenter)
                .Include(a => a.Slot)
                .Include(a => a.Status)
                .Include(a => a.Package)
                .Include(a => a.AppointmentServices)
                    .ThenInclude(aps => aps.Service)
                .AsSplitQuery() // ✅ CRITICAL: Split into multiple optimized queries
                .ToListAsync(cancellationToken);

            return PagedResultFactory.Create(items, totalCount, query.Page, query.PageSize);
        }

        private IQueryable<Appointment> ApplyFilters(
            IQueryable<Appointment> query,
            AppointmentQueryDto filter)
        {
            if (filter.CustomerId.HasValue)
                query = query.Where(a => a.CustomerId == filter.CustomerId.Value);

            if (filter.ServiceCenterId.HasValue)
                query = query.Where(a => a.ServiceCenterId == filter.ServiceCenterId.Value);

            if (filter.StatusId.HasValue)
                query = query.Where(a => a.StatusId == filter.StatusId.Value);

            if (filter.StartDate.HasValue)
                query = query.Where(a => a.Slot!.SlotDate >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(a => a.Slot!.SlotDate <= filter.EndDate.Value);

            if (!string.IsNullOrWhiteSpace(filter.Priority))
                query = query.Where(a => a.Priority == filter.Priority);

            if (!string.IsNullOrWhiteSpace(filter.Source))
                query = query.Where(a => a.Source == filter.Source);

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var search = filter.SearchTerm.ToLower();
                query = query.Where(a =>
                    EF.Functions.Like(a.AppointmentCode, $"%{search}%") ||
                    EF.Functions.Like(a.Customer.FullName, $"%{search}%") ||
                    a.Vehicle.LicensePlate != null &&
                     EF.Functions.Like(a.Vehicle.LicensePlate, $"%{search}%"));
            }

            return query;
        }

        private IQueryable<Appointment> ApplySorting(
            IQueryable<Appointment> query,
            AppointmentQueryDto filter)
        {
            return filter.SortBy.ToLower() switch
            {
                "appointmentdate" => filter.SortOrder.ToLower() == "asc"
                    ? query.OrderBy(a => a.AppointmentDate)
                    : query.OrderByDescending(a => a.AppointmentDate),
                "createddate" => filter.SortOrder.ToLower() == "asc"
                    ? query.OrderBy(a => a.CreatedDate)
                    : query.OrderByDescending(a => a.CreatedDate),
                "estimatedcost" => filter.SortOrder.ToLower() == "asc"
                    ? query.OrderBy(a => a.EstimatedCost)
                    : query.OrderByDescending(a => a.EstimatedCost),
                _ => query.OrderByDescending(a => a.AppointmentDate)
            };
        }

        public async Task<IEnumerable<Appointment>> GetByCustomerIdAsync(
            int customerId,
            CancellationToken cancellationToken = default)
        {
            // ✅ FIX: Include ALL navigation properties needed by mapper
            return await _context.Appointments
                .AsNoTracking()
                .Where(a => a.CustomerId == customerId)
                .Include(a => a.Customer) // ✅ ADDED: Customer info
                .Include(a => a.ServiceCenter)
                .Include(a => a.Slot)
                .Include(a => a.Status)
                .Include(a => a.Vehicle)
                    .ThenInclude(v => v.Model)
                        .ThenInclude(m => m!.Brand)
                .Include(a => a.Package) // ✅ ADDED: Package info (if subscription)
                .Include(a => a.AppointmentServices) // ✅ ADDED: Services list
                    .ThenInclude(aps => aps.Service)
                .Include(a => a.PreferredTechnician) // ✅ ADDED: Technician info
                .AsSplitQuery() // ✅ CRITICAL: Split into multiple optimized queries
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Appointment>> GetByServiceCenterAndDateAsync(
            int serviceCenterId,
            DateOnly date,
            CancellationToken cancellationToken = default)
        {
            return await _context.Appointments
                .AsNoTracking()
                .Where(a => a.ServiceCenterId == serviceCenterId && a.Slot!.SlotDate == date)
                .Select(a => new Appointment
                {
                    AppointmentId = a.AppointmentId,
                    AppointmentCode = a.AppointmentCode,
                    Customer = new Customer
                    {
                        FullName = a.Customer.FullName,
                        PhoneNumber = a.Customer.PhoneNumber
                    },
                    Vehicle = new CustomerVehicle
                    {
                        LicensePlate = a.Vehicle.LicensePlate
                    },
                    Slot = new TimeSlot
                    {
                        StartTime = a.Slot!.StartTime,
                        EndTime = a.Slot.EndTime
                    },
                    Status = new AppointmentStatus
                    {
                        StatusName = a.Status.StatusName,
                        StatusColor = a.Status.StatusColor
                    },
                    EstimatedDuration = a.EstimatedDuration
                })
                .OrderBy(a => a.Slot!.StartTime)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Appointment>> GetUpcomingByCustomerAsync(
            int customerId,
            int limit = 5,
            CancellationToken cancellationToken = default)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            // ✅ FIX: Use Include to load ALL navigation properties (same as GetByCustomerIdAsync)
            return await _context.Appointments
                .AsNoTracking()
                .Where(a => a.CustomerId == customerId && a.Slot!.SlotDate >= today)
                .OrderBy(a => a.Slot!.SlotDate)
                    .ThenBy(a => a.Slot!.StartTime)
                .Take(limit)
                .Include(a => a.Customer) // ✅ ADDED: Customer info
                .Include(a => a.ServiceCenter)
                .Include(a => a.Slot)
                .Include(a => a.Status)
                .Include(a => a.Vehicle)
                    .ThenInclude(v => v.Model)
                        .ThenInclude(m => m!.Brand)
                .Include(a => a.Package) // ✅ ADDED: Package info (if subscription)
                .Include(a => a.AppointmentServices) // ✅ ADDED: Services list
                    .ThenInclude(aps => aps.Service)
                .Include(a => a.PreferredTechnician) // ✅ ADDED: Technician info
                .AsSplitQuery() // ✅ CRITICAL: Split into multiple optimized queries
                .ToListAsync(cancellationToken);
        }

        public async Task<int> GetCountByStatusAsync(
            int statusId,
            CancellationToken cancellationToken = default)
        {
            // Simple count - no joins needed
            return await _context.Appointments
                .AsNoTracking()
                .CountAsync(a => a.StatusId == statusId, cancellationToken);
        }

        public async Task<IEnumerable<Appointment>> GetBySlotIdAsync(
            int slotId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Appointments
                .AsNoTracking()
                .Where(a => a.SlotId == slotId)
                .Select(a => new Appointment
                {
                    AppointmentId = a.AppointmentId,
                    Customer = new Customer
                    {
                        FullName = a.Customer.FullName
                    },
                    Status = new AppointmentStatus
                    {
                        StatusName = a.Status.StatusName
                    }
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<int> GetActiveCountBySlotIdAsync(
            int slotId,
            CancellationToken cancellationToken = default)
        {
            var activeStatuses = AppointmentStatusHelper.ActiveBookings;

            return await _context.Appointments
                .AsNoTracking()
                .CountAsync(a => a.SlotId == slotId && activeStatuses.Contains(a.StatusId),
                    cancellationToken);
        }

        public async Task<IEnumerable<Appointment>> GetByVehicleIdAsync(
            int vehicleId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Appointments
                .AsNoTracking()
                .Where(a => a.VehicleId == vehicleId)
                .Select(a => new Appointment
                {
                    AppointmentId = a.AppointmentId,
                    AppointmentCode = a.AppointmentCode,
                    ServiceCenter = new ServiceCenter { CenterName = a.ServiceCenter.CenterName },
                    Slot = a.Slot == null ? null : new TimeSlot
                    {
                        SlotDate = a.Slot.SlotDate,
                        StartTime = a.Slot.StartTime
                    },
                    Status = new AppointmentStatus { StatusName = a.Status.StatusName },
                    EstimatedCost = a.EstimatedCost,
                    AppointmentDate = a.AppointmentDate
                })
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> CountRescheduleTimesAsync(
     int appointmentId,
     CancellationToken cancellationToken = default)
        {
            // Đếm số appointment có RescheduledFromId trỏ về appointmentId này
            return await _context.Appointments
                .CountAsync(a => a.RescheduledFromId == appointmentId, cancellationToken);
        }

        public async Task<bool> HasBeenRescheduledAsync(
            int appointmentId,
            CancellationToken cancellationToken = default)
        {
            // Kiểm tra có appointment nào có RescheduledFromId = appointmentId không
            return await _context.Appointments
                .AnyAsync(a => a.RescheduledFromId == appointmentId, cancellationToken);
        }

        public async Task<List<int>> GetRescheduleChainAsync(
            int appointmentId,
            CancellationToken cancellationToken = default)
        {
            var chain = new List<int> { appointmentId };

            // Bước 1: Tìm ngược về appointment gốc
            var current = await _context.Appointments
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId, cancellationToken);

            while (current?.RescheduledFromId != null)
            {
                chain.Insert(0, current.RescheduledFromId.Value);
                current = await _context.Appointments
                    .FirstOrDefaultAsync(a => a.AppointmentId == current.RescheduledFromId, cancellationToken);
            }

            // Bước 2: Tìm tiến về các appointment mới (nếu có)
            var appointmentIds = new Queue<int>(new[] { appointmentId });

            while (appointmentIds.Count > 0)
            {
                var currentId = appointmentIds.Dequeue();
                var children = await _context.Appointments
                    .Where(a => a.RescheduledFromId == currentId)
                    .Select(a => a.AppointmentId)
                    .ToListAsync(cancellationToken);

                foreach (var childId in children)
                {
                    if (!chain.Contains(childId))
                    {
                        chain.Add(childId);
                        appointmentIds.Enqueue(childId);
                    }
                }
            }

            return chain;
        }

        public async Task<List<Appointment>> GetVehicleAppointmentsByDateAsync(
            int vehicleId,
            DateOnly date,
            CancellationToken cancellationToken = default)
        {
            return await _context.Appointments
                .AsNoTracking()
                .Include(a => a.Slot)
                .Include(a => a.Status)
                .Where(a => a.VehicleId == vehicleId && a.Slot!.SlotDate == date)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Appointment>> GetTechnicianAppointmentsByDateAsync(
            int technicianId,
            int serviceCenterId,
            DateOnly date,
            CancellationToken cancellationToken = default)
        {
            return await _context.Appointments
                .AsNoTracking()
                .Include(a => a.Slot)
                .Include(a => a.Status)
                .Where(a => a.PreferredTechnicianId == technicianId
                         && a.ServiceCenterId == serviceCenterId  // ✅ PER CENTER
                         && a.Slot!.SlotDate == date)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Appointment>> GetServiceCenterAppointmentsByDateAsync(
            int serviceCenterId,
            DateOnly date,
            CancellationToken cancellationToken = default)
        {
            return await _context.Appointments
                .AsNoTracking()
                .Include(a => a.Slot)
                .Include(a => a.Status)
                .Where(a => a.ServiceCenterId == serviceCenterId && a.Slot!.SlotDate == date)
                .ToListAsync(cancellationToken);
        }
    }
}
