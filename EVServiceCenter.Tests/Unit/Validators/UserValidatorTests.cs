using FluentAssertions;
using FluentValidation;
using EVServiceCenter.API.Validators;
using EVServiceCenter.Core.Domains.Identity.Entities;
using EVServiceCenter.Core.Enums;
using Xunit;

namespace EVServiceCenter.Tests.Unit.Validators;

/// <summary>
/// Unit tests for UserValidator
/// </summary>
public class UserValidatorTests
{
    private readonly UserValidator _validator;

    public UserValidatorTests()
    {
        _validator = new UserValidator();
    }

    [Fact]
    public void Validate_CreateRuleSet_WithValidUser_ShouldPass()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            FullName = "Test User",
            RoleId = (int)UserRoles.Customer,
            Email = "test@example.com",
            PhoneNumber = "0923456789"
        };

        // Act
        var result = _validator.Validate(user, options => options.IncludeRuleSets(ValidationRules.RuleSetCreate));

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_CreateRuleSet_WithEmptyUsername_ShouldFail()
    {
        // Arrange
        var user = new User
        {
            Username = "",
            FullName = "Test User",
            RoleId = (int)UserRoles.Customer
        };

        // Act
        var result = _validator.Validate(user, options => options.IncludeRuleSets(ValidationRules.RuleSetCreate));

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Username" && e.ErrorMessage.Contains("required"));
    }

    [Fact]
    public void Validate_CreateRuleSet_WithUsernameTooLong_ShouldFail()
    {
        // Arrange
        var user = new User
        {
            Username = new string('a', 51), // 51 characters
            FullName = "Test User",
            RoleId = (int)UserRoles.Customer
        };

        // Act
        var result = _validator.Validate(user, options => options.IncludeRuleSets(ValidationRules.RuleSetCreate));

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Username" && e.ErrorMessage.Contains("at most"));
    }

    [Fact]
    public void Validate_CreateRuleSet_WithUsernameWithLeadingTrailingSpaces_ShouldFail()
    {
        // Arrange
        var user = new User
        {
            Username = " testuser ", // Has leading/trailing spaces
            FullName = "Test User",
            RoleId = (int)UserRoles.Customer
        };

        // Act
        var result = _validator.Validate(user, options => options.IncludeRuleSets(ValidationRules.RuleSetCreate));

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Username");
    }

    [Fact]
    public void Validate_CreateRuleSet_WithEmptyFullName_ShouldFail()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            FullName = "",
            RoleId = (int)UserRoles.Customer
        };

        // Act
        var result = _validator.Validate(user, options => options.IncludeRuleSets(ValidationRules.RuleSetCreate));

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FullName" && e.ErrorMessage.Contains("required"));
    }

    [Fact]
    public void Validate_CreateRuleSet_WithFullNameTooLong_ShouldFail()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            FullName = new string('a', 101), // 101 characters
            RoleId = (int)UserRoles.Customer
        };

        // Act
        var result = _validator.Validate(user, options => options.IncludeRuleSets(ValidationRules.RuleSetCreate));

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FullName" && e.ErrorMessage.Contains("at most"));
    }

    [Fact]
    public void Validate_CreateRuleSet_WithInvalidRoleId_ShouldFail()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            FullName = "Test User",
            RoleId = 0 // Invalid role ID
        };

        // Act
        var result = _validator.Validate(user, options => options.IncludeRuleSets(ValidationRules.RuleSetCreate));

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "RoleId" && e.ErrorMessage.Contains("greater than 0"));
    }

    [Fact]
    public void Validate_UpdateRuleSet_WithValidUser_ShouldPass()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            FullName = "Test User"
        };

        // Act
        var result = _validator.Validate(user, options => options.IncludeRuleSets(ValidationRules.RuleSetUpdate));

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_UpdateRuleSet_WithEmptyUsername_ShouldFail()
    {
        // Arrange
        var user = new User
        {
            Username = ""
        };

        // Act
        var result = _validator.Validate(user, options => options.IncludeRuleSets(ValidationRules.RuleSetUpdate));

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Username" && e.ErrorMessage.Contains("required"));
    }

    [Fact]
    public void Validate_WithInvalidEmail_ShouldFail()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            FullName = "Test User",
            RoleId = (int)UserRoles.Customer,
            Email = "invalid-email"
        };

        // Act
        var result = _validator.Validate(user);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email" && e.ErrorMessage.Contains("valid email"));
    }

    [Fact]
    public void Validate_WithValidEmail_ShouldPass()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            FullName = "Test User",
            RoleId = (int)UserRoles.Customer,
            Email = "test@example.com"
        };

        // Act
        var result = _validator.Validate(user);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyEmail_ShouldPass()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            FullName = "Test User",
            RoleId = (int)UserRoles.Customer,
            Email = ""
        };

        // Act
        var result = _validator.Validate(user);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmailTooLong_ShouldFail()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            FullName = "Test User",
            RoleId = (int)UserRoles.Customer,
            Email = new string('a', 95) + "@example.com" // 101 characters total
        };

        // Act
        var result = _validator.Validate(user);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email" && e.ErrorMessage.Contains("at most"));
    }

    [Fact]
    public void Validate_WithPhoneNumberTooLong_ShouldFail()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            FullName = "Test User",
            RoleId = (int)UserRoles.Customer,
            PhoneNumber = new string('1', 21) // 21 characters
        };

        // Act
        var result = _validator.Validate(user);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PhoneNumber" && e.ErrorMessage.Contains("at most"));
    }

    [Fact]
    public void Validate_WithValidPhoneNumber_ShouldPass()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            FullName = "Test User",
            RoleId = (int)UserRoles.Customer,
            PhoneNumber = "0923456789"
        };

        // Act
        var result = _validator.Validate(user);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmployeeCodeTooLong_ShouldFail()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            FullName = "Test User",
            RoleId = (int)UserRoles.Customer,
            EmployeeCode = new string('E', 21) // 21 characters
        };

        // Act
        var result = _validator.Validate(user);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "EmployeeCode" && e.ErrorMessage.Contains("at most"));
    }

    [Fact]
    public void Validate_WithDepartmentTooLong_ShouldFail()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            FullName = "Test User",
            RoleId = (int)UserRoles.Customer,
            Department = new string('D', 51) // 51 characters
        };

        // Act
        var result = _validator.Validate(user);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Department" && e.ErrorMessage.Contains("at most"));
    }

    [Fact]
    public void Validate_WithNegativeSalary_ShouldFail()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            FullName = "Test User",
            RoleId = (int)UserRoles.Customer,
            Salary = -1000
        };

        // Act
        var result = _validator.Validate(user);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Salary" && e.ErrorMessage.Contains("greater than or equal to 0"));
    }

    [Fact]
    public void Validate_WithValidSalary_ShouldPass()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            FullName = "Test User",
            RoleId = (int)UserRoles.Customer,
            Salary = 5000000
        };

        // Act
        var result = _validator.Validate(user);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithNegativeFailedLoginAttempts_ShouldFail()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            FullName = "Test User",
            RoleId = (int)UserRoles.Customer,
            FailedLoginAttempts = -1
        };

        // Act
        var result = _validator.Validate(user);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FailedLoginAttempts" && e.ErrorMessage.Contains("greater than or equal to 0"));
    }

    [Fact]
    public void Validate_WithValidFailedLoginAttempts_ShouldPass()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            FullName = "Test User",
            RoleId = (int)UserRoles.Customer,
            FailedLoginAttempts = 3
        };

        // Act
        var result = _validator.Validate(user);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithProfilePictureTooLong_ShouldFail()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            FullName = "Test User",
            RoleId = (int)UserRoles.Customer,
            ProfilePicture = new string('P', 501) // 501 characters
        };

        // Act
        var result = _validator.Validate(user);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ProfilePicture" && e.ErrorMessage.Contains("at most"));
    }

    [Fact]
    public void Validate_WithInvalidCreatedBy_ShouldFail()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            FullName = "Test User",
            RoleId = (int)UserRoles.Customer,
            CreatedBy = 0
        };

        // Act
        var result = _validator.Validate(user);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CreatedBy" && e.ErrorMessage.Contains("greater than 0"));
    }

    [Fact]
    public void Validate_WithValidCreatedBy_ShouldPass()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            FullName = "Test User",
            RoleId = (int)UserRoles.Customer,
            CreatedBy = 1
        };

        // Act
        var result = _validator.Validate(user);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
