using System.ComponentModel.DataAnnotations;

namespace EVServiceCenter.Core.Domains.CustomerTypes.DTOs.Requests
{
    public class CreateCustomerTypeRequestDto
    {
        public string TypeName { get; set; } = string.Empty;
        public decimal DiscountPercent { get; set; } = 0;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
