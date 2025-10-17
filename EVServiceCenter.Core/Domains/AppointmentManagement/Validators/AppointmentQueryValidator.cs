using EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Query;
using FluentValidation;

namespace EVServiceCenter.Core.Domains.AppointmentManagement.Validators
{
    public class AppointmentQueryValidator : AbstractValidator<AppointmentQueryDto>
    {
        public AppointmentQueryValidator()
        {
            RuleFor(x => x.Page)
                .GreaterThan(0).WithMessage("Số trang phải lớn hơn 0");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100).WithMessage("Kích thước trang phải từ 1 đến 100")
                .LessThanOrEqualTo(50)
                .WithMessage("Để tối ưu performance, kích thước trang không nên vượt quá 50. Sử dụng phân trang để load thêm dữ liệu.");

            // ✅ Performance warning for large page sizes
            RuleFor(x => x.PageSize)
                .Must((dto, pageSize) => pageSize <= 20 || !string.IsNullOrEmpty(dto.SearchTerm))
                .WithMessage("Với query không có filter, khuyến nghị page size <= 20 để tối ưu performance")
                .When(x => !x.CustomerId.HasValue && !x.ServiceCenterId.HasValue && !x.StatusId.HasValue);

            When(x => x.CustomerId.HasValue, () =>
            {
                RuleFor(x => x.CustomerId)
                    .GreaterThan(0).WithMessage("ID khách hàng không hợp lệ");
            });

            When(x => x.ServiceCenterId.HasValue, () =>
            {
                RuleFor(x => x.ServiceCenterId)
                    .GreaterThan(0).WithMessage("ID trung tâm không hợp lệ");
            });

            When(x => x.StatusId.HasValue, () =>
            {
                RuleFor(x => x.StatusId)
                    .InclusiveBetween(1, 8).WithMessage("Trạng thái phải từ 1 đến 8");
            });

            When(x => x.StartDate.HasValue && x.EndDate.HasValue, () =>
            {
                RuleFor(x => x.EndDate)
                    .GreaterThanOrEqualTo(x => x.StartDate!.Value)
                    .WithMessage("Ngày kết thúc phải sau hoặc bằng ngày bắt đầu");
            });

            When(x => !string.IsNullOrEmpty(x.Priority), () =>
            {
                RuleFor(x => x.Priority)
                    .Must(p => new[] { "Normal", "High", "Urgent" }.Contains(p))
                    .WithMessage("Độ ưu tiên phải là Normal, High hoặc Urgent");
            });

            When(x => !string.IsNullOrEmpty(x.Source), () =>
            {
                RuleFor(x => x.Source)
                    .Must(s => new[] { "Online", "Walk-in", "Phone" }.Contains(s))
                    .WithMessage("Nguồn phải là Online, Walk-in hoặc Phone");
            });

            When(x => !string.IsNullOrEmpty(x.SearchTerm), () =>
            {
                RuleFor(x => x.SearchTerm)
                    .MaximumLength(100).WithMessage("Từ khóa tìm kiếm không được vượt quá 100 ký tự");
            });

            RuleFor(x => x.SortOrder)
                .Must(so => new[] { "asc", "desc" }.Contains(so.ToLower()))
                .WithMessage("Thứ tự sắp xếp phải là asc hoặc desc");
        }
    }
}
