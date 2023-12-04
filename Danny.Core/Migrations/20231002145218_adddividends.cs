using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Danny.Core.Migrations
{
    /// <inheritdoc />
    public partial class adddividends : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_m_klines_stocks_stock_temp_id",
                table: "m_klines");

            migrationBuilder.CreateTable(
                name: "dividends",
                columns: table => new
                {
                    stock_code = table.Column<string>(type: "text", nullable: false),
                    pay_date = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    exclude_right_date = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    currency_code = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dividends", x => new { x.stock_code, x.pay_date });
                    table.ForeignKey(
                        name: "fk_dividends_stocks_stock_temp_id",
                        column: x => x.stock_code,
                        principalTable: "stocks",
                        principalColumn: "code");
                });

            migrationBuilder.AddForeignKey(
                name: "fk_m_klines_stocks_stock_temp_id1",
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
                name: "fk_m_klines_stocks_stock_temp_id1",
                table: "m_klines");

            migrationBuilder.DropTable(
                name: "dividends");

            migrationBuilder.AddForeignKey(
                name: "fk_m_klines_stocks_stock_temp_id",
                table: "m_klines",
                column: "stock_code",
                principalTable: "stocks",
                principalColumn: "code",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
