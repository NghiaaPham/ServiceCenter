using EVServiceCenter.Core.Domains.CarModels.Entities;
using EVServiceCenter.Core.Entities;

namespace EVServiceCenter.Infrastructure.Persistence.Seeders
{
    public static class CarModelSeeder
    {
        public static void SeedCarModels(EVDbContext context)
        {
            // Get brands
            var vinfast = context.CarBrands.FirstOrDefault(b => b.BrandName == "VinFast");
            var tesla = context.CarBrands.FirstOrDefault(b => b.BrandName == "Tesla");
            var byd = context.CarBrands.FirstOrDefault(b => b.BrandName == "BYD");
            var hyundai = context.CarBrands.FirstOrDefault(b => b.BrandName == "Hyundai");
            var bmw = context.CarBrands.FirstOrDefault(b => b.BrandName == "BMW");
            var audi = context.CarBrands.FirstOrDefault(b => b.BrandName == "Audi");

            var models = new List<CarModel>();

            // ========== VinFast Models ==========
            if (vinfast != null)
            {
                // VF 8
                if (!context.CarModels.Any(m => m.ModelName == "VF 8" && m.BrandId == vinfast.BrandId))
                {
                    models.Add(new CarModel
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
                    });
                }

                // VF 9
                if (!context.CarModels.Any(m => m.ModelName == "VF 9" && m.BrandId == vinfast.BrandId))
                {
                    models.Add(new CarModel
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
                    });
                }

                // VF e34
                if (!context.CarModels.Any(m => m.ModelName == "VF e34" && m.BrandId == vinfast.BrandId))
                {
                    models.Add(new CarModel
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
                    });
                }

                // VF 5
                if (!context.CarModels.Any(m => m.ModelName == "VF 5" && m.BrandId == vinfast.BrandId))
                {
                    models.Add(new CarModel
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
                    });
                }
            }

            // ========== Tesla Models ==========
            if (tesla != null)
            {
                // Model 3
                if (!context.CarModels.Any(m => m.ModelName == "Model 3" && m.BrandId == tesla.BrandId))
                {
                    models.Add(new CarModel
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
                    });
                }

                // Model Y
                if (!context.CarModels.Any(m => m.ModelName == "Model Y" && m.BrandId == tesla.BrandId))
                {
                    models.Add(new CarModel
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
                    });
                }

                // Model S
                if (!context.CarModels.Any(m => m.ModelName == "Model S" && m.BrandId == tesla.BrandId))
                {
                    models.Add(new CarModel
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
                    });
                }
            }

            // ========== BYD Models ==========
            if (byd != null)
            {
                // Atto 3
                if (!context.CarModels.Any(m => m.ModelName == "Atto 3" && m.BrandId == byd.BrandId))
                {
                    models.Add(new CarModel
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
                    });
                }

                // Seal
                if (!context.CarModels.Any(m => m.ModelName == "Seal" && m.BrandId == byd.BrandId))
                {
                    models.Add(new CarModel
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
                    });
                }
            }

            // ========== Hyundai Models ==========
            if (hyundai != null)
            {
                // IONIQ 5
                if (!context.CarModels.Any(m => m.ModelName == "IONIQ 5" && m.BrandId == hyundai.BrandId))
                {
                    models.Add(new CarModel
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
                    });
                }

                // IONIQ 6
                if (!context.CarModels.Any(m => m.ModelName == "IONIQ 6" && m.BrandId == hyundai.BrandId))
                {
                    models.Add(new CarModel
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
                    });
                }
            }

            // ========== BMW Models ==========
            if (bmw != null)
            {
                // iX
                if (!context.CarModels.Any(m => m.ModelName == "iX" && m.BrandId == bmw.BrandId))
                {
                    models.Add(new CarModel
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
                    });
                }

                // i4
                if (!context.CarModels.Any(m => m.ModelName == "i4" && m.BrandId == bmw.BrandId))
                {
                    models.Add(new CarModel
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
                    });
                }

                // i3 - MỚI
                if (!context.CarModels.Any(m => m.ModelName == "i3" && m.BrandId == bmw.BrandId))
                {
                    models.Add(new CarModel
                    {
                        BrandId = bmw.BrandId,
                        ModelName = "i3",
                        Year = 2022,
                        BatteryCapacity = 42.2m,
                        MaxRange = 307,
                        ChargingType = "DC Fast Charging",
                        MotorPower = 125m,
                        AccelerationTime = 7.3m,
                        TopSpeed = 150,
                        ServiceInterval = 15000,
                        ServiceIntervalMonths = 12,
                        WarrantyPeriod = 96,
                        ImageUrl = "https://example.com/images/i3.jpg",
                        Description = "Xe điện compact thành phố, thiết kế độc đáo",
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    });
                }

                // i7 - MỚI
                if (!context.CarModels.Any(m => m.ModelName == "i7" && m.BrandId == bmw.BrandId))
                {
                    models.Add(new CarModel
                    {
                        BrandId = bmw.BrandId,
                        ModelName = "i7",
                        Year = 2024,
                        BatteryCapacity = 101.7m,
                        MaxRange = 625,
                        ChargingType = "DC Fast Charging",
                        MotorPower = 400m,
                        AccelerationTime = 4.7m,
                        TopSpeed = 240,
                        ServiceInterval = 15000,
                        ServiceIntervalMonths = 12,
                        WarrantyPeriod = 96,
                        ImageUrl = "https://example.com/images/i7.jpg",
                        Description = "Sedan hạng sang điện flagship với công nghệ đỉnh cao",
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    });
                }
            }

            // ========== Audi Models - MỚI ==========
            if (audi != null)
            {
                // e-tron
                if (!context.CarModels.Any(m => m.ModelName == "e-tron" && m.BrandId == audi.BrandId))
                {
                    models.Add(new CarModel
                    {
                        BrandId = audi.BrandId,
                        ModelName = "e-tron",
                        Year = 2024,
                        BatteryCapacity = 95m,
                        MaxRange = 582,
                        ChargingType = "DC Fast Charging",
                        MotorPower = 300m,
                        AccelerationTime = 5.7m,
                        TopSpeed = 200,
                        ServiceInterval = 15000,
                        ServiceIntervalMonths = 12,
                        WarrantyPeriod = 96,
                        ImageUrl = "https://example.com/images/etron.jpg",
                        Description = "SUV điện cao cấp với công nghệ quattro electric",
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    });
                }

                // Q4 e-tron
                if (!context.CarModels.Any(m => m.ModelName == "Q4 e-tron" && m.BrandId == audi.BrandId))
                {
                    models.Add(new CarModel
                    {
                        BrandId = audi.BrandId,
                        ModelName = "Q4 e-tron",
                        Year = 2024,
                        BatteryCapacity = 77m,
                        MaxRange = 520,
                        ChargingType = "DC Fast Charging",
                        MotorPower = 220m,
                        AccelerationTime = 6.2m,
                        TopSpeed = 180,
                        ServiceInterval = 15000,
                        ServiceIntervalMonths = 12,
                        WarrantyPeriod = 96,
                        ImageUrl = "https://example.com/images/q4etron.jpg",
                        Description = "SUV compact điện với thiết kế hiện đại",
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    });
                }

                // RS e-tron GT
                if (!context.CarModels.Any(m => m.ModelName == "RS e-tron GT" && m.BrandId == audi.BrandId))
                {
                    models.Add(new CarModel
                    {
                        BrandId = audi.BrandId,
                        ModelName = "RS e-tron GT",
                        Year = 2024,
                        BatteryCapacity = 93.4m,
                        MaxRange = 481,
                        ChargingType = "800V Fast Charging",
                        MotorPower = 475m,
                        AccelerationTime = 3.3m,
                        TopSpeed = 250,
                        ServiceInterval = 15000,
                        ServiceIntervalMonths = 12,
                        WarrantyPeriod = 96,
                        ImageUrl = "https://example.com/images/rsetrongt.jpg",
                        Description = "Gran Turismo điện hiệu năng cao với 637 hp",
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    });
                }
            }

            // Add new models to database
            if (models.Any())
            {
                context.CarModels.AddRange(models);
                context.SaveChanges();
                Console.WriteLine($"✓ Seeded {models.Count} car models (new or existing)");
            }
            else
            {
                Console.WriteLine("✓ All car models already exist, skipping...");
            }
        }
    }
}