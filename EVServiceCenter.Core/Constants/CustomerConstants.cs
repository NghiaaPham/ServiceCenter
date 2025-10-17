namespace EVServiceCenter.Core.Constants
{
    /// <summary>
    /// Customer-related constants and configuration values
    /// </summary>
    public static class CustomerConstants
    {
        /// <summary>
        /// Customer Type IDs from CustomerTypes table
        /// </summary>
        public static class CustomerTypeIds
        {
            /// <summary>
            /// Standard customer - Default type for walk-in customers
            /// </summary>
            public const int Standard = 20;

            /// <summary>
            /// Silver customer - First tier loyalty customer
            /// </summary>
            public const int Silver = 21;

            /// <summary>
            /// Gold customer - Second tier loyalty customer
            /// </summary>
            public const int Gold = 22;

            /// <summary>
            /// VIP customer - Premium tier customer
            /// </summary>
            public const int VIP = 23;

            /// <summary>
            /// Diamond customer - Highest tier loyalty customer
            /// </summary>
            public const int Diamond = 24;
        }

        /// <summary>
        /// Default customer type for new walk-in customers
        /// </summary>
        public const int DefaultCustomerTypeId = CustomerTypeIds.Standard;

        /// <summary>
        /// Default loyalty points for new customers
        /// </summary>
        public const int DefaultLoyaltyPoints = 0;

        /// <summary>
        /// Default active status for new customers
        /// </summary>
        public const bool DefaultIsActive = true;

        /// <summary>
        /// Default marketing opt-in status
        /// </summary>
        public const bool DefaultMarketingOptIn = false;

        /// <summary>
        /// Customer code prefix
        /// </summary>
        public const string CustomerCodePrefix = "KH";
    }
}
