using FluentAssertions;
using FluentValidation;
using EVServiceCenter.API.Validators;
using EVServiceCenter.Core.Entities;
using Xunit;

namespace EVServiceCenter.Tests.Unit.Validators;

/// <summary>
/// Unit tests for WorkOrderValidator
/// </summary>
public class WorkOrderValidatorTests
{
    private readonly WorkOrderValidator _validator;

    public WorkOrderValidatorTests()
    {
        _validator = new WorkOrderValidator();
    }

    [Fact]
    public void Validate_CreateRuleSet_WithValidWorkOrder_ShouldPass()
    {
        // Arrange
        var workOrder = new WorkOrder
        {
            WorkOrderCode = "WO001",
            CustomerId = 1,
            VehicleId = 1,
            ServiceCenterId = 1,
            StatusId = 1,
            Priority = "High",
            TechnicianId = 1,
            AdvisorId = 1,
            SupervisorId = 1,
            EstimatedAmount = 1000000,
            TotalAmount = 1200000,
            DiscountAmount = 100000
        };

        // Act
        var result = _validator.Validate(workOrder, options => options.IncludeRuleSets(ValidationRules.RuleSetCreate));

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_CreateRuleSet_WithEmptyWorkOrderCode_ShouldFail()
    {
        // Arrange
        var workOrder = new WorkOrder
        {
            WorkOrderCode = "",
            CustomerId = 1,
            VehicleId = 1,
            ServiceCenterId = 1,
            StatusId = 1
        };

        // Act
        var result = _validator.Validate(workOrder, options => options.IncludeRuleSets(ValidationRules.RuleSetCreate));

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "WorkOrderCode" && e.ErrorMessage.Contains("required"));
    }

    [Fact]
    public void Validate_CreateRuleSet_WithWorkOrderCodeTooLong_ShouldFail()
    {
        // Arrange
        var workOrder = new WorkOrder
        {
            WorkOrderCode = new string('W', 21), // 21 characters
            CustomerId = 1,
            VehicleId = 1,
            ServiceCenterId = 1,
            StatusId = 1
        };

        // Act
        var result = _validator.Validate(workOrder, options => options.IncludeRuleSets(ValidationRules.RuleSetCreate));

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "WorkOrderCode" && e.ErrorMessage.Contains("at most"));
    }

    [Fact]
    public void Validate_CreateRuleSet_WithInvalidCustomerId_ShouldFail()
    {
        // Arrange
        var workOrder = new WorkOrder
        {
            WorkOrderCode = "WO001",
            CustomerId = 0, // Invalid
            VehicleId = 1,
            ServiceCenterId = 1,
            StatusId = 1
        };

        // Act
        var result = _validator.Validate(workOrder, options => options.IncludeRuleSets(ValidationRules.RuleSetCreate));

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CustomerId" && e.ErrorMessage.Contains("greater than 0"));
    }

    [Fact]
    public void Validate_CreateRuleSet_WithInvalidVehicleId_ShouldFail()
    {
        // Arrange
        var workOrder = new WorkOrder
        {
            WorkOrderCode = "WO001",
            CustomerId = 1,
            VehicleId = 0, // Invalid
            ServiceCenterId = 1,
            StatusId = 1
        };

        // Act
        var result = _validator.Validate(workOrder, options => options.IncludeRuleSets(ValidationRules.RuleSetCreate));

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "VehicleId" && e.ErrorMessage.Contains("greater than 0"));
    }

    [Fact]
    public void Validate_CreateRuleSet_WithInvalidServiceCenterId_ShouldFail()
    {
        // Arrange
        var workOrder = new WorkOrder
        {
            WorkOrderCode = "WO001",
            CustomerId = 1,
            VehicleId = 1,
            ServiceCenterId = 0, // Invalid
            StatusId = 1
        };

        // Act
        var result = _validator.Validate(workOrder, options => options.IncludeRuleSets(ValidationRules.RuleSetCreate));

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ServiceCenterId" && e.ErrorMessage.Contains("greater than 0"));
    }

    [Fact]
    public void Validate_CreateRuleSet_WithInvalidStatusId_ShouldFail()
    {
        // Arrange
        var workOrder = new WorkOrder
        {
            WorkOrderCode = "WO001",
            CustomerId = 1,
            VehicleId = 1,
            ServiceCenterId = 1,
            StatusId = 0 // Invalid
        };

        // Act
        var result = _validator.Validate(workOrder, options => options.IncludeRuleSets(ValidationRules.RuleSetCreate));

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "StatusId" && e.ErrorMessage.Contains("greater than 0"));
    }

    [Fact]
    public void Validate_UpdateRuleSet_WithValidWorkOrder_ShouldPass()
    {
        // Arrange
        var workOrder = new WorkOrder
        {
            WorkOrderCode = "WO001"
        };

        // Act
        var result = _validator.Validate(workOrder, options => options.IncludeRuleSets(ValidationRules.RuleSetUpdate));

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_UpdateRuleSet_WithEmptyWorkOrderCode_ShouldFail()
    {
        // Arrange
        var workOrder = new WorkOrder
        {
            WorkOrderCode = ""
        };

        // Act
        var result = _validator.Validate(workOrder, options => options.IncludeRuleSets(ValidationRules.RuleSetUpdate));

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "WorkOrderCode" && e.ErrorMessage.Contains("required"));
    }

    [Fact]
    public void Validate_WithPriorityTooLong_ShouldFail()
    {
        // Arrange
        var workOrder = new WorkOrder
        {
            WorkOrderCode = "WO001",
            CustomerId = 1,
            VehicleId = 1,
            ServiceCenterId = 1,
            StatusId = 1,
            Priority = new string('P', 21) // 21 characters
        };

        // Act
        var result = _validator.Validate(workOrder);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Priority" && e.ErrorMessage.Contains("at most"));
    }

    [Fact]
    public void Validate_WithValidPriority_ShouldPass()
    {
        // Arrange
        var workOrder = new WorkOrder
        {
            WorkOrderCode = "WO001",
            CustomerId = 1,
            VehicleId = 1,
            ServiceCenterId = 1,
            StatusId = 1,
            Priority = "High"
        };

        // Act
        var result = _validator.Validate(workOrder);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithInvalidTechnicianId_ShouldFail()
    {
        // Arrange
        var workOrder = new WorkOrder
        {
            WorkOrderCode = "WO001",
            CustomerId = 1,
            VehicleId = 1,
            ServiceCenterId = 1,
            StatusId = 1,
            TechnicianId = 0 // Invalid
        };

        // Act
        var result = _validator.Validate(workOrder);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "TechnicianId" && e.ErrorMessage.Contains("greater than 0"));
    }

    [Fact]
    public void Validate_WithValidTechnicianId_ShouldPass()
    {
        // Arrange
        var workOrder = new WorkOrder
        {
            WorkOrderCode = "WO001",
            CustomerId = 1,
            VehicleId = 1,
            ServiceCenterId = 1,
            StatusId = 1,
            TechnicianId = 1
        };

        // Act
        var result = _validator.Validate(workOrder);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithNullTechnicianId_ShouldPass()
    {
        // Arrange
        var workOrder = new WorkOrder
        {
            WorkOrderCode = "WO001",
            CustomerId = 1,
            VehicleId = 1,
            ServiceCenterId = 1,
            StatusId = 1,
            TechnicianId = null // Null is allowed
        };

        // Act
        var result = _validator.Validate(workOrder);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithInvalidAdvisorId_ShouldFail()
    {
        // Arrange
        var workOrder = new WorkOrder
        {
            WorkOrderCode = "WO001",
            CustomerId = 1,
            VehicleId = 1,
            ServiceCenterId = 1,
            StatusId = 1,
            AdvisorId = 0 // Invalid
        };

        // Act
        var result = _validator.Validate(workOrder);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "AdvisorId" && e.ErrorMessage.Contains("greater than 0"));
    }

    [Fact]
    public void Validate_WithInvalidSupervisorId_ShouldFail()
    {
        // Arrange
        var workOrder = new WorkOrder
        {
            WorkOrderCode = "WO001",
            CustomerId = 1,
            VehicleId = 1,
            ServiceCenterId = 1,
            StatusId = 1,
            SupervisorId = 0 // Invalid
        };

        // Act
        var result = _validator.Validate(workOrder);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "SupervisorId" && e.ErrorMessage.Contains("greater than 0"));
    }

    [Fact]
    public void Validate_WithNegativeEstimatedAmount_ShouldFail()
    {
        // Arrange
        var workOrder = new WorkOrder
        {
            WorkOrderCode = "WO001",
            CustomerId = 1,
            VehicleId = 1,
            ServiceCenterId = 1,
            StatusId = 1,
            EstimatedAmount = -1000 // Negative
        };

        // Act
        var result = _validator.Validate(workOrder);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "EstimatedAmount" && e.ErrorMessage.Contains("greater than or equal to 0"));
    }

    [Fact]
    public void Validate_WithValidEstimatedAmount_ShouldPass()
    {
        // Arrange
        var workOrder = new WorkOrder
        {
            WorkOrderCode = "WO001",
            CustomerId = 1,
            VehicleId = 1,
            ServiceCenterId = 1,
            StatusId = 1,
            EstimatedAmount = 1000000
        };

        // Act
        var result = _validator.Validate(workOrder);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithZeroEstimatedAmount_ShouldPass()
    {
        // Arrange
        var workOrder = new WorkOrder
        {
            WorkOrderCode = "WO001",
            CustomerId = 1,
            VehicleId = 1,
            ServiceCenterId = 1,
            StatusId = 1,
            EstimatedAmount = 0 // Zero is allowed
        };

        // Act
        var result = _validator.Validate(workOrder);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithNullEstimatedAmount_ShouldPass()
    {
        // Arrange
        var workOrder = new WorkOrder
        {
            WorkOrderCode = "WO001",
            CustomerId = 1,
            VehicleId = 1,
            ServiceCenterId = 1,
            StatusId = 1,
            EstimatedAmount = null // Null is allowed
        };

        // Act
        var result = _validator.Validate(workOrder);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithNegativeTotalAmount_ShouldFail()
    {
        // Arrange
        var workOrder = new WorkOrder
        {
            WorkOrderCode = "WO001",
            CustomerId = 1,
            VehicleId = 1,
            ServiceCenterId = 1,
            StatusId = 1,
            TotalAmount = -1000 // Negative
        };

        // Act
        var result = _validator.Validate(workOrder);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "TotalAmount" && e.ErrorMessage.Contains("greater than or equal to 0"));
    }

    [Fact]
    public void Validate_WithNegativeDiscountAmount_ShouldFail()
    {
        // Arrange
        var workOrder = new WorkOrder
        {
            WorkOrderCode = "WO001",
            CustomerId = 1,
            VehicleId = 1,
            ServiceCenterId = 1,
            StatusId = 1,
            DiscountAmount = -1000 // Negative
        };

        // Act
        var result = _validator.Validate(workOrder);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DiscountAmount" && e.ErrorMessage.Contains("greater than or equal to 0"));
    }

    [Fact]
    public void Validate_WithAllFieldsValid_ShouldPass()
    {
        // Arrange
        var workOrder = new WorkOrder
        {
            WorkOrderCode = "WO001",
            CustomerId = 1,
            VehicleId = 1,
            ServiceCenterId = 1,
            StatusId = 1,
            Priority = "High",
            TechnicianId = 1,
            AdvisorId = 1,
            SupervisorId = 1,
            EstimatedAmount = 1000000,
            TotalAmount = 1200000,
            DiscountAmount = 100000
        };

        // Act
        var result = _validator.Validate(workOrder);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithMultipleValidationErrors_ShouldFailWithMultipleErrors()
    {
        // Arrange
        var workOrder = new WorkOrder
        {
            WorkOrderCode = "", // Empty
            CustomerId = 0, // Invalid
            VehicleId = 0, // Invalid
            ServiceCenterId = 0, // Invalid
            StatusId = 0, // Invalid
            Priority = new string('P', 21), // Too long
            TechnicianId = 0, // Invalid
            AdvisorId = 0, // Invalid
            SupervisorId = 0, // Invalid
            EstimatedAmount = -1000, // Negative
            TotalAmount = -1000, // Negative
            DiscountAmount = -1000 // Negative
        };

        // Act
        var result = _validator.Validate(workOrder, options => options.IncludeRuleSets(ValidationRules.RuleSetCreate));

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThan(4); // Multiple errors expected
        result.Errors.Should().Contain(e => e.PropertyName == "WorkOrderCode");
        result.Errors.Should().Contain(e => e.PropertyName == "CustomerId");
        result.Errors.Should().Contain(e => e.PropertyName == "VehicleId");
        result.Errors.Should().Contain(e => e.PropertyName == "ServiceCenterId");
        result.Errors.Should().Contain(e => e.PropertyName == "StatusId");
    }
}
