using FluentAssertions;
using Xunit;
using Microsoft.EntityFrameworkCore;
using EVServiceCenter.Core.Entities;
using EVServiceCenter.Core.Domains.Customers.Entities;
using EVServiceCenter.Core.Domains.Identity.Entities;

namespace EVServiceCenter.Tests.CustomerFlow;

/// <summary>
/// Tests cho Authentication Flow - Phase 1
/// Đăng ký, xác thực email, đăng nhập
/// </summary>
public class AuthenticationFlowTests : TestBase
{
    [Fact]
    public async Task RegisterCustomer_WithValidData_ShouldCreateUserAndCustomer()
    {
        // Arrange
        var email = "newuser@test.com";
        var username = "newuser";
        var fullName = "Nguyễn Văn A";
        var phoneNumber = "0901234567";

        // Act - Create User
        var passwordSalt = BCrypt.Net.BCrypt.GenerateSalt();
        var user = new User
        {
            Username = username,
            Email = email,
            PasswordHash = System.Text.Encoding.UTF8.GetBytes(BCrypt.Net.BCrypt.HashPassword("Password123!", passwordSalt)),
            PasswordSalt = System.Text.Encoding.UTF8.GetBytes(passwordSalt),
            FullName = fullName,
            RoleId = 4, // Customer role
            IsActive = false, // Chưa verify email
            CreatedDate = DateTime.UtcNow
        };

        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();

        // Create Customer
        var customer = new Customer
        {
            UserId = user.UserId,
            CustomerCode = "CUST000001",
            FullName = fullName,
            PhoneNumber = phoneNumber,
            Email = email,
            IsActive = true,
            LoyaltyPoints = 0,
            TotalSpent = 0,
            CreatedDate = DateTime.UtcNow
        };

        DbContext.Customers.Add(customer);
        await DbContext.SaveChangesAsync();

        // Assert
        var savedUser = await DbContext.Users
            .FirstOrDefaultAsync(u => u.Email == email);
        var savedCustomer = await DbContext.Customers
            .FirstOrDefaultAsync(c => c.Email == email);

        savedUser.Should().NotBeNull();
        savedUser!.Username.Should().Be(username);
        savedUser.IsActive.Should().BeFalse("Email chưa verify");

        savedCustomer.Should().NotBeNull();
        savedCustomer!.FullName.Should().Be(fullName);
        savedCustomer.PhoneNumber.Should().Be(phoneNumber);
        savedCustomer.LoyaltyPoints.Should().Be(0);
    }

    [Fact]
    public async Task RegisterCustomer_WithDuplicateEmail_ShouldDetectConflict()
    {
        // Arrange
        var email = "duplicate@test.com";
        var salt1 = BCrypt.Net.BCrypt.GenerateSalt();
        var user1 = new User
        {
            Username = "user1",
            Email = email,
            FullName = "User 1",
            PasswordHash = System.Text.Encoding.UTF8.GetBytes(BCrypt.Net.BCrypt.HashPassword("Password123!", salt1)),
            PasswordSalt = System.Text.Encoding.UTF8.GetBytes(salt1),
            RoleId = 4,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        DbContext.Users.Add(user1);
        await DbContext.SaveChangesAsync();

        // Act - Try to register with same email
        var emailExists = await DbContext.Users
            .AnyAsync(u => u.Email == email);

        // Assert
        emailExists.Should().BeTrue("Email đã tồn tại trong hệ thống");
    }

    [Fact]
    public async Task RegisterCustomer_WithDuplicatePhone_ShouldDetectConflict()
    {
        // Arrange
        var phoneNumber = "0901234567";
        var customer1 = new Customer
        {
            CustomerCode = "CUST000001",
            FullName = "User 1",
            PhoneNumber = phoneNumber,
            Email = "user1@test.com",
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        DbContext.Customers.Add(customer1);
        await DbContext.SaveChangesAsync();

        // Act - Try to register with same phone
        var phoneExists = await DbContext.Customers
            .AnyAsync(c => c.PhoneNumber == phoneNumber);

        // Assert
        phoneExists.Should().BeTrue("Số điện thoại đã tồn tại trong hệ thống");
    }

    [Fact]
    public async Task VerifyEmail_ShouldActivateAccount()
    {
        // Arrange
        var salt = BCrypt.Net.BCrypt.GenerateSalt();
        var user = new User
        {
            Username = "testuser",
            Email = "test@test.com",
            FullName = "Test User",
            PasswordHash = System.Text.Encoding.UTF8.GetBytes(BCrypt.Net.BCrypt.HashPassword("Password123!", salt)),
            PasswordSalt = System.Text.Encoding.UTF8.GetBytes(salt),
            RoleId = 4,
            IsActive = false,
            CreatedDate = DateTime.UtcNow
        };

        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();

        // Act - Simulate email verification
        user.IsActive = true;
        await DbContext.SaveChangesAsync();

        var verifiedUser = await DbContext.Users
            .FirstOrDefaultAsync(u => u.UserId == user.UserId);

        // Assert
        verifiedUser.Should().NotBeNull();
        verifiedUser!.IsActive.Should().BeTrue("Email đã được verify");
    }

    [Fact]
    public async Task Login_WithVerifiedEmail_ShouldReturnUserAndCustomer()
    {
        // Arrange
        var email = "verified@test.com";
        var salt = BCrypt.Net.BCrypt.GenerateSalt();
        var user = new User
        {
            Username = "verified",
            Email = email,
            FullName = "Verified User",
            PasswordHash = System.Text.Encoding.UTF8.GetBytes(BCrypt.Net.BCrypt.HashPassword("Password123!", salt)),
            PasswordSalt = System.Text.Encoding.UTF8.GetBytes(salt),
            RoleId = 4,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();

        var customer = new Customer
        {
            UserId = user.UserId,
            CustomerCode = "CUST000001",
            FullName = "Verified User",
            PhoneNumber = "0901234567",
            Email = email,
            IsActive = true,
            LoyaltyPoints = 100,
            TotalSpent = 5000000,
            CreatedDate = DateTime.UtcNow
        };

        DbContext.Customers.Add(customer);
        await DbContext.SaveChangesAsync();

        // Act - Simulate login
        var loginResult = await DbContext.Users
            .Where(u => u.Email == email && u.IsActive == true)
            .Select(u => new
            {
                User = u,
                Customer = DbContext.Customers.FirstOrDefault(c => c.UserId == u.UserId)
            })
            .FirstOrDefaultAsync();

        // Assert
        loginResult.Should().NotBeNull();
        loginResult!.User.Should().NotBeNull();
        loginResult.Customer.Should().NotBeNull();
        loginResult.Customer!.CustomerCode.Should().Be("CUST000001");
        loginResult.Customer.LoyaltyPoints.Should().Be(100);
        loginResult.Customer.TotalSpent.Should().Be(5000000);
    }

    [Fact]
    public async Task Login_WithUnverifiedEmail_ShouldFail()
    {
        // Arrange
        var email = "unverified@test.com";
        var salt = BCrypt.Net.BCrypt.GenerateSalt();
        var user = new User
        {
            Username = "unverified",
            Email = email,
            FullName = "Unverified User",
            PasswordHash = System.Text.Encoding.UTF8.GetBytes(BCrypt.Net.BCrypt.HashPassword("Password123!", salt)),
            PasswordSalt = System.Text.Encoding.UTF8.GetBytes(salt),
            RoleId = 4,
            IsActive = false, // Chưa verify
            CreatedDate = DateTime.UtcNow
        };

        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();

        // Act - Try to login
        var canLogin = await DbContext.Users
            .AnyAsync(u => u.Email == email && u.IsActive == true);

        // Assert
        canLogin.Should().BeFalse("Không thể login khi email chưa verify");
    }

    [Fact]
    public async Task GenerateCustomerCode_ShouldAutoIncrement()
    {
        // Arrange
        var existingCustomers = new List<Customer>
        {
            new Customer { CustomerCode = "CUST000001", FullName = "C1", Email = "c1@test.com", PhoneNumber = "0901", IsActive = true, CreatedDate = DateTime.UtcNow },
            new Customer { CustomerCode = "CUST000002", FullName = "C2", Email = "c2@test.com", PhoneNumber = "0902", IsActive = true, CreatedDate = DateTime.UtcNow },
            new Customer { CustomerCode = "CUST000003", FullName = "C3", Email = "c3@test.com", PhoneNumber = "0903", IsActive = true, CreatedDate = DateTime.UtcNow }
        };

        DbContext.Customers.AddRange(existingCustomers);
        await DbContext.SaveChangesAsync();

        // Act - Get last code and generate next
        var lastCode = await DbContext.Customers
            .OrderByDescending(c => c.CustomerCode)
            .Select(c => c.CustomerCode)
            .FirstOrDefaultAsync();

        // Simulate code generation
        var lastNumber = int.Parse(lastCode!.Substring(4)); // Remove "CUST"
        var nextCode = $"CUST{(lastNumber + 1):D6}";

        // Assert
        lastCode.Should().Be("CUST000003");
        nextCode.Should().Be("CUST000004");
        nextCode.Should().MatchRegex(@"^CUST\d{6}$");
    }

    [Fact]
    public async Task UpdateCustomerProfile_ShouldSucceed()
    {
        // Arrange
        var customer = new Customer
        {
            CustomerCode = "CUST000001",
            FullName = "Old Name",
            PhoneNumber = "0901234567",
            Email = "test@test.com",
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        DbContext.Customers.Add(customer);
        await DbContext.SaveChangesAsync();

        // Act - Update profile
        customer.FullName = "New Name";
        customer.PhoneNumber = "0987654321";
        customer.UpdatedDate = DateTime.UtcNow;
        await DbContext.SaveChangesAsync();

        var updated = await DbContext.Customers
            .FirstOrDefaultAsync(c => c.CustomerId == customer.CustomerId);

        // Assert
        updated.Should().NotBeNull();
        updated!.FullName.Should().Be("New Name");
        updated.PhoneNumber.Should().Be("0987654321");
        updated.UpdatedDate.Should().NotBeNull();
    }
}
