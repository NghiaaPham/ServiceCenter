using EVServiceCenter.Core.Domains.AppointmentManagement.Entities;
using EVServiceCenter.Core.Domains.TimeSlots.Entities;
using EVServiceCenter.Core.Entities;

namespace EVServiceCenter.Infrastructure.Persistence.Seeders
{
    public static class TimeSlotSeeder
    {
        public static void SeedTimeSlots(EVDbContext context)
        {
            if (context.TimeSlots.Any())
            {
                Console.WriteLine("✓ TimeSlots already seeded, skipping...");
                return;
            }

            var centers = context.ServiceCenters.ToList();
            if (!centers.Any())
            {
                Console.WriteLine("⚠ No ServiceCenters found, skipping TimeSlot seeding");
                return;
            }

            var slots = new List<TimeSlot>();
            var startDate = DateOnly.FromDateTime(DateTime.Today);
            var endDate = startDate.AddDays(30); // Generate 30 days

            foreach (var center in centers)
            {
                var currentDate = startDate;

                while (currentDate <= endDate)
                {
                    // Working hours: 8:00 - 18:00
                    var workingStart = TimeOnly.Parse("08:00");
                    var workingEnd = TimeOnly.Parse("18:00");
                    var slotDuration = 60; // 1 hour slots

                    var currentTime = workingStart;

                    while (currentTime < workingEnd)
                    {
                        var slotEnd = currentTime.AddMinutes(slotDuration);
                        if (slotEnd > workingEnd)
                            break;

                        slots.Add(new TimeSlot
                        {
                            CenterId = center.CenterId,
                            SlotDate = currentDate,
                            StartTime = currentTime,
                            EndTime = slotEnd,
                            MaxBookings = center.Capacity > 10 ? 2 : 1,
                            SlotType = "Regular",
                            IsBlocked = false,
                            CreatedDate = DateTime.UtcNow
                        });

                        currentTime = slotEnd;
                    }

                    currentDate = currentDate.AddDays(1);
                }
            }

            context.TimeSlots.AddRange(slots);
            context.SaveChanges();

            Console.WriteLine($"✓ Seeded {slots.Count} timeslots for {centers.Count} centers (30 days)");
        }
    }
}