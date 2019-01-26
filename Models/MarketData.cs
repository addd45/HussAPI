using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace HussAPI.Models
{
    public class MarketData
    {
        [Key]
        public string Symbol { get; set; }
        public DateTime TradeDay { get; set; }
        public string CompanyName { get; set; }
        public decimal BoughtPrice { get; set; }
        public DateTime BoughtTime { get; set; }
        public double Volume { get; set; }
        public bool WentUp { get; set; }
        public decimal MaxPercUp { get; set; }
        public decimal MaxPrice { get; set; }
        public DateTime MaxPercUpTime { get; set; }
    }
}
