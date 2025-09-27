namespace EVServiceCenter.Core.Constants
{
    public static class ValidationConstants
    {
        // Phone number patterns
        public const string VIETNAM_PHONE_PATTERN = @"^(0[3-9][0-9]{8}|\+84[3-9][0-9]{8})$";

        // Email pattern
        public const string EMAIL_PATTERN = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

        // License plate pattern (Vietnamese)
        public const string LICENSE_PLATE_PATTERN = @"^[0-9]{2}[A-Z]{1,2}-[0-9]{3,5}$";

        // VIN pattern
        public const string VIN_PATTERN = @"^[A-HJ-NPR-Z0-9]{17}$";

        // Identity number patterns
        public const string IDENTITY_9_DIGITS = @"^[0-9]{9}$";
        public const string IDENTITY_12_DIGITS = @"^[0-9]{12}$";
    }
}
