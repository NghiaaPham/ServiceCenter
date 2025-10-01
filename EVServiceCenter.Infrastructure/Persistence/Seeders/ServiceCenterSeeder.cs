using EVServiceCenter.Core.Domains.ServiceCenters.Entities;
using EVServiceCenter.Core.Entities;

namespace EVServiceCenter.Infrastructure.Persistence.Seeders
{
    public static class ServiceCenterSeeder
    {
        public static void SeedData(EVDbContext context)
        {
            if (context.ServiceCenters.Any())
                return;

            var centers = new List<ServiceCenter>
        {
            new ServiceCenter
            {
                CenterCode = "EVSC-HCM-Q1",
                CenterName = "EV Service Center - Quận 1",
                Address = "123 Nguyễn Huệ",
                Ward = "Phường Bến Nghé",
                District = "Quận 1",
                Province = "TP. Hồ Chí Minh",
                PostalCode = "700000",
                PhoneNumber = "028-12345678",
                Email = "q1@evservicecenter.vn",
                Website = "https://evservicecenter.vn/q1",
                OpenTime = new TimeOnly(7, 30),
                CloseTime = new TimeOnly(19, 0),
                Capacity = 15,
                Latitude = 10.7769m,
                Longitude = 106.7009m,
                Facilities = "WiFi miễn phí, Phòng chờ máy lạnh, Nước uống, Bãi đỗ xe",
                Description = "Trung tâm dịch vụ xe điện tại trung tâm Sài Gòn",
                ImageUrl = "/images/centers/q1.png",
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            },
            new ServiceCenter
            {
                CenterCode = "EVSC-HCM-BT",
                CenterName = "EV Service Center - Bình Thạnh",
                Address = "234 Điện Biên Phủ",
                Ward = "Phường 15",
                District = "Quận Bình Thạnh",
                Province = "TP. Hồ Chí Minh",
                PostalCode = "700000",
                PhoneNumber = "028-23456789",
                Email = "binhthanh@evservicecenter.vn",
                Website = "https://evservicecenter.vn/binhthanh",
                OpenTime = new TimeOnly(8, 0),
                CloseTime = new TimeOnly(18, 0),
                Capacity = 12,
                Latitude = 10.8031m,
                Longitude = 106.7144m,
                Facilities = "Phòng chờ VIP, Cafe, Bãi đỗ xe rộng",
                Description = "Trung tâm dịch vụ hiện đại tại Bình Thạnh",
                ImageUrl = "/images/centers/binhthanh.png",
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            },
            new ServiceCenter
            {
                CenterCode = "EVSC-HN-TX",
                CenterName = "EV Service Center - Hà Nội Thanh Xuân",
                Address = "456 Nguyễn Trãi",
                Ward = "Phường Thượng Đình",
                District = "Quận Thanh Xuân",
                Province = "Hà Nội",
                PostalCode = "100000",
                PhoneNumber = "024-12345678",
                Email = "thanhxuan@evservicecenter.vn",
                Website = "https://evservicecenter.vn/thanhxuan",
                OpenTime = new TimeOnly(8, 0),
                CloseTime = new TimeOnly(18, 30),
                Capacity = 20,
                Latitude = 21.0024m,
                Longitude = 105.8201m,
                Facilities = "Phòng chờ hiện đại, Máy pha cafe, Khu vực đọc sách",
                Description = "Trung tâm dịch vụ hàng đầu tại Hà Nội",
                ImageUrl = "/images/centers/thanhxuan.png",
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            }
        };

            context.ServiceCenters.AddRange(centers);
            context.SaveChanges();
        }
    }
}
