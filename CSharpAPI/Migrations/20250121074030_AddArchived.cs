using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CSharpAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddArchived : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ArchivedOrderModelid",
                table: "Items",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ArchivedShipmentModelid",
                table: "Items",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ArchivedItems",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    uid = table.Column<string>(type: "TEXT", nullable: true),
                    description = table.Column<string>(type: "TEXT", nullable: true),
                    short_description = table.Column<string>(type: "TEXT", nullable: true),
                    upc_code = table.Column<string>(type: "TEXT", nullable: true),
                    model_number = table.Column<string>(type: "TEXT", nullable: true),
                    commodity_code = table.Column<string>(type: "TEXT", nullable: true),
                    item_line = table.Column<string>(type: "TEXT", nullable: true),
                    item_group = table.Column<string>(type: "TEXT", nullable: true),
                    item_type = table.Column<string>(type: "TEXT", nullable: true),
                    unit_purchase_quantity = table.Column<float>(type: "REAL", nullable: false),
                    unit_order_quantity = table.Column<float>(type: "REAL", nullable: false),
                    pack_order_quantity = table.Column<float>(type: "REAL", nullable: false),
                    supplier_id = table.Column<int>(type: "INTEGER", nullable: false),
                    supplier_code = table.Column<string>(type: "TEXT", nullable: true),
                    supplier_part_number = table.Column<string>(type: "TEXT", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    archived_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchivedItems", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ArchivedOrders",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    source_id = table.Column<int>(type: "INTEGER", nullable: false),
                    order_date = table.Column<string>(type: "TEXT", nullable: true),
                    request_date = table.Column<string>(type: "TEXT", nullable: true),
                    reference = table.Column<string>(type: "TEXT", nullable: true),
                    reference_extra = table.Column<string>(type: "TEXT", nullable: true),
                    order_status = table.Column<string>(type: "TEXT", nullable: true),
                    notes = table.Column<string>(type: "TEXT", nullable: true),
                    shipping_notes = table.Column<string>(type: "TEXT", nullable: true),
                    picking_notes = table.Column<string>(type: "TEXT", nullable: true),
                    warehouse_id = table.Column<int>(type: "INTEGER", nullable: false),
                    ship_to = table.Column<int>(type: "INTEGER", nullable: false),
                    bill_to = table.Column<int>(type: "INTEGER", nullable: false),
                    shipment_id = table.Column<int>(type: "INTEGER", nullable: false),
                    total_amount = table.Column<float>(type: "REAL", nullable: false),
                    total_discount = table.Column<float>(type: "REAL", nullable: false),
                    total_tax = table.Column<float>(type: "REAL", nullable: false),
                    total_surcharge = table.Column<float>(type: "REAL", nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    archived_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchivedOrders", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ArchivedShipments",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    order_id = table.Column<int>(type: "INTEGER", nullable: false),
                    source_id = table.Column<int>(type: "INTEGER", nullable: false),
                    order_date = table.Column<string>(type: "TEXT", nullable: true),
                    request_date = table.Column<string>(type: "TEXT", nullable: true),
                    shipment_date = table.Column<string>(type: "TEXT", nullable: true),
                    shipment_type = table.Column<string>(type: "TEXT", nullable: true),
                    shipment_status = table.Column<string>(type: "TEXT", nullable: true),
                    notes = table.Column<string>(type: "TEXT", nullable: true),
                    carrier_code = table.Column<string>(type: "TEXT", nullable: true),
                    carrier_description = table.Column<string>(type: "TEXT", nullable: true),
                    service_code = table.Column<string>(type: "TEXT", nullable: true),
                    payment_type = table.Column<string>(type: "TEXT", nullable: true),
                    transfer_mode = table.Column<string>(type: "TEXT", nullable: true),
                    total_package_count = table.Column<int>(type: "INTEGER", nullable: false),
                    total_package_weight = table.Column<float>(type: "REAL", nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    archived_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchivedShipments", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Items_ArchivedOrderModelid",
                table: "Items",
                column: "ArchivedOrderModelid");

            migrationBuilder.CreateIndex(
                name: "IX_Items_ArchivedShipmentModelid",
                table: "Items",
                column: "ArchivedShipmentModelid");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_ArchivedOrders_ArchivedOrderModelid",
                table: "Items",
                column: "ArchivedOrderModelid",
                principalTable: "ArchivedOrders",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_ArchivedShipments_ArchivedShipmentModelid",
                table: "Items",
                column: "ArchivedShipmentModelid",
                principalTable: "ArchivedShipments",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Items_ArchivedOrders_ArchivedOrderModelid",
                table: "Items");

            migrationBuilder.DropForeignKey(
                name: "FK_Items_ArchivedShipments_ArchivedShipmentModelid",
                table: "Items");

            migrationBuilder.DropTable(
                name: "ArchivedItems");

            migrationBuilder.DropTable(
                name: "ArchivedOrders");

            migrationBuilder.DropTable(
                name: "ArchivedShipments");

            migrationBuilder.DropIndex(
                name: "IX_Items_ArchivedOrderModelid",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Items_ArchivedShipmentModelid",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "ArchivedOrderModelid",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "ArchivedShipmentModelid",
                table: "Items");
        }
    }
}
