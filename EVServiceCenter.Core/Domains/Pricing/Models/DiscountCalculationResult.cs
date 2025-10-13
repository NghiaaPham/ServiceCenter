using System.Collections.Generic;

namespace EVServiceCenter.Core.Domains.Pricing.Models
{
    /// <summary>
    /// Kết quả tính discount
    /// </summary>
    public class DiscountCalculationResult
    {
        /// <summary>
        /// Tổng tiền gốc (trước discount)
        /// </summary>
        public decimal OriginalTotal { get; set; }

        /// <summary>
        /// Discount từ CustomerType (nếu có)
        /// </summary>
        public decimal CustomerTypeDiscount { get; set; }

        /// <summary>
        /// Discount từ Promotion (nếu có)
        /// </summary>
        public decimal PromotionDiscount { get; set; }

        /// <summary>
        /// Discount cuối cùng được áp dụng = MAX(CustomerType, Promotion)
        /// </summary>
        public decimal FinalDiscount { get; set; }

        /// <summary>
        /// Tổng tiền sau discount
        /// </summary>
        public decimal FinalTotal { get; set; }

        /// <summary>
        /// Loại discount được áp dụng: "None" | "CustomerType" | "Promotion"
        /// </summary>
        public string AppliedDiscountType { get; set; } = "None";

        /// <summary>
        /// Mã promotion đã sử dụng (nếu có)
        /// </summary>
        public string? PromotionCodeUsed { get; set; }

        /// <summary>
        /// ID của promotion (nếu có)
        /// </summary>
        public int? PromotionId { get; set; }

        /// <summary>
        /// Chi tiết discount từng service
        /// </summary>
        public List<ServiceDiscountBreakdown> ServiceBreakdowns { get; set; } = new();
    }

    /// <summary>
    /// Chi tiết discount của 1 service
    /// </summary>
    public class ServiceDiscountBreakdown
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string ServiceSource { get; set; } = string.Empty;
        public decimal OriginalPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalPrice { get; set; }
        public string DiscountReason { get; set; } = string.Empty;
    }
}
