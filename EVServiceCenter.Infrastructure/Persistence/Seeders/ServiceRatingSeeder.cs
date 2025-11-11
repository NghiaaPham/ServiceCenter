using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using EVServiceCenter.Core.Entities;
using EVServiceCenter.Core.Domains.Customers.Entities;
using EVServiceCenter.Core.Domains.CustomerVehicles.Entities;
using EVServiceCenter.Core.Domains.CarBrands.Entities;
using EVServiceCenter.Core.Domains.CarModels.Entities;
using EVServiceCenter.Core.Domains.ServiceCenters.Entities;
using EVServiceCenter.Core.Domains.CustomerTypes.Entities;

namespace EVServiceCenter.Infrastructure.Persistence.Seeders
{
    public static class ServiceRatingSeeder
    {
        public static void SeedDemoTestimonials(EVDbContext context, ILogger logger)
        {
            try
            {
                var existingCount = context.ServiceRatings.Count();
                logger.LogInformation("ServiceRatingSeeder: existing testimonials count = {Count}", existingCount);

                // If already seeded, skip
                if (existingCount > 0)
                {
                    logger.LogInformation("ServiceRatingSeeder: testimonials exist, skipping seeding.");
                    return;
                }

                var modelsToSeed = new List<CarModel>();

                // Ensure a demo brand exists
                var demoBrand = context.CarBrands.FirstOrDefault(b => b.BrandName == "DemoBrand");
                if (demoBrand == null)
                {
                    demoBrand = new CarBrand
                    {
                        BrandName = "DemoBrand",
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    };
                    context.CarBrands.Add(demoBrand);
                    context.SaveChanges();
                }

                // Ensure there is a model with ModelId = 10 so queries like ?modelId=10 return data
                var modelWithId10 = context.CarModels.Find(10);
                if (modelWithId10 == null)
                {
                    logger.LogInformation("ServiceRatingSeeder: ModelId=10 not found, attempting IDENTITY_INSERT to create it.");
                    try
                    {
                        var tableName = "dbo.CarModels";
                        var modelName = "DemoModel-10";
                        var year = DateTime.Now.Year;

                        var sqlOn = $"SET IDENTITY_INSERT {tableName} ON;";
                        var insert = $@"INSERT INTO {tableName} (ModelId, BrandId, ModelName, [Year], IsActive) VALUES (10, {demoBrand.BrandId}, '{modelName}', {year}, 1);";
                        var sqlOff = $"SET IDENTITY_INSERT {tableName} OFF;";

                        context.Database.ExecuteSqlRaw(sqlOn + insert + sqlOff);

                        modelWithId10 = context.CarModels.Find(10);
                        if (modelWithId10 != null)
                        {
                            logger.LogInformation("ServiceRatingSeeder: Successfully created CarModel with ModelId=10.");
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "ServiceRatingSeeder: failed to create CarModel with explicit ID=10 via IDENTITY_INSERT. Will fall back to regular demo model creation.");
                    }
                }

                if (modelWithId10 != null)
                    modelsToSeed.Add(modelWithId10);

                // Create a demo model named DemoModel-10 if not already present (fallback)
                var demoModel = context.CarModels.FirstOrDefault(m => m.ModelName != null && m.ModelName.Contains("DemoModel-10"));
                if (demoModel == null)
                {
                    demoModel = new CarModel
                    {
                        BrandId = demoBrand.BrandId,
                        ModelName = "DemoModel-10",
                        Year = DateTime.Now.Year,
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    };
                    context.CarModels.Add(demoModel);
                    context.SaveChanges();

                    logger.LogInformation("ServiceRatingSeeder: Created demo model with ModelId={ModelId}", demoModel.ModelId);
                }

                if (!modelsToSeed.Any(m => m.ModelId == demoModel.ModelId))
                    modelsToSeed.Add(demoModel);

                // Add up to 3 existing models (take first few) to have broader coverage
                var existingModels = context.CarModels.AsNoTracking().Take(3).ToList();
                foreach (var m in existingModels)
                {
                    if (!modelsToSeed.Any(x => x.ModelId == m.ModelId))
                        modelsToSeed.Add(context.CarModels.Find(m.ModelId)!);
                }

                // Ensure there is at least one service center
                var center = context.ServiceCenters.FirstOrDefault();
                if (center == null)
                {
                    center = new ServiceCenter
                    {
                        CenterName = "Demo Center",
                        Address = "Demo Address",
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    };
                    context.ServiceCenters.Add(center);
                    context.SaveChanges();
                }

                // Use provided default CustomerType Id = 145
                const int requestedDefaultTypeId = 145;
                CustomerType defaultCustomerType = context.CustomerTypes.Find(requestedDefaultTypeId);
                if (defaultCustomerType == null)
                {
                    try
                    {
                        // Insert only existing columns: TypeID, TypeName, DiscountPercent, IsActive
                        var sql = $@"SET IDENTITY_INSERT dbo.CustomerTypes ON;
INSERT INTO dbo.CustomerTypes (TypeID, TypeName, DiscountPercent, IsActive)
VALUES ({requestedDefaultTypeId}, 'Standard', 0, 1);
SET IDENTITY_INSERT dbo.CustomerTypes OFF;";
                        context.Database.ExecuteSqlRaw(sql);
                        defaultCustomerType = context.CustomerTypes.Find(requestedDefaultTypeId);
                        if (defaultCustomerType != null)
                            logger.LogInformation("ServiceRatingSeeder: Created CustomerType with TypeId={TypeId}", requestedDefaultTypeId);
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "ServiceRatingSeeder: Failed to create CustomerType with explicit ID {TypeId} via IDENTITY_INSERT. Falling back to creating a normal CustomerType.");
                    }

                    if (defaultCustomerType == null)
                    {
                        defaultCustomerType = context.CustomerTypes.FirstOrDefault(ct => ct.TypeName == "Standard");
                        if (defaultCustomerType == null)
                        {
                            defaultCustomerType = new CustomerType
                            {
                                TypeName = "Standard",
                                DiscountPercent = 0,
                                IsActive = true
                            };
                            context.CustomerTypes.Add(defaultCustomerType);
                            context.SaveChanges();
                            logger.LogInformation("ServiceRatingSeeder: Created fallback CustomerType 'Standard' with TypeId={TypeId}", defaultCustomerType.TypeId);
                        }
                    }
                }

                int defaultTypeId = defaultCustomerType.TypeId;

                int seededRatings = 0;

                // For each model, create demo customer, vehicle, workorder and ratings
                foreach (var model in modelsToSeed)
                {
                    if (model == null) continue;

                    for (int i = 1; i <= 3; i++)
                    {
                        var email = $"demo.{model.ModelId}.user{i}@example.com";
                        var customer = context.Customers.FirstOrDefault(c => c.Email == email);
                        if (customer == null)
                        {
                            customer = new Customer
                            {
                                CustomerCode = $"DM{model.ModelId:000}{i:00}",
                                FullName = $"Demo User {model.ModelId}-{i}",
                                Email = email,
                                PhoneNumber = $"090{1000000 + model.ModelId * 10 + i}",
                                Address = "Seeded demo address",
                                TypeId = defaultTypeId,
                                CreatedDate = DateTime.UtcNow.AddDays(-i * 5),
                                IsActive = true
                            };
                            context.Customers.Add(customer);
                            context.SaveChanges();
                        }

                        // create vehicle for customer with this model
                        var license = $"51K-{model.ModelId % 1000}-{i:000}";
                        var vehicle = context.CustomerVehicles.FirstOrDefault(v => v.LicensePlate == license && v.CustomerId == customer.CustomerId);
                        if (vehicle == null)
                        {
                            vehicle = new CustomerVehicle
                            {
                                CustomerId = customer.CustomerId,
                                ModelId = model.ModelId,
                                LicensePlate = license,
                                CreatedDate = DateTime.UtcNow.AddDays(-i),
                                IsActive = true
                            };
                            context.CustomerVehicles.Add(vehicle);
                            context.SaveChanges();
                        }

                        // create work order
                        var workOrderCode = $"WO-DEMO-{model.ModelId}-{i}-{DateTime.UtcNow.Ticks % 100000}";
                        var workOrder = new WorkOrder
                        {
                            WorkOrderCode = workOrderCode,
                            CustomerId = customer.CustomerId,
                            VehicleId = vehicle.VehicleId,
                            ServiceCenterId = center.CenterId,
                            CreatedDate = DateTime.UtcNow.AddDays(-i),
                            StatusId = context.WorkOrderStatuses.Select(s => s.StatusId).FirstOrDefault()
                        };
                        context.WorkOrders.Add(workOrder);
                        context.SaveChanges();

                        // create 2 ratings per work order
                        var r1 = new ServiceRating
                        {
                            WorkOrderId = workOrder.WorkOrderId,
                            CustomerId = customer.CustomerId,
                            OverallRating = 5 - (i % 2),
                            ServiceQuality = 5 - (i % 3),
                            StaffProfessionalism = 5,
                            FacilityQuality = 4,
                            WaitingTime = 3 + i % 2,
                            PriceValue = 4,
                            CommunicationQuality = 5,
                            PositiveFeedback = "Nhân viên nhi?t tình, x? lý nhanh.",
                            NegativeFeedback = i % 2 == 0 ? "Ch? lâu lúc cao ?i?m." : null,
                            Suggestions = i % 2 == 0 ? "C?n cung c?p n??c u?ng." : "Không c?n thay ??i.",
                            WouldRecommend = true,
                            WouldReturn = true,
                            RatingDate = DateTime.UtcNow.AddDays(-i * 2)
                        };

                        var r2 = new ServiceRating
                        {
                            WorkOrderId = workOrder.WorkOrderId,
                            CustomerId = customer.CustomerId,
                            OverallRating = Math.Max(3, 5 - (i % 4)),
                            ServiceQuality = 4,
                            StaffProfessionalism = 4,
                            FacilityQuality = 3,
                            WaitingTime = 4,
                            PriceValue = 3,
                            CommunicationQuality = 4,
                            PositiveFeedback = "K? thu?t viên có kinh nghi?m.",
                            NegativeFeedback = i % 3 == 0 ? "Ph? tùng thay th? ??t." : null,
                            Suggestions = "C?n thông báo ti?n ?? rõ ràng h?n.",
                            WouldRecommend = i % 3 != 0,
                            WouldReturn = true,
                            RatingDate = DateTime.UtcNow.AddDays(-i * 3)
                        };

                        context.ServiceRatings.AddRange(new[] { r1, r2 });
                        context.SaveChanges();

                        seededRatings += 2;
                    }
                }

                logger.LogInformation("ServiceRatingSeeder: Seeded additional {Count} demo testimonials", seededRatings);

                var seededModelIds = modelsToSeed.Where(m => m != null).Select(m => m.ModelId).Distinct();
                logger.LogInformation("ServiceRatingSeeder: Demo testimonials created for ModelIds: {ModelIds}", string.Join(',', seededModelIds));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "ServiceRatingSeeder: Failed to seed testimonials");
            }
        }
    }
}
