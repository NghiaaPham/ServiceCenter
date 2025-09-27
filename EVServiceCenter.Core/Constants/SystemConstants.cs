namespace EVServiceCenter.Core.Constants
{
    public static class SystemConstants
    {
        public const int DEFAULT_PAGE_SIZE = 20;
        public const int MAX_PAGE_SIZE = 100;

        public const int PASSWORD_MIN_LENGTH = 6;
        public const int PASSWORD_MAX_LENGTH = 50;

        public const int USERNAME_MIN_LENGTH = 3;
        public const int USERNAME_MAX_LENGTH = 50;

        public const decimal MAX_DISCOUNT_PERCENT = 100m;
        public const decimal DEFAULT_TAX_RATE = 0.1m; 

        public const int DEFAULT_WARRANTY_PERIOD_DAYS = 90;
        public const int MAX_APPOINTMENT_DURATION_HOURS = 12;

        public const string DEFAULT_CURRENCY = "VND";
        public const string DATE_FORMAT = "dd/MM/yyyy";
        public const string DATETIME_FORMAT = "dd/MM/yyyy HH:mm";
    }
}
