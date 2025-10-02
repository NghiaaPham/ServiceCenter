using EVServiceCenter.Core.Domains.MaintenanceServices.DTOs.Requests;
using FluentValidation;

namespace EVServiceCenter.Core.Domains.MaintenanceServices.Validators
{
    public class UpdateMaintenanceServiceValidator : AbstractValidator<UpdateMaintenanceServiceRequestDto>
    {
        private readonly string[] _validSkillLevels = new[] { "Entry", "Intermediate", "Expert" };

        public UpdateMaintenanceServiceValidator()
        {
            RuleFor(x => x.ServiceId)
                .GreaterThan(0).WithMessage("ID dịch vụ không hợp lệ");

            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("Loại dịch vụ không hợp lệ");

            RuleFor(x => x.ServiceCode)
                .NotEmpty().WithMessage("Mã dịch vụ không được để trống")
                .MaximumLength(20).WithMessage("Mã dịch vụ không được vượt quá 20 ký tự")
                .Matches(@"^[A-Z0-9-]+$").WithMessage("Mã dịch vụ chỉ chứa chữ in hoa, số và dấu gạch ngang");

            RuleFor(x => x.ServiceName)
                .NotEmpty().WithMessage("Tên dịch vụ không được để trống")
                .MaximumLength(200).WithMessage("Tên dịch vụ không được vượt quá 200 ký tự");

            When(x => !string.IsNullOrEmpty(x.Description), () =>
            {
                RuleFor(x => x.Description)
                    .MaximumLength(1000).WithMessage("Mô tả không được vượt quá 1000 ký tự");
            });

            RuleFor(x => x.StandardTime)
                .GreaterThan(0).WithMessage("Thời gian chuẩn phải lớn hơn 0 phút")
                .LessThanOrEqualTo(480).WithMessage("Thời gian chuẩn không được vượt quá 480 phút");

            RuleFor(x => x.BasePrice)
                .GreaterThanOrEqualTo(0).WithMessage("Giá cơ bản phải >= 0");

            When(x => x.LaborCost.HasValue, () =>
            {
                RuleFor(x => x.LaborCost!.Value)
                    .GreaterThanOrEqualTo(0).WithMessage("Chi phí nhân công phải >= 0");
            });

            When(x => !string.IsNullOrEmpty(x.SkillLevel), () =>
            {
                RuleFor(x => x.SkillLevel)
                    .Must(level => _validSkillLevels.Contains(level!, StringComparer.OrdinalIgnoreCase))
                    .WithMessage($"Cấp độ kỹ năng phải là: {string.Join(", ", _validSkillLevels)}");
            });

            When(x => !string.IsNullOrEmpty(x.RequiredCertification), () =>
            {
                RuleFor(x => x.RequiredCertification)
                    .MaximumLength(200).WithMessage("Chứng chỉ yêu cầu không được vượt quá 200 ký tự");
            });

            When(x => x.IsWarrantyService, () =>
            {
                RuleFor(x => x.WarrantyPeriod)
                    .NotNull().WithMessage("Phải có thời hạn bảo hành khi là dịch vụ bảo hành")
                    .GreaterThan(0).WithMessage("Thời hạn bảo hành phải > 0 tháng")
                    .LessThanOrEqualTo(60).WithMessage("Thời hạn bảo hành không được vượt quá 60 tháng");
            });

            When(x => !string.IsNullOrEmpty(x.ImageUrl), () =>
            {
                RuleFor(x => x.ImageUrl)
                    .MaximumLength(500).WithMessage("URL hình ảnh không được vượt quá 500 ký tự")
                    .Must(BeAValidUrl).WithMessage("URL hình ảnh không hợp lệ");
            });
        }

        private bool BeAValidUrl(string? url)
        {
            if (string.IsNullOrEmpty(url)) return true;
            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }
    }
}