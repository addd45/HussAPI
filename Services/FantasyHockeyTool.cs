using Microsoft.Extensions.Hosting;
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
using YahooFantasyWrapper.Client;
using YahooFantasyWrapper.Models;
using System.Collections.Specialized;

namespace HussAPI.Services
{
    public class FantasyHockeyTool
    {
        ILogger _logger;
        private readonly IYahooFantasyClient _fantasy;
        private readonly IYahooAuthClient _client;

        public FantasyHockeyTool(ILogger<FantasyHockeyTool> logger, IYahooAuthClient client, IYahooFantasyClient fantasyClient)
        {
            _logger = logger;
            _client = client;
            _fantasy = fantasyClient;
        }

        public string GetAccessToken()
        {
            return _client.Auth.AccessToken;
        }

        public string GetAuthUrl()
        {
            return _client.GetLoginLinkUri();
        }

        public async Task<UserInfo> GetUserProfile(NameValueCollection parameters)
        {
            return await _client.GetUserInfo(parameters);
        }
    }
}
