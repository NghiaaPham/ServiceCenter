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
            // Base query without includes (for counting)
            var baseQuery = _context.Appointments.AsNoTracking().AsQueryable();

            // Apply filters
            baseQuery = ApplyFilters(baseQuery, query);

            // COUNT without joins - FAST
            var totalCount = await baseQuery.CountAsync(cancellationToken);

            if (totalCount == 0)
            {
                return PagedResultFactory.Empty<Appointment>(query.Page, query.PageSize);
            }

            // Apply sorting
            baseQuery = ApplySorting(baseQuery, query);

            // NOW add includes for actual data fetch
            var items = await baseQuery
                .Skip(query.Skip)
                .Take(query.PageSize)
                // Select specific fields only
                .Select(a => new Appointment
                {
                    AppointmentId = a.AppointmentId,
                    AppointmentCode = a.AppointmentCode,
                    CustomerId = a.CustomerId,
                    Customer = new Customer
                    {
                        CustomerId = a.Customer.CustomerId,
                        FullName = a.Customer.FullName,
                        PhoneNumber = a.Customer.PhoneNumber,
                        Email = a.Customer.Email
                    },
                    VehicleId = a.VehicleId,
                    Vehicle = new CustomerVehicle
                    {
                        VehicleId = a.Vehicle.VehicleId,
                        LicensePlate = a.Vehicle.LicensePlate,
                        Model = new CarModel
                        {
                            ModelId = a.Vehicle.Model.ModelId,
                            ModelName = a.Vehicle.Model.ModelName,
                            Year = a.Vehicle.Model.Year,
                            Brand = new CarBrand
                            {
                                BrandId = a.Vehicle.Model.Brand.BrandId,
                                BrandName = a.Vehicle.Model.Brand.BrandName
                            }
                        }
                    },
                    ServiceCenterId = a.ServiceCenterId,
                    ServiceCenter = new ServiceCenter
                    {
                        CenterId = a.ServiceCenter.CenterId,
                        CenterName = a.ServiceCenter.CenterName,
                        Address = a.ServiceCenter.Address
                    },
                    SlotId = a.SlotId,
                    Slot = a.Slot == null ? null : new TimeSlot
                    {
                        SlotId = a.Slot.SlotId,
                        SlotDate = a.Slot.SlotDate,
                        StartTime = a.Slot.StartTime,
                        EndTime = a.Slot.EndTime
                    },
                    StatusId = a.StatusId,
                    Status = new AppointmentStatus
                    {
                        StatusId = a.Status.StatusId,
                        StatusName = a.Status.StatusName,
                        StatusColor = a.Status.StatusColor
                    },
                    PackageId = a.PackageId,
                    Package = a.Package == null ? null : new MaintenancePackage
                    {
                        PackageId = a.Package.PackageId,
                        PackageName = a.Package.PackageName,
                        TotalPrice = a.Package.TotalPrice
                    },
                    EstimatedDuration = a.EstimatedDuration,
                    EstimatedCost = a.EstimatedCost,
                    Priority = a.Priority,
                    Source = a.Source,
                    CreatedDate = a.CreatedDate,
                    // Load services separately to avoid cartesian product
                    AppointmentServices = a.AppointmentServices.Select(aps => new AppointmentService
                    {
                        AppointmentServiceId = aps.AppointmentServiceId,
                        ServiceId = aps.ServiceId,
                        ServiceSource = aps.ServiceSource,
                        Price = aps.Price,
                        EstimatedTime = aps.EstimatedTime,
                        Service = new MaintenanceService
                        {
                            ServiceId = aps.Service.ServiceId,
                            ServiceCode = aps.Service.ServiceCode,
                            ServiceName = aps.Service.ServiceName
                        }
                    }).ToList()
                })
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
            return await _context.Appointments
                .AsNoTracking()
                .Where(a => a.CustomerId == customerId)
                .Select(a => new Appointment
                {
                    AppointmentId = a.AppointmentId,
                    AppointmentCode = a.AppointmentCode,
                    ServiceCenter = new ServiceCenter
                    {
                        CenterId = a.ServiceCenter.CenterId,
                        CenterName = a.ServiceCenter.CenterName
                    },
                    Slot = a.Slot == null ? null : new TimeSlot
                    {
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
                    Vehicle = new CustomerVehicle
                    {
                        LicensePlate = a.Vehicle.LicensePlate,
                        Model = new CarModel
                        {
                            ModelName = a.Vehicle.Model.ModelName,
                            Brand = new CarBrand
                            {
                                BrandName = a.Vehicle.Model.Brand.BrandName
                            }
                        }
                    },
                    EstimatedCost = a.EstimatedCost,
                    AppointmentDate = a.AppointmentDate
                })
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

            return await _context.Appointments
                .AsNoTracking()
                .Where(a => a.CustomerId == customerId && a.Slot!.SlotDate >= today)
                .OrderBy(a => a.Slot!.SlotDate)
                    .ThenBy(a => a.Slot!.StartTime)
                .Take(limit)
                .Select(a => new Appointment
                {
                    AppointmentId = a.AppointmentId,
                    AppointmentCode = a.AppointmentCode,
                    ServiceCenter = new ServiceCenter
                    {
                        CenterName = a.ServiceCenter.CenterName,
                        Address = a.ServiceCenter.Address
                    },
                    Slot = new TimeSlot
                    {
                        SlotDate = a.Slot!.SlotDate,
                        StartTime = a.Slot.StartTime
                    },
                    Status = new AppointmentStatus
                    {
                        StatusName = a.Status.StatusName
                    },
                    Vehicle = new CustomerVehicle
                    {
                        LicensePlate = a.Vehicle.LicensePlate
                    }
                })
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
    }
}
