using System.ComponentModel.DataAnnotations;

namespace EVServiceCenter.Core.Domains.Customers.DTOs.Requests
{
    /// <summary>
    /// DTO cho customer t? update xe c?a mình
    /// Ch? cho phép update m?t s? field nh?t ??nh, không cho s?a critical fields
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
        /// Màu xe (có th? thay ??i khi s?n l?i)
        /// </summary>
        [StringLength(50, ErrorMessage = "Màu xe không ???c v??t quá 50 ký t?")]
        public string? Color { get; set; }

        /// <summary>
        /// S?c kh?e pin (%)
        /// Range: 0-100
        /// </summary>
        [Range(0, 100, ErrorMessage = "S?c kh?e pin ph?i t? 0-100%")]
        public decimal? BatteryHealthPercent { get; set; }

        /// <summary>
        /// Tình tr?ng xe: Good, Fair, Poor, Excellent
        /// </summary>
        [StringLength(20, ErrorMessage = "Tình tr?ng xe không ???c v??t quá 20 ký t?")]
        public string? VehicleCondition { get; set; }

        /// <summary>
        /// S? b?o hi?m
        /// </summary>
        [StringLength(50, ErrorMessage = "S? b?o hi?m không ???c v??t quá 50 ký t?")]
        public string? InsuranceNumber { get; set; }

        /// <summary>
        /// Ngày h?t h?n b?o hi?m
        /// </summary>
        public DateOnly? InsuranceExpiry { get; set; }

        /// <summary>
        /// Ngày h?t h?n ??ng ki?m
        /// </summary>
        public DateOnly? RegistrationExpiry { get; set; }
    }
}
