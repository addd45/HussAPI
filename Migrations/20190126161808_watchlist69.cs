using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HussAPI.Migrations
{
    public partial class watchlist69 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WatchList",
                columns: table => new
                {
                    Symbol = table.Column<string>(nullable: false),
                    TradeDay = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WatchList", x => new { x.Symbol, x.TradeDay });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WatchList");
        }
    }
}
