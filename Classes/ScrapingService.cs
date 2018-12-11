using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NHLAPISCrape.BluesScraper;
using HussAPI.Interfaces;
using Newtonsoft.Json;
using Microsoft.Extensions.Options;

namespace HussAPI.Classes
{
    public class ScrapingService : IScrapingService
    {
        ILogger _logger;
        BluesScraper _scraper;
        MqttHelper _mqttHelper;
        IOptions<MQTTSettings> _mqttOptions;
        Timer _gameDayTimer, _liveUpdateTimer;

        public ScrapingService(ILogger<ScrapingService> logger, IOptions<MQTTSettings> mqttOptions)
        {
            _logger = logger;
            _mqttOptions = mqttOptions;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Scraping Service starting");
            _mqttHelper = new MqttHelper(_mqttOptions, _logger);
            _gameDayTimer = new Timer(PollGameDay, null, TimeSpan.Zero, TimeSpan.FromHours(24));

            return;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Scraping Service stopping");

            return;
        }

        public async void PollGameDay(object state)
        {
           var info = await GameDayCheck();

            //If game day
            if (info.Item1)
            {
                _logger.LogInformation("Game day!");

                //in the event the game has already started, start timer asap as possible
                var timerStart = info.Item2 < TimeSpan.Zero ? TimeSpan.Zero : info.Item2;

                //Get timer setup to go once game 'starts'
                _logger.LogInformation("Game apparently starting in about {0} hours ", timerStart.TotalHours);
                _liveUpdateTimer = new Timer(ExecuteScrape, info.Item3, timerStart, TimeSpan.FromMilliseconds(-1));
                _logger.LogInformation("Timer setup. Now we wait..");
            }
            else
            {
                _logger.LogInformation("No game today...back to sleep");
                return;
            }
        }

        public async void ExecuteScrape(object gamePk)
        {
            _logger.LogDebug("Entered ExecuteScrape()");
            int gameCode = int.Parse(gamePk.ToString());
            _logger.LogInformation("Scrape time. Game code: {0}", gamePk);

            if (!_mqttHelper.IsConnected)
            {
                _logger.LogInformation("Mqtt not connected. reconnecting");

                //TODO Smart reconnect logic
                await _mqttHelper.Reconnect();
            }

            _scraper = new BluesScraper(gameCode);
            _logger.LogDebug("BluesScraper Object built");

            await _mqttHelper.SendConfigData(Constants.GameStartedCommand);
            //Begin our nifty long running task
            while (true)
            {
                _logger.LogInformation("Scraping Loop started");
                try
                {
                    var data = await _scraper.RefreshData();
                    TimeSpan delay = (BluesScraper.GetDelayTime(data.Item2));
                    string json = JsonConvert.SerializeObject(data.Item1);

                    await _mqttHelper.SendData(json);

                    //If critical action or less than 60seconds left, send command to show time remaining
                    await SendShowTimeConfig(data);

                    //End execution 
                    if (delay == TimeSpan.Zero)
                    {
                        break;
                    }

                    _logger.LogInformation($"Sleeping for {delay}");
                    _logger.LogInformation("Game status: {0}", data.Item2.ToString());
                    Thread.Sleep(delay);
                }
                catch (Exception e)
                {
                    _logger.LogCritical("Exception during scraping loop: {0}", e.Message);
                    _logger.LogCritical("Cannot continue");
                }
            }

            //Game Over
            _logger.LogInformation("Game ended. Exiting scrape method");
            await _mqttHelper.SendConfigData(Constants.GameEndedCommand);
            _liveUpdateTimer.Dispose();
        }

        public async Task<Tuple<bool, TimeSpan, string>> GameDayCheck()
        {
            _logger.LogDebug("GameDayCheck()");
            var timeToGame = TimeSpan.Zero;

            var nextGameInfo = await BluesScraper.GetNextGameTimeAndCode(Constants.NextGameURL);
            var utcGameTime = DateTime.Parse(nextGameInfo.Item1);
            _logger.LogDebug("UTC Game time: {0}", utcGameTime.ToLongTimeString());

            var timeTil = utcGameTime - DateTime.UtcNow;
            bool isGameDay = timeTil.TotalHours < 24;

            _logger.LogDebug("Time til game: {0} minutes. Is game day? {1}", timeTil.TotalMinutes, isGameDay);

            return Tuple.Create(isGameDay, timeTil, nextGameInfo.Item2);
        }

        private async Task SendShowTimeConfig(Tuple<NHLAPIScrape.GameInfo, NHLAPIScrape.GameStatuses> data)
        {
            if (data.Item2 == NHLAPIScrape.GameStatuses.CriticalAction || (data.Item1.TimeRemaining > 1 && data.Item1.TimeRemaining < 65))
            {
                await _mqttHelper.SendConfigData(Constants.ShowTimeCommand);
            }
        }

        public void Dispose()
        {
            _logger.LogDebug("Dispose()-ing");
            _gameDayTimer?.Dispose();
            _mqttHelper.Dispose();
            _scraper.Dispose();
        }
    }
}
