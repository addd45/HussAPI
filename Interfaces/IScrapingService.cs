using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HussAPI.Interfaces
{
    public interface IScrapingService: IHostedService, IDisposable
    {
        void ExecuteScrape(object gamePk);
        Task<Tuple<bool, TimeSpan, string>> GameDayCheck();
        void PollGameDay(object state);
    }
}
