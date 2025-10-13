namespace EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Response
{
    /// <summary>
    /// DTO hi?n th? breakdown discount chi ti?t cho customer
    /// ???c s? d?ng trong AppointmentResponseDto ?? transparency v? pricing
    /// </summary>
    public class DiscountSummaryDto
    {
        /// <summary>
        /// T?ng giá g?c c?a các services (ch? tính Regular/Extra, không tính Subscription)
        /// </summary>
        public decimal OriginalTotal { get; set; }

        /// <summary>
        /// Discount t? CustomerType (VD: VIP 10%, Gold 5%)
        /// = OriginalTotal × (CustomerTypeDiscountPercent / 100)
        /// </summary>
        public decimal CustomerTypeDiscount { get; set; }

        /// <summary>
        /// Tên lo?i customer (VD: "VIP", "Gold", "Silver", "Regular")
        /// </summary>
        public string? CustomerTypeName { get; set; }

        /// <summary>
        /// Discount t? Promotion code
        /// Có th? là Percentage ho?c Fixed Amount tùy lo?i promotion
        /// </summary>
        public decimal PromotionDiscount { get; set; }

        /// <summary>
        /// Mã promotion ?ã s? d?ng (n?u có)
        /// VD: "SUMMER20", "FLASH100K"
        /// </summary>
        public string? PromotionCodeUsed { get; set; }

        /// <summary>
        /// Discount cu?i cùng ???c áp d?ng
        /// = MAX(CustomerTypeDiscount, PromotionDiscount)
        /// Theo rule: Ch?n discount cao nh?t, không c?ng d?n
        /// </summary>
        public decimal FinalDiscount { get; set; }

        /// <summary>
        /// Lo?i discount ???c apply:
        /// - "None": Không có discount
        /// - "CustomerType": Áp d?ng discount t? lo?i khách hàng
        /// - "Promotion": Áp d?ng discount t? mã khuy?n mãi
        /// </summary>
        public string AppliedDiscountType { get; set; } = "None";

        /// <summary>
        /// T?ng cu?i cùng sau khi tr? discount
        /// = OriginalTotal - FinalDiscount
        /// </summary>
        public decimal FinalTotal { get; set; }

        /// <summary>
        /// Text formatted ?? hi?n th? trên UI
        /// Bao g?m emoji và format ti?n t? VN?
        /// </summary>
        public string DisplayText
        {
            get
            {
                if (FinalDiscount <= 0)
                {
                    return $"?? T?ng c?ng: {OriginalTotal:N0}?";
                }

                var sb = new System.Text.StringBuilder();
                sb.AppendLine($"?? Giá g?c: {OriginalTotal:N0}?");
                sb.AppendLine();

                if (CustomerTypeDiscount > 0 || PromotionDiscount > 0)
                {
                    sb.AppendLine("?? GI?M GIÁ:");

                    if (CustomerTypeDiscount > 0)
                    {
                        sb.AppendLine($"  ? Khách hàng {CustomerTypeName ?? "VIP"}: -{CustomerTypeDiscount:N0}?");
                    }

                    if (PromotionDiscount > 0)
                    {
                        sb.AppendLine($"  ?? Mã {PromotionCodeUsed}: -{PromotionDiscount:N0}?");
                    }

                    sb.AppendLine($"  ? Áp d?ng cao nh?t: -{FinalDiscount:N0}? ({AppliedDiscountType})");
                    sb.AppendLine();
                }

                sb.AppendLine($"?? T?ng c?ng: {FinalTotal:N0}?");
                sb.AppendLine($"? B?n ti?t ki?m: {FinalDiscount:N0}?");

                return sb.ToString();
            }
        }
    }
}
