﻿// <auto-generated />
using System;
using HussAPI.DBContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace HussAPI.Migrations
{
    [DbContext(typeof(StockMarketContext))]
    [Migration("20190125022349_watchlist")]
    partial class watchlist
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.1.4-rtm-31024")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("HussAPI.Models.MarketData", b =>
                {
                    b.Property<DateTime>("TradeDay");

                    b.Property<string>("Symbol");

                    b.Property<decimal>("BoughtPrice");

                    b.Property<DateTime>("BoughtTime");

                    b.Property<string>("CompanyName");

                    b.Property<decimal>("MaxPercUp");

                    b.Property<DateTime>("MaxPercUpTime");

                    b.Property<decimal>("MaxPrice");

                    b.Property<double>("Volume");

                    b.Property<bool>("WentUp");

                    b.HasKey("TradeDay", "Symbol");

                    b.HasAlternateKey("Symbol");

                    b.ToTable("MarketData");
                });
#pragma warning restore 612, 618
        }
    }
}
