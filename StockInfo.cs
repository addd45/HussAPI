using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HussAPI.Helpers;

namespace HussAPI
{
    public class StockInfo
    {
        public StockInfo(AlphaVantage.Net.Stocks.BatchQuotes.StockQuote quote)
        {
            LatestPrice =  quote.Price;
            LatestVolume = (long)quote.Volume;
            Symbol = quote.Symbol;      
        }

        public string Symbol { get; set; }
        public string CompanyName { get; set; }
        public decimal LatestPrice { get; set; }
        public double LatestVolume { get; set; }
        public double ChangePercent { get; set; }
        public double AverageTotalVolume { get; set; }
        public decimal Week52High { get; set; }
        public decimal Week52Low { get; set; }
    }
}
