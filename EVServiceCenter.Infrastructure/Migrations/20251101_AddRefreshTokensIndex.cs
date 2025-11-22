using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EVServiceCenter.Infrastructure.Migrations
{
    public partial class AddRefreshTokensIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create a filtered nonclustered index to speed up revocation queries
            migrationBuilder.Sql(@"
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes i
    JOIN sys.objects o ON i.object_id = o.object_id
    WHERE o.name = 'RefreshTokens' AND i.name = 'IX_RefreshTokens_UserId_CreatedByIp_NotRevoked'
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_RefreshTokens_UserId_CreatedByIp_NotRevoked
    ON dbo.RefreshTokens (UserId, CreatedByIp)
    INCLUDE (Selector, Expires, ReplacedByTokenHash)
    WHERE Revoked IS NULL;
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1 FROM sys.indexes i
    JOIN sys.objects o ON i.object_id = o.object_id
    WHERE o.name = 'RefreshTokens' AND i.name = 'IX_RefreshTokens_UserId_CreatedByIp_NotRevoked'
)
BEGIN
    DROP INDEX IX_RefreshTokens_UserId_CreatedByIp_NotRevoked ON dbo.RefreshTokens;
END
");
        }
    }
}
