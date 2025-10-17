namespace EVServiceCenter.Core.Constants
{
    /// <summary>
    /// Constants for Refund status and methods
    /// </summary>
    public static class RefundConstants
    {
        /// <summary>
        /// Refund status values
        /// </summary>
        public static class Status
        {
            public const string Pending = "Pending";
            public const string Processing = "Processing";
            public const string Completed = "Completed";
            public const string Failed = "Failed";
            public const string Cancelled = "Cancelled";
        }

        /// <summary>
        /// Refund method values
        /// </summary>
        public static class Method
        {
            public const string Original = "Original";      // Hoàn về phương thức gốc
            public const string BankTransfer = "BankTransfer";  // Chuyển khoản
            public const string Cash = "Cash";              // Tiền mặt
        }
    }
}
