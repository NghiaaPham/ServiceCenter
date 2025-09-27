using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVServiceCenter.Core.Constants
{
    public static class StatusConstants
    {
        // Appointment Status
        public const string APPOINTMENT_SCHEDULED = "Scheduled";
        public const string APPOINTMENT_CONFIRMED = "Confirmed";
        public const string APPOINTMENT_COMPLETED = "Completed";
        public const string APPOINTMENT_CANCELLED = "Cancelled";

        // Work Order Status
        public const string WORKORDER_CREATED = "Created";
        public const string WORKORDER_IN_PROGRESS = "In Progress";
        public const string WORKORDER_COMPLETED = "Completed";
        public const string WORKORDER_CANCELLED = "Cancelled";

        // Service Status
        public const string SERVICE_PENDING = "Pending";
        public const string SERVICE_IN_PROGRESS = "In Progress";
        public const string SERVICE_COMPLETED = "Completed";

        // Payment Status
        public const string PAYMENT_PENDING = "Pending";
        public const string PAYMENT_COMPLETED = "Completed";
        public const string PAYMENT_FAILED = "Failed";

        // Invoice Status
        public const string INVOICE_UNPAID = "Unpaid";
        public const string INVOICE_PAID = "Paid";
        public const string INVOICE_CANCELLED = "Cancelled";

        // Warranty Status
        public const string WARRANTY_ACTIVE = "Active";
        public const string WARRANTY_EXPIRED = "Expired";
        public const string WARRANTY_CLAIMED = "Claimed";
    }
}
