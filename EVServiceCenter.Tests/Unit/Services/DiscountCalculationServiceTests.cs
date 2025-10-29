using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using EVServiceCenter.Infrastructure.Domains.Pricing.Services;
using EVServiceCenter.Core.Domains.Pricing.Interfaces;
using EVServiceCenter.Core.Domains.Pricing.Models;
using Xunit;

namespace EVServiceCenter.Tests.Unit.Services;

/// <summary>
/// Unit tests for DiscountCalculationService
/// </summary>
public class DiscountCalculationServiceTests
{
    private readonly Mock<IPromotionService> _mockPromotionService;
    private readonly Mock<ILogger<DiscountCalculationService>> _mockLogger;
    private readonly DiscountCalculationService _service;

    public DiscountCalculationServiceTests()
    {
        _mockPromotionService = new Mock<IPromotionService>();
        _mockLogger = new Mock<ILogger<DiscountCalculationService>>();
        _service = new DiscountCalculationService(_mockPromotionService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task CalculateDiscountAsync_WithSubscriptionServices_ShouldReturn100PercentDiscount()
    {
        // Arrange
        var request = new DiscountCalculationRequest
        {
            CustomerId = 1,
            Services = new List<ServiceLineItem>
            {
                new ServiceLineItem
                {
                    ServiceId = 1,
                    ServiceName = "Oil Change",
                    ServiceSource = "Subscription",
                    BasePrice = 500000,
                    Quantity = 1,
                    SubscriptionId = 123
                }
            }
        };

        // Act
        var result = await _service.CalculateDiscountAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.OriginalTotal.Should().Be(0); // Subscription services don't count toward original total
        result.FinalTotal.Should().Be(0);
        result.ServiceBreakdowns.Should().HaveCount(1);
        
        var breakdown = result.ServiceBreakdowns.First();
        breakdown.ServiceSource.Should().Be("Subscription");
        breakdown.OriginalPrice.Should().Be(500000);
        breakdown.DiscountAmount.Should().Be(500000); // 100% discount
        breakdown.FinalPrice.Should().Be(0);
        breakdown.DiscountReason.Should().Be("Dịch vụ từ gói subscription (miễn phí 100%)");
    }

    [Fact]
    public async Task CalculateDiscountAsync_WithRegularServices_ShouldCalculateCorrectly()
    {
        // Arrange
        var request = new DiscountCalculationRequest
        {
            CustomerId = 1,
            Services = new List<ServiceLineItem>
            {
                new ServiceLineItem
                {
                    ServiceId = 1,
                    ServiceName = "Oil Change",
                    ServiceSource = "Regular",
                    BasePrice = 500000,
                    Quantity = 1
                },
                new ServiceLineItem
                {
                    ServiceId = 2,
                    ServiceName = "Brake Check",
                    ServiceSource = "Regular",
                    BasePrice = 300000,
                    Quantity = 2
                }
            }
        };

        // Act
        var result = await _service.CalculateDiscountAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.OriginalTotal.Should().Be(1100000); // 500000 + (300000 * 2)
        result.FinalTotal.Should().Be(1100000); // No discount applied
        result.ServiceBreakdowns.Should().HaveCount(2);
    }

    [Fact]
    public async Task CalculateDiscountAsync_WithCustomerTypeDiscount_ShouldApplyCorrectDiscount()
    {
        // Arrange
        var request = new DiscountCalculationRequest
        {
            CustomerId = 1,
            CustomerTypeId = 2,
            CustomerTypeDiscountPercent = 15,
            Services = new List<ServiceLineItem>
            {
                new ServiceLineItem
                {
                    ServiceId = 1,
                    ServiceName = "Oil Change",
                    ServiceSource = "Regular",
                    BasePrice = 1000000,
                    Quantity = 1
                }
            }
        };

        // Act
        var result = await _service.CalculateDiscountAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.OriginalTotal.Should().Be(1000000);
        result.CustomerTypeDiscount.Should().Be(150000); // 15% of 1,000,000
        result.FinalDiscount.Should().Be(150000);
        result.FinalTotal.Should().Be(850000); // 1,000,000 - 150,000
        result.AppliedDiscountType.Should().Be("CustomerType");
    }

    [Fact]
    public async Task CalculateDiscountAsync_WithValidPromotion_ShouldApplyPromotionDiscount()
    {
        // Arrange
        var request = new DiscountCalculationRequest
        {
            CustomerId = 1,
            PromotionCode = "SAVE20",
            Services = new List<ServiceLineItem>
            {
                new ServiceLineItem
                {
                    ServiceId = 1,
                    ServiceName = "Oil Change",
                    ServiceSource = "Regular",
                    BasePrice = 1000000,
                    Quantity = 1
                }
            }
        };

        var promotionValidation = new PromotionValidationResult
        {
            IsValid = true,
            DiscountAmount = 200000,
            PromotionId = 1
        };

        _mockPromotionService
            .Setup(x => x.ValidatePromotionAsync("SAVE20", 1, 1000000, It.IsAny<List<int>>()))
            .ReturnsAsync(promotionValidation);

        // Act
        var result = await _service.CalculateDiscountAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.OriginalTotal.Should().Be(1000000);
        result.PromotionDiscount.Should().Be(200000);
        result.FinalDiscount.Should().Be(200000);
        result.FinalTotal.Should().Be(800000);
        result.AppliedDiscountType.Should().Be("Promotion");
        result.PromotionCodeUsed.Should().Be("SAVE20");
        result.PromotionId.Should().Be(1);
    }

    [Fact]
    public async Task CalculateDiscountAsync_WithInvalidPromotion_ShouldFallbackToCustomerTypeDiscount()
    {
        // Arrange
        var request = new DiscountCalculationRequest
        {
            CustomerId = 1,
            CustomerTypeId = 2,
            CustomerTypeDiscountPercent = 10,
            PromotionCode = "INVALID",
            Services = new List<ServiceLineItem>
            {
                new ServiceLineItem
                {
                    ServiceId = 1,
                    ServiceName = "Oil Change",
                    ServiceSource = "Regular",
                    BasePrice = 1000000,
                    Quantity = 1
                }
            }
        };

        var promotionValidation = new PromotionValidationResult
        {
            IsValid = false,
            ErrorMessage = "Promotion not found"
        };

        _mockPromotionService
            .Setup(x => x.ValidatePromotionAsync("INVALID", 1, 1000000, It.IsAny<List<int>>()))
            .ReturnsAsync(promotionValidation);

        // Act
        var result = await _service.CalculateDiscountAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.OriginalTotal.Should().Be(1000000);
        result.CustomerTypeDiscount.Should().Be(100000); // 10% of 1,000,000
        result.PromotionDiscount.Should().Be(0); // Invalid promotion
        result.FinalDiscount.Should().Be(100000); // Falls back to customer type discount
        result.FinalTotal.Should().Be(900000);
        result.AppliedDiscountType.Should().Be("CustomerType");
    }

    [Fact]
    public async Task CalculateDiscountAsync_WithPromotionBetterThanCustomerType_ShouldChoosePromotion()
    {
        // Arrange
        var request = new DiscountCalculationRequest
        {
            CustomerId = 1,
            CustomerTypeId = 2,
            CustomerTypeDiscountPercent = 10, // 100,000 discount
            PromotionCode = "BIGSAVE",
            Services = new List<ServiceLineItem>
            {
                new ServiceLineItem
                {
                    ServiceId = 1,
                    ServiceName = "Oil Change",
                    ServiceSource = "Regular",
                    BasePrice = 1000000,
                    Quantity = 1
                }
            }
        };

        var promotionValidation = new PromotionValidationResult
        {
            IsValid = true,
            DiscountAmount = 250000, // Better than customer type discount
            PromotionId = 2
        };

        _mockPromotionService
            .Setup(x => x.ValidatePromotionAsync("BIGSAVE", 1, 1000000, It.IsAny<List<int>>()))
            .ReturnsAsync(promotionValidation);

        // Act
        var result = await _service.CalculateDiscountAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.CustomerTypeDiscount.Should().Be(100000);
        result.PromotionDiscount.Should().Be(250000);
        result.FinalDiscount.Should().Be(250000); // Chooses promotion (higher)
        result.FinalTotal.Should().Be(750000);
        result.AppliedDiscountType.Should().Be("Promotion");
    }

    [Fact]
    public async Task CalculateDiscountAsync_WithMixedServices_ShouldHandleCorrectly()
    {
        // Arrange
        var request = new DiscountCalculationRequest
        {
            CustomerId = 1,
            CustomerTypeId = 2,
            CustomerTypeDiscountPercent = 20,
            Services = new List<ServiceLineItem>
            {
                new ServiceLineItem
                {
                    ServiceId = 1,
                    ServiceName = "Oil Change",
                    ServiceSource = "Subscription",
                    BasePrice = 500000,
                    Quantity = 1,
                    SubscriptionId = 123
                },
                new ServiceLineItem
                {
                    ServiceId = 2,
                    ServiceName = "Brake Check",
                    ServiceSource = "Regular",
                    BasePrice = 300000,
                    Quantity = 1
                }
            }
        };

        // Act
        var result = await _service.CalculateDiscountAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.OriginalTotal.Should().Be(300000); // Only regular service counts
        result.CustomerTypeDiscount.Should().Be(60000); // 20% of 300,000
        result.FinalDiscount.Should().Be(60000);
        result.FinalTotal.Should().Be(240000);
        result.ServiceBreakdowns.Should().HaveCount(2);

        var subscriptionBreakdown = result.ServiceBreakdowns.First(s => s.ServiceSource == "Subscription");
        subscriptionBreakdown.FinalPrice.Should().Be(0);

        var regularBreakdown = result.ServiceBreakdowns.First(s => s.ServiceSource == "Regular");
        regularBreakdown.OriginalPrice.Should().Be(300000);
        regularBreakdown.DiscountAmount.Should().Be(60000);
        regularBreakdown.FinalPrice.Should().Be(240000);
    }

    [Fact]
    public async Task CalculateDiscountAsync_WithEmptyServices_ShouldReturnZeroTotals()
    {
        // Arrange
        var request = new DiscountCalculationRequest
        {
            CustomerId = 1,
            Services = new List<ServiceLineItem>()
        };

        // Act
        var result = await _service.CalculateDiscountAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.OriginalTotal.Should().Be(0);
        result.FinalTotal.Should().Be(0);
        result.CustomerTypeDiscount.Should().Be(0);
        result.PromotionDiscount.Should().Be(0);
        result.FinalDiscount.Should().Be(0);
        result.ServiceBreakdowns.Should().BeEmpty();
    }

    [Fact]
    public async Task CalculateDiscountAsync_WithZeroQuantity_ShouldHandleCorrectly()
    {
        // Arrange
        var request = new DiscountCalculationRequest
        {
            CustomerId = 1,
            Services = new List<ServiceLineItem>
            {
                new ServiceLineItem
                {
                    ServiceId = 1,
                    ServiceName = "Oil Change",
                    ServiceSource = "Regular",
                    BasePrice = 500000,
                    Quantity = 0
                }
            }
        };

        // Act
        var result = await _service.CalculateDiscountAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.OriginalTotal.Should().Be(0);
        result.FinalTotal.Should().Be(0);
        result.ServiceBreakdowns.Should().HaveCount(1);
        
        var breakdown = result.ServiceBreakdowns.First();
        breakdown.OriginalPrice.Should().Be(0);
        breakdown.DiscountAmount.Should().Be(0);
        breakdown.FinalPrice.Should().Be(0);
    }
}
