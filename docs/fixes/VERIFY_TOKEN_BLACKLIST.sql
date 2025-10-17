-- ============================================
-- JWT TOKEN BLACKLIST - DATABASE VERIFICATION
-- ============================================
-- 
-- Script này dùng ?? verify và maintain Token Blacklist
--
-- ============================================

USE EVServiceCenterV2;
GO

-- ============================================
-- 1. VERIFY TABLE STRUCTURE
-- ============================================

-- Check RevokedTokens table exists
IF OBJECT_ID('RevokedTokens', 'U') IS NOT NULL
    PRINT '? RevokedTokens table exists'
ELSE
    PRINT '? RevokedTokens table NOT FOUND!'
GO

-- Check table structure
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'RevokedTokens'
ORDER BY ORDINAL_POSITION;
GO

-- Check indexes
SELECT 
    i.name AS IndexName,
    i.type_desc AS IndexType,
    COL_NAME(ic.object_id, ic.column_id) AS ColumnName
FROM sys.indexes i
INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
WHERE i.object_id = OBJECT_ID('RevokedTokens')
ORDER BY i.name, ic.key_ordinal;
GO

-- ============================================
-- 2. VIEW CURRENT BLACKLIST
-- ============================================

-- View all revoked tokens
SELECT 
    Id,
    LEFT(Token, 50) + '...' AS TokenPreview,
    UserId,
    RevokeReason,
    RevokedAt,
    ExpiresAt,
    IpAddress,
    LEFT(UserAgent, 50) AS UserAgentPreview,
    CASE 
        WHEN ExpiresAt > GETUTCDATE() THEN 'Active'
        ELSE 'Expired'
    END AS Status
FROM RevokedTokens
ORDER BY RevokedAt DESC;
GO

-- Count tokens by status
SELECT 
    CASE 
        WHEN ExpiresAt > GETUTCDATE() THEN 'Active'
        ELSE 'Expired'
    END AS Status,
    COUNT(*) AS TokenCount
FROM RevokedTokens
GROUP BY 
    CASE 
        WHEN ExpiresAt > GETUTCDATE() THEN 'Active'
        ELSE 'Expired'
    END;
GO

-- ============================================
-- 3. TOKENS BY USER
-- ============================================

SELECT 
    u.UserId,
    u.Username,
    u.FullName,
    COUNT(rt.Id) AS RevokedTokensCount,
    MAX(rt.RevokedAt) AS LastLogoutTime
FROM Users u
LEFT JOIN RevokedTokens rt ON u.UserId = rt.UserId
GROUP BY u.UserId, u.Username, u.FullName
HAVING COUNT(rt.Id) > 0
ORDER BY RevokedTokensCount DESC;
GO

-- ============================================
-- 4. TOKENS BY REASON
-- ============================================

SELECT 
    RevokeReason,
    COUNT(*) AS TokenCount,
    MIN(RevokedAt) AS FirstRevoked,
    MAX(RevokedAt) AS LastRevoked
FROM RevokedTokens
GROUP BY RevokeReason
ORDER BY TokenCount DESC;
GO

-- ============================================
-- 5. RECENT LOGOUT ACTIVITY (Last 24 hours)
-- ============================================

SELECT 
    rt.Id,
    u.Username,
    u.FullName,
    rt.RevokeReason,
    rt.RevokedAt,
    rt.IpAddress,
    LEFT(rt.UserAgent, 100) AS UserAgent
FROM RevokedTokens rt
INNER JOIN Users u ON rt.UserId = u.UserId
WHERE rt.RevokedAt >= DATEADD(HOUR, -24, GETUTCDATE())
ORDER BY rt.RevokedAt DESC;
GO

-- ============================================
-- 6. FIND SPECIFIC TOKEN (for debugging)
-- ============================================

-- Replace with actual token to search
DECLARE @SearchToken NVARCHAR(1000) = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...';

SELECT 
    rt.*,
    u.Username,
    u.FullName
FROM RevokedTokens rt
INNER JOIN Users u ON rt.UserId = u.UserId
WHERE rt.Token LIKE '%' + @SearchToken + '%';
GO

-- ============================================
-- 7. CLEANUP EXPIRED TOKENS (Manual)
-- ============================================

-- Check how many expired tokens exist
SELECT COUNT(*) AS ExpiredTokenCount
FROM RevokedTokens
WHERE ExpiresAt < GETUTCDATE();
GO

-- DELETE expired tokens (run manually for cleanup)
-- CAUTION: This permanently deletes data
/*
BEGIN TRANSACTION;

DELETE FROM RevokedTokens
WHERE ExpiresAt < GETUTCDATE();

PRINT '??? Deleted ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + ' expired tokens';

COMMIT TRANSACTION;
GO
*/

-- ============================================
-- 8. PERFORMANCE ANALYSIS
-- ============================================

-- Check table size
SELECT 
    t.name AS TableName,
    p.rows AS RowCount,
    SUM(a.total_pages) * 8 AS TotalSpaceKB,
    SUM(a.used_pages) * 8 AS UsedSpaceKB
FROM sys.tables t
INNER JOIN sys.indexes i ON t.object_id = i.object_id
INNER JOIN sys.partitions p ON i.object_id = p.object_id AND i.index_id = p.index_id
INNER JOIN sys.allocation_units a ON p.partition_id = a.container_id
WHERE t.name = 'RevokedTokens'
GROUP BY t.name, p.rows;
GO

-- Check index usage
SELECT 
    i.name AS IndexName,
    s.user_seeks AS UserSeeks,
    s.user_scans AS UserScans,
    s.user_lookups AS UserLookups,
    s.user_updates AS UserUpdates,
    s.last_user_seek AS LastUserSeek,
    s.last_user_scan AS LastUserScan
FROM sys.indexes i
LEFT JOIN sys.dm_db_index_usage_stats s 
    ON i.object_id = s.object_id 
    AND i.index_id = s.index_id
WHERE i.object_id = OBJECT_ID('RevokedTokens');
GO

-- ============================================
-- 9. AUTOMATED CLEANUP JOB (T-SQL)
-- ============================================

-- Create stored procedure for cleanup
IF OBJECT_ID('sp_CleanupExpiredTokens', 'P') IS NOT NULL
    DROP PROCEDURE sp_CleanupExpiredTokens;
GO

CREATE PROCEDURE sp_CleanupExpiredTokens
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @DeletedCount INT;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Delete expired tokens
        DELETE FROM RevokedTokens
        WHERE ExpiresAt < GETUTCDATE();
        
        SET @DeletedCount = @@ROWCOUNT;
        
        COMMIT TRANSACTION;
        
        -- Log result
        PRINT '? Cleanup completed: ' + CAST(@DeletedCount AS NVARCHAR(10)) + ' expired tokens deleted';
        
        RETURN @DeletedCount;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
            
        PRINT '? Cleanup failed: ' + ERROR_MESSAGE();
        THROW;
    END CATCH
END;
GO

-- Test cleanup procedure
EXEC sp_CleanupExpiredTokens;
GO

-- ============================================
-- 10. SECURITY AUDIT QUERIES
-- ============================================

-- Find users with multiple logout events in short time (suspicious)
SELECT 
    u.UserId,
    u.Username,
    COUNT(*) AS LogoutCount,
    MIN(rt.RevokedAt) AS FirstLogout,
    MAX(rt.RevokedAt) AS LastLogout,
    DATEDIFF(MINUTE, MIN(rt.RevokedAt), MAX(rt.RevokedAt)) AS TimeSpanMinutes
FROM RevokedTokens rt
INNER JOIN Users u ON rt.UserId = u.UserId
WHERE rt.RevokedAt >= DATEADD(HOUR, -1, GETUTCDATE())
GROUP BY u.UserId, u.Username
HAVING COUNT(*) >= 5
ORDER BY LogoutCount DESC;
GO

-- Find logout from unusual IPs
SELECT 
    u.Username,
    rt.IpAddress,
    COUNT(*) AS LogoutCount,
    MAX(rt.RevokedAt) AS LastLogout
FROM RevokedTokens rt
INNER JOIN Users u ON rt.UserId = u.UserId
WHERE rt.IpAddress IS NOT NULL
GROUP BY u.Username, rt.IpAddress
ORDER BY LogoutCount DESC;
GO

-- ============================================
-- 11. CORRELATION WITH USER SESSIONS
-- ============================================

-- Compare RevokedTokens with UserSessions
SELECT 
    us.SessionId,
    us.UserId,
    u.Username,
    us.LoginTime,
    us.LogoutTime,
    us.IsActive,
    COUNT(rt.Id) AS RevokedTokensCount
FROM UserSessions us
INNER JOIN Users u ON us.UserId = u.UserId
LEFT JOIN RevokedTokens rt ON us.UserId = rt.UserId 
    AND rt.RevokedAt >= us.LoginTime
    AND (us.LogoutTime IS NULL OR rt.RevokedAt <= us.LogoutTime)
WHERE us.LoginTime >= DATEADD(DAY, -7, GETUTCDATE())
GROUP BY us.SessionId, us.UserId, u.Username, us.LoginTime, us.LogoutTime, us.IsActive
ORDER BY us.LoginTime DESC;
GO

-- ============================================
-- 12. RECOMMENDATIONS
-- ============================================

PRINT '============================================';
PRINT '?? TOKEN BLACKLIST HEALTH CHECK';
PRINT '============================================';

-- Check 1: Table exists
IF OBJECT_ID('RevokedTokens', 'U') IS NOT NULL
    PRINT '? Table: RevokedTokens exists';
ELSE
    PRINT '? Table: RevokedTokens NOT FOUND!';

-- Check 2: Indexes exist
IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('RevokedTokens') AND name = 'UX_RevokedTokens_Token')
    PRINT '? Index: UX_RevokedTokens_Token exists';
ELSE
    PRINT '?? Index: UX_RevokedTokens_Token missing!';

IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('RevokedTokens') AND name = 'IX_RevokedTokens_ExpiresAt')
    PRINT '? Index: IX_RevokedTokens_ExpiresAt exists';
ELSE
    PRINT '?? Index: IX_RevokedTokens_ExpiresAt missing!';

-- Check 3: Cleanup procedure exists
IF OBJECT_ID('sp_CleanupExpiredTokens', 'P') IS NOT NULL
    PRINT '? Procedure: sp_CleanupExpiredTokens exists';
ELSE
    PRINT '?? Procedure: sp_CleanupExpiredTokens missing!';

-- Check 4: Table size
DECLARE @RowCount INT;
SELECT @RowCount = COUNT(*) FROM RevokedTokens;

IF @RowCount < 10000
    PRINT '? Table size: ' + CAST(@RowCount AS NVARCHAR(10)) + ' rows (healthy)';
ELSE IF @RowCount < 100000
    PRINT '?? Table size: ' + CAST(@RowCount AS NVARCHAR(10)) + ' rows (consider cleanup)';
ELSE
    PRINT '? Table size: ' + CAST(@RowCount AS NVARCHAR(10)) + ' rows (cleanup URGENT!)';

PRINT '============================================';
PRINT '?? RECOMMENDATIONS:';
PRINT '1. Run sp_CleanupExpiredTokens daily';
PRINT '2. Monitor table size weekly';
PRINT '3. Archive old tokens before deletion';
PRINT '4. Review logout patterns for security';
PRINT '============================================';
GO
