using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EVServiceCenter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AllowNullableInvoiceWorkOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DECLARE @constraint NVARCHAR(200);
SELECT @constraint = df.name
FROM sys.default_constraints df
JOIN sys.columns c ON df.parent_object_id = c.object_id AND df.parent_column_id = c.column_id
WHERE df.parent_object_id = OBJECT_ID('dbo.Invoices') AND c.name = 'WorkOrderID';
IF @constraint IS NOT NULL
BEGIN
    EXEC('ALTER TABLE [dbo].[Invoices] DROP CONSTRAINT [' + @constraint + ']');
END");

            migrationBuilder.Sql(@"
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Invoices_WorkOrderID_InvoiceDate' AND object_id = OBJECT_ID('dbo.Invoices'))
BEGIN
    DROP INDEX [IX_Invoices_WorkOrderID_InvoiceDate] ON [dbo].[Invoices];
END");

            migrationBuilder.AlterColumn<int>(
                name: "WorkOrderID",
                table: "Invoices",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Invoices_WorkOrderID_InvoiceDate' AND object_id = OBJECT_ID('dbo.Invoices'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Invoices_WorkOrderID_InvoiceDate]
    ON [dbo].[Invoices] ([WorkOrderID], [InvoiceDate])
    INCLUDE ([Status], [GrandTotal], [OutstandingAmount])
    WHERE [WorkOrderID] IS NOT NULL
    WITH (ONLINE = ON, FILLFACTOR = 90);
END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Invoices_WorkOrderID_InvoiceDate' AND object_id = OBJECT_ID('dbo.Invoices'))
BEGIN
    DROP INDEX [IX_Invoices_WorkOrderID_InvoiceDate] ON [dbo].[Invoices];
END");

            migrationBuilder.AlterColumn<int>(
                name: "WorkOrderID",
                table: "Invoices",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Invoices_WorkOrderID_InvoiceDate' AND object_id = OBJECT_ID('dbo.Invoices'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Invoices_WorkOrderID_InvoiceDate]
    ON [dbo].[Invoices] ([WorkOrderID], [InvoiceDate])
    INCLUDE ([Status], [GrandTotal], [OutstandingAmount])
    WHERE [WorkOrderID] IS NOT NULL
    WITH (ONLINE = ON, FILLFACTOR = 90);
END");
        }
    }
}
