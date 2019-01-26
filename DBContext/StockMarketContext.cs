using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HussAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace HussAPI.DBContext
{
    public class StockMarketContext: DbContext
    {
        public DbSet<MarketData> MarketData { get; set; }
        public DbSet<StockWatchList> WatchList { get; set; }
        
        public StockMarketContext():base(){}
        
        public StockMarketContext(DbContextOptions<StockMarketContext> options) : base(options)
        {            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MarketData>().HasKey(x => new { x.TradeDay, x.Symbol });
        }
        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
         //   => optionsBuilder.UseNpgsql("Host=my_host;Database=my_db;Username=my_user;Password=my_pw");
    }
}
