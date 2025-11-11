using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EVServiceCenter.Core.Domains.AppointmentManagement.Entities;
using EVServiceCenter.Core.Domains.Customers.Entities;
using EVServiceCenter.Core.Domains.CustomerVehicles.Entities;
using EVServiceCenter.Core.Domains.Identity.Entities;
using EVServiceCenter.Core.Entities;
using EVServiceCenter.Core.Enums;
using EVServiceCenter.Core.Helpers;
using Microsoft.Extensions.Logging;

namespace EVServiceCenter.Infrastructure.Persistence.Seeders;

/// <summary>
/// Seeds deterministic data that powers the customer mainflow Postman collection.
/// Ensures demo customer account, vehicle, subscription, work order history,
/// invoice/payment, and sample notifications exist with predictable identifiers.
/// </summary>
public static class MainflowDemoSeeder
{
    private const string DemoUsername = "nghiadaucau1@gmail.com";
    private const string DemoPassword = "changeme";
    private const string DemoCustomerCode = "KHMAIN001";
    private const string DemoVehiclePlate = "MAIN-TEST-001";
    private const string DemoWorkOrderCode = "WO-DEMO-0001";
    private const string DemoInvoiceCode = "INV-DEMO-0001";
    private const string DemoPaymentCode = "PAY-DEMO-0001";
    private const string DemoNotificationCodePrefix = "NTF-DEMO-";

    public static void Seed(EVDbContext context, ILogger logger)
    {
        EnsurePaymentMethods(context, logger);

        var user = EnsureDemoUser(context, logger);
        var customer = EnsureDemoCustomer(context, user, logger);
        var vehicle = EnsureDemoVehicle(context, customer, logger);
        var subscription = EnsureDemoSubscription(context, customer, vehicle, logger);
        EnsureDemoMaintenanceHistory(context, vehicle, logger);
        EnsureDemoWorkOrderPipeline(context, customer, vehicle, logger);
        EnsureDemoNotifications(context, customer, user, logger);

        // Flush any pending changes
        context.SaveChanges();

        logger.LogInformation("✅ Mainflow demo data ensured successfully.");
    }

    private static void EnsurePaymentMethods(EVDbContext context, ILogger logger)
    {
        var paymentMethods = new[]
        {
            new PaymentMethod
            {
                MethodCode = "VNPAY",
                MethodName = "VNPay Gateway",
                PaymentType = "Online",
                GatewayProvider = "VNPay",
                ProcessingFee = 0.015m,
                FixedFee = 0,
                IsOnline = true,
                RequiresApproval = false,
                IsActive = true,
                DisplayOrder = 1
            },
            new PaymentMethod
            {
                MethodCode = "MOMO",
                MethodName = "MoMo Wallet",
                PaymentType = "Online",
                GatewayProvider = "MoMo",
                ProcessingFee = 0.02m,
                FixedFee = 0,
                IsOnline = true,
                RequiresApproval = false,
                IsActive = true,
                DisplayOrder = 2
            },
            new PaymentMethod
            {
                MethodCode = "CASH",
                MethodName = "Cash at Counter",
                PaymentType = "Offline",
                GatewayProvider = "Internal",
                ProcessingFee = 0,
                FixedFee = 0,
                IsOnline = false,
                RequiresApproval = false,
                IsActive = true,
                DisplayOrder = 3
            }
        };

        foreach (var method in paymentMethods)
        {
            var existing = context.PaymentMethods.FirstOrDefault(pm => pm.MethodCode == method.MethodCode);
            if (existing != null)
            {
                existing.MethodName = method.MethodName;
                existing.PaymentType = method.PaymentType;
                existing.GatewayProvider = method.GatewayProvider;
                existing.ProcessingFee = method.ProcessingFee;
                existing.FixedFee = method.FixedFee;
                existing.IsOnline = method.IsOnline;
                existing.RequiresApproval = method.RequiresApproval;
                existing.IsActive = method.IsActive;
                existing.DisplayOrder = method.DisplayOrder;
            }
            else
            {
                context.PaymentMethods.Add(method);
                logger.LogInformation("➕ Seeded payment method {Code}", method.MethodCode);
            }
        }

        context.SaveChanges();
    }

    private static User EnsureDemoUser(EVDbContext context, ILogger logger)
    {
        var user = context.Users.FirstOrDefault(u => u.Username == DemoUsername);
        var now = DateTime.UtcNow;

        if (user == null)
        {
            var salt = SecurityHelper.GenerateSalt();
            var hash = SecurityHelper.HashPassword(DemoPassword, salt);

            user = new User
            {
                Username = DemoUsername,
                FullName = "Nguyễn Nghĩa (Demo Customer)",
                Email = DemoUsername,
                PhoneNumber = "0900000001",
                RoleId = (int)UserRoles.Customer,
                IsActive = true,
                EmailVerified = true,
                CreatedDate = now,
                PasswordHash = Encoding.UTF8.GetBytes(hash),
                PasswordSalt = Encoding.UTF8.GetBytes(salt),
                LastLoginDate = now
            };

            context.Users.Add(user);
            context.SaveChanges();

            logger.LogInformation("➕ Seeded demo user {Email}", DemoUsername);
        }
        else
        {
            var existingHash = user.PasswordHash != null
                ? Encoding.UTF8.GetString(user.PasswordHash)
                : string.Empty;

            if (string.IsNullOrEmpty(existingHash) || !SecurityHelper.VerifyPassword(DemoPassword, existingHash))
            {
                var salt = SecurityHelper.GenerateSalt();
                var hash = SecurityHelper.HashPassword(DemoPassword, salt);
                user.PasswordSalt = Encoding.UTF8.GetBytes(salt);
                user.PasswordHash = Encoding.UTF8.GetBytes(hash);
                logger.LogInformation("♻️  Reset password hash for demo user {Email}", DemoUsername);
            }

            if (user.RoleId != (int)UserRoles.Customer)
            {
                user.RoleId = (int)UserRoles.Customer;
                logger.LogInformation("♻️  Updated role for demo user {Email} to Customer", DemoUsername);
            }

            user.IsActive = true;
            user.EmailVerified = true;
            user.LastLoginDate ??= now;
            context.SaveChanges();
        }

        return user;
    }

    private static Customer EnsureDemoCustomer(EVDbContext context, User user, ILogger logger)
    {
        var customer = context.Customers.FirstOrDefault(c =>
            c.UserId == user.UserId || c.CustomerCode == DemoCustomerCode || c.Email == DemoUsername);

        if (customer == null)
        {
            customer = new Customer
            {
                CustomerCode = DemoCustomerCode,
                FullName = user.FullName ?? "Demo Customer",
                PhoneNumber = user.PhoneNumber ?? "0900000001",
                Email = DemoUsername,
                Address = "123 Nguyễn Huệ, Quận 1, TP.HCM",
                TypeId = 1,
                IsActive = true,
                CreatedDate = DateTime.UtcNow.AddMonths(-6),
                UserId = user.UserId,
                PreferredLanguage = "vi",
                MarketingOptIn = true,
                LoyaltyPoints = 1200,
                TotalSpent = 15800000m
            };

            context.Customers.Add(customer);
            context.SaveChanges();

            logger.LogInformation("➕ Seeded demo customer {Code}", DemoCustomerCode);
        }
        else
        {
            customer.UserId = user.UserId;
            customer.IsActive = true;
            if (string.IsNullOrWhiteSpace(customer.CustomerCode))
            {
                customer.CustomerCode = DemoCustomerCode;
            }

            if (string.IsNullOrWhiteSpace(customer.PhoneNumber))
            {
                customer.PhoneNumber = "0900000001";
            }

            context.SaveChanges();
        }

        return customer;
    }

    private static CustomerVehicle EnsureDemoVehicle(EVDbContext context, Customer customer, ILogger logger)
    {
        var vehicle = context.CustomerVehicles
            .FirstOrDefault(v => v.CustomerId == customer.CustomerId && v.LicensePlate == DemoVehiclePlate);

        if (vehicle != null)
        {
            vehicle.IsActive = true;
            context.SaveChanges();
            return vehicle;
        }

        var model = context.CarModels.FirstOrDefault(m => m.ModelName == "Model 3")
                    ?? context.CarModels.First();

        vehicle = new CustomerVehicle
        {
            CustomerId = customer.CustomerId,
            ModelId = model.ModelId,
            LicensePlate = DemoVehiclePlate,
            Vin = "5YJ3E1EA7HF000999",
            Color = "Trắng",
            PurchaseDate = new DateOnly(DateTime.UtcNow.Year - 1, 6, 15),
            Mileage = 22000,
            LastMaintenanceDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-3)),
            NextMaintenanceDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(3)),
            LastMaintenanceMileage = 18000,
            NextMaintenanceMileage = 28000,
            BatteryHealthPercent = 94.5m,
            VehicleCondition = "Good",
            InsuranceNumber = "INS-MAIN-001",
            InsuranceExpiry = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(8)),
            RegistrationExpiry = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(10)),
            Notes = "Demo vehicle cho mainflow automation",
            IsActive = true,
            CreatedDate = DateTime.UtcNow.AddMonths(-9)
        };

        context.CustomerVehicles.Add(vehicle);
        context.SaveChanges();

        logger.LogInformation("➕ Seeded demo vehicle {Plate}", DemoVehiclePlate);

        return vehicle;
    }

    private static CustomerPackageSubscription EnsureDemoSubscription(
        EVDbContext context,
        Customer customer,
        CustomerVehicle vehicle,
        ILogger logger)
    {
        var subscription = context.CustomerPackageSubscriptions
            .FirstOrDefault(s => s.SubscriptionCode == "SUB-DEMO-001");

        var package = context.MaintenancePackages.FirstOrDefault(p => p.PackageCode == "PKG-PREMIUM-2025")
                      ?? context.MaintenancePackages.First();

        var vnPayMethodId = context.PaymentMethods.First(pm => pm.MethodCode == "VNPAY").MethodId;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        if (subscription == null)
        {
            subscription = new CustomerPackageSubscription
            {
                SubscriptionCode = "SUB-DEMO-001",
                CustomerId = customer.CustomerId,
                PackageId = package.PackageId,
                VehicleId = vehicle.VehicleId,
                StartDate = today.AddMonths(-4),
                ExpirationDate = today.AddMonths(8),
                Status = "Active",
                AutoRenew = true,
                PaymentMethodId = vnPayMethodId,
                OriginalPrice = 4500000m,
                DiscountPercent = 20,
                DiscountAmount = 900000m,
                PaymentAmount = 3600000m,
                RemainingServices = 6,
                UsedServices = 2,
                LastServiceDate = today.AddMonths(-1),
                Notes = "Demo subscription cho Postman mainflow",
                PurchaseDate = DateTime.UtcNow.AddMonths(-4),
                InitialVehicleMileage = 15000,
                CreatedDate = DateTime.UtcNow.AddMonths(-4)
            };

            context.CustomerPackageSubscriptions.Add(subscription);
            context.SaveChanges();

            logger.LogInformation("➕ Seeded demo subscription SUB-DEMO-001");
        }
        else
        {
            subscription.Status = "Active";
            subscription.VehicleId = vehicle.VehicleId;
            subscription.PackageId = package.PackageId;
            subscription.AutoRenew = true;
            subscription.PaymentMethodId = vnPayMethodId;
            subscription.ExpirationDate = today.AddMonths(8);
            subscription.RemainingServices = Math.Max(subscription.RemainingServices ?? 0, 4);
            subscription.UsedServices = Math.Min(subscription.UsedServices ?? 0, 4);
            context.SaveChanges();
        }

        // Ensure PackageServiceUsage exists for few common services
        var includedServiceCodes = new[] { "BD-10K", "PIN-CHECK", "PHANH-CHECK" };
        foreach (var serviceCode in includedServiceCodes)
        {
            var service = context.MaintenanceServices.FirstOrDefault(ms => ms.ServiceCode == serviceCode);
            if (service == null)
                continue;

            var usage = context.PackageServiceUsages
                .FirstOrDefault(u => u.SubscriptionId == subscription.SubscriptionId && u.ServiceId == service.ServiceId);

            if (usage == null)
            {
                context.PackageServiceUsages.Add(new PackageServiceUsage
                {
                    SubscriptionId = subscription.SubscriptionId,
                    ServiceId = service.ServiceId,
                    TotalAllowedQuantity = 2,
                    UsedQuantity = 1,
                    RemainingQuantity = 1,
                    LastUsedDate = DateTime.UtcNow.AddMonths(-1),
                    Notes = "Demo allowance"
                });
            }
        }

        context.SaveChanges();
        return subscription;
    }

    private static void EnsureDemoMaintenanceHistory(EVDbContext context, CustomerVehicle vehicle, ILogger logger)
    {
        if (context.MaintenanceHistories.Any(h => h.VehicleId == vehicle.VehicleId))
        {
            return;
        }

        var histories = new List<MaintenanceHistory>
        {
            new()
            {
                VehicleId = vehicle.VehicleId,
                WorkOrderId = EnsureDemoWorkOrder(context, vehicle.CustomerId, vehicle.VehicleId, logger).WorkOrderId,
                ServiceDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-6)),
                Mileage = 15000,
                ServicesPerformed = "Bảo dưỡng 10.000 km, kiểm tra pin",
                PartsReplaced = "Lọc gió cabin",
                TotalServiceCost = 1200000m,
                TotalPartsCost = 250000m,
                TotalCost = 1450000m,
                NextServiceDue = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-3)),
                NextServiceMileage = 20000,
                BatteryHealthBefore = 96.5m,
                BatteryHealthAfter = 96.8m,
                TechnicianNotes = "Xe hoạt động tốt, đã cập nhật firmware phiên bản mới.",
                CreatedDate = DateTime.UtcNow.AddMonths(-6)
            },
            new()
            {
                VehicleId = vehicle.VehicleId,
                WorkOrderId = EnsureDemoWorkOrder(context, vehicle.CustomerId, vehicle.VehicleId, logger).WorkOrderId,
                ServiceDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-2)),
                Mileage = 20000,
                ServicesPerformed = "Bảo dưỡng định kỳ 20.000 km, cân chỉnh lốp",
                PartsReplaced = "Vệ sinh phanh, cân bằng lốp",
                TotalServiceCost = 1650000m,
                TotalPartsCost = 350000m,
                TotalCost = 2000000m,
                NextServiceDue = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(4)),
                NextServiceMileage = 30000,
                BatteryHealthBefore = 95.2m,
                BatteryHealthAfter = 95.5m,
                TechnicianNotes = "Đã đặt lịch vệ sinh điều hòa cho lần bảo dưỡng tiếp theo.",
                CreatedDate = DateTime.UtcNow.AddMonths(-2)
            }
        };

        context.MaintenanceHistories.AddRange(histories);
        context.SaveChanges();

        logger.LogInformation("➕ Seeded {Count} maintenance history records for demo vehicle", histories.Count);
    }

    private static WorkOrder EnsureDemoWorkOrder(
        EVDbContext context,
        int customerId,
        int vehicleId,
        ILogger logger)
    {
        var workOrder = context.WorkOrders.FirstOrDefault(wo => wo.WorkOrderCode == DemoWorkOrderCode);
        if (workOrder != null)
        {
            return workOrder;
        }

        var status = context.WorkOrderStatuses.FirstOrDefault(s => s.StatusName == "Completed")
                     ?? context.WorkOrderStatuses.First();
        var serviceCenter = context.ServiceCenters.FirstOrDefault(sc => sc.CenterCode == "EVSC-HCM-Q1")
                            ?? context.ServiceCenters.First();
        var technician = context.Users.FirstOrDefault(u => u.RoleId == (int)UserRoles.Technician && u.IsActive == true);

        var startDate = DateTime.UtcNow.AddMonths(-2).AddDays(-2);
        var completedDate = startDate.AddDays(1);

        workOrder = new WorkOrder
        {
            WorkOrderCode = DemoWorkOrderCode,
            CustomerId = customerId,
            VehicleId = vehicleId,
            ServiceCenterId = serviceCenter.CenterId,
            StatusId = status.StatusId,
            Status = status,
            Priority = "Normal",
            SourceType = "Online",
            TechnicianId = technician?.UserId,
            AdvisorId = technician?.UserId,
            StartDate = startDate,
            EstimatedCompletionDate = completedDate,
            CompletedDate = completedDate,
            EstimatedAmount = 2500000m,
            TotalAmount = 2500000m,
            DiscountAmount = 250000m,
            TaxAmount = 150000m,
            FinalAmount = 2400000m,
            CustomerNotes = "Bảo dưỡng demo phục vụ automation test",
            InternalNotes = "Demo work order seeded cho Postman flow",
            TechnicianNotes = "Đã hoàn tất bảo dưỡng và cập nhật thông số pin.",
            CreatedDate = startDate,
            UpdatedDate = completedDate
        };

        context.WorkOrders.Add(workOrder);
        context.SaveChanges();

        logger.LogInformation("➕ Seeded demo work order {Code}", DemoWorkOrderCode);

        EnsureDemoInvoiceAndPayment(context, workOrder, logger);

        return workOrder;
    }

    private static void EnsureDemoInvoiceAndPayment(EVDbContext context, WorkOrder workOrder, ILogger logger)
    {
        var invoice = context.Invoices.FirstOrDefault(i => i.InvoiceCode == DemoInvoiceCode);
        if (invoice == null)
        {
            invoice = new Invoice
            {
                InvoiceCode = DemoInvoiceCode,
                WorkOrderId = workOrder.WorkOrderId,
                CustomerId = workOrder.CustomerId,
                InvoiceDate = DateTime.UtcNow.AddMonths(-2),
                DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-2).AddDays(5)),
                ServiceSubTotal = 2200000m,
                PartsSubTotal = 300000m,
                ServiceDiscount = 220000m,
                PartsDiscount = 30000m,
                TotalDiscount = 250000m,
                ServiceTax = 110000m,
                PartsTax = 15000m,
                TotalTax = 125000m,
                GrandTotal = 2400000m,
                PaidAmount = 2400000m,
                OutstandingAmount = 0,
                Status = "Paid",
                PaymentTerms = "Thanh toán ngay",
                SentToCustomer = true,
                SentDate = DateTime.UtcNow.AddMonths(-2),
                Notes = "Hóa đơn demo phục vụ Postman mainflow",
                CreatedDate = DateTime.UtcNow.AddMonths(-2)
            };

            context.Invoices.Add(invoice);
            context.SaveChanges();

            logger.LogInformation("➕ Seeded demo invoice {Code}", DemoInvoiceCode);
        }

        var vnPayMethodId = context.PaymentMethods.First(pm => pm.MethodCode == "VNPAY").MethodId;

        var payment = context.Payments.FirstOrDefault(p => p.PaymentCode == DemoPaymentCode);
        if (payment == null)
        {
            payment = new Payment
            {
                PaymentCode = DemoPaymentCode,
                InvoiceId = invoice.InvoiceId,
                MethodId = vnPayMethodId,
                Amount = invoice.GrandTotal ?? 2400000m,
                ProcessingFee = 0,
                NetAmount = invoice.GrandTotal ?? 2400000m,
                PaymentDate = invoice.InvoiceDate?.AddDays(1),
                TransactionRef = "VNPAY-DEMO-001",
                Status = "Completed",
                Notes = "Thanh toán VNPay demo cho work order hoàn tất",
                CreatedDate = invoice.InvoiceDate?.AddDays(1)
            };

            context.Payments.Add(payment);
            context.SaveChanges();

            logger.LogInformation("➕ Seeded demo payment {Code}", DemoPaymentCode);
        }
    }

    private static void EnsureDemoWorkOrderPipeline(EVDbContext context, Customer customer, CustomerVehicle vehicle, ILogger logger)
    {
        // Ensure work order and invoice exist
        EnsureDemoWorkOrder(context, customer.CustomerId, vehicle.VehicleId, logger);
    }

    private static void EnsureDemoNotifications(EVDbContext context, Customer customer, User user, ILogger logger)
    {
        var existingCount = context.Notifications.Count(n => n.CustomerId == customer.CustomerId);
        if (existingCount >= 3)
        {
            return;
        }

        var now = DateTime.UtcNow;
        var newNotifications = new[]
        {
            new Notification
            {
                NotificationCode = $"{DemoNotificationCodePrefix}001",
                TemplateId = null,
                RecipientType = "Customer",
                UserId = user.UserId,
                CustomerId = customer.CustomerId,
                Channel = "InApp",
                Priority = "High",
                Subject = "Lịch bảo dưỡng sắp tới",
                Message = "Xe Model 3 của bạn sắp tới hạn bảo dưỡng. Đừng quên đặt lịch để được ưu đãi 10%.",
                RecipientAddress = DemoUsername,
                RecipientName = customer.FullName,
                ScheduledDate = null,
                SendDate = now.AddDays(-7),
                DeliveredDate = now.AddDays(-7),
                ReadDate = null,
                Status = "Sent",
                CreatedDate = now.AddDays(-7)
            },
            new Notification
            {
                NotificationCode = $"{DemoNotificationCodePrefix}002",
                TemplateId = null,
                RecipientType = "Customer",
                UserId = user.UserId,
                CustomerId = customer.CustomerId,
                Channel = "InApp",
                Priority = "Normal",
                Subject = "Thanh toán thành công",
                Message = "Bạn đã thanh toán thành công hóa đơn INV-DEMO-0001. Cảm ơn bạn đã sử dụng EV Service Center!",
                RecipientAddress = DemoUsername,
                RecipientName = customer.FullName,
                SendDate = now.AddMonths(-2),
                DeliveredDate = now.AddMonths(-2),
                ReadDate = now.AddMonths(-2).AddDays(1),
                Status = "Read",
                CreatedDate = now.AddMonths(-2)
            },
            new Notification
            {
                NotificationCode = $"{DemoNotificationCodePrefix}003",
                TemplateId = null,
                RecipientType = "Customer",
                UserId = user.UserId,
                CustomerId = customer.CustomerId,
                Channel = "Email",
                Priority = "Normal",
                Subject = "Ưu đãi gói bảo dưỡng cao cấp",
                Message = "Nhận ngay ưu đãi 20% khi nâng cấp gói bảo dưỡng Premium trong tháng này.",
                RecipientAddress = DemoUsername,
                RecipientName = customer.FullName,
                SendDate = now.AddDays(-3),
                DeliveredDate = now.AddDays(-3),
                ReadDate = null,
                Status = "Sent",
                CreatedDate = now.AddDays(-3)
            }
        };

        context.Notifications.AddRange(newNotifications);
        context.SaveChanges();

        logger.LogInformation("➕ Seeded {Count} demo notifications", newNotifications.Length);
    }
}
