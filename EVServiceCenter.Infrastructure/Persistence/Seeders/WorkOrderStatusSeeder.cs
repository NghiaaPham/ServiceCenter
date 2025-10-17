using EVServiceCenter.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace EVServiceCenter.Infrastructure.Persistence.Seeders
{
    /// <summary>
    /// Seeder cho WorkOrderStatus
    /// Ph?i có data này tr??c khi CheckIn appointment (t?o WorkOrder)
    /// </summary>
    public static class WorkOrderStatusSeeder
    {
        public static void SeedWorkOrderStatuses(EVDbContext context)
        {
            if (context.WorkOrderStatuses.Any())
            {
                Console.WriteLine("? WorkOrderStatuses already seeded, skipping...");
                return;
            }

            var statuses = new List<WorkOrderStatus>
            {
                new WorkOrderStatus
                {
                    StatusId = 1, // Created
                    StatusName = "Created",
                    StatusColor = "#FFA500", // Orange
                    Description = "WorkOrder ?ã ???c t?o, ch? phân công",
                    IsActive = true,
                    DisplayOrder = 1,
                    AllowEdit = true,
                    RequireApproval = false
                },
                new WorkOrderStatus
                {
                    StatusId = 2, // Assigned
                    StatusName = "Assigned",
                    StatusColor = "#0066CC", // Blue
                    Description = "WorkOrder ?ã ???c phân công cho k? thu?t viên",
                    IsActive = true,
                    DisplayOrder = 2,
                    AllowEdit = true,
                    RequireApproval = false
                },
                new WorkOrderStatus
                {
                    StatusId = 3, // InProgress
                    StatusName = "InProgress",
                    StatusColor = "#3366FF", // Light Blue
                    Description = "K? thu?t viên ?ang th?c hi?n công vi?c",
                    IsActive = true,
                    DisplayOrder = 3,
                    AllowEdit = true,
                    RequireApproval = false
                },
                new WorkOrderStatus
                {
                    StatusId = 4, // AwaitingParts
                    StatusName = "AwaitingParts",
                    StatusColor = "#FF9900", // Light Orange
                    Description = "?ang ch? ph? tùng/linh ki?n",
                    IsActive = true,
                    DisplayOrder = 4,
                    AllowEdit = true,
                    RequireApproval = false
                },
                new WorkOrderStatus
                {
                    StatusId = 5, // QualityCheck
                    StatusName = "QualityCheck",
                    StatusColor = "#9933FF", // Purple
                    Description = "?ang ki?m tra ch?t l??ng",
                    IsActive = true,
                    DisplayOrder = 5,
                    AllowEdit = true,
                    RequireApproval = true
                },
                new WorkOrderStatus
                {
                    StatusId = 6, // Completed
                    StatusName = "Completed",
                    StatusColor = "#00CC00", // Green
                    Description = "WorkOrder ?ã hoàn thành",
                    IsActive = true,
                    DisplayOrder = 6,
                    AllowEdit = false, // Không cho s?a sau khi hoàn thành
                    RequireApproval = false
                },
                new WorkOrderStatus
                {
                    StatusId = 7, // Cancelled
                    StatusName = "Cancelled",
                    StatusColor = "#CC0000", // Red
                    Description = "WorkOrder ?ã b? h?y",
                    IsActive = true,
                    DisplayOrder = 7,
                    AllowEdit = false,
                    RequireApproval = false
                }
            };

            using (var transaction = context.Database.BeginTransaction())
            {
                try
                {
                    // Enable IDENTITY_INSERT ?? insert v?i ID c? th?
                    context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT [WorkOrderStatus] ON");
                    
                    context.WorkOrderStatuses.AddRange(statuses);
                    context.SaveChanges();
                    
                    context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT [WorkOrderStatus] OFF");
                    
                    transaction.Commit();
                    
                    Console.WriteLine($"? Seeded {statuses.Count} WorkOrderStatuses");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Console.WriteLine($"? Error seeding WorkOrderStatuses: {ex.Message}");
                    throw;
                }
            }
        }
    }
}
