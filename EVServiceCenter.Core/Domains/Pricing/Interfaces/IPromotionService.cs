using EVServiceCenter.Core.Domains.Pricing.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EVServiceCenter.Core.Domains.Pricing.Interfaces
{
    /// <summary>
    /// Service quản lý promotion codes
    /// </summary>
    public interface IPromotionService
    {
        /// <summary>
        /// Validate promotion code
        /// </summary>
        /// <param name="promotionCode">Mã khuyến mãi</param>
        /// <param name="customerId">ID customer</param>
        /// <param name="orderAmount">Tổng tiền đơn hàng</param>
        /// <param name="serviceIds">Danh sách service IDs</param>
        /// <returns>Kết quả validation</returns>
        Task<PromotionValidationResult> ValidatePromotionAsync(
            string promotionCode,
            int customerId,
            decimal orderAmount,
            List<int> serviceIds);

        /// <summary>
        /// Tăng usage count của promotion sau khi apply thành công
        /// </summary>
        /// <param name="promotionCode">Mã khuyến mãi</param>
        Task IncrementUsageAsync(string promotionCode);

        /// <summary>
        /// Giảm usage count của promotion khi appointment bị cancel
        /// ✅ FIX GAP #8: Decrement promotion usage on cancellation
        /// </summary>
        /// <param name="promotionCode">Mã khuyến mãi</param>
        Task DecrementUsageAsync(string promotionCode);
    }
}
