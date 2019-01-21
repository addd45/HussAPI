using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HussAPI.Migrations
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MarketData",
                columns: table => new
                {
                    Symbol = table.Column<string>(nullable: false),
                    TradeDay = table.Column<DateTime>(nullable: false),
                    CompanyName = table.Column<string>(nullable: true),
                    BoughtPrice = table.Column<decimal>(nullable: false),
                    BoughtTime = table.Column<DateTime>(nullable: false),
                    Volume = table.Column<double>(nullable: false),
                    WentUp = table.Column<bool>(nullable: false),
                    MaxPercUp = table.Column<int>(nullable: false),
                    MaxPrice = table.Column<decimal>(nullable: false),
                    MaxPercUpTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketData", x => new { x.TradeDay, x.Symbol });
                    table.UniqueConstraint("AK_MarketData_Symbol", x => x.Symbol);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MarketData");
        }
    }
}
