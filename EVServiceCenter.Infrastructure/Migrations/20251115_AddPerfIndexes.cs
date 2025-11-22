using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EVServiceCenter.Infrastructure.Migrations
{
    public partial class AddPerfIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Index to speed up lookups by ModelId when filtering services for a model
            migrationBuilder.CreateIndex(
                name: "IX_ModelServicePricings_ModelId",
                table: "ModelServicePricings",
                column: "ModelId");

            // Index to speed up finding packages that contain a specific service
            migrationBuilder.CreateIndex(
                name: "IX_PackageServices_ServiceId",
                table: "PackageServices",
                column: "ServiceId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ModelServicePricings_ModelId",
                table: "ModelServicePricings");

            migrationBuilder.DropIndex(
                name: "IX_PackageServices_ServiceId",
                table: "PackageServices");
        }
    }
}
