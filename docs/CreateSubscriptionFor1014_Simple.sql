-- Simple script to create subscription for customer 1014
USE EVServiceCenterV2;
GO

-- Insert subscription directly
INSERT INTO CustomerPackageSubscriptions (
    SubscriptionCode, CustomerID, PackageID, VehicleID,
    StartDate, ExpirationDate, Status, AutoRenew,
    OriginalPrice, DiscountPercent, DiscountAmount, PaymentAmount,
    PurchaseDate, InitialVehicleMileage,
    RemainingServices, UsedServices,
    CreatedDate, Notes
) VALUES (
    'SUB-TEST-1014',
    1014, -- Customer 1014
    3,    -- VIP Package
    7,    -- Vehicle 7
    CAST(GETDATE() AS DATE),
    DATEADD(DAY, 730, CAST(GETDATE() AS DATE)),
    'Active',
    1,
    8000000, 30, 2400000, 5600000,
    GETUTCDATE(), 5000,
    131, 0,
    GETUTCDATE(),
    'Test VIP subscription for customer 1014'
);

-- Get the subscription ID
DECLARE @SubId INT = SCOPE_IDENTITY();

-- Create usage tracking for each service in VIP package
INSERT INTO PackageServiceUsages (SubscriptionID, ServiceID, TotalAllowedQuantity, UsedQuantity, RemainingQuantity, Notes)
SELECT @SubId, ps.ServiceID, ps.Quantity, 0, ps.Quantity, 'Ready to use'
FROM PackageServices ps
WHERE ps.PackageID = 3 AND ps.IncludedInPackage = 1;

-- Show result
SELECT
    s.SubscriptionID,
    s.SubscriptionCode,
    s.Status,
    c.CustomerID,
    c.FullName,
    v.VehicleID,
    v.LicensePlate,
    p.PackageName,
    s.RemainingServices
FROM CustomerPackageSubscriptions s
JOIN Customers c ON s.CustomerID = c.CustomerID
LEFT JOIN CustomerVehicles v ON s.VehicleID = v.VehicleID
JOIN MaintenancePackages p ON s.PackageID = p.PackageID
WHERE s.SubscriptionCode = 'SUB-TEST-1014';

PRINT 'Subscription created! Use the SubscriptionID above for API testing.';
GO
