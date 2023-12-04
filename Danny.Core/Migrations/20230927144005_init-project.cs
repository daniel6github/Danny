using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Danny.Core.Migrations
{
    /// <inheritdoc />
    public partial class initproject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "stocks",
                columns: table => new
                {
                    code = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    market_code = table.Column<int>(type: "integer", nullable: false),
                    is_valid = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stocks", x => x.code);
                });

            migrationBuilder.CreateTable(
                name: "kline",
                columns: table => new
                {
                    stock_code = table.Column<string>(type: "text", nullable: false),
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    timestamp = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    volumn = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    open = table.Column<double>(type: "double precision", nullable: false),
                    high = table.Column<double>(type: "double precision", nullable: false),
                    low = table.Column<double>(type: "double precision", nullable: false),
                    close = table.Column<double>(type: "double precision", nullable: false),
                    chg = table.Column<double>(type: "double precision", nullable: false),
                    percent = table.Column<double>(type: "double precision", nullable: false),
                    turnoverrate = table.Column<double>(type: "double precision", nullable: false),
                    amount = table.Column<double>(type: "double precision", nullable: false),
                    kline_type = table.Column<char>(type: "character(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_kline", x => new { x.stock_code, x.id });
                    table.ForeignKey(
                        name: "fk_kline_stocks_stock_temp_id",
                        column: x => x.stock_code,
                        principalTable: "stocks",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "kline");

            migrationBuilder.DropTable(
                name: "stocks");
        }
    }
}
