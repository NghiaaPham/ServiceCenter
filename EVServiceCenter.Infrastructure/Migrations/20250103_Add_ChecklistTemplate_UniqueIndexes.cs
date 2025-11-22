using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EVServiceCenter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_ChecklistTemplate_UniqueIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ? Ensure only 1 active template per service
            migrationBuilder.CreateIndex(
                name: "UX_ChecklistTemplate_ServiceId",
                table: "ChecklistTemplates",
                column: "ServiceID",
                unique: true,
                filter: "[ServiceID] IS NOT NULL AND [IsActive] = 1");

            // ? Ensure only 1 active template per category (without service)
            migrationBuilder.CreateIndex(
                name: "UX_ChecklistTemplate_CategoryId",
                table: "ChecklistTemplates",
                column: "CategoryID",
                unique: true,
                filter: "[ServiceID] IS NULL AND [CategoryID] IS NOT NULL AND [IsActive] = 1");

            // ? Ensure only 1 generic template
            migrationBuilder.CreateIndex(
                name: "UX_ChecklistTemplate_Generic",
                table: "ChecklistTemplates",
                column: "IsActive",
                unique: true,
                filter: "[ServiceID] IS NULL AND [CategoryID] IS NULL AND [IsActive] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_ChecklistTemplate_ServiceId",
                table: "ChecklistTemplates");

            migrationBuilder.DropIndex(
                name: "UX_ChecklistTemplate_CategoryId",
                table: "ChecklistTemplates");

            migrationBuilder.DropIndex(
                name: "UX_ChecklistTemplate_Generic",
                table: "ChecklistTemplates");
        }
    }
}
