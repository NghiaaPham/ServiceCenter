namespace EVServiceCenter.Core.Domains.Pricing.Models
{
    /// <summary>
    /// Kết quả validation promotion code
    /// </summary>
    public class PromotionValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public decimal DiscountAmount { get; set; }
        public int? PromotionId { get; set; }

        public static PromotionValidationResult Valid(int promotionId, decimal discountAmount)
        {
            return new PromotionValidationResult
            {
                IsValid = true,
                ErrorMessage = string.Empty,
                DiscountAmount = discountAmount,
                PromotionId = promotionId
            };
        }

        public static PromotionValidationResult Invalid(string errorMessage)
        {
            return new PromotionValidationResult
            {
                IsValid = false,
                ErrorMessage = errorMessage,
                DiscountAmount = 0,
                PromotionId = null
            };
        }
    }
}
