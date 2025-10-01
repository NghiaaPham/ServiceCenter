
using EVServiceCenter.Core.Domains.CarBrands.Entities;
using EVServiceCenter.Core.Entities;

namespace EVServiceCenter.Infrastructure.Persistence.Seeders
{
    public static class CarBrandSeeder
    {
        public static void SeedCarBrands(EVDbContext context)
        {
            if (context.CarBrands.Any())
                return;

            var brands = new List<CarBrand>
            {
                // Vietnamese Brands
                new CarBrand
                {
                    BrandName = "VinFast",
                    Country = "Vietnam",
                    LogoUrl = "https://example.com/logos/vinfast.png",
                    Website = "https://vinfastauto.com",
                    Description = "Thương hiệu ô tô điện hàng đầu Việt Nam, thuộc Tập đoàn Vingroup",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                // American Brands
                new CarBrand
                {
                    BrandName = "Tesla",
                    Country = "United States",
                    LogoUrl = "https://example.com/logos/tesla.png",
                    Website = "https://tesla.com",
                    Description = "Nhà sản xuất xe điện và năng lượng sạch hàng đầu thế giới",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new CarBrand
                {
                    BrandName = "Chevrolet",
                    Country = "United States",
                    LogoUrl = "https://example.com/logos/chevrolet.png",
                    Website = "https://chevrolet.com",
                    Description = "Thương hiệu ô tô Mỹ với dòng xe điện Bolt",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                // Chinese Brands
                new CarBrand
                {
                    BrandName = "BYD",
                    Country = "China",
                    LogoUrl = "https://example.com/logos/byd.png",
                    Website = "https://bydauto.com",
                    Description = "Nhà sản xuất xe điện lớn nhất Trung Quốc với công nghệ pin Blade",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new CarBrand
                {
                    BrandName = "NIO",
                    Country = "China",
                    LogoUrl = "https://example.com/logos/nio.png",
                    Website = "https://nio.com",
                    Description = "Thương hiệu xe điện cao cấp của Trung Quốc",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new CarBrand
                {
                    BrandName = "XPeng",
                    Country = "China",
                    LogoUrl = "https://example.com/logos/xpeng.png",
                    Website = "https://xiaopeng.com",
                    Description = "Xe điện thông minh với công nghệ tự lái tiên tiến",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                // Korean Brands
                new CarBrand
                {
                    BrandName = "Hyundai",
                    Country = "South Korea",
                    LogoUrl = "https://example.com/logos/hyundai.png",
                    Website = "https://hyundai.com",
                    Description = "Thương hiệu ô tô Hàn Quốc với dòng xe điện IONIQ",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new CarBrand
                {
                    BrandName = "Kia",
                    Country = "South Korea",
                    LogoUrl = "https://example.com/logos/kia.png",
                    Website = "https://kia.com",
                    Description = "Thương hiệu Hàn Quốc với dòng xe điện EV6",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                // German Brands
                new CarBrand
                {
                    BrandName = "BMW",
                    Country = "Germany",
                    LogoUrl = "https://example.com/logos/bmw.png",
                    Website = "https://bmw.com",
                    Description = "Thương hiệu xe sang Đức với dòng xe điện i-Series",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new CarBrand
                {
                    BrandName = "Mercedes-Benz",
                    Country = "Germany",
                    LogoUrl = "https://example.com/logos/mercedes.png",
                    Website = "https://mercedes-benz.com",
                    Description = "Thương hiệu xe sang hạng sang với dòng EQ điện",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new CarBrand
                {
                    BrandName = "Audi",
                    Country = "Germany",
                    LogoUrl = "https://example.com/logos/audi.png",
                    Website = "https://audi.com",
                    Description = "Thương hiệu cao cấp Đức với dòng e-tron",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new CarBrand
                {
                    BrandName = "Porsche",
                    Country = "Germany",
                    LogoUrl = "https://example.com/logos/porsche.png",
                    Website = "https://porsche.com",
                    Description = "Thương hiệu xe thể thao sang trọng với Taycan điện",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                // Japanese Brands
                new CarBrand
                {
                    BrandName = "Nissan",
                    Country = "Japan",
                    LogoUrl = "https://example.com/logos/nissan.png",
                    Website = "https://nissan.com",
                    Description = "Thương hiệu Nhật với dòng xe điện Leaf nổi tiếng",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                // French Brands
                new CarBrand
                {
                    BrandName = "Peugeot",
                    Country = "France",
                    LogoUrl = "https://example.com/logos/peugeot.png",
                    Website = "https://peugeot.com",
                    Description = "Thương hiệu Pháp với dòng xe điện e-208, e-2008",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                // Swedish Brands
                new CarBrand
                {
                    BrandName = "Volvo",
                    Country = "Sweden",
                    LogoUrl = "https://example.com/logos/volvo.png",
                    Website = "https://volvocars.com",
                    Description = "Thương hiệu Thụy Điển với cam kết 100% điện hóa",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                // Inactive brand (for testing)
                new CarBrand
                {
                    BrandName = "Fisker",
                    Country = "United States",
                    LogoUrl = "https://example.com/logos/fisker.png",
                    Website = "https://fiskerinc.com",
                    Description = "Thương hiệu xe điện sang trọng (tạm ngưng hoạt động)",
                    IsActive = false,
                    CreatedDate = DateTime.UtcNow
                }
            };

            context.CarBrands.AddRange(brands);
            context.SaveChanges();
        }
    }
}
