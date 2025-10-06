-- Script để thêm 20 khách hàng mới vào database
USE EVServiceCenterV2;
GO

-- Lấy CustomerID cao nhất để tính toán CustomerCode
DECLARE @MaxID INT;
DECLARE @StartCode INT;

SELECT @MaxID = ISNULL(MAX(CustomerID), 0) FROM Customers;
SET @StartCode = @MaxID + 1;

PRINT 'Bắt đầu thêm từ CustomerID: ' + CAST(@StartCode AS VARCHAR(10));

-- Insert từng khách hàng một
INSERT INTO Customers (CustomerCode, FullName, PhoneNumber, Email, Address, TypeID, IsActive, CreatedDate, LoyaltyPoints, TotalSpent)
VALUES ('KH' + RIGHT('000000' + CAST(@StartCode AS VARCHAR), 6), N'Ngô Văn L', '0801234568', 'ngovanl@gmail.com', N'12 Lê Văn Sỹ, Quận 3, TP.HCM', 1, 1, DATEADD(MONTH, -7, GETUTCDATE()), 0, 0);

INSERT INTO Customers (CustomerCode, FullName, PhoneNumber, Email, Address, TypeID, IsActive, CreatedDate, LoyaltyPoints, TotalSpent)
VALUES ('KH' + RIGHT('000000' + CAST(@StartCode + 1 AS VARCHAR), 6), N'Trương Thị M', '0811234568', 'truongthim@gmail.com', N'34 Nguyễn Đình Chiểu, Quận 1, TP.HCM', 2, 1, DATEADD(MONTH, -9, GETUTCDATE()), 0, 0);

INSERT INTO Customers (CustomerCode, FullName, PhoneNumber, Email, Address, TypeID, IsActive, CreatedDate, LoyaltyPoints, TotalSpent)
VALUES ('KH' + RIGHT('000000' + CAST(@StartCode + 2 AS VARCHAR), 6), N'Phan Văn N', '0821234568', 'phanvann@gmail.com', N'56 Pasteur, Quận 1, TP.HCM', 1, 1, DATEADD(MONTH, -11, GETUTCDATE()), 0, 0);

INSERT INTO Customers (CustomerCode, FullName, PhoneNumber, Email, Address, TypeID, IsActive, CreatedDate, LoyaltyPoints, TotalSpent)
VALUES ('KH' + RIGHT('000000' + CAST(@StartCode + 3 AS VARCHAR), 6), N'Lý Thị O', '0831234568', 'lythio@gmail.com', N'78 Nam Kỳ Khởi Nghĩa, Quận 3, TP.HCM', 1, 1, DATEADD(MONTH, -13, GETUTCDATE()), 0, 0);

INSERT INTO Customers (CustomerCode, FullName, PhoneNumber, Email, Address, TypeID, IsActive, CreatedDate, LoyaltyPoints, TotalSpent)
VALUES ('KH' + RIGHT('000000' + CAST(@StartCode + 4 AS VARCHAR), 6), N'Mai Văn P', '0841234568', 'maivanp@gmail.com', N'90 Trần Quang Khải, Quận 1, TP.HCM', 2, 1, DATEADD(MONTH, -14, GETUTCDATE()), 0, 0);

INSERT INTO Customers (CustomerCode, FullName, PhoneNumber, Email, Address, TypeID, IsActive, CreatedDate, LoyaltyPoints, TotalSpent)
VALUES ('KH' + RIGHT('000000' + CAST(@StartCode + 5 AS VARCHAR), 6), N'Võ Thị Q', '0851234568', 'vothiq@gmail.com', N'102 Bùi Viện, Quận 1, TP.HCM', 1, 1, DATEADD(MONTH, -16, GETUTCDATE()), 0, 0);

INSERT INTO Customers (CustomerCode, FullName, PhoneNumber, Email, Address, TypeID, IsActive, CreatedDate, LoyaltyPoints, TotalSpent)
VALUES ('KH' + RIGHT('000000' + CAST(@StartCode + 6 AS VARCHAR), 6), N'Huỳnh Văn R', '0861234568', 'huynhvanr@gmail.com', N'114 Đề Thám, Quận 1, TP.HCM', 1, 1, DATEADD(MONTH, -17, GETUTCDATE()), 0, 0);

INSERT INTO Customers (CustomerCode, FullName, PhoneNumber, Email, Address, TypeID, IsActive, CreatedDate, LoyaltyPoints, TotalSpent)
VALUES ('KH' + RIGHT('000000' + CAST(@StartCode + 7 AS VARCHAR), 6), N'Tô Thị S', '0871234568', 'tothis@gmail.com', N'126 Cống Quỳnh, Quận 1, TP.HCM', 2, 1, DATEADD(MONTH, -19, GETUTCDATE()), 0, 0);

INSERT INTO Customers (CustomerCode, FullName, PhoneNumber, Email, Address, TypeID, IsActive, CreatedDate, LoyaltyPoints, TotalSpent)
VALUES ('KH' + RIGHT('000000' + CAST(@StartCode + 8 AS VARCHAR), 6), N'Đỗ Văn T', '0881234568', 'dovant@gmail.com', N'138 Lý Tự Trọng, Quận 1, TP.HCM', 1, 1, DATEADD(MONTH, -20, GETUTCDATE()), 0, 0);

INSERT INTO Customers (CustomerCode, FullName, PhoneNumber, Email, Address, TypeID, IsActive, CreatedDate, LoyaltyPoints, TotalSpent)
VALUES ('KH' + RIGHT('000000' + CAST(@StartCode + 9 AS VARCHAR), 6), N'Cao Thị U', '0891234568', 'caothiu@gmail.com', N'150 Đồng Khởi, Quận 1, TP.HCM', 2, 1, DATEADD(MONTH, -21, GETUTCDATE()), 0, 0);

INSERT INTO Customers (CustomerCode, FullName, PhoneNumber, Email, Address, TypeID, IsActive, CreatedDate, LoyaltyPoints, TotalSpent)
VALUES ('KH' + RIGHT('000000' + CAST(@StartCode + 10 AS VARCHAR), 6), N'Hồ Văn V', '0701234568', 'hovanv@gmail.com', N'162 Nguyễn Thái Bình, Quận 1, TP.HCM', 1, 1, DATEADD(MONTH, -22, GETUTCDATE()), 0, 0);

INSERT INTO Customers (CustomerCode, FullName, PhoneNumber, Email, Address, TypeID, IsActive, CreatedDate, LoyaltyPoints, TotalSpent)
VALUES ('KH' + RIGHT('000000' + CAST(@StartCode + 11 AS VARCHAR), 6), N'Lưu Thị X', '0711234568', 'luuthix@gmail.com', N'174 Tôn Đức Thắng, Quận 1, TP.HCM', 1, 0, DATEADD(MONTH, -23, GETUTCDATE()), 0, 0);

INSERT INTO Customers (CustomerCode, FullName, PhoneNumber, Email, Address, TypeID, IsActive, CreatedDate, LoyaltyPoints, TotalSpent)
VALUES ('KH' + RIGHT('000000' + CAST(@StartCode + 12 AS VARCHAR), 6), N'Tạ Văn Y', '0721234568', 'tavany@gmail.com', N'186 Võ Thị Sáu, Quận 3, TP.HCM', 2, 1, DATEADD(MONTH, -24, GETUTCDATE()), 0, 0);

INSERT INTO Customers (CustomerCode, FullName, PhoneNumber, Email, Address, TypeID, IsActive, CreatedDate, LoyaltyPoints, TotalSpent)
VALUES ('KH' + RIGHT('000000' + CAST(@StartCode + 13 AS VARCHAR), 6), N'Châu Thị Z', '0731234568', 'chauthiz@gmail.com', N'198 Nguyễn Bỉnh Khiêm, Quận 1, TP.HCM', 1, 1, DATEADD(MONTH, -25, GETUTCDATE()), 0, 0);

INSERT INTO Customers (CustomerCode, FullName, PhoneNumber, Email, Address, TypeID, IsActive, CreatedDate, LoyaltyPoints, TotalSpent)
VALUES ('KH' + RIGHT('000000' + CAST(@StartCode + 14 AS VARCHAR), 6), N'Quách Văn AA', '0741234568', 'quachvanaa@gmail.com', N'200 Lê Thánh Tôn, Quận 1, TP.HCM', 2, 1, DATEADD(MONTH, -1, GETUTCDATE()), 0, 0);

INSERT INTO Customers (CustomerCode, FullName, PhoneNumber, Email, Address, TypeID, IsActive, CreatedDate, LoyaltyPoints, TotalSpent)
VALUES ('KH' + RIGHT('000000' + CAST(@StartCode + 15 AS VARCHAR), 6), N'Ông Thị BB', '0751234568', 'ongthibb@gmail.com', N'212 Mạc Đĩnh Chi, Quận 1, TP.HCM', 1, 1, DATEADD(DAY, -15, GETUTCDATE()), 0, 0);

INSERT INTO Customers (CustomerCode, FullName, PhoneNumber, Email, Address, TypeID, IsActive, CreatedDate, LoyaltyPoints, TotalSpent)
VALUES ('KH' + RIGHT('000000' + CAST(@StartCode + 16 AS VARCHAR), 6), N'Khổng Văn CC', '0761234568', 'khongvancc@gmail.com', N'224 Nguyễn Thị Minh Khai, Quận 3, TP.HCM', 1, 0, DATEADD(DAY, -10, GETUTCDATE()), 0, 0);

INSERT INTO Customers (CustomerCode, FullName, PhoneNumber, Email, Address, TypeID, IsActive, CreatedDate, LoyaltyPoints, TotalSpent)
VALUES ('KH' + RIGHT('000000' + CAST(@StartCode + 17 AS VARCHAR), 6), N'Kiều Thị DD', '0771234568', 'kieuthidd@gmail.com', N'236 Cách Mạng Tháng 8, Quận 10, TP.HCM', 2, 1, DATEADD(DAY, -5, GETUTCDATE()), 0, 0);

INSERT INTO Customers (CustomerCode, FullName, PhoneNumber, Email, Address, TypeID, IsActive, CreatedDate, LoyaltyPoints, TotalSpent)
VALUES ('KH' + RIGHT('000000' + CAST(@StartCode + 18 AS VARCHAR), 6), N'Ưng Văn EE', '0781234568', 'ungvanee@gmail.com', N'248 Đinh Tiên Hoàng, Quận 1, TP.HCM', 1, 1, DATEADD(DAY, -3, GETUTCDATE()), 0, 0);

INSERT INTO Customers (CustomerCode, FullName, PhoneNumber, Email, Address, TypeID, IsActive, CreatedDate, LoyaltyPoints, TotalSpent)
VALUES ('KH' + RIGHT('000000' + CAST(@StartCode + 19 AS VARCHAR), 6), N'Viên Thị FF', '0791234568', 'vienthiff@gmail.com', N'260 Nguyễn Công Trứ, Quận 1, TP.HCM', 2, 1, DATEADD(DAY, -1, GETUTCDATE()), 0, 0);

PRINT 'Đã thêm 20 khách hàng mới thành công!';

-- Kiểm tra kết quả
SELECT COUNT(*) AS TotalCustomers FROM Customers;
SELECT TOP 10 CustomerCode, FullName, PhoneNumber FROM Customers ORDER BY CustomerID DESC;
