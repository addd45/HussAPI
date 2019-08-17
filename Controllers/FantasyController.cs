using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using YahooFantasyWrapper.Client;
using YahooFantasyWrapper.Models;

namespace HussAPI.Controllers
{
    [Route("HussAPI/[controller]")]
    public class FantasyController: ControllerBase
    {
        private readonly Services.FantasyHockeyTool _fantasyTool;

        public FantasyController(Services.FantasyHockeyTool fht)
        {
            _fantasyTool = fht;
        }

        [HttpGet("login")]
        public RedirectResult Login()
        {
            return new RedirectResult(_fantasyTool.GetAuthUrl());
        }

        public async Task<IActionResult> Index()
        {
            if (this.Parameters != null & this.Parameters.Count > 0)
            {
                UserInfo userInfo = await this._fantasyTool.GetUserProfile(this.Parameters);
                UserInfo user = userInfo;

                string token = _fantasyTool.GetAccessToken();
                return Ok(new { token });
            }
            else return BadRequest();
        }

        private NameValueCollection Parameters
        {
            get
            {
                return HttpUtility.ParseQueryString(Request.QueryString.Value);
            }
        }
    }
}
