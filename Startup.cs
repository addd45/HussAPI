using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using HussAPI.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using YahooFantasyWrapper.Configuration;
using YahooFantasyWrapper.Client;
using YahooFantasyWrapper.Infrastructure;

namespace HussAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.Configure<MQTTSettings>(Configuration.GetSection("MQTTSettings"));
            services.Configure<YahooConfiguration>(Configuration.GetSection("YahooConfiguration"));

            //DBContexts
            string postgresPW = Environment.GetEnvironmentVariable("POSTGRES_PASS");
            services.AddDbContext<DBContext.StockMarketContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("marketdb").Replace("redacted", postgresPW))
            );

            services.AddTransient<IYahooFantasyClient, YahooFantasyClient>();
            services.AddSingleton<IYahooAuthClient, YahooAuthClient>();
            services.AddTransient<IRequestFactory, RequestFactory>();
            services.AddTransient<FantasyHockeyTool>(); 

            services.AddHostedService<ScrapingService>();
            services.AddHostedService<MarketService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
