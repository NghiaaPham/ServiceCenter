using EVServiceCenter.Core.Domains.CustomerTypes.DTOs.Requests;
using FluentValidation;

public class CreateCustomerTypeRequestValidator : AbstractValidator<CreateCustomerTypeRequestDto>
{
    public CreateCustomerTypeRequestValidator()
    {
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
            .PrecisionScale(5, 2, ignoreTrailingZeros: false)
            .WithMessage("Phần trăm giảm giá chỉ được có tối đa 2 chữ số thập phân");

        RuleFor(x => x.Description)
            .MaximumLength(200)
            .WithMessage("Mô tả không được vượt quá 200 ký tự")
            .Must(description => string.IsNullOrWhiteSpace(description) || !ContainsBadWords(description))
            .WithMessage("Mô tả chứa nội dung không phù hợp");

        RuleFor(x => x.IsActive)
            .NotNull()
            .WithMessage("Trạng thái hoạt động là bắt buộc");

        RuleFor(x => x)
            .Must(dto => ValidateBusinessRules(dto))
            .WithMessage("Dữ liệu không thỏa mãn quy tắc nghiệp vụ")
            .OverridePropertyName("BusinessRules");
    }

    private static bool ContainsBadWords(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        var badWords = new[] { "spam", "scam", "fake" };
        var lowerText = text.ToLowerInvariant();
        return badWords.Any(badWord => lowerText.Contains(badWord));
    }

    private static bool ValidateBusinessRules(CreateCustomerTypeRequestDto dto)
    {
        // Synchronous business rules
        if (dto.TypeName.Contains("VIP", StringComparison.OrdinalIgnoreCase) && dto.DiscountPercent < 5)
            return false;

        if (dto.TypeName.Contains("Corporate", StringComparison.OrdinalIgnoreCase) && dto.DiscountPercent == 0)
            return false;

        return true;
    }
}