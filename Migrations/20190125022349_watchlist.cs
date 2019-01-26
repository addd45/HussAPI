using Microsoft.EntityFrameworkCore.Migrations;

namespace HussAPI.Migrations
{
    public partial class watchlist : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "MaxPercUp",
                table: "MarketData",
                nullable: false,
                oldClrType: typeof(int));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "MaxPercUp",
                table: "MarketData",
                nullable: false,
                oldClrType: typeof(decimal));
        }
    }
}
