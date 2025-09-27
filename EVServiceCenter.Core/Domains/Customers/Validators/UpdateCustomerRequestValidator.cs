using EVServiceCenter.Core.Domains.Customers.DTOs.Requests;
using EVServiceCenter.Core.Domains.Customers.Interfaces;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVServiceCenter.Core.Domains.Customers.Validators
{
    public class UpdateCustomerRequestValidator : AbstractValidator<UpdateCustomerRequestDto>
    {
        public UpdateCustomerRequestValidator()
        {
            RuleFor(x => x.CustomerId)
                .GreaterThan(0)
                .WithMessage("ID khách hàng phải lớn hơn 0");

            // Include all validation rules from CreateCustomerRequestValidator
            Include(new CreateCustomerRequestValidator());
        }
    }
}
