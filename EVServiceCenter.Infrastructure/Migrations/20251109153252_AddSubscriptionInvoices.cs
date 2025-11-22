using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EVServiceCenter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptionInvoices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InvoiceID",
                table: "CustomerPackageSubscriptions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerPackageSubscriptions_InvoiceID",
                table: "CustomerPackageSubscriptions",
                column: "InvoiceID");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerPackageSubscriptions_Invoices",
                table: "CustomerPackageSubscriptions",
                column: "InvoiceID",
                principalTable: "Invoices",
                principalColumn: "InvoiceID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerPackageSubscriptions_Invoices",
                table: "CustomerPackageSubscriptions");

            migrationBuilder.DropIndex(
                name: "IX_CustomerPackageSubscriptions_InvoiceID",
                table: "CustomerPackageSubscriptions");

            migrationBuilder.DropColumn(
                name: "InvoiceID",
                table: "CustomerPackageSubscriptions");
        }
    }
}
