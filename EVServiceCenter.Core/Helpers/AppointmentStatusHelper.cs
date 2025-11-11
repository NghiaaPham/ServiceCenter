using EVServiceCenter.Core.Enums;

namespace EVServiceCenter.Core.Helpers
{
    /// <summary>
    /// Helper class for appointment status business rules
    /// </summary>
    public static class AppointmentStatusHelper
    {
        /// <summary>
        /// Statuses that do NOT occupy capacity/timeslots
        /// </summary>
        public static readonly int[] ExcludedFromCapacity = new[]
        {
            (int)AppointmentStatusEnum.Cancelled,
            (int)AppointmentStatusEnum.NoShow,
            (int)AppointmentStatusEnum.Rescheduled
        };

        /// <summary>
        /// Statuses that occupy capacity/timeslots (active bookings)
        /// </summary>
        public static readonly int[] ActiveBookings = new[]
        {
            (int)AppointmentStatusEnum.Pending,
            (int)AppointmentStatusEnum.Confirmed,
            (int)AppointmentStatusEnum.CheckedIn,
            (int)AppointmentStatusEnum.InProgress,
            (int)AppointmentStatusEnum.Completed,
            (int)AppointmentStatusEnum.CompletedWithUnpaidBalance  // ✅ Thêm status mới
        };

        /// <summary>
        /// Final statuses that cannot be changed
        /// </summary>
        public static readonly int[] FinalStatuses = new[]
        {
            (int)AppointmentStatusEnum.Completed,
            (int)AppointmentStatusEnum.CompletedWithUnpaidBalance,  // ✅ Status mới cũng là final
            (int)AppointmentStatusEnum.Cancelled,
            (int)AppointmentStatusEnum.Rescheduled,
            (int)AppointmentStatusEnum.NoShow
        };

        /// <summary>
        /// Check if status counts as active booking
        /// </summary>
        public static bool IsActiveBooking(int statusId)
            => ActiveBookings.Contains(statusId);

        /// <summary>
        /// Check if status should be excluded from capacity calculation
        /// </summary>
        public static bool ShouldExcludeFromCapacity(int statusId)
            => ExcludedFromCapacity.Contains(statusId);

        /// <summary>
        /// Check if status is final (cannot transition)
        /// </summary>
        public static bool IsFinalStatus(int statusId)
            => FinalStatuses.Contains(statusId);

        /// <summary>
        /// Validate status transition
        /// </summary>
        /// <param name="currentStatusId">Current status ID</param>
        /// <param name="newStatusId">New status ID to transition to</param>
        /// <returns>True if transition is valid</returns>
        public static bool CanTransitionTo(int currentStatusId, int newStatusId)
        {
            // Cannot change final statuses
            if (IsFinalStatus(currentStatusId))
                return false;

            var validTransitions = new Dictionary<int, int[]>
            {
                {
                    (int)AppointmentStatusEnum.Pending,
                    new[]
                    {
                        (int)AppointmentStatusEnum.Confirmed,
                        (int)AppointmentStatusEnum.Cancelled,
                        (int)AppointmentStatusEnum.Rescheduled
                    }
                },
                {
                    (int)AppointmentStatusEnum.Confirmed,
                    new[]
                    {
                        (int)AppointmentStatusEnum.CheckedIn,
                        (int)AppointmentStatusEnum.Cancelled,
                        (int)AppointmentStatusEnum.Rescheduled,
                        (int)AppointmentStatusEnum.NoShow
                    }
                },
                {
                    (int)AppointmentStatusEnum.CheckedIn,
                    new[]
                    {
                        (int)AppointmentStatusEnum.InProgress,
                        (int)AppointmentStatusEnum.Cancelled
                    }
                },
                {
                    (int)AppointmentStatusEnum.InProgress,
                    new[]
                    {
                        (int)AppointmentStatusEnum.Completed,
                        (int)AppointmentStatusEnum.CompletedWithUnpaidBalance  // ✅ Cho phép transition
                    }
                }
            };

            return validTransitions.ContainsKey(currentStatusId) &&
                   validTransitions[currentStatusId].Contains(newStatusId);
        }

        /// <summary>
        /// Get status display name in Vietnamese
        /// </summary>
        public static string GetStatusDisplayName(int statusId)
        {
            return statusId switch
            {
                (int)AppointmentStatusEnum.Pending => "Chờ xác nhận",
                (int)AppointmentStatusEnum.Confirmed => "Đã xác nhận",
                (int)AppointmentStatusEnum.CheckedIn => "Đã check-in",
                (int)AppointmentStatusEnum.InProgress => "Đang thực hiện",
                (int)AppointmentStatusEnum.Completed => "Hoàn thành",
                (int)AppointmentStatusEnum.CompletedWithUnpaidBalance => "Hoàn thành (còn công nợ)",  // ✅ Label mới
                (int)AppointmentStatusEnum.Cancelled => "Đã hủy",
                (int)AppointmentStatusEnum.Rescheduled => "Đã dời lịch",
                (int)AppointmentStatusEnum.NoShow => "Không đến",
                _ => "Không xác định"
            };
        }

        /// <summary>
        /// Get valid next statuses from current status
        /// </summary>
        public static int[] GetValidNextStatuses(int currentStatusId)
        {
            if (IsFinalStatus(currentStatusId))
                return Array.Empty<int>();

            var validTransitions = new Dictionary<int, int[]>
            {
                { (int)AppointmentStatusEnum.Pending, new[]
                    {
                        (int)AppointmentStatusEnum.Confirmed,
                        (int)AppointmentStatusEnum.Cancelled,
                        (int)AppointmentStatusEnum.Rescheduled
                    }
                },
                { (int)AppointmentStatusEnum.Confirmed, new[]
                    {
                        (int)AppointmentStatusEnum.CheckedIn,
                        (int)AppointmentStatusEnum.Cancelled,
                        (int)AppointmentStatusEnum.Rescheduled,
                        (int)AppointmentStatusEnum.NoShow
                    }
                },
                { (int)AppointmentStatusEnum.CheckedIn, new[]
                    {
                        (int)AppointmentStatusEnum.InProgress,
                        (int)AppointmentStatusEnum.Cancelled
                    }
                },
                { (int)AppointmentStatusEnum.InProgress, new[]
                    {
                        (int)AppointmentStatusEnum.Completed,
                        (int)AppointmentStatusEnum.CompletedWithUnpaidBalance  // ✅ Valid transition
                    }
                }
            };

            return validTransitions.TryGetValue(currentStatusId, out var statuses)
                ? statuses
                : Array.Empty<int>();
        }
    }
}