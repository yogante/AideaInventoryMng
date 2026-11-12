using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AideaInventoryMng.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStocktakeLineDifferenceConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_stocktake_lines_difference_quantity",
                table: "stocktake_lines");

            migrationBuilder.AddCheckConstraint(
                name: "ck_stocktake_lines_difference_quantity",
                table: "stocktake_lines",
                sql: "(\r\nactual_quantity IS NULL\r\nAND difference_quantity IS NULL\r\n) OR (\r\n    actual_quantity IS NOT NULL\r\n    AND difference_quantity IS NOT NULL\r\n    AND difference_quantity = actual_quantity - book_quantity\r\n)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_stocktake_lines_difference_quantity",
                table: "stocktake_lines");

            migrationBuilder.AddCheckConstraint(
                name: "ck_stocktake_lines_difference_quantity",
                table: "stocktake_lines",
                sql: "difference_quantity IS NULL\r\nOR (\r\n    actual_quantity IS NOT NULL\r\n    AND difference_quantity = actual_quantity - book_quantity\r\n)");
        }
    }
}
