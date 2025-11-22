using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EVServiceCenter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDescriptionToChecklistTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "ChecklistTemplates",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "UX_OnlinePayment_Gateway_Transaction",
                table: "OnlinePayments",
                columns: new[] { "GatewayName", "GatewayTransactionID" },
                unique: true,
                filter: "[GatewayTransactionId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_OnlinePayment_Gateway_Transaction",
                table: "OnlinePayments");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "ChecklistTemplates");
        }
    }
}
