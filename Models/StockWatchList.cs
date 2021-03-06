using System;
using System.ComponentModel.DataAnnotations;

namespace HussAPI.Models
{
    public class StockWatchList
    {
        [Key]
        public string Symbol { get; set; }
        [Key]
        public DateTime TradeDay { get; set; }
        [Required]
        public decimal Week52High { get; set; }
        [Required]
        public decimal AverageVolume { get; set; }
    }
}