using EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Request;
using FluentValidation;

namespace EVServiceCenter.Core.Domains.AppointmentManagement.Validators
{
    /// <summary>
    /// Validator cho AdjustServiceSourceRequestDto
    /// Đảm bảo dữ liệu hợp lệ trước khi Admin điều chỉnh ServiceSource
    /// </summary>
    public class AdjustServiceSourceValidator : AbstractValidator<AdjustServiceSourceRequestDto>
    {
        /// <summary>
        /// Các ServiceSource hợp lệ
        /// </summary>
        private static readonly string[] ValidServiceSources = { "Subscription", "Extra", "Regular" };

        public AdjustServiceSourceValidator()
        {
            // Validate NewServiceSource
            RuleFor(x => x.NewServiceSource)
                .NotEmpty()
                .WithMessage("NewServiceSource không được để trống")
                .Must(source => ValidServiceSources.Contains(source))
                .WithMessage($"NewServiceSource phải là một trong: {string.Join(", ", ValidServiceSources)}");

            // Validate NewPrice
            RuleFor(x => x.NewPrice)
                .GreaterThanOrEqualTo(0)
                .WithMessage("NewPrice phải >= 0");

            // Validate Reason
            RuleFor(x => x.Reason)
                .NotEmpty()
                .WithMessage("Reason không được để trống (bắt buộc cho audit trail)")
                .MinimumLength(10)
                .WithMessage("Reason phải ít nhất 10 ký tự để đảm bảo mô tả rõ ràng")
                .MaximumLength(500)
                .WithMessage("Reason không được vượt quá 500 ký tự");
        }
    }
}
