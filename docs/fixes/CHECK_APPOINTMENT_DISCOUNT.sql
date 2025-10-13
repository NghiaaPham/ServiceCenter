-- Check appointment discount fields
SELECT 
    a.AppointmentID,
    a.AppointmentCode,
    a.CustomerID,
    c.FullName AS CustomerName,
    ct.TypeName AS CustomerType,
    ct.DiscountPercent AS CustomerTypeDiscount,
    a.EstimatedCost,
    a.DiscountAmount,
    a.DiscountType,
    a.PromotionID,
    CASE 
        WHEN a.DiscountAmount IS NULL OR a.DiscountAmount = 0 THEN '? NO DISCOUNT'
        ELSE '? HAS DISCOUNT'
    END AS DiscountStatus
FROM Appointments a
INNER JOIN Customers c ON a.CustomerID = c.CustomerID
LEFT JOIN CustomerTypes ct ON c.TypeID = ct.TypeID
WHERE a.AppointmentID = 1;

-- Check customer type
SELECT 
    c.CustomerID,
    c.FullName,
    c.TypeID,
    ct.TypeName,
    ct.DiscountPercent,
    ct.IsActive,
    CASE 
        WHEN ct.IsActive = 1 THEN '? ACTIVE'
        ELSE '? INACTIVE'
    END AS TypeStatus
FROM Customers c
LEFT JOIN CustomerTypes ct ON c.TypeID = ct.TypeID
WHERE c.CustomerID = 1014;

-- Check appointment services
SELECT 
    aps.AppointmentServiceID,
    aps.ServiceID,
    ms.ServiceName,
    aps.ServiceSource,
    aps.Price,
    CASE 
        WHEN aps.ServiceSource = 'Subscription' THEN '?? FREE (Subscription)'
        WHEN aps.ServiceSource = 'Regular' THEN '?? REGULAR'
        WHEN aps.ServiceSource = 'Extra' THEN '?? EXTRA (Paid)'
        ELSE aps.ServiceSource
    END AS SourceType
FROM AppointmentServices aps
INNER JOIN MaintenanceServices ms ON aps.ServiceID = ms.ServiceID
WHERE aps.AppointmentID = 1;
