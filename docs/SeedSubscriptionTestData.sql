-- =============================================
-- SEED TEST DATA FOR SUBSCRIPTION TESTING
-- =============================================
-- This script creates test subscriptions for testing appointment booking with subscriptions
-- Run this after seeding base data (Customers, MaintenancePackages, Services, etc.)
-- =============================================

USE EVServiceCenterV2;
GO

-- Check if MaintenancePackages exist
IF NOT EXISTS (SELECT 1 FROM MaintenancePackages WHERE PackageCode LIKE 'PKG-%')
BEGIN
    PRINT 'ERROR: MaintenancePackages not found. Please run MaintenancePackageSeeder first!';
    RETURN;
END

-- Check if Customers exist
IF NOT EXISTS (SELECT 1 FROM Customers WHERE CustomerCode LIKE 'KH%')
BEGIN
    PRINT 'ERROR: Customers not found. Please run CustomerSeeder first!';
    RETURN;
END

PRINT 'Starting Subscription seed...';

-- =============================================
-- 1. SEED CUSTOMER PACKAGE SUBSCRIPTIONS
-- =============================================

-- Get Package IDs
DECLARE @BasicPackageId INT = (SELECT TOP 1 PackageID FROM MaintenancePackages WHERE PackageCode = 'PKG-BASIC-2025');
DECLARE @PremiumPackageId INT = (SELECT TOP 1 PackageID FROM MaintenancePackages WHERE PackageCode = 'PKG-PREMIUM-2025');
DECLARE @VIPPackageId INT = (SELECT TOP 1 PackageID FROM MaintenancePackages WHERE PackageCode = 'PKG-VIP-2025');

-- Get Customer IDs (first 5 active customers)
DECLARE @Customer1Id INT = (SELECT TOP 1 CustomerID FROM Customers WHERE IsActive = 1 ORDER BY CustomerID);
DECLARE @Customer2Id INT = (SELECT CustomerID FROM (SELECT CustomerID, ROW_NUMBER() OVER (ORDER BY CustomerID) as rn FROM Customers WHERE IsActive = 1) t WHERE rn = 2);
DECLARE @Customer3Id INT = (SELECT CustomerID FROM (SELECT CustomerID, ROW_NUMBER() OVER (ORDER BY CustomerID) as rn FROM Customers WHERE IsActive = 1) t WHERE rn = 3);
DECLARE @Customer4Id INT = (SELECT CustomerID FROM (SELECT CustomerID, ROW_NUMBER() OVER (ORDER BY CustomerID) as rn FROM Customers WHERE IsActive = 1) t WHERE rn = 4);
DECLARE @Customer5Id INT = (SELECT CustomerID FROM (SELECT CustomerID, ROW_NUMBER() OVER (ORDER BY CustomerID) as rn FROM Customers WHERE IsActive = 1) t WHERE rn = 5);

-- Get Vehicle IDs for customers
DECLARE @Vehicle1Id INT = (SELECT TOP 1 VehicleID FROM CustomerVehicles WHERE CustomerID = @Customer1Id);
DECLARE @Vehicle2Id INT = (SELECT TOP 1 VehicleID FROM CustomerVehicles WHERE CustomerID = @Customer2Id);
DECLARE @Vehicle3Id INT = (SELECT TOP 1 VehicleID FROM CustomerVehicles WHERE CustomerID = @Customer3Id);
DECLARE @Vehicle4Id INT = (SELECT TOP 1 VehicleID FROM CustomerVehicles WHERE CustomerID = @Customer4Id);
DECLARE @Vehicle5Id INT = (SELECT TOP 1 VehicleID FROM CustomerVehicles WHERE CustomerID = @Customer5Id);

PRINT 'Package IDs: Basic=' + CAST(@BasicPackageId AS VARCHAR) + ', Premium=' + CAST(@PremiumPackageId AS VARCHAR) + ', VIP=' + CAST(@VIPPackageId AS VARCHAR);
PRINT 'Customer IDs: ' + CAST(@Customer1Id AS VARCHAR) + ', ' + CAST(@Customer2Id AS VARCHAR) + ', ' + CAST(@Customer3Id AS VARCHAR);

-- Delete existing test subscriptions if any
DELETE FROM PackageServiceUsages WHERE SubscriptionID IN (SELECT SubscriptionID FROM CustomerPackageSubscriptions WHERE SubscriptionCode LIKE 'SUB-%');
DELETE FROM CustomerPackageSubscriptions WHERE SubscriptionCode LIKE 'SUB-%';

-- Subscription 1: Customer 1 - Basic Package - Active (Unused)
INSERT INTO CustomerPackageSubscriptions (
    SubscriptionCode, CustomerID, PackageID, VehicleID,
    StartDate, ExpirationDate, Status, AutoRenew,
    OriginalPrice, DiscountPercent, DiscountAmount, PaymentAmount,
    PurchaseDate, InitialVehicleMileage,
    RemainingServices, UsedServices,
    CreatedDate, Notes
) VALUES (
    'SUB-2025-001', @Customer1Id, @BasicPackageId, @Vehicle1Id,
    DATEADD(DAY, -30, GETDATE()), DATEADD(DAY, 335, GETDATE()), 'Active', 0,
    2000000, 20, 400000, 1600000,
    DATEADD(DAY, -30, GETUTCDATE()), 5000,
    8, 0,
    DATEADD(DAY, -30, GETUTCDATE()), 'Test subscription for Basic package - Active, unused'
);

-- Subscription 2: Customer 2 - Premium Package - Active (Partially used)
INSERT INTO CustomerPackageSubscriptions (
    SubscriptionCode, CustomerID, PackageID, VehicleID,
    StartDate, ExpirationDate, Status, AutoRenew,
    OriginalPrice, DiscountPercent, DiscountAmount, PaymentAmount,
    PurchaseDate, InitialVehicleMileage,
    RemainingServices, UsedServices, LastServiceDate,
    CreatedDate, Notes
) VALUES (
    'SUB-2025-002', @Customer2Id, @PremiumPackageId, @Vehicle2Id,
    DATEADD(DAY, -90, GETDATE()), DATEADD(DAY, 275, GETDATE()), 'Active', 1,
    4500000, 25, 1125000, 3375000,
    DATEADD(DAY, -90, GETUTCDATE()), 8000,
    14, 2, DATEADD(DAY, -15, GETDATE()),
    DATEADD(DAY, -90, GETUTCDATE()), 'Test subscription for Premium package - Partially used'
);

-- Subscription 3: Customer 3 - VIP Package - Active (FOR TESTING)
INSERT INTO CustomerPackageSubscriptions (
    SubscriptionCode, CustomerID, PackageID, VehicleID,
    StartDate, ExpirationDate, Status, AutoRenew,
    OriginalPrice, DiscountPercent, DiscountAmount, PaymentAmount,
    PurchaseDate, InitialVehicleMileage,
    RemainingServices, UsedServices, LastServiceDate,
    CreatedDate, Notes
) VALUES (
    'SUB-2025-003', @Customer3Id, @VIPPackageId, @Vehicle3Id,
    DATEADD(DAY, -60, GETDATE()), DATEADD(DAY, 670, GETDATE()), 'Active', 1,
    8000000, 30, 2400000, 5600000,
    DATEADD(DAY, -60, GETUTCDATE()), 12000,
    131, 1, DATEADD(DAY, -20, GETDATE()),
    DATEADD(DAY, -60, GETUTCDATE()), 'Test subscription for VIP package - USE THIS FOR TESTING'
);

-- Subscription 4: Customer 4 - Premium Package - Expired
INSERT INTO CustomerPackageSubscriptions (
    SubscriptionCode, CustomerID, PackageID, VehicleID,
    StartDate, ExpirationDate, Status, AutoRenew,
    OriginalPrice, DiscountPercent, DiscountAmount, PaymentAmount,
    PurchaseDate, InitialVehicleMileage,
    RemainingServices, UsedServices, LastServiceDate,
    CreatedDate, Notes
) VALUES (
    'SUB-2024-004', @Customer4Id, @PremiumPackageId, @Vehicle4Id,
    DATEADD(DAY, -400, GETDATE()), DATEADD(DAY, -35, GETDATE()), 'Expired', 0,
    4500000, 25, 1125000, 3375000,
    DATEADD(DAY, -400, GETUTCDATE()), 3000,
    0, 16, DATEADD(DAY, -50, GETDATE()),
    DATEADD(DAY, -400, GETUTCDATE()), 'Test subscription - Expired'
);

-- Subscription 5: Customer 5 - Basic Package - Active (New)
INSERT INTO CustomerPackageSubscriptions (
    SubscriptionCode, CustomerID, PackageID, VehicleID,
    StartDate, ExpirationDate, Status, AutoRenew,
    OriginalPrice, DiscountPercent, DiscountAmount, PaymentAmount,
    PurchaseDate, InitialVehicleMileage,
    RemainingServices, UsedServices,
    CreatedDate, Notes
) VALUES (
    'SUB-2025-005', @Customer5Id, @BasicPackageId, @Vehicle5Id,
    DATEADD(DAY, -5, GETDATE()), DATEADD(DAY, 360, GETDATE()), 'Active', 0,
    2000000, 20, 400000, 1600000,
    DATEADD(DAY, -5, GETUTCDATE()), 15000,
    8, 0,
    DATEADD(DAY, -5, GETUTCDATE()), 'Test subscription - Newly purchased'
);

PRINT 'Created 5 subscriptions';

-- =============================================
-- 2. SEED PACKAGE SERVICE USAGE
-- =============================================

-- Get subscription IDs
DECLARE @Sub1Id INT = (SELECT SubscriptionID FROM CustomerPackageSubscriptions WHERE SubscriptionCode = 'SUB-2025-001');
DECLARE @Sub2Id INT = (SELECT SubscriptionID FROM CustomerPackageSubscriptions WHERE SubscriptionCode = 'SUB-2025-002');
DECLARE @Sub3Id INT = (SELECT SubscriptionID FROM CustomerPackageSubscriptions WHERE SubscriptionCode = 'SUB-2025-003');
DECLARE @Sub4Id INT = (SELECT SubscriptionID FROM CustomerPackageSubscriptions WHERE SubscriptionCode = 'SUB-2024-004');
DECLARE @Sub5Id INT = (SELECT SubscriptionID FROM CustomerPackageSubscriptions WHERE SubscriptionCode = 'SUB-2025-005');

PRINT 'Creating PackageServiceUsage records...';

-- Create usage records for each subscription
DECLARE @PackageId INT;
DECLARE @SubscriptionId INT;
DECLARE @ServiceId INT;
DECLARE @Quantity INT;
DECLARE @UsedQty INT;
DECLARE @Status NVARCHAR(20);

-- Cursor to iterate through subscriptions
DECLARE sub_cursor CURSOR FOR
SELECT SubscriptionID, PackageID, Status FROM CustomerPackageSubscriptions WHERE SubscriptionCode LIKE 'SUB-%';

OPEN sub_cursor;
FETCH NEXT FROM sub_cursor INTO @SubscriptionId, @PackageId, @Status;

WHILE @@FETCH_STATUS = 0
BEGIN
    -- Get package services
    DECLARE service_cursor CURSOR FOR
    SELECT ServiceID, Quantity FROM PackageServices WHERE PackageID = @PackageId AND IncludedInPackage = 1;

    OPEN service_cursor;
    FETCH NEXT FROM service_cursor INTO @ServiceId, @Quantity;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        -- Determine used quantity
        SET @UsedQty = 0;

        IF @SubscriptionId = @Sub2Id AND @ServiceId % 2 = 0
            SET @UsedQty = 1; -- Sub 2: Premium - some services used
        ELSE IF @SubscriptionId = @Sub3Id AND @ServiceId = (SELECT TOP 1 ServiceID FROM MaintenanceServices ORDER BY ServiceID)
            SET @UsedQty = 1; -- Sub 3: VIP - first service used
        ELSE IF @Status = 'Expired'
            SET @UsedQty = @Quantity; -- Expired: all used

        -- Insert usage record
        INSERT INTO PackageServiceUsages (
            SubscriptionID, ServiceID,
            TotalAllowedQuantity, UsedQuantity, RemainingQuantity,
            LastUsedDate,
            Notes
        ) VALUES (
            @SubscriptionId, @ServiceId,
            @Quantity, @UsedQty, @Quantity - @UsedQty,
            CASE WHEN @UsedQty > 0 THEN GETDATE() ELSE NULL END,
            'Auto-generated usage tracking'
        );

        FETCH NEXT FROM service_cursor INTO @ServiceId, @Quantity;
    END;

    CLOSE service_cursor;
    DEALLOCATE service_cursor;

    FETCH NEXT FROM sub_cursor INTO @SubscriptionId, @PackageId, @Status;
END;

CLOSE sub_cursor;
DEALLOCATE sub_cursor;

PRINT 'Created PackageServiceUsage records';

-- =============================================
-- 3. VERIFICATION
-- =============================================

PRINT '';
PRINT '========== VERIFICATION ==========';
PRINT 'Total Subscriptions: ' + CAST((SELECT COUNT(*) FROM CustomerPackageSubscriptions WHERE SubscriptionCode LIKE 'SUB-%') AS VARCHAR);
PRINT 'Total Usage Records: ' + CAST((SELECT COUNT(*) FROM PackageServiceUsages WHERE SubscriptionID IN (SELECT SubscriptionID FROM CustomerPackageSubscriptions WHERE SubscriptionCode LIKE 'SUB-%')) AS VARCHAR);
PRINT '';
PRINT '========== SUBSCRIPTION DETAILS ==========';

SELECT
    s.SubscriptionCode,
    s.Status,
    c.FullName AS CustomerName,
    c.CustomerCode,
    p.PackageName,
    s.StartDate,
    s.ExpirationDate,
    s.PaymentAmount,
    s.RemainingServices,
    s.UsedServices
FROM CustomerPackageSubscriptions s
JOIN Customers c ON s.CustomerID = c.CustomerID
JOIN MaintenancePackages p ON s.PackageID = p.PackageID
WHERE s.SubscriptionCode LIKE 'SUB-%'
ORDER BY s.SubscriptionID;

PRINT '';
PRINT '========== USAGE SUMMARY ==========';

SELECT
    s.SubscriptionCode,
    p.PackageName,
    COUNT(*) AS TotalServices,
    SUM(psu.TotalAllowedQuantity) AS TotalAllowed,
    SUM(psu.UsedQuantity) AS TotalUsed,
    SUM(psu.RemainingQuantity) AS TotalRemaining
FROM PackageServiceUsages psu
JOIN CustomerPackageSubscriptions s ON psu.SubscriptionID = s.SubscriptionID
JOIN MaintenancePackages p ON s.PackageID = p.PackageID
WHERE s.SubscriptionCode LIKE 'SUB-%'
GROUP BY s.SubscriptionCode, p.PackageName
ORDER BY s.SubscriptionCode;

PRINT '';
PRINT '========== TEST INSTRUCTIONS ==========';
PRINT 'Use Subscription ID = 3 (SUB-2025-003) for testing appointment booking';
PRINT 'Customer for Sub-3: ' + CAST(@Customer3Id AS VARCHAR);
PRINT 'Vehicle for Sub-3: ' + CAST(@Vehicle3Id AS VARCHAR);
PRINT '';
PRINT 'Seed completed successfully!';
GO
