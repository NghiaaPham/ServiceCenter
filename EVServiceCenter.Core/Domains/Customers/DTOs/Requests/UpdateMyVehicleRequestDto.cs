using System.ComponentModel.DataAnnotations;

namespace EVServiceCenter.Core.Domains.Customers.DTOs.Requests
{
    /// <summary>
    /// DTO cho customer t? update xe c?a m�nh
    /// Ch? cho ph�p update m?t s? field nh?t ??nh, kh�ng cho s?a critical fields
    /// 
    /// Performance optimizations:
    /// - Ch? ch?a fields c?n thi?t
    /// - Nullable ?? support partial updates
    /// - Data annotations cho server-side validation
    /// </summary>
    public class UpdateMyVehicleRequestDto
    {
        /// <summary>
        /// S? km hi?n t?i c?a xe
        /// Validation: Ph?i >= s? km c? (checked in service layer)
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "S? km ph?i >= 0")]
        public int? Mileage { get; set; }

        /// <summary>
        /// M�u xe (c� th? thay ??i khi s?n l?i)
        /// </summary>
        [StringLength(50, ErrorMessage = "M�u xe kh�ng ???c v??t qu� 50 k� t?")]
        public string? Color { get; set; }

        /// <summary>
        /// S?c kh?e pin (%)
        /// Range: 0-100
        /// </summary>
        [Range(0, 100, ErrorMessage = "S?c kh?e pin ph?i t? 0-100%")]
        public decimal? BatteryHealthPercent { get; set; }

        /// <summary>
        /// T�nh tr?ng xe: Good, Fair, Poor, Excellent
        /// </summary>
        [StringLength(20, ErrorMessage = "T�nh tr?ng xe kh�ng ???c v??t qu� 20 k� t?")]
        public string? VehicleCondition { get; set; }

        /// <summary>
        /// S? b?o hi?m
        /// </summary>
        [StringLength(50, ErrorMessage = "S? b?o hi?m kh�ng ???c v??t qu� 50 k� t?")]
        public string? InsuranceNumber { get; set; }

        /// <summary>
        /// Ng�y h?t h?n b?o hi?m
        /// </summary>
        public DateOnly? InsuranceExpiry { get; set; }

        /// <summary>
        /// Ng�y h?t h?n ??ng ki?m
        /// </summary>
        public DateOnly? RegistrationExpiry { get; set; }
    }
}
