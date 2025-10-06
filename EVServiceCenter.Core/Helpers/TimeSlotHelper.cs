using EVServiceCenter.Core.Domains.TimeSlots.Entities;

namespace EVServiceCenter.Core.Helpers
{
    /// <summary>
    /// Helper methods for TimeSlot operations and overlap detection
    /// </summary>
    public static class TimeSlotHelper
    {
        /// <summary>
        /// Check if two time ranges overlap
        /// </summary>
        /// <param name="start1">Start of range 1</param>
        /// <param name="end1">End of range 1</param>
        /// <param name="start2">Start of range 2</param>
        /// <param name="end2">End of range 2</param>
        /// <returns>True if ranges overlap</returns>
        public static bool DoTimesOverlap(DateTime start1, DateTime end1, DateTime start2, DateTime end2)
        {
            // Two ranges overlap if: (Start1 < End2) AND (End1 > Start2)
            return start1 < end2 && end1 > start2;
        }

        /// <summary>
        /// Check if two slots overlap (same date and overlapping time)
        /// </summary>
        public static bool DoSlotsOverlap(TimeSlot slot1, TimeSlot slot2)
        {
            // Different dates → no overlap
            if (slot1.SlotDate != slot2.SlotDate)
                return false;

            var start1 = slot1.SlotDate.ToDateTime(slot1.StartTime);
            var end1 = slot1.SlotDate.ToDateTime(slot1.EndTime);
            var start2 = slot2.SlotDate.ToDateTime(slot2.StartTime);
            var end2 = slot2.SlotDate.ToDateTime(slot2.EndTime);

            return DoTimesOverlap(start1, end1, start2, end2);
        }

        /// <summary>
        /// Check if two slots are adjacent (liền kề)
        /// Example: 8h-9h và 9h-10h
        /// </summary>
        public static bool AreSlotsAdjacent(TimeSlot slot1, TimeSlot slot2)
        {
            // Must be same date
            if (slot1.SlotDate != slot2.SlotDate)
                return false;

            // Check if end of slot1 == start of slot2 OR end of slot2 == start of slot1
            return slot1.EndTime == slot2.StartTime || slot2.EndTime == slot1.StartTime;
        }

        /// <summary>
        /// Get slot start DateTime
        /// </summary>
        public static DateTime GetSlotStartDateTime(TimeSlot slot)
        {
            return slot.SlotDate.ToDateTime(slot.StartTime);
        }

        /// <summary>
        /// Get slot end DateTime
        /// </summary>
        public static DateTime GetSlotEndDateTime(TimeSlot slot)
        {
            return slot.SlotDate.ToDateTime(slot.EndTime);
        }

        /// <summary>
        /// Check if slot is on the same day as given date
        /// </summary>
        public static bool IsSameDay(TimeSlot slot, DateOnly date)
        {
            return slot.SlotDate == date;
        }

        /// <summary>
        /// Check if slot is in the past
        /// </summary>
        public static bool IsSlotInPast(TimeSlot slot)
        {
            var slotDateTime = GetSlotStartDateTime(slot);
            return slotDateTime < DateTime.UtcNow;
        }
    }
}
