using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Request;
using EVServiceCenter.Core.Domains.AppointmentManagement.Entities;
using EVServiceCenter.Core.Domains.AppointmentManagement.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.Customers.Interfaces;
using EVServiceCenter.Core.Domains.CustomerVehicles.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.MaintenanceServices.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.ModelServicePricings.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.PackageSubscriptions.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.Payments.Entities;
using EVServiceCenter.Core.Domains.Payments.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.Payments.Interfaces.Services;
using EVServiceCenter.Core.Domains.Payments.Interfaces;
using EVServiceCenter.Core.Domains.TimeSlots.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.Pricing.Interfaces;
using EVServiceCenter.Core.Domains.Invoices.Interfaces;
using EVServiceCenter.Core.Domains.Checklists.Interfaces;
using EVServiceCenter.Core.Entities;
using EVServiceCenter.Core.Enums;
using EVServiceCenter.Core.Interfaces.Services;
using EVServiceCenter.Infrastructure.Domains.AppointmentManagement.Services;
using EVServiceCenter.Infrastructure.Domains.Payments.Repositories;
using EVServiceCenter.Infrastructure.Domains.Payments.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace EVServiceCenter.Tests.AppointmentManagement
{
  public class AppointmentCommandServicePaymentTests : IDisposable
  {
    private readonly TestDbContext _context;
    private readonly IPaymentIntentService _paymentIntentService;
    private readonly AppointmentCommandService _service;

    public AppointmentCommandServicePaymentTests()
    {
      var options = new DbContextOptionsBuilder<EVDbContext>()
          .UseInMemoryDatabase(Guid.NewGuid().ToString())
          .Options;

      _context = new TestDbContext(options);
      var paymentIntentRepository = new PaymentIntentRepository(_context);
      _paymentIntentService = new PaymentIntentService(paymentIntentRepository, NullLogger<PaymentIntentService>.Instance);

      var configuration = new ConfigurationBuilder()
          .AddInMemoryCollection(new Dictionary<string, string?>
          {
            ["Payments:IntentExpiryHours"] = "24"
          })
          .Build();

            _service = new AppointmentCommandService(
          Mock.Of<IAppointmentRepository>(),
          Mock.Of<IAppointmentCommandRepository>(),
          Mock.Of<IAppointmentQueryRepository>(),
          Mock.Of<ITimeSlotRepository>(),
          Mock.Of<IMaintenanceServiceRepository>(),
          Mock.Of<IModelServicePricingRepository>(),
          Mock.Of<ICustomerVehicleRepository>(),
          Mock.Of<IPackageSubscriptionQueryRepository>(), // ? FIXED: typo
          Mock.Of<IPackageSubscriptionCommandRepository>(),
          Mock.Of<IServiceSourceAuditService>(),
          Mock.Of<IDiscountCalculationService>(),
          Mock.Of<IPromotionService>(),
          Mock.Of<ICustomerRepository>(),
          _paymentIntentService,
          Mock.Of<IPaymentService>(),
          Mock.Of<IRefundRepository>(),
          _context,
          configuration,
          NullLogger<AppointmentCommandService>.Instance,
          Mock.Of<IInvoiceService>(),
          Mock.Of<IChecklistService>()); // âœ… ADDED: Mock IChecklistService
    }

    [Fact]
    public async Task CreatePaymentIntentAsync_ShouldUseOutstandingAmount_WhenRequestAmountNull()
    {
      var appointment = new Appointment
      {
        AppointmentId = 101,
        CustomerId = 55,
        StatusId = (int)AppointmentStatusEnum.Confirmed,
        FinalCost = 500m,
        PaidAmount = 120m,
        PaymentStatus = PaymentStatusEnum.Pending.ToString(),
        PaymentIntents = new List<PaymentIntent>(),
        CreatedDate = DateTime.UtcNow
      };

      _context.Appointments.Add(appointment);
      await _context.SaveChangesAsync();

      var request = new CreatePaymentIntentRequestDto
      {
        AppointmentId = appointment.AppointmentId,
        Currency = "vnd"
      };

      var response = await _service.CreatePaymentIntentAsync(request, currentUserId: 999, CancellationToken.None);

      response.Amount.Should().Be(380m);
      response.Currency.Should().Be("VND");
      response.Status.Should().Be(PaymentIntentStatusEnum.Pending.ToString());
      response.PaymentIntentId.Should().BeGreaterThan(0);
      response.IntentCode.Should().NotBeNullOrWhiteSpace();
      response.Transactions.Should().BeEmpty();

      var updatedAppointment = await _context.Appointments.SingleAsync(a => a.AppointmentId == appointment.AppointmentId);
      updatedAppointment.PaymentIntentCount.Should().Be(1);
      updatedAppointment.LatestPaymentIntentId.Should().Be(response.PaymentIntentId);
      updatedAppointment.PaymentStatus.Should().Be(PaymentStatusEnum.Pending.ToString());

      var storedIntent = await _context.PaymentIntents.SingleAsync();
      storedIntent.Amount.Should().Be(380m);
      storedIntent.PaymentIntentId.Should().Be(response.PaymentIntentId);
      storedIntent.CustomerId.Should().Be(appointment.CustomerId);
    }

    [Fact]
    public async Task CreatePaymentIntentAsync_ShouldThrow_WhenNoOutstandingAmount()
    {
      var appointment = new Appointment
      {
        AppointmentId = 202,
        CustomerId = 12,
        StatusId = (int)AppointmentStatusEnum.Confirmed,
        FinalCost = 200m,
        PaidAmount = 200m,
        PaymentStatus = PaymentStatusEnum.Completed.ToString(),
        PaymentIntents = new List<PaymentIntent>(),
        CreatedDate = DateTime.UtcNow
      };

      _context.Appointments.Add(appointment);
      await _context.SaveChangesAsync();

      var request = new CreatePaymentIntentRequestDto
      {
        AppointmentId = appointment.AppointmentId
      };

      Func<Task> act = async () => await _service.CreatePaymentIntentAsync(request, currentUserId: 1, CancellationToken.None);

      await act.Should().ThrowAsync<InvalidOperationException>()
          .WithMessage("Lá»‹ch háº¹n khÃ´ng cÃ²n khoáº£n cáº§n thanh toÃ¡n");
    }

    [Fact]
    public async Task CreatePaymentIntentAsync_ShouldValidateAmountDoesNotExceedOutstanding()
    {
      var appointment = new Appointment
      {
        AppointmentId = 303,
        CustomerId = 88,
        StatusId = (int)AppointmentStatusEnum.Confirmed,
        FinalCost = 400m,
        PaidAmount = 150m,
        PaymentStatus = PaymentStatusEnum.Pending.ToString(),
        PaymentIntents = new List<PaymentIntent>(),
        CreatedDate = DateTime.UtcNow
      };

      _context.Appointments.Add(appointment);
      await _context.SaveChangesAsync();

      var request = new CreatePaymentIntentRequestDto
      {
        AppointmentId = appointment.AppointmentId,
        Amount = 260.01m
      };

      Func<Task> act = async () => await _service.CreatePaymentIntentAsync(request, currentUserId: 1, CancellationToken.None);

      await act.Should().ThrowAsync<InvalidOperationException>()
          .WithMessage("Sá»‘ tiá»n yÃªu cáº§u vÆ°á»£t quÃ¡ khoáº£n outstanding hiá»‡n táº¡i");
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
        if (!optionsBuilder.IsConfigured)
        {
          base.OnConfiguring(optionsBuilder);
        }
      }
    }
  }
}


