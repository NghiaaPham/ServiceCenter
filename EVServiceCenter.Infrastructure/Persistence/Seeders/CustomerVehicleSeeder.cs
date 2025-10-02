using EVServiceCenter.Core.Domains.CustomerVehicles.Entities;
using EVServiceCenter.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace EVServiceCenter.Infrastructure.Persistence.Seeders
{
    public static class CustomerVehicleSeeder
    {
        public static void SeedCustomerVehicles(EVDbContext context)
        {
            if (context.CustomerVehicles.Any())
            {
                Console.WriteLine("✓ CustomerVehicles already seeded, skipping...");
                return;
            }

            var customers = context.Customers
                .OrderBy(c => c.CustomerId)
                .Take(10)
                .ToList();

            if (!customers.Any())
            {
                Console.WriteLine("⚠️ No customers found. Please seed Customers first.");
                Console.WriteLine($"   Current Customers count: {context.Customers.Count()}");
                return;
            }

            // ✅ Get models with Include to load Brand data
            var models = context.CarModels
                .Include(m => m.Brand)
                .OrderBy(m => m.ModelId)
                .ToList();

            if (!models.Any())
            {
                Console.WriteLine("⚠️ No car models found. Please seed CarModels first.");
                Console.WriteLine($"   Current CarBrands count: {context.CarBrands.Count()}");
                Console.WriteLine($"   Current CarModels count: {context.CarModels.Count()}");
                return;
            }

            // Find specific models - based on your actual data
            var model3 = models.FirstOrDefault(m => m.ModelName == "Model 3");      // ModelId: 1, BrandId: 2 (Tesla)
            var modelY = models.FirstOrDefault(m => m.ModelName == "Model Y");      // ModelId: 2, BrandId: 2 (Tesla)
            var modelS = models.FirstOrDefault(m => m.ModelName == "Model S");      // ModelId: 3, BrandId: 2 (Tesla)
            var atto3 = models.FirstOrDefault(m => m.ModelName == "Atto 3");        // ModelId: 4, BrandId: 4 (BYD)
            var seal = models.FirstOrDefault(m => m.ModelName == "Seal");           // ModelId: 5, BrandId: 4 (BYD)
            var ioniq5 = models.FirstOrDefault(m => m.ModelName == "IONIQ 5");      // ModelId: 6, BrandId: 7 (Hyundai)
            var ioniq6 = models.FirstOrDefault(m => m.ModelName == "IONIQ 6");      // ModelId: 7, BrandId: 7 (Hyundai)
            var ix = models.FirstOrDefault(m => m.ModelName == "iX");               // ModelId: 8, BrandId: 9 (BMW)
            var i4 = models.FirstOrDefault(m => m.ModelName == "i4");               // ModelId: 9, BrandId: 9 (BMW)
            var vf6 = models.FirstOrDefault(m => m.ModelName == "VF 6");            // ModelId: 10, BrandId: 1 (VinFast)

            if (model3 == null)
            {
                Console.WriteLine("⚠️ Required models not found. Available models:");
                foreach (var m in models.Take(5))
                {
                    Console.WriteLine($"   - {m.Brand?.BrandName} {m.ModelName} (ModelId: {m.ModelId})");
                }
                return;
            }

            var vehicles = new List<CustomerVehicle>();

            // Customer 1 - Tesla Model 3
            if (customers.Count > 0 && model3 != null)
            {
                vehicles.Add(new CustomerVehicle
                {
                    CustomerId = customers[0].CustomerId,
                    ModelId = model3.ModelId,
                    LicensePlate = "30A-12345",
                    Vin = "5YJ3E1EA0LF000001",
                    Color = "Đỏ",
                    PurchaseDate = new DateOnly(2023, 6, 15),
                    Mileage = 15000,
                    LastMaintenanceDate = new DateOnly(2024, 8, 1),
                    NextMaintenanceDate = new DateOnly(2025, 2, 1),
                    LastMaintenanceMileage = 10000,
                    NextMaintenanceMileage = 20000,
                    BatteryHealthPercent = 98.5m,
                    VehicleCondition = "Excellent",
                    InsuranceNumber = "TES-INS-001",
                    InsuranceExpiry = new DateOnly(2025, 6, 15),
                    RegistrationExpiry = new DateOnly(2025, 6, 15),
                    Notes = "Xe sử dụng thường xuyên, đi trong thành phố",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow.AddMonths(-15)
                });
            }

            // Customer 1 - Tesla Model Y (second vehicle)
            if (customers.Count > 0 && modelY != null)
            {
                vehicles.Add(new CustomerVehicle
                {
                    CustomerId = customers[0].CustomerId,
                    ModelId = modelY.ModelId,
                    LicensePlate = "30B-67890",
                    Vin = "5YJYGDEE0MF000001",
                    Color = "Trắng",
                    PurchaseDate = new DateOnly(2024, 1, 10),
                    Mileage = 5000,
                    LastMaintenanceDate = new DateOnly(2024, 7, 10),
                    NextMaintenanceDate = new DateOnly(2025, 1, 10),
                    LastMaintenanceMileage = 0,
                    NextMaintenanceMileage = 10000,
                    BatteryHealthPercent = 99.8m,
                    VehicleCondition = "New",
                    InsuranceNumber = "TES-INS-002",
                    InsuranceExpiry = new DateOnly(2025, 1, 10),
                    RegistrationExpiry = new DateOnly(2025, 1, 10),
                    Notes = "Xe gia đình, đi xa cuối tuần",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow.AddMonths(-9)
                });
            }

            // Customer 2 - Tesla Model S
            if (customers.Count > 1 && modelS != null)
            {
                vehicles.Add(new CustomerVehicle
                {
                    CustomerId = customers[1].CustomerId,
                    ModelId = modelS.ModelId,
                    LicensePlate = "29C-11111",
                    Vin = "5YJSA1E20HF000001",
                    Color = "Xanh đen",
                    PurchaseDate = new DateOnly(2023, 3, 20),
                    Mileage = 25000,
                    LastMaintenanceDate = new DateOnly(2024, 9, 15),
                    NextMaintenanceDate = new DateOnly(2025, 3, 15),
                    LastMaintenanceMileage = 20000,
                    NextMaintenanceMileage = 30000,
                    BatteryHealthPercent = 95.2m,
                    VehicleCondition = "Good",
                    InsuranceNumber = "TES-INS-003",
                    InsuranceExpiry = new DateOnly(2025, 3, 20),
                    RegistrationExpiry = new DateOnly(2025, 3, 20),
                    Notes = "Sử dụng hàng ngày đi làm",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow.AddMonths(-18)
                });
            }

            // Customer 2 - BYD Atto 3
            if (customers.Count > 1 && atto3 != null)
            {
                vehicles.Add(new CustomerVehicle
                {
                    CustomerId = customers[1].CustomerId,
                    ModelId = atto3.ModelId,
                    LicensePlate = "51F-22222",
                    Vin = "LGXC16EF0N0000001",
                    Color = "Xám",
                    PurchaseDate = new DateOnly(2024, 5, 1),
                    Mileage = 8000,
                    LastMaintenanceDate = new DateOnly(2024, 9, 1),
                    NextMaintenanceDate = new DateOnly(2024, 11, 1),
                    LastMaintenanceMileage = 5000,
                    NextMaintenanceMileage = 15000,
                    BatteryHealthPercent = 99.5m,
                    VehicleCondition = "Excellent",
                    InsuranceNumber = "BYD-INS-001",
                    InsuranceExpiry = new DateOnly(2025, 5, 1),
                    RegistrationExpiry = new DateOnly(2025, 5, 1),
                    Notes = "Xe mới, còn bảo hành",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow.AddMonths(-5)
                });
            }

            // Customer 3 - BYD Seal
            if (customers.Count > 2 && seal != null)
            {
                vehicles.Add(new CustomerVehicle
                {
                    CustomerId = customers[2].CustomerId,
                    ModelId = seal.ModelId,
                    LicensePlate = "92A-33333",
                    Vin = "LGXC66EF0N0000001",
                    Color = "Xanh dương",
                    PurchaseDate = new DateOnly(2023, 10, 10),
                    Mileage = 18000,
                    LastMaintenanceDate = new DateOnly(2024, 8, 10),
                    NextMaintenanceDate = new DateOnly(2024, 10, 10),
                    LastMaintenanceMileage = 15000,
                    NextMaintenanceMileage = 25000,
                    BatteryHealthPercent = 97.0m,
                    VehicleCondition = "Good",
                    InsuranceNumber = "BYD-INS-002",
                    InsuranceExpiry = new DateOnly(2024, 10, 10),
                    RegistrationExpiry = new DateOnly(2024, 10, 10),
                    Notes = "Cần kiểm tra bảo hiểm sắp hết hạn",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow.AddMonths(-12)
                });
            }

            // Customer 3 - Hyundai IONIQ 5
            if (customers.Count > 2 && ioniq5 != null)
            {
                vehicles.Add(new CustomerVehicle
                {
                    CustomerId = customers[2].CustomerId,
                    ModelId = ioniq5.ModelId,
                    LicensePlate = "43D-44444",
                    Vin = "KMHC081ABPU000001",
                    Color = "Trắng ngọc trai",
                    PurchaseDate = new DateOnly(2024, 2, 14),
                    Mileage = 12000,
                    LastMaintenanceDate = new DateOnly(2024, 8, 14),
                    NextMaintenanceDate = new DateOnly(2025, 2, 14),
                    LastMaintenanceMileage = 10000,
                    NextMaintenanceMileage = 20000,
                    BatteryHealthPercent = 98.8m,
                    VehicleCondition = "Excellent",
                    InsuranceNumber = "HYU-INS-001",
                    InsuranceExpiry = new DateOnly(2025, 2, 14),
                    RegistrationExpiry = new DateOnly(2025, 2, 14),
                    Notes = "Xe công ty, sử dụng cho khách hàng VIP",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow.AddMonths(-8)
                });
            }

            // Customer 4 - Hyundai IONIQ 6
            if (customers.Count > 3 && ioniq6 != null)
            {
                vehicles.Add(new CustomerVehicle
                {
                    CustomerId = customers[3].CustomerId,
                    ModelId = ioniq6.ModelId,
                    LicensePlate = "77E-55555",
                    Vin = "KMHC082ABPU000001",
                    Color = "Đen",
                    PurchaseDate = new DateOnly(2023, 1, 5),
                    Mileage = 32000,
                    LastMaintenanceDate = new DateOnly(2024, 1, 5),
                    NextMaintenanceDate = new DateOnly(2024, 7, 5),
                    LastMaintenanceMileage = 20000,
                    NextMaintenanceMileage = 30000,
                    BatteryHealthPercent = 92.5m,
                    VehicleCondition = "Fair",
                    InsuranceNumber = "HYU-INS-002",
                    InsuranceExpiry = new DateOnly(2025, 1, 5),
                    RegistrationExpiry = new DateOnly(2025, 1, 5),
                    Notes = "CẦN BẢO DƯỠNG GẤP - đã quá hạn",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow.AddMonths(-21)
                });
            }

            // Customer 4 - BMW iX
            if (customers.Count > 3 && ix != null)
            {
                vehicles.Add(new CustomerVehicle
                {
                    CustomerId = customers[3].CustomerId,
                    ModelId = ix.ModelId,
                    LicensePlate = "59G-66666",
                    Vin = "WBY11AK07N7A00001",
                    Color = "Bạc",
                    PurchaseDate = new DateOnly(2022, 5, 20),
                    Mileage = 45000,
                    LastMaintenanceDate = new DateOnly(2024, 5, 20),
                    NextMaintenanceDate = null,
                    LastMaintenanceMileage = 40000,
                    NextMaintenanceMileage = null,
                    BatteryHealthPercent = 88.0m,
                    VehicleCondition = "Fair",
                    InsuranceNumber = "BMW-INS-001",
                    InsuranceExpiry = new DateOnly(2024, 5, 20),
                    RegistrationExpiry = new DateOnly(2024, 5, 20),
                    Notes = "Xe đã bán, chờ xóa khỏi hệ thống",
                    IsActive = false,
                    CreatedDate = DateTime.UtcNow.AddMonths(-30)
                });
            }

            // Customer 5 - BMW i4
            if (customers.Count > 4 && i4 != null)
            {
                vehicles.Add(new CustomerVehicle
                {
                    CustomerId = customers[4].CustomerId,
                    ModelId = i4.ModelId,
                    LicensePlate = "50H-77777",
                    Vin = "WBA63AP04N7B00001",
                    Color = "Xanh navy",
                    PurchaseDate = new DateOnly(2024, 3, 10),
                    Mileage = 7500,
                    LastMaintenanceDate = new DateOnly(2024, 9, 10),
                    NextMaintenanceDate = new DateOnly(2025, 3, 10),
                    LastMaintenanceMileage = 5000,
                    NextMaintenanceMileage = 15000,
                    BatteryHealthPercent = 99.2m,
                    VehicleCondition = "Excellent",
                    InsuranceNumber = "BMW-INS-002",
                    InsuranceExpiry = new DateOnly(2025, 3, 10),
                    RegistrationExpiry = new DateOnly(2025, 3, 10),
                    Notes = "Xe thể thao, hiệu suất cao",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow.AddMonths(-7)
                });
            }

            // Customer 5 - VinFast VF 6
            if (customers.Count > 4 && vf6 != null)
            {
                vehicles.Add(new CustomerVehicle
                {
                    CustomerId = customers[4].CustomerId,
                    ModelId = vf6.ModelId,
                    LicensePlate = "61C-88888",
                    Vin = "LVSHFAMB6NE000001",
                    Color = "Đỏ burgundy",
                    PurchaseDate = new DateOnly(2024, 6, 1),
                    Mileage = 3500,
                    LastMaintenanceDate = null,
                    NextMaintenanceDate = new DateOnly(2024, 12, 1),
                    LastMaintenanceMileage = null,
                    NextMaintenanceMileage = 10000,
                    BatteryHealthPercent = 100.0m,
                    VehicleCondition = "New",
                    InsuranceNumber = "VIN-INS-001",
                    InsuranceExpiry = new DateOnly(2025, 6, 1),
                    RegistrationExpiry = new DateOnly(2025, 6, 1),
                    Notes = "Xe mới hoàn toàn, chưa bảo dưỡng lần nào",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow.AddMonths(-4)
                });
            }

            if (!vehicles.Any())
            {
                Console.WriteLine("⚠️ No vehicles could be created. Check customers and models data.");
                return;
            }

            context.CustomerVehicles.AddRange(vehicles);
            context.SaveChanges();

            Console.WriteLine($"✓ Seeded {vehicles.Count} customer vehicles successfully");
            Console.WriteLine($"  - Customers used: {vehicles.Select(v => v.CustomerId).Distinct().Count()}");
            Console.WriteLine($"  - Models used: {vehicles.Select(v => v.ModelId).Distinct().Count()}");
        }
    }
}