namespace EVServiceCenter.Core.Domains.Customers.DTOs.Requests
{
    public class UpdateCustomerRequestDto : CreateCustomerRequestDto
    {
        public int CustomerId { get; set; }
    }
}
