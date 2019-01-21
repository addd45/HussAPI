using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HussAPI
{
    public class StockInfo
    {
        public string Symbol { get; set; }
        public string CompanyName { get; set; }
        public decimal LatestPrice { get; set; }
        public double LatestVolume { get; set; }
        public decimal ChangePercent { get; set; }
        public double AverageTotalVolume { get; set; }
        public double PERatio { get; set; }
        public decimal Week52High { get; set; }
        public decimal Week52Low { get; set; }
    }
}
