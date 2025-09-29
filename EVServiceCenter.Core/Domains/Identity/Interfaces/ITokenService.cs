using EVServiceCenter.Core.Domains.Identity.Entities;

namespace EVServiceCenter.Core.Domains.Identity.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(User user, int? customerId = null);
    }
}
