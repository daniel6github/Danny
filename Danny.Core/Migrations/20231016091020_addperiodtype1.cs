using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Danny.Core.Migrations
{
    /// <inheritdoc />
    public partial class addperiodtype1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_m_klines",
                table: "m_klines");

            migrationBuilder.AddPrimaryKey(
                name: "pk_m_klines",
                table: "m_klines",
                columns: new[] { "stock_code", "timestamp", "period_type" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_m_klines",
                table: "m_klines");

            migrationBuilder.AddPrimaryKey(
                name: "pk_m_klines",
                table: "m_klines",
                columns: new[] { "stock_code", "timestamp" });
        }
    }
}
