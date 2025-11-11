using System;
using System.Threading.Tasks;
using EVServiceCenter.Core.Domains.Payments.Entities;
using EVServiceCenter.Core.Domains.Payments.Interfaces.Repositories;
using EVServiceCenter.Core.Entities;
using EVServiceCenter.Infrastructure.Domains.Payments.Repositories;
using EVServiceCenter.Infrastructure.Domains.Payments.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace EVServiceCenter.Tests.AppointmentManagement
{
  public class PaymentIntentServiceTests : IDisposable
  {
    private readonly TestDbContext _context;
    private readonly IPaymentIntentRepository _repository;
    private readonly PaymentIntentService _service;

    public PaymentIntentServiceTests()
    {
      var options = new DbContextOptionsBuilder<EVDbContext>()
          .UseInMemoryDatabase(Guid.NewGuid().ToString())
          .Options;

      _context = new TestDbContext(options);
      _repository = new PaymentIntentRepository(_context);
      _service = new PaymentIntentService(_repository, NullLogger<PaymentIntentService>.Instance);
    }

    [Fact]
    public void BuildPendingIntent_ShouldNormalizeValues()
    {
      var intent = _service.BuildPendingIntent(
          customerId: 42,
          amount: 123.456m,
          createdBy: 7,
          currency: null,
          expiresAt: DateTime.UtcNow.AddHours(2),
          paymentMethod: "Card",
          idempotencyKey: "test-key");

      intent.Amount.Should().Be(123.46m);
      intent.Currency.Should().Be("VND");
      intent.Status.Should().Be("Pending");
      intent.CapturedAmount.Should().Be(0);
      intent.CreatedBy.Should().Be(7);
      intent.CustomerId.Should().Be(42);
      intent.IntentCode.Should().NotBeNullOrWhiteSpace();
      intent.ExpiresAt.Should().NotBeNull();
      intent.DueDate.Should().Be(intent.ExpiresAt);
    }

    [Fact]
    public async Task AppendNewIntentAsync_ShouldPersistIntent()
    {
      var intent = _service.BuildPendingIntent(
          customerId: 5,
          amount: 200m,
          createdBy: 1,
          currency: "VND",
          expiresAt: DateTime.UtcNow.AddHours(1));

      intent.AppointmentId = 10;

      var saved = await _service.AppendNewIntentAsync(intent);

      saved.PaymentIntentId.Should().BeGreaterThan(0);
      saved.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

      var stored = await _context.PaymentIntents.SingleAsync();
      stored.PaymentIntentId.Should().Be(saved.PaymentIntentId);
      stored.Amount.Should().Be(200m);
      stored.Status.Should().Be("Pending");
    }

    [Fact]
    public async Task GetByAppointmentAsync_ShouldReturnIntentsInDescendingOrder()
    {
      var first = _service.BuildPendingIntent(customerId: 1, amount: 50m, createdBy: 1);
      first.AppointmentId = 77;
      await _service.AppendNewIntentAsync(first);

      await Task.Delay(5); // ensure different timestamps

      var second = _service.BuildPendingIntent(customerId: 1, amount: 30m, createdBy: 2);
      second.AppointmentId = 77;
      await _service.AppendNewIntentAsync(second);

      var intents = await _service.GetByAppointmentAsync(77);

      intents.Should().HaveCount(2);
      intents[0].PaymentIntentId.Should().BeGreaterThan(intents[1].PaymentIntentId);
      intents[0].Amount.Should().Be(30m);
      intents[1].Amount.Should().Be(50m);
    }

    public void Dispose()
    {
      _context.Dispose();
    }

    private sealed class TestDbContext : EVDbContext
    {
      public TestDbContext(DbContextOptions<EVDbContext> options)
          : base(options)
      {
      }

      protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
      {
        // Prevent base context from overriding test configuration
        if (!optionsBuilder.IsConfigured)
        {
          base.OnConfiguring(optionsBuilder);
        }
      }
    }
  }
}
