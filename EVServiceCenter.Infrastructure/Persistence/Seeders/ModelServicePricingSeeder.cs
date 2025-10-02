using EVServiceCenter.Core.Entities;

namespace EVServiceCenter.Infrastructure.Persistence.Seeders
{
    public static class ModelServicePricingSeeder
    {
        public static void SeedModelServicePricings(EVDbContext context)
        {
            // Get BMW Models
            var bmwI3 = context.CarModels.FirstOrDefault(m => m.ModelName == "i3")?.ModelId ?? 0;
            var bmwI4 = context.CarModels.FirstOrDefault(m => m.ModelName == "i4")?.ModelId ?? 0;
            var bmwI7 = context.CarModels.FirstOrDefault(m => m.ModelName == "i7")?.ModelId ?? 0;
            var bmwIx = context.CarModels.FirstOrDefault(m => m.ModelName == "iX")?.ModelId ?? 0;

            // Get Audi Models
            var audiEtron = context.CarModels.FirstOrDefault(m => m.ModelName == "e-tron")?.ModelId ?? 0;
            var audiQ4Etron = context.CarModels.FirstOrDefault(m => m.ModelName == "Q4 e-tron")?.ModelId ?? 0;
            var audiRsEtronGt = context.CarModels.FirstOrDefault(m => m.ModelName == "RS e-tron GT")?.ModelId ?? 0;

            // Get Services
            var baoDuong10k = context.MaintenanceServices.FirstOrDefault(s => s.ServiceCode == "BD-10K")?.ServiceId ?? 0;
            var baoDuong20k = context.MaintenanceServices.FirstOrDefault(s => s.ServiceCode == "BD-20K")?.ServiceId ?? 0;
            var pinCheck = context.MaintenanceServices.FirstOrDefault(s => s.ServiceCode == "PIN-CHECK")?.ServiceId ?? 0;
            var phanhReplace = context.MaintenanceServices.FirstOrDefault(s => s.ServiceCode == "PHANH-REPLACE")?.ServiceId ?? 0;
            var acService = context.MaintenanceServices.FirstOrDefault(s => s.ServiceCode == "AC-SERVICE")?.ServiceId ?? 0;
            var diagFull = context.MaintenanceServices.FirstOrDefault(s => s.ServiceCode == "DIAG-FULL")?.ServiceId ?? 0;

            var pricings = new List<ModelServicePricing>();

            // ========== BMW i3 - Compact Electric ==========
            if (bmwI3 > 0 && baoDuong10k > 0 && !context.ModelServicePricings.Any(p => p.ModelId == bmwI3 && p.ServiceId == baoDuong10k))
            {
                pricings.Add(new ModelServicePricing
                {
                    ModelId = bmwI3,
                    ServiceId = baoDuong10k,
                    CustomPrice = 800000,
                    CustomTime = 60,
                    Notes = "BMW i3 - xe điện compact, chi phí bảo dưỡng thấp",
                    IsActive = true,
                    EffectiveDate = DateOnly.FromDateTime(DateTime.Today),
                    CreatedDate = DateTime.UtcNow
                });
            }

            if (bmwI3 > 0 && pinCheck > 0 && !context.ModelServicePricings.Any(p => p.ModelId == bmwI3 && p.ServiceId == pinCheck))
            {
                pricings.Add(new ModelServicePricing
                {
                    ModelId = bmwI3,
                    ServiceId = pinCheck,
                    CustomPrice = 500000,
                    CustomTime = 50,
                    Notes = "BMW i3 - pin compact, kiểm tra đơn giản",
                    IsActive = true,
                    EffectiveDate = DateOnly.FromDateTime(DateTime.Today),
                    CreatedDate = DateTime.UtcNow
                });
            }

            if (bmwI3 > 0 && phanhReplace > 0 && !context.ModelServicePricings.Any(p => p.ModelId == bmwI3 && p.ServiceId == phanhReplace))
            {
                pricings.Add(new ModelServicePricing
                {
                    ModelId = bmwI3,
                    ServiceId = phanhReplace,
                    CustomPrice = 2200000,
                    CustomTime = 70,
                    Notes = "BMW i3 - phanh ceramic cao cấp",
                    IsActive = true,
                    EffectiveDate = DateOnly.FromDateTime(DateTime.Today),
                    CreatedDate = DateTime.UtcNow
                });
            }

            // ========== BMW i4 - Sports Sedan ==========
            if (bmwI4 > 0 && baoDuong10k > 0 && !context.ModelServicePricings.Any(p => p.ModelId == bmwI4 && p.ServiceId == baoDuong10k))
            {
                pricings.Add(new ModelServicePricing
                {
                    ModelId = bmwI4,
                    ServiceId = baoDuong10k,
                    CustomPrice = 1200000,
                    CustomTime = 75,
                    Notes = "BMW i4 - sedan thể thao cao cấp",
                    IsActive = true,
                    EffectiveDate = DateOnly.FromDateTime(DateTime.Today),
                    CreatedDate = DateTime.UtcNow
                });
            }

            if (bmwI4 > 0 && baoDuong20k > 0 && !context.ModelServicePricings.Any(p => p.ModelId == bmwI4 && p.ServiceId == baoDuong20k))
            {
                pricings.Add(new ModelServicePricing
                {
                    ModelId = bmwI4,
                    ServiceId = baoDuong20k,
                    CustomPrice = 2500000,
                    CustomTime = 150,
                    Notes = "BMW i4 - bảo dưỡng toàn diện",
                    IsActive = true,
                    EffectiveDate = DateOnly.FromDateTime(DateTime.Today),
                    CreatedDate = DateTime.UtcNow
                });
            }

            if (bmwI4 > 0 && phanhReplace > 0 && !context.ModelServicePricings.Any(p => p.ModelId == bmwI4 && p.ServiceId == phanhReplace))
            {
                pricings.Add(new ModelServicePricing
                {
                    ModelId = bmwI4,
                    ServiceId = phanhReplace,
                    CustomPrice = 3000000,
                    CustomTime = 80,
                    Notes = "BMW i4 - phanh hiệu năng cao M Sport",
                    IsActive = true,
                    EffectiveDate = DateOnly.FromDateTime(DateTime.Today),
                    CreatedDate = DateTime.UtcNow
                });
            }

            if (bmwI4 > 0 && diagFull > 0 && !context.ModelServicePricings.Any(p => p.ModelId == bmwI4 && p.ServiceId == diagFull))
            {
                pricings.Add(new ModelServicePricing
                {
                    ModelId = bmwI4,
                    ServiceId = diagFull,
                    CustomPrice = 800000,
                    CustomTime = 70,
                    Notes = "BMW i4 - chẩn đoán hệ thống phức tạp",
                    IsActive = true,
                    EffectiveDate = DateOnly.FromDateTime(DateTime.Today),
                    CreatedDate = DateTime.UtcNow
                });
            }

            // ========== BMW iX - Luxury SUV ==========
            if (bmwIx > 0 && baoDuong10k > 0 && !context.ModelServicePricings.Any(p => p.ModelId == bmwIx && p.ServiceId == baoDuong10k))
            {
                pricings.Add(new ModelServicePricing
                {
                    ModelId = bmwIx,
                    ServiceId = baoDuong10k,
                    CustomPrice = 1500000,
                    CustomTime = 90,
                    Notes = "BMW iX - SUV điện cao cấp nhất",
                    IsActive = true,
                    EffectiveDate = DateOnly.FromDateTime(DateTime.Today),
                    CreatedDate = DateTime.UtcNow
                });
            }

            if (bmwIx > 0 && baoDuong20k > 0 && !context.ModelServicePricings.Any(p => p.ModelId == bmwIx && p.ServiceId == baoDuong20k))
            {
                pricings.Add(new ModelServicePricing
                {
                    ModelId = bmwIx,
                    ServiceId = baoDuong20k,
                    CustomPrice = 3000000,
                    CustomTime = 180,
                    Notes = "BMW iX - bảo dưỡng toàn diện cao cấp",
                    IsActive = true,
                    EffectiveDate = DateOnly.FromDateTime(DateTime.Today),
                    CreatedDate = DateTime.UtcNow
                });
            }

            if (bmwIx > 0 && pinCheck > 0 && !context.ModelServicePricings.Any(p => p.ModelId == bmwIx && p.ServiceId == pinCheck))
            {
                pricings.Add(new ModelServicePricing
                {
                    ModelId = bmwIx,
                    ServiceId = pinCheck,
                    CustomPrice = 700000,
                    CustomTime = 60,
                    Notes = "BMW iX - pin dung lượng lớn 100+ kWh",
                    IsActive = true,
                    EffectiveDate = DateOnly.FromDateTime(DateTime.Today),
                    CreatedDate = DateTime.UtcNow
                });
            }

            if (bmwIx > 0 && phanhReplace > 0 && !context.ModelServicePricings.Any(p => p.ModelId == bmwIx && p.ServiceId == phanhReplace))
            {
                pricings.Add(new ModelServicePricing
                {
                    ModelId = bmwIx,
                    ServiceId = phanhReplace,
                    CustomPrice = 3500000,
                    CustomTime = 90,
                    Notes = "BMW iX - phanh kích thước lớn với công nghệ regenerative",
                    IsActive = true,
                    EffectiveDate = DateOnly.FromDateTime(DateTime.Today),
                    CreatedDate = DateTime.UtcNow
                });
            }

            // ========== BMW i7 - Flagship Sedan ==========
            if (bmwI7 > 0 && baoDuong10k > 0 && !context.ModelServicePricings.Any(p => p.ModelId == bmwI7 && p.ServiceId == baoDuong10k))
            {
                pricings.Add(new ModelServicePricing
                {
                    ModelId = bmwI7,
                    ServiceId = baoDuong10k,
                    CustomPrice = 1800000,
                    CustomTime = 100,
                    Notes = "BMW i7 - flagship sedan siêu sang",
                    IsActive = true,
                    EffectiveDate = DateOnly.FromDateTime(DateTime.Today),
                    CreatedDate = DateTime.UtcNow
                });
            }

            if (bmwI7 > 0 && baoDuong20k > 0 && !context.ModelServicePricings.Any(p => p.ModelId == bmwI7 && p.ServiceId == baoDuong20k))
            {
                pricings.Add(new ModelServicePricing
                {
                    ModelId = bmwI7,
                    ServiceId = baoDuong20k,
                    CustomPrice = 3500000,
                    CustomTime = 200,
                    Notes = "BMW i7 - bảo dưỡng toàn diện ultra-luxury",
                    IsActive = true,
                    EffectiveDate = DateOnly.FromDateTime(DateTime.Today),
                    CreatedDate = DateTime.UtcNow
                });
            }

            if (bmwI7 > 0 && acService > 0 && !context.ModelServicePricings.Any(p => p.ModelId == bmwI7 && p.ServiceId == acService))
            {
                pricings.Add(new ModelServicePricing
                {
                    ModelId = bmwI7,
                    ServiceId = acService,
                    CustomPrice = 2000000,
                    CustomTime = 120,
                    Notes = "BMW i7 - hệ thống điều hòa 4 vùng cao cấp",
                    IsActive = true,
                    EffectiveDate = DateOnly.FromDateTime(DateTime.Today),
                    CreatedDate = DateTime.UtcNow
                });
            }

            // ========== AUDI e-tron - Luxury SUV ==========
            if (audiEtron > 0 && baoDuong10k > 0 && !context.ModelServicePricings.Any(p => p.ModelId == audiEtron && p.ServiceId == baoDuong10k))
            {
                pricings.Add(new ModelServicePricing
                {
                    ModelId = audiEtron,
                    ServiceId = baoDuong10k,
                    CustomPrice = 1400000,
                    CustomTime = 85,
                    Notes = "Audi e-tron - SUV điện hạng sang",
                    IsActive = true,
                    EffectiveDate = DateOnly.FromDateTime(DateTime.Today),
                    CreatedDate = DateTime.UtcNow
                });
            }

            if (audiEtron > 0 && baoDuong20k > 0 && !context.ModelServicePricings.Any(p => p.ModelId == audiEtron && p.ServiceId == baoDuong20k))
            {
                pricings.Add(new ModelServicePricing
                {
                    ModelId = audiEtron,
                    ServiceId = baoDuong20k,
                    CustomPrice = 2800000,
                    CustomTime = 170,
                    Notes = "Audi e-tron - bảo dưỡng quattro electric",
                    IsActive = true,
                    EffectiveDate = DateOnly.FromDateTime(DateTime.Today),
                    CreatedDate = DateTime.UtcNow
                });
            }

            if (audiEtron > 0 && phanhReplace > 0 && !context.ModelServicePricings.Any(p => p.ModelId == audiEtron && p.ServiceId == phanhReplace))
            {
                pricings.Add(new ModelServicePricing
                {
                    ModelId = audiEtron,
                    ServiceId = phanhReplace,
                    CustomPrice = 3200000,
                    CustomTime = 85,
                    Notes = "Audi e-tron - phanh 6 piston",
                    IsActive = true,
                    EffectiveDate = DateOnly.FromDateTime(DateTime.Today),
                    CreatedDate = DateTime.UtcNow
                });
            }

            // ========== AUDI Q4 e-tron - Compact SUV ==========
            if (audiQ4Etron > 0 && baoDuong10k > 0 && !context.ModelServicePricings.Any(p => p.ModelId == audiQ4Etron && p.ServiceId == baoDuong10k))
            {
                pricings.Add(new ModelServicePricing
                {
                    ModelId = audiQ4Etron,
                    ServiceId = baoDuong10k,
                    CustomPrice = 1000000,
                    CustomTime = 70,
                    Notes = "Audi Q4 e-tron - compact SUV điện",
                    IsActive = true,
                    EffectiveDate = DateOnly.FromDateTime(DateTime.Today),
                    CreatedDate = DateTime.UtcNow
                });
            }

            if (audiQ4Etron > 0 && pinCheck > 0 && !context.ModelServicePricings.Any(p => p.ModelId == audiQ4Etron && p.ServiceId == pinCheck))
            {
                pricings.Add(new ModelServicePricing
                {
                    ModelId = audiQ4Etron,
                    ServiceId = pinCheck,
                    CustomPrice = 550000,
                    CustomTime = 55,
                    Notes = "Audi Q4 e-tron - pin 77 kWh",
                    IsActive = true,
                    EffectiveDate = DateOnly.FromDateTime(DateTime.Today),
                    CreatedDate = DateTime.UtcNow
                });
            }

            if (audiQ4Etron > 0 && phanhReplace > 0 && !context.ModelServicePricings.Any(p => p.ModelId == audiQ4Etron && p.ServiceId == phanhReplace))
            {
                pricings.Add(new ModelServicePricing
                {
                    ModelId = audiQ4Etron,
                    ServiceId = phanhReplace,
                    CustomPrice = 2500000,
                    CustomTime = 75,
                    Notes = "Audi Q4 e-tron - phanh hiệu năng cao",
                    IsActive = true,
                    EffectiveDate = DateOnly.FromDateTime(DateTime.Today),
                    CreatedDate = DateTime.UtcNow
                });
            }

            // ========== AUDI RS e-tron GT - Performance ==========
            if (audiRsEtronGt > 0 && baoDuong10k > 0 && !context.ModelServicePricings.Any(p => p.ModelId == audiRsEtronGt && p.ServiceId == baoDuong10k))
            {
                pricings.Add(new ModelServicePricing
                {
                    ModelId = audiRsEtronGt,
                    ServiceId = baoDuong10k,
                    CustomPrice = 2000000,
                    CustomTime = 110,
                    Notes = "Audi RS e-tron GT - supercar điện hiệu năng cao",
                    IsActive = true,
                    EffectiveDate = DateOnly.FromDateTime(DateTime.Today),
                    CreatedDate = DateTime.UtcNow
                });
            }

            if (audiRsEtronGt > 0 && baoDuong20k > 0 && !context.ModelServicePricings.Any(p => p.ModelId == audiRsEtronGt && p.ServiceId == baoDuong20k))
            {
                pricings.Add(new ModelServicePricing
                {
                    ModelId = audiRsEtronGt,
                    ServiceId = baoDuong20k,
                    CustomPrice = 4000000,
                    CustomTime = 220,
                    Notes = "Audi RS e-tron GT - bảo dưỡng hiệu năng 600+ hp",
                    IsActive = true,
                    EffectiveDate = DateOnly.FromDateTime(DateTime.Today),
                    CreatedDate = DateTime.UtcNow
                });
            }

            if (audiRsEtronGt > 0 && phanhReplace > 0 && !context.ModelServicePricings.Any(p => p.ModelId == audiRsEtronGt && p.ServiceId == phanhReplace))
            {
                pricings.Add(new ModelServicePricing
                {
                    ModelId = audiRsEtronGt,
                    ServiceId = phanhReplace,
                    CustomPrice = 5000000,
                    CustomTime = 100,
                    Notes = "Audi RS e-tron GT - phanh carbon ceramic",
                    IsActive = true,
                    EffectiveDate = DateOnly.FromDateTime(DateTime.Today),
                    CreatedDate = DateTime.UtcNow
                });
            }

            if (audiRsEtronGt > 0 && diagFull > 0 && !context.ModelServicePricings.Any(p => p.ModelId == audiRsEtronGt && p.ServiceId == diagFull))
            {
                pricings.Add(new ModelServicePricing
                {
                    ModelId = audiRsEtronGt,
                    ServiceId = diagFull,
                    CustomPrice = 1000000,
                    CustomTime = 80,
                    Notes = "Audi RS e-tron GT - chẩn đoán hệ thống performance",
                    IsActive = true,
                    EffectiveDate = DateOnly.FromDateTime(DateTime.Today),
                    CreatedDate = DateTime.UtcNow
                });
            }

            // Add new pricings
            if (pricings.Any())
            {
                context.ModelServicePricings.AddRange(pricings);
                context.SaveChanges();
                Console.WriteLine($"✓ Seeded {pricings.Count} BMW & Audi model service pricings");
            }
            else
            {
                Console.WriteLine("✓ All model service pricings already exist, skipping...");
            }
        }
    }
}