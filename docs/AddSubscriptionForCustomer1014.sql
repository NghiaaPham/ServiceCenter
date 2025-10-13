-- =============================================
-- ADD SUBSCRIPTION FOR CUSTOMER 1014 (NghiaPham)
-- =============================================

USE EVServiceCenterV2;
GO

-- Get customer info
DECLARE @CustomerId INT = 1014;
DECLARE @CustomerName NVARCHAR(100);

SELECT @CustomerName = FullName FROM Customers WHERE CustomerID = @CustomerId;

IF @CustomerName IS NULL
BEGIN
    PRINT 'ERROR: Customer 1014 not found!';
    RETURN;
END

PRINT 'Creating subscription for: ' + @CustomerName;

-- Get VIP Package ID
DECLARE @VIPPackageId INT = (SELECT TOP 1 PackageID FROM MaintenancePackages WHERE PackageCode = 'PKG-VIP-2025');

IF @VIPPackageId IS NULL
BEGIN
    PRINT 'ERROR: VIP Package not found!';
    RETURN;
END

-- Get Vehicle ID for customer 1014
DECLARE @VehicleId INT = (SELECT TOP 1 VehicleID FROM CustomerVehicles WHERE CustomerID = @CustomerId);

PRINT 'VehicleID: ' + CAST(ISNULL(@VehicleId, 0) AS VARCHAR);

-- Delete existing subscription for customer 1014 if any
DELETE FROM PackageServiceUsages
WHERE SubscriptionID IN (SELECT SubscriptionID FROM CustomerPackageSubscriptions WHERE CustomerID = @CustomerId AND SubscriptionCode LIKE 'SUB-TEST-%');

DELETE FROM CustomerPackageSubscriptions
WHERE CustomerID = @CustomerId AND SubscriptionCode LIKE 'SUB-TEST-%';

-- Create new VIP subscription for customer 1014
INSERT INTO CustomerPackageSubscriptions (
    SubscriptionCode, CustomerID, PackageID, VehicleID,
    StartDate, ExpirationDate, Status, AutoRenew,
    OriginalPrice, DiscountPercent, DiscountAmount, PaymentAmount,
    PurchaseDate, InitialVehicleMileage,
    RemainingServices, UsedServices,
    CreatedDate, Notes
) VALUES (
    'SUB-TEST-1014', @CustomerId, @VIPPackageId, @VehicleId,
    DATEADD(DAY, -10, GETDATE()), DATEADD(DAY, 720, GETDATE()), 'Active', 1,
    8000000, 30, 2400000, 5600000,
    DATEADD(DAY, -10, GETUTCDATE()), 5000,
    132, 0,  -- All services available, none used
    DATEADD(DAY, -10, GETUTCDATE()), 'Test VIP subscription for customer 1014 - FOR API TESTING'
);

DECLARE @NewSubId INT = SCOPE_IDENTITY();

PRINT 'Created Subscription ID: ' + CAST(@NewSubId AS VARCHAR);

-- Create PackageServiceUsage records
INSERT INTO PackageServiceUsages (SubscriptionID, ServiceID, TotalAllowedQuantity, UsedQuantity, RemainingQuantity, Notes)
SELECT
    @NewSubId,
    ps.ServiceID,
    ps.Quantity,
    0,  -- Not used yet
    ps.Quantity,
    'Auto-generated for customer 1014 test subscription'
FROM PackageServices ps
WHERE ps.PackageID = @VIPPackageId AND ps.IncludedInPackage = 1;

PRINT 'Created PackageServiceUsage records';

-- Verification
PRINT '';
PRINT '========== CREATED SUBSCRIPTION ==========';

SELECT
    s.SubscriptionID,
    s.SubscriptionCode,
    s.Status,
    c.CustomerID,
    c.FullName AS CustomerName,
    c.CustomerCode,
    v.VehicleID,
    v.LicensePlate,
    p.PackageName,
    s.StartDate,
    s.ExpirationDate,
    s.PaymentAmount,
    s.RemainingServices
FROM CustomerPackageSubscriptions s
JOIN Customers c ON s.CustomerID = c.CustomerID
LEFT JOIN CustomerVehicles v ON s.VehicleID = v.VehicleID
JOIN MaintenancePackages p ON s.PackageID = p.PackageID
WHERE s.SubscriptionCode = 'SUB-TEST-1014';

PRINT '';
PRINT '========== TEST API REQUEST ==========';
PRINT 'Use this data for your API request:';
PRINT '';

SELECT
    'customerId: ' + CAST(c.CustomerID AS VARCHAR) AS Field1,
    'vehicleId: ' + CAST(ISNULL(v.VehicleID, 0) AS VARCHAR) AS Field2,
    'subscriptionId: ' + CAST(s.SubscriptionID AS VARCHAR) AS Field3,
    'serviceCenterId: 2' AS Field4,
    'slotId: 201' AS Field5
FROM CustomerPackageSubscriptions s
JOIN Customers c ON s.CustomerID = c.CustomerID
LEFT JOIN CustomerVehicles v ON s.VehicleID = v.VehicleID
WHERE s.SubscriptionCode = 'SUB-TEST-1014';

PRINT '';
PRINT 'Subscription created successfully!';
GO
