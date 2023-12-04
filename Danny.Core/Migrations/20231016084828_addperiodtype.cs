using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Danny.Core.Migrations
{
    /// <inheritdoc />
    public partial class addperiodtype : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "m_tranding_data");

            migrationBuilder.AddColumn<int>(
                name: "period_type",
                table: "m_klines",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "trading_data",
                columns: table => new
                {
                    symbol = table.Column<string>(type: "text", nullable: false),
                    timestamp = table.Column<long>(type: "bigint", nullable: false),
                    period_type = table.Column<int>(type: "integer", nullable: false),
                    value = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_trading_data", x => new { x.symbol, x.timestamp, x.period_type });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "trading_data");

            migrationBuilder.DropColumn(
                name: "period_type",
                table: "m_klines");

            migrationBuilder.CreateTable(
                name: "m_tranding_data",
                columns: table => new
                {
                    symbol = table.Column<string>(type: "text", nullable: false),
                    timestamp = table.Column<long>(type: "bigint", nullable: false),
                    value = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_m_tranding_data", x => new { x.symbol, x.timestamp });
                });
        }
    }
}
