using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Web;
using Toolkit;

namespace Api
{
    public class Program
    {
        private static readonly NLog.ILogger Log = NLogBuilder
            .ConfigureNLog("Config/NLog.config")
            .GetCurrentClassLogger();

        public static void Main(string[] args)
        {
            try
            {
                Log.Debug("[Init] starting");
                BuildWebHost(args).Run();
            }
            catch (Exception ex)
            {
                Log.ErrorExceptionFormat(ex, "[Init] stopped program because of exception");
                throw;
            }
            finally
            {
                Log.Debug("[Init] shutting down");
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                LogManager.Shutdown();
            }
        }

        private static IWebHost BuildWebHost(string[] args) =>
            WebHost
                /*
                 * Default builder will deal with configuration in root directory and also spinning up Kestrel
                 */
                .CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .ConfigureLogging(logging =>
                {
                    // remove all other loggers
                    logging.ClearProviders();
                    // setup log levels for other
                    logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                })
                // Register NLog other code using Microsoft.Extensions.ILogger
                .UseNLog()
                .Build();
    }
}