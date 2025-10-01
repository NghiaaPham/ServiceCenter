using EVServiceCenter.Core.Domains.AppointmentManagement.Entities;
using EVServiceCenter.Core.Domains.Customers.Entities;
using EVServiceCenter.Core.Domains.CustomerVehicles.Entities;
using EVServiceCenter.Core.Entities;

namespace EVServiceCenter.API.Validators;

public sealed class InvoiceValidator : AbstractValidator<Invoice>
{
    public InvoiceValidator()
    {
        RuleSet(ValidationRules.RuleSetCreate, () =>
        {
            RuleFor(x => x.InvoiceCode).NotEmpty().WithMessage(ValidationRules.Messages.Required).MaximumLength(20).WithMessage(ValidationRules.Messages.MaxLength);
            RuleFor(x => x.WorkOrderId).GreaterThan(0).WithMessage(ValidationRules.Messages.MustBePositive);
            RuleFor(x => x.CustomerId).GreaterThan(0).WithMessage(ValidationRules.Messages.MustBePositive);
        });

        RuleFor(x => x.InvoiceDate).LessThanOrEqualTo(DateTime.UtcNow).When(x => x.InvoiceDate.HasValue).WithMessage(ValidationRules.Messages.NotInFuture);
        RuleFor(x => x.DueDate).GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow.Date)).When(x => x.DueDate.HasValue);
        RuleFor(x => x.ServiceSubTotal).GreaterThanOrEqualTo(0).When(x => x.ServiceSubTotal.HasValue);
        RuleFor(x => x.PartsSubTotal).GreaterThanOrEqualTo(0).When(x => x.PartsSubTotal.HasValue);
        RuleFor(x => x.SubTotal).GreaterThanOrEqualTo(0).When(x => x.SubTotal.HasValue);
        RuleFor(x => x.TotalDiscount).GreaterThanOrEqualTo(0).When(x => x.TotalDiscount.HasValue);
        RuleFor(x => x.TotalTax).GreaterThanOrEqualTo(0).When(x => x.TotalTax.HasValue);
        RuleFor(x => x.GrandTotal).GreaterThanOrEqualTo(0).When(x => x.GrandTotal.HasValue);
        RuleFor(x => x.PaidAmount).GreaterThanOrEqualTo(0).When(x => x.PaidAmount.HasValue);
        RuleFor(x => x.OutstandingAmount).GreaterThanOrEqualTo(0).When(x => x.OutstandingAmount.HasValue);
        RuleFor(x => x.Status).MaximumLength(20).When(x => x.Status != null);
        RuleFor(x => x.PaymentTerms).MaximumLength(100).When(x => x.PaymentTerms != null);
        RuleFor(x => x.Notes).MaximumLength(1000).When(x => x.Notes != null);
        RuleFor(x => x.SentMethod).MaximumLength(20).When(x => x.SentMethod != null);
    }
}

public sealed class CustomerValidator : AbstractValidator<Customer>
{
    public CustomerValidator()
    {
        RuleSet(ValidationRules.RuleSetCreate, () =>
        {
            RuleFor(x => x.CustomerCode).NotEmpty().WithMessage(ValidationRules.Messages.Required).MaximumLength(20);
            RuleFor(x => x.FullName).NotEmpty().WithMessage(ValidationRules.Messages.Required).MaximumLength(100);
            RuleFor(x => x.PhoneNumber).NotEmpty().WithMessage(ValidationRules.Messages.Required).MaximumLength(20);
        });

        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email)).WithMessage(ValidationRules.Messages.InvalidEmail).MaximumLength(100);
        RuleFor(x => x.Address).MaximumLength(500).When(x => x.Address != null);
        RuleFor(x => x.Gender).MaximumLength(10).When(x => x.Gender != null);
        RuleFor(x => x.PreferredLanguage).MaximumLength(10).When(x => x.PreferredLanguage != null);
        RuleFor(x => x.LoyaltyPoints).GreaterThanOrEqualTo(0).When(x => x.LoyaltyPoints.HasValue);
        RuleFor(x => x.TotalSpent).GreaterThanOrEqualTo(0).When(x => x.TotalSpent.HasValue);
        RuleFor(x => x.Notes).MaximumLength(1000).When(x => x.Notes != null);
        RuleFor(x => x.CreatedBy).GreaterThan(0).When(x => x.CreatedBy.HasValue);
    }
}

public sealed class CustomerVehicleValidator : AbstractValidator<CustomerVehicle>
{
    public CustomerVehicleValidator()
    {
        RuleSet(ValidationRules.RuleSetCreate, () =>
        {
            RuleFor(x => x.CustomerId).GreaterThan(0).WithMessage(ValidationRules.Messages.MustBePositive);
            RuleFor(x => x.ModelId).GreaterThan(0).WithMessage(ValidationRules.Messages.MustBePositive);
            RuleFor(x => x.LicensePlate).NotEmpty().WithMessage(ValidationRules.Messages.Required).MaximumLength(20);
        });

        RuleFor(x => x.Vin).MaximumLength(50).When(x => x.Vin != null);
        RuleFor(x => x.Color).MaximumLength(50).When(x => x.Color != null);
        RuleFor(x => x.Mileage).GreaterThanOrEqualTo(0).When(x => x.Mileage.HasValue);
        RuleFor(x => x.BatteryHealthPercent).InclusiveBetween(0, 100).When(x => x.BatteryHealthPercent.HasValue);
        RuleFor(x => x.VehicleCondition).MaximumLength(50).When(x => x.VehicleCondition != null);
        RuleFor(x => x.InsuranceNumber).MaximumLength(50).When(x => x.InsuranceNumber != null);
    }
}

public sealed class PartInventoryValidator : AbstractValidator<PartInventory>
{
    public PartInventoryValidator()
    {
        RuleSet(ValidationRules.RuleSetCreate, () =>
        {
            RuleFor(x => x.PartId).GreaterThan(0);
            RuleFor(x => x.CenterId).GreaterThan(0);
        });

        RuleFor(x => x.CurrentStock).GreaterThanOrEqualTo(0).When(x => x.CurrentStock.HasValue);
        RuleFor(x => x.ReservedStock).GreaterThanOrEqualTo(0).When(x => x.ReservedStock.HasValue);
        RuleFor(x => x.AvailableStock).GreaterThanOrEqualTo(0).When(x => x.AvailableStock.HasValue);
        RuleFor(x => x.Location).MaximumLength(100).When(x => x.Location != null);
    }
}

public sealed class PaymentMethodValidator : AbstractValidator<PaymentMethod>
{
    public PaymentMethodValidator()
    {
        RuleSet(ValidationRules.RuleSetCreate, () =>
        {
            RuleFor(x => x.MethodCode).NotEmpty().MaximumLength(20);
            RuleFor(x => x.MethodName).NotEmpty().MaximumLength(50);
            RuleFor(x => x.PaymentType).NotEmpty().MaximumLength(20);
        });

        RuleFor(x => x.GatewayProvider).MaximumLength(100).When(x => x.GatewayProvider != null);
        RuleFor(x => x.ProcessingFee).GreaterThanOrEqualTo(0).When(x => x.ProcessingFee.HasValue);
        RuleFor(x => x.FixedFee).GreaterThanOrEqualTo(0).When(x => x.FixedFee.HasValue);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0).When(x => x.DisplayOrder.HasValue);
        RuleFor(x => x.IconUrl).MaximumLength(500).When(x => x.IconUrl != null);
    }
}

public sealed class ServiceRatingValidator : AbstractValidator<ServiceRating>
{
    public ServiceRatingValidator()
    {
        RuleSet(ValidationRules.RuleSetCreate, () =>
        {
            RuleFor(x => x.WorkOrderId).GreaterThan(0);
            RuleFor(x => x.CustomerId).GreaterThan(0);
        });

        RuleFor(x => x.OverallRating).InclusiveBetween(1, 5).When(x => x.OverallRating.HasValue);
        RuleFor(x => x.ServiceQuality).InclusiveBetween(1, 5).When(x => x.ServiceQuality.HasValue);
        RuleFor(x => x.StaffProfessionalism).InclusiveBetween(1, 5).When(x => x.StaffProfessionalism.HasValue);
        RuleFor(x => x.FacilityQuality).InclusiveBetween(1, 5).When(x => x.FacilityQuality.HasValue);
        RuleFor(x => x.WaitingTime).InclusiveBetween(1, 5).When(x => x.WaitingTime.HasValue);
        RuleFor(x => x.PriceValue).InclusiveBetween(1, 5).When(x => x.PriceValue.HasValue);
        RuleFor(x => x.CommunicationQuality).InclusiveBetween(1, 5).When(x => x.CommunicationQuality.HasValue);
        RuleFor(x => x.PositiveFeedback).MaximumLength(1000).When(x => x.PositiveFeedback != null);
        RuleFor(x => x.NegativeFeedback).MaximumLength(1000).When(x => x.NegativeFeedback != null);
        RuleFor(x => x.Suggestions).MaximumLength(1000).When(x => x.Suggestions != null);
        RuleFor(x => x.RatingDate).LessThanOrEqualTo(DateTime.UtcNow).When(x => x.RatingDate.HasValue);
        RuleFor(x => x.ResponseMethod).MaximumLength(20).When(x => x.ResponseMethod != null);
        RuleFor(x => x.RespondedBy).GreaterThan(0).When(x => x.RespondedBy.HasValue);
    }
}

public sealed class WarrantyValidator : AbstractValidator<Warranty>
{
    public WarrantyValidator()
    {
        RuleSet(ValidationRules.RuleSetCreate, () =>
        {
            RuleFor(x => x.WarrantyCode).NotEmpty().MaximumLength(20);
            RuleFor(x => x.WorkOrderId).GreaterThan(0);
            RuleFor(x => x.WarrantyTypeId).GreaterThan(0);
            RuleFor(x => x.StartDate).NotEmpty();
            RuleFor(x => x.EndDate).NotEmpty();
        });

        RuleFor(x => x.Mileage).GreaterThanOrEqualTo(0).When(x => x.Mileage.HasValue);
        RuleFor(x => x.MileageLimit).GreaterThanOrEqualTo(0).When(x => x.MileageLimit.HasValue);
        RuleFor(x => x.Status).MaximumLength(20).When(x => x.Status != null);
        RuleFor(x => x.VoidReason).MaximumLength(500).When(x => x.VoidReason != null);
    }
}

public sealed class TimeSlotValidator : AbstractValidator<TimeSlot>
{
    public TimeSlotValidator()
    {
        RuleSet(ValidationRules.RuleSetCreate, () =>
        {
            RuleFor(x => x.CenterId).GreaterThan(0);
            RuleFor(x => x.SlotDuration).GreaterThan(0);
        });

        RuleFor(x => x.MaxBookings).GreaterThanOrEqualTo(0).When(x => x.MaxBookings.HasValue);
        RuleFor(x => x.CurrentBookings).GreaterThanOrEqualTo(0).When(x => x.CurrentBookings.HasValue);
        RuleFor(x => x.SlotType).MaximumLength(20).When(x => x.SlotType != null);
    }
}


