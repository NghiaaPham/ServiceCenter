using EVServiceCenter.Core.Domains.Identity.Entities;
using EVServiceCenter.Core.Entities;
using EVServiceCenter.Core.Enums;
using System.Security.Cryptography;
using System.Text;

namespace EVServiceCenter.Infrastructure.Persistence.Seeders;

/// <summary>
/// Seeder for Technician users with realistic test data
/// Creates 10 technicians across 2 service centers
/// </summary>
public static class TechnicianSeeder
{
    public static void SeedTechnicians(EVDbContext context)
    {
        if (context.Users.Any(u => u.RoleId == (int)UserRoles.Technician))
        {
            Console.WriteLine("Technicians already seeded. Skipping...");
            return;
        }

        // Create password hash/salt for default password: "Technician@123"
        var (passwordHash, passwordSalt) = CreatePasswordHash("Technician@123");

        var technicians = new List<User>
        {
            // Service Center 1 - 6 technicians
            new User
            {
                Username = "tech001",
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                FullName = "Nguyễn Văn Tùng",
                Email = "nguyenvantung.tech@evservice.com",
                PhoneNumber = "0901111001",
                RoleId = (int)UserRoles.Technician,
                EmployeeCode = "TECH-001",
                Department = "Maintenance",
                HireDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-5)),
                Salary = 15000000,
                IsActive = true,
                EmailVerified = true,
                CreatedDate = DateTime.UtcNow.AddYears(-5)
            },
            new User
            {
                Username = "tech002",
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                FullName = "Trần Minh Đức",
                Email = "tranminhduc.tech@evservice.com",
                PhoneNumber = "0901111002",
                RoleId = (int)UserRoles.Technician,
                EmployeeCode = "TECH-002",
                Department = "Maintenance",
                HireDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-4)),
                Salary = 14000000,
                IsActive = true,
                EmailVerified = true,
                CreatedDate = DateTime.UtcNow.AddYears(-4)
            },
            new User
            {
                Username = "tech003",
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                FullName = "Lê Hoàng Nam",
                Email = "lehoangnam.tech@evservice.com",
                PhoneNumber = "0901111003",
                RoleId = (int)UserRoles.Technician,
                EmployeeCode = "TECH-003",
                Department = "Diagnostics",
                HireDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-3)),
                Salary = 13000000,
                IsActive = true,
                EmailVerified = true,
                CreatedDate = DateTime.UtcNow.AddYears(-3)
            },
            new User
            {
                Username = "tech004",
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                FullName = "Phạm Quốc Huy",
                Email = "phamquochuy.tech@evservice.com",
                PhoneNumber = "0901111004",
                RoleId = (int)UserRoles.Technician,
                EmployeeCode = "TECH-004",
                Department = "Maintenance",
                HireDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-2)),
                Salary = 12000000,
                IsActive = true,
                EmailVerified = true,
                CreatedDate = DateTime.UtcNow.AddYears(-2)
            },
            new User
            {
                Username = "tech005",
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                FullName = "Võ Thanh Tùng",
                Email = "vothanhtung.tech@evservice.com",
                PhoneNumber = "0901111005",
                RoleId = (int)UserRoles.Technician,
                EmployeeCode = "TECH-005",
                Department = "Diagnostics",
                HireDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-2)),
                Salary = 12500000,
                IsActive = true,
                EmailVerified = true,
                CreatedDate = DateTime.UtcNow.AddYears(-2)
            },
            new User
            {
                Username = "tech006",
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                FullName = "Đặng Minh Quân",
                Email = "dangminhquan.tech@evservice.com",
                PhoneNumber = "0901111006",
                RoleId = (int)UserRoles.Technician,
                EmployeeCode = "TECH-006",
                Department = "Maintenance",
                HireDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-18)),
                Salary = 11000000,
                IsActive = true,
                EmailVerified = true,
                CreatedDate = DateTime.UtcNow.AddMonths(-18)
            },

            // Service Center 2 - 4 technicians
            new User
            {
                Username = "tech007",
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                FullName = "Huỳnh Văn Khoa",
                Email = "huynhvankhoa.tech@evservice.com",
                PhoneNumber = "0901111007",
                RoleId = (int)UserRoles.Technician,
                EmployeeCode = "TECH-007",
                Department = "Maintenance",
                HireDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-3)),
                Salary = 13500000,
                IsActive = true,
                EmailVerified = true,
                CreatedDate = DateTime.UtcNow.AddYears(-3)
            },
            new User
            {
                Username = "tech008",
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                FullName = "Bùi Công Minh",
                Email = "buicongminh.tech@evservice.com",
                PhoneNumber = "0901111008",
                RoleId = (int)UserRoles.Technician,
                EmployeeCode = "TECH-008",
                Department = "Diagnostics",
                HireDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-2)),
                Salary = 12000000,
                IsActive = true,
                EmailVerified = true,
                CreatedDate = DateTime.UtcNow.AddYears(-2)
            },
            new User
            {
                Username = "tech009",
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                FullName = "Ngô Quang Hải",
                Email = "ngoquanghai.tech@evservice.com",
                PhoneNumber = "0901111009",
                RoleId = (int)UserRoles.Technician,
                EmployeeCode = "TECH-009",
                Department = "Maintenance",
                HireDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-15)),
                Salary = 11500000,
                IsActive = true,
                EmailVerified = true,
                CreatedDate = DateTime.UtcNow.AddMonths(-15)
            },
            new User
            {
                Username = "tech010",
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                FullName = "Phan Đức Anh",
                Email = "phanducanh.tech@evservice.com",
                PhoneNumber = "0901111010",
                RoleId = (int)UserRoles.Technician,
                EmployeeCode = "TECH-010",
                Department = "Diagnostics",
                HireDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-12)),
                Salary = 11000000,
                IsActive = true,
                EmailVerified = true,
                CreatedDate = DateTime.UtcNow.AddMonths(-12)
            }
        };

        context.Users.AddRange(technicians);
        context.SaveChanges();

        Console.WriteLine($"✅ Seeded {technicians.Count} technicians");
        Console.WriteLine("📝 Default credentials: username/password = tech001-010 / Technician@123");
    }

    /// <summary>
    /// Create password hash and salt using HMACSHA512
    /// </summary>
    private static (byte[] hash, byte[] salt) CreatePasswordHash(string password)
    {
        using var hmac = new HMACSHA512();
        var salt = hmac.Key;
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        return (hash, salt);
    }
}
