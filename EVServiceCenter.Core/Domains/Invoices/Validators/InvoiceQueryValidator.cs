using EVServiceCenter.Core.Domains.Invoices.DTOs.Requests;
using FluentValidation;

namespace EVServiceCenter.Core.Domains.Invoices.Validators;

public class InvoiceQueryValidator : AbstractValidator<InvoiceQueryDto>
{
    public InvoiceQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0")
            .LessThanOrEqualTo(1000)
            .WithMessage("Page cannot exceed 1000");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("PageSize must be greater than 0")
            .LessThanOrEqualTo(100)
            .WithMessage("PageSize cannot exceed 100");

        RuleFor(x => x.SearchTerm)
            .MaximumLength(100)
            .WithMessage("SearchTerm cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.SearchTerm));

        RuleFor(x => x.CustomerId)
            .GreaterThan(0)
            .WithMessage("CustomerId must be greater than 0")
            .When(x => x.CustomerId.HasValue);

        RuleFor(x => x.WorkOrderId)
            .GreaterThan(0)
            .WithMessage("WorkOrderId must be greater than 0")
            .When(x => x.WorkOrderId.HasValue);

        RuleFor(x => x.Status)
            .Must(status => string.IsNullOrEmpty(status) || new[] {
                "Draft", "Unpaid", "PartiallyPaid", "Paid", "Cancelled", "Refunded"
            }.Contains(status, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Status must be one of: Draft, Unpaid, PartiallyPaid, Paid, Cancelled, Refunded")
            .When(x => !string.IsNullOrEmpty(x.Status));

        RuleFor(x => x.InvoiceDateFrom)
            .LessThanOrEqualTo(x => x.InvoiceDateTo)
            .WithMessage("InvoiceDateFrom must be before or equal to InvoiceDateTo")
            .When(x => x.InvoiceDateFrom.HasValue && x.InvoiceDateTo.HasValue);

        RuleFor(x => x.DueDateFrom)
            .LessThanOrEqualTo(x => x.DueDateTo)
            .WithMessage("DueDateFrom must be before or equal to DueDateTo")
            .When(x => x.DueDateFrom.HasValue && x.DueDateTo.HasValue);

        RuleFor(x => x.SortBy)
            .Must(sortBy => new[] { "invoicedate", "duedate", "grandtotal", "status" }
                .Contains(sortBy.ToLowerInvariant()))
            .WithMessage("SortBy must be one of: invoiceDate, dueDate, grandTotal, status");

        RuleFor(x => x.SortDirection)
            .Must(dir => dir.Equals("asc", StringComparison.OrdinalIgnoreCase) ||
                        dir.Equals("desc", StringComparison.OrdinalIgnoreCase))
            .WithMessage("SortDirection must be 'asc' or 'desc'");
    }
}
