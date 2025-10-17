using EVServiceCenter.Core.Domains.Customers.DTOs.Requests;
using FluentValidation;

namespace EVServiceCenter.Infrastructure.Domains.Customers.Validators
{
    /// <summary>
    /// Validator cho UpdateMyVehicleRequestDto
    /// 
    /// Performance optimizations:
    /// - S? d?ng When() ?? skip validation n?u field null
    /// - Không query database (validation ??n gi?n)
    /// - Fast-fail v?i simple rules
    /// </summary>
    public class UpdateMyVehicleValidator : AbstractValidator<UpdateMyVehicleRequestDto>
    {
        // Pre-defined valid conditions ?? tránh allocate array m?i l?n
        private static readonly string[] ValidConditions = { "Good", "Fair", "Poor", "Excellent" };

        public UpdateMyVehicleValidator()
        {
            // Mileage validation - ch? validate n?u có value
            When(x => x.Mileage.HasValue, () =>
            {
                RuleFor(x => x.Mileage!.Value)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("S? km ph?i >= 0");
            });

            // Color validation - ch? validate n?u không null/empty
            When(x => !string.IsNullOrWhiteSpace(x.Color), () =>
            {
                RuleFor(x => x.Color!)
                    .MaximumLength(50)
                    .WithMessage("Màu xe không ???c v??t quá 50 ký t?");
            });

            // Battery health validation
            When(x => x.BatteryHealthPercent.HasValue, () =>
            {
                RuleFor(x => x.BatteryHealthPercent!.Value)
                    .InclusiveBetween(0, 100)
                    .WithMessage("S?c kh?e pin ph?i t? 0-100%");
            });

            // Vehicle condition validation - s? d?ng static array
            When(x => !string.IsNullOrWhiteSpace(x.VehicleCondition), () =>
            {
                RuleFor(x => x.VehicleCondition!)
                    .Must(condition => ValidConditions.Contains(condition))
                    .WithMessage("Tình tr?ng xe ph?i là: Good, Fair, Poor ho?c Excellent");
            });

            // Insurance number validation
            When(x => !string.IsNullOrWhiteSpace(x.InsuranceNumber), () =>
            {
                RuleFor(x => x.InsuranceNumber!)
                    .MaximumLength(50)
                    .WithMessage("S? b?o hi?m không ???c v??t quá 50 ký t?");
            });

            // Insurance expiry validation - cho phép past date (?ã h?t h?n)
            // Nh?ng không quá xa trong quá kh? (> 1 n?m)
            When(x => x.InsuranceExpiry.HasValue, () =>
            {
                RuleFor(x => x.InsuranceExpiry!.Value)
                    .GreaterThan(DateOnly.FromDateTime(DateTime.Today.AddYears(-1)))
                    .WithMessage("Ngày h?t h?n b?o hi?m không h?p l? (quá c?)");
            });

            // Registration expiry validation
            When(x => x.RegistrationExpiry.HasValue, () =>
            {
                RuleFor(x => x.RegistrationExpiry!.Value)
                    .GreaterThan(DateOnly.FromDateTime(DateTime.Today.AddYears(-1)))
                    .WithMessage("Ngày h?t h?n ??ng ki?m không h?p l? (quá c?)");
            });
        }
    }
}
