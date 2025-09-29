using System.Threading.Tasks;

namespace EVServiceCenter.Core.Domains.Identity.Interfaces
{
    public interface IEmailService
    {
        Task SendCustomerEmailVerificationAsync(string email, string fullName, string verificationToken);
        Task SendPasswordResetAsync(string email, string fullName, string resetToken);
        Task SendWelcomeEmailAsync(string email, string fullName);
        Task SendNotificationAsync(string email, string subject, string message);
        Task SendInternalStaffWelcomeEmailAsync(string email, string fullName, string username, string role, string department);
    }
}
