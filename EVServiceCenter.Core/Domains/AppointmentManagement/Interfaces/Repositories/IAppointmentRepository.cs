using EVServiceCenter.Core.Domains.AppointmentManagement.Entities;
using EVServiceCenter.Core.Domains.Shared.Interfaces;

namespace EVServiceCenter.Core.Domains.AppointmentManagement.Interfaces.Repositories
{
    public interface IAppointmentRepository : IRepository<Appointment>
    {
        Task<Appointment?> GetByIdWithDetailsAsync(
            int appointmentId,
            CancellationToken cancellationToken = default);

        Task<Appointment?> GetByCodeAsync(
            string appointmentCode,
            CancellationToken cancellationToken = default);

        Task<bool> ExistsByCodeAsync(
            string appointmentCode,
            CancellationToken cancellationToken = default);
    }
}