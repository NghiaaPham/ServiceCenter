using EVServiceCenter.Core.Domains.Pricing.Models;
using System.Threading.Tasks;

namespace EVServiceCenter.Core.Domains.Pricing.Interfaces
{
    /// <summary>
    /// Service tính toán discount cho appointment
    /// Rule: Subscription services = 100% free, Regular/Extra services = MAX(CustomerType, Promotion)
    /// </summary>
    public interface IDiscountCalculationService
    {
        /// <summary>
        /// Tính discount cho một appointment
        /// </summary>
        /// <param name="request">Thông tin appointment và customer</param>
        /// <returns>Kết quả discount calculation</returns>
        Task<DiscountCalculationResult> CalculateDiscountAsync(DiscountCalculationRequest request);
    }
}
