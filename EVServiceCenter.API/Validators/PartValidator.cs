using EVServiceCenter.Core.Entities;

namespace EVServiceCenter.API.Validators;

public sealed class PartValidator : AbstractValidator<Part>
{
    public PartValidator()
    {
        RuleSet(ValidationRules.RuleSetCreate, () =>
        {
            RuleFor(x => x.PartCode).NotEmpty().WithMessage(ValidationRules.Messages.Required).MaximumLength(50).WithMessage(ValidationRules.Messages.MaxLength);
            RuleFor(x => x.PartName).NotEmpty().WithMessage(ValidationRules.Messages.Required).MaximumLength(200).WithMessage(ValidationRules.Messages.MaxLength);
            RuleFor(x => x.CategoryId).GreaterThan(0).WithMessage(ValidationRules.Messages.MustBePositive);
        });

        RuleSet(ValidationRules.RuleSetUpdate, () =>
        {
            RuleFor(x => x.PartCode).NotEmpty().WithMessage(ValidationRules.Messages.Required).MaximumLength(50).WithMessage(ValidationRules.Messages.MaxLength);
        });

        RuleFor(x => x.BarCode).MaximumLength(50).WithMessage(ValidationRules.Messages.MaxLength);
        RuleFor(x => x.Unit).MaximumLength(20).WithMessage(ValidationRules.Messages.MaxLength);
        RuleFor(x => x.CostPrice).GreaterThanOrEqualTo(0).When(x => x.CostPrice.HasValue).WithMessage(ValidationRules.Messages.MustBeNonNegative);
        RuleFor(x => x.SellingPrice).GreaterThanOrEqualTo(0).When(x => x.SellingPrice.HasValue).WithMessage(ValidationRules.Messages.MustBeNonNegative);
        RuleFor(x => x.MinStock).GreaterThanOrEqualTo(0).When(x => x.MinStock.HasValue).WithMessage(ValidationRules.Messages.MustBeNonNegative);
        RuleFor(x => x.CurrentStock).GreaterThanOrEqualTo(0).When(x => x.CurrentStock.HasValue).WithMessage(ValidationRules.Messages.MustBeNonNegative);
        RuleFor(x => x.ReorderLevel).GreaterThanOrEqualTo(0).When(x => x.ReorderLevel.HasValue).WithMessage(ValidationRules.Messages.MustBeNonNegative);
        RuleFor(x => x.MaxStock).GreaterThanOrEqualTo(0).When(x => x.MaxStock.HasValue).WithMessage(ValidationRules.Messages.MustBeNonNegative);
        RuleFor(x => x.Location).MaximumLength(100).WithMessage(ValidationRules.Messages.MaxLength);
        RuleFor(x => x.Weight).GreaterThanOrEqualTo(0).When(x => x.Weight.HasValue).WithMessage(ValidationRules.Messages.MustBeNonNegative);
        RuleFor(x => x.Dimensions).MaximumLength(100).WithMessage(ValidationRules.Messages.MaxLength);
        RuleFor(x => x.WarrantyPeriod).GreaterThanOrEqualTo(0).When(x => x.WarrantyPeriod.HasValue).WithMessage(ValidationRules.Messages.MustBeNonNegative);
        RuleFor(x => x.PartCondition).MaximumLength(20).WithMessage(ValidationRules.Messages.MaxLength);
        RuleFor(x => x.ImageUrl).MaximumLength(500).WithMessage(ValidationRules.Messages.MaxLength);
        RuleFor(x => x.AlternativePartIds).MaximumLength(200).WithMessage(ValidationRules.Messages.MaxLength);
    }
}

