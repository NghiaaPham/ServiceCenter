using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVServiceCenter.Core.Domains.CarBrands.DTOs.Requests
{
    public class BrandStatistics
    {
        public int TotalModels { get; set; }
        public int ActiveModels { get; set; }
        public int TotalVehicles { get; set; }
    }
}
