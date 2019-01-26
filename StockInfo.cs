using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HussAPI.Helpers;

namespace HussAPI
{
    public class StockInfo
    {
        public StockInfo(CompanyInfoResponse quote)
        {
            AverageTotalVolume = quote.avgTotalVolume;
            ChangePercent = quote.changePercent;
            CompanyName = quote.companyName;
            LatestPrice =  quote.latestPrice;
            LatestVolume = quote.latestVolume;
            PERatio = quote.peRatio;
            Symbol = quote.symbol;
            Week52High = quote.week52High;
            Week52Low = quote.week52Low;         
        }

        public string Symbol { get; set; }
        public string CompanyName { get; set; }
        public decimal LatestPrice { get; set; }
        public double LatestVolume { get; set; }
        public double ChangePercent { get; set; }
        public double AverageTotalVolume { get; set; }
        public double? PERatio { get; set; }
        public decimal Week52High { get; set; }
        public decimal Week52Low { get; set; }
    }
}
