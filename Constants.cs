using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HussAPI
{
    public static class Constants
    {
        public static string BluesTeamNumber => "19";
        public static string NextGameURL => "https://statsapi.web.nhl.com/api/v1/teams/19/?expand=team.schedule.next";
    }
}
