using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AideaInventoryMng.Migrations
{
    /// <inheritdoc />
    public partial class AddVersionToWarehousesAndLocations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_stock_movements_location_id",
                table: "stock_movements");

            migrationBuilder.DropIndex(
                name: "IX_stock_movements_product_id",
                table: "stock_movements");

            migrationBuilder.AddColumn<int>(
                name: "version",
                table: "warehouses",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "version",
                table: "locations",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddCheckConstraint(
                name: "ck_warehouses_version",
                table: "warehouses",
                sql: "version >= 1");

            migrationBuilder.CreateIndex(
                name: "IX_stock_movements_location_id_occurred_at",
                table: "stock_movements",
                columns: new[] { "location_id", "occurred_at" });

            migrationBuilder.CreateIndex(
                name: "IX_stock_movements_product_id_occurred_at",
                table: "stock_movements",
                columns: new[] { "product_id", "occurred_at" });

            migrationBuilder.CreateIndex(
                name: "IX_stock_movements_transfer_no",
                table: "stock_movements",
                column: "transfer_no");

            migrationBuilder.AddCheckConstraint(
                name: "ck_locations_version",
                table: "locations",
                sql: "version >= 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_warehouses_version",
                table: "warehouses");

            migrationBuilder.DropIndex(
                name: "IX_stock_movements_location_id_occurred_at",
                table: "stock_movements");

            migrationBuilder.DropIndex(
                name: "IX_stock_movements_product_id_occurred_at",
                table: "stock_movements");

            migrationBuilder.DropIndex(
                name: "IX_stock_movements_transfer_no",
                table: "stock_movements");

            migrationBuilder.DropCheckConstraint(
                name: "ck_locations_version",
                table: "locations");

            migrationBuilder.DropColumn(
                name: "version",
                table: "warehouses");

            migrationBuilder.DropColumn(
                name: "version",
                table: "locations");

            migrationBuilder.CreateIndex(
                name: "IX_stock_movements_location_id",
                table: "stock_movements",
                column: "location_id");

            migrationBuilder.CreateIndex(
                name: "IX_stock_movements_product_id",
                table: "stock_movements",
                column: "product_id");
        }
    }
}
