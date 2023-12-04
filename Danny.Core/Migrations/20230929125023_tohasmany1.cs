using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Danny.Core.Migrations
{
    /// <inheritdoc />
    public partial class tohasmany1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_m_kline_stocks_stock_temp_id",
                table: "m_kline");

            migrationBuilder.DropPrimaryKey(
                name: "pk_m_kline",
                table: "m_kline");

            migrationBuilder.RenameTable(
                name: "m_kline",
                newName: "m_klines");

            migrationBuilder.AddPrimaryKey(
                name: "pk_m_klines",
                table: "m_klines",
                columns: new[] { "stock_code", "timestamp" });

            migrationBuilder.AddForeignKey(
                name: "fk_m_klines_stocks_stock_temp_id",
                table: "m_klines",
                column: "stock_code",
                principalTable: "stocks",
                principalColumn: "code",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_m_klines_stocks_stock_temp_id",
                table: "m_klines");

            migrationBuilder.DropPrimaryKey(
                name: "pk_m_klines",
                table: "m_klines");

            migrationBuilder.RenameTable(
                name: "m_klines",
                newName: "m_kline");

            migrationBuilder.AddPrimaryKey(
                name: "pk_m_kline",
                table: "m_kline",
                columns: new[] { "stock_code", "timestamp" });

            migrationBuilder.AddForeignKey(
                name: "fk_m_kline_stocks_stock_temp_id",
                table: "m_kline",
                column: "stock_code",
                principalTable: "stocks",
                principalColumn: "code",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
