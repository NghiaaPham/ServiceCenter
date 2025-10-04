using EVServiceCenter.Core.Domains.AppointmentManagement.Entities;
using EVServiceCenter.Core.Domains.AppointmentManagement.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.CarBrands.Entities;
using EVServiceCenter.Core.Domains.CarModels.Entities;
using EVServiceCenter.Core.Domains.Customers.Entities;
using EVServiceCenter.Core.Domains.CustomerVehicles.Entities;
using EVServiceCenter.Core.Domains.Identity.Entities;
using EVServiceCenter.Core.Domains.MaintenanceServices.Entities;
using EVServiceCenter.Core.Domains.ServiceCategories.Entities;
using EVServiceCenter.Core.Domains.ServiceCenters.Entities;
using EVServiceCenter.Core.Domains.TimeSlots.Entities;
using EVServiceCenter.Core.Entities;
using EVServiceCenter.Infrastructure.Domains.Shared.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EVServiceCenter.Infrastructure.Domains.AppointmentManagement.Repositories
{
    public class AppointmentRepository : Repository<Appointment>, IAppointmentRepository
    {
        public AppointmentRepository(EVDbContext context) : base(context)
        {
        }

        public async Task<Appointment?> GetByIdWithDetailsAsync(
    int appointmentId,
    CancellationToken cancellationToken = default)
        {
            return await _context.Appointments
                .AsNoTracking()
                .Where(a => a.AppointmentId == appointmentId)
                .Select(a => new Appointment
                {
                    AppointmentId = a.AppointmentId,
                    AppointmentCode = a.AppointmentCode,
                    AppointmentDate = a.AppointmentDate,

                    // Customer - chỉ lấy fields cần thiết
                    CustomerId = a.CustomerId,
                    Customer = new Customer
                    {
                        CustomerId = a.Customer.CustomerId,
                        FullName = a.Customer.FullName,
                        Email = a.Customer.Email,
                        PhoneNumber = a.Customer.PhoneNumber
                    },

                    // Vehicle
                    VehicleId = a.VehicleId,
                    Vehicle = new CustomerVehicle
                    {
                        VehicleId = a.Vehicle.VehicleId,
                        LicensePlate = a.Vehicle.LicensePlate,
                        Vin = a.Vehicle.Vin,
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

                    // Service Center
                    ServiceCenterId = a.ServiceCenterId,
                    ServiceCenter = new ServiceCenter
                    {
                        CenterId = a.ServiceCenter.CenterId,
                        CenterName = a.ServiceCenter.CenterName,
                        CenterCode = a.ServiceCenter.CenterCode,
                        Address = a.ServiceCenter.Address,
                        PhoneNumber = a.ServiceCenter.PhoneNumber,
                        Email = a.ServiceCenter.Email
                    },

                    // Time Slot
                    SlotId = a.SlotId,
                    Slot = a.Slot == null ? null : new TimeSlot
                    {
                        SlotId = a.Slot.SlotId,
                        SlotDate = a.Slot.SlotDate,
                        StartTime = a.Slot.StartTime,
                        EndTime = a.Slot.EndTime,
                        MaxBookings = a.Slot.MaxBookings
                    },

                    // Status
                    StatusId = a.StatusId,
                    Status = new AppointmentStatus
                    {
                        StatusId = a.Status.StatusId,
                        StatusName = a.Status.StatusName,
                        StatusColor = a.Status.StatusColor,
                        Description = a.Status.Description,
                    },

                    // Package (if any)
                    PackageId = a.PackageId,
                    Package = a.Package == null ? null : new MaintenancePackage
                    {
                        PackageId = a.Package.PackageId,
                        PackageCode = a.Package.PackageCode,
                        PackageName = a.Package.PackageName,
                        Description = a.Package.Description,
                        TotalPrice = a.Package.TotalPrice,
                        ValidityPeriod = a.Package.ValidityPeriod
                    },

                    // Services - projection to avoid cartesian product
                    AppointmentServices = a.AppointmentServices.Select(aps => new AppointmentService
                    {
                        AppointmentServiceId = aps.AppointmentServiceId,
                        ServiceId = aps.ServiceId,
                        ServiceSource = aps.ServiceSource,
                        Price = aps.Price,
                        EstimatedTime = aps.EstimatedTime,
                        Notes = aps.Notes,
                        Service = new MaintenanceService
                        {
                            ServiceId = aps.Service.ServiceId,
                            ServiceCode = aps.Service.ServiceCode,
                            ServiceName = aps.Service.ServiceName,
                            Description = aps.Service.Description,
                            BasePrice = aps.Service.BasePrice,
                            Category = new ServiceCategory
                            {
                                CategoryId = aps.Service.Category.CategoryId,
                                CategoryName = aps.Service.Category.CategoryName
                            }
                        }
                    }).ToList(),

                    // Preferred Technician (if any)
                    PreferredTechnicianId = a.PreferredTechnicianId,
                    PreferredTechnician = a.PreferredTechnician == null ? null : new User
                    {
                        UserId = a.PreferredTechnician.UserId,
                        FullName = a.PreferredTechnician.FullName,
                        Email = a.PreferredTechnician.Email
                    },

                    // Other fields
                    EstimatedDuration = a.EstimatedDuration,
                    EstimatedCost = a.EstimatedCost,
                    ServiceDescription = a.ServiceDescription,
                    CustomerNotes = a.CustomerNotes,
                    Priority = a.Priority,
                    Source = a.Source,

                    // Confirmation
                    ConfirmationDate = a.ConfirmationDate,
                    ConfirmationMethod = a.ConfirmationMethod,
                    ConfirmationStatus = a.ConfirmationStatus,

                    // Reminder
                    ReminderSent = a.ReminderSent,
                    ReminderSentDate = a.ReminderSentDate,

                    // Flags
                    NoShowFlag = a.NoShowFlag,

                    // Cancellation
                    CancellationReason = a.CancellationReason,
                    RescheduledFromId = a.RescheduledFromId,

                    // Audit
                    CreatedDate = a.CreatedDate,
                    CreatedBy = a.CreatedBy,
                    UpdatedDate = a.UpdatedDate,
                    UpdatedBy = a.UpdatedBy
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<Appointment?> GetByCodeAsync(
            string appointmentCode,
            CancellationToken cancellationToken = default)
        {
            return await _context.Appointments
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.AppointmentCode == appointmentCode, cancellationToken);
        }

        public async Task<bool> ExistsByCodeAsync(
            string appointmentCode,
            CancellationToken cancellationToken = default)
        {
            return await _context.Appointments
                .AsNoTracking()
                .AnyAsync(a => a.AppointmentCode == appointmentCode, cancellationToken);
        }
    }
}