using EVServiceCenter.Core.Domains.MaintenanceServices.Entities;
using EVServiceCenter.Core.Entities;

namespace EVServiceCenter.Infrastructure.Persistence.Seeders
{
    public static class MaintenanceServiceSeeder
    {
        public static void SeedMaintenanceServices(EVDbContext context)
        {
            if (context.MaintenanceServices.Any())
            {
                Console.WriteLine("✓ MaintenanceServices already seeded, skipping...");
                return;
            }

            var categoryBaoDuong = context.ServiceCategories.FirstOrDefault(c => c.CategoryName == "Bảo dưỡng định kỳ")?.CategoryId ?? 1;
            var categoryPin = context.ServiceCategories.FirstOrDefault(c => c.CategoryName == "Pin và động cơ điện")?.CategoryId ?? 2;
            var categoryPhanh = context.ServiceCategories.FirstOrDefault(c => c.CategoryName == "Hệ thống phanh")?.CategoryId ?? 3;
            var categoryLop = context.ServiceCategories.FirstOrDefault(c => c.CategoryName == "Lốp xe")?.CategoryId ?? 4;
            var categoryDieuHoa = context.ServiceCategories.FirstOrDefault(c => c.CategoryName == "Điều hòa không khí")?.CategoryId ?? 5;
            var categoryKiemTra = context.ServiceCategories.FirstOrDefault(c => c.CategoryName == "Kiểm tra chẩn đoán")?.CategoryId ?? 6;
            var categoryPhanMem = context.ServiceCategories.FirstOrDefault(c => c.CategoryName == "Phần mềm và cập nhật")?.CategoryId ?? 7;

            var services = new List<MaintenanceService>
            {
                new MaintenanceService
                {
                    CategoryId = categoryBaoDuong,
                    ServiceCode = "BD-10K",
                    ServiceName = "Bảo dưỡng 10,000 km",
                    Description = "Bảo dưỡng định kỳ cơ bản: kiểm tra tổng quan, thay dầu phanh, kiểm tra pin",
                    StandardTime = 60,
                    BasePrice = 500000,
                    LaborCost = 200000,
                    SkillLevel = "Entry",
                    IsWarrantyService = false,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new MaintenanceService
                {
                    CategoryId = categoryBaoDuong,
                    ServiceCode = "BD-20K",
                    ServiceName = "Bảo dưỡng 20,000 km",
                    Description = "Bảo dưỡng định kỳ toàn diện: kiểm tra hệ thống phanh, treo, điều hòa",
                    StandardTime = 120,
                    BasePrice = 1200000,
                    LaborCost = 400000,
                    SkillLevel = "Intermediate",
                    IsWarrantyService = false,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                // Pin và động cơ điện
                new MaintenanceService
                {
                    CategoryId = categoryPin,
                    ServiceCode = "PIN-CHECK",
                    ServiceName = "Kiểm tra sức khỏe pin",
                    Description = "Chẩn đoán tình trạng pin, kiểm tra dung lượng và hiệu suất sạc",
                    StandardTime = 45,
                    BasePrice = 300000,
                    LaborCost = 150000,
                    SkillLevel = "Intermediate",
                    RequiredCertification = "EV Battery Level 1",
                    IsWarrantyService = true,
                    WarrantyPeriod = 12,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new MaintenanceService
                {
                    CategoryId = categoryPin,
                    ServiceCode = "PIN-COOL",
                    ServiceName = "Bảo dưỡng hệ thống làm mát pin",
                    Description = "Vệ sinh và kiểm tra hệ thống làm mát pin, thay dung dịch làm mát",
                    StandardTime = 90,
                    BasePrice = 1500000,
                    LaborCost = 500000,
                    SkillLevel = "Expert",
                    RequiredCertification = "EV Battery Level 2",
                    IsWarrantyService = true,
                    WarrantyPeriod = 6,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new MaintenanceService
                {
                    CategoryId = categoryPin,
                    ServiceCode = "MOTOR-CHECK",
                    ServiceName = "Kiểm tra động cơ điện",
                    Description = "Chẩn đoán và kiểm tra hiệu suất động cơ điện",
                    StandardTime = 60,
                    BasePrice = 800000,
                    LaborCost = 300000,
                    SkillLevel = "Expert",
                    IsWarrantyService = true,
                    WarrantyPeriod = 12,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                // Hệ thống phanh
                new MaintenanceService
                {
                    CategoryId = categoryPhanh,
                    ServiceCode = "PHANH-CHECK",
                    ServiceName = "Kiểm tra hệ thống phanh",
                    Description = "Kiểm tra má phanh, đĩa phanh, dầu phanh",
                    StandardTime = 30,
                    BasePrice = 200000,
                    LaborCost = 100000,
                    SkillLevel = "Entry",
                    IsWarrantyService = false,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new MaintenanceService
                {
                    CategoryId = categoryPhanh,
                    ServiceCode = "PHANH-REPLACE",
                    ServiceName = "Thay má phanh trước",
                    Description = "Thay má phanh trước, kiểm tra đĩa phanh",
                    StandardTime = 60,
                    BasePrice = 1500000,
                    LaborCost = 300000,
                    SkillLevel = "Intermediate",
                    IsWarrantyService = true,
                    WarrantyPeriod = 6,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                // Lốp xe
                new MaintenanceService
                {
                    CategoryId = categoryLop,
                    ServiceCode = "LOP-ROTATE",
                    ServiceName = "Cân chỉnh và xoay lốp",
                    Description = "Cân chỉnh góc đặt bánh xe, xoay vị trí lốp",
                    StandardTime = 45,
                    BasePrice = 400000,
                    LaborCost = 150000,
                    SkillLevel = "Entry",
                    IsWarrantyService = false,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new MaintenanceService
                {
                    CategoryId = categoryLop,
                    ServiceCode = "LOP-CHANGE",
                    ServiceName = "Thay lốp xe",
                    Description = "Thay lốp mới, cân bằng lốp (giá chưa bao gồm lốp)",
                    StandardTime = 30,
                    BasePrice = 200000,
                    LaborCost = 200000,
                    SkillLevel = "Entry",
                    IsWarrantyService = false,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                // Điều hòa
                new MaintenanceService
                {
                    CategoryId = categoryDieuHoa,
                    ServiceCode = "AC-SERVICE",
                    ServiceName = "Bảo dưỡng điều hòa",
                    Description = "Vệ sinh dàn lạnh, thay lọc gió điều hòa, nạp gas",
                    StandardTime = 90,
                    BasePrice = 1200000,
                    LaborCost = 400000,
                    SkillLevel = "Intermediate",
                    IsWarrantyService = true,
                    WarrantyPeriod = 3,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                // Kiểm tra chẩn đoán
                new MaintenanceService
                {
                    CategoryId = categoryKiemTra,
                    ServiceCode = "DIAG-FULL",
                    ServiceName = "Chẩn đoán tổng quát",
                    Description = "Chẩn đoán toàn bộ hệ thống xe bằng máy chuyên dụng",
                    StandardTime = 60,
                    BasePrice = 500000,
                    LaborCost = 300000,
                    SkillLevel = "Expert",
                    RequiredCertification = "EV Diagnostic Level 2",
                    IsWarrantyService = false,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new MaintenanceService
                {
                    CategoryId = categoryKiemTra,
                    ServiceCode = "INSPECT-ANNUAL",
                    ServiceName = "Kiểm tra định kỳ hàng năm",
                    Description = "Kiểm tra toàn diện theo quy định nhà sản xuất",
                    StandardTime = 120,
                    BasePrice = 800000,
                    LaborCost = 400000,
                    SkillLevel = "Intermediate",
                    IsWarrantyService = false,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                // Phần mềm
                new MaintenanceService
                {
                    CategoryId = categoryPhanMem,
                    ServiceCode = "SW-UPDATE",
                    ServiceName = "Cập nhật phần mềm hệ thống",
                    Description = "Cập nhật firmware, phần mềm điều khiển xe",
                    StandardTime = 90,
                    BasePrice = 1000000,
                    LaborCost = 500000,
                    SkillLevel = "Expert",
                    RequiredCertification = "EV Software Level 2",
                    IsWarrantyService = true,
                    WarrantyPeriod = 1,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                }
            };

            context.MaintenanceServices.AddRange(services);
            context.SaveChanges();

            Console.WriteLine($"✓ Seeded {services.Count} maintenance services successfully");
        }
    }
}