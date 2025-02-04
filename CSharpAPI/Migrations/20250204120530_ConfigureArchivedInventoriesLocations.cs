using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CSharpAPI.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureArchivedInventoriesLocations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArchivedInventories_locations_ArchivedInventories_ArchivedInventorieModelid",
                table: "ArchivedInventories_locations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ArchivedInventories_locations",
                table: "ArchivedInventories_locations");

            migrationBuilder.RenameColumn(
                name: "ArchivedInventorieModelid",
                table: "ArchivedInventories_locations",
                newName: "ArchivedInventorieModelId");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "ArchivedInventories_locations",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ArchivedInventories_locations",
                table: "ArchivedInventories_locations",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ArchivedInventories_locations_ArchivedInventorieModelId",
                table: "ArchivedInventories_locations",
                column: "ArchivedInventorieModelId");

            migrationBuilder.AddForeignKey(
                name: "FK_ArchivedInventories_locations_ArchivedInventories_ArchivedInventorieModelId",
                table: "ArchivedInventories_locations",
                column: "ArchivedInventorieModelId",
                principalTable: "ArchivedInventories",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArchivedInventories_locations_ArchivedInventories_ArchivedInventorieModelId",
                table: "ArchivedInventories_locations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ArchivedInventories_locations",
                table: "ArchivedInventories_locations");

            migrationBuilder.DropIndex(
                name: "IX_ArchivedInventories_locations_ArchivedInventorieModelId",
                table: "ArchivedInventories_locations");

            migrationBuilder.RenameColumn(
                name: "ArchivedInventorieModelId",
                table: "ArchivedInventories_locations",
                newName: "ArchivedInventorieModelid");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "ArchivedInventories_locations",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ArchivedInventories_locations",
                table: "ArchivedInventories_locations",
                columns: new[] { "ArchivedInventorieModelid", "Id" });

            migrationBuilder.AddForeignKey(
                name: "FK_ArchivedInventories_locations_ArchivedInventories_ArchivedInventorieModelid",
                table: "ArchivedInventories_locations",
                column: "ArchivedInventorieModelid",
                principalTable: "ArchivedInventories",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
