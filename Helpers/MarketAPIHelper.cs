using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AlphaVantage.Net.Stocks;
using AlphaVantage.Net.Core;
using AlphaVantage.Net.Stocks.TimeSeries;

namespace HussAPI.Helpers
{
    public static class MarketAPIHelper
    {
        const string API_KEY_ALPHA_VANTAGE = "QLB1JP2S3AIBB3TA";

        public async static Task<IEnumerable<CompanyInfoResponse>> GetMostActive()
        {
            string endpoint = "https://api.iextrading.com/1.0/stock/market/collection/list?collectionName=gainers";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    //For IP-API
                    HttpResponseMessage response = await client.GetAsync(endpoint);
                    if (response.IsSuccessStatusCode)
                    {
                        var list = await response.Content.ReadAsAsync<IEnumerable<CompanyInfoResponse>>();
                        return list;              
                    }
                    else throw new HttpRequestException($"response code was {response.StatusCode}");
                }
            }
            catch(Exception){
                throw;
            }
        }

        public async static Task<List<StockDataPoint>> GetRealTimeQuote(string symbol, int numDataPoints)
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

        public async static Task GetRSI(string symbol)
        {
            var avClient = new AlphaVantageCoreClient();
            var query = new Dictionary<string, string>(){{"symbols", "FB,AAPL"}};  
            await avClient.RequestApiAsync(symbol, ApiFunction.RSI, query);
        }
    }

    public class CompanyInfoResponse
    {
        public string symbol { get; set; }
        public string companyName { get; set; }
        public string primaryExchange { get; set; }
        public string sector { get; set; }
        public string calculationPrice { get; set; }
        public decimal open { get; set; }
        public long openTime { get; set; }
        public decimal close { get; set; }
        public long closeTime { get; set; }
        public decimal high { get; set; }
        public decimal low { get; set; }
        public decimal latestPrice { get; set; }
        public string latestSource { get; set; }
        public string latestTime { get; set; }
        public long latestUpdate { get; set; }
        public double latestVolume { get; set; }
        public string iexRealtimePrice { get; set; }
        public string iexRealtimeSize { get; set; }
        public string iexLastUpdated { get; set; }
        public decimal delayedPrice { get; set; }
        public long delayedPriceTime { get; set; }
        public decimal previousClose { get; set; }
        public double change { get; set; }
        public double changePercent { get; set; }
        public string iexMarketPercent { get; set; }
        public string iexVolume { get; set; }
        public double avgTotalVolume { get; set; }
        public string iexBidPrice { get; set; }
        public string iexBidSize { get; set; }
        public string iexAskPrice { get; set; }
        public string iexAskSize { get; set; }
        public long marketCap { get; set; }
        public double? peRatio { get; set; }
        public decimal week52High { get; set; }
        public decimal week52Low { get; set; }
        public double ytdChange { get; set; }
    }
}
