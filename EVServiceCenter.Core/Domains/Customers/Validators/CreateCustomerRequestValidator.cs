using EVServiceCenter.Core.Domains.Customers.DTOs.Requests;
using EVServiceCenter.Core.Domains.Customers.Interfaces;
using FluentValidation;
using System.Text.RegularExpressions;

namespace EVServiceCenter.Core.Domains.Customers.Validators
{
    public class CreateCustomerRequestValidator : AbstractValidator<CreateCustomerRequestDto>
    {
        public CreateCustomerRequestValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty()
                .WithMessage("Họ tên không được để trống")
                .Length(2, 100)
                .WithMessage("Họ tên phải từ 2 đến 100 ký tự")
                .Matches(@"^[\p{L}\s\.]+$")
                .WithMessage("Họ tên chỉ được chứa chữ cái, khoảng trắng và dấu chấm")
                .Must(name => IsValidVietnameseName(name))
                .WithMessage("Họ tên phải có ít nhất 2 từ (họ và tên)");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty()
                .WithMessage("Số điện thoại không được để trống")
                .Must(phone => IsValidVietnamesePhoneNumber(phone))
                .WithMessage("Số điện thoại không hợp lệ. Định dạng: 0xxx-xxx-xxx hoặc +84-xxx-xxx-xxx");

            RuleFor(x => x.Email)
                .EmailAddress()
                .WithMessage("Định dạng email không hợp lệ")
                .MaximumLength(100)
                .WithMessage("Email không được vượt quá 100 ký tự")
                .When(x => !string.IsNullOrWhiteSpace(x.Email));

            RuleFor(x => x.Address)
                .MaximumLength(500)
                .WithMessage("Địa chỉ không được vượt quá 500 ký tự");

            RuleFor(x => x.DateOfBirth)
                .Must(BeAValidAge)
                .WithMessage("Tuổi phải từ 16 đến 120")
                .When(x => x.DateOfBirth.HasValue);

            RuleFor(x => x.Gender)
                .Must(gender => string.IsNullOrWhiteSpace(gender) || IsValidGender(gender))
                .WithMessage("Giới tính chỉ được là: Nam, Nữ, Khác");

            RuleFor(x => x.IdentityNumber)
                .Must(id => string.IsNullOrWhiteSpace(id) || IsValidVietnameseId(id))
                .WithMessage("Số CMND/CCCD không hợp lệ")
                .When(x => !string.IsNullOrWhiteSpace(x.IdentityNumber));

            RuleFor(x => x.TypeId)
                .GreaterThan(0)
                .WithMessage("Loại khách hàng không hợp lệ")
                .When(x => x.TypeId.HasValue);

            RuleFor(x => x.PreferredLanguage)
                .Must(lang => IsValidLanguageCode(lang))
                .WithMessage("Ngôn ngữ ưu tiên không hợp lệ. Chỉ hỗ trợ: vi-VN, en-US");

            RuleFor(x => x.Notes)
                .MaximumLength(1000)
                .WithMessage("Ghi chú không được vượt quá 1000 ký tự");
        }

        private static bool IsValidVietnameseName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;

            var words = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return words.Length >= 2;
        }

        private static bool IsValidVietnamesePhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber)) return false;

            var cleanPhone = phoneNumber.Replace(" ", "").Replace("-", "");

            var patterns = new[]
            {
                @"^(0)(3|5|7|8|9)[0-9]{8}$",
                @"^(\+84)(3|5|7|8|9)[0-9]{8}$",
                @"^(84)(3|5|7|8|9)[0-9]{8}$"
            };

            return patterns.Any(pattern => Regex.IsMatch(cleanPhone, pattern));
        }

        private static bool BeAValidAge(DateOnly? dateOfBirth)
        {
            if (!dateOfBirth.HasValue) return true;

            var today = DateOnly.FromDateTime(DateTime.Today);
            var age = today.Year - dateOfBirth.Value.Year;

            if (today.DayOfYear < dateOfBirth.Value.DayOfYear)
                age--;

            return age >= 16 && age <= 120;
        }

        private static bool IsValidGender(string gender)
        {
            var validGenders = new[] { "Nam", "Nữ", "Khác", "Male", "Female", "Other" };
            return validGenders.Contains(gender, StringComparer.OrdinalIgnoreCase);
        }

        private static bool IsValidVietnameseId(string identityNumber)
        {
            if (string.IsNullOrWhiteSpace(identityNumber)) return true;

            var cleanId = identityNumber.Replace(" ", "");

            return Regex.IsMatch(cleanId, @"^[0-9]{9}$") ||
                   Regex.IsMatch(cleanId, @"^[0-9]{12}$");
        }

        private static bool IsValidLanguageCode(string languageCode)
        {
            var validLanguages = new[] { "vi-VN", "en-US" };
            return validLanguages.Contains(languageCode, StringComparer.OrdinalIgnoreCase);
        }
    }
}
