using HussAPI.Interfaces;
using HussAPI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AlphaVantage.Net.Stocks;
using HussAPI.DBContext;
using HussAPI.Models;
using AlphaVantage.Net.Stocks.TimeSeries;
using Microsoft.Extensions.Logging;

namespace HussAPI.Services
{
    public class MarketService : IMarketService
    {
        Timer _marketOpenTimer;
        AlphaVantageStocksClient _alphaClient;
        StockMarketContext _dbContext;
        ILogger _logger;

        public MarketService(ILogger<MarketService> logger)
        {
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("MarketService starting");
            _marketOpenTimer = new Timer(ExecuteService, null, 0, System.Threading.Timeout.Infinite);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("MarketService stopping");            
        }

        private async void ExecuteService(object state)
        {
            _logger.LogInformation("Executing Service");

            if (!TradingHours())
            {
                var nextOpen = DateTime.Today.AddDays(1).AddHours(8); //tomorrow at 8
                var timeSpan = nextOpen - DateTime.Now;
                _marketOpenTimer.Change(timeSpan, TimeSpan.FromMilliseconds( -1));
                
                _logger.LogInformation($"Not market hours, waiting for {timeSpan.TotalHours} more hours");
                return;
            }

            var best = new Dictionary<string, MarketData>();
            var old_interestingStocks = new Dictionary<string, StockInfo>(5);
            while (TradingHours())
            {
                var interestingStocks = await GetStocksOfInterest();
                
                //add all that didnt exist to old object
                foreach(StockInfo iStock in interestingStocks){
                    old_interestingStocks.TryAdd(iStock.Symbol, iStock);
                }

                var allStocks = interestingStocks.Union(old_interestingStocks.Values);
                _logger.LogDebug($"{interestingStocks.Count} interesting stocks found");

                foreach(var interestingStock in allStocks)
                {
                    var quote = await MarketAPIHelper.GetRealTimeQuote(interestingStock.Symbol, 2);

                    //ty next
                //TODO: be smarter about narrowing down (RSI n stuff)
                    if (!ShouldBuy(quote)){
                        _logger.LogDebug($"Not buying {interestingStock.Symbol}");
                        continue;
                    }

                    //fresh means best
                    if (!best.ContainsKey(interestingStock.Symbol))
                    {
                        _logger.LogDebug($"buying stock {interestingStock.Symbol} for {interestingStock.LatestPrice}");

                        best.Add(interestingStock.Symbol, new MarketData(){
                            BoughtPrice = interestingStock.LatestPrice,
                            BoughtTime = DateTime.Now,
                            CompanyName = interestingStock.CompanyName,
                            Symbol = interestingStock.Symbol,
                            WentUp = false,
                            TradeDay = DateTime.Today,
                            MaxPrice = interestingStock.LatestPrice,
                            Volume = interestingStock.LatestVolume
                        });
                    }
                    //Data already exists now check whether or not its better
                    else
                    {
                        var currentData = best[interestingStock.Symbol];
                        var betterStock = GetBetterStock(currentData, interestingStock);
                        best[interestingStock.Symbol] = betterStock;
                    }
                }
                Thread.Sleep(90000);
            }
            await UpdateDatabase(best.Values);
        }

        private MarketData GetBetterStock(MarketData currentData, StockInfo newData)
        {
            //Price went up which is nice
            if (newData.LatestPrice > currentData.BoughtPrice)
            {
                currentData.MaxPrice = newData.LatestPrice;
                currentData.MaxPercUp = ((newData.LatestPrice - currentData.BoughtPrice) / newData.LatestPrice);
                currentData.MaxPercUpTime = DateTime.Now;
                currentData.WentUp = true;

                _logger.LogDebug($"{newData.Symbol} price went up {currentData.MaxPercUp} percent");
                return currentData;
            }
            else
            {
                _logger.LogDebug($"Stock price of {currentData.Symbol} went down {currentData.BoughtPrice - newData.LatestPrice}");
                return currentData;
            }
        }

        private async Task<Queue<StockInfo>> GetStocksOfInterest()
        {
            var mostActive = await MarketAPIHelper.GetMostActive();

            var myList = from quote in mostActive
                         where quote.latestPrice < 7
                         where quote.latestVolume > quote.avgTotalVolume
                         where quote.primaryExchange.ToLower().StartsWith("nasdaq")
                         //where quote.week52High < quote.latestPrice //TODO: think about it
                         select new StockInfo(quote);

            return new Queue<StockInfo>(myList);
        }

        private bool ShouldBuy(IEnumerable<StockDataPoint> dataPoints)
        {
            if (dataPoints == null && dataPoints.Count() < 2){
                return false;
            }

            //TODO: smarter stuff
            var dataPoint_newer = dataPoints.First();
            var dataPoint_older = dataPoints.ElementAt(1);

            //recent point closed higher than previous closed and volume increasin
            return (dataPoint_newer.ClosingPrice > dataPoint_older.ClosingPrice &&  dataPoint_newer.Volume > dataPoint_older.Volume);
        }

        private async Task UpdateDatabase(IEnumerable<MarketData> marketData)
        {
            using (var context = new StockMarketContext())
            {
                await _dbContext.AddRangeAsync(marketData);
                await context.SaveChangesAsync();
            }
        }

        private bool TradingHours()
        {
            //return true;
            var timeSinceMidnight = DateTime.Now.TimeOfDay;

            //between 8 - 3
            if (timeSinceMidnight.Hours >= 7 && timeSinceMidnight.Hours < 16)
            {
                return true;
            }
            return false;
        }
        
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
