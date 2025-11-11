using EVServiceCenter.Core.Domains.ServiceRatings.DTOs.Requests;
using FluentValidation;

namespace EVServiceCenter.Core.Domains.ServiceRatings.Validators;

/// <summary>
/// Validator for CreateServiceRatingRequestDto
/// Validates all rating fields and feedback content
/// </summary>
public class CreateServiceRatingValidator : AbstractValidator<CreateServiceRatingRequestDto>
{
    public CreateServiceRatingValidator()
    {
        // WorkOrder ID validation
        RuleFor(x => x.WorkOrderId)
            .GreaterThan(0)
            .WithMessage("WorkOrder ID phải lớn hơn 0");

        // Overall rating validation (required)
        RuleFor(x => x.OverallRating)
            .InclusiveBetween(1, 5)
            .WithMessage("Đánh giá tổng thể phải từ 1 đến 5 sao");

        // Service quality rating (optional, but if provided must be 1-5)
        When(x => x.ServiceQuality.HasValue, () =>
        {
            RuleFor(x => x.ServiceQuality!.Value)
                .InclusiveBetween(1, 5)
                .WithMessage("Đánh giá chất lượng dịch vụ phải từ 1 đến 5 sao");
        });

        // Staff professionalism rating (optional)
        When(x => x.StaffProfessionalism.HasValue, () =>
        {
            RuleFor(x => x.StaffProfessionalism!.Value)
                .InclusiveBetween(1, 5)
                .WithMessage("Đánh giá tính chuyên nghiệp của nhân viên phải từ 1 đến 5 sao");
        });

        // Facility quality rating (optional)
        When(x => x.FacilityQuality.HasValue, () =>
        {
            RuleFor(x => x.FacilityQuality!.Value)
                .InclusiveBetween(1, 5)
                .WithMessage("Đánh giá cơ sở vật chất phải từ 1 đến 5 sao");
        });

        // Waiting time satisfaction (optional)
        When(x => x.WaitingTime.HasValue, () =>
        {
            RuleFor(x => x.WaitingTime!.Value)
                .InclusiveBetween(1, 5)
                .WithMessage("Đánh giá thời gian chờ phải từ 1 đến 5 sao");
        });

        // Price-value ratio (optional)
        When(x => x.PriceValue.HasValue, () =>
        {
            RuleFor(x => x.PriceValue!.Value)
                .InclusiveBetween(1, 5)
                .WithMessage("Đánh giá giá trị - chi phí phải từ 1 đến 5 sao");
        });

        // Communication quality (optional)
        When(x => x.CommunicationQuality.HasValue, () =>
        {
            RuleFor(x => x.CommunicationQuality!.Value)
                .InclusiveBetween(1, 5)
                .WithMessage("Đánh giá chất lượng giao tiếp phải từ 1 đến 5 sao");
        });

        // Positive feedback validation (optional)
        When(x => !string.IsNullOrEmpty(x.PositiveFeedback), () =>
        {
            RuleFor(x => x.PositiveFeedback)
                .MaximumLength(1000)
                .WithMessage("Phản hồi tích cực không được vượt quá 1000 ký tự");
        });

        // Negative feedback validation (optional)
        When(x => !string.IsNullOrEmpty(x.NegativeFeedback), () =>
        {
            RuleFor(x => x.NegativeFeedback)
                .MaximumLength(1000)
                .WithMessage("Phản hồi tiêu cực không được vượt quá 1000 ký tự");
        });

        // Suggestions validation (optional)
        When(x => !string.IsNullOrEmpty(x.Suggestions), () =>
        {
            RuleFor(x => x.Suggestions)
                .MaximumLength(1000)
                .WithMessage("Góp ý không được vượt quá 1000 ký tự");
        });

        // Business rule: If rating is low (1-2 stars), encourage feedback
        When(x => x.OverallRating <= 2, () =>
        {
            RuleFor(x => x.NegativeFeedback)
                .NotEmpty()
                .WithMessage("Với đánh giá thấp (1-2 sao), vui lòng cung cấp phản hồi để chúng tôi cải thiện")
                .When(x => string.IsNullOrEmpty(x.Suggestions));
        });

        // Business rule: If rating is high (4-5 stars), encourage positive feedback
        When(x => x.OverallRating >= 4, () =>
        {
            RuleFor(x => x)
                .Must(x => !string.IsNullOrEmpty(x.PositiveFeedback) || x.WouldRecommend == true)
                .WithMessage("Với đánh giá cao, vui lòng chia sẻ trải nghiệm tích cực hoặc đánh dấu sẽ giới thiệu cho người khác");
        });
    }
}
