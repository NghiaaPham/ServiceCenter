using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EVServiceCenter.Infrastructure.Migrations.WorkOrders
{
    /// <inheritdoc />
    public partial class AddMileageFields2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ActualMileage",
                table: "WorkOrders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CustomerReportedMileage",
                table: "Appointments",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActualMileage",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "CustomerReportedMileage",
                table: "Appointments");
        }
    }
}
