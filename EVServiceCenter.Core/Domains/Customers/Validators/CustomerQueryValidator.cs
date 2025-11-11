using EVServiceCenter.Core.Domains.Customers.DTOs.Requests;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVServiceCenter.Core.Domains.Customers.Validators
{
    public class CustomerQueryValidator : AbstractValidator<CustomerQueryDto>
    {
        public CustomerQueryValidator()
        {
            RuleFor(x => x.Page)
                .GreaterThan(0)
                .WithMessage("Số trang phải lớn hơn 0")
                .LessThanOrEqualTo(1000)
                .WithMessage("Số trang không được vượt quá 1000");

            RuleFor(x => x.PageSize)
                .GreaterThan(0)
                .WithMessage("Kích thước trang phải lớn hơn 0")
                .LessThanOrEqualTo(100)
                .WithMessage("Kích thước trang không được vượt quá 100");

            RuleFor(x => x.SearchTerm)
                .MaximumLength(100)
                .WithMessage("Từ khóa tìm kiếm không được vượt quá 100 ký tự")
                .When(x => !string.IsNullOrEmpty(x.SearchTerm));

            RuleFor(x => x.TypeId)
                .GreaterThan(0)
                .WithMessage("ID loại khách hàng phải lớn hơn 0")
                .When(x => x.TypeId.HasValue);

            RuleFor(x => x.Gender)
                .Must(gender => string.IsNullOrEmpty(gender) || IsValidGender(gender))
                .WithMessage("Giới tính chỉ được là: Nam, Nữ, Khác")
                .When(x => !string.IsNullOrEmpty(x.Gender));

            RuleFor(x => x.DateOfBirthFrom)
                .LessThan(x => x.DateOfBirthTo)
                .WithMessage("Ngày sinh từ phải nhỏ hơn ngày sinh đến")
                .When(x => x.DateOfBirthFrom.HasValue && x.DateOfBirthTo.HasValue);

            RuleFor(x => x.TotalSpentFrom)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Tổng chi tiêu từ phải lớn hơn hoặc bằng 0")
                .LessThan(x => x.TotalSpentTo)
                .WithMessage("Tổng chi tiêu từ phải nhỏ hơn tổng chi tiêu đến")
                .When(x => x.TotalSpentFrom.HasValue && x.TotalSpentTo.HasValue);

            RuleFor(x => x.LoyaltyPointsFrom)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Điểm thưởng từ phải lớn hơn hoặc bằng 0")
                .LessThan(x => x.LoyaltyPointsTo)
                .WithMessage("Điểm thưởng từ phải nhỏ hơn điểm thưởng đến")
                .When(x => x.LoyaltyPointsFrom.HasValue && x.LoyaltyPointsTo.HasValue);

            RuleFor(x => x.SortBy)
                .Must(sortBy => IsValidSortField(sortBy))
                .WithMessage("Trường sắp xếp không hợp lệ. Các trường được phép: FullName, CustomerCode, CreatedDate, LastVisitDate, TotalSpent, LoyaltyPoints");

            // Performance rules
            RuleFor(x => x)
                .Must(query => !query.IncludeStats || query.PageSize <= 50)
                .WithMessage("Khi bao gồm thống kê, kích thước trang không được vượt quá 50")
                .When(x => x.IncludeStats);
        }

        private static bool IsValidGender(string gender)
        {
            var validGenders = new[] { "Nam", "Nữ", "Khác", "Male", "Female", "Other" };
            return validGenders.Contains(gender, StringComparer.OrdinalIgnoreCase);
        }

        private static bool IsValidSortField(string sortBy)
        {
            var validSortFields = new[]
            {
                "FullName", "CustomerCode", "CreatedDate",
                "LastVisitDate", "TotalSpent", "LoyaltyPoints",
                "PhoneNumber", "Email"
            };
            return validSortFields.Contains(sortBy, StringComparer.OrdinalIgnoreCase);
        }
    }
}
