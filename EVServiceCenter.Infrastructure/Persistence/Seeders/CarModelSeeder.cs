using EVServiceCenter.Core.Domains.CarModels.Entities;
using EVServiceCenter.Core.Entities;

namespace EVServiceCenter.Infrastructure.Persistence.Seeders
{
    public static class CarModelSeeder
    {
        public static void SeedCarModels(EVDbContext context)
        {
            if (context.CarModels.Any())
                return;

            // Get brands
            var vinfast = context.CarBrands.FirstOrDefault(b => b.BrandName == "VinFast");
            var tesla = context.CarBrands.FirstOrDefault(b => b.BrandName == "Tesla");
            var byd = context.CarBrands.FirstOrDefault(b => b.BrandName == "BYD");
            var hyundai = context.CarBrands.FirstOrDefault(b => b.BrandName == "Hyundai");
            var bmw = context.CarBrands.FirstOrDefault(b => b.BrandName == "BMW");

            var models = new List<CarModel>();

            // VinFast Models
            if (vinfast != null)
            {
                models.AddRange(new[]
                {
                    new CarModel
                    {
                        BrandId = vinfast.BrandId,
                        ModelName = "VF 8",
                        Year = 2023,
                        BatteryCapacity = 87.7m,
                        MaxRange = 447,
                        ChargingType = "DC Fast Charging",
                        MotorPower = 300m,
                        AccelerationTime = 5.5m,
                        TopSpeed = 200,
                        ServiceInterval = 10000,
                        ServiceIntervalMonths = 12,
                        WarrantyPeriod = 120,
                        ImageUrl = "https://example.com/images/vf8.jpg",
                        Description = "SUV điện hạng trung với thiết kế hiện đại và công nghệ tiên tiến",
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    },
                    new CarModel
                    {
                        BrandId = vinfast.BrandId,
                        ModelName = "VF 9",
                        Year = 2023,
                        BatteryCapacity = 123m,
                        MaxRange = 594,
                        ChargingType = "DC Fast Charging",
                        MotorPower = 300m,
                        AccelerationTime = 6.5m,
                        TopSpeed = 200,
                        ServiceInterval = 10000,
                        ServiceIntervalMonths = 12,
                        WarrantyPeriod = 120,
                        ImageUrl = "https://example.com/images/vf9.jpg",
                        Description = "SUV điện 7 chỗ cao cấp với không gian rộng rãi",
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    },
                    new CarModel
                    {
                        BrandId = vinfast.BrandId,
                        ModelName = "VF e34",
                        Year = 2022,
                        BatteryCapacity = 42m,
                        MaxRange = 285,
                        ChargingType = "AC/DC Charging",
                        MotorPower = 110m,
                        AccelerationTime = 9.5m,
                        TopSpeed = 145,
                        ServiceInterval = 10000,
                        ServiceIntervalMonths = 12,
                        WarrantyPeriod = 96,
                        ImageUrl = "https://example.com/images/vfe34.jpg",
                        Description = "SUV điện cỡ nhỏ giá cả phải chăng",
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    },
                    new CarModel
                    {
                        BrandId = vinfast.BrandId,
                        ModelName = "VF 5",
                        Year = 2023,
                        BatteryCapacity = 37.3m,
                        MaxRange = 326,
                        ChargingType = "AC/DC Charging",
                        MotorPower = 100m,
                        AccelerationTime = 9.0m,
                        TopSpeed = 140,
                        ServiceInterval = 10000,
                        ServiceIntervalMonths = 12,
                        WarrantyPeriod = 96,
                        ImageUrl = "https://example.com/images/vf5.jpg",
                        Description = "SUV điện cỡ A phù hợp cho đô thị",
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    }
                });
            }

            // Tesla Models
            if (tesla != null)
            {
                models.AddRange(new[]
                {
                    new CarModel
                    {
                        BrandId = tesla.BrandId,
                        ModelName = "Model 3",
                        Year = 2024,
                        BatteryCapacity = 82m,
                        MaxRange = 567,
                        ChargingType = "Supercharger",
                        MotorPower = 283m,
                        AccelerationTime = 3.1m,
                        TopSpeed = 261,
                        ServiceInterval = 20000,
                        ServiceIntervalMonths = 12,
                        WarrantyPeriod = 96,
                        ImageUrl = "https://example.com/images/model3.jpg",
                        Description = "Sedan điện bán chạy nhất thế giới",
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    },
                    new CarModel
                    {
                        BrandId = tesla.BrandId,
                        ModelName = "Model Y",
                        Year = 2024,
                        BatteryCapacity = 75m,
                        MaxRange = 533,
                        ChargingType = "Supercharger",
                        MotorPower = 299m,
                        AccelerationTime = 3.5m,
                        TopSpeed = 217,
                        ServiceInterval = 20000,
                        ServiceIntervalMonths = 12,
                        WarrantyPeriod = 96,
                        ImageUrl = "https://example.com/images/modely.jpg",
                        Description = "SUV điện compact với hiệu suất cao",
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    },
                    new CarModel
                    {
                        BrandId = tesla.BrandId,
                        ModelName = "Model S",
                        Year = 2024,
                        BatteryCapacity = 100m,
                        MaxRange = 652,
                        ChargingType = "Supercharger",
                        MotorPower = 670m,
                        AccelerationTime = 2.1m,
                        TopSpeed = 322,
                        ServiceInterval = 20000,
                        ServiceIntervalMonths = 12,
                        WarrantyPeriod = 96,
                        ImageUrl = "https://example.com/images/models.jpg",
                        Description = "Sedan điện cao cấp với hiệu suất đỉnh cao",
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    }
                });
            }

            // BYD Models
            if (byd != null)
            {
                models.AddRange(new[]
                {
                    new CarModel
                    {
                        BrandId = byd.BrandId,
                        ModelName = "Atto 3",
                        Year = 2023,
                        BatteryCapacity = 60.48m,
                        MaxRange = 420,
                        ChargingType = "DC Fast Charging",
                        MotorPower = 150m,
                        AccelerationTime = 7.3m,
                        TopSpeed = 160,
                        ServiceInterval = 10000,
                        ServiceIntervalMonths = 12,
                        WarrantyPeriod = 96,
                        ImageUrl = "https://example.com/images/atto3.jpg",
                        Description = "SUV điện cỡ B với công nghệ pin Blade",
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    },
                    new CarModel
                    {
                        BrandId = byd.BrandId,
                        ModelName = "Seal",
                        Year = 2023,
                        BatteryCapacity = 82.56m,
                        MaxRange = 570,
                        ChargingType = "DC Fast Charging",
                        MotorPower = 230m,
                        AccelerationTime = 3.8m,
                        TopSpeed = 180,
                        ServiceInterval = 10000,
                        ServiceIntervalMonths = 12,
                        WarrantyPeriod = 96,
                        ImageUrl = "https://example.com/images/seal.jpg",
                        Description = "Sedan điện thể thao với thiết kế ấn tượng",
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    }
                });
            }

            // Hyundai Models
            if (hyundai != null)
            {
                models.AddRange(new[]
                {
                    new CarModel
                    {
                        BrandId = hyundai.BrandId,
                        ModelName = "IONIQ 5",
                        Year = 2024,
                        BatteryCapacity = 77.4m,
                        MaxRange = 481,
                        ChargingType = "800V Fast Charging",
                        MotorPower = 225m,
                        AccelerationTime = 5.2m,
                        TopSpeed = 185,
                        ServiceInterval = 15000,
                        ServiceIntervalMonths = 12,
                        WarrantyPeriod = 96,
                        ImageUrl = "https://example.com/images/ioniq5.jpg",
                        Description = "SUV điện với thiết kế retro-futuristic",
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    },
                    new CarModel
                    {
                        BrandId = hyundai.BrandId,
                        ModelName = "IONIQ 6",
                        Year = 2024,
                        BatteryCapacity = 77.4m,
                        MaxRange = 614,
                        ChargingType = "800V Fast Charging",
                        MotorPower = 225m,
                        AccelerationTime = 5.1m,
                        TopSpeed = 185,
                        ServiceInterval = 15000,
                        ServiceIntervalMonths = 12,
                        WarrantyPeriod = 96,
                        ImageUrl = "https://example.com/images/ioniq6.jpg",
                        Description = "Sedan điện với hệ số cản gió ấn tượng",
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    }
                });
            }

            // BMW Models
            if (bmw != null)
            {
                models.AddRange(new[]
                {
                    new CarModel
                    {
                        BrandId = bmw.BrandId,
                        ModelName = "iX",
                        Year = 2024,
                        BatteryCapacity = 111.5m,
                        MaxRange = 630,
                        ChargingType = "DC Fast Charging",
                        MotorPower = 385m,
                        AccelerationTime = 4.6m,
                        TopSpeed = 200,
                        ServiceInterval = 15000,
                        ServiceIntervalMonths = 12,
                        WarrantyPeriod = 96,
                        ImageUrl = "https://example.com/images/ix.jpg",
                        Description = "SUV điện cao cấp với công nghệ hiện đại",
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    },
                    new CarModel
                    {
                        BrandId = bmw.BrandId,
                        ModelName = "i4",
                        Year = 2024,
                        BatteryCapacity = 83.9m,
                        MaxRange = 590,
                        ChargingType = "DC Fast Charging",
                        MotorPower = 250m,
                        AccelerationTime = 5.7m,
                        TopSpeed = 190,
                        ServiceInterval = 15000,
                        ServiceIntervalMonths = 12,
                        WarrantyPeriod = 96,
                        ImageUrl = "https://example.com/images/i4.jpg",
                        Description = "Gran Coupe điện thể thao sang trọng",
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    }
                });
            }

            context.CarModels.AddRange(models);
            context.SaveChanges();
        }
    }
}