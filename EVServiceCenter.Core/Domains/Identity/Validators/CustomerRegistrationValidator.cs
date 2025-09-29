using EVServiceCenter.Core.Domains.Identity.DTOs.Requests;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EVServiceCenter.Core.Domains.Identity.Validators
{
    public class CustomerRegistrationValidator : AbstractValidator<CustomerRegistrationDto>
    {
        public CustomerRegistrationValidator()
        {
            // Username validation
            RuleFor(x => x.Username)
                .NotEmpty()
                .WithMessage("Tên đăng nhập không được để trống")
                .Length(3, 50)
                .WithMessage("Tên đăng nhập phải từ 3 đến 50 ký tự")
                .Matches("^[a-zA-Z0-9_]+$")
                .WithMessage("Tên đăng nhập chỉ được chứa chữ cái, số và dấu gạch dưới")
                .Must(username => !username.StartsWith("_") && !username.EndsWith("_"))
                .WithMessage("Tên đăng nhập không được bắt đầu hoặc kết thúc bằng dấu gạch dưới");

            // Password validation
            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("Mật khẩu không được để trống")
                .Length(6, 50)
                .WithMessage("Mật khẩu phải từ 6 đến 50 ký tự")
                .Must(password => password.Any(char.IsDigit))
                .WithMessage("Mật khẩu phải chứa ít nhất 1 chữ số")
                .Must(password => password.Any(char.IsLetter))
                .WithMessage("Mật khẩu phải chứa ít nhất 1 chữ cái");

            // Full name validation - simplified
            RuleFor(x => x.FullName)
                .NotEmpty()
                .WithMessage("Họ tên không được để trống")
                .Length(2, 100)
                .WithMessage("Họ tên phải từ 2 đến 100 ký tự")
                .Matches(@"^[\p{L}\s\.]+$")
                .WithMessage("Họ tên chỉ được chứa chữ cái, khoảng trắng và dấu chấm");

            // Email validation
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email không được để trống")
                .EmailAddress()
                .WithMessage("Định dạng email không hợp lệ")
                .MaximumLength(100)
                .WithMessage("Email không được vượt quá 100 ký tự");

            // Phone validation
            RuleFor(x => x.PhoneNumber)
                .NotEmpty()
                .WithMessage("Số điện thoại không được để trống")
                .Must(phone => IsValidVietnamesePhoneNumber(phone))
                .WithMessage("Số điện thoại không hợp lệ. Định dạng: 0xxx-xxx-xxx");

            // Terms acceptance - MANDATORY
            RuleFor(x => x.AcceptTerms)
                .Equal(true)
                .WithMessage("Bạn phải đồng ý với điều khoản sử dụng để đăng ký");
        }

        private static bool IsValidVietnamesePhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber)) return false;

            var cleanPhone = phoneNumber.Replace(" ", "").Replace("-", "").Replace(".", "");
            var patterns = new[]
            {
                @"^(0)(3|5|7|8|9)[0-9]{8}$",          // 0xxx-xxx-xxx
                @"^(\+84)(3|5|7|8|9)[0-9]{8}$",      // +84xxx-xxx-xxx
                @"^(84)(3|5|7|8|9)[0-9]{8}$"         // 84xxx-xxx-xxx
            };

            return patterns.Any(pattern => Regex.IsMatch(cleanPhone, pattern));
        }
    }
}
