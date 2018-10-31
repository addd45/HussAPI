using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HussAPI.Classes;
using HussAPI.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace RestAPI.Controllers
{
    [Route("HussAPI/[controller]")]
    [ApiController]
    public class ScoreBoardController : ControllerBase
    {
        ILogger _log;
        IScrapingService _scrapingService;

        public ScoreBoardController(ILogger<ScrapingService> log, ScrapingService scrapingService)
        {
            _log = log;
            _scrapingService = scrapingService;
            
        }

        [HttpPost("{gameID:int}")]
        public Task StartScoreBoard(int gameID)
        {
            _scrapingService.ExecuteScrape(gameID);
            return Task.CompletedTask;
        }

    }
}