using EVServiceCenter.Core.Domains.Customers.DTOs.Requests;
using FluentValidation;

namespace EVServiceCenter.Infrastructure.Domains.Customers.Validators
{
    /// <summary>
    /// Validator cho UpdateMyVehicleRequestDto
    /// 
    /// Performance optimizations:
    /// - S? d?ng When() ?? skip validation n?u field null
    /// - Kh�ng query database (validation ??n gi?n)
    /// - Fast-fail v?i simple rules
    /// </summary>
    public class UpdateMyVehicleValidator : AbstractValidator<UpdateMyVehicleRequestDto>
    {
        // Pre-defined valid conditions ?? tr�nh allocate array m?i l?n
        private static readonly string[] ValidConditions = { "Good", "Fair", "Poor", "Excellent" };

        public UpdateMyVehicleValidator()
        {
            // Mileage validation - ch? validate n?u c� value
            When(x => x.Mileage.HasValue, () =>
            {
                RuleFor(x => x.Mileage!.Value)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("S? km ph?i >= 0");
            });

            // Color validation - ch? validate n?u kh�ng null/empty
            When(x => !string.IsNullOrWhiteSpace(x.Color), () =>
            {
                RuleFor(x => x.Color!)
                    .MaximumLength(50)
                    .WithMessage("M�u xe kh�ng ???c v??t qu� 50 k� t?");
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
                    .WithMessage("T�nh tr?ng xe ph?i l�: Good, Fair, Poor ho?c Excellent");
            });

            // Insurance number validation
            When(x => !string.IsNullOrWhiteSpace(x.InsuranceNumber), () =>
            {
                RuleFor(x => x.InsuranceNumber!)
                    .MaximumLength(50)
                    .WithMessage("S? b?o hi?m kh�ng ???c v??t qu� 50 k� t?");
            });

            // Insurance expiry validation - cho ph�p past date (?� h?t h?n)
            // Nh?ng kh�ng qu� xa trong qu� kh? (> 1 n?m)
            When(x => x.InsuranceExpiry.HasValue, () =>
            {
                RuleFor(x => x.InsuranceExpiry!.Value)
                    .GreaterThan(DateOnly.FromDateTime(DateTime.Today.AddYears(-1)))
                    .WithMessage("Ng�y h?t h?n b?o hi?m kh�ng h?p l? (qu� c?)");
            });

            // Registration expiry validation
            When(x => x.RegistrationExpiry.HasValue, () =>
            {
                RuleFor(x => x.RegistrationExpiry!.Value)
                    .GreaterThan(DateOnly.FromDateTime(DateTime.Today.AddYears(-1)))
                    .WithMessage("Ng�y h?t h?n ??ng ki?m kh�ng h?p l? (qu� c?)");
            });
        }
    }
}
