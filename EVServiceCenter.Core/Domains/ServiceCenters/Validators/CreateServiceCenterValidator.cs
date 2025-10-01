using EVServiceCenter.Core.Domains.ServiceCenters.DTOs.Requests;
using FluentValidation;

namespace EVServiceCenter.Core.Domains.ServiceCenters.Validators
{
    public class CreateServiceCenterValidator : AbstractValidator<CreateServiceCenterRequestDto>
    {
        public CreateServiceCenterValidator()
        {
            RuleFor(x => x.CenterName)
                .NotEmpty().WithMessage("Tên trung tâm không được để trống")
                .MaximumLength(100).WithMessage("Tên trung tâm không được vượt quá 100 ký tự")
                .Matches(@"^[\p{L}\p{N}\s\-.,()]+$").WithMessage("Tên trung tâm chứa ký tự không hợp lệ");

            RuleFor(x => x.CenterCode)
                .NotEmpty().WithMessage("Mã trung tâm không được để trống")
                .MaximumLength(20).WithMessage("Mã trung tâm không được vượt quá 20 ký tự")
                .Matches(@"^[A-Z0-9\-]+$").WithMessage("Mã trung tâm chỉ được chứa chữ hoa, số và dấu gạch ngang");

            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Địa chỉ không được để trống")
                .MaximumLength(200).WithMessage("Địa chỉ không được vượt quá 200 ký tự");

            When(x => !string.IsNullOrEmpty(x.Ward), () =>
            {
                RuleFor(x => x.Ward)
                    .MaximumLength(100).WithMessage("Tên phường/xã không được vượt quá 100 ký tự");
            });

            When(x => !string.IsNullOrEmpty(x.District), () =>
            {
                RuleFor(x => x.District)
                    .MaximumLength(100).WithMessage("Tên quận/huyện không được vượt quá 100 ký tự");
            });

            When(x => !string.IsNullOrEmpty(x.Province), () =>
            {
                RuleFor(x => x.Province)
                    .MaximumLength(100).WithMessage("Tên tỉnh/thành không được vượt quá 100 ký tự");
            });

            When(x => !string.IsNullOrEmpty(x.PostalCode), () =>
            {
                RuleFor(x => x.PostalCode)
                    .Matches(@"^\d{5,6}$").WithMessage("Mã bưu điện phải có 5-6 chữ số");
            });

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Số điện thoại không được để trống")
                .Matches(@"^(0|\+84)(3|5|7|8|9)\d{8}$")
                .WithMessage("Số điện thoại không hợp lệ (ví dụ: 0901234567)");

            When(x => !string.IsNullOrEmpty(x.Email), () =>
            {
                RuleFor(x => x.Email)
                    .EmailAddress().WithMessage("Email không hợp lệ")
                    .MaximumLength(100).WithMessage("Email không được vượt quá 100 ký tự");
            });

            When(x => !string.IsNullOrEmpty(x.Website), () =>
            {
                RuleFor(x => x.Website)
                    .Must(BeAValidUrl).WithMessage("Website không hợp lệ");
            });

            When(x => x.Latitude.HasValue, () =>
            {
                RuleFor(x => x.Latitude)
                    .InclusiveBetween(-90, 90).WithMessage("Vĩ độ phải trong khoảng -90 đến 90");
            });

            When(x => x.Longitude.HasValue, () =>
            {
                RuleFor(x => x.Longitude)
                    .InclusiveBetween(-180, 180).WithMessage("Kinh độ phải trong khoảng -180 đến 180");
            });

            RuleFor(x => x.OpenTime)
                .Must(BeAValidTime).WithMessage("Giờ mở cửa không hợp lệ");

            RuleFor(x => x.CloseTime)
                .Must(BeAValidTime).WithMessage("Giờ đóng cửa không hợp lệ");

            RuleFor(x => x)
                .Must(x => x.CloseTime > x.OpenTime)
                .WithMessage("Giờ đóng cửa phải sau giờ mở cửa")
                .WithName("CloseTime");

            RuleFor(x => x.Capacity)
                .GreaterThan(0).WithMessage("Công suất phải lớn hơn 0")
                .LessThanOrEqualTo(100).WithMessage("Công suất không được vượt quá 100");

            When(x => !string.IsNullOrEmpty(x.Description), () =>
            {
                RuleFor(x => x.Description)
                    .MaximumLength(500).WithMessage("Mô tả không được vượt quá 500 ký tự");
            });

            When(x => !string.IsNullOrEmpty(x.Facilities), () =>
            {
                RuleFor(x => x.Facilities)
                    .Must(BeValidFacilities).WithMessage("Thông tin tiện ích quá dài");
            });
        }

        private bool BeAValidTime(TimeOnly time)
        {
            return time >= new TimeOnly(0, 0) && time <= new TimeOnly(23, 59, 59);
        }

        private bool BeAValidUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return true;

            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        private bool BeValidFacilities(string? facilities)
        {
            return facilities == null || facilities.Length <= 5000;
        }
    }
}