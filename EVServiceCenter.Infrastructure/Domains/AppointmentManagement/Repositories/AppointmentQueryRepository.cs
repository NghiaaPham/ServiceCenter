using EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Query;
using EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Response;
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
            // ✅ STEP 1: Base query WITHOUT includes for fast counting
            var baseQuery = _context.Appointments.AsNoTracking().AsQueryable();

            // Apply filters
            baseQuery = ApplyFilters(baseQuery, query);

            // ✅ STEP 2: COUNT without any joins - VERY FAST
            var totalCount = await baseQuery.CountAsync(cancellationToken);

            if (totalCount == 0)
            {
                return PagedResultFactory.Empty<Appointment>(query.Page, query.PageSize);
            }

            // Apply sorting
            baseQuery = ApplySorting(baseQuery, query);

            // ✅ STEP 3: Get IDs only first (super fast, minimal data transfer)
            var appointmentIds = await baseQuery
                .Skip(query.Skip)
                .Take(query.PageSize)
                .Select(a => a.AppointmentId)
                .ToListAsync(cancellationToken);

            // ✅ STEP 4: Load full entities with optimized includes
            // Use AsSplitQuery to prevent cartesian explosion
            var items = await _context.Appointments
                .AsNoTracking()
                .Where(a => appointmentIds.Contains(a.AppointmentId))
                .Include(a => a.Customer) // Essential for display
                .Include(a => a.Vehicle)
                    .ThenInclude(v => v.Model) // Need model name
                        .ThenInclude(m => m!.Brand) // Need brand name
                .Include(a => a.ServiceCenter) // Essential for display
                .Include(a => a.Slot) // Essential for time display
                .Include(a => a.Status) // Essential for status display
                // ✅ OPTIMIZED: Only load AppointmentServices with essential data
                .Include(a => a.AppointmentServices)
                    .ThenInclude(aps => aps.Service)
                .AsSplitQuery() // ✅ CRITICAL: Prevents cartesian explosion
                .ToListAsync(cancellationToken);

            // ✅ STEP 5: Restore original order (preserve sorting from step 3)
            var orderedItems = appointmentIds
                .Select(id => items.First(a => a.AppointmentId == id))
                .ToList();

            return PagedResultFactory.Create(orderedItems, totalCount, query.Page, query.PageSize);
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
            // ⚡ PERFORMANCE OPTIMIZED: 2-step query pattern
            
            // STEP 1: Get appointment IDs only (FAST - no joins)
            var appointmentIds = await _context.Appointments
                .AsNoTracking()
                .Where(a => a.CustomerId == customerId)
                .OrderByDescending(a => a.AppointmentDate)
                .Take(100) // ✅ LIMIT to prevent loading too many
                .Select(a => a.AppointmentId)
                .ToListAsync(cancellationToken);

            if (!appointmentIds.Any())
                return Enumerable.Empty<Appointment>();

            // STEP 2: Load full entities with essential includes only
            return await _context.Appointments
                .AsNoTracking()
                .Where(a => appointmentIds.Contains(a.AppointmentId))
                // ✅ ESSENTIAL ONLY (reduce from 11 to 7 includes)
                .Include(a => a.Customer)
                .Include(a => a.ServiceCenter)
                .Include(a => a.Slot)
                .Include(a => a.Status)
                .Include(a => a.Vehicle)
                    .ThenInclude(v => v.Model)
                        .ThenInclude(m => m!.Brand)
                // ✅ LOAD SERVICES SEPARATELY (reduce cartesian explosion)
                .AsSplitQuery()
                .ToListAsync(cancellationToken)
                .ContinueWith(async task =>
                {
                    var appointments = await task;
                    
                    // STEP 3: Load AppointmentServices in separate query
                    var services = await _context.AppointmentServices
                        .AsNoTracking()
                        .Where(aps => appointmentIds.Contains(aps.AppointmentId))
                        .Include(aps => aps.Service)
                        .ToListAsync(cancellationToken);
                    
                    // STEP 4: Manually attach services to appointments
                    foreach (var appointment in appointments)
                    {
                        appointment.AppointmentServices = services
                            .Where(s => s.AppointmentId == appointment.AppointmentId)
                            .ToList();
                    }
                    
                    return appointments;
                })
                .Unwrap();
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

            // ⚡ PERFORMANCE OPTIMIZED: 2-step query pattern
            
            // STEP 1: Get appointment IDs only (FAST)
            var appointmentIds = await _context.Appointments
                .AsNoTracking()
                .Where(a => a.CustomerId == customerId && a.Slot!.SlotDate >= today)
                .OrderBy(a => a.Slot!.SlotDate)
                    .ThenBy(a => a.Slot!.StartTime)
                .Take(limit)
                .Select(a => a.AppointmentId)
                .ToListAsync(cancellationToken);

            if (!appointmentIds.Any())
                return Enumerable.Empty<Appointment>();

            // OPTIMIZED STEP 2: Project only needed fields in a single query (avoid heavy Include()/AsSplitQuery materialization)
            var appointments = await _context.Appointments
                .AsNoTracking()
                .Where(a => appointmentIds.Contains(a.AppointmentId))
                .Select(a => new Appointment
                {
                    AppointmentId = a.AppointmentId,
                    AppointmentCode = a.AppointmentCode,
                    CustomerId = a.CustomerId,
                    VehicleId = a.VehicleId,
                    ServiceCenterId = a.ServiceCenterId,
                    SlotId = a.SlotId,
                    StatusId = a.StatusId,
                    AppointmentDate = a.AppointmentDate,
                    EstimatedDuration = a.EstimatedDuration,
                    EstimatedCost = a.EstimatedCost,
                    FinalCost = a.FinalCost,
                    DiscountAmount = a.DiscountAmount,
                    DiscountType = a.DiscountType,

                    // Lightweight navigations (only required display fields)
                    Customer = new Customer
                    {
                        CustomerId = a.Customer.CustomerId,
                        FullName = a.Customer.FullName,
                        PhoneNumber = a.Customer.PhoneNumber,
                        Email = a.Customer.Email
                    },

                    ServiceCenter = new ServiceCenter
                    {
                        CenterId = a.ServiceCenter.CenterId,
                        CenterName = a.ServiceCenter.CenterName,
                        Address = a.ServiceCenter.Address
                    },

                    Slot = a.Slot == null ? null : new TimeSlot
                    {
                        SlotId = a.Slot.SlotId,
                        SlotDate = a.Slot.SlotDate,
                        StartTime = a.Slot.StartTime,
                        EndTime = a.Slot.EndTime
                    },

                    Status = new AppointmentStatus
                    {
                        StatusId = a.Status.StatusId,
                        StatusName = a.Status.StatusName,
                        StatusColor = a.Status.StatusColor
                    },

                    Vehicle = a.Vehicle == null ? null : new CustomerVehicle
                    {
                        VehicleId = a.Vehicle.VehicleId,
                        LicensePlate = a.Vehicle.LicensePlate,
                        Vin = a.Vehicle.Vin,
                        Model = a.Vehicle.Model == null ? null : new CarModel
                        {
                            ModelId = a.Vehicle.Model.ModelId,
                            ModelName = a.Vehicle.Model.ModelName,
                            Year = a.Vehicle.Model.Year,
                            Brand = a.Vehicle.Model.Brand == null ? null : new CarBrand
                            {
                                BrandId = a.Vehicle.Model.Brand.BrandId,
                                BrandName = a.Vehicle.Model.Brand.BrandName
                            }
                        }
                    }
                })
                .ToListAsync(cancellationToken);

            // STEP 3: Load services separately (single query, minimal projection)
            var services = await _context.AppointmentServices
                .AsNoTracking()
                .Where(aps => appointmentIds.Contains(aps.AppointmentId))
                .Select(aps => new AppointmentService
                {
                    AppointmentServiceId = aps.AppointmentServiceId,
                    AppointmentId = aps.AppointmentId,
                    ServiceId = aps.ServiceId,
                    Price = aps.Price,
                    EstimatedTime = aps.EstimatedTime,
                    Service = aps.Service == null ? null : new MaintenanceService
                    {
                        ServiceId = aps.Service.ServiceId,
                        ServiceName = aps.Service.ServiceName
                    }
                })
                .ToListAsync(cancellationToken);

            // STEP 4: Attach services to appointments (use lookup)
            var servicesLookup = services.GroupBy(s => s.AppointmentId).ToDictionary(g => g.Key, g => g.ToList());

            foreach (var appointment in appointments)
            {
                servicesLookup.TryGetValue(appointment.AppointmentId, out var list);
                appointment.AppointmentServices = list ?? new List<AppointmentService>();
            }

            // STEP 5: Restore original order and return
            var ordered = appointmentIds
                .Select(id => appointments.First(a => a.AppointmentId == id))
                .ToList();

            return ordered;
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

        public async Task<IEnumerable<AppointmentResponseDto>> GetUpcomingDtosByCustomerAsync(
            int customerId,
            int limit = 5,
            CancellationToken cancellationToken = default)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            var dtos = await _context.Appointments
                .AsNoTracking()
                .Where(a => a.CustomerId == customerId && a.Slot!.SlotDate >= today)
                .OrderBy(a => a.Slot!.SlotDate)
                .ThenBy(a => a.Slot!.StartTime)
                .Take(limit)
                .Select(a => new AppointmentResponseDto
                {
                    AppointmentId = a.AppointmentId,
                    AppointmentCode = a.AppointmentCode,

                    CustomerId = a.CustomerId,
                    CustomerName = a.Customer.FullName,
                    CustomerPhone = a.Customer.PhoneNumber,
                    CustomerEmail = a.Customer.Email,

                    VehicleId = a.VehicleId,
                    VehicleName = a.Vehicle.Model != null ? (a.Vehicle.Model.Brand.BrandName + " " + a.Vehicle.Model.ModelName + " " + a.Vehicle.Model.Year.ToString()) : "",
                    LicensePlate = a.Vehicle.LicensePlate,
                    VIN = a.Vehicle.Vin,

                    ServiceCenterId = a.ServiceCenterId,
                    ServiceCenterName = a.ServiceCenter.CenterName,
                    ServiceCenterAddress = a.ServiceCenter.Address,

                    SlotId = a.SlotId,
                    SlotDate = a.Slot!.SlotDate,
                    SlotStartTime = a.Slot.StartTime,
                    SlotEndTime = a.Slot.EndTime,

                    StatusId = a.StatusId,
                    StatusName = a.Status.StatusName,
                    StatusColor = a.Status.StatusColor,

                    EstimatedDuration = a.EstimatedDuration,
                    EstimatedCost = a.EstimatedCost,
                    FinalCost = a.FinalCost,

                    DiscountSummary = null, // lightweight

                    PaymentStatus = a.PaymentStatus,
                    PaidAmount = a.PaidAmount,
                    PaymentIntentCount = a.PaymentIntentCount,
                    LatestPaymentIntentId = a.LatestPaymentIntentId,
                    OutstandingAmount = ((a.FinalCost ?? a.EstimatedCost) ?? 0m) - (a.PaidAmount ?? 0m),

                    CustomerNotes = a.CustomerNotes,
                    Priority = a.Priority ?? "Normal",
                    Source = a.Source ?? "",

                    CreatedDate = a.CreatedDate ?? DateTime.UtcNow,
                    UpdatedDate = a.UpdatedDate,
                    ConfirmationDate = a.ConfirmationDate,

                    CancellationReason = a.CancellationReason,
                    RescheduledFromId = a.RescheduledFromId
                })
                .ToListAsync(cancellationToken);

            // Load services separately and map to DTOs
            var appointmentIds = dtos.Select(d => d.AppointmentId).ToList();
            if (appointmentIds.Any())
            {
                var services = await _context.AppointmentServices
                    .AsNoTracking()
                    .Where(s => appointmentIds.Contains(s.AppointmentId))
                    .Select(s => new { s.AppointmentId, s.ServiceId, ServiceName = s.Service!.ServiceName, s.Price, s.EstimatedTime, s.ServiceSource })
                    .ToListAsync(cancellationToken);

                var servicesLookup = services.GroupBy(s => s.AppointmentId).ToDictionary(g => g.Key, g => g.Select(x => new AppointmentServiceDto
                {
                    AppointmentServiceId = 0,
                    ServiceId = x.ServiceId,
                    ServiceName = x.ServiceName,
                    Price = x.Price,
                    EstimatedTime = x.EstimatedTime,
                    ServiceSource = x.ServiceSource ?? ""
                }).ToList());

                foreach (var dto in dtos)
                {
                    servicesLookup.TryGetValue(dto.AppointmentId, out var list);
                    dto.Services = list ?? new List<AppointmentServiceDto>();
                }
            }

            return dtos;
        }
    }
}
