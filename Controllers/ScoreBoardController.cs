using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace RestAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScoreBoardController : ControllerBase
    {
        [HttpPost("{gameID:int}")]
        public Task StartScoreBoard(int gameID)
        {
            return Task.CompletedTask;
        }

    }
}