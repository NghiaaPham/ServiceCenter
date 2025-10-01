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
                }
            };

            context.Customers.AddRange(customers);
            context.SaveChanges();

            Console.WriteLine($"Seeded {customers.Count} customers");
        }
    }
}