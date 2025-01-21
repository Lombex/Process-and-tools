using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CSharpAPI.Migrations
{
    /// <inheritdoc />
    public partial class ChangedArchivemodels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "order_id",
                table: "ArchivedShipments");

            migrationBuilder.DropColumn(
                name: "shipment_id",
                table: "ArchivedOrders");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "order_id",
                table: "ArchivedShipments",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "shipment_id",
                table: "ArchivedOrders",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }
    }
}
