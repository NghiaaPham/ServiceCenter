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
    public static class CacheKeys
    {
        public const string SERVICE_CENTER_PREFIX = "ServiceCenter_";
        public const string SERVICE_CENTER_ACTIVE = "ServiceCenter_Active";
        public const string SERVICE_CENTER_ALL_STATS = "ServiceCenter_AllStats";

        public static string GetByIdKey(int centerId, bool includeStats)
            => $"{SERVICE_CENTER_PREFIX}{centerId}_{includeStats}";

        public static string GetStatsKey(int centerId)
            => $"{SERVICE_CENTER_PREFIX}Stats_{centerId}";

        public static string GetProvinceKey(string province)
            => $"{SERVICE_CENTER_PREFIX}Province_{province}";

        public static string GetDistrictKey(string district)
            => $"{SERVICE_CENTER_PREFIX}District_{district}";
    }

    public static class CacheSettings
    {
        public const int DEFAULT_CACHE_DURATION_MINUTES = 5;
        public const int STATS_CACHE_DURATION_MINUTES = 10;
        public const int LIST_CACHE_DURATION_MINUTES = 3;
    }
}
