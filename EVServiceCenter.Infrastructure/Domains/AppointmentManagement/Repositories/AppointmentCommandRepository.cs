﻿using EVServiceCenter.Core.Domains.AppointmentManagement.Entities;
using EVServiceCenter.Core.Domains.AppointmentManagement.Interfaces.Repositories;
using EVServiceCenter.Core.Entities;
using EVServiceCenter.Core.Enums;

using Microsoft.EntityFrameworkCore;

namespace EVServiceCenter.Infrastructure.Domains.AppointmentManagement.Repositories
{
    public class AppointmentCommandRepository : IAppointmentCommandRepository
    {
        private readonly EVDbContext _context;

        public AppointmentCommandRepository(EVDbContext context)
        {
            _context = context;
        }

        public async Task<Appointment> CreateWithServicesAsync(
            Appointment appointment,
            List<AppointmentService> appointmentServices,
            CancellationToken cancellationToken = default)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                await _context.Appointments.AddAsync(appointment, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                foreach (var service in appointmentServices)
                {
                    service.AppointmentId = appointment.AppointmentId;
                    service.CreatedDate = DateTime.UtcNow;
                }

                await _context.AppointmentServices.AddRangeAsync(appointmentServices, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);
                return appointment;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task UpdateServicesAsync(
            int appointmentId,
            List<AppointmentService> newServices,
            CancellationToken cancellationToken = default)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var oldServices = await _context.AppointmentServices
                    .Where(aps => aps.AppointmentId == appointmentId)
                    .ToListAsync(cancellationToken);

                _context.AppointmentServices.RemoveRange(oldServices);

                foreach (var service in newServices)
                {
                    service.AppointmentId = appointmentId;
                    service.CreatedDate = DateTime.UtcNow;
                }

                await _context.AppointmentServices.AddRangeAsync(newServices, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<bool> UpdateStatusAsync(
            int appointmentId,
            int newStatusId,
            CancellationToken cancellationToken = default)
        {
            var rows = await _context.Appointments
                .Where(a => a.AppointmentId == appointmentId)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(a => a.StatusId, newStatusId)
                        .SetProperty(a => a.UpdatedDate, DateTime.UtcNow),
                    cancellationToken);

            return rows > 0;
        }

        public async Task<bool> CancelAsync(
            int appointmentId,
            string cancellationReason,
            CancellationToken cancellationToken = default)
        {
            var cancelledStatusId = (int)AppointmentStatusEnum.Cancelled;

            var rows = await _context.Appointments
                .Where(a => a.AppointmentId == appointmentId)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(a => a.StatusId, cancelledStatusId)
                        .SetProperty(a => a.CancellationReason, cancellationReason)
                        .SetProperty(a => a.UpdatedDate, DateTime.UtcNow),
                    cancellationToken);

            return rows > 0;
        }

        public async Task<Appointment> RescheduleAsync(
            int oldAppointmentId,
            Appointment newAppointment,
            List<AppointmentService> newServices,
            CancellationToken cancellationToken = default)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var rescheduledStatusId = (int)AppointmentStatusEnum.Rescheduled;
                await _context.Appointments
                    .Where(a => a.AppointmentId == oldAppointmentId)
                    .ExecuteUpdateAsync(
                        setters => setters
                            .SetProperty(a => a.StatusId, rescheduledStatusId)
                            .SetProperty(a => a.UpdatedDate, DateTime.UtcNow),
                        cancellationToken);

                newAppointment.RescheduledFromId = oldAppointmentId;
                await _context.Appointments.AddAsync(newAppointment, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                foreach (var service in newServices)
                {
                    service.AppointmentId = newAppointment.AppointmentId;
                    service.CreatedDate = DateTime.UtcNow;
                }

                await _context.AppointmentServices.AddRangeAsync(newServices, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);
                return newAppointment;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<bool> ConfirmAsync(
            int appointmentId,
            string confirmationMethod,
            CancellationToken cancellationToken = default)
        {
            var confirmedStatusId = (int)AppointmentStatusEnum.Confirmed;

            var rows = await _context.Appointments
                .Where(a => a.AppointmentId == appointmentId)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(a => a.StatusId, confirmedStatusId)
                        .SetProperty(a => a.ConfirmationDate, DateTime.UtcNow)
                        .SetProperty(a => a.ConfirmationMethod, confirmationMethod)
                        .SetProperty(a => a.ConfirmationStatus, "Confirmed")
                        .SetProperty(a => a.UpdatedDate, DateTime.UtcNow),
                    cancellationToken);

            return rows > 0;
        }

        public async Task<bool> MarkAsNoShowAsync(
            int appointmentId,
            CancellationToken cancellationToken = default)
        {
            var noShowStatusId = (int)AppointmentStatusEnum.NoShow;

            var rows = await _context.Appointments
                .Where(a => a.AppointmentId == appointmentId)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(a => a.StatusId, noShowStatusId)
                        .SetProperty(a => a.NoShowFlag, true)
                        .SetProperty(a => a.UpdatedDate, DateTime.UtcNow),
                    cancellationToken);

            return rows > 0;
        }

        public async Task<bool> DeleteIfPossibleAsync(
            int appointmentId,
            CancellationToken cancellationToken = default)
        {
            var pendingStatusId = (int)AppointmentStatusEnum.Pending;

            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId, cancellationToken);

            if (appointment == null || appointment.StatusId != pendingStatusId)
                return false;

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }

        public async Task<bool> UpdateAsync(
            Appointment appointment,
            CancellationToken cancellationToken = default)
        {
            _context.Appointments.Update(appointment);
            var rows = await _context.SaveChangesAsync(cancellationToken);
            return rows > 0;
        }
    }
}