using EVServiceCenter.Core.Entities;
using EVServiceCenter.Core.Domains.ServiceCenters.Entities;
using EVServiceCenter.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;

namespace EVServiceCenter.Infrastructure.Persistence.Seeders;

/// <summary>
/// Seed sample inventory data (parts, suppliers, part inventory) for demo/testing.
/// </summary>
public static class InventoryDemoSeeder
{
    public static void Seed(EVDbContext context, ILogger logger)
    {
        try
        {
            // Only seed when inventory is empty to avoid duplicating data
            if (context.PartInventories.Any())
            {
                logger.LogInformation("Inventory already contains data. Skipping inventory demo seeding.");
                return;
            }

            var now = DateTime.UtcNow;

            // Ensure service centers exist (other seeders should have created them)
            var centers = context.ServiceCenters
                .OrderBy(c => c.CenterId)
                .Take(3)
                .ToList();

            if (!centers.Any())
            {
                logger.LogWarning("No service centers found. Inventory demo seeding skipped.");
                return;
            }

            // Create or reuse suppliers
            var suppliers = EnsureSuppliers(context, now);

            // Create or reuse part categories
            var categories = EnsureCategories(context);

            context.SaveChanges();

            // Create / update demo parts
            var parts = EnsureParts(context, categories, suppliers, now);
            context.SaveChanges();

            // Seed inventory per center
            SeedPartInventory(context, parts, centers, now);
            context.SaveChanges();

            // Update part aggregated stock numbers
            UpdatePartStockSummary(context, parts, now);
            context.SaveChanges();

            logger.LogInformation("Inventory demo data seeded: {PartCount} parts across {CenterCount} centers.",
                parts.Count, centers.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error seeding inventory demo data.");
        }
    }

    private static Dictionary<string, Supplier> EnsureSuppliers(EVDbContext context, DateTime now)
    {
        var suppliers = new Dictionary<string, Supplier>();

        suppliers["EV Power Components"] = GetOrCreateSupplier(
            context,
            code: "EVSP-001",
            name: "EV Power Components",
            contact: "Nguyen Van Binh",
            phone: "028-7300-1111",
            email: "sales@evpower.com",
            address: "123 Nguyen Van Troi, Phu Nhuan, HCM",
            city: "HCMC",
            paymentTerms: "Net 30",
            creditLimit: 500_000_000m,
            rating: 5,
            isPreferred: true,
            now);

        suppliers["Smart Mobility Supply"] = GetOrCreateSupplier(
            context,
            code: "EVSP-002",
            name: "Smart Mobility Supply",
            contact: "Tran Thi Thao",
            phone: "024-7300-2222",
            email: "support@smartmobility.vn",
            address: "456 Nguyen Trai, Thanh Xuan, Ha Noi",
            city: "Ha Noi",
            paymentTerms: "Net 15",
            creditLimit: 300_000_000m,
            rating: 4,
            isPreferred: true,
            now);

        suppliers["GreenParts Depot"] = GetOrCreateSupplier(
            context,
            code: "EVSP-003",
            name: "GreenParts Depot",
            contact: "Le Quoc Huy",
            phone: "028-7300-3333",
            email: "orders@greenparts.vn",
            address: "789 Dien Bien Phu, Binh Thanh, HCM",
            city: "HCMC",
            paymentTerms: "Prepaid",
            creditLimit: 150_000_000m,
            rating: 4,
            isPreferred: false,
            now);

        return suppliers;
    }

    private static Supplier GetOrCreateSupplier(
        EVDbContext context,
        string code,
        string name,
        string contact,
        string phone,
        string email,
        string address,
        string city,
        string paymentTerms,
        decimal creditLimit,
        int rating,
        bool isPreferred,
        DateTime now)
    {
        var supplier = context.Suppliers.FirstOrDefault(s => s.SupplierCode == code);
        if (supplier != null)
        {
            return supplier;
        }

        supplier = new Supplier
        {
            SupplierCode = code,
            SupplierName = name,
            ContactName = contact,
            PhoneNumber = phone,
            Email = email,
            Address = address,
            City = city,
            PaymentTerms = paymentTerms,
            CreditLimit = creditLimit,
            Rating = rating,
            IsPreferred = isPreferred,
            IsActive = true,
            CreatedDate = now
        };

        context.Suppliers.Add(supplier);
        return supplier;
    }

    private static Dictionary<string, PartCategory> EnsureCategories(EVDbContext context)
    {
        var categories = new Dictionary<string, PartCategory>();

        categories["Battery System"] = GetOrCreateCategory(
            context,
            "Battery System",
            "EV high-voltage battery packs, modules, and thermal components.");

        categories["Chassis & Suspension"] = GetOrCreateCategory(
            context,
            "Chassis & Suspension",
            "Shock absorbers, springs, brake components for EV platforms.");

        categories["Electronics & Control"] = GetOrCreateCategory(
            context,
            "Electronics & Control",
            "Charging stations, control units, inverters, and power electronics.");

        categories["Thermal Management"] = GetOrCreateCategory(
            context,
            "Thermal Management",
            "Coolant loops, pumps, and components dedicated to battery & motor cooling.");

        categories["Interior & Comfort"] = GetOrCreateCategory(
            context,
            "Interior & Comfort",
            "HVAC consumables, cabin filters, and comfort-related accessories.");

        categories["Safety & Sensors"] = GetOrCreateCategory(
            context,
            "Safety & Sensors",
            "ABS sensors, safety controllers, ADAS-related replacement parts.");

        categories["Tire & Wheel"] = GetOrCreateCategory(
            context,
            "Tire & Wheel",
            "High-performance EV tires, TPMS sensors, and wheel accessories.");

        return categories;
    }

    private static PartCategory GetOrCreateCategory(EVDbContext context, string name, string description)
    {
        var category = context.PartCategories.FirstOrDefault(c => c.CategoryName == name);
        if (category != null)
        {
            return category;
        }

        category = new PartCategory
        {
            CategoryName = name,
            Description = description,
            IsActive = true
        };

        context.PartCategories.Add(category);
        return category;
    }

    private static List<Part> EnsureParts(
        EVDbContext context,
        Dictionary<string, PartCategory> categories,
        Dictionary<string, Supplier> suppliers,
        DateTime now)
    {
        var result = new List<Part>();

        var partDefinitions = new[]
        {
            new
            {
                Code = "EV-BAT-60",
                Name = "EV Battery Module 60kWh",
                Category = categories["Battery System"],
                Supplier = suppliers["EV Power Components"],
                Unit = "PCS",
                Cost = 60_000_000m,
                Price = 75_000_000m,
                MinStock = 2,
                Reorder = 3,
                MaxStock = 12,
                Location = "A1-01",
                Weight = 180.5m,
                Dimensions = "1200x800x150mm",
                Warranty = 24,
                Condition = "New",
                Consumable = false,
                Specs = "Lithium-ion module, 400V system",
                Models = "Falcon X; Falcon Y"
            },
            new
            {
                Code = "EV-BRAKE-SET",
                Name = "Regenerative Brake Pad Set",
                Category = categories["Chassis & Suspension"],
                Supplier = suppliers["GreenParts Depot"],
                Unit = "SET",
                Cost = 1_200_000m,
                Price = 2_900_000m,
                MinStock = 5,
                Reorder = 6,
                MaxStock = 30,
                Location = "B1-02",
                Weight = 4.2m,
                Dimensions = "320x220x80mm",
                Warranty = 12,
                Condition = "New",
                Consumable = true,
                Specs = "Low-dust ceramic, regenerative braking optimized",
                Models = "Falcon X; Falcon City"
            },
            new
            {
                Code = "EV-CHG-11KW",
                Name = "11kW Wallbox Charger",
                Category = categories["Electronics & Control"],
                Supplier = suppliers["Smart Mobility Supply"],
                Unit = "PCS",
                Cost = 9_500_000m,
                Price = 15_900_000m,
                MinStock = 3,
                Reorder = 4,
                MaxStock = 20,
                Location = "C2-05",
                Weight = 8.7m,
                Dimensions = "400x300x120mm",
                Warranty = 18,
                Condition = "New",
                Consumable = false,
                Specs = "11kW AC charger, Type 2, smart load balancing",
                Models = "Compatible with all EV models"
            },
            new
            {
                Code = "EV-COOLANT-5L",
                Name = "Battery Coolant Pack 5L",
                Category = categories["Thermal Management"],
                Supplier = suppliers["EV Power Components"],
                Unit = "BOTTLE",
                Cost = 850_000m,
                Price = 1_450_000m,
                MinStock = 8,
                Reorder = 12,
                MaxStock = 60,
                Location = "C1-03",
                Weight = 5.2m,
                Dimensions = "300x180x120mm",
                Warranty = 12,
                Condition = "New",
                Consumable = true,
                Specs = "EV-grade glycol coolant, compatible with PIN-COOL service",
                Models = "Falcon X; Falcon City; Falcon Cargo"
            },
            new
            {
                Code = "EV-HVAC-FILTER",
                Name = "HEPA Cabin Filter Gen2",
                Category = categories["Interior & Comfort"],
                Supplier = suppliers["Smart Mobility Supply"],
                Unit = "PCS",
                Cost = 320_000m,
                Price = 720_000m,
                MinStock = 15,
                Reorder = 20,
                MaxStock = 120,
                Location = "D1-04",
                Weight = 0.6m,
                Dimensions = "260x210x40mm",
                Warranty = 6,
                Condition = "New",
                Consumable = true,
                Specs = "HEPA + activated carbon layer, used in HVAC refresh service",
                Models = "Falcon X; Falcon City; Falcon SUV"
            },
            new
            {
                Code = "EV-ABS-SENSOR",
                Name = "ABS Speed Sensor Kit",
                Category = categories["Safety & Sensors"],
                Supplier = suppliers["GreenParts Depot"],
                Unit = "SET",
                Cost = 780_000m,
                Price = 1_850_000m,
                MinStock = 6,
                Reorder = 8,
                MaxStock = 35,
                Location = "B2-03",
                Weight = 1.1m,
                Dimensions = "240x160x70mm",
                Warranty = 18,
                Condition = "New",
                Consumable = false,
                Specs = "Wheel speed sensor + harness, matches brake diagnostics service",
                Models = "Falcon X; Falcon City"
            },
            new
            {
                Code = "EV-TIRE-19",
                Name = "19\" EV Performance Tire",
                Category = categories["Tire & Wheel"],
                Supplier = suppliers["Smart Mobility Supply"],
                Unit = "PCS",
                Cost = 4_200_000m,
                Price = 6_500_000m,
                MinStock = 12,
                Reorder = 16,
                MaxStock = 80,
                Location = "E1-01",
                Weight = 22.5m,
                Dimensions = "680x680x240mm",
                Warranty = 24,
                Condition = "New",
                Consumable = true,
                Specs = "Low rolling resistance, foam-lined for cabin quietness",
                Models = "Falcon SUV; Falcon Y"
            },
            new
            {
                Code = "EV-INVERTER-400",
                Name = "400V Drive Inverter Module",
                Category = categories["Electronics & Control"],
                Supplier = suppliers["EV Power Components"],
                Unit = "PCS",
                Cost = 18_000_000m,
                Price = 26_500_000m,
                MinStock = 2,
                Reorder = 3,
                MaxStock = 10,
                Location = "C3-02",
                Weight = 14.3m,
                Dimensions = "420x320x140mm",
                Warranty = 24,
                Condition = "New",
                Consumable = false,
                Specs = "400V SiC inverter, used in Motor diagnostics/repairs",
                Models = "Falcon X; Falcon Performance"
            }
        };

        foreach (var def in partDefinitions)
        {
            var part = context.Parts.FirstOrDefault(p => p.PartCode == def.Code);
            if (part == null)
            {
                part = new Part
                {
                    PartCode = def.Code,
                    PartName = def.Name,
                    CategoryId = def.Category.CategoryId,
                    SupplierId = def.Supplier.SupplierId,
                    Unit = def.Unit,
                    CostPrice = def.Cost,
                    SellingPrice = def.Price,
                    MinStock = def.MinStock,
                    ReorderLevel = def.Reorder,
                    MaxStock = def.MaxStock,
                    Location = def.Location,
                    Weight = def.Weight,
                    Dimensions = def.Dimensions,
                    WarrantyPeriod = def.Warranty,
                    PartCondition = def.Condition,
                    IsConsumable = def.Consumable,
                    IsActive = true,
                    TechnicalSpecs = def.Specs,
                    CompatibleModels = def.Models,
                    ImageUrl = null,
                    CreatedDate = now,
                    LastStockUpdateDate = now
                };
                context.Parts.Add(part);
            }
            else
            {
                part.CategoryId = def.Category.CategoryId;
                part.SupplierId = def.Supplier.SupplierId;
                part.Unit = def.Unit;
                part.CostPrice = def.Cost;
                part.SellingPrice = def.Price;
                part.MinStock = def.MinStock;
                part.ReorderLevel = def.Reorder;
                part.MaxStock = def.MaxStock;
                part.Location = def.Location;
                part.Weight = def.Weight;
                part.Dimensions = def.Dimensions;
                part.WarrantyPeriod = def.Warranty;
                part.PartCondition = def.Condition;
                part.IsConsumable = def.Consumable;
                part.TechnicalSpecs = def.Specs;
                part.CompatibleModels = def.Models;
                part.LastStockUpdateDate = now;
                part.IsActive = true;
            }

            result.Add(part);
        }

        return result;
    }

    private static void SeedPartInventory(
        EVDbContext context,
        List<Part> parts,
        List<ServiceCenter> centers,
        DateTime now)
    {
        var inventoryEntries = new List<(string PartCode, int CenterId, int Current, int Reserved, string Location)>
        {
            ("EV-BAT-60", centers[0].CenterId, 4, 1, "A1-01"),
            ("EV-BAT-60", centers[1].CenterId, 2, 0, "A1-02"),
            ("EV-BAT-60", centers[2].CenterId, 1, 0, "A1-03"),
            ("EV-BRAKE-SET", centers[0].CenterId, 1, 0, "B1-02"),
            ("EV-BRAKE-SET", centers[1].CenterId, 1, 0, "B1-03"),
            ("EV-CHG-11KW", centers[2].CenterId, 5, 2, "C2-05"),
            ("EV-CHG-11KW", centers[0].CenterId, 2, 0, "C2-01"),
            ("EV-COOLANT-5L", centers[0].CenterId, 12, 2, "C1-01"),
            ("EV-COOLANT-5L", centers[1].CenterId, 8, 1, "C1-02"),
            ("EV-HVAC-FILTER", centers[0].CenterId, 20, 4, "D1-01"),
            ("EV-HVAC-FILTER", centers[2].CenterId, 18, 3, "D1-02"),
            ("EV-ABS-SENSOR", centers[1].CenterId, 6, 1, "B2-01"),
            ("EV-ABS-SENSOR", centers[2].CenterId, 4, 0, "B2-02"),
            ("EV-TIRE-19", centers[0].CenterId, 12, 2, "E1-02"),
            ("EV-TIRE-19", centers[1].CenterId, 10, 2, "E1-03"),
            ("EV-INVERTER-400", centers[0].CenterId, 3, 1, "C3-02"),
            ("EV-INVERTER-400", centers[2].CenterId, 2, 0, "C3-03")
        };

        foreach (var entry in inventoryEntries)
        {
            var part = parts.FirstOrDefault(p => p.PartCode == entry.PartCode);
            if (part == null)
            {
                continue;
            }

            var existing = context.PartInventories
                .FirstOrDefault(pi => pi.PartId == part.PartId && pi.CenterId == entry.CenterId);

            var available = entry.Current - entry.Reserved;
            if (available < 0) available = 0;

            if (existing == null)
            {
                context.PartInventories.Add(new PartInventory
                {
                    PartId = part.PartId,
                    CenterId = entry.CenterId,
                    CurrentStock = entry.Current,
                    ReservedStock = entry.Reserved,
                    AvailableStock = available,
                    Location = entry.Location,
                    UpdatedDate = now
                });
            }
            else
            {
                existing.CurrentStock = entry.Current;
                existing.ReservedStock = entry.Reserved;
                existing.AvailableStock = available;
                existing.Location = entry.Location;
                existing.UpdatedDate = now;
            }
        }
    }

    private static void UpdatePartStockSummary(EVDbContext context, List<Part> parts, DateTime now)
    {
        var partIds = parts.Select(p => p.PartId).ToList();
        if (!partIds.Any())
        {
            return;
        }

        var aggregates = context.PartInventories
            .Where(pi => partIds.Contains(pi.PartId))
            .GroupBy(pi => pi.PartId)
            .Select(g => new
            {
                PartId = g.Key,
                Current = g.Sum(pi => pi.CurrentStock ?? 0),
                Reserved = g.Sum(pi => pi.ReservedStock ?? 0),
                Available = g.Sum(pi => pi.AvailableStock ?? 0)
            })
            .ToList();

        foreach (var agg in aggregates)
        {
            var part = parts.FirstOrDefault(p => p.PartId == agg.PartId);
            if (part == null)
            {
                continue;
            }

            part.CurrentStock = agg.Current;
            part.LastStockUpdateDate = now;
        }
    }
}
