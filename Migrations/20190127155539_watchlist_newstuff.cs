using Microsoft.EntityFrameworkCore.Migrations;

namespace HussAPI.Migrations
{
    public partial class watchlist_newstuff : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AverageVolume",
                table: "WatchList",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Week52High",
                table: "WatchList",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AverageVolume",
                table: "WatchList");

            migrationBuilder.DropColumn(
                name: "Week52High",
                table: "WatchList");
        }
    }
}
