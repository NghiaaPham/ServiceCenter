using EVServiceCenter.Core.Domains.InventoryManagement.DTOs.Requests;
using FluentValidation;

namespace EVServiceCenter.Core.Domains.InventoryManagement.Validators;

public class StockAdjustmentValidator : AbstractValidator<StockAdjustmentRequestDto>
{
    public StockAdjustmentValidator()
    {
        RuleFor(x => x.PartId)
            .GreaterThan(0)
            .WithMessage("PartId must be greater than 0");

        RuleFor(x => x.ServiceCenterId)
            .GreaterThan(0)
            .WithMessage("ServiceCenterId must be greater than 0");

        RuleFor(x => x.TransactionType)
            .NotEmpty()
            .WithMessage("TransactionType is required")
            .Must(type => new[] { "IN", "OUT", "ADJUST", "TRANSFER" }
                .Contains(type, StringComparer.OrdinalIgnoreCase))
            .WithMessage("TransactionType must be one of: IN, OUT, ADJUST, TRANSFER");

        RuleFor(x => x.Quantity)
            .NotEqual(0)
            .WithMessage("Quantity cannot be zero")
            .GreaterThan(-100000)
            .WithMessage("Quantity cannot be less than -100000")
            .LessThan(100000)
            .WithMessage("Quantity cannot exceed 100000");

        // Business Rule: IN transactions should have UnitCost
        RuleFor(x => x.UnitCost)
            .GreaterThan(0)
            .WithMessage("UnitCost must be greater than 0 for IN transactions")
            .When(x => x.TransactionType.Equals("IN", StringComparison.OrdinalIgnoreCase));

        // Business Rule: IN transactions should have Supplier
        RuleFor(x => x.SupplierId)
            .GreaterThan(0)
            .WithMessage("SupplierId is recommended for IN transactions")
            .When(x => x.TransactionType.Equals("IN", StringComparison.OrdinalIgnoreCase));

        RuleFor(x => x.InvoiceNumber)
            .MaximumLength(50)
            .WithMessage("InvoiceNumber cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.InvoiceNumber));

        RuleFor(x => x.BatchNumber)
            .MaximumLength(50)
            .WithMessage("BatchNumber cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.BatchNumber));

        RuleFor(x => x.Location)
            .MaximumLength(100)
            .WithMessage("Location cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Location));

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .WithMessage("Notes cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));

        // Business Rule: ExpiryDate must be in the future
        RuleFor(x => x.ExpiryDate)
            .GreaterThan(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("ExpiryDate must be in the future")
            .When(x => x.ExpiryDate.HasValue);
    }
}
