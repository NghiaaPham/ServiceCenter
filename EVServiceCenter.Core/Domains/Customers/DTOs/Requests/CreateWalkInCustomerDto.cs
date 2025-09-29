using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVServiceCenter.Core.Domains.Customers.DTOs.Requests
{
    public class CreateWalkInCustomerDto
    {
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Email { get; set; }  // Optional
        public string? Address { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public int? TypeId { get; set; }
        public string? Notes { get; set; }
    }
}
