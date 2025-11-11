using EVServiceCenter.Core.Entities;
using EVServiceCenter.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace EVServiceCenter.Infrastructure.Persistence.Seeders;

/// <summary>
/// Seeder for Technician Schedules
/// Creates work schedules for next 30 days for all technicians
/// </summary>
public static class TechnicianScheduleSeeder
{
    public static void SeedTechnicianSchedules(EVDbContext context)
    {
        if (context.TechnicianSchedules.Any())
        {
            Console.WriteLine("Technician schedules already seeded. Skipping...");
            return;
        }

        // Get all technicians
        var technicians = context.Users
            .Where(u => u.RoleId == (int)UserRoles.Technician)
            .OrderBy(u => u.UserId)
            .ToList();

        if (!technicians.Any())
        {
            Console.WriteLine("⚠️ No technicians found. Please seed technicians first.");
            return;
        }

        // Get service centers
        var serviceCenters = context.ServiceCenters.OrderBy(sc => sc.CenterId).ToList();
        if (!serviceCenters.Any())
        {
            Console.WriteLine("⚠️ No service centers found. Cannot create schedules.");
            return;
        }

        var schedules = new List<TechnicianSchedule>();

        // Assign technicians to service centers
        // First 6 technicians -> Service Center 1
        // Last 4 technicians -> Service Center 2 (or first center if only 1 exists)
        var centerAssignments = new Dictionary<int, int>();
        for (int i = 0; i < technicians.Count; i++)
        {
            if (i < 6)
                centerAssignments[technicians[i].UserId] = serviceCenters[0].CenterId;
            else
                centerAssignments[technicians[i].UserId] = serviceCenters.Count > 1
                    ? serviceCenters[1].CenterId
                    : serviceCenters[0].CenterId;
        }

        // Create schedules for next 30 days
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var scheduleEndDate = today.AddDays(30);

        foreach (var tech in technicians)
        {
            var centerId = centerAssignments[tech.UserId];

            for (var date = today; date <= scheduleEndDate; date = date.AddDays(1))
            {
                // Skip Sundays (day of week = 0)
                if (date.DayOfWeek == DayOfWeek.Sunday)
                    continue;

                // Some technicians work Morning shift, some Afternoon, some Full day
                var shiftType = GetShiftType(tech.UserId, date.DayOfWeek);

                if (shiftType == "Off")
                    continue;

                schedules.Add(CreateSchedule(tech.UserId, centerId, date, shiftType));
            }
        }

        context.TechnicianSchedules.AddRange(schedules);
        context.SaveChanges();

        Console.WriteLine($"✅ Seeded {schedules.Count} technician schedules for {technicians.Count} technicians over 30 days");
    }

    /// <summary>
    /// Determine shift type based on technician ID and day of week
    /// Creates variety: some work full days, some split shifts
    /// </summary>
    private static string GetShiftType(int technicianId, DayOfWeek dayOfWeek)
    {
        // Pattern: Different technicians have different weekly schedules
        var techMod = technicianId % 3;

        switch (techMod)
        {
            case 0: // Always full day (Mon-Sat)
                return "Full Day";

            case 1: // Morning Mon/Wed/Fri, Afternoon Tue/Thu/Sat
                return (dayOfWeek == DayOfWeek.Monday ||
                        dayOfWeek == DayOfWeek.Wednesday ||
                        dayOfWeek == DayOfWeek.Friday)
                    ? "Morning"
                    : "Afternoon";

            case 2: // Afternoon Mon/Wed/Fri, Morning Tue/Thu, Off Saturday
                if (dayOfWeek == DayOfWeek.Saturday)
                    return "Off";
                return (dayOfWeek == DayOfWeek.Monday ||
                        dayOfWeek == DayOfWeek.Wednesday ||
                        dayOfWeek == DayOfWeek.Friday)
                    ? "Afternoon"
                    : "Morning";

            default:
                return "Full Day";
        }
    }

    /// <summary>
    /// Create a schedule entry based on shift type
    /// </summary>
    private static TechnicianSchedule CreateSchedule(
        int technicianId,
        int centerId,
        DateOnly workDate,
        string shiftType)
    {
        TimeOnly startTime, endTime, breakStart, breakEnd;
        int maxCapacityMinutes;

        switch (shiftType)
        {
            case "Morning":
                startTime = new TimeOnly(8, 0);
                endTime = new TimeOnly(12, 0);
                breakStart = new TimeOnly(10, 0);
                breakEnd = new TimeOnly(10, 15);
                maxCapacityMinutes = 225; // 4 hours - 15 min break
                break;

            case "Afternoon":
                startTime = new TimeOnly(13, 0);
                endTime = new TimeOnly(17, 0);
                breakStart = new TimeOnly(15, 0);
                breakEnd = new TimeOnly(15, 15);
                maxCapacityMinutes = 225; // 4 hours - 15 min break
                break;

            case "Full Day":
            default:
                startTime = new TimeOnly(8, 0);
                endTime = new TimeOnly(17, 0);
                breakStart = new TimeOnly(12, 0);
                breakEnd = new TimeOnly(13, 0);
                maxCapacityMinutes = 480; // 9 hours - 1 hour lunch
                break;
        }

        return new TechnicianSchedule
        {
            TechnicianId = technicianId,
            CenterId = centerId,
            WorkDate = workDate,
            StartTime = startTime,
            EndTime = endTime,
            BreakStartTime = breakStart,
            BreakEndTime = breakEnd,
            MaxCapacityMinutes = maxCapacityMinutes,
            BookedMinutes = 0, // No bookings initially
            AvailableMinutes = maxCapacityMinutes,
            IsAvailable = true,
            ShiftType = shiftType,
            Notes = null,
            CreatedDate = DateTime.UtcNow
        };
    }
}
