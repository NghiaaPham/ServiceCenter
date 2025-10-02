using EVServiceCenter.Core.Domains.AppointmentManagement.Entities;
using EVServiceCenter.Core.Entities;
using Microsoft.EntityFrameworkCore; // Để dùng ExecuteSqlRaw

namespace EVServiceCenter.Infrastructure.Persistence.Seeders
{
    public static class AppointmentStatusSeeder
    {
        public static void SeedAppointmentStatuses(EVDbContext context)
        {
            if (context.AppointmentStatuses.Any())
            {
                Console.WriteLine("✓ AppointmentStatuses already seeded, skipping...");
                return;
            }

            var statuses = new List<AppointmentStatus>
            {
                new AppointmentStatus
                {
                    StatusId = 1,
                    StatusName = "Pending",
                    StatusColor = "#FFA500",
                    Description = "Lịch hẹn đang chờ xác nhận",
                    DisplayOrder = 1,
                    IsActive = true
                },
                new AppointmentStatus
                {
                    StatusId = 2,
                    StatusName = "Confirmed",
                    StatusColor = "#0066CC",
                    Description = "Lịch hẹn đã được xác nhận",
                    DisplayOrder = 2,
                    IsActive = true
                },
                new AppointmentStatus
                {
                    StatusId = 3,
                    StatusName = "CheckedIn",
                    StatusColor = "#9933FF",
                    Description = "Khách hàng đã check-in tại trung tâm",
                    DisplayOrder = 3,
                    IsActive = true
                },
                new AppointmentStatus
                {
                    StatusId = 4,
                    StatusName = "InProgress",
                    StatusColor = "#3366FF",
                    Description = "Đang thực hiện dịch vụ",
                    DisplayOrder = 4,
                    IsActive = true
                },
                new AppointmentStatus
                {
                    StatusId = 5,
                    StatusName = "Completed",
                    StatusColor = "#00CC00",
                    Description = "Hoàn thành dịch vụ",
                    DisplayOrder = 5,
                    IsActive = true
                },
                new AppointmentStatus
                {
                    StatusId = 6,
                    StatusName = "Cancelled",
                    StatusColor = "#CC0000",
                    Description = "Lịch hẹn đã bị hủy",
                    DisplayOrder = 6,
                    IsActive = true
                },
                new AppointmentStatus
                {
                    StatusId = 7,
                    StatusName = "Rescheduled",
                    StatusColor = "#FF9900",
                    Description = "Lịch hẹn đã được dời sang thời gian khác",
                    DisplayOrder = 7,
                    IsActive = true
                },
                new AppointmentStatus
                {
                    StatusId = 8,
                    StatusName = "NoShow",
                    StatusColor = "#666666",
                    Description = "Khách hàng không đến theo lịch hẹn",
                    DisplayOrder = 8,
                    IsActive = true
                }
            };

            // Đảm bảo các lệnh chạy cùng 1 transaction/kết nối
            using (var transaction = context.Database.BeginTransaction())
            {
                context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT [AppointmentStatus] ON");
                context.AppointmentStatuses.AddRange(statuses);
                context.SaveChanges();
                context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT [AppointmentStatus] OFF");
                transaction.Commit();
            }

            Console.WriteLine($"✓ Seeded {statuses.Count} appointment statuses");
        }
    }
}