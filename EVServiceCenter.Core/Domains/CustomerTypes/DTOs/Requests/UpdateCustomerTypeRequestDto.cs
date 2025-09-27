using System.ComponentModel.DataAnnotations;

namespace EVServiceCenter.Core.Domains.CustomerTypes.DTOs.Requests
{
    public class UpdateCustomerTypeRequestDto : CreateCustomerTypeRequestDto
    {
        public int TypeId { get; set; }
    }
}
