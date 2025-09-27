using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVServiceCenter.Core.Constants
{
    public static class CacheConstants
    {
        public const int SHORT_CACHE_MINUTES = 15;
        public const int MEDIUM_CACHE_MINUTES = 60;
        public const int LONG_CACHE_HOURS = 24;

        public const string USER_CACHE_KEY = "user_{0}";
        public const string CUSTOMER_CACHE_KEY = "customer_{0}";
        public const string VEHICLE_CACHE_KEY = "vehicle_{0}";
        public const string SERVICE_CACHE_KEY = "service_{0}";
    }
}
