using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Toolkit;

namespace Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = BuildWebHost(args);

            var log = host.Services.GetService(typeof(ILogger<Program>)) as ILogger;

            try
            {
                log.Debug("[Init] starting");
                host.Run();
            }
            catch (Exception ex)
            {
                log.ErrorExceptionFormat(ex, "[Init] stopped program because of exception");
                throw;
            }
            finally
            {
                log.Debug("[Init] shutting down");
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
            }
        }

        private static IWebHost BuildWebHost(string[] args) =>
            WebHost
                /*
                 * Default builder will deal with configuration in root directory and also spinning up Kestrel
                 */
                .CreateDefaultBuilder(args)
                .ConfigureLogging((context, logging) =>
                {
                    // setup log levels for other
                    logging.SetMinimumLevel(LogLevel.Trace);

                    // see https://github.com/andrewlock/NetEscapades.Extensions.Logging/blob/master/sample/SampleApp/Program.cs
                    logging.AddFile(opts =>
                    {
                        opts.LogDirectory = Environment.GetEnvironmentVariable("TMPDIR")
                                            ?? Environment.GetEnvironmentVariable("TMP")
                                            ?? opts.LogDirectory;

                        context.Configuration.GetSection("Logging.File").Bind(opts);
                    });
                })
                .UseStartup<Startup>()
                .Build();
    }
}