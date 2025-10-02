using EVServiceCenter.Core.Domains.ServiceCategories.Entities;
using EVServiceCenter.Core.Entities;

namespace EVServiceCenter.Infrastructure.Persistence.Seeders
{
    public static class ServiceCategorySeeder
    {
        public static void SeedServiceCategories(EVDbContext context)
        {
            if (context.ServiceCategories.Any())
            {
                Console.WriteLine("✓ ServiceCategories already seeded, skipping...");
                return;
            }

            var categories = new List<ServiceCategory>
            {
                new ServiceCategory
                {
                    CategoryName = "Bảo dưỡng định kỳ",
                    Description = "Các dịch vụ bảo dưỡng định kỳ theo km hoặc thời gian sử dụng",
                    IconUrl = "https://example.com/icons/maintenance.png",
                    DisplayOrder = 1,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new ServiceCategory
                {
                    CategoryName = "Pin và động cơ điện",
                    Description = "Kiểm tra, bảo dưỡng và sửa chữa hệ thống pin và động cơ điện",
                    IconUrl = "https://example.com/icons/battery.png",
                    DisplayOrder = 2,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new ServiceCategory
                {
                    CategoryName = "Hệ thống phanh",
                    Description = "Kiểm tra và thay thế các bộ phận phanh",
                    IconUrl = "https://example.com/icons/brake.png",
                    DisplayOrder = 3,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new ServiceCategory
                {
                    CategoryName = "Lốp xe",
                    Description = "Thay lốp, cân chỉnh, bơm lốp và các dịch vụ liên quan đến lốp",
                    IconUrl = "https://example.com/icons/tire.png",
                    DisplayOrder = 4,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new ServiceCategory
                {
                    CategoryName = "Điều hòa không khí",
                    Description = "Bảo dưỡng và sửa chữa hệ thống điều hòa",
                    IconUrl = "https://example.com/icons/ac.png",
                    DisplayOrder = 5,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new ServiceCategory
                {
                    CategoryName = "Kiểm tra chẩn đoán",
                    Description = "Kiểm tra tổng quát và chẩn đoán lỗi hệ thống",
                    IconUrl = "https://example.com/icons/diagnostic.png",
                    DisplayOrder = 6,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new ServiceCategory
                {
                    CategoryName = "Phần mềm và cập nhật",
                    Description = "Cập nhật phần mềm hệ thống, firmware và các tính năng mới",
                    IconUrl = "https://example.com/icons/software.png",
                    DisplayOrder = 7,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new ServiceCategory
                {
                    CategoryName = "Nội thất và ngoại thất",
                    Description = "Làm sạch, bảo dưỡng nội thất và sơn phủ ngoại thất",
                    IconUrl = "https://example.com/icons/detail.png",
                    DisplayOrder = 8,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new ServiceCategory
                {
                    CategoryName = "Hệ thống treo và giảm xóc",
                    Description = "Kiểm tra và thay thế hệ thống treo, giảm xóc",
                    IconUrl = "https://example.com/icons/suspension.png",
                    DisplayOrder = 9,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new ServiceCategory
                {
                    CategoryName = "Đèn chiếu sáng",
                    Description = "Thay thế và điều chỉnh hệ thống đèn",
                    IconUrl = "https://example.com/icons/light.png",
                    DisplayOrder = 10,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                }
            };

            context.ServiceCategories.AddRange(categories);
            context.SaveChanges();

            Console.WriteLine($"✓ Seeded {categories.Count} service categories successfully");
        }
    }
}