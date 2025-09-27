using EVServiceCenter.Core.Domains.CustomerTypes.DTOs.Requests;
using FluentValidation;

public class UpdateCustomerTypeRequestValidator : AbstractValidator<UpdateCustomerTypeRequestDto>
{
    public UpdateCustomerTypeRequestValidator()
    {
        RuleFor(x => x.TypeId)
            .GreaterThan(0)
            .WithMessage("ID loại khách hàng phải lớn hơn 0");

        RuleFor(x => x.TypeName)
            .NotEmpty()
            .WithMessage("Tên loại khách hàng không được để trống")
            .Length(1, 50)
            .WithMessage("Tên loại khách hàng phải từ 1 đến 50 ký tự")
            .Matches("^[a-zA-ZÀ-ỹ0-9\\s\\-_]+$")
            .WithMessage("Tên loại khách hàng chỉ được chứa chữ cái, số, khoảng trắng và dấu gạch ngang");

        RuleFor(x => x.DiscountPercent)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Phần trăm giảm giá phải lớn hơn hoặc bằng 0")
            .LessThanOrEqualTo(100)
            .WithMessage("Phần trăm giảm giá phải nhỏ hơn hoặc bằng 100")
            .ScalePrecision(2, 5)
            .WithMessage("Phần trăm giảm giá chỉ được có tối đa 2 chữ số thập phân");

        RuleFor(x => x.Description)
            .MaximumLength(200)
            .WithMessage("Mô tả không được vượt quá 200 ký tự");

        RuleFor(x => x.IsActive)
            .NotNull()
            .WithMessage("Trạng thái hoạt động là bắt buộc");
    }
}