using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EVServiceCenter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserLoginIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Users_Email_Login' AND object_id = OBJECT_ID('[dbo].[Users]'))
    DROP INDEX [IX_Users_Email_Login] ON [Users];");

            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Users_Username_Login' AND object_id = OBJECT_ID('[dbo].[Users]'))
    DROP INDEX [IX_Users_Username_Login] ON [Users];");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email_Login",
                table: "Users",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username_Login",
                table: "Users",
                column: "Username");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Users_Email_Login' AND object_id = OBJECT_ID('[dbo].[Users]'))
    DROP INDEX [IX_Users_Email_Login] ON [Users];");

            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Users_Username_Login' AND object_id = OBJECT_ID('[dbo].[Users]'))
    DROP INDEX [IX_Users_Username_Login] ON [Users];");
        }
    }
}
