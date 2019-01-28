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
using Microsoft.Extensions.DependencyInjection;

namespace HussAPI.Services
{
    public class MarketService : IMarketService
    {
        Timer _marketOpenTimer;
        AlphaVantageStocksClient _alphaClient;
        private readonly IServiceScopeFactory _scopeFactory;
        ILogger _logger;

        public MarketService(ILogger<MarketService> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
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
                SetTimer();
                return;
            }

            await AnalyzeStocks();
            SetTimer();
        }

        private void SetTimer()
        {
            var nextOpen = DateTime.Today.AddDays(1).AddHours(8); //tomorrow at 8
            var timeSpan = nextOpen - DateTime.Now;
            _marketOpenTimer.Change(timeSpan, TimeSpan.FromMilliseconds(-1));

            _logger.LogInformation($"Not market hours, waiting for {timeSpan.TotalHours} more hours");
            return;
        }

        private async Task AnalyzeStocks()
        {
            Dictionary<string, MarketData> marketDatas = new Dictionary<string, MarketData>(10);
            MarketServiceHelper.DailyOldestPrices = new Dictionary<string, decimal>();

            while (TradingHours())
            {
                Queue<StockInfo> toBuyStocks = new Queue<StockInfo>();
                if (DateTime.Now.Hour <= 10)
                {
                    toBuyStocks = await GetStocksToBuy();
                }
                else if (toBuyStocks.Count == 0) // the time has passed to look for stocks and we didn't find anything. we're done.
                {
                    return;
                }

                _logger.LogDebug($"{toBuyStocks.Count} interesting stocks found");

                foreach (var buyStocks in toBuyStocks)
                {
                    //fresh means best
                    if (!marketDatas.ContainsKey(buyStocks.Symbol))
                    {
                        _logger.LogDebug($"buying stock {buyStocks.Symbol} for {buyStocks.LatestPrice}");

                        marketDatas.Add(buyStocks.Symbol, new MarketData()
                        {
                            BoughtPrice = buyStocks.LatestPrice,
                            BoughtTime = DateTime.Now,
                            CompanyName = "notSupportedCurrently",
                            Symbol = buyStocks.Symbol,
                            WentUp = false,
                            TradeDay = DateTime.Today,
                            MaxPrice = buyStocks.LatestPrice,
                            Volume = buyStocks.LatestVolume
                        });
                    }
                    //Data already exists now check whether or not its better
                    else
                    {
                        var currentData = marketDatas[buyStocks.Symbol];
                        var betterStock = GetBetterStock(currentData, buyStocks);
                        marketDatas[buyStocks.Symbol] = betterStock;
                    }
                }
                Thread.Sleep(90000); //wait 1.5 min to check again
            }
            await UpdateDatabase(marketDatas.Values);
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

        private async Task<Queue<StockInfo>> GetStocksToBuy()
        {
            using (var dbContext = GetDbContext())
            {
                var watchStocks = dbContext.WatchList.Where(x => x.TradeDay.Date == DateTime.Today.Date).ToArray();

                var watchSymbols = watchStocks.Select(x => x.Symbol);
                int minutesSinceOpen = (DateTime.Now - DateTime.Today.Date.AddHours(8)).Minutes;
                var stockQuotes = await MarketAPIHelper.GetQuotes(watchSymbols.ToArray());
                var ret = new Queue<StockInfo>(watchStocks.Length);

                foreach (var quote in stockQuotes)
                {
                    var dataPoints = await MarketAPIHelper.GetIntraDayInfo(quote.Symbol, minutesSinceOpen + 1);
                    var watchStock = watchStocks.Where(x => x.Symbol == quote.Symbol).FirstOrDefault();

                    if (await ShouldBuy(watchStock.Week52High, watchStock.AverageVolume, dataPoints, quote.Symbol))
                    {
                        var tempSI = new StockInfo(quote)
                        {
                            Week52High = watchStock.Week52High,
                            AverageTotalVolume = (double)watchStock.AverageVolume
                        };
                        ret.Enqueue(tempSI);
                    }
                    else
                    {
                        _logger.LogDebug("Deciding not to buy {0} .... yet", quote.Symbol);
                    }
                }

                return ret;
            }
        }

        private async Task<bool> ShouldBuy(decimal week52high, decimal avgVol, IEnumerable<StockDataPoint> dataPoints, string symbol)
        {
            var list_dataPoints = dataPoints.ToList();
            if (list_dataPoints == null && list_dataPoints.Count < 2) {
                return false;
            }

            var currentOldest = list_dataPoints.Where(x => x.Time.Day == DateTime.Today.Day).Last();
            decimal currentOldestPrice = currentOldest.OpeningPrice;
            bool firstCheck = MarketServiceHelper.DailyOldestPrices.TryAdd(symbol, currentOldestPrice); //add oldest price if it doesnt exist already
            var oldestTime = currentOldest.Time.TimeOfDay; 
            bool isTodaysOpenValues = oldestTime.Hours == 9 && oldestTime.Minutes < 36; //found the damn near open price
            int indexOfoldest = list_dataPoints.IndexOf(currentOldest);
            list_dataPoints.RemoveRange(indexOfoldest, (list_dataPoints.Count - 1 - indexOfoldest)); //TODO: check this removes all the yesterday values
            bool trendingUpish = TrendingUp(list_dataPoints.Select(l => l.ClosingPrice).ToArray());

            //Check for 52 week high momentum
            ///is it early and high volume and price above 52 week high and trending up?
            if (isTodaysOpenValues && trendingUpish && list_dataPoints.First().ClosingPrice > week52high && list_dataPoints.First().Volume > avgVol)
            {
                return true;
            }
            else if (trendingUpish)
            {
                //GET RSI
                var rsiTask = MarketAPIHelper.GetRSI(symbol);
                var cciTask = MarketAPIHelper.GetCCI(symbol);
                var aroonTask = MarketAPIHelper.GetAroonOsc(symbol);

                var results = await Task.WhenAll(rsiTask, cciTask, aroonTask);
                decimal smartValue = SmartAlgorithm(results[0], results[1], results[2]);

                return (smartValue >= 60); //TODO: obvi
            }

            return false;
        }


        //return a confidence percentage from 0 - 100
        private decimal SmartAlgorithm(Queue<decimal> rsi, Queue<decimal> cci, Queue<decimal> aroon)
        {
            bool rsiTrendUp = TrendingUp(rsi.ToArray());
            bool cciTrendUp = TrendingUp(cci.ToArray());
            bool aroonTrendup = TrendingUp(aroon.ToArray());

            //TODO figure out if we can even use these to predict
            int binary = Convert.ToInt32(rsiTrendUp) + Convert.ToInt32(cciTrendUp) + Convert.ToInt32(aroonTrendup);


            return 30 * binary; //TODO: whatever smart value I come up with that eventually determines buy or nah
        }

        private bool TrendingUp(params decimal[] values)
        {
            int denom = values.Count();
            if (denom < 1)
            {
                return false;
            }

            decimal last = values.Last();
            decimal avg = values.Average();

            return last > avg;
        }

        private async Task UpdateDatabase(IEnumerable<MarketData> marketData)
        {
            using (var dbContext = GetDbContext())
            {
                await dbContext.AddRangeAsync(marketData);
                await dbContext.SaveChangesAsync();
            }
        }

        private bool TradingHours()
        {
            if (DateTime.Today.DayOfWeek == DayOfWeek.Saturday || DateTime.Today.DayOfWeek == DayOfWeek.Sunday){
                return false;
            }
            var timeSinceMidnight = DateTime.Now.TimeOfDay;

            //between 8 - 3
            if (timeSinceMidnight.Hours >= 7 && timeSinceMidnight.Hours < 16)
            {
                return true;
            }
            return false;
        }

        private StockMarketContext GetDbContext()
        {
            var scope = _scopeFactory.CreateScope();
            return scope.ServiceProvider.GetRequiredService<StockMarketContext>();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
