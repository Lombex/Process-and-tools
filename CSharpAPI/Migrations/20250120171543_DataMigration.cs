using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CSharpAPI.Migrations
{
    /// <inheritdoc />
    public partial class DataMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApiUsers",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    api_key = table.Column<string>(type: "TEXT", nullable: false),
                    app = table.Column<string>(type: "TEXT", nullable: false),
                    role = table.Column<string>(type: "TEXT", nullable: false),
                    warehouse_id = table.Column<int>(type: "INTEGER", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiUsers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ClientModels",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", nullable: true),
                    address = table.Column<string>(type: "TEXT", nullable: true),
                    city = table.Column<string>(type: "TEXT", nullable: true),
                    zip_code = table.Column<string>(type: "TEXT", nullable: true),
                    province = table.Column<string>(type: "TEXT", nullable: true),
                    country = table.Column<string>(type: "TEXT", nullable: true),
                    contact_name = table.Column<string>(type: "TEXT", nullable: true),
                    contact_phone = table.Column<string>(type: "TEXT", nullable: true),
                    contact_email = table.Column<string>(type: "TEXT", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientModels", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "DockModels",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    warehouse_id = table.Column<int>(type: "INTEGER", nullable: false),
                    code = table.Column<string>(type: "TEXT", nullable: true),
                    name = table.Column<string>(type: "TEXT", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DockModels", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Inventors",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    item_id = table.Column<string>(type: "TEXT", nullable: true),
                    description = table.Column<string>(type: "TEXT", nullable: true),
                    item_reference = table.Column<string>(type: "TEXT", nullable: true),
                    locations = table.Column<string>(type: "TEXT", nullable: true),
                    total_on_hand = table.Column<int>(type: "INTEGER", nullable: false),
                    total_expected = table.Column<int>(type: "INTEGER", nullable: false),
                    total_ordered = table.Column<int>(type: "INTEGER", nullable: false),
                    total_allocated = table.Column<int>(type: "INTEGER", nullable: false),
                    total_available = table.Column<int>(type: "INTEGER", nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inventors", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ItemGroups",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", nullable: true),
                    description = table.Column<string>(type: "TEXT", nullable: true),
                    itemtype_id = table.Column<int>(type: "INTEGER", nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemGroups", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ItemLine",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", nullable: true),
                    description = table.Column<string>(type: "TEXT", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemLine", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "itemModels",
                columns: table => new
                {
                    uid = table.Column<string>(type: "TEXT", nullable: false),
                    code = table.Column<string>(type: "TEXT", nullable: true),
                    description = table.Column<string>(type: "TEXT", nullable: true),
                    short_description = table.Column<string>(type: "TEXT", nullable: true),
                    upc_code = table.Column<string>(type: "TEXT", nullable: true),
                    model_number = table.Column<string>(type: "TEXT", nullable: true),
                    commodity_code = table.Column<string>(type: "TEXT", nullable: true),
                    item_line = table.Column<int>(type: "INTEGER", nullable: false),
                    item_group = table.Column<int>(type: "INTEGER", nullable: false),
                    item_type = table.Column<int>(type: "INTEGER", nullable: false),
                    unit_purchase_quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    unit_order_quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    pack_order_quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    supplier_id = table.Column<int>(type: "INTEGER", nullable: false),
                    supplier_code = table.Column<string>(type: "TEXT", nullable: true),
                    supplier_part_number = table.Column<string>(type: "TEXT", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_itemModels", x => x.uid);
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    item_id = table.Column<string>(type: "TEXT", nullable: false),
                    amount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.item_id);
                });

            migrationBuilder.CreateTable(
                name: "ItemType",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", nullable: true),
                    description = table.Column<string>(type: "TEXT", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemType", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Location",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    warehouse_id = table.Column<int>(type: "INTEGER", nullable: false),
                    code = table.Column<string>(type: "TEXT", nullable: true),
                    name = table.Column<string>(type: "TEXT", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Location", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Order",
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
                    total_amount = table.Column<float>(type: "REAL", nullable: false),
                    total_discount = table.Column<float>(type: "REAL", nullable: false),
                    total_tax = table.Column<float>(type: "REAL", nullable: false),
                    total_surcharge = table.Column<float>(type: "REAL", nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    items = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Order", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "OrderShipments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrderId = table.Column<int>(type: "INTEGER", nullable: false),
                    ShipmentId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderShipments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    role = table.Column<string>(type: "TEXT", nullable: false),
                    resource = table.Column<string>(type: "TEXT", nullable: false),
                    can_view = table.Column<bool>(type: "INTEGER", nullable: false),
                    can_create = table.Column<bool>(type: "INTEGER", nullable: false),
                    can_update = table.Column<bool>(type: "INTEGER", nullable: false),
                    can_delete = table.Column<bool>(type: "INTEGER", nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Shipment",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
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
                    items = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shipment", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Suppliers",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    code = table.Column<string>(type: "TEXT", nullable: true),
                    name = table.Column<string>(type: "TEXT", nullable: true),
                    address = table.Column<string>(type: "TEXT", nullable: true),
                    address_extra = table.Column<string>(type: "TEXT", nullable: true),
                    city = table.Column<string>(type: "TEXT", nullable: true),
                    zip_code = table.Column<string>(type: "TEXT", nullable: true),
                    province = table.Column<string>(type: "TEXT", nullable: true),
                    country = table.Column<string>(type: "TEXT", nullable: true),
                    contact_name = table.Column<string>(type: "TEXT", nullable: true),
                    contact_phone = table.Column<string>(type: "TEXT", nullable: true),
                    contact_email = table.Column<string>(type: "TEXT", nullable: true),
                    reference = table.Column<string>(type: "TEXT", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suppliers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Transfer",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    reference = table.Column<string>(type: "TEXT", nullable: true),
                    transfer_from = table.Column<int>(type: "INTEGER", nullable: true),
                    transfer_to = table.Column<int>(type: "INTEGER", nullable: true),
                    transfer_status = table.Column<string>(type: "TEXT", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    items = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transfer", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Warehouse",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    code = table.Column<string>(type: "TEXT", nullable: true),
                    name = table.Column<string>(type: "TEXT", nullable: true),
                    address = table.Column<string>(type: "TEXT", nullable: true),
                    zip = table.Column<string>(type: "TEXT", nullable: true),
                    city = table.Column<string>(type: "TEXT", nullable: true),
                    province = table.Column<string>(type: "TEXT", nullable: true),
                    country = table.Column<string>(type: "TEXT", nullable: true),
                    contact_name = table.Column<string>(type: "TEXT", nullable: true),
                    contact_phone = table.Column<string>(type: "TEXT", nullable: true),
                    contact_email = table.Column<string>(type: "TEXT", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Warehouse", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApiUsers_api_key",
                table: "ApiUsers",
                column: "api_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_role_resource",
                table: "RolePermissions",
                columns: new[] { "role", "resource" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApiUsers");

            migrationBuilder.DropTable(
                name: "ClientModels");

            migrationBuilder.DropTable(
                name: "DockModels");

            migrationBuilder.DropTable(
                name: "Inventors");

            migrationBuilder.DropTable(
                name: "ItemGroups");

            migrationBuilder.DropTable(
                name: "ItemLine");

            migrationBuilder.DropTable(
                name: "itemModels");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "ItemType");

            migrationBuilder.DropTable(
                name: "Location");

            migrationBuilder.DropTable(
                name: "Order");

            migrationBuilder.DropTable(
                name: "OrderShipments");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "Shipment");

            migrationBuilder.DropTable(
                name: "Suppliers");

            migrationBuilder.DropTable(
                name: "Transfer");

            migrationBuilder.DropTable(
                name: "Warehouse");
        }
    }
}
