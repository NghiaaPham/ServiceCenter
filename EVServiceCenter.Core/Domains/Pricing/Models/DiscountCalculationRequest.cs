using System.Collections.Generic;

namespace EVServiceCenter.Core.Domains.Pricing.Models
{
    /// <summary>
    /// Request model để tính discount cho một appointment
    /// </summary>
    public class DiscountCalculationRequest
    {
        /// <summary>
        /// ID của customer
        /// </summary>
        public int CustomerId { get; set; }

        /// <summary>
        /// ID loại khách hàng (VIP, Regular, New)
        /// </summary>
        public int? CustomerTypeId { get; set; }

        /// <summary>
        /// % discount của CustomerType (0-100)
        /// </summary>
        public decimal? CustomerTypeDiscountPercent { get; set; }

        /// <summary>
        /// Mã khuyến mãi (nếu có)
        /// </summary>
        public string? PromotionCode { get; set; }

        /// <summary>
        /// Danh sách services trong appointment
        /// </summary>
        public List<ServiceLineItem> Services { get; set; } = new();
    }

    /// <summary>
    /// Thông tin 1 service trong appointment
    /// </summary>
    public class ServiceLineItem
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public int Quantity { get; set; }

        /// <summary>
        /// Source của service: "Subscription" | "Extra" | "Regular"
        /// </summary>
        public string ServiceSource { get; set; } = "Regular";

        /// <summary>
        /// ID của subscription (nếu ServiceSource = "Subscription")
        /// </summary>
        public int? SubscriptionId { get; set; }
    }
}
