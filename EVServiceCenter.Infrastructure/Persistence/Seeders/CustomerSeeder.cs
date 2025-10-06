// CustomerSeeder.cs
using EVServiceCenter.Core.Domains.Customers.Entities;
using EVServiceCenter.Core.Entities;

namespace EVServiceCenter.Infrastructure.Persistence.Seeders
{
    public static class CustomerSeeder
    {
        public static void SeedCustomers(EVDbContext context)
        {
            if (context.Customers.Any())
                return;

            var customers = new List<Customer>
            {
                new Customer
                {
                    CustomerCode = "KH000001",
                    FullName = "Nguyễn Văn A",
                    PhoneNumber = "0901234567",
                    Email = "nguyenvana@gmail.com",
                    Address = "123 Nguyễn Huệ, Quận 1, TP.HCM",
                    TypeId = 1,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow.AddMonths(-18)
                },
                new Customer
                {
                    CustomerCode = "KH000002",
                    FullName = "Trần Thị B",
                    PhoneNumber = "0912345678",
                    Email = "tranthib@gmail.com",
                    Address = "456 Lê Lợi, Quận 3, TP.HCM",
                    TypeId = 1,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow.AddMonths(-15)
                },
                new Customer
                {
                    CustomerCode = "KH000003",
                    FullName = "Lê Văn C",
                    PhoneNumber = "0923456789",
                    Email = "levanc@gmail.com",
                    Address = "789 Võ Văn Tần, Quận 5, TP.HCM",
                    TypeId = 1,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow.AddMonths(-12)
                },
                new Customer
                {
                    CustomerCode = "KH000004",
                    FullName = "Phạm Thị D",
                    PhoneNumber = "0934567890",
                    Email = "phamthid@gmail.com",
                    Address = "321 Trần Hưng Đạo, Quận 10, TP.HCM",
                    TypeId = 1,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow.AddMonths(-10)
                },
                new Customer
                {
                    CustomerCode = "KH000005",
                    FullName = "Hoàng Văn E",
                    PhoneNumber = "0945678901",
                    Email = "hoangvane@gmail.com",
                    Address = "654 Cách Mạng Tháng 8, Quận Tân Bình, TP.HCM",
                    TypeId = 1,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow.AddMonths(-8)
                },
                new Customer
                {
                    CustomerCode = "KH000006",
                    FullName = "Đặng Thị F",
                    PhoneNumber = "0956789012",
                    Email = "dangthif@gmail.com",
                    Address = "987 Lý Thường Kiệt, Quận 11, TP.HCM",
                    TypeId = 1,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow.AddMonths(-6)
                },
                new Customer
                {
                    CustomerCode = "KH000007",
                    FullName = "Vũ Văn G",
                    PhoneNumber = "0967890123",
                    Email = "vuvang@gmail.com",
                    Address = "147 Nguyễn Trãi, Quận 1, TP.HCM",
                    TypeId = 1,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow.AddMonths(-5)
                },
                new Customer
                {
                    CustomerCode = "KH000008",
                    FullName = "Bùi Thị H",
                    PhoneNumber = "0978901234",
                    Email = "buithih@gmail.com",
                    Address = "258 Hai Bà Trưng, Quận 3, TP.HCM",
                    TypeId = 1,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow.AddMonths(-4)
                },
                new Customer
                {
                    CustomerCode = "KH000009",
                    FullName = "Đinh Văn I",
                    PhoneNumber = "0989012345",
                    Email = "dinhvani@gmail.com",
                    Address = "369 Phạm Ngũ Lão, Quận 1, TP.HCM",
                    TypeId = 1,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow.AddMonths(-3)
                },
                new Customer
                {
                    CustomerCode = "KH000010",
                    FullName = "Dương Thị K",
                    PhoneNumber = "0990123456",
                    Email = "duongthik@gmail.com",
                    Address = "741 Điện Biên Phủ, Quận Bình Thạnh, TP.HCM",
                    TypeId = 1,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow.AddMonths(-2)
                },
                new Customer
                {
                    CustomerCode = "KH000011",
                    FullName = "Ngô Văn L",
                    PhoneNumber = "0901234568",
                    Email = "ngovanl@gmail.com",
                    Address = "12 Lê Văn Sỹ, Quận 3, TP.HCM",
                    TypeId = 1,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow.AddMonths(-7)
                },
                new Customer
                {
                    CustomerCode = "KH000012",
                    FullName = "Trương Thị M",
                    PhoneNumber = "0912345679",
                    Email = "truongthim@gmail.com",
                    Address = "34 Nguyễn Đình Chiểu, Quận 1, TP.HCM",
                    TypeId = 2,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow.AddMonths(-9)
                },
                new Customer
                {
                    CustomerCode = "KH000013",
                    FullName = "Phan Văn N",
                    PhoneNumber = "0923456780",
                    Email = "phanvann@gmail.com",
                    Address = "56 Pasteur, Quận 1, TP.HCM",
                    TypeId = 1,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow.AddMonths(-11)
                },
                new Customer
                {
                    CustomerCode = "KH000014",
                    FullName = "Lý Thị O",
                    PhoneNumber = "0934567891",
                    Email = "lythio@gmail.com",
                    Address = "78 Nam Kỳ Khởi Nghĩa, Quận 3, TP.HCM",
                    TypeId = 1,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow.AddMonths(-13)
                },
                new Customer
                {
                    CustomerCode = "KH000015",
                    FullName = "Mai Văn P",
                    PhoneNumber = "0945678902",
                    Email = "maivanp@gmail.com",
                    Address = "90 Trần Quang Khải, Quận 1, TP.HCM",
                    TypeId = 2,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow.AddMonths(-14)
                },
                new Customer
                {
                    CustomerCode = "KH000016",
                    FullName = "Võ Thị Q",
                    PhoneNumber = "0956789013",
                    Email = "vothiq@gmail.com",
                    Address = "102 Bùi Viện, Quận 1, TP.HCM",
                    TypeId = 1,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow.AddMonths(-16)
                },
                new Customer
                {
                    CustomerCode = "KH000017",
                    FullName = "Huỳnh Văn R",
                    PhoneNumber = "0967890124",
                    Email = "huynhvanr@gmail.com",
                    Address = "114 Đề Thám, Quận 1, TP.HCM",
                    TypeId = 1,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow.AddMonths(-17)
                },
                new Customer
                {
                    CustomerCode = "KH000018",
                    FullName = "Tô Thị S",
                    PhoneNumber = "0978901235",
                    Email = "tothis@gmail.com",
                    Address = "126 Cống Quỳnh, Quận 1, TP.HCM",
                    TypeId = 2,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow.AddMonths(-19)
                },
                new Customer
                {
                    CustomerCode = "KH000019",
                    FullName = "Đỗ Văn T",
                    PhoneNumber = "0989012346",
                    Email = "dovant@gmail.com",
                    Address = "138 Lý Tự Trọng, Quận 1, TP.HCM",
                    TypeId = 1,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow.AddMonths(-20)
                },
                new Customer
                {
                    CustomerCode = "KH000020",
                    FullName = "Cao Thị U",
                    PhoneNumber = "0990123457",
                    Email = "caothiu@gmail.com",
                    Address = "150 Đồng Khởi, Quận 1, TP.HCM",
                    TypeId = 2,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow.AddMonths(-21)
                },
                new Customer
                {
                    CustomerCode = "KH000021",
                    FullName = "Hồ Văn V",
                    PhoneNumber = "0901234569",
                    Email = "hovanv@gmail.com",
                    Address = "162 Nguyễn Thái Bình, Quận 1, TP.HCM",
                    TypeId = 1,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow.AddMonths(-22)
                },
                new Customer
                {
                    CustomerCode = "KH000022",
                    FullName = "Lưu Thị X",
                    PhoneNumber = "0912345670",
                    Email = "luuthix@gmail.com",
                    Address = "174 Tôn Đức Thắng, Quận 1, TP.HCM",
                    TypeId = 1,
                    IsActive = false,
                    CreatedDate = DateTime.UtcNow.AddMonths(-23)
                },
                new Customer
                {
                    CustomerCode = "KH000023",
                    FullName = "Tạ Văn Y",
                    PhoneNumber = "0923456781",
                    Email = "tavany@gmail.com",
                    Address = "186 Võ Thị Sáu, Quận 3, TP.HCM",
                    TypeId = 2,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow.AddMonths(-24)
                },
                new Customer
                {
                    CustomerCode = "KH000024",
                    FullName = "Châu Thị Z",
                    PhoneNumber = "0934567892",
                    Email = "chauthiz@gmail.com",
                    Address = "198 Nguyễn Bỉnh Khiêm, Quận 1, TP.HCM",
                    TypeId = 1,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow.AddMonths(-25)
                },
                new Customer
                {
                    CustomerCode = "KH000025",
                    FullName = "Quách Văn AA",
                    PhoneNumber = "0945678903",
                    Email = "quachvanaa@gmail.com",
                    Address = "200 Lê Thánh Tôn, Quận 1, TP.HCM",
                    TypeId = 2,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow.AddMonths(-1)
                },
                new Customer
                {
                    CustomerCode = "KH000026",
                    FullName = "Ông Thị BB",
                    PhoneNumber = "0956789014",
                    Email = "ongthibb@gmail.com",
                    Address = "212 Mạc Đĩnh Chi, Quận 1, TP.HCM",
                    TypeId = 1,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow.AddDays(-15)
                },
                new Customer
                {
                    CustomerCode = "KH000027",
                    FullName = "Khổng Văn CC",
                    PhoneNumber = "0967890125",
                    Email = "khongvancc@gmail.com",
                    Address = "224 Nguyễn Thị Minh Khai, Quận 3, TP.HCM",
                    TypeId = 1,
                    IsActive = false,
                    CreatedDate = DateTime.UtcNow.AddDays(-10)
                },
                new Customer
                {
                    CustomerCode = "KH000028",
                    FullName = "Kiều Thị DD",
                    PhoneNumber = "0978901236",
                    Email = "kieuthidd@gmail.com",
                    Address = "236 Cách Mạng Tháng 8, Quận 10, TP.HCM",
                    TypeId = 2,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow.AddDays(-5)
                },
                new Customer
                {
                    CustomerCode = "KH000029",
                    FullName = "Ưng Văn EE",
                    PhoneNumber = "0989012347",
                    Email = "ungvanee@gmail.com",
                    Address = "248 Đinh Tiên Hoàng, Quận 1, TP.HCM",
                    TypeId = 1,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow.AddDays(-3)
                },
                new Customer
                {
                    CustomerCode = "KH000030",
                    FullName = "Viên Thị FF",
                    PhoneNumber = "0990123458",
                    Email = "vienthiff@gmail.com",
                    Address = "260 Nguyễn Công Trứ, Quận 1, TP.HCM",
                    TypeId = 2,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow.AddDays(-1)
                }
            };

            context.Customers.AddRange(customers);
            context.SaveChanges();

            Console.WriteLine($"Seeded {customers.Count} customers");
        }
    }
}