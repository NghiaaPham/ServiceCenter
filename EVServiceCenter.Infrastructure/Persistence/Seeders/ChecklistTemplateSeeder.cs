using EVServiceCenter.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text;

namespace EVServiceCenter.Infrastructure.Persistence.Seeders
{
    public static class ChecklistTemplateSeeder
    {
        // JSON serializer options with UTF8 encoding support
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = false
        };

        public static void SeedChecklistTemplates(EVDbContext context)
        {
            if (context.ChecklistTemplates.Any())
            {
                Console.WriteLine("✓ ChecklistTemplates already seeded, skipping...");
                return;
            }

            // Lấy CategoryId và ServiceId từ database (bao gồm cả Category của Service)
            var categoryBaoDuong = context.ServiceCategories.FirstOrDefault(c => c.CategoryName == "Bảo dưỡng định kỳ")?.CategoryId;
            var categoryPin = context.ServiceCategories.FirstOrDefault(c => c.CategoryName == "Pin và động cơ điện")?.CategoryId;
            var categoryPhanh = context.ServiceCategories.FirstOrDefault(c => c.CategoryName == "Hệ thống phanh")?.CategoryId;
            var categoryLop = context.ServiceCategories.FirstOrDefault(c => c.CategoryName == "Lốp xe")?.CategoryId;
            var categoryDieuHoa = context.ServiceCategories.FirstOrDefault(c => c.CategoryName == "Điều hòa không khí")?.CategoryId;

            // Load services with their categories
            var serviceBD10K = context.MaintenanceServices.Include(s => s.Category).FirstOrDefault(s => s.ServiceCode == "BD-10K");
            var serviceBD20K = context.MaintenanceServices.Include(s => s.Category).FirstOrDefault(s => s.ServiceCode == "BD-20K");
            var servicePinCheck = context.MaintenanceServices.Include(s => s.Category).FirstOrDefault(s => s.ServiceCode == "PIN-CHECK");
            var servicePinCool = context.MaintenanceServices.Include(s => s.Category).FirstOrDefault(s => s.ServiceCode == "PIN-COOL");
            var servicePhanhCheck = context.MaintenanceServices.Include(s => s.Category).FirstOrDefault(s => s.ServiceCode == "PHANH-CHECK");
            var servicePhanhReplace = context.MaintenanceServices.Include(s => s.Category).FirstOrDefault(s => s.ServiceCode == "PHANH-REPLACE");
            var serviceLopRotate = context.MaintenanceServices.Include(s => s.Category).FirstOrDefault(s => s.ServiceCode == "LOP-ROTATE");
            var serviceLopChange = context.MaintenanceServices.Include(s => s.Category).FirstOrDefault(s => s.ServiceCode == "LOP-CHANGE");
            var serviceACService = context.MaintenanceServices.Include(s => s.Category).FirstOrDefault(s => s.ServiceCode == "AC-SERVICE");

            var templates = new List<ChecklistTemplate>
            {
                // ========== SERVICE-SPECIFIC TEMPLATES (Priority 1) ==========
                
                // Template cho "Bảo dưỡng 10,000 km"
                new ChecklistTemplate
                {
                    ServiceId = serviceBD10K?.ServiceId,
                    CategoryId = serviceBD10K?.CategoryId, // Auto-fill từ service
                    TemplateName = "Checklist Bảo dưỡng 10,000 km",
                    Description = "Quy trình bảo dưỡng định kỳ cơ bản cho xe điện",
                    Items = JsonSerializer.Serialize(new[]
                    {
                        new { Description = "Kiểm tra mức dầu phanh", ItemOrder = 1, IsRequired = true },
                        new { Description = "Kiểm tra hệ thống treo", ItemOrder = 2, IsRequired = true },
                        new { Description = "Kiểm tra áp suất lốp", ItemOrder = 3, IsRequired = true },
                        new { Description = "Kiểm tra đèn chiếu sáng", ItemOrder = 4, IsRequired = true },
                        new { Description = "Kiểm tra gạt mưa", ItemOrder = 5, IsRequired = false }
                    }, _jsonOptions),
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                // Template cho "Bảo dưỡng 20,000 km"
                new ChecklistTemplate
                {
                    ServiceId = serviceBD20K?.ServiceId,
                    CategoryId = serviceBD20K?.CategoryId, // Auto-fill từ service
                    TemplateName = "Checklist Bảo dưỡng 20,000 km",
                    Description = "Quy trình bảo dưỡng toàn diện",
                    Items = JsonSerializer.Serialize(new[]
                    {
                        new { Description = "Thay dầu phanh", ItemOrder = 1, IsRequired = true },
                        new { Description = "Kiểm tra hệ thống phanh", ItemOrder = 2, IsRequired = true },
                        new { Description = "Kiểm tra má phanh trước/sau", ItemOrder = 3, IsRequired = true },
                        new { Description = "Kiểm tra hệ thống treo", ItemOrder = 4, IsRequired = true },
                        new { Description = "Kiểm tra hệ thống điều hòa", ItemOrder = 5, IsRequired = true },
                        new { Description = "Xoay vị trí lốp", ItemOrder = 6, IsRequired = true },
                        new { Description = "Kiểm tra pin 12V phụ", ItemOrder = 7, IsRequired = false }
                    }, _jsonOptions),
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                // Template cho "Kiểm tra sức khỏe pin"
                new ChecklistTemplate
                {
                    ServiceId = servicePinCheck?.ServiceId,
                    CategoryId = servicePinCheck?.CategoryId, // Auto-fill từ service
                    TemplateName = "Checklist Kiểm tra Pin",
                    Description = "Chẩn đoán và kiểm tra hệ thống pin xe điện",
                    Items = JsonSerializer.Serialize(new[]
                    {
                        new { Description = "Kết nối máy chẩn đoán", ItemOrder = 1, IsRequired = true },
                        new { Description = "Đọc mã lỗi hệ thống pin", ItemOrder = 2, IsRequired = true },
                        new { Description = "Kiểm tra dung lượng pin (kWh)", ItemOrder = 3, IsRequired = true },
                        new { Description = "Kiểm tra SOC (State of Charge)", ItemOrder = 4, IsRequired = true },
                        new { Description = "Kiểm tra SOH (State of Health)", ItemOrder = 5, IsRequired = true },
                        new { Description = "Kiểm tra nhiệt độ pin", ItemOrder = 6, IsRequired = true },
                        new { Description = "Kiểm tra cân bằng cell", ItemOrder = 7, IsRequired = false },
                        new { Description = "In báo cáo sức khỏe pin", ItemOrder = 8, IsRequired = true }
                    }, _jsonOptions),
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                // Template cho "Bảo dưỡng hệ thống làm mát pin"
                new ChecklistTemplate
                {
                    ServiceId = servicePinCool?.ServiceId,
                    CategoryId = servicePinCool?.CategoryId, // Auto-fill từ service
                    TemplateName = "Checklist Làm mát Pin",
                    Description = "Bảo dưỡng hệ thống làm mát pin EV",
                    Items = JsonSerializer.Serialize(new[]
                    {
                        new { Description = "Kiểm tra mức dung dịch làm mát", ItemOrder = 1, IsRequired = true },
                        new { Description = "Kiểm tra chất lượng dung dịch", ItemOrder = 2, IsRequired = true },
                        new { Description = "Kiểm tra đường ống dẫn", ItemOrder = 3, IsRequired = true },
                        new { Description = "Kiểm tra bơm tuần hoàn", ItemOrder = 4, IsRequired = true },
                        new { Description = "Kiểm tra van điều nhiệt", ItemOrder = 5, IsRequired = true },
                        new { Description = "Xả và thay dung dịch làm mát", ItemOrder = 6, IsRequired = true },
                        new { Description = "Nạp đầy dung dịch mới", ItemOrder = 7, IsRequired = true },
                        new { Description = "Kiểm tra rò rỉ", ItemOrder = 8, IsRequired = true },
                        new { Description = "Test hệ thống hoạt động", ItemOrder = 9, IsRequired = true }
                    }, _jsonOptions),
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                // Template cho "Kiểm tra hệ thống phanh"
                new ChecklistTemplate
                {
                    ServiceId = servicePhanhCheck?.ServiceId,
                    CategoryId = servicePhanhCheck?.CategoryId, // Auto-fill từ service
                    TemplateName = "Checklist Kiểm tra Phanh",
                    Description = "Kiểm tra tổng quan hệ thống phanh",
                    Items = JsonSerializer.Serialize(new[]
                    {
                        new { Description = "Kiểm tra độ dày má phanh trước", ItemOrder = 1, IsRequired = true },
                        new { Description = "Kiểm tra độ dày má phanh sau", ItemOrder = 2, IsRequired = true },
                        new { Description = "Kiểm tra mức dầu phanh", ItemOrder = 3, IsRequired = true },
                        new { Description = "Kiểm tra đĩa phanh có rãnh/nứt", ItemOrder = 4, IsRequired = true },
                        new { Description = "Kiểm tra ống dẫn phanh", ItemOrder = 5, IsRequired = true },
                        new { Description = "Test phanh tái sinh (regenerative braking)", ItemOrder = 6, IsRequired = false }
                    }, _jsonOptions),
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                // Template cho "Thay má phanh trước"
                new ChecklistTemplate
                {
                    ServiceId = servicePhanhReplace?.ServiceId,
                    CategoryId = servicePhanhReplace?.CategoryId, // Auto-fill từ service
                    TemplateName = "Checklist Thay Má Phanh",
                    Description = "Quy trình thay má phanh trước",
                    Items = JsonSerializer.Serialize(new[]
                    {
                        new { Description = "Nâng xe lên giá đỡ", ItemOrder = 1, IsRequired = true },
                        new { Description = "Tháo bánh xe", ItemOrder = 2, IsRequired = true },
                        new { Description = "Kiểm tra đĩa phanh", ItemOrder = 3, IsRequired = true },
                        new { Description = "Tháo má phanh cũ", ItemOrder = 4, IsRequired = true },
                        new { Description = "Làm sạch caliper và piston", ItemOrder = 5, IsRequired = true },
                        new { Description = "Lắp má phanh mới", ItemOrder = 6, IsRequired = true },
                        new { Description = "Kiểm tra mức dầu phanh", ItemOrder = 7, IsRequired = true },
                        new { Description = "Lắp bánh xe", ItemOrder = 8, IsRequired = true },
                        new { Description = "Test phanh trên đường thử", ItemOrder = 9, IsRequired = true }
                    }, _jsonOptions),
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                // Template cho "Cân chỉnh và xoay lốp"
                new ChecklistTemplate
                {
                    ServiceId = serviceLopRotate?.ServiceId,
                    CategoryId = serviceLopRotate?.CategoryId, // Auto-fill từ service
                    TemplateName = "Checklist Xoay và Cân Chỉnh Lốp",
                    Description = "Quy trình xoay vị trí và cân chỉnh lốp",
                    Items = JsonSerializer.Serialize(new[]
                    {
                        new { Description = "Kiểm tra độ mòn lốp", ItemOrder = 1, IsRequired = true },
                        new { Description = "Kiểm tra áp suất lốp", ItemOrder = 2, IsRequired = true },
                        new { Description = "Xoay lốp theo sơ đồ", ItemOrder = 3, IsRequired = true },
                        new { Description = "Cân chỉnh góc đặt bánh xe", ItemOrder = 4, IsRequired = true },
                        new { Description = "Cân bằng động lốp", ItemOrder = 5, IsRequired = true },
                        new { Description = "Bơm lốp đúng áp suất", ItemOrder = 6, IsRequired = true },
                        new { Description = "Test xe trên đường thử", ItemOrder = 7, IsRequired = false }
                    }, _jsonOptions),
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                // Template cho "Thay lốp xe"
                new ChecklistTemplate
                {
                    ServiceId = serviceLopChange?.ServiceId,
                    CategoryId = serviceLopChange?.CategoryId, // Auto-fill từ service
                    TemplateName = "Checklist Thay Lốp",
                    Description = "Quy trình thay lốp xe điện",
                    Items = JsonSerializer.Serialize(new[]
                    {
                        new { Description = "Kiểm tra lốp mới (kích cỡ, áp suất khuyến nghị)", ItemOrder = 1, IsRequired = true },
                        new { Description = "Nâng xe lên giá đỡ", ItemOrder = 2, IsRequired = true },
                        new { Description = "Tháo lốp cũ", ItemOrder = 3, IsRequired = true },
                        new { Description = "Kiểm tra lazang (không biến dạng/nứt)", ItemOrder = 4, IsRequired = true },
                        new { Description = "Lắp lốp mới", ItemOrder = 5, IsRequired = true },
                        new { Description = "Cân bằng động lốp", ItemOrder = 6, IsRequired = true },
                        new { Description = "Bơm lốp đúng áp suất", ItemOrder = 7, IsRequired = true },
                        new { Description = "Lắp bánh xe", ItemOrder = 8, IsRequired = true }
                    }, _jsonOptions),
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                // Template cho "Bảo dưỡng điều hòa"
                new ChecklistTemplate
                {
                    ServiceId = serviceACService?.ServiceId,
                    CategoryId = serviceACService?.CategoryId, // Auto-fill từ service
                    TemplateName = "Checklist Bảo dưỡng Điều hòa",
                    Description = "Quy trình bảo dưỡng hệ thống điều hòa",
                    Items = JsonSerializer.Serialize(new[]
                    {
                        new { Description = "Kiểm tra gas điều hòa", ItemOrder = 1, IsRequired = true },
                        new { Description = "Thay lọc gió cabin", ItemOrder = 2, IsRequired = true },
                        new { Description = "Vệ sinh dàn lạnh", ItemOrder = 3, IsRequired = true },
                        new { Description = "Kiểm tra compressor", ItemOrder = 4, IsRequired = true },
                        new { Description = "Kiểm tra đường ống gas", ItemOrder = 5, IsRequired = true },
                        new { Description = "Nạp gas (nếu cần)", ItemOrder = 6, IsRequired = false },
                        new { Description = "Test làm lạnh", ItemOrder = 7, IsRequired = true }
                    }, _jsonOptions),
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                // ========== CATEGORY-SPECIFIC TEMPLATES (Priority 2) ==========

                // Template cho Category "Pin và động cơ điện"
                new ChecklistTemplate
                {
                    ServiceId = null,
                    CategoryId = categoryPin,
                    TemplateName = "Checklist Pin và Động cơ điện (Chung)",
                    Description = "Template chung cho các dịch vụ liên quan đến pin và động cơ",
                    Items = JsonSerializer.Serialize(new[]
                    {
                        new { Description = "Kiểm tra kết nối điện", ItemOrder = 1, IsRequired = true },
                        new { Description = "Kiểm tra dung lượng pin", ItemOrder = 2, IsRequired = true },
                        new { Description = "Kiểm tra nhiệt độ pin", ItemOrder = 3, IsRequired = true },
                        new { Description = "Kiểm tra động cơ điện", ItemOrder = 4, IsRequired = false }
                    }, _jsonOptions),
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                // Template cho Category "Hệ thống phanh"
                new ChecklistTemplate
                {
                    ServiceId = null,
                    CategoryId = categoryPhanh,
                    TemplateName = "Checklist Hệ thống Phanh (Chung)",
                    Description = "Template chung cho các dịch vụ phanh",
                    Items = JsonSerializer.Serialize(new[]
                    {
                        new { Description = "Kiểm tra má phanh", ItemOrder = 1, IsRequired = true },
                        new { Description = "Kiểm tra đĩa phanh", ItemOrder = 2, IsRequired = true },
                        new { Description = "Kiểm tra dầu phanh", ItemOrder = 3, IsRequired = true }
                    }, _jsonOptions),
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                // ========== GENERIC TEMPLATE (Priority 3) ==========

                new ChecklistTemplate
                {
                    ServiceId = null,
                    CategoryId = null,
                    TemplateName = "Checklist Bảo dưỡng Tổng quát",
                    Description = "Template chung cho mọi loại dịch vụ không có template cụ thể",
                    Items = JsonSerializer.Serialize(new[]
                    {
                        new { Description = "Kiểm tra tổng quan xe", ItemOrder = 1, IsRequired = true },
                        new { Description = "Kiểm tra hệ thống phanh", ItemOrder = 2, IsRequired = true },
                        new { Description = "Kiểm tra áp suất lốp", ItemOrder = 3, IsRequired = true },
                        new { Description = "Kiểm tra đèn chiếu sáng", ItemOrder = 4, IsRequired = true },
                        new { Description = "Kiểm tra mức dầu phanh", ItemOrder = 5, IsRequired = false },
                        new { Description = "Ghi chú thêm nếu có", ItemOrder = 6, IsRequired = false }
                    }, _jsonOptions),
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                }
            };

            context.ChecklistTemplates.AddRange(templates);
            context.SaveChanges();

            Console.WriteLine($"✓ Seeded {templates.Count} checklist templates successfully");
        }
    }
}
