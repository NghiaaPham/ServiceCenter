using EVServiceCenter.Core.Domains.Identity.Entities;
using EVServiceCenter.Core.Domains.Identity.DTOs;

namespace EVServiceCenter.Core.Domains.Identity.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(User user, TokenCustomerInfo? customerInfo = null);
    }
}
