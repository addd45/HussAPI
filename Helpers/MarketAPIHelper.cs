using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AlphaVantage.Net.Stocks;
using AlphaVantage.Net.Core;
using AlphaVantage.Net.Stocks.TimeSeries;
using AlphaVantage.Net.Stocks.BatchQuotes;
using Newtonsoft.Json.Linq;

namespace HussAPI.Helpers
{
    public static class MarketAPIHelper
    {
        const string API_KEY_ALPHA_VANTAGE = "QLB1JP2S3AIBB3TA";

        public async static Task<IEnumerable<StockQuote>> GetQuotes(params string[] symbols)
        {
            var avClient = new AlphaVantageStocksClient(API_KEY_ALPHA_VANTAGE);
            return await avClient.RequestBatchQuotesAsync(symbols);
        }

        public async static Task<List<StockDataPoint>> GetIntraDayInfo(string symbol, int numDataPoints)
        {
            try
            {
                var avClient = new AlphaVantageStocksClient(API_KEY_ALPHA_VANTAGE);
                var info = await avClient.RequestIntradayTimeSeriesAsync(symbol, IntradayInterval.OneMin);
                return info.DataPoints.Take(numDataPoints).ToList();
            }
            catch(Exception e)
            {
                throw;
            }
        }

        public async static Task<Queue<decimal>> GetRSI(string symbol)
        {
            var avClient = new AlphaVantageCoreClient();
            var ret = new Queue<decimal>(5);

            var query = GetDefaultQueryDic(symbol);
            JObject rsiObj = await avClient.RequestApiAsync(API_KEY_ALPHA_VANTAGE, ApiFunction.RSI, query);

            //error parsing
            if (!rsiObj.ContainsKey("Meta Data"))
            {
                throw new AlphaVantage.Net.Core.Exceptions.AlphaVantageException($"Error parsing json object. Got: {rsiObj.ToString()}");
            }
            JToken rsiDataObj = rsiObj["Technical Analysis: RSI"];

            foreach(var timeRsi in rsiDataObj.Take(5))
            {
                decimal wow = timeRsi.First.First.First.Value<decimal>();
                ret.Enqueue(wow);
            }

            return ret;
        }

        public async static Task<Queue<decimal>> GetAroonOsc(string symbol)
        {
            var avClient = new AlphaVantageCoreClient();
            var ret = new Queue<decimal>(5);

            var query = GetDefaultQueryDic(symbol);
            query.Remove("series_type"); // not needed
            JObject aroonObj = await avClient.RequestApiAsync(API_KEY_ALPHA_VANTAGE, ApiFunction.AROONOSC, query);

            //error parsing
            if (!aroonObj.ContainsKey("Meta Data"))
            {
                throw new AlphaVantage.Net.Core.Exceptions.AlphaVantageException($"Error parsing json object. Got: {aroonObj.ToString()}");
            }
            JToken aroonDataObj = aroonObj["Technical Analysis: AROONOSC"];

            foreach (var timeAroon in aroonDataObj.Take(5))
            {
                decimal wow = timeAroon.First.First.First.Value<decimal>();
                ret.Enqueue(wow);
            }

            return ret;
        }

        public async static Task<Queue<decimal>> GetCCI(string symbol)
        {
            var avClient = new AlphaVantageCoreClient();
            var ret = new Queue<decimal>(5);

            var query = GetDefaultQueryDic(symbol);
            query.Remove("series_type"); // not needed
            JObject cciObj = await avClient.RequestApiAsync(API_KEY_ALPHA_VANTAGE, ApiFunction.CCI, query);

            //error parsing
            if (!cciObj.ContainsKey("Meta Data"))
            {
                throw new AlphaVantage.Net.Core.Exceptions.AlphaVantageException($"Error parsing json object. Got: {cciObj.ToString()}");
            }
            JToken cciDataObj = cciObj["Technical Analysis: CCI"];

            foreach (var timeCCI in cciDataObj.Take(5))
            {
                decimal wow = timeCCI.First.First.First.Value<decimal>();
                ret.Enqueue(wow);
            }

            return ret;
        }

        private static Dictionary<string, string> GetDefaultQueryDic(string symbol)
        {
            return new Dictionary<string, string>()
            { {"symbol", symbol}, { "interval", "1min" }, { "time_period", "14" }, {"series_type", "close" } };
        }
    }

}
