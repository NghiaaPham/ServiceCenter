namespace EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Response
{
    /// <summary>
    /// DTO hi?n th? breakdown discount chi ti?t cho customer
    /// ???c s? d?ng trong AppointmentResponseDto ?? transparency v? pricing
    /// </summary>
    public class DiscountSummaryDto
    {
        /// <summary>
        /// T?ng gi� g?c c?a c�c services (ch? t�nh Regular/Extra, kh�ng t�nh Subscription)
        /// </summary>
        public decimal OriginalTotal { get; set; }

        /// <summary>
        /// Discount t? CustomerType (VD: VIP 10%, Gold 5%)
        /// = OriginalTotal � (CustomerTypeDiscountPercent / 100)
        /// </summary>
        public decimal CustomerTypeDiscount { get; set; }

        /// <summary>
        /// T�n lo?i customer (VD: "VIP", "Gold", "Silver", "Regular")
        /// </summary>
        public string? CustomerTypeName { get; set; }

        /// <summary>
        /// Discount t? Promotion code
        /// C� th? l� Percentage ho?c Fixed Amount t�y lo?i promotion
        /// </summary>
        public decimal PromotionDiscount { get; set; }

        /// <summary>
        /// M� promotion ?� s? d?ng (n?u c�)
        /// VD: "SUMMER20", "FLASH100K"
        /// </summary>
        public string? PromotionCodeUsed { get; set; }

        /// <summary>
        /// Discount cu?i c�ng ???c �p d?ng
        /// = MAX(CustomerTypeDiscount, PromotionDiscount)
        /// Theo rule: Ch?n discount cao nh?t, kh�ng c?ng d?n
        /// </summary>
        public decimal FinalDiscount { get; set; }

        /// <summary>
        /// Lo?i discount ???c apply:
        /// - "None": Kh�ng c� discount
        /// - "CustomerType": �p d?ng discount t? lo?i kh�ch h�ng
        /// - "Promotion": �p d?ng discount t? m� khuy?n m�i
        /// </summary>
        public string AppliedDiscountType { get; set; } = "None";

        /// <summary>
        /// T?ng cu?i c�ng sau khi tr? discount
        /// = OriginalTotal - FinalDiscount
        /// </summary>
        public decimal FinalTotal { get; set; }

        /// <summary>
        /// Text formatted ?? hi?n th? tr�n UI
        /// Bao g?m emoji v� format ti?n t? VN?
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
                sb.AppendLine($"?? Gi� g?c: {OriginalTotal:N0}?");
                sb.AppendLine();

                if (CustomerTypeDiscount > 0 || PromotionDiscount > 0)
                {
                    sb.AppendLine("?? GI?M GI�:");

                    if (CustomerTypeDiscount > 0)
                    {
                        sb.AppendLine($"  ? Kh�ch h�ng {CustomerTypeName ?? "VIP"}: -{CustomerTypeDiscount:N0}?");
                    }

                    if (PromotionDiscount > 0)
                    {
                        sb.AppendLine($"  ?? M� {PromotionCodeUsed}: -{PromotionDiscount:N0}?");
                    }

                    sb.AppendLine($"  ? �p d?ng cao nh?t: -{FinalDiscount:N0}? ({AppliedDiscountType})");
                    sb.AppendLine();
                }

                sb.AppendLine($"?? T?ng c?ng: {FinalTotal:N0}?");
                sb.AppendLine($"? B?n ti?t ki?m: {FinalDiscount:N0}?");

                return sb.ToString();
            }
        }
    }
}
