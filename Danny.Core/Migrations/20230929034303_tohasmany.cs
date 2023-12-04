using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Danny.Core.Migrations
{
    /// <inheritdoc />
    public partial class tohasmany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "kline");

            migrationBuilder.CreateTable(
                name: "m_kline",
                columns: table => new
                {
                    stock_code = table.Column<string>(type: "text", nullable: false),
                    timestamp = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    volumn = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    open = table.Column<double>(type: "double precision", nullable: false),
                    high = table.Column<double>(type: "double precision", nullable: false),
                    low = table.Column<double>(type: "double precision", nullable: false),
                    close = table.Column<double>(type: "double precision", nullable: false),
                    chg = table.Column<double>(type: "double precision", nullable: false),
                    percent = table.Column<double>(type: "double precision", nullable: false),
                    turnoverrate = table.Column<double>(type: "double precision", nullable: false),
                    amount = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_m_kline", x => new { x.stock_code, x.timestamp });
                    table.ForeignKey(
                        name: "fk_m_kline_stocks_stock_temp_id",
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
                name: "m_kline");

            migrationBuilder.CreateTable(
                name: "kline",
                columns: table => new
                {
                    stock_code = table.Column<string>(type: "text", nullable: false),
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    amount = table.Column<double>(type: "double precision", nullable: false),
                    chg = table.Column<double>(type: "double precision", nullable: false),
                    close = table.Column<double>(type: "double precision", nullable: false),
                    high = table.Column<double>(type: "double precision", nullable: false),
                    kline_type = table.Column<char>(type: "character(1)", nullable: false),
                    low = table.Column<double>(type: "double precision", nullable: false),
                    open = table.Column<double>(type: "double precision", nullable: false),
                    percent = table.Column<double>(type: "double precision", nullable: false),
                    timestamp = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    turnoverrate = table.Column<double>(type: "double precision", nullable: false),
                    volumn = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
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
    }
}
