using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Danny.Core.Migrations
{
    /// <inheritdoc />
    public partial class addperiodtype2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_m_klines_stocks_stock_temp_id1",
                table: "m_klines");

            migrationBuilder.DropPrimaryKey(
                name: "pk_m_klines",
                table: "m_klines");

            migrationBuilder.RenameTable(
                name: "m_klines",
                newName: "klines");

            migrationBuilder.AddPrimaryKey(
                name: "pk_klines",
                table: "klines",
                columns: new[] { "stock_code", "timestamp", "period_type" });

            migrationBuilder.AddForeignKey(
                name: "fk_klines_stocks_stock_temp_id1",
                table: "klines",
                column: "stock_code",
                principalTable: "stocks",
                principalColumn: "code",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_klines_stocks_stock_temp_id1",
                table: "klines");

            migrationBuilder.DropPrimaryKey(
                name: "pk_klines",
                table: "klines");

            migrationBuilder.RenameTable(
                name: "klines",
                newName: "m_klines");

            migrationBuilder.AddPrimaryKey(
                name: "pk_m_klines",
                table: "m_klines",
                columns: new[] { "stock_code", "timestamp", "period_type" });

            migrationBuilder.AddForeignKey(
                name: "fk_m_klines_stocks_stock_temp_id1",
                table: "m_klines",
                column: "stock_code",
                principalTable: "stocks",
                principalColumn: "code",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
