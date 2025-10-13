-- ========================================
-- SMART SUBSCRIPTION LOGIC - TEST DATA SETUP
-- ========================================
-- Execute this script to create test data for testing Smart Subscription Logic
-- Author: AI Assistant
-- Date: December 2024

USE EVServiceCenterDB;
GO

-- ========================================
-- CLEANUP (Optional - Run if you want to start fresh)
-- ========================================
/*
DELETE FROM AppointmentServices WHERE AppointmentID IN (SELECT AppointmentID FROM Appointments WHERE CustomerID IN (1001, 1002));
DELETE FROM Appointments WHERE CustomerID IN (1001, 1002);
DELETE FROM PackageServiceUsages WHERE SubscriptionID IN (SELECT SubscriptionID FROM CustomerPackageSubscriptions WHERE CustomerID IN (1001, 1002));
DELETE FROM CustomerPackageSubscriptions WHERE CustomerID IN (1001, 1002);
DELETE FROM CustomerVehicles WHERE CustomerID IN (1001, 1002);
DELETE FROM Customers WHERE CustomerID IN (1001, 1002);
*/

-- ========================================
-- 1. CREATE TEST CUSTOMERS
-- ========================================
PRINT 'Creating test customers...';

SET IDENTITY_INSERT Customers ON;

INSERT INTO Customers (CustomerID, FullName, PhoneNumber, Email, DateOfBirth, Address, City, District, IsActive, CreatedDate)
VALUES 
(1001, 'Test Customer A', '0901234001', 'testA@test.com', '1990-01-01', '123 Test St', 'Hà N?i', 'Ba ?ình', 1, GETUTCDATE()),
(1002, 'Test Customer B', '0901234002', 'testB@test.com', '1992-05-15', '456 Test Ave', 'Hà N?i', 'C?u Gi?y', 1, GETUTCDATE());

SET IDENTITY_INSERT Customers OFF;

PRINT 'Created 2 test customers (IDs: 1001, 1002)';

-- ========================================
-- 2. CREATE TEST VEHICLES
-- ========================================
PRINT 'Creating test vehicles...';

DECLARE @VF8ModelID INT = (SELECT TOP 1 ModelID FROM CarModels WHERE ModelName LIKE '%VF8%');

IF @VF8ModelID IS NULL
BEGIN
    PRINT 'ERROR: VF8 model not found. Please create VF8 model first.';
    RETURN;
END

SET IDENTITY_INSERT CustomerVehicles ON;

INSERT INTO CustomerVehicles (VehicleID, CustomerID, ModelID, PlateNumber, VIN, Year, Color, Mileage, PurchaseDate, IsActive, CreatedDate)
VALUES 
(2001, 1001, @VF8ModelID, '30TEST-001', 'VIN001TEST', 2024, '??', 5000, '2024-01-01', 1, GETUTCDATE()),
(2002, 1002, @VF8ModelID, '30TEST-002', 'VIN002TEST', 2024, 'Xanh', 3000, '2024-06-01', 1, GETUTCDATE());

SET IDENTITY_INSERT CustomerVehicles OFF;

PRINT 'Created 2 test vehicles (IDs: 2001, 2002)';

-- ========================================
-- 3. GET SERVICE IDs
-- ========================================
PRINT 'Getting service IDs...';

DECLARE @ServiceOilChangeID INT = (SELECT TOP 1 ServiceID FROM MaintenanceServices WHERE ServiceName LIKE '%d?u%');
DECLARE @ServiceBrakeCheckID INT = (SELECT TOP 1 ServiceID FROM MaintenanceServices WHERE ServiceName LIKE '%phanh%');
DECLARE @ServiceCarWashID INT = (SELECT TOP 1 ServiceID FROM MaintenanceServices WHERE ServiceName LIKE '%r?a%');

IF @ServiceOilChangeID IS NULL OR @ServiceBrakeCheckID IS NULL
BEGIN
    PRINT 'ERROR: Required services not found. Please create services first.';
    RETURN;
END

PRINT 'Service IDs:';
PRINT '  - Oil Change: ' + CAST(@ServiceOilChangeID AS VARCHAR(10));
PRINT '  - Brake Check: ' + CAST(@ServiceBrakeCheckID AS VARCHAR(10));
PRINT '  - Car Wash: ' + CAST(ISNULL(@ServiceCarWashID, 0) AS VARCHAR(10));

-- ========================================
-- 4. GET PACKAGE ID
-- ========================================
PRINT 'Getting package ID...';

DECLARE @PackageID INT = (SELECT TOP 1 PackageID FROM MaintenancePackages WHERE IsActive = 1);

IF @PackageID IS NULL
BEGIN
    PRINT 'ERROR: No active package found. Please create a package first.';
    RETURN;
END

PRINT 'Package ID: ' + CAST(@PackageID AS VARCHAR(10));

-- ========================================
-- 5. CREATE TEST SUBSCRIPTIONS
-- ========================================
PRINT 'Creating test subscriptions...';

-- 5.1. Customer A - Subscription A (S?p h?t h?n, còn 1 l??t)
DECLARE @SubA_ID INT;

SET IDENTITY_INSERT CustomerPackageSubscriptions ON;

INSERT INTO CustomerPackageSubscriptions (
    SubscriptionID, SubscriptionCode, CustomerID, PackageID, VehicleID,
    PurchaseDate, StartDate, ExpirationDate, InitialVehicleMileage, PaymentAmount,
    Status, CreatedDate
)
VALUES (
    3001, 'SUB-TEST-A', 1001, @PackageID, 2001,
    DATEADD(DAY, -27, GETUTCDATE()), -- Mua 27 ngày tr??c
    DATEADD(DAY, -27, CAST(GETUTCDATE() AS DATE)), 
    DATEADD(DAY, 3, CAST(GETUTCDATE() AS DATE)), -- H?t h?n sau 3 ngày ?
    5000, 2500000,
    'Active', GETUTCDATE()
);

SET @SubA_ID = 3001;

SET IDENTITY_INSERT CustomerPackageSubscriptions OFF;

PRINT 'Created Subscription A (ID: 3001) - Expires in 3 days';

-- 5.2. Customer A - Subscription B (Còn lâu h?t h?n, còn 5 l??t)
DECLARE @SubB_ID INT;

SET IDENTITY_INSERT CustomerPackageSubscriptions ON;

INSERT INTO CustomerPackageSubscriptions (
    SubscriptionID, SubscriptionCode, CustomerID, PackageID, VehicleID,
    PurchaseDate, StartDate, ExpirationDate, InitialVehicleMileage, PaymentAmount,
    Status, CreatedDate
)
VALUES (
    3002, 'SUB-TEST-B', 1001, @PackageID, 2001,
    DATEADD(DAY, -7, GETUTCDATE()), -- Mua 7 ngày tr??c
    DATEADD(DAY, -7, CAST(GETUTCDATE() AS DATE)),
    DATEADD(DAY, 30, CAST(GETUTCDATE() AS DATE)), -- H?t h?n sau 30 ngày
    5000, 5000000,
    'Active', GETUTCDATE()
);

SET @SubB_ID = 3002;

SET IDENTITY_INSERT CustomerPackageSubscriptions OFF;

PRINT 'Created Subscription B (ID: 3002) - Expires in 30 days';

-- 5.3. Customer B - No subscription (?? test scenario không có subscription)
PRINT 'Customer B has NO subscription (for testing Extra services)';

-- ========================================
-- 6. CREATE PACKAGE SERVICE USAGES
-- ========================================
PRINT 'Creating package service usages...';

-- Subscription A: Còn 1 l??t "Thay d?u", h?t l??t "Ki?m tra phanh"
INSERT INTO PackageServiceUsages (SubscriptionID, ServiceID, TotalAllowedQuantity, UsedQuantity, RemainingQuantity)
VALUES 
(@SubA_ID, @ServiceOilChangeID, 2, 1, 1), -- Còn 1 l??t ??
(@SubA_ID, @ServiceBrakeCheckID, 2, 2, 0); -- H?t l??t

PRINT 'Sub A usages: Oil Change (1 remaining), Brake Check (0 remaining)';

-- Subscription B: Còn 5 l??t "Thay d?u", 2 l??t "Ki?m tra phanh"
INSERT INTO PackageServiceUsages (SubscriptionID, ServiceID, TotalAllowedQuantity, UsedQuantity, RemainingQuantity)
VALUES 
(@SubB_ID, @ServiceOilChangeID, 5, 0, 5), -- Còn 5 l??t
(@SubB_ID, @ServiceBrakeCheckID, 2, 0, 2); -- Còn 2 l??t

PRINT 'Sub B usages: Oil Change (5 remaining), Brake Check (2 remaining)';

-- ========================================
-- 7. CREATE TEST TIME SLOTS
-- ========================================
PRINT 'Creating test time slots...';

DECLARE @ServiceCenterID INT = (SELECT TOP 1 ServiceCenterID FROM ServiceCenters WHERE IsActive = 1);

IF @ServiceCenterID IS NULL
BEGIN
    PRINT 'ERROR: No active service center found.';
    RETURN;
END

SET IDENTITY_INSERT TimeSlots ON;

DECLARE @TestDate DATE = DATEADD(DAY, 7, CAST(GETUTCDATE() AS DATE)); -- 7 ngày sau

INSERT INTO TimeSlots (SlotID, ServiceCenterID, SlotDate, StartTime, EndTime, MaxBookings, IsAvailable, CreatedDate)
VALUES 
(4001, @ServiceCenterID, @TestDate, '08:00:00', '10:00:00', 5, 1, GETUTCDATE()),
(4002, @ServiceCenterID, @TestDate, '10:00:00', '12:00:00', 5, 1, GETUTCDATE()),
(4003, @ServiceCenterID, @TestDate, '14:00:00', '16:00:00', 5, 1, GETUTCDATE());

SET IDENTITY_INSERT TimeSlots OFF;

PRINT 'Created 3 test time slots (IDs: 4001-4003) for ' + CAST(@TestDate AS VARCHAR(20));

-- ========================================
-- 8. SUMMARY
-- ========================================
PRINT '';
PRINT '========================================';
PRINT 'TEST DATA SETUP COMPLETE!';
PRINT '========================================';
PRINT '';
PRINT 'Test Accounts:';
PRINT '  - Customer A (ID=1001, Phone=0901234001): Has 2 subscriptions';
PRINT '    * Sub A (3001): Expires in 3 days, 1 Oil Change left';
PRINT '    * Sub B (3002): Expires in 30 days, 5 Oil Changes left';
PRINT '  - Customer B (ID=1002, Phone=0901234002): NO subscription';
PRINT '';
PRINT 'Test Vehicles:';
PRINT '  - Vehicle A (2001): 30TEST-001 (Customer A)';
PRINT '  - Vehicle B (2002): 30TEST-002 (Customer B)';
PRINT '';
PRINT 'Test Time Slots:';
PRINT '  - Slot 4001: 08:00-10:00 on ' + CAST(@TestDate AS VARCHAR(20));
PRINT '  - Slot 4002: 10:00-12:00 on ' + CAST(@TestDate AS VARCHAR(20));
PRINT '  - Slot 4003: 14:00-16:00 on ' + CAST(@TestDate AS VARCHAR(20));
PRINT '';
PRINT 'Service IDs:';
PRINT '  - Oil Change: ' + CAST(@ServiceOilChangeID AS VARCHAR(10));
PRINT '  - Brake Check: ' + CAST(@ServiceBrakeCheckID AS VARCHAR(10));
IF @ServiceCarWashID IS NOT NULL
    PRINT '  - Car Wash: ' + CAST(@ServiceCarWashID AS VARCHAR(10));
PRINT '';
PRINT 'Next Steps:';
PRINT '1. Use Postman/Swagger to test API endpoints';
PRINT '2. Refer to SMART_SUBSCRIPTION_API_EXAMPLES.md for test scenarios';
PRINT '3. Check logs for Smart Logic execution';
PRINT '';

-- ========================================
-- 9. VERIFICATION QUERIES
-- ========================================
PRINT 'Verification Queries (Run these to check data):';
PRINT '';
PRINT '-- Check customers';
PRINT 'SELECT * FROM Customers WHERE CustomerID IN (1001, 1002);';
PRINT '';
PRINT '-- Check vehicles';
PRINT 'SELECT * FROM CustomerVehicles WHERE CustomerID IN (1001, 1002);';
PRINT '';
PRINT '-- Check subscriptions';
PRINT 'SELECT * FROM CustomerPackageSubscriptions WHERE CustomerID = 1001;';
PRINT '';
PRINT '-- Check service usages';
PRINT 'SELECT s.SubscriptionCode, m.ServiceName, u.TotalAllowedQuantity, u.UsedQuantity, u.RemainingQuantity';
PRINT 'FROM PackageServiceUsages u';
PRINT 'JOIN CustomerPackageSubscriptions s ON u.SubscriptionID = s.SubscriptionID';
PRINT 'JOIN MaintenanceServices m ON u.ServiceID = m.ServiceID';
PRINT 'WHERE s.CustomerID = 1001';
PRINT 'ORDER BY s.SubscriptionID, m.ServiceName;';
PRINT '';
PRINT '-- Check time slots';
PRINT 'SELECT * FROM TimeSlots WHERE SlotID IN (4001, 4002, 4003);';

GO
